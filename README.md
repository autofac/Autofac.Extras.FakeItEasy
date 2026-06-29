# Autofac.Extras.FakeItEasy

FakeItEasy auto mocking integration for [Autofac](https://autofac.org).

[![Build status](https://github.com/autofac/Autofac.Extras.FakeItEasy/actions/workflows/main.yml/badge.svg)](https://github.com/autofac/Autofac.Extras.FakeItEasy/actions/workflows/main.yml) [![codecov](https://codecov.io/gh/Autofac/Autofac.Extras.FakeItEasy/branch/develop/graph/badge.svg)](https://codecov.io/gh/Autofac/Autofac.Extras.FakeItEasy) [![NuGet](https://img.shields.io/nuget/v/Autofac.Extras.FakeItEasy.svg)](https://nuget.org/packages/Autofac.Extras.FakeItEasy)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/fakeiteasy.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Extras.FakeItEasy)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Extras.FakeItEasy)

## Quick Start

The primary entry point for this integration is the `Autofac.Extras.FakeItEasy.AutoFake` class. Once you create an `AutoFake`, you can `Resolve` the class under test and Autofac will automatically fill its constructor dependencies with FakeItEasy fakes. You then configure and assert on those fakes the way you normally would with FakeItEasy.

```csharp
[Fact]
public void DependenciesAreAutomaticallyFaked()
{
    using (var fake = new AutoFake())
    {
        // Configure the fake dependency that will be injected.
        A.CallTo(() => fake.Resolve<IDependency>().GetValue()).Returns("expected value");

        // Resolve the system under test; its IDependency is the fake configured above.
        var sut = fake.Resolve<SystemUnderTest>();

        // Act and assert as usual.
        Assert.Equal("expected value", sut.DoWork());
        A.CallTo(() => fake.Resolve<IDependency>().GetValue()).MustHaveHappened();
    }
}
```

To inject a specific instance or implementation instead of an automatic fake, register it through the `configureAction` constructor argument:

```csharp
var dependency = new Dependency();
using (var fake = new AutoFake(configureAction: cfg => cfg.RegisterInstance(dependency).As<IDependency>()))
{
    // SystemUnderTest receives your dependency instance.
    var sut = fake.Resolve<SystemUnderTest>();
}
```

See the [documentation](https://autofac.readthedocs.io/en/latest/integration/fakeiteasy.html) for more usage details and options.

## Migrating from 7.0.0

Version 8.0.0 removed the `AutoFake.Provide<TService>(instance)` and `AutoFake.Provide<TService, TImplementation>()` methods. Each `Provide` call started a new lifetime scope, which meant a fake resolved _before_ the call was a different instance than the one injected _afterward_ - so configuration applied to the earlier fake silently had no effect.

Register specific dependencies through the `configureAction` constructor argument instead. Everything is registered into a single scope before the container is built, so the instances you configure are the instances that get injected.

```csharp
// Before (7.0.0)
using (var fake = new AutoFake())
{
    var dependency = A.Fake<IDependency>();
    fake.Provide(dependency);
    var sut = fake.Resolve<SystemUnderTest>();
}

// After (8.0.0)
var dependency = A.Fake<IDependency>();
using (var fake = new AutoFake(configureAction: cfg => cfg.RegisterInstance(dependency).As<IDependency>()))
{
    var sut = fake.Resolve<SystemUnderTest>();
}
```

For implementation registrations, register the type in `configureAction` and move any Autofac parameters onto the registration with `WithParameter`:

```csharp
// Before (7.0.0): fake.Provide<IDependency, Dependency>();
// After (8.0.0):
using (var fake = new AutoFake(configureAction: cfg => cfg.RegisterType<Dependency>().As<IDependency>()))
{
    var sut = fake.Resolve<SystemUnderTest>();
}
```

Version 8.0.0 also removed the `builder` constructor parameter (the one that let you pass your own `ContainerBuilder`). It was redundant with `configureAction`, so move any registrations from your own builder into the `configureAction` callback.

See the [Migrating from 7.0.0](https://autofac.readthedocs.io/en/latest/integration/fakeiteasy.html#migrating-from-7-0-0) documentation for the full details.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
