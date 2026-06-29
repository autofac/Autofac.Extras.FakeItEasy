// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace Autofac.Extras.FakeItEasy;

/// <summary>
/// Wrapper around <see cref="Autofac"/> and <see cref="FakeItEasy"/>.
/// </summary>
[SecurityCritical]
public class AutoFake : IDisposable
{
    private readonly ILifetimeScope _scope;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoFake" /> class.
    /// </summary>
    /// <param name="strict">
    /// <see langword="true" /> to create strict fakes. This means that any
    /// calls to the fakes that have not been explicitly configured will throw
    /// an exception.
    /// </param>
    /// <param name="callsBaseMethods">
    /// <see langword="true" /> to delegate configured method calls to the base
    /// method of the faked method.
    /// </param>
    /// <param name="configureFake">
    /// Specifies an action that should be run over a fake object before it's
    /// created.
    /// </param>
    /// <param name="configureAction">
    /// Specifies actions that need to be performed on the container builder,
    /// like registering additional services. Use this to provide specific
    /// dependency instances or implementations to the system under test (for
    /// example, <c>configureAction: b =&gt;
    /// b.RegisterInstance(myDependency).As&lt;IDependency&gt;()</c>).
    /// </param>
    public AutoFake(
        bool strict = false,
        bool callsBaseMethods = false,
        Action<object>? configureFake = null,
        Action<ContainerBuilder>? configureAction = null)
    {
        var builder = new ContainerBuilder();

        builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource().WithRegistrationsAs(b => b.InstancePerLifetimeScope()));
        builder.RegisterSource(new FakeRegistrationHandler(strict, callsBaseMethods, configureFake));
        configureAction?.Invoke(builder);
        Container = builder.Build();
        _scope = Container.BeginLifetimeScope();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AutoFake"/> class.
    /// </summary>
    [SecuritySafeCritical]
    [SuppressMessage("CA1063", "CA1063", Justification = "False positive - the message wants us to call Dispose(false) and we already do that.")]
    ~AutoFake()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the <see cref="IContainer"/> that handles the component resolution.
    /// </summary>
    public IContainer Container
    {
        get;
    }

    /// <summary>
    /// Disposes internal container.
    /// </summary>
    [SecuritySafeCritical]
    [SuppressMessage("CA1063", "CA1063", Justification = "False positive - the message wants us to call Dispose(true) / SuppressFinalize and we already do that.")]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resolve the specified type in the container (register it if needed).
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="parameters">Optional parameters.</param>
    /// <returns>The service.</returns>
    public T Resolve<T>(params Parameter[] parameters)
        where T : notnull
            => _scope.Resolve<T>(parameters);

    /// <summary>
    /// Handles disposal of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to dispose of managed resources (during a manual
    /// execution of <see cref="AutoFake.Dispose()"/>); or
    /// <see langword="false" /> if this is getting run as part of finalization
    /// where managed resources may have already been cleaned up.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _scope.Dispose();
                Container.Dispose();
            }

            _disposed = true;
        }
    }
}
