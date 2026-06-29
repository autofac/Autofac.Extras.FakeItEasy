// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using FakeItEasy;
using FakeItEasy.Creation;
using FakeItEasy.Sdk;

namespace Autofac.Extras.FakeItEasy;

/// <summary> Resolves unknown interfaces and Fakes. </summary>
internal sealed class FakeRegistrationHandler : IRegistrationSource
{
    private readonly bool _strict;
    private readonly bool _callsBaseMethods;
    private readonly Action<object>? _configureFake;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeRegistrationHandler" /> class.
    /// </summary>
    /// <param name="strict">Whether fakes should be created with strict semantics.</param>
    /// <param name="callsBaseMethods">Whether fakes should call base methods.</param>
    /// <param name="configureFake">An action to perform on a fake before it's created.</param>
    public FakeRegistrationHandler(bool strict, bool callsBaseMethods, Action<object>? configureFake)
    {
        _strict = strict;
        _callsBaseMethods = callsBaseMethods;
        _configureFake = configureFake;
    }

    /// <summary>
    /// Gets a value indicating whether the registrations provided by this source are 1:1 adapters on top
    /// of other components (I.e. like Meta, Func or Owned.)
    /// </summary>
    public bool IsAdapterForIndividualComponents => false;

    /// <summary>
    /// Retrieve registrations for an unregistered service, to be used
    /// by the container.
    /// </summary>
    /// <param name="service">The service that was requested.</param>
    /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
    /// <returns>Registrations providing the service.</returns>
    public IEnumerable<IComponentRegistration> RegistrationsFor(
        Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var typedService = service as TypedService;
        if (typedService == null ||
            (!typedService.ServiceType.GetTypeInfo().IsInterface && !typedService.ServiceType.GetTypeInfo().IsAbstract) ||
            (typedService.ServiceType.GetTypeInfo().IsGenericType && typedService.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
            typedService.ServiceType.IsArray ||
            typeof(IStartable).IsAssignableFrom(typedService.ServiceType))
        {
            return [];
        }

        var rb = RegistrationBuilder.ForDelegate((c, p) => CreateFake(typedService))
            .As(service)
            .InstancePerLifetimeScope();

        return new[] { rb.CreateRegistration() };
    }

    /// <summary>
    /// Creates a fake object.
    /// </summary>
    /// <param name="typedService">The typed service.</param>
    /// <returns>A fake object.</returns>
    private object CreateFake(TypedService typedService) => Create.Fake(typedService.ServiceType, ApplyOptions);

    private void ApplyOptions(IFakeOptions options)
    {
        if (_strict)
        {
            options = options.Strict();
        }

        if (_configureFake != null)
        {
            options = options.ConfigureFake(x => _configureFake(x));
        }

        if (_callsBaseMethods)
        {
            options.CallsBaseMethods();
        }
    }
}
