//-----------------------------------------------------------------------
// <copyright file="SimpleInjectorDependencyResolver.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2017 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.DI.Core;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using SI = SimpleInjector;

namespace Akka.DI.SimpleInjector
{
    public class SimpleInjectorDependencyResolver : IDependencyResolver
    {
        private readonly Container _container;
        private readonly ActorSystem _actorSystem;
        private readonly ConcurrentDictionary<string, Type> _typeCache;
        private readonly ConditionalWeakTable<ActorBase, SI.Scope> _references;
 
        public SimpleInjectorDependencyResolver(Container container, ActorSystem actorSystem)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));

            _actorSystem.AddDependencyResolver(this);

            _typeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            _references = new ConditionalWeakTable<ActorBase, SI.Scope>();
        }
 
        public Type GetType(string actorName)
        {
            return _typeCache.GetOrAdd(actorName, actorName.GetTypeValue());
        }
 
        public Func<ActorBase> CreateActorFactory(Type actorType)
        {
            return () =>
            {
                var scope = AsyncScopedLifestyle.BeginScope(_container);

                var actor = (ActorBase)_container.GetInstance(actorType);

                _references.Add(actor, scope);
 
                return actor;
            };
        }
 
        public Props Create<TActor>() where TActor : ActorBase
        {
            return Create(typeof(TActor));
        }
 
        public Props Create(Type actorType)
        {
            return _actorSystem.GetExtension<DIExt>().Props(actorType);
        }
 
        public void Release(ActorBase actor)
        {
            SI.Scope scope;

            if (_references.TryGetValue(actor, out scope))
            {
                scope.Dispose();

                _references.Remove(actor);
            }
        }
    }
}