using System.Collections.Generic;
using NUnit.Framework;
using NExpect;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.EasyArgs.Tests
{
    [TestFixture]
    public class TestArgsParser
    {
        [Test]
        public void ShouldParseArgumentBasedOnShortName()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { "-p", expected.ToString() };
            // Act
            var result = args.ParseTo<IArgs>();
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldParseArgumentBasedOnLongName()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { "--listen-port", expected.ToString() };

            // Act
            var result = args.ParseTo<IArgs>();
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
        }

        [TestCase("--other-property")]
        [TestCase("--otherproperty")]
        [TestCase("--otherProperty")]
        [TestCase("--OtherProperty")]
        public void ShouldFallBackToMatchingPropertyNameForLongName(string arg)
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { arg, expected.ToString() };
            // Act
            var result = args.ParseTo<IArgs>();
            // Assert
            Expect(result.OtherProperty)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeCaseSensitiveForShortName()
        {
            // Arrange
            var e1 = GetRandomInt(1, 32768);
            var e2 = GetAnother(e1);
            var args = new[] { "-p", e1.ToString(), "-P", e2.ToString() };

            // Act
            var result = args.ParseTo<IArgs>();
            // Assert
            Expect(result.Port)
                .To.Equal(e1);
            Expect(result.UpperCaseP)
                .To.Equal(e2);
        }

        [Test]
        public void ShouldNotCollectMultipleValuesForNonEnumerableProperty()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var unexpected = GetAnother(expected);
            var args = new[] { "-p", expected.ToString(), unexpected.ToString() };
            // Act
            var result = args.ParseTo<IArgs>(out var uncollected);
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
            Expect(uncollected)
                .To.Equal(new[] { unexpected.ToString() });
        }

        [Test]
        public void ShouldCollectMultipleValuesForEnumerableProperty()
        {
            // Arrange
            var args = new[] { "-v", "1", "2", "3" };
            // Act
            var result = args.ParseTo<ISum>();
            // Assert
            Expect(result.Values)
                .To.Equal(new[] { 1, 2, 3 });
        }

        [Test]
        public void ShouldSetFlagFalseWhenNotProvided()
        {
            // Arrange
            var args = new string[0];
            // Act
            var result = args.ParseTo<IHasFlags>();
            // Assert
            Expect(result.Frob)
                .To.Be.False();
        }

        [Test]
        public void ShouldSetFlagToTrueWhenProvided()
        {
            // Arrange
            var args = new[] { "--frob" };
            // Act
            var result = args.ParseTo<IHasFlags>();
            // Assert
            Expect(result.Frob)
                .To.Be.True();
        }

        [Test]
        public void ShouldSetFlagToTrueWhenDefaultedTrueAndMissing()
        {
            // Arrange
            var args = new string[0];
            // Act
            var result = args.ParseTo<IHasDefaultTrueFrob>();
            // Assert
            Expect(result.Frob)
                .To.Be.True();
        }

        [Test]
        public void ShouldUnderstandImplicitFlagNegations()
        {
            // Arrange
            var args = new[] { "--no-frob" };
            // Act
            var result = args.ParseTo<IHasDefaultTrueFrob>();
            // Assert
            Expect(result.Frob)
                .To.Be.False();
        }

        [Test]
        public void ShouldErrorAndExitWhenConflictingArgumentsGiven()
        {
            // Arrange
            var args = new[] { "--flag1", "--flag2" };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };

            // Act
            args.ParseTo<IHasConflictingFlags>(
                out var uncollected,
                opts
            );
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(line => line.Contains("--flag1 conflicts with --flag2"));
            Expect(lines)
                .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
        }

        [Test]
        public void ShouldErrorAndExitIfFlagAndNoFlagSpecified()
        {
            // Arrange
            var args = new[] { "--flag1", "--no-flag1" };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };
            var expected = $"{args[1]} conflicts with {args[0]}";

            // Act
            args.ParseTo<IHasConflictingFlags>(
                out var uncollected,
                opts
            );
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(
                    line => line.Contains(expected),
                    () => $"expected: {expected}\nreceieved: {lines.JoinWith("\n")}");
        }

        [Test]
        public void ShouldErrorAndExitIfFlagAndNoFlagSpecifiedReversed()
        {
            // Arrange
            var args = new[] { "--no-flag1", "--flag1" };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };
            var expected = $"{args[1]} conflicts with {args[0]}";

            // Act
            args.ParseTo<IHasConflictingFlags>(
                out var uncollected,
                opts
            );
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(
                    line => line.Contains(expected),
                    () => $"expected: {expected}\nreceieved: {lines.JoinWith("\n")}");
        }

        [Test]
        public void ShouldErrorAndExitIfSingleValueArgAlreadySpecified()
        {
            // Arrange
            var args = new[] { "--listen-port", 1.ToString(), "-p", 2.ToString() };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };
            // Act
            args.ParseTo<IArgs>(out var uncollected, opts);

            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(
                    line => line.Contains("--listen-port specified more than once but only accepts one value"));
        }

        [Test]
        public void ShouldErrorAndExitByDefaultWhenEncounteringUnknownSwitch()
        {
            // Arrange
            var args = new[] { "--flag1", "--port" };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };

            // Act
            args.ParseTo<IHasConflictingFlags>(out var collected, opts);
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(
                    line => line.Contains("unknown option: --port")
                );
        }

        [Test]
        public void ShouldErrorAndExitIfRequiredArgIsMissing()
        {
            // Arrange
            var args = new string[0];
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };
            // Act
            args.ParseTo<IHasRequiredArg>(out var uncollected, opts);
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1).Item();
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(l => l.Contains("--port is required"));
        }

        public interface IHasRequiredArg
        {
            [Required]
            int Port { get; }
        }

        public interface IHasConflictingFlags
        {
            bool Flag1 { get; set; }

            [ConflictsWith(nameof(Flag1))]
            bool Flag2 { get; set; }
        }

        public interface IHasFlags
        {
            bool Frob { get; }
        }

        public interface IHasDefaultTrueFrob
        {
            [Default(true)]
            bool Frob { get; }
        }

        public interface ISum
        {
            int[] Values { get; set; }
        }

        [Test]
        public void ParsingToPOCO()
        {
            // Arrange
            var args = new[] { "--port", "123" };
            // Act
            var result = args.ParseTo<PocoArgs>();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Port)
                .To.Equal(123);
        }

        [Test]
        public void ShouldBeAbleToConsumeWithNoSwitchesAndOnlyOutArgs()
        {
            // Arrange
            var args = new[] { "something-something" };
            // Act
            var result = args.ParseTo<IFoo>(out var remaining);
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(remaining)
                .To.Equal(args);
            
        }

        public interface IFoo
        {
            public bool Bar { get; set; }
        }

        public class PocoArgs
        {
            public int Port { get; set; }
        }

        [TestFixture]
        public class GenerateHelpText
        {
            [Test]
            public void ShouldGenerateHelp()
            {
                // Arrange
                var expected = @"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files

-f, --flag                the flag
    --help                shows this help
-h, --host [text]         host to connect to (localhost)
-l, --long-option [text]
-P, --password [text]
-p, --port [number]
-u, --user [text]         user name to use

Negate any flag argument with --no-{option}

This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
";
                var args = new[] { "--help" };
                int? exitCode = null;
                var lines = new List<string>();
                var opts = new ParserOptions()
                {
                    ExitAction = c => exitCode = c,
                    LineWriter = str => lines.Add(str)
                };
                // Act
                args.ParseTo<IHelpArgs>(out var uncollected, opts);
                // Assert
                Expect(exitCode)
                    .To.Equal(2);
                var output = lines.JoinWith("\r\n");
                Expect(output)
                    .To.Equal(expected, () =>
                    {
                        var i = 0;
                        foreach (var c in output) 
                        {
                            if (i > expected.Length)
                            {
                                return $"mismatch starts at {output.Substring(i)}";
                            }

                            if (c != expected[i])
                            {
                                return $@"difference at {i}: expected
'{expected.Substring(i)}'
received:
'{output.Substring(i)}'";
                            }

                            i++;
                        };
                        return "dunno";
                    });
            }

            [Attributes.Description(@"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files
")]
            [MoreInfo(@"
This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
")]
            public interface IHelpArgs
            {
                [Attributes.Description("user name to use")]
                public string User { get; set; }

                [Attributes.Description("host to connect to")]
                [Default("localhost")]
                public string Host { get; set; }

                // should get -p
                public int Port { get; set; }

                // should get -P
                public string Password { get; set; }

                // should get kebab-cased long name
                public string LongOption { get; set; }

                [Attributes.Description("the flag")]
                public bool Flag { get; set; }
            }
        }

        public interface IArgs
        {
            [ShortName('p')]
            [LongName("listen-port")]
            public int Port { get; set; }

            public int OtherProperty { get; set; }

            [ShortName('P')]
            public int UpperCaseP { get; set; }
        }
    }
}