using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Autofac.Builder;
using Autofac;
using System.Reflection;

namespace AutoFacWrapper
{
    public static class ClassFactory
    {
        public static Func<string, Type, object, object> Getinstance;
        private static ContainerBuilder _builder = null;
        private static IContainer _iConainer = null; 


        /// <summary>
        /// readOnly
        /// </summary>
        public static ContainerBuilder Builder
        {
            get
            {
                return _builder;
            }
        }
        /// <summary>
        /// readOnly
        /// </summary>
        public static IContainer IConainer
        {
            get
            {
                return _iConainer;
            }
        }

        static ClassFactory()
        {
            _builder = new ContainerBuilder();
        }
        /// <summary>
        /// IConainer 相当于单例
        /// </summary>
        /// <returns></returns>
        public static IContainer GetContainer()
        {
            if (IConainer==null)
            {
                _iConainer = Builder.Build(Autofac.Builder.ContainerBuildOptions.None);
            }
            return IConainer;
        }

        /// <summary>
        /// 获取实例 泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T GetInstance<T>(params Parameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                return GetContainer().Resolve<T>();
            }   
            return GetContainer().Resolve<T>( parameters);
        }
        /// <summary>
        /// 重写 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T GetInstance<T>()
        {
            return GetContainer().Resolve<T>();
        }


        public static object GetInstance(Type type,params Parameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                return GetContainer().Resolve(type);
            }
          
            return GetContainer().Resolve(type, parameters);
        }

        public static object GetInstance(Type type)
        {
            return GetInstance(type, new Parameter[0]);
        }
        /// <summary>
        /// 注册程序集
        /// </summary>
        /// <param name="parameters"></param>
        public static void RegisterAssemblis(params Assembly[] parameters)

        {
            //AsSelf 必须写，注册程序集里没有继承接口的Types
            //Below is the code for my Autofac module.  
            //Please make sure you add the .AsSelf convention.  
            //If you do NOT do this your hubs will not be created.  
            //This is because there is no interface attached so Autofac will have no idea how to create the class.
            //AsImplementedInterfaces 
            Builder.RegisterAssemblyTypes(parameters).AsSelf().AsImplementedInterfaces();
        }
        /// <summary>
        /// 注册成单例
        /// </summary>
        /// <typeparam name="TLT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypeSingleInstance<TLT, T>() where TLT : T
        {
            Builder.RegisterType<TLT>().As<T>().SingleInstance();
        }
        /// <summary>
        /// 正常注册
        /// </summary>
        /// <typeparam name="TLT"></typeparam>
        /// <typeparam name="T"></typeparam>
        public static void RegisterTypePerDependency<TLT, T>() where TLT : T
        {
            Builder.RegisterType<TLT>().As<T>().InstancePerDependency();
        }


        //TODO Register Aop instance

    }
}
