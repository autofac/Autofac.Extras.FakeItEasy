// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using FakeItEasy;
using Xunit;

namespace Autofac.Extras.FakeItEasy.Test
{
    public class AutoFakeFixture
    {
        public interface IBar
        {
            bool Gone { get; }

            void Go();

            IBar Spawn();
        }

        public interface IBaz
        {
            void Go();
        }

        [Fact]
        public void ByDefaultAbstractTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoFake())
            {
                var bar1 = fake.Resolve<IBar>();
                var bar2 = fake.Resolve<IBar>();

                Assert.Same(bar1, bar2);
            }
        }

        [Fact]
        public void ByDefaultConcreteTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoFake())
            {
                var baz1 = fake.Resolve<Baz>();
                var baz2 = fake.Resolve<Baz>();

                Assert.Same(baz1, baz2);
            }
        }

        [Fact]
        public void ByDefaultFakesAreNotStrict()
        {
            using (var fake = new AutoFake())
            {
                var foo = fake.Resolve<Foo>();

                // Should not throw.
                foo.Go();
            }
        }

        [Fact]
        public void ByDefaultFakesDoNotCallBaseMethods()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.False(bar.Gone);
            }
        }

        [Fact]
        public void ByDefaultFakesRespondToCalls()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Resolve<IBar>();
                var result = bar.Spawn();
                Assert.NotNull(result);
            }
        }

        [Fact]
        public void CanResolveFakesWhichCallsBaseMethods()
        {
            using (var fake = new AutoFake(callsBaseMethods: true))
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.True(bar.Gone);
            }
        }

        [Fact]
        public void CanResolveFakesWhichCallsBaseMethodsAndInvokeAbstractMethod()
        {
            using (var fake = new AutoFake(callsBaseMethods: true))
            {
                var bar = fake.Resolve<Bar>();
                bar.GoAbstractly();
            }
        }

        [Fact]
        public void CanResolveFakesWhichInvokeActionsWhenResolved()
        {
            var resolvedFake = (object)null;
            using (var fake = new AutoFake(configureFake: obj => resolvedFake = obj))
            {
                var bar = fake.Resolve<IBar>();
                Assert.Same(bar, resolvedFake);
            }
        }

        [Fact]
        public void CanResolveStrictFakes()
        {
            using (var fake = new AutoFake(strict: true))
            {
                var foo = fake.Resolve<Foo>();
                Assert.Throws<ExpectationException>(() => foo.Go());
            }
        }

        [Fact]
        public void ProvidesImplementations()
        {
            using (var fake = new AutoFake())
            {
                var baz = fake.Provide<IBaz, Baz>();

                Assert.NotNull(baz);
                Assert.True(baz is Baz);
            }
        }

        [Fact]
        public void ProvidesInstances()
        {
            using (var fake = new AutoFake())
            {
                var bar = A.Fake<IBar>();
                fake.Provide(bar);

                var foo = fake.Resolve<Foo>();
                foo.Go();

                A.CallTo(() => bar.Go()).MustHaveHappened();
            }
        }

        [Fact]
        public void CallsBaseMethodsOverridesStrict()
        {
            // A characterization test, intended to detect accidental changes in behavior.
            // This is an odd situation, since specifying both strict and callsBaseMethods only makes
            // sense when there are concrete methods on the fake that we want to be executed, but we
            // want to reject the invocation of any methods that are left abstract on the faked type.
            using (var fake = new AutoFake(callsBaseMethods: true, strict: true))
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.True(bar.Gone);
            }
        }

        [Fact]
        public void CallsBaseMethodsOverridesConfigureFake()
        {
            // A characterization test, intended to detect accidental changes in behavior.
            // Since callsBaseMethods applies globally and configureFake can affect individual
            // members, having configureFake override callsBaseMethods may be preferred.
            using (var fake = new AutoFake(
                callsBaseMethods: true,
                configureFake: f => A.CallTo(() => ((Bar)f).Go()).DoesNothing()))
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.True(bar.Gone);
            }
        }

        [Fact]
        public void ConfigureFakeOverridesStrict()
        {
            using (var fake = new AutoFake(
                strict: true,
                configureFake: f => A.CallTo(() => ((Bar)f).Go()).DoesNothing()))
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
            }
        }

        [Fact]
        public void ReturnsInstanceFromChildScope()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Resolve<IBar>();
                fake.Provide(new Baz());
                var bar2 = fake.Resolve<IBar>();

                Assert.Equal(bar, bar2);
            }
        }

        public abstract class Bar : IBar
        {
            private bool _gone;

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }

            public IBar Spawn()
            {
                throw new NotImplementedException();
            }

            public abstract void GoAbstractly();
        }

        public class Baz : IBaz
        {
            private bool _gone;

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }
        }

        public class Foo
        {
            private readonly IBar _bar;

            private readonly IBaz _baz;

            public Foo(IBar bar, IBaz baz)
            {
                this._bar = bar;
                this._baz = baz;
            }

            public virtual void Go()
            {
                this._bar.Go();
                this._baz.Go();
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class ForTestAttribute : Attribute
        {
        }
    }
}
