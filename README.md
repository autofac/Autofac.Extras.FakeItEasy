# Autofac.Extras.FakeItEasy

FakeItEasy auto mocking integration for [Autofac](https://autofac.org).

[![Build status](https://github.com/autofac/Autofac.Extras.FakeItEasy/actions/workflows/main.yml/badge.svg)](https://github.com/autofac/Autofac.Extras.FakeItEasy/actions/workflows/main.yml) [![codecov](https://codecov.io/gh/Autofac/Autofac.Extras.FakeItEasy/branch/develop/graph/badge.svg)](https://codecov.io/gh/Autofac/Autofac.Extras.FakeItEasy) [![NuGet](https://img.shields.io/nuget/v/Autofac.Extras.FakeItEasy.svg)](https://nuget.org/packages/Autofac.Extras.FakeItEasy)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/fakeiteasy.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Extras.FakeItEasy)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Extras.FakeItEasy)

## Quick Start

The primary entry point for this integration is the `Autofac.Extras.FakeItEasy.AutoFake` class. Once you create an `AutoFake`, you can start "resolving" classes from the provider where dependencies will be automatically filled in by Autofac.

```csharp
[Fact]
public void TwoAutoFakedInstances()
{
    using (var fake = new AutoFake())
    {
        var bar1 = fake.Resolve<IBar>();
        var bar2 = fake.Resolve<IBar>();

        Assert.Same(bar1, bar2);
    }
}
```

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
