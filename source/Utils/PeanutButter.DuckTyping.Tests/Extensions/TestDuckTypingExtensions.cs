using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using PeanutButter.Utils.Dictionaries;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable NotNullMemberIsNotInitialized
// ReSharper disable NotAccessedField.Local
// ReSharper disable ConvertToLocalFunction
// ReSharper disable InconsistentNaming
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverQueried.Global

namespace PeanutButter.DuckTyping.Tests.Extensions;

[TestFixture]
public class TestDuckTypingExtensions
{
    public interface IHasReadOnlyName
    {
        string Name { get; }
    }

    [Test]
    public void
        CanDuckAs_GivenTypeWithOnePropertyAndObjectWhichDoesNotImplement_ShouldReturnFalse()
    {
        //--------------- Arrange -------------------
        var obj = new
        {
            Id = GetRandomInt()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = obj.CanDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.False();
    }

    [Test]
    public void
        CanDuckAs_GivenTypeWithOnePropertyAndObjectImplementingProperty_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var obj = new
        {
            Name = GetRandomString()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = obj.CanDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    public interface IHasReadWriteName
    {
        string Name { get; set; }
    }

    public class HasReadOnlyName
    {
        public string Name { get; }
    }

    [Test]
    public void CanDuckAs_ShouldRequireSameReadWritePermissionsOnProperties()
    {
        //--------------- Arrange -------------------
        var obj = new HasReadOnlyName();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result1 = obj.CanDuckAs<IHasReadWriteName>();
        var result2 = obj.CanDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result1)
            .To.Be.False();
        Expect(result2)
            .To.Be.True();
    }

    [Test]
    public void
        DuckAs_GivenThrowOnErrorIsTrue_WhenHaveReadWriteMismatch_ShouldGiveBackErrorInException()
    {
        //--------------- Arrange -------------------
        var obj = new HasReadOnlyName();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var ex = Assert.Throws<UnDuckableException>(() => obj.DuckAs<IHasReadWriteName>(true));

        //--------------- Assert -----------------------
        var error = ex.Errors.Single();
        Expect(error)
            .To.Contain("Mismatched target accessors for Name").And("get -> get/set");
    }


    public class HasReadWriteNameAndId
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Test]
    public void CanDuckAs_ShouldReturnTrueWhenObjectImplementsMoreThanRequiredInterface()
    {
        //--------------- Arrange -------------------
        var obj = new HasReadWriteNameAndId();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result1 = obj.CanDuckAs<IHasReadOnlyName>();
        var result2 = obj.CanDuckAs<IHasReadWriteName>();

        //--------------- Assert -----------------------
        Expect(result1)
            .To.Be.True();
        Expect(result2)
            .To.Be.True();
    }

    public interface ICow
    {
        void Moo();
    }

    public class Duck
    {
        public void Quack()
        {
        }
    }

    [Test]
    public void CanDuckAs_ShouldReturnFalseWhenSrcObjectIsMissingInterfaceMethod()
    {
        //--------------- Arrange -------------------
        var src = new Duck();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.CanDuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.False();
    }

    public class Cow
    {
        public void Moo()
        {
        }
    }

    [Test]
    public void CanDuckAs_ShouldReturnTrueWhenRequiredMethodsExist()
    {
        //--------------- Arrange -------------------
        var src = new Cow();
        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.CanDuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    public class AutoCow
    {
        // ReSharper disable once UnusedParameter.Global
        public void Moo(int howManyTimes)
        {
            /* Empty on purpose */
        }
    }

    [Test]
    public void CanDuckAs_ShouldReturnFalseWhenMethodParametersMisMatch()
    {
        //--------------- Arrange -------------------
        var src = new AutoCow();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.CanDuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.False();
    }


    [Test]
    public void DuckAs_OperatingOnNull_ShouldReturnNull()
    {
        //--------------- Arrange -------------------
        var src = null as object;

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        // ReSharper disable once ExpressionIsAlwaysNull
        var result = src.DuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.Null();
    }

    [Test]
    public void DuckAs_OperatingDuckable_ShouldReturnDuckTypedWrapper()
    {
        //--------------- Arrange -------------------
        var expected = GetRandomString();
        Func<object> makeSource = () =>
            new
            {
                Name = expected
            };
        var src = makeSource();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.DuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Name)
            .To.Equal(expected);
    }

    [Test]
    public void DuckAs_OperatingOnNonDuckable_ShouldReturnNull()
    {
        //--------------- Arrange -------------------
        Func<object> makeSource = () =>
            new
            {
                Name = GetRandomString()
            };
        var src = makeSource();
        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.DuckAs<IHasReadWriteName>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.Null();
    }

    [Test]
    public void CanFuzzyDuckAs_OperatingOnSimilarPropertiedThing_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var thing = new
        {
            nAmE = GetRandomString()
        } as object;

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = thing.CanFuzzyDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    public class LowerCaseCow
    {
        // ReSharper disable once InconsistentNaming
        public void moo()
        {
        }
    }

