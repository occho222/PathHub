using ModernLauncher.Interfaces;
using ModernLauncher.Services;
using System;
using System.Collections.Generic;

namespace ModernLauncher.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator? instance;
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static ServiceLocator Instance => instance ??= new ServiceLocator();

        private ServiceLocator()
        {
            RegisterServices();
        }

        private void RegisterServices()
        {
            services[typeof(IProjectService)] = new ProjectService();
            services[typeof(ILauncherService)] = new LauncherService();
            services[typeof(ISmartLauncherService)] = new SmartLauncherService((ILauncherService)services[typeof(ILauncherService)]);
        }

        public T GetService<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        public void RegisterService<T>(T service) where T : class
        {
            services[typeof(T)] = service;
        }
    }
}