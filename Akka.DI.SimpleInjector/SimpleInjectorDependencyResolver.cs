﻿using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.DI.Core;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
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
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            if (actorSystem == null)
            {
                throw new ArgumentNullException("actorSystem");
            }

            _container = container;

            _actorSystem = actorSystem;

            _actorSystem.AddDependencyResolver(this);

            _typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

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
                var scope = _container.BeginExecutionContextScope();

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