using System;
using System.IO;
using System.Net;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;

namespace SqliteCompare.WPFCore.Controls
{
    public class GifImageExceptionRoutedEventArgs : RoutedEventArgs
    {
        public Exception ErrorException;

        public GifImageExceptionRoutedEventArgs(RoutedEvent routedEvent, object obj)
            : base(routedEvent, obj)
        {
        }
    }

    internal class WebReadState
    {
        public byte[] buffer;
        public MemoryStream memoryStream;
        public Stream readStream;
        public WebRequest webRequest;
    }


    public class GifImage : UserControl
    {
        public delegate void ExceptionRoutedEventHandler(object sender, GifImageExceptionRoutedEventArgs args);

        public static readonly DependencyProperty ForceGifAnimProperty = DependencyProperty.Register("ForceGifAnim",
            typeof (bool), typeof (GifImage), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof (string),
            typeof (GifImage),
            new FrameworkPropertyMetadata("",
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                OnSourceChanged));

        // Using a DependencyProperty as the backing store for BytesSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BytesSourceProperty =
            DependencyProperty.Register("BytesSource", typeof (byte[]), typeof (GifImage),
                new UIPropertyMetadata(null, BytesSourceChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch",
            typeof (Stretch), typeof (GifImage),
            new FrameworkPropertyMetadata(Stretch.Fill, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnStretchChanged));

        public static readonly DependencyProperty StretchDirectionProperty =
            DependencyProperty.Register("StretchDirection", typeof (StretchDirection), typeof (GifImage),
                new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnStretchDirectionChanged));

        public static readonly RoutedEvent ImageFailedEvent = EventManager.RegisterRoutedEvent("ImageFailed",
            RoutingStrategy.Bubble, typeof (ExceptionRoutedEventHandler), typeof (GifImage));

        private GifAnimation gifAnimation;
        private Image image;

        public bool ForceGifAnim
        {
            get { return (bool) GetValue(ForceGifAnimProperty); }
            set { SetValue(ForceGifAnimProperty, value); }
        }

        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public byte[] BytesSource
        {
            get { return (byte[]) GetValue(BytesSourceProperty); }
            set { SetValue(BytesSourceProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch) GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public StretchDirection StretchDirection
        {
            get { return (StretchDirection) GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (string) e.NewValue;
            obj.CreateFromSourceString(s);
        }

        private static void BytesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (string) e.NewValue;
            obj.CreateFromSourceString(s);
        }

        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (Stretch) e.NewValue;
            if (obj.gifAnimation != null)
            {
                obj.gifAnimation.Stretch = s;
            }
            else if (obj.image != null)
            {
                obj.image.Stretch = s;
            }
        }

        private static void OnStretchDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (GifImage) d;
            var s = (StretchDirection) e.NewValue;
            if (obj.gifAnimation != null)
            {
                obj.gifAnimation.StretchDirection = s;
            }
            else if (obj.image != null)
            {
                obj.image.StretchDirection = s;
            }
        }

        public event ExceptionRoutedEventHandler ImageFailed
        {
            add { AddHandler(ImageFailedEvent, value); }
            remove { RemoveHandler(ImageFailedEvent, value); }
        }

        private void image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            RaiseImageFailedEvent(e.ErrorException);
        }

        private void RaiseImageFailedEvent(Exception exp)
        {
            var newArgs = new GifImageExceptionRoutedEventArgs(ImageFailedEvent, this);
            newArgs.ErrorException = exp;
            RaiseEvent(newArgs);
        }

        private void DeletePreviousImage()
        {
            if (image != null)
            {
                RemoveLogicalChild(image);
                image = null;
            }
            if (gifAnimation != null)
            {
                RemoveLogicalChild(gifAnimation);
                gifAnimation = null;
            }
        }

        private void CreateNonGifAnimationImage()
        {
            image = new Image();
            image.ImageFailed += image_ImageFailed;
            var src = (ImageSource) (new ImageSourceConverter().ConvertFromString(Source));
            image.Source = src;
            image.Stretch = Stretch;
            image.StretchDirection = StretchDirection;
            AddChild(image);
        }

        private void CreateGifAnimation(MemoryStream memoryStream)
        {
            gifAnimation = new GifAnimation();
            gifAnimation.CreateGifAnimation(memoryStream);
            gifAnimation.Stretch = Stretch;
            gifAnimation.StretchDirection = StretchDirection;
            AddChild(gifAnimation);
        }

