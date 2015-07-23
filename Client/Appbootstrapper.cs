using System;
using System.Reflection;
using System.Windows;
using AutoFacWrapper;
using Caliburn.Micro;
using Client;
using SqliteCompare.Repository;
using SqliteCompare.Service;
using SqliteCompare.Service.InterFace;


namespace SqliteCompare.Shell
{
    public class Appbootstrapper : Bootstrapper<object>
    {
        protected override object GetInstance(Type service, string key)
        {
            return ClassFactory.GetInstance(service);
        }

        protected override void Configure()
        {
            base.Configure();
            ClassFactory.RegisterAssemblis(Assembly.GetExecutingAssembly());
            AppRegister.RegisterModular();
            ClassFactory.RegisterTypeSingleInstance<WindowManager, IWindowManager>();
           

         
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var _windowManager = ClassFactory.GetInstance<IWindowManager>();
            _windowManager.ShowWindow(ClassFactory.GetInstance<ShellViewModel>());
        }
    }
}