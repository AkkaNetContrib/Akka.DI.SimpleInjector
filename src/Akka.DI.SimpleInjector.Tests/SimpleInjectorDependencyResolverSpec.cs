//-----------------------------------------------------------------------
// <copyright file="WindsorDependencyResolver.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.TestKit;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using Xunit;

namespace Akka.DI.SimpleInjector.Tests
{
    public class SimpleInjectorDependencyResolverSpec : DiResolverSpec
    {
        protected override object NewDiContainer()
        {
            var container =  new Container();

            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            return container;
        }

        protected override IDependencyResolver NewDependencyResolver(object diContainer, ActorSystem system)
        {
            var container = ToContainer(diContainer);

            return new SimpleInjectorDependencyResolver(container, system);
        }
 
        protected override void Bind<T>(object diContainer, Func<T> generator)
        {
            var container = ToContainer(diContainer);

            container.Register(typeof(T), () => generator(), Lifestyle.Scoped);
        }
 
        protected override void Bind<T>(object diContainer)
        {
            var container = ToContainer(diContainer);

            container.Register(typeof(T), typeof(T), Lifestyle.Scoped);
        }
 
        private static Container ToContainer(object diContainer)
        {
            var container = diContainer as Container;

            Assert.True(container != null, "container != null");

            return container;
        }
    }
}