        private void CreateFromSourceString(string source)
        {
            DeletePreviousImage();
            Uri uri;

            try
            {
                uri = new Uri(source, UriKind.RelativeOrAbsolute);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
                return;
            }

            if (source.Trim().ToUpper().EndsWith(".GIF") || ForceGifAnim)
            {
                if (!uri.IsAbsoluteUri)
                {
                    GetGifStreamFromPack(uri);
                }
                else
                {
                    var leftPart = uri.GetLeftPart(UriPartial.Scheme);

                    if (leftPart == "http://" || leftPart == "ftp://" || leftPart == "file://")
                    {
                        GetGifStreamFromHttp(uri);
                    }
                    else if (leftPart == "pack://")
                    {
                        GetGifStreamFromPack(uri);
                    }
                    else
                    {
                        CreateNonGifAnimationImage();
                    }
                }
            }
            else
            {
                CreateNonGifAnimationImage();
            }
        }

        private void WebRequestFinished(MemoryStream memoryStream)
        {
            CreateGifAnimation(memoryStream);
        }

        private void WebRequestError(Exception exp)
        {
            RaiseImageFailedEvent(exp);
        }

        private void WebResponseCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState) asyncResult.AsyncState;
            WebResponse webResponse;
            try
            {
                webResponse = webReadState.webRequest.EndGetResponse(asyncResult);
                webReadState.readStream = webResponse.GetResponseStream();
                webReadState.buffer = new byte[100000];
                webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length, WebReadCallback,
                    webReadState);
            }
            catch (WebException exp)
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
            }
        }

        private void WebReadCallback(IAsyncResult asyncResult)
        {
            var webReadState = (WebReadState) asyncResult.AsyncState;
            var count = webReadState.readStream.EndRead(asyncResult);
            if (count > 0)
            {
                webReadState.memoryStream.Write(webReadState.buffer, 0, count);
                try
                {
                    webReadState.readStream.BeginRead(webReadState.buffer, 0, webReadState.buffer.Length,
                        WebReadCallback, webReadState);
                }
                catch (WebException exp)
                {
                    Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestErrorDelegate(WebRequestError), exp);
                }
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Render, new WebRequestFinishedDelegate(WebRequestFinished),
                    webReadState.memoryStream);
            }
        }

        private void GetGifStreamFromHttp(Uri uri)
        {
            try
            {
                var webReadState = new WebReadState();
                webReadState.memoryStream = new MemoryStream();
                webReadState.webRequest = WebRequest.Create(uri);
                webReadState.webRequest.Timeout = 10000;

                webReadState.webRequest.BeginGetResponse(WebResponseCallback, webReadState);
            }
            catch (SecurityException)
            {
                CreateNonGifAnimationImage();
            }
        }

        private void ReadGifStreamSynch(Stream s)
        {
            byte[] gifData;
            MemoryStream memoryStream;
            using (s)
            {
                memoryStream = new MemoryStream((int) s.Length);
                var br = new BinaryReader(s);
                gifData = br.ReadBytes((int) s.Length);
                memoryStream.Write(gifData, 0, (int) s.Length);
                memoryStream.Flush();
            }
            CreateGifAnimation(memoryStream);
        }

        private void GetGifStreamFromPack(Uri uri)
        {
            try
            {
                StreamResourceInfo streamInfo;

                if (!uri.IsAbsoluteUri)
                {
                    streamInfo = Application.GetContentStream(uri);
                    if (streamInfo == null)
                    {
                        streamInfo = Application.GetResourceStream(uri);
                    }
                }
                else
                {
                    if (uri.GetLeftPart(UriPartial.Authority).Contains("siteoforigin"))
                    {
                        streamInfo = Application.GetRemoteStream(uri);
                    }
                    else
                    {
                        streamInfo = Application.GetContentStream(uri);
                        if (streamInfo == null)
                        {
                            streamInfo = Application.GetResourceStream(uri);
                        }
                    }
                }
                if (streamInfo == null)
                {
                    throw new FileNotFoundException("Resource not found.", uri.ToString());
                }
                ReadGifStreamSynch(streamInfo.Stream);
            }
            catch (Exception exp)
            {
                RaiseImageFailedEvent(exp);
            }
        }

        private delegate void WebRequestFinishedDelegate(MemoryStream memoryStream);

        private delegate void WebRequestErrorDelegate(Exception exp);
    }
}