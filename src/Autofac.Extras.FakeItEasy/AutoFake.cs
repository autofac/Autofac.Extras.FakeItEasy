// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace Autofac.Extras.FakeItEasy
{
    /// <summary>
    /// Wrapper around <see cref="Autofac"/> and <see cref="FakeItEasy"/>.
    /// </summary>
    [SecurityCritical]
    public class AutoFake : IDisposable
    {
        private bool _disposed;

        private readonly Stack<ILifetimeScope> _scopes = new Stack<ILifetimeScope>();

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "It's only a reference, dispose is called from the _scopes Stack")]
        private ILifetimeScope _currentScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFake" /> class.
        /// </summary>
        /// <param name="strict">
        /// <see langword="true" /> to create strict fakes.
        /// This means that any calls to the fakes that have not been explicitly configured will throw an exception.
        /// </param>
        /// <param name="callsBaseMethods">
        /// <see langword="true" /> to delegate configured method calls to the base method of the faked method.
        /// </param>
        /// <param name="builder">The container builder to use to build the container.</param>
        /// <param name="configureFake">Specifies an action that should be run over a fake object before it's created.</param>
        /// <param name="configureAction">Specifies actions that needs to be performed on the container builder, like registering additional services.</param>
        public AutoFake(
            bool strict = false,
            bool callsBaseMethods = false,
            Action<object> configureFake = null,
            ContainerBuilder builder = null,
            Action<ContainerBuilder> configureAction = null)
        {
            if (builder == null)
            {
                builder = new ContainerBuilder();
            }

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(b => b.InstancePerLifetimeScope()));
            builder.RegisterSource(new FakeRegistrationHandler(strict, callsBaseMethods, configureFake));
            configureAction?.Invoke(builder);
            this.Container = builder.Build();
            this._currentScope = this.Container.BeginLifetimeScope();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AutoFake"/> class.
        /// </summary>
        [SecuritySafeCritical]
        ~AutoFake()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that handles the component resolution.
        /// </summary>
        public IContainer Container { get; }

        /// <summary>
        /// Disposes internal container.
        /// </summary>
        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="parameters">Optional parameters.</param>
        /// <returns>The service.</returns>
        public T Resolve<T>(params Parameter[] parameters)
        {
            return this._currentScope.Resolve<T>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
        /// </summary>
        /// <typeparam name="T">The type of the service.</typeparam>
        /// <param name="parameters">Optional parameters.</param>
        /// <returns>The service.</returns>
        [Obsolete("Use Resolve<T>() instead")]
        public T Create<T>(params Parameter[] parameters)
        {
            return this.Resolve<T>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register it if needed).
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the service.</typeparam>
        /// <param name="parameters">Optional parameters.</param>
        /// <returns>The service.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService, TImplementation>(params Parameter[] parameters)
        {
            var scope = this._currentScope.BeginLifetimeScope(b =>
            {
                b.RegisterType<TImplementation>().As<TService>().InstancePerLifetimeScope();
            });

            this._scopes.Push(scope);
            this._currentScope = scope;

            return this._currentScope.Resolve<TService>(parameters);
        }

        /// <summary>
        /// Resolve the specified type in the container (register specified instance if needed).
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The instance to register if needed.</param>
        /// <returns>The instance resolved from container.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The component registry is responsible for registration disposal.")]
        public TService Provide<TService>(TService instance)
            where TService : class
        {
            var scope = this._currentScope.BeginLifetimeScope(b =>
            {
                b.Register(c => instance).InstancePerLifetimeScope();
            });

            this._scopes.Push(scope);
            this._currentScope = scope;

            return this._currentScope.Resolve<TService>();
        }

        /// <summary>
        /// Handles disposal of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to dispose of managed resources (during a manual execution
        /// of <see cref="Autofac.Extras.FakeItEasy.AutoFake.Dispose()"/>); or
        /// <see langword="false" /> if this is getting run as part of finalization where
        /// managed resources may have already been cleaned up.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    while (this._scopes.Count > 0)
                        this._scopes.Pop().Dispose();

                    this.Container.Dispose();
                }

                this._disposed = true;
            }
        }
    }
}