    [Test]
    public void CanFuzzyDuckAs_OperatingOnSimilarThingWithMethods_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var cow = new LowerCaseCow();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = cow.CanFuzzyDuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }


    [Test]
    public void FuzzyDuckAs_OperatingOnObjectWhichFuzzyMatchesProperties_ShouldReturnFuzzyDuck()
    {
        //--------------- Arrange -------------------
        var src = new
        {
            nAmE = GetRandomString()
        } as object;

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.FuzzyDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
    }


    [Test]
    public void FuzzyDuckAs_OperatingOnObjectWithFuzzyMatchingMethods_ShouldReturnFuzzyDuck()
    {
        //--------------- Arrange -------------------
        var src = new LowerCaseCow();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.FuzzyDuckAs<ICow>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
    }

    public interface ISomeActivityParameters : IActivityParameters<Guid>
    {
    }

    [Test]
    public void DuckAs_ShouldNotBeConfusedByInterfaceInheritance()
    {
        //--------------- Arrange -------------------
        var src = new
        {
            ActorId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            Payload = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.DuckAs<ISomeActivityParameters>();

        //--------------- Assert -----------------------
        Assert.That(() => result.ActorId, Throws.Nothing);
        Assert.That(() => result.TaskId, Throws.Nothing);
        Assert.That(() => result.Payload, Throws.Nothing);
    }

    [Test]
    public void DuckAs_ShouldNotSmashPropertiesOnObjectType()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            Payload = new
            {
                Id = 1,
                Name = "Moosicle"
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.DuckAs<IInterfaceWithPayload>();

        //--------------- Assert -----------------------
        var props = result.Payload.GetType().GetProperties();
        Assert.That(props.Select(p => p.Name), Does.Contain("Id"));
        Assert.That(props.Select(p => p.Name), Does.Contain("Name"));
    }

    public interface IInterfaceWithInterfacedPayload
    {
        IInterfaceWithPayload OuterPayload { get; set; }
    }

    public interface IInterfaceWithPayload
    {
        object Payload { get; }
    }


    [Test]
    public void DuckAs_WhenShouldNotBeAbleToDuckDueToAccessDifferences_ShouldNotDuckSubProp()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            OuterPayload = new
            {
                Color = "Red"
            } // Interface property is read/write, but this is a property on an anonymous object
        };

        Expect(input.CanDuckAs<IInterfaceWithInterfacedPayload>()).To.Be.False();
        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.DuckAs<IInterfaceWithInterfacedPayload>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.Null();
    }

    public class RestrictionTest
    {
        private string _writeOnly;
        public string ReadOnly { get; }

        public string WriteOnly
        {
            set => _writeOnly = value;
        }

        public string ReadWrite { get; set; }
    }

    [Test]
    public void IsNoMoreRestrictiveThan()
    {
        // Arrange
        var type = typeof(RestrictionTest);
        var readonlyProp = type.GetProperty("ReadOnly");
        var writeOnlyProp = type.GetProperty("WriteOnly");
        var readWriteProp = type.GetProperty("ReadWrite");

        // Pre-Assert

        // Act
        Expect(readWriteProp.IsNoMoreRestrictiveThan(readonlyProp)).To.Be.True();
        Expect(readWriteProp.IsNoMoreRestrictiveThan(writeOnlyProp)).To.Be.True();

        Expect(readonlyProp.IsNoMoreRestrictiveThan(writeOnlyProp)).To.Be.False();
        Expect(readonlyProp.IsNoMoreRestrictiveThan(readWriteProp)).To.Be.False();

        Expect(writeOnlyProp.IsNoMoreRestrictiveThan(readonlyProp)).To.Be.False();
        Expect(writeOnlyProp.IsNoMoreRestrictiveThan(readWriteProp)).To.Be.False();

        // Assert
    }

    [Test]
    public void IsMoreRestrictiveThan()
    {
        // Arrange
        var type = typeof(RestrictionTest);
        var readonlyProp = type.GetProperty("ReadOnly");
        var writeOnlyProp = type.GetProperty("WriteOnly");
        var readWriteProp = type.GetProperty("ReadWrite");

        // Pre-Assert

        // Act
        Expect(readWriteProp.IsMoreRestrictiveThan(readonlyProp)).To.Be.False();
        Expect(readWriteProp.IsMoreRestrictiveThan(writeOnlyProp)).To.Be.False();

        Expect(readonlyProp.IsMoreRestrictiveThan(writeOnlyProp)).To.Be.True();
        Expect(readonlyProp.IsMoreRestrictiveThan(readWriteProp)).To.Be.True();

        Expect(writeOnlyProp.IsMoreRestrictiveThan(readonlyProp)).To.Be.True();
        Expect(writeOnlyProp.IsMoreRestrictiveThan(readWriteProp)).To.Be.True();

        // Assert
    }

    public class InterfaceWithPayloadImpl : IInterfaceWithPayload
    {
        public object Payload { get; set; }
    }

    public interface IObjectIdentifier
    {
        object Identifier { get; }
    }

    public interface IGuidIdentifier
    {
        Guid Identifier { get; }
    }

    [Test]
    public void CanDuckAs_ShouldNotTreatGuidAsObject()
    {
        //--------------- Arrange -------------------
        var inputWithGuid = new
        {
            Identifier = new Guid(),
        };
        var inputWithObject = new
        {
            Identifier = new object()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(inputWithGuid.CanDuckAs<IObjectIdentifier>()).To.Be.True();
        Expect(inputWithGuid.CanDuckAs<IGuidIdentifier>()).To.Be.True();
        Expect(inputWithObject.CanDuckAs<IGuidIdentifier>()).To.Be.False();
        Expect(inputWithObject.CanDuckAs<IObjectIdentifier>()).To.Be.True();

        //--------------- Assert -----------------------
    }

    [Test]
    public void CanFuzzyDuckAs_ShouldNotTreatGuidAsObject()
    {
        //--------------- Arrange -------------------
        var inputWithGuid = new
        {
            identifier = new Guid(),
        };
        var inputWithObject = new
        {
            identifier = new object()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(inputWithGuid.CanFuzzyDuckAs<IObjectIdentifier>()).To.Be.True();
        Expect(inputWithGuid.CanFuzzyDuckAs<IGuidIdentifier>()).To.Be.True();
        Expect(inputWithObject.CanFuzzyDuckAs<IGuidIdentifier>()).To.Be.False();
        Expect(inputWithObject.CanFuzzyDuckAs<IObjectIdentifier>()).To.Be.True();

        //--------------- Assert -----------------------
    }

    public interface IWithStringId
    {
        string Id { get; set; }
    }

    [Test]
    public void FuzzyDuckAs_WhenReadingProperty_ShouldBeAbleToConvertBetweenGuidAndString()
    {
        //--------------- Arrange -------------------
        var input = new WithGuidId()
        {
            id = Guid.NewGuid()
        };
        var expected = input.id.ToString();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var ducked = input.FuzzyDuckAs<IWithStringId>();

        //--------------- Assert -----------------------
        Expect(ducked)
            .Not.To.Be.Null();
        Expect(ducked.Id)
            .To.Equal(expected);
    }

    public interface IWithGuidId
    {
        Guid Id { get; set; }
    }

    public class WithGuidId
    {
        public Guid id { get; set; }
    }

    public class WithStringId
    {
        public string id { get; set; }
    }

    [Test]
    public void FuzzyDuckAs_WhenReadingProperty_ShouldBeAbleToConvertFromStringToGuid()
    {
        //--------------- Arrange -------------------
        var expected = Guid.NewGuid();
        var input = new WithStringId()
        {
            id = expected.ToString()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var ducked = input.FuzzyDuckAs<IWithGuidId>();

        //--------------- Assert -----------------------
        Expect(ducked)
            .Not.To.Be.Null();
        Expect(ducked.Id)
            .To.Equal(expected);
    }

    [Test]
    public void FuzzyDuckAs_WhenWritingProperty_ShouldBeAbleToConvertFromGuidToString()
    {
        //--------------- Arrange -------------------
        var newValue = Guid.NewGuid();
        var expected = newValue.ToString();
        var input = new WithStringId()
        {
            id = GetRandomString()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var ducked = input.FuzzyDuckAs<IWithGuidId>();

        //--------------- Assert -----------------------
        Expect(ducked)
            .Not.To.Be.Null();
        ducked.Id = newValue;
        Expect(input.id)
            .To.Equal(expected);
        Expect(ducked.Id)
            .To.Equal(newValue);
    }

    [Test]
    public void FuzzyDuckAs_WhenWritingProperty_ShouldBeAbleToConvertFromValidGuidStringToGuid()
    {
        //--------------- Arrange -------------------
        var newGuid = Guid.NewGuid();
        var newValue = newGuid.ToString();
        var input = new WithGuidId();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var ducked = input.FuzzyDuckAs<IWithStringId>();

        //--------------- Assert -----------------------
        Expect(ducked)
            .Not.To.Be.Null();
        ducked.Id = newValue;
        Expect(ducked.Id)
            .To.Equal(newValue);
        Expect(input.id)
            .To.Equal(newGuid);
    }


    public interface IHasAnActorId
    {
        Guid ActorId { get; }
    }

    public interface IActivityParametersInherited : IHasAnActorId
    {
        Guid TaskId { get; }
    }

    [Test]
    public void CanFuzzyDuckAs_ShouldFailWhenExpectedToFail()
    {
        //--------------- Arrange -------------------
        var parameters = new
        {
            travellerId = new Guid(), // should be actorId!
            taskId = new Guid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = parameters.CanFuzzyDuckAs<IActivityParametersInherited>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.False();
    }

    [Test]
    public void
        FuzzyDuckAs_NonGeneric_ActingOnObject_ShouldThrowWhenInstructedToAndFailingToDuck()
    {
        //--------------- Arrange -------------------
        var parameters = new
        {
            travellerId = new Guid(), // should be actorId!
            taskId = new Guid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true))
            .To.Throw<UnDuckableException>();

        //--------------- Assert -----------------------
    }

    [Test]
    public void
        FuzzyDuckAs_NonGeneric_ActingOnDictionary_ShouldThrowWhenInstructedToAndFailingToDuck()
    {
        //--------------- Arrange -------------------
        var parameters = new Dictionary<string, object>()
        {
            {
                "travellerId", new Guid()
            },
            {
                "taskIdMoo", new Guid()
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(() => parameters.FuzzyDuckAs(typeof(IActivityParametersInherited), true))
            .To.Throw<UnDuckableException>();

        //--------------- Assert -----------------------
    }

    [Test]
    public void ForcedConcreteDuckShouldWorkNeatly()
    {
        // Arrange
        var data = new Dictionary<string, object>()
        {
            ["id"] = 1,
            ["name"] = "moo-cow"
        };
        // Act
        var result = data.ForceFuzzyDuckAs<ForceMe>(forceConcreteType: true);
        // Assert
        // virtual member will work
        Expect(result.Name)
            .To.Equal("moo-cow");
        Expect(result.Id)
            .To.Equal(0);
        Expect(result.Get<int>(nameof(ForceMe.Id)))
            .To.Equal(1);
        result.Set(nameof(ForceMe.Id), 3);
        // fixme: fuzzy ducks don't write back to their
        // underlying dictionaries!
        Expect(result.Get<int>(nameof(ForceMe.Id)))
            .To.Equal(3);
        Expect(data["id"])
            .To.Equal(3);
    }

    public class ForceMe
    {
        public int Id { get; set; }
        public virtual string Name { get; set; }
    }


    public interface IStringCollection
    {
        string Moo { get; }
    }

    [Test]
    public void DuckAs_ActingOnStringStringDictionary()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            ["Moo"] = "Cow"
        };

        // Pre-assert
        // Act
        var result = data.DuckAs<IStringCollection>();
        // Assert
        Expect(result.Moo)
            .To.Equal("Cow");
    }

    public interface IConvertMe
    {
        bool Flag { get; }
        int Number { get; }
    }

    [Test]
    public void DuckAs_ActingOnStringStringDict_WithConversionProps()
    {
        // Arrange
        var data = new Dictionary<string, string>()
        {
            ["Flag"] = "true",
            ["Number"] = "42"
        };
        // Pre-assert
        // Act
        try
        {
            var result = data.FuzzyDuckAs<IConvertMe>(true);
            // Assert
            Expect(result.Flag)
                .To.Be.True();
            Expect(result.Number)
                .To.Equal(42);
        }
        catch (UnDuckableException e)
        {
            e.Errors.ForEach(er => Console.WriteLine(er));
            throw;
        }
    }

    public interface IDictionaryInner
    {
        string Name { get; set; }
    }

    public interface IDictionaryOuter
    {
        int Id { get; set; }
        IDictionaryInner Inner { get; set; }
    }


    [Test]
    public void
        CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenAllPropertiesAreFound_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var expected = GetRandomString();
        var data = new Dictionary<string, object>()
        {
            {
                "Name", expected
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.CanDuckAs<IDictionaryInner>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    [Test]
    public void
        CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenNullablePropertyIsFound_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var data = new Dictionary<string, object>()
        {
            {
                "Name", null
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.CanDuckAs<IDictionaryInner>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    public interface IHaveId
    {
        int Id { get; set; }
    }

    [Test]
    public void
        CanDuckAs_OperatingOnSingleLevelDictionaryOfStringAndObject_WhenNonNullablePropertyIsFoundAsNull_ShouldReturnFalse()
    {
        //--------------- Arrange -------------------
        var data = new Dictionary<string, object>()
        {
            {
                "Id", null
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.CanDuckAs<IHaveId>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.False();
    }

    [Test]
    public void
        CanDuckAs_OperatingOnMultiLevelDictionary_WhenAllPropertiesFound_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var data = new Dictionary<string, object>()
        {
            {
                "Id", GetRandomInt()
            },
            {
                "Inner", new Dictionary<string, object>()
                {
                    {
                        "Name", GetRandomString()
                    }
                }
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.CanDuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    [Test]
    public void DuckAs_OperatingOnDictionaryOfStringAndObject_WhenIsDuckable_ShouldDuck()
    {
        //--------------- Arrange -------------------
        var expectedId = GetRandomInt();
        var expectedName = GetRandomString();
        var input = new Dictionary<string, object>()
        {
            {
                "Id", expectedId
            },
            {
                "Inner", new Dictionary<string, object>()
                {
                    {
                        "Name", expectedName
                    }
                }
            }
        };
        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.DuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expectedId);
        Expect(result.Inner)
            .Not.To.Be.Null();
        Expect(result.Inner.Name)
            .To.Equal(expectedName);
    }

    [Test]
    public void
        CanFuzzyDuckAs_OperatingOnAppropriateCaseInsensitiveDictionary_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var expectedId = GetRandomInt();
        var expectedName = GetRandomString();
        var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "id", expectedId
            },
            {
                "inner", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "nAmE", expectedName
                    }
                }
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.CanFuzzyDuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    [Test]
    public void FuzzyDuckAs_OperatingOnCaseInsensitiveDictionary_ShouldWork()
    {
        //--------------- Arrange -------------------
        var expectedId = GetRandomInt();
        var expectedName = GetRandomString();
        var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "id", expectedId
            },
            {
                "inner", new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "nAmE", expectedName
                    }
                }
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expectedId);
        Expect(result.Inner)
            .Not.To.Be.Null();
        Expect(result.Inner.Name)
            .To.Equal(expectedName);
    }


    [Test]
    public void
        CanFuzzyDuckAs_OperatingOnWouldBeAppropriateCaseSensitiveDictionary_ShouldReturnTrue()
    {
        //--------------- Arrange -------------------
        var expectedId = GetRandomInt();
        var expectedName = GetRandomString();
        var input = new Dictionary<string, object>()
        {
            {
                "id", expectedId
            },
            {
                "inner", new Dictionary<string, object>()
                {
                    {
                        "nAmE", expectedName
                    }
                }
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.CanFuzzyDuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------
        Expect(result)
            .To.Be.True();
    }

    [Test]
    public void FuzzyDuckAs_OperatingOnCaseDifferentCaseSensitiveDictionary_ShouldReturnObject()
    {
        //--------------- Arrange -------------------
        var expectedId = GetRandomInt();
        var expectedName = GetRandomString();
        var input = new Dictionary<string, object>()
        {
            {
                "id", expectedId
            },
            {
                "inner", new Dictionary<string, object>()
                {
                    {
                        "nAmE", expectedName
                    }
                }
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IDictionaryOuter>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expectedId);
        Expect(result.Inner)
            .Not.To.Be.Null();
        Expect(result.Inner.Name)
            .To.Equal(expectedName);
    }

    [Test]
    public void DuckAs_IssueSeenInWildShouldNotHappen()
    {
        //--------------- Arrange -------------------
        var instance = new ActivityParameters<string>(Guid.Empty, Guid.Empty, "foo");

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = instance.DuckAs<ISpecificActivityParameters>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
    }

    public interface IHasAGuid
    {
        Guid TaskId { get; set; }
    }

    [Test]
    public void FuzzyDuckAs_ShouldBeAbleToDuckADictionaryWithConvertableTypes()
    {
        //--------------- Arrange -------------------
        var id = Guid.NewGuid();
        var data = new Dictionary<string, object>()
        {
            {
                "taskId", id.ToString()
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = data.FuzzyDuckAs<IHasAGuid>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.TaskId)
            .To.Equal(id);
    }

    public interface IWorkflowTaskStatusFilters
    {
        string[] Statuses { get; }
    }


    [Test]
    public void FuzzyDuckAs_ShouldBeAbleToDuckSimpleObjectWithStringArray()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            Statuses = new[]
            {
                "foo",
                "bar"
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IWorkflowTaskStatusFilters>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Statuses)
            .To.Contain.Exactly(1).Equal.To("foo");
        Expect(result.Statuses)
            .To.Contain.Exactly(1).Equal.To("bar");
    }

    public class MooAttribute : Attribute
    {
        public string Dialect { get; }

        public MooAttribute(string dialect)
        {
            Dialect = dialect;
        }
    }

    public class WoofAttribute : Attribute
    {
        public string Intent { get; }

        public WoofAttribute(string intent)
        {
            Intent = intent;
        }
    }

    public class NamedArgumentAttribute : Attribute
    {
        public string NamedProperty { get; set; }
        public string NamedField;
    }

    [Woof("playful")]
    public interface IHasCustomAttributes
    {
        [Moo("northern")]
        [NamedArgument(NamedProperty = "whizzle", NamedField = "nom")]
        string Name { get; }
    }

    [Test]
    public void DuckAs_ShouldCopyCustomAttributes_OnProperties()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            name = "cow"
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasCustomAttributes>();

        //--------------- Assert -----------------------
        var propInfo = result.GetType().GetProperty("Name");
        Expect(propInfo)
            .Not.To.Be.Null();
        var attrib = propInfo.GetCustomAttributes(false).OfType<MooAttribute>()
            .FirstOrDefault();
        Expect(attrib)
            .Not.To.Be.Null();
        Expect(attrib.Dialect)
            .To.Equal("northern");
    }

    [Test]
    public void DuckAs_ShouldCopyNamedArgumentCustomAttributes_OnProperties()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            name = "cow"
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasCustomAttributes>();

        //--------------- Assert -----------------------
        var propInfo = result.GetType().GetProperty("Name");
        Expect(propInfo)
            .Not.To.Be.Null();
        var attrib = propInfo.GetCustomAttributes(false).OfType<NamedArgumentAttribute>()
            .FirstOrDefault();

        Expect(attrib)
            .Not.To.Be.Null();
        Expect(attrib.NamedProperty)
            .To.Equal("whizzle");
        Expect(attrib.NamedField)
            .To.Equal("nom");
    }

    [Test]
    public void DuckAs_ShouldCopyCustomAttributes_OnTheInterface()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            name = "cow"
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasCustomAttributes>();

        //--------------- Assert -----------------------
        var attrib = result.GetType().GetCustomAttributes(false).OfType<WoofAttribute>()
            .FirstOrDefault();

        Expect(attrib)
            .Not.To.Be.Null();
        Expect(attrib.Intent)
            .To.Equal("playful");
    }

    public class DialectAttribute : Attribute
    {
        public string Dialect { get; set; }

        public DialectAttribute(string dialect)
        {
            Dialect = dialect;
        }
    }

    public interface IRegionSpecificCow
    {
        [Dialect("Country")]
        string Moo { get; }
    }

    [Test]
    public void DuckAs_ShouldCopyComplexCustomAttributes()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            moo = "Moo, eh"
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IRegionSpecificCow>();

        //--------------- Assert -----------------------
        var propInfo = result.GetType().GetProperty("Moo");
        Expect(propInfo)
            .Not.To.Be.Null();
        var attrib = propInfo.GetCustomAttributes(true).OfType<DialectAttribute>()
            .FirstOrDefault();
        Expect(attrib)
            .Not.To.Be.Null();
        Expect(attrib.Dialect)
            .To.Equal("Country");
    }


    [Test]
    public void FuzzyDuckAsNonGeneric_ShouldDuckWhenPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            actorId = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.FuzzyDuckAs(toType);

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.GetPropertyValue("ActorId")).To.Equal(src.actorId);
    }

    [Test]
    public void DuckAsNonGeneric_ShouldDuckWhenPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            ActorId = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.DuckAs(toType);

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.GetPropertyValue("ActorId")).To.Equal(src.ActorId);
    }

    [Test]
    public void DuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldDuckWhenPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            ActorId = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.DuckAs(toType, true);

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.GetPropertyValue("ActorId")).To.Equal(src.ActorId);
    }

    [Test]
    public void DuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldThrowWhenNotPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            bob = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(() => src.DuckAs(toType, true))
            .To.Throw<UnDuckableException>();

        //--------------- Assert -----------------------
    }

    [Test]
    public void FuzzyDuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldDuckWhenPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            actoRId = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = src.FuzzyDuckAs(toType, true);

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.GetPropertyValue("ActorId")).To.Equal(src.actoRId);
    }

    [Test]
    public void FuzzyDuckAs_NonGenericWhenThrowOnErrorSetTrue_ShouldThrowWhenNotPossible()
    {
        //--------------- Arrange -------------------
        var toType = typeof(IHasAnActorId);
        var src = new
        {
            bob = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------

        Expect(() => src.FuzzyDuckAs(toType, true))
            .To.Throw<UnDuckableException>();

        //--------------- Assert -----------------------
    }

    public interface IHasNullableId
    {
        Guid? Id { get; set; }
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnDictionary_WhenSourcePropertyIsNullableAndMissing_SHouldDuckAsNullProperty()
    {
        //--------------- Arrange -------------------
        var input = new Dictionary<string, object>();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasNullableId>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Be.Null();
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnDictionary_WhenSourcePropertyIsGenericNullableAndMissing_ShouldDuckAsNullProperty()
    {
        //--------------- Arrange -------------------
        var input = new Dictionary<string, object>();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasNullableId>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Be.Null();
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnDictionary_WhenSourcePropertyIsNullableTypeAndMissing_ShouldDuckAsNullProperty()
    {
        //--------------- Arrange -------------------
        var input = new Dictionary<string, string>();

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasReadOnlyName>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Name)
            .To.Be.Null();
    }

    public interface IHasNullableReadonlyId
    {
        Guid? Id { get; }
    }

    [Test]
    public void
        DuckAs_OperatingOnObjectWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadOnlyProperty_ShouldDuck()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            Id = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.DuckAs<IHasNullableReadonlyId>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(input.Id);
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnObjectWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadOnlyProperty_ShouldDuck()
    {
        //--------------- Arrange -------------------
        var input = new
        {
            id = Guid.NewGuid()
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasNullableReadonlyId>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(input.id);
    }

    [Test]
    public void
        DuckAs_OperatingOnDictionaryWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadonlyProperty_ShouldDuck()
    {
        //--------------- Arrange -------------------
        var expected = Guid.NewGuid();
        var input = new Dictionary<string, object>()
        {
            {
                "Id", expected
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.DuckAs<IHasNullableReadonlyId>();

        //--------------- Assert -----------------------

        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expected);
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnDictionaryWithNotNullableProperty_WhenRequestedInterfaceHasNullableReadonlyProperty_ShouldFuzzyDuck()
    {
        //--------------- Arrange -------------------
        var expected = Guid.NewGuid();
        var input = new Dictionary<string, object>()
        {
            {
                "Id", expected
            }
        };

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = input.FuzzyDuckAs<IHasNullableReadonlyId>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Id)
            .To.Equal(expected);
    }

    public interface IHasFlag
    {
        bool? Flag { get; }
        int? Number1 { get; }
        long? Number2 { get; }
        byte? Byte { get; }
        DateTime? TheDate { get; }
        TimeSpan? TheTimeSpan { get; }
    }

    [Test]
    public void FuzzyDuckingToNullableProps()
    {
        var input = new
        {
            flag = "true",
            bYtE = "100",
            number1 = "1",
            number2 = "2",
            theTimeSpan = "",
            theDate = "2015-01-01"
        };
        // Arrange
        // Act
        try
        {
            var result = input.ForceFuzzyDuckAs<IHasFlag>();
            // Assert
            Expect(result.Flag)
                .To.Be.True();
            Expect(result.Number1)
                .To.Equal(1);
            Expect(result.Number2)
                .To.Equal(2);
            Expect(result.Byte)
                .To.Equal((byte)100);
            Expect(result.TheDate)
                .To.Equal(new DateTime(2015, 1, 1));
            Expect(result.TheTimeSpan)
                .To.Be.Null();
        }
        catch (UnDuckableException ex)
        {
            Console.WriteLine(ex.Errors.JoinWith("\n"));
            throw;
        }
    }

    public interface IAnimal
    {
        int Id { get; }
        string Name { get; }
    }

    public class AnimalConverter : IConverter<string, IAnimal>
    {
        public class Animal : IAnimal
        {
            public int Id { get; }
            public string Name { get; }

            public Animal(
                int id,
                string name
            )
            {
                Id = id;
                Name = name;
            }
        }

        public Type T1 => typeof(string);
        public Type T2 => typeof(IAnimal);

        public string Convert(IAnimal input)
        {
            return $"{input?.Id}/{input?.Name}";
        }

        public IAnimal Convert(string input)
        {
            var parts = (input ?? "").Split('/');
            if (int.TryParse(parts[0], out var id))
            {
                return new Animal(id, parts.Skip(1).FirstOrDefault());
            }

            return null;
        }

        public bool CanConvert(
            Type t1,
            Type t2
        )
        {
            return (t1 == T1 || t1 == T2) &&
                (t2 == T1 || t2 == T2) &&
                t1 != t2;
        }

        public bool IsInitialised => true;
    }

    [Test]
    public void AddingCustomConverter()
    {
        // Arrange
        var input = "1/bob";
        // Act
        try
        {
            var result = input.FuzzyDuckAs<IAnimal>(true);
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Id)
                .To.Equal(1);
            Expect(result.Name)
                .To.Equal("bob");
        }
        catch (UnDuckableException ex)
        {
            Console.WriteLine(ex.Errors.JoinWith("\n"));
            throw;
        }
    }


    [Test]
    public void FailingWildDuck1()
    {
        //--------------- Arrange -------------------
        var json =
            "{\"flowId\":\"Travel Request\",\"activityId\":\"Capture Travel Request Details\",\"payload\":{\"taskId\":\"4e53c85b-ca72-4c12-b185-50342ed0fc30\",\"payload\":{\"Initiated\":\"\",\"DepartingFrom\":\"123\",\"TravellingTo\":\"123\",\"Departing\":\"\",\"PreferredDepartureTime\":\"\",\"Returning\":\"\",\"PreferredReturnTime\":\"\",\"ReasonForTravel\":\"123\",\"CarRequired\":\"\",\"AccomodationRequired\":\"\",\"AccommodationNotes\":\"213\"}}}";
        var jobject = JsonConvert.DeserializeObject<JObject>(json);
        var dict = jobject.ToDictionary();
        (dict["payload"] as Dictionary<string, object>)["actorId"] = Guid.Empty.ToString();
        var payload = dict["payload"];

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        var result = payload.FuzzyDuckAs<ITravelRequestCaptureDetailsActivityParameters>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
    }

    [Test]
    public void
        ForceFuzzyDuckAs_GivenEmptyDictionaryAndInterfaceToMimic_WhenCanWriteBack_ShouldWriteBack()
    {
        //--------------- Arrange -------------------
        // TODO: provide a shimming layer so that the input dictionary doesn't have to be case-insensitive to allow write-back
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var expected = new TravelRequestDetails()
        {
            Initiated = GetRandomDate(),
            DepartingFrom = GetRandomString(),
            TravellingTo = GetRandomString(),
            Departing = GetRandomDate(),
            PreferredDepartureTime = GetRandomString(),
            Returning = GetRandomDate(),
            PreferredReturnTime = GetRandomString(),
            ReasonForTravel = GetRandomString(),
            CarRequired = GetRandomBoolean(),
            AccomodationRequired = GetRandomBoolean(),
            AccommodationNotes = GetRandomString()
        };
        var expectedDuck = expected.DuckAs<ITravelRequestDetails>();

        //--------------- Assume ----------------
        Expect(expectedDuck)
            .Not.To.Be.Null();

        //--------------- Act ----------------------
        var result = dict.ForceFuzzyDuckAs<ITravelRequestDetails>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        // ReSharper disable once IsExpressionAlwaysTrue
        Expect(result is ITravelRequestDetails)
            .To.Be.True();
        Expect(() =>
                {
                    result.Initiated = expectedDuck.Initiated;
                    result.DepartingFrom = expectedDuck.DepartingFrom;
                    result.TravellingTo = expectedDuck.TravellingTo;
                    result.Departing = expectedDuck.Departing;
                    result.PreferredDepartureTime = expectedDuck.PreferredDepartureTime;
                    result.Returning = expectedDuck.Returning;
                    result.PreferredReturnTime = expectedDuck.PreferredReturnTime;
                    result.ReasonForTravel = expectedDuck.ReasonForTravel;
                    result.CarRequired = expectedDuck.CarRequired;
                    result.AccomodationRequired = expectedDuck.AccomodationRequired;
                    result.AccommodationNotes = expectedDuck.AccommodationNotes;
                }
            )
            .Not.To.Throw();

        foreach (var prop in result.GetType().GetProperties())
        {
            Expect(dict[prop.Name])
                .To.Equal(prop.GetValue(result));
        }
    }

    public interface ISillyConfig
    {
        bool Moo { get; }
        bool Cake { get; }
    }

    [Test]
    public void ForceFuzzyDuckAs_Wild()
    {
        // Arrange
        var dict = new Dictionary<string, object>()
        {
            ["moo"] = true
        };
        // Pre-Assert
        // Act
        var result = dict.ForceFuzzyDuckAs<ISillyConfig>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Moo)
            .To.Be.True();
        Expect(result.Cake)
            .To.Be.False();
    }

    [Test]
    public void ForceDuckAs_GivenEmptyDictionaryAndInterfaceToMimic_ShouldHandleIt()
    {
        //--------------- Arrange -------------------
        var dict = new Dictionary<string, object>();
        var expected = new TravelRequestDetails()
        {
            Initiated = GetRandomDate(),
            DepartingFrom = GetRandomString(),
            TravellingTo = GetRandomString(),
            Departing = GetRandomDate(),
            PreferredDepartureTime = GetRandomString(),
            Returning = GetRandomDate(),
            PreferredReturnTime = GetRandomString(),
            ReasonForTravel = GetRandomString(),
            CarRequired = GetRandomBoolean(),
            AccomodationRequired = GetRandomBoolean(),
            AccommodationNotes = GetRandomString()
        };
        var expectedDuck = expected.DuckAs<ITravelRequestDetails>(true);

        //--------------- Assume ----------------
        Expect(expectedDuck)
            .Not.To.Be.Null();

        //--------------- Act ----------------------
        var result = dict.ForceDuckAs<ITravelRequestDetails>();

        //--------------- Assert -----------------------
        Expect(result)
            .Not.To.Be.Null();
        Expect(() =>
                {
                    result.Initiated = expectedDuck.Initiated;
                    result.DepartingFrom = expectedDuck.DepartingFrom;
                    result.TravellingTo = expectedDuck.TravellingTo;
                    result.Departing = expectedDuck.Departing;
                    result.PreferredDepartureTime = expectedDuck.PreferredDepartureTime;
                    result.Returning = expectedDuck.Returning;
                    result.PreferredReturnTime = expectedDuck.PreferredReturnTime;
                    result.ReasonForTravel = expectedDuck.ReasonForTravel;
                    result.CarRequired = expectedDuck.CarRequired;
                    result.AccomodationRequired = expectedDuck.AccomodationRequired;
                    result.AccommodationNotes = expectedDuck.AccommodationNotes;
                }
            )
            .Not.To.Throw();

        foreach (var prop in result.GetType().GetProperties())
        {
            Expect(dict[prop.Name])
                .To.Equal(prop.GetValue(result));
        }
    }

    [Test]
    public void
        DuckFail_WhenHaveBadDictionaryImplementation_GivingNullKeys_ShouldThrowRecognisableError()
    {
        // Arrange
        var src = new DictionaryWithNullKeys<string, object>();
        // Pre-Assert
        // Act
        Expect(() => src.ForceFuzzyDuckAs<ISillyConfig>())
            .To.Throw<InvalidOperationException>()
            .With.Message.Containing("Provided dictionary gives null for keys");
        // Assert
    }

    public class DictionaryWithNullKeys<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(
            KeyValuePair<TKey, TValue>[] array,
            int arrayIndex
        )
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public void Add(
            TKey key,
            TValue value
        )
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(
            TKey key,
            out TValue value
        )
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<TKey> Keys { get; }
        public ICollection<TValue> Values { get; }
    }


    [Test]
    public void FuzzyDuckOnDefaultDictionary_ShouldWork()
    {
        // Arrange
        var expected = "moo";
        var dict = new DefaultDictionary<string, object>(() => expected);
        // Pre-Assert
        Expect(dict.CanFuzzyDuckAs<IConfig>()).To.Be.True();
        // Act
        var result = dict.FuzzyDuckAs<IConfig>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.BaseUrl)
            .To.Equal(expected);
    }

    public interface IHaveFlags
    {
        bool Flag1 { get; }
        bool Flag2 { get; }
    }

    [Test]
    public void FuzzyDuckOnDefaultDictionary_Part2()
    {
        // Arrange
        var expected = true;
        var dict = new DefaultDictionary<string, object>(() => expected);
        // Pre-Assert
        Expect(dict.CanFuzzyDuckAs<IHaveFlags>()).To.Be.True();
        // Act
        var result = dict.FuzzyDuckAs<IHaveFlags>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Flag1)
            .To.Be.True();
        Expect(result.Flag2)
            .To.Be.True();
    }


    public interface IConfig
    {
        string BaseUrl { get; set; }
    }

    public interface IConfig2
    {
        int Port { get; set; }
    }

    [Test]
    public void DuckAs_OperatingOnStringStringDictionary_WhenCanDuck_ShouldDuck()
    {
        // Arrange
        var src = new Dictionary<string, string>()
        {
            ["BaseUrl"] = GetRandomHttpUrl()
        };

        // Pre-Assert

        // Act
        var result = src.DuckAs<IConfig>(true);

        // Assert
        Expect(result.BaseUrl)
            .To.Equal(src["BaseUrl"]);
    }

    [Test]
    public void
        FuzzyDuckAs_OperatingOnStringStringDictionary_ShouldForgiveWhitespaceInPropertyNames()
    {
        // Arrange
        var src = new Dictionary<string, string>()
        {
            ["base url"] = GetRandomHttpUrl()
        };

        // Pre-Assert

        // Act
        var result = src.FuzzyDuckAs<IConfig>();

        // Assert
        Expect(result.BaseUrl)
            .To.Equal(src["base url"]);
    }

    [Test]
    public void FuzzyDuckAs_OperatingOnMergeDictionaryWithFallback_ShouldDuck()
    {
        // Arrange
        var expected = GetRandomString();
        var src = new MergeDictionary<string, object>(
            new DefaultDictionary<string, object>(() => expected)
        );

        Expect(src["BaseUrl"])
            .To.Equal(expected);

        // Pre-Assert
        Expect(src.CanFuzzyDuckAs<IConfig>()).To.Be.True();

        // Act
        var result = src.FuzzyDuckAs<IConfig>();

        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.BaseUrl)
            .To.Equal(expected);
    }

    [Test]
    public void DuckAs_OperatingOnNameValueCollection_WhenCanDuck_ShouldDuckBothWays()
    {
        // Arrange
        var src = new NameValueCollection();
        var expected1 = GetRandomHttpUrl();
        var expected2 = GetRandom(o => o != expected1, GetRandomHttpUrl);
        src.Add("BaseUrl", expected1);

        // Pre-Assert

        // Act
        var result = src.DuckAs<IConfig>();

        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.BaseUrl)
            .To.Equal(expected1);
        result.BaseUrl = expected2;
        Expect(result.BaseUrl)
            .To.Equal(expected2);
        Expect(src["BaseUrl"])
            .To.Equal(expected2);
    }

    [TestCase(" ")]
    [TestCase("  ")]
    [TestCase(".")]
    [TestCase(":")]
    [TestCase("_")]
    [TestCase("-")]
    public void
        FuzzyDuckAs_OperatingOnNameValueCollection_ShouldDuck_WhenSourcePropertyIncludes(
            string chars
        )
    {
        // Arrange
        var src = new NameValueCollection();
        var expected = GetRandomHttpUrl();
        src.Add($"base{chars}Url", expected);

        // Pre-Assert

        // Act
        var result = src.FuzzyDuckAs<IConfig>();

        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.BaseUrl)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldFuzzyDuckVeryLooselyOnAlphaNumericMatch()
    {
        // Arrange
        var src = new NameValueCollection();
        var expected = GetRandomHttpUrl();
        src.Add("%^_base-url!#$", expected);
        // Act
        var result = src.FuzzyDuckAs<IConfig>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.BaseUrl)
            .To.Equal(expected);
    }

    [Test]
    public void ForceDuckAs_OperatingOnNameValueCollection_ShouldGoBothWays()
    {
        // Arrange
        var src = new NameValueCollection();
        var expected = GetRandomHttpUrl();

        // Pre-Assert

        // Act
        var result = src.ForceDuckAs<IConfig>();

        // Assert
        Expect(result.BaseUrl)
            .To.Be.Null();
        result.BaseUrl = expected;

        Expect(result.BaseUrl)
            .To.Equal(expected);
        Expect(src["BaseUrl"])
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToFuzzyDuckStringArrayToIntArray()
    {
        // Arrange
        var data = new Dictionary<string, object>()
        {
            ["numbers"] = new[]
            {
                "1",
                "2",
                "3"
            }
        };
        // Act
        var result = data.FuzzyDuckAs<IHasInts>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Numbers)
            .To.Equal(
                new[]
                {
                    1,
                    2,
                    3
                }
            );
        result.Numbers = new[]
        {
            4,
            5,
            6
        };
        Expect(data)
            .To.Contain.Key("numbers")
            .With.Value(
                new[]
                {
                    4,
                    5,
                    6
                }
            );
    }

    public interface IHasInts
    {
        int[] Numbers { get; set; }
    }

    public interface IConfigWithUnconfiguredFeature : IConfig
    {
        bool UnconfiguredFeature { get; set; }
    }

    public enum FeatureSets
    {
        Level1,
        Level2
    }

    public class FeatureConfig
    {
        public Guid Id { get; set; }

        public FeatureSets FeatureSet { get; set; }
        public Dictionary<string, bool> Settings { get; set; }
    }


    public interface IExtendedConfig : IConfig
    {
        string Name { get; set; }
    }

    [Test]
    public void ForceFuzzyDuckAs_OperatingOnNameValueCollection_ShouldGoBothWays()
    {
        // Arrange
        var src = new NameValueCollection
        {
            ["name"] = GetRandomString()
        };
        var expected = GetRandomHttpUrl();

        // Pre-Assert

        // Act
        var result = src.ForceFuzzyDuckAs<IExtendedConfig>();

        // Assert

        Expect(result.Name)
            .To.Equal(src["Name"]);
        Expect(result.BaseUrl)
            .To.Be.Null();
        result.BaseUrl = expected;
        Expect(result.BaseUrl)
            .To.Equal(expected);
        Expect(src["BaseUrl"])
            .To.Equal(expected);
    }

    [Test]
    public void DuckingToConcreteType()
    {
        // Arrange
        var data = new
        {
            Id = 1,
            Name = "Bob"
        };
        // Act
        var result = data.DuckAs<SimpleReadOnlyPoco>();
        // Assert
        Expect(result)
            .To.Deep.Equal(data);
    }

    public class SimpleReadOnlyPoco
    {
        public virtual int Id { get; }
        public virtual string Name { get; }
    }

    [TestFixture]
    public class GivenKeyTransformFunctions
    {
        [TestFixture]
        public class OperatingOnDictionary
        {
            [TestFixture]
            public class DuckAs
            {
                [Test]
                public void GivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
                {
                    // Arrange
                    var expected = GetRandomHttpUrl();
                    var data = new Dictionary<string, object>
                    {
                        ["Config.BaseUrl"] = expected
                    };

                    // Pre-Assert

                    // Act
                    var result =
                        data.DuckAs<IConfig>(
                            s => "Config." + s,
                            s => s.RegexReplace("Config.", "")
                        );

                    // Assert

                    Expect(result)
                        .Not.To.Be.Null();
                    Expect(result.BaseUrl)
                        .To.Equal(expected);
                }

                [Test]
                public void GivenKeyTransformFunctions_AndCantDuck_ShouldReturnNull()
                {
                    // Arrange
                    var expected = GetRandomHttpUrl();
                    var data = new Dictionary<string, object>
                    {
                        ["Config.BaseUrl"] = expected
                    };

                    // Pre-Assert

                    // Act
                    var result = data.DuckAs<IConfig>(
                        s => "Config." + s,
                        s => s.RegexReplace("Config.", GetRandomString(7))
                    );

                    // Assert

                    Expect(result)
                        .To.Be.Null();
                }

                [Test]
                public void GivenKeyTransformFunctionsAndMustThrow_AndCanDuck_ShouldDuck()
                {
                    // Arrange
                    var expected = GetRandomHttpUrl();
                    var data = new Dictionary<string, object>
                    {
                        ["Config.BaseUrl"] = expected
                    };

                    // Pre-Assert

                    // Act
                    var result = data.DuckAs<IConfig>(
                        s => "Config." + s,
                        s => s.RegexReplace("Config.", ""),
                        true
                    );

                    // Assert

                    Expect(result)
                        .Not.To.Be.Null();
                    Expect(result.BaseUrl)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class FuzzyDuckAs
            {
                [Test]
                public void GivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
                {
                    // Arrange
                    var expected = GetRandomHttpUrl();
                    var data = new Dictionary<string, object>
                    {
                        ["Config.baseUrl"] = expected
                    };

                    // Pre-Assert

                    // Act
                    var result =
                        data.FuzzyDuckAs<IConfig>(
                            s => "Config." + s,
                            s => s.RegexReplace("Config.", "")
                        );

                    // Assert

                    Expect(result)
                        .Not.To.Be.Null();
                    Expect(result.BaseUrl)
                        .To.Equal(expected);
                }

                [Test]
                public void GivenKeyTransformFunctions_AndCantDuck_ShouldReturnNull()
                {
                    // Arrange
                    var expected = GetRandomInt();
                    var data = new Dictionary<string, object>
                    {
                        ["Config.Port"] = expected
                    };

                    // Pre-Assert

                    // Act
                    var result = data.FuzzyDuckAs<IConfig2>(
                        s => "Config." + s,
                        s => s.RegexReplace("Config.", GetRandomString(7))
                    );

                    // Assert
                    Expect(result.Port)
                        .To.Equal(expected);
                }
            }
        }


        [Test]
        public void
            DuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new NameValueCollection
            {
                ["Config.BaseUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result =
                data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", ""));

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        [Test]
        public void
            DuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndNoThrow_AndCantDuck_ShouldReturnNull()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new NameValueCollection
            {
                ["Config.BaseUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result =
                data.DuckAs<IConfig>(s => "Config." + s, s => s.RegexReplace("Config.", "moo"));

            // Assert

            Expect(result)
                .To.Be.Null();
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctionsAndMustThrowIsTrue_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new Dictionary<string, object>
            {
                ["Config.BAseURl"] = expected
            };

            // Pre-Assert

            // Act
            var result =
                data.FuzzyDuckAs<IConfig>(
                    s => "Config." + s,
                    s => s.RegexReplace("Config.", ""),
                    true
                );

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyTransformFunctionsAndMustThrowIsTrue_AndCannotDuck_ShouldThrow()
        {
            // Arrange
            var expected = GetRandomInt();
            var data = new Dictionary<string, object>
            {
                ["Config.PortMoo"] = expected
            };

            // Pre-Assert

            // Act
            Expect(() => data.FuzzyDuckAs<IConfig2>(
                        s => "Config." + s,
                        s => s.RegexReplace("Config.", ""),
                        true
                    )
                )
                .To.Throw<UnDuckableException>();
            // Assert
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctions_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new NameValueCollection
            {
                ["Config.BAseUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result =
                data.FuzzyDuckAs<IConfig>(
                    s => "Config." + s,
                    s => s.RegexReplace("Config.", "")
                );

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndNoThrow_AndCantDuck_ShouldReturnNull()
        {
            // Arrange
            var expected = GetRandomInt();
            var data = new NameValueCollection
            {
                ["Config.Port"] = expected.ToString()
            };

            // Pre-Assert

            // Act
            var result =
                data.FuzzyDuckAs<IConfig2>(
                    toNativeTransform: s => s.RegexReplace("Config.", "moo"),
                    fromNativeTransform: s => "Config." + s
                );

            // Assert

            Expect(result)
                .To.Be.Null();
        }

        [Test]
        public void
            FuzzyDuckAs_OperatingOnNameValueCollection_WhenGivenKeyTransformFunctionsAndMustThrow_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new NameValueCollection
            {
                ["Config.BaSeUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result =
                data.FuzzyDuckAs<IConfig>(
                    s => "Config." + s,
                    s => s.RegexReplace("Config.", ""),
                    true
                );

            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        public interface IIgnoringPeriods
        {
            string ConfigBaseUrl { get; }
            bool IsProduction { get; }
        }

        [Test]
        public void IgnoringPeriods()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var data = new NameValueCollection()
            {
                ["Config.BaseUrl"] = expected,
                ["IsProduction"] = "True"
            };
            // Pre-assert
            // Act
            var result = data.FuzzyDuckAs<IIgnoringPeriods>();
            // Assert
            Expect(result.ConfigBaseUrl)
                .To.Equal(expected);
            Expect(result.IsProduction)
                .To.Be.True();
        }
    }

    [TestFixture]
    public class GivenKeyPrefix_OperatingOnDictionary
    {
        [TestFixture]
        public class OperatingOnDictionary
        {
            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig>(prefix);

                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.BaseUrl)
                    .To.Equal(expected);
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomInt(1);
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}Port123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.FuzzyDuckAs<IConfig2>(prefix);

                // Assert
                Expect(result)
                    .To.Be.Null();
            }

            [Test]
            public void
                FuzzyDuckAs_OperatingOnDictionary_WhenGivenKeyPrefixAndThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomInt();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}Port1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.FuzzyDuckAs<IConfig2>(prefix, true))
                    .To.Throw<UnDuckableException>();

                // Assert
            }

            [Test]
            public void DuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.BaseUrl)
                    .To.Equal(expected);
            }

            [Test]
            public void
                DuckAs_OperatingOnDictionary_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl123"] = expected
                };

                // Pre-Assert

                // Act
                var result = data.DuckAs<IConfig>(prefix);

                // Assert
                Expect(result)
                    .To.Be.Null();
            }

            [Test]
            public void
                DuckAs_OperatingOnDictionary_WhenGivenKeyPrefixAndThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
            {
                // Arrange
                var expected = GetRandomHttpUrl();
                var prefix = GetRandomString(4) + ".";
                var data = new Dictionary<string, object>
                {
                    [$"{prefix}BaseUrl1"] = expected
                };

                // Pre-Assert

                // Act
                Expect(() => data.DuckAs<IConfig>(prefix, true))
                    .To.Throw<UnDuckableException>();

                // Assert
            }
        }
    }

    public class GivenKeyPrefix_OperatingOnNameValueCollection
    {
        [Test]
        public void FuzzyDuckAs_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}BaseUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result = data.FuzzyDuckAs<IConfig>(prefix);

            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        [Test]
        public void FuzzyDuckAs_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
        {
            // Arrange
            var expected = GetRandomInt();
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}Port123"] = expected.ToString()
            };

            // Pre-Assert

            // Act
            var result = data.FuzzyDuckAs<IConfig2>(prefix);

            // Assert
            Expect(result)
                .To.Be.Null();
        }

        [Test]
        public void
            FuzzyDuckAs_WhenGivenKeyPrefixAndThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
        {
            // Arrange
            var expected = GetRandomInt(1);
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}Port1"] = expected.ToString()
            };

            // Pre-Assert

            // Act
            Expect(() => data.FuzzyDuckAs<IConfig2>(prefix, true))
                .To.Throw<UnDuckableException>();

            // Assert
        }

        [Test]
        public void DuckAs_WhenGivenKeyPrefix_AndCanDuck_ShouldDuck()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}BaseUrl"] = expected
            };

            // Pre-Assert

            // Act
            var result = data.DuckAs<IConfig>(prefix);

            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.BaseUrl)
                .To.Equal(expected);
        }

        [Test]
        public void DuckAs_WhenGivenKeyPrefix_AndCannotDuck_ShouldReturnNull()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}BaseUrl123"] = expected
            };

            // Pre-Assert

            // Act
            var result = data.DuckAs<IConfig>(prefix);

            // Assert
            Expect(result)
                .To.Be.Null();
        }

        [Test]
        public void
            DuckAs_WhenGivenKeyPrefixAndThrowOnErrorIsTrue_AndCannotDuck_ShouldThrow()
        {
            // Arrange
            var expected = GetRandomHttpUrl();
            var prefix = GetRandomString(4) + ".";
            var data = new NameValueCollection
            {
                [$"{prefix}BaseUrl1"] = expected
            };

            // Pre-Assert

            // Act
            Expect(() => data.DuckAs<IConfig>(prefix, true))
                .To.Throw<UnDuckableException>();

            // Assert
        }
    }

    [TestFixture]
    public class ForcefulDefaultDucking
    {
        public interface IHasId
        {
            int Id { get; }
        }

        [TestFixture]
        public class OperatingOnObject
        {
            [Test]
            public void ShouldAllowDefaultValueForMissingReadonlyProperty()
            {
                // Arrange
                var input = new
                {
                };
                // Act
                var result = input.ForceFuzzyDuckAs<IHasId>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Id)
                    .To.Equal(default(int));
            }
        }

        [TestFixture]
        public class OperatingOnDictionary
        {
            [Test]
            public void ShouldAllowDefaultValueForMissingReadonlyProperty()
            {
                // Arrange
                var input = new Dictionary<string, object>();
                // Act
                var result = input.ForceFuzzyDuckAs<IHasId>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Id)
                    .To.Equal(default(int));
            }

            [Test]
            public void ShouldMapFuzzyNames()
            {
                // Arrange
                var dict = new Dictionary<string, string>()
                {
                    ["Lock.Count"] = "1000"
                };
                var unforced = dict.FuzzyDuckAs<IHasDottedProperty>();
                Expect(unforced.LockCount)
                    .To.Equal(1000);
                // Act
                var forced = dict.ForceFuzzyDuckAs<IHasDottedProperty>();
                // Assert
                Expect(forced.LockCount)
                    .To.Equal(1000);
            }

            public interface IHasDottedProperty
            {
                int LockCount { get; set; }
            }

            public interface IHasWritableId
            {
                int Id { get; set; }
            }
        }
    }

    [TestFixture]
    public class WildFailures
    {
        public interface IBackedByDictionary
        {
            string Name { get; set; }
        }

        public interface IDuckFromDictionaryProperty
        {
            IBackedByDictionary Prop { get; set; }
        }

        public class DuckMe : IDuckFromDictionaryProperty
        {
            public IBackedByDictionary Prop { get; set; }
        }

        public class DuckFromMe
        {
            public Dictionary<string, string> Prop { get; set; }

            public DuckFromMe()
            {
                Prop = new Dictionary<string, string>();
            }
        }

        [Test]
        public void ShouldBeAbleToAutoDuckDictionaryProperties()
        {
            // Arrange
            var expected = GetRandomString();
            var src = new DuckFromMe
            {
                Prop =
                {
                    ["Name"] = expected
                }
            };
            // Pre-Assert
            // Act
            var result = src.FuzzyDuckAs<IDuckFromDictionaryProperty>();
            // Assert

            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Prop.Name)
                .To.Equal(expected);
        }

        public interface IHasReadonlyName
        {
            string Name { get; }
        }

        [Test]
        public void FuzzyDuckAs_ShouldDuckMissingStringAsNull()
        {
            // Arrange
            var src = new MergeDictionary<string, string>(new Dictionary<string, string>());
            // Act
            try
            {
                var result = src.FuzzyDuckAs<IHasReadonlyName>(true);
                // Assert
                Expect(result.Name)
                    .To.Be.Null();
            }
            catch (UnDuckableException ex)
            {
                Assert.Fail(ex.Errors.JoinWith("\n"));
            }
        }

        [Test]
        public void FuzzyDuckAs_ShouldNotSpazOnConfigWithMissingKeys()
        {
            // Arrange
            using (var tempFile = new AutoTempFile(
                       Path.GetTempPath(),
                       "appsettings.json"
                   ))
            {
                var config = CreateConfig(tempFile.Path);
                // Act
                var settings = GetSettingsFrom(config);
                // Assert
                Expect(settings)
                    .Not.To.Be.Null();
                Expect(settings.NlogConfigLocation)
                    .To.Be.Null();
            }
        }

        private void WriteOutAppSettings(string target)
        {
            var data = @"{
  ""Logging"": {
                ""LogLevel"": {
                    ""Default"": ""Debug"",
                    ""System"": ""Information"",
                    ""Microsoft"": ""Information""
                },
                ""ConnectionString"": ""SERVER=localhost; DATABASE=spar_rcms; UID=sqltracking; PASSWORD=sqltracking; Allow User Variables=true;"",
                ""Console"": {
                    ""IncludeScopes"": true
                }
            },
            ""AllowedHosts"": ""*"",
            ""ConnectionStrings"": {
                ""DefaultConnection"": ""SERVER=localhost; DATABASE=spar_rcms; UID=sqltracking; PASSWORD=sqltracking; Allow User Variables=true;""
            },
            ""Settings"": {
            ""logLevel"": ""debug"",
            ""RetailStudioUrl"": ""https://test-retailstudio.spar.co.za/rcp"",
            ""RetailStudioClientId"": ""df77e380-97a7-4029-ac75-0725d5ef0245"",
            ""RetailStudioSharedKey"": ""39158E459CCF47BC8E1188C60504FD2B"",
            ""EncryptionKey"": ""AAECAwQFBgcICQoLDA0ODw=="",
            ""Secret"": ""AAECAwQFBgcICQoLDA0ODw==""
        }
    }";
            File.WriteAllText(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    target
                ),
                data
            );
        }

        private IConfigurationRoot CreateConfig(string appSettingsFile)
        {
            WriteOutAppSettings(appSettingsFile);
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appSettingsFile)
                .Build();
        }

        private static IAppSettings GetSettingsFrom(
            IConfigurationRoot config
        )
        {
            var defaultConfig = new Dictionary<string, string>();
            var providedConfig = config.GetSection("Settings")
                ?.GetChildren()
                .ToDictionary(s => s.Key, s => s.Value) ?? new Dictionary<string, string>();
            var merged = new MergeDictionary<string, string>(providedConfig, defaultConfig);
            try
            {
                return merged.FuzzyDuckAs<IAppSettings>(true);
            }
            catch (UnDuckableException ex)
            {
                Console.WriteLine(ex.Errors.JoinWith("\n"));
                throw;
            }
        }

        public interface IAppSettings
        {
            LogLevels LogLevel { get; }
            string RetailStudioUrl { get; }
            string RetailStudioClientId { get; }
            string RetailStudioSharedKey { get; }
            string EncryptionKey { get; }
            string Secret { get; }
            string NlogConfigLocation { get; }
            string AlgoliaApplicationId { get; }
            string AlgoliaKey { get; }
        }

        public enum LogLevels
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }
    }

    [TestFixture]
    public class EnumDucking
    {
        public enum Priorities
        {
            Low,
            Medium,
            High
        }

        public interface IHaveAnEnumProperty
        {
            Priorities Priority { get; }
        }

        [TestFixture]
        public class OperatingOnAnObject
        {
            [Test]
            public void CanDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
            {
                // Arrange
                var input = new
                {
                    Priority = "Medium"
                };
                // Pre-Assert
                // Act
                var result = input.CanDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void DuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
            {
                // Arrange
                var input = new
                {
                    Priority = "Medium"
                };
                // Pre-Assert
                // Act
                var result = input.DuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Priority)
                    .To.Equal(Priorities.Medium);
            }

            [Test]
            public void CanFuzzyDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
            {
                // Arrange
                var input = new
                {
                    Priority = "MeDium"
                };
                // Pre-Assert
                // Act
                var result = input.CanFuzzyDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void FuzzyDuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
            {
                // Arrange
                var input = new
                {
                    PrIority = "medium"
                };
                // Pre-Assert
                // Act
                var result = input.FuzzyDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Priority)
                    .To.Equal(Priorities.Medium);
            }
        }

        [TestFixture]
        public class OperatingOnADictionary
        {
            [Test]
            public void CanDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
            {
                // Arrange
                var input = new Dictionary<string, object>()
                {
                    ["Priority"] = "Medium"
                };
                // Pre-Assert
                // Act
                var result = input.CanDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void DuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
            {
                // Arrange
                var input = new Dictionary<string, object>
                {
                    ["Priority"] = "Medium"
                };
                // Pre-Assert
                // Act
                var result = input.DuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Priority)
                    .To.Equal(Priorities.Medium);
            }

            [Test]
            public void CanFuzzyDuckAs_WhenCanDuckEnumByValue_ShouldReturnTrue()
            {
                // Arrange
                var input = new
                {
                    Priority = "MeDium"
                };
                // Pre-Assert
                // Act
                var result = input.CanFuzzyDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void FuzzyDuckAs_WhenCanDuckFromStringToEnum_ShouldReturnDucked()
            {
                // Arrange
                var input = new
                {
                    Priority = "medium"
                };
                // Pre-Assert
                // Act
                var result = input.FuzzyDuckAs<IHaveAnEnumProperty>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Priority)
                    .To.Equal(Priorities.Medium);
            }
        }
    }

    public interface IConnectionStrings
    {
        string SomeString { get; }
    }

    [TestFixture]
    public class OperatingOnConnectionStringSettingsCollection
    {
        [Test]
        public void WhenCanFuzzyDuck_ShouldFuzzyDuck()
        {
            // Arrange
            var expected = GetRandomString(2);
            var setting = new ConnectionStringSettings("some string", expected);
            var settings = new ConnectionStringSettingsCollection()
            {
                setting
            };
            // Pre-Assert
            // Act
            var result = settings.FuzzyDuckAs<IConnectionStrings>();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.SomeString)
                .To.Equal(expected);
        }

        [Test]
        public void
            WhenCantDuckBecauseStringPropertyIsMissing_GivenShouldThrow_ShouldNotThrow()
        {
            // Arrange
            var setting = new ConnectionStringSettings("123some string", "some value");
            var settings = new ConnectionStringSettingsCollection()
            {
                setting
            };
            // Pre-Assert
            // Act
            Expect(() => settings.DuckAs<IConnectionStrings>(true))
                .To.Throw<UnDuckableException>();
            // Assert
        }

        [Test]
        public void WhenCanDuck_ShouldFuzzyDuck()
        {
            // Arrange
            var expected = GetRandomString(2);
            var setting = new ConnectionStringSettings("SomeString", expected);
            var settings = new ConnectionStringSettingsCollection()
            {
                setting
            };
            // Pre-Assert
            // Act
            var result = settings.DuckAs<IConnectionStrings>();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.SomeString)
                .To.Equal(expected);
        }

        [Test]
        public void WhenCannotDuck_GivenShouldThrow_ShouldThrow()
        {
            // Arrange
            var setting = new ConnectionStringSettings("MooSomeString", "some value");
            var settings = new ConnectionStringSettingsCollection()
            {
                setting
            };
            // Pre-Assert
            // Act
            Expect(() => settings.DuckAs<IConnectionStrings>(true))
                .To.Throw<UnDuckableException>();
            // Assert
        }
    }

    [TestFixture]
    public class DuckingDictionaryWithFuncPropToInterfaceWithMethod
    {
        [Test]
        public void ShouldCallThroughForDirectMatch()
        {
            // Arrange
            var dict = new Dictionary<string, object>()
            {
                ["Add"] = new Func<int, int, int>((
                        a,
                        b
                    ) => a + b
                )
            };

            // Act
            var result = dict.DuckAs<ICalculator>();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Add(1, 2))
                .To.Equal(3);
        }

        [Test]
        public void ShouldCallThroughForUpcastingMatch()
        {
            // Arrange
            var dict = new Dictionary<string, object>()
            {
                ["Add"] = new Func<long, long, long>((
                        a,
                        b
                    ) => a + b
                )
            };

            Expect(typeof(int).IsAssignableOrUpCastableTo(typeof(long)))
                .To.Be.True();
            Expect(typeof(long).IsAssignableOrUpCastableTo(typeof(int)))
                .To.Be.False();

            // Act
            var ducked = dict.DuckAs<ICalculator>(true);
            Expect(ducked)
                .Not.To.Be.Null();
            var result = ducked.Add(1, 2);
            // Assert
            Expect(result)
                .To.Equal(3);
        }

        [Test]
        public void ShouldCallThroughToVoidMethod()
        {
            // Arrange
            var store = new List<int>();
            var dict = new Dictionary<string, object>()
            {
                ["Store"] = new Action<int>(i => store.Add(i))
            };

            // Act
            var ducked = dict.DuckAs<IStore>(true);
            Expect(ducked)
                .Not.To.Be.Null();
            ducked.Store(1);
            // Assert
            Expect(store)
                .To.Equal(
                    new[]
                    {
                        1
                    }
                );
        }

        public interface ICalculator
        {
            int Add(
                int a,
                int b
            );
        }

        public interface IStore
        {
            void Store(int value);
        }
    }

    [TestFixture]
    public class DuckingDictionaryWithNullPropValue
    {
        [TestFixture]
        public class WhenPropTypeIsNullable
        {
            [Test]
            public void ShouldReturnNull()
            {
                // Arrange
                var dict = new Dictionary<string, object>()
                {
                    ["StringProp"] = null as string
                };
                // Act
                var result1 = dict.FuzzyDuckAs<IHasStringProp>();
                var result2 = dict.DuckAs<IHasStringProp>();
                // Assert
                Expect(result1.StringProp)
                    .To.Be.Null();
                Expect(result2.StringProp)
                    .To.Be.Null();
            }
        }

        [TestFixture]
        public class WhenPropTypeNotNullAble
        {
            [TestFixture]
            public class WhenForcedFuzzy
            {
                [Test]
                public void CaseInsensitiveDictGen()
                {
                    // Arrange
                    var dict = new Dictionary<string, object>()
                    {
                        ["IntProp"] = null
                    };
                    // Act
                    var result = dict.ToCaseInsensitiveDictionary();
                    // Assert
                    Expect(result["intprop"])
                        .To.Be.Null();
                }

                [Test]
                public void ShouldReturnDefaultValueForType()
                {
                    // Arrange
                    var dict = new Dictionary<string, object>()
                    {
                        ["IntProp"] = null
                    };
                    // Act
                    var result = dict.ForceFuzzyDuckAs<IHasIntProp>();
                    // Assert
                    Expect(result.IntProp)
                        .To.Equal(default(int));
                }
            }

            [TestFixture]
            public class WhenFuzzy
            {
                [Test]
                public void ShouldThrow()
                {
                    // Arrange
                    var dict = new Dictionary<string, object>()
                    {
                        ["IntProp"] = null
                    };
                    // Act
                    Expect(() => dict.FuzzyDuckAs<IHasIntProp>(true))
                        .To.Throw<UnDuckableException>();
                    // Assert
                }
            }

            [TestFixture]
            public class WhenStrict
            {
                [Test]
                public void ShouldThrow()
                {
                    // Arrange
                    var dict = new Dictionary<string, object>()
                    {
                        ["IntProp"] = null
                    };
                    // Act
                    Expect(() => dict.DuckAs<IHasIntProp>(true))
                        .To.Throw<UnDuckableException>();
                    // Assert
                }
            }
        }

        public interface IHasStringProp
        {
            string StringProp { get; }
        }

        public interface IHasIntProp
        {
            int IntProp { get; }
        }
    }

    [TestFixture]
    public class FuzzyDuckingStringsToNullableEnums
    {
        [Test]
        public void ShouldDoIt()
        {
            // Arrange
            var data = new Dictionary<string, string>()
            {
                ["Number"] = "Two",
                ["Other"] = "Three"
            };
            // Act
            var result = data.FuzzyDuckAs<IHasANumber>(throwOnError: true);
            // Assert
            Expect(result.Number)
                .To.Equal(Numbers.Two);
            Expect(result.Other)
                .To.Equal(Numbers.Three);
        }

        public enum Numbers
        {
            One,
            Two,
            Three
        }

        public interface IHasANumber
        {
            Numbers? Number { get; set; }
            Numbers Other { get; set; }
        }
    }

    [TestFixture]
    public class MethodDucking
    {
        [Test]
        public void Simplest()
        {
            // Arrange
            var actual = Substitute.For<IAddInts>()
                .With(o => o.Add(Arg.Any<int>(), Arg.Any<int>())
                    .Returns(ci => (int)ci.Args()[0] + (int)ci.Args()[1])
                );
            Expect(actual.Add(1, 2))
                .To.Equal(3);

            var ducked = actual.DuckAs<IAddInts>();
            Expect(ducked)
                .Not.To.Be(actual);
            var a = GetRandomInt();
            var b = GetRandomInt();
            var expected = a + b;
            // Act
            var result = ducked.Add(a, b);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldSelectCorrectOverloadForExactMatch()
        {
            // Arrange
            var actual = new Adder();
            Expect(actual.Add(1, 2))
                .To.Equal(3);
            Expect(actual.Add("1", "2"))
                .To.Equal("12");
            try
            {
                var ducked = actual.DuckAs<IAddStrings>(throwOnError: true);
                var a = GetRandomString();
                var b = GetRandomString();
                var expected = a + b;
                // Act
                var result = ducked.Add(a, b);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
            catch (UnDuckableException e)
            {
                throw new Exception($"Duck fails:\n{string.Join("\n", e.Errors)}");
            }
        }

        [Test]
        public void ShouldSelectCorrectOverloadForUnorderedParameters()
        {
            // Arrange
            var actual = new Adder();
            var stringValue = GetRandomString();
            var intValue = GetRandomInt();
            var expected = $"{intValue}{stringValue}";
            // Act
            try
            {
                var ducked = actual.FuzzyDuckAs<IAddStringsAndInts>(throwOnError: true);
                var result = ducked.Add(stringValue, intValue);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
            catch (UnDuckableException e)
            {
                throw new Exception($"Duck fails:\n{string.Join("\n", e.Errors)}");
            }
        }

        [TestCase("true", true)]
        [TestCase("True", true)]
        [TestCase("yes", true)]
        [TestCase("1", true)]
        [TestCase("false", false)]
        [TestCase("False", false)]
        [TestCase("0", false)]
        public void ShouldParseNullableBooleans_(
            string stringValue,
            bool expected
        )
        {
            // Arrange
            var data = new
            {
                flag = stringValue,
                number1 = "123"
            };
            // Act
            var result = data.ForceFuzzyDuckAs<IHasFlag>();
            // Assert
            Expect(result.Flag)
                .To.Equal(expected);
        }

        [TestCase("true", true)]
        [TestCase("True", true)]
        [TestCase("yes", true)]
        [TestCase("1", true)]
        [TestCase("false", false)]
        [TestCase("False", false)]
        [TestCase("0", false)]
        public void ShouldParseBooleans_(
            string stringValue,
            bool expected
        )
        {
            // Arrange
            var data = new
            {
                flag = stringValue,
                number1 = "123"
            };
            // Act
            var result = data.ForceFuzzyDuckAs<IHasNonNullableFlag>();
            // Assert
            Expect(result.Flag)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToDuckToInterfaceWithObjectProperties()
        {
            // Arrange
            var person = GetRandom<ITypedPerson>()
                .With(o => o.Id = GetRandomInt(1, 100));
            // Act
            var result = person.DuckAs<IUntypedPerson>();
            // Assert
            Expect(result)
                .To.Deep.Equal(person);
            var newId = GetRandomInt(1000, 2000);
            result.Id = newId;
            Expect(person.Id)
                .To.Equal(newId);
        }


        public interface ITypedPerson
        {
            int Id { get; set; }
            string Name { get; set; }
            DateTime DateOfBirth { get; set; }
            bool Flag { get; set; }
        }

        public interface IUntypedPerson
        {
            int Id { get; set; }
            string Name { get; set; }
            DateTime DateOfBirth { get; set; }
            bool Flag { get; set; }
        }

        public interface IHasNonNullableFlag
        {
            bool Flag { get; set; }
        }

        public interface IAddInts
        {
            int Add(
                int a,
                int b
            );
        }

        public interface IAddStrings
        {
            string Add(
                string a,
                string b
            );
        }

        public interface IAddStringsAndInts
        {
            string Add(
                string a,
                int b
            );
        }

        public interface IAddIntsAndStrings
        {
            string Add(
                int a,
                string b
            );
        }

        public interface IAdder : IAddInts, IAddStrings, IAddIntsAndStrings
        {
        }

        public class Adder : IAdder
        {
            public int Add(
                int a,
                int b
            )
            {
                return a + b;
            }

            public string Add(
                string a,
                string b
            )
            {
                return a + b;
            }

            public string Add(
                int a,
                string b
            )
            {
                return $"{a}{b}";
            }
        }
    }


    public class TravelRequestDetails : ITravelRequestDetails
    {
        public DateTime Initiated { get; set; }
        public string DepartingFrom { get; set; }
        public string TravellingTo { get; set; }
        public DateTime Departing { get; set; }
        public string PreferredDepartureTime { get; set; }
        public DateTime Returning { get; set; }
        public string PreferredReturnTime { get; set; }
        public string ReasonForTravel { get; set; }
        public bool CarRequired { get; set; }
        public bool AccomodationRequired { get; set; }
        public string AccommodationNotes { get; set; }
    }

    public interface ITravelRequestDetails
    {
        DateTime Initiated { get; set; }
        string DepartingFrom { get; set; }
        string TravellingTo { get; set; }
        DateTime Departing { get; set; }
        string PreferredDepartureTime { get; set; }
        DateTime Returning { get; set; }
        string PreferredReturnTime { get; set; }
        string ReasonForTravel { get; set; }
        bool CarRequired { get; set; }
        bool AccomodationRequired { get; set; }
        string AccommodationNotes { get; set; }
    }

    public interface ITravelRequestCaptureDetailsActivityParameters
        : IActivityParameters<ITravelRequestDetails>
    {
    }

    public interface IActivityParameters : IHasAnActorId
    {
        Guid TaskId { get; }
    }

    public interface IActivityParameters<out T> : IActivityParameters
    {
        T Payload { get; }
    }

    public interface ISpecificActivityParameters : IActivityParameters<string>
    {
    }

    public class ActivityParameters : IActivityParameters
    {
        public Guid ActorId { get; }
        public Guid TaskId { get; }

        public void DoNothing()
        {
            /* does nothing */
        }

        public ActivityParameters(
            Guid actorId,
            Guid taskId
        )
        {
            ActorId = actorId;
            TaskId = taskId;
        }
    }

    public class ActivityParameters<T> : ActivityParameters, IActivityParameters<T>
    {
        public T Payload { get; set; }

        public ActivityParameters(
            Guid actorId,
            Guid taskId,
            T payload
        )
            : base(actorId, taskId)
        {
            Payload = payload;
        }
    }
}