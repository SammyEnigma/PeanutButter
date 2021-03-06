﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using PeanutButter.Utils;

namespace PeanutButter.Utils.Windsor
{
    public static class WindsorContainerExtensions
    {
        public static void RegisterAllOneToOneResolutionsAsTransientFrom(
            this IWindsorContainer container,
            params Assembly[] assemblies)
        {
            container.RegisterAllOneToOneResolutionsAsTransientExcept(
                new Type[0],
                assemblies
            );
        }

        public static void RegisterAllOneToOneResolutionsAsTransientExcept(
            this IWindsorContainer container,
            IEnumerable<Type> exclusions,
            IEnumerable<Assembly> assemblies)
        {
            var assemblyArray = assemblies as Assembly[] ?? assemblies.ToArray();
            if (assemblyArray.IsEmpty())
                throw NoAssembliesException;
            var allTypes = assemblyArray.SelectMany(a => a.GetTypes()).ToArray();
            var allInterfaces = allTypes.Where(t => t.IsInterface)
                .Where(t => !exclusions.Contains(t))
                .ToArray();
            var allImplementations = allTypes.Where(t => !t.IsInterface &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    !exclusions.Contains(t))
                .ToArray();
            allInterfaces.ForEach(interfaceType =>
            {
                if (container.Kernel.HasComponent(interfaceType))
                    return;
                var implementingTypes = allImplementations
                    .Where(interfaceType.IsAssignableFrom)
                    .ToArray();
                if (implementingTypes.Length != 1)
                    return;
                var implementation = implementingTypes.Single();
                container.Register(Component.For(interfaceType)
                    .ImplementedBy(implementation)
                    .LifestyleTransient()
                    .Named(RegistrationNameFor(interfaceType, implementation)));
            });
        }

        private static string RegistrationNameFor(
            Type interfaceType,
            Type implementationType
        )
        {
            return $"{interfaceType.Name} => {implementationType.Name}";
        }

        [Obsolete("Please use RegisterAllMvcControllersFrom instead")]
        public static void RegisterAllControllersFrom(this IWindsorContainer container, params Assembly[] assemblies)
        {
            container.RegisterAllMvcControllersFrom(assemblies);
        }

        public static void RegisterAllMvcControllersFrom(this IWindsorContainer container, params Assembly[] assemblies)
        {
            if (assemblies.IsEmpty())
                throw NoAssembliesException;
            var controllerTypes = assemblies.SelectMany(a => a.GetTypes().Where(IsBasedOnMvcController));
            controllerTypes.ForEach(t => container.Register(Component.For(t).ImplementedBy(t).LifestyleTransient()));
        }

        public static void RegisterAllApiControllersFrom(this IWindsorContainer container, params Assembly[] assemblies)
        {
            if (assemblies.IsEmpty())
                throw NoAssembliesException;
            var controllerTypes = assemblies.SelectMany(a => a.GetTypes().Where(IsBasedOnApiController));
            controllerTypes.ForEach(t => container.Register(Component.For(t).ImplementedBy(t).LifestyleTransient()));
        }

        public static void RegisterSingleton<TService, TImplementation>(this IWindsorContainer container)
            where TService : class
            where TImplementation : TService
        {
            container.Register(Component.For<TService>()
                .ImplementedBy<TImplementation>()
                .LifestyleSingleton());
        }

        public static void RegisterSingleton(this IWindsorContainer container,
            Type serviceType,
            Type implementationType)
        {
            container.Register(Component.For(serviceType)
                .ImplementedBy(implementationType)
                .LifestyleSingleton());
        }

        public static void RegisterTransient<TService, TImplementation>(
            this IWindsorContainer container)
            where TService : class
            where TImplementation : TService
        {
            container.Register(Component.For<TService>()
                .ImplementedBy<TImplementation>()
                .LifestyleTransient());
        }

        public static void RegisterPerWebRequest<TService, TImplementation>(
            this IWindsorContainer container)
            where TService : class
            where TImplementation : TService
        {
            throw new NotImplementedException(
                    new[]
                    {
                        "Castle.Windsor have removed LifestylePerWebRequest",
                        "so you have some options:",
                        "- check out https://github.com/castleproject/Windsor/issues/359",
                        "- use another IOC container. I highly recommend DryIOC"
                    }.JoinWith("\n")
                );
        }

        public static void RegisterInstance<TService>(this IWindsorContainer container, TService instance)
            where TService : class
        {
            container.Register(
                Component.For<TService>()
                    .Instance(instance)
                    .Named($"{typeof(TService)} -> {instance?.GetType().Name}"));
        }

        private const string API_CONTROLLER_NAMESPACE = "System.Web.Http";
        private const string API_CONTROLLER_ASSEMBLY = API_CONTROLLER_NAMESPACE + ",";

        private static bool IsBasedOnApiController(Type type)
        {
            return type.Ancestry()
                .Any(t => t.Name == "ApiController" &&
                    t.Namespace == API_CONTROLLER_NAMESPACE &&
                    t.Assembly.FullName.StartsWith(API_CONTROLLER_ASSEMBLY));
        }

        private const string MVC_CONTROLLER_NAMESPACE = "System.Web.Mvc";
        private const string MVC_CONTROLLER_ASSEMBLY = MVC_CONTROLLER_NAMESPACE + ",";

        private static bool IsBasedOnMvcController(Type type)
        {
            return type.Ancestry()
                .Any(t => t.Name == "Controller" &&
                    t.Namespace == MVC_CONTROLLER_NAMESPACE &&
                    t.Assembly.FullName.StartsWith(MVC_CONTROLLER_ASSEMBLY));
        }

        private static ArgumentException NoAssembliesException =>
            new ArgumentException("No assemblies provided to search for registrations");
    }
}