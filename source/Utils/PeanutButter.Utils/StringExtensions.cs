using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Provides utility extensions for strings
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
    public
#endif
    static class StringExtensions
{
    /// <summary>
    /// Replaces patterns matched by the given regex pattern with the given replaceWith string
    /// </summary>
    /// <param name="input">Starting string</param>
    /// <param name="pattern">Regex pattern to search for</param>
    /// <param name="replaceWith">String to replace occurrences with</param>
    /// <returns>New string with matches replaces</returns>
    public static string RegexReplace(
        this string input,
        string pattern,
        string replaceWith
    )
    {
        return input.RegexReplaceAll(replaceWith, pattern);
    }

    /// <summary>
    /// Replace all matching regular expression strings in input with the given string
    /// </summary>
    /// <param name="input"></param>
    /// <param name="replaceWith"></param>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public static string RegexReplaceAll(
        this string input,
        string replaceWith,
        params string[] patterns
    )
    {
        return input.RegexReplaceAll(
            replaceWith,
            patterns.Select(p => new Regex(p)).ToArray()
        );
    }

    /// <summary>
    /// Replace all matching Regex patterns in input with the given string
    /// </summary>
    /// <param name="input"></param>
    /// <param name="replaceWith"></param>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public static string RegexReplaceAll(
        this string input,
        string replaceWith,
        params Regex[] patterns
    )
    {
        return patterns.Aggregate(
            input,
            (acc, cur) => cur.Replace(acc, replaceWith)
        );
    }

    /// <summary>
    /// Convenience extension to return another string if the input is null or empty
    /// </summary>
    /// <param name="input">String to test</param>
    /// <param name="alternative">String to return if the input was null or empty</param>
    /// <returns>The original string when it is not null or empty; the alternative when the original is null or empty</returns>
    public static string Or(
        this string input,
        string alternative
    )
    {
        return string.IsNullOrEmpty(input)
            ? alternative
            : input;
    }

    /// <summary>
    /// Attempts conversion from a string value to a boolean value matching the following (case-insensitive) to True:
    /// - "yes"
    /// - "y"
    /// - "1"
    /// - "true"
    /// All other string values are considered to be false
    /// </summary>
    /// <param name="input">String to attempt to convert</param>
    /// <returns>True for truthy values, False otherwise</returns>
    public static bool AsBoolean(
        this string input
    )
    {
        return !string.IsNullOrWhiteSpace(input) &&
            Truthy.Any(item => item == input.ToLower());
    }

    private static readonly string[] Truthy =
    {
        "yes",
        "y",
        "1",
        "true",
        "on"
    };

    /// <summary>
    /// Searches a master string for occurrences of any of the given strings,
    /// case-insensitive
    /// </summary>
    /// <param name="haystack">String to search</param>
    /// <param name="needles">Strings to search for</param>
    /// <returns>True if any of the needles are found in the haystack; False otherwise</returns>
    public static bool ContainsOneOf(
        this string haystack,
        params string[] needles
    )
    {
        return MultiMatch(
            haystack,
            needles,
            h => needles.Any(
                n => h.Contains(
                    n.ToLower(



#if NETSTANDARD
#else
                            CultureInfo.CurrentCulture
#endif
                    )
                )
            )
        );
    }

    /// <summary>
    /// Much like ContainsOneOf, but performs a case-insensitive exact match
    /// for the needles against the haystack
    /// </summary>
    /// <param name="haystack"></param>
    /// <param name="needles"></param>
    /// <returns></returns>
    public static bool EqualsOneOf(
        this string haystack,
        params string[] needles
    )
    {
        return MultiMatch(
            haystack,
            needles,
            h => needles.Any(
                n => h?.Equals(n, StringComparison.CurrentCultureIgnoreCase) ?? false
            )
        );
    }

    /// <summary>
    /// Searches a master string for occurrences of any of the given strings
    /// </summary>
    /// <param name="haystack">String to search</param>
    /// <param name="needles">Strings to search for</param>
    /// <returns>True if all of the needles are found in the haystack; False otherwise</returns>
    public static bool ContainsAllOf(
        this string haystack,
        params string[] needles
    )
    {
        return MultiMatch(
            haystack,
            needles,
            h => needles.All(n => h.Contains(n.ToLower(CultureInfo.CurrentCulture)))
        );
    }

    private static bool MultiMatch(
        string haystack,
        // ReSharper disable once UnusedParameter.Local
        string[] needles,
        Func<string, bool> operation
    )
    {
        if (needles.Length == 0)
        {
            throw new ArgumentException(
                "No needles provided to search haystack for",
                nameof(needles)
            );
        }

        if (needles.Any(n => n == null))
        {
            throw new ArgumentException(
                "Null needle provided",
                nameof(needles)
            );
        }

        if (haystack == null)
        {
            return false;
        }

        haystack = haystack.ToLower(CultureInfo.InvariantCulture);
        return operation(haystack);
    }

    /// <summary>
    /// Tests if a string starts with one of the provided search strings
    /// </summary>
    /// <param name="src">String to test</param>
    /// <param name="search">Strings to look for at the start of {src}</param>
    /// <returns>True if {src} starts with any one of provided search strings; False otherwise</returns>
    public static bool StartsWithOneOf(
        this string src,
        params string[] search
    )
    {
        if (src == null)
        {
            return false;
        }

        src = src.ToLower(CultureInfo.InvariantCulture);
        return search.Any(s => src.StartsWith(s.ToLower(CultureInfo.InvariantCulture)));
    }

    /// <summary>
    /// Calls AsBytes extension method with the UTF8 encoding
    /// </summary>
    /// <param name="src">String to operate on</param>
    /// <returns>Byte array representing string, from UTF8 encoding</returns>
    public static byte[] AsBytes(
        this string src
    )
    {
        return src.AsBytes(Encoding.UTF8);
    }

    /// <summary>
    /// Convenience function to convert a string to a byte array
    /// </summary>
    /// <param name="src">String to convert</param>
    /// <param name="encoding">Encoding to use</param>
    /// <returns>Byte array of the {src} string when decoded as UTF-8</returns>
    public static byte[] AsBytes(
        this string src,
        Encoding encoding
    )
    {
        return src == null
            ? null
            : encoding.GetBytes(src);
    }

    /// <summary>
    /// Converts a string to a Stream of bytes, assuming utf-8 encoding
    /// </summary>
    /// <param name="src">String to convert</param>
    /// <returns>Stream or null if src is null</returns>
    public static Stream AsStream(
        this string src
    )
    {
        return src.AsStream(Encoding.UTF8);
    }

    /// <summary>
    /// Lowercases a string collection with Invariant culture. Tolerates nulls.
    /// </summary>
    /// <param name="src">Collection to lower-case</param>
    /// <returns>Input, lower-cased</returns>
    public static IEnumerable<string> ToLower(
        this IEnumerable<string> src
    )
    {
        return src.ToLower(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Lowercases a string collection with Invariant culture. Tolerates nulls.
    /// </summary>
    /// <param name="src">Collection to lower-case</param>
    /// <param name="cultureInfo">Culture to use in the operation</param>
    /// <returns>Input, lower-cased</returns>
    public static IEnumerable<string> ToLower(
        this IEnumerable<string> src,
        CultureInfo cultureInfo
    )
    {
        return src.Select(s => s?.ToLower(cultureInfo));
    }

    /// <summary>
    /// Uppercases a string collection with Invariant culture. Tolerates nulls.
    /// </summary>
    /// <param name="src">Collection to lower-case</param>
    /// <returns>Input, lower-cased</returns>
    public static IEnumerable<string> ToUpper(
        this IEnumerable<string> src
    )
    {
        return src.ToUpper(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Uppercases a string collection with Invariant culture. Tolerates nulls.
    /// </summary>
    /// <param name="src">Collection to lower-case</param>
    /// <param name="cultureInfo">Culture to use in the operation. Note that .NET's ToUpper doesn't accept a culture, so really, your only choices here are "Invariant" or "whatever .Net uses by default".</param>
    /// <returns>Input, lower-cased</returns>
    public static IEnumerable<string> ToUpper(
        this IEnumerable<string> src,
        CultureInfo cultureInfo
    )
    {
        return src.Select(
            s =>
                cultureInfo.Equals(CultureInfo.InvariantCulture)
                    ? s?.ToUpperInvariant()
                    : s?.ToUpper()
        );
    }

    /// <summary>
    /// Converts a string to a Stream of bytes with the provided encoding
    /// </summary>
    /// <param name="src">String to convert</param>
    /// <param name="encoding">Encoding to use</param>
    /// <returns>Stream or null if src is null</returns>
    public static Stream AsStream(
        this string src,
        Encoding encoding
    )
    {
        return src?.AsBytes(encoding)?.AsStream();
    }

    /// <summary>
    /// Attempts to encode the given byte array as if it contained a
    /// utf8-encoded string
    /// </summary>
    /// <param name="data">Bytes to encode</param>
    /// <returns>The utf8 string, if possible; will return null if given null</returns>
    public static string AsString(
        this byte[] data
    )
    {
        return data.AsString(Encoding.UTF8);
    }

    /// <summary>
    /// Attempts to encode the given byte array as if it contained a
    /// string encoded with the given encoding
    /// </summary>
    /// <param name="data">Bytes to encode</param>
    /// <param name="encoding"></param>
    /// <returns>The string, if possible; will return null if given null</returns>
    public static string AsString(
        this byte[] data,
        Encoding encoding
    )
    {
        return data is null
            ? ""
            : encoding.GetString(data);
    }

    /// <summary>
    /// Convenience function to wrap a given byte array in a MemoryStream.
    /// </summary>
    /// <param name="src">Bytes to wrapp</param>
    /// <returns>Stream wrapping the bytes or null if the source is null</returns>
    public static Stream AsStream(
        this byte[] src
    )
    {
        return src == null
            ? null
            : new MemoryStream(src);
    }

    /// <summary>
    /// Tests if a string represents an integer value
    /// </summary>
    /// <param name="src">String to test</param>
    /// <returns>True if the string can be converted to an integer; False otherwise</returns>
    public static bool IsInteger(
        this string src
    )
    {
        return int.TryParse(src, out var _);
    }

    /// <summary>
    /// Performs exceptionless conversion of a string to an integer
    /// </summary>
    /// <param name="value">String to convert</param>
    /// <returns>The integer value of the string; 0 if it cannot be converted</returns>
    public static int AsInteger(
        this string value
    )
    {
        var interestingPart = GetLeadingIntegerCharsFrom(value ?? string.Empty);
        int.TryParse(interestingPart, out var result);
        return result;
    }

    /// <summary>
    /// Turns string.IsNullOrWhiteSpace into an extension method for fluency
    /// </summary>
    /// <param name="value">String to test</param>
    /// <returns>True if is null or whitespace; False otherwise</returns>
    public static bool IsNullOrWhiteSpace(
        this string value
    )
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Turns string.IsNullOrEmpty into an extension method for fluency
    /// </summary>
    /// <param name="value">String to test</param>
    /// <returns>True if is null or whitespace; False otherwise</returns>
    public static bool IsNullOrEmpty(
        this string value
    )
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Tests if a string is empty or all whitespace
    /// NB: will return false for null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsEmptyOrWhiteSpace(
        this string value
    )
    {
        if (value is null)
        {
            return false;
        }

        return value == "" ||
            value.IsWhiteSpace();
    }

    /// <summary>
    /// Tests if a string is non-empty, but made entirely of whitespace
    /// NB: will return false for null or empty string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsWhiteSpace(
        this string value
    )
    {
        if (value is null)
        {
            return false;
        }

        if (value.Length == 0)
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!char.IsWhiteSpace(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the base64-encoded representation of a string value
    /// </summary>
    /// <param name="value">Input string value</param>
    /// <returns>The base64-encoded representation of the string, or null if the string is null</returns>
    public static string ToBase64(
        this string value
    )
    {
        return value?.AsBytes()?.ToBase64();
    }

    /// <summary>
    /// Returns true if the string is non-empty and contains only base64-encoding
    /// characters
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool LooksLikeBase64(
        this string str
    )
    {
        return !string.IsNullOrWhiteSpace(str) &&
            str.All(c => Base64Characters.Contains(c));
    }

    private const string BASE64_CHARACTER_SET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    private static readonly HashSet<char> Base64Characters = new(BASE64_CHARACTER_SET);

    /// <summary>
    /// Returns "0" if the input string is empty or null
    /// </summary>
    /// <param name="input">String to test</param>
    /// <returns>Original string or "0" if empty or null</returns>
    public static string ZeroIfEmptyOrNull(
        this string input
    )
    {
        return input.DefaultIfEmptyOrNull("0");
    }

    /// <summary>
    /// Returns a given fallback value if the input string is whitespace or null
    /// </summary>
    /// <param name="input">String to test</param>
    /// <param name="fallback">Fallback value if the input is whitespace or null</param>
    /// <returns>Original string or the given fallback if the input is whitespace or null</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string DefaultIfEmptyOrNull(
        this string input,
        string fallback
    )
    {
        return string.IsNullOrWhiteSpace(input)
            ? fallback
            : input;
    }

    /// <summary>
    /// Safely trims a string, returning an empty string if given null
    /// </summary>
    /// <param name="input">String to trim</param>
    /// <param name="trimChars">Optional params of chars to trim, passed to standard String.Trim() method</param>
    /// <returns>Empty string if input is null, otherwise trimmed input</returns>
    public static string SafeTrim(
        this string input,
        params char[] trimChars
    )
    {
        return input?.Trim(trimChars) ?? "";
    }

    /// <summary>
    /// Converts an input string to kebab-case
    /// </summary>
    /// <param name="input">string to convert</param>
    /// <returns>kebab-cased-output</returns>
    public static string ToKebabCase(
        this string input
    )
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return input.ToWordsArray()
            .JoinWith("-")
            .ToLower();
    }

    /// <summary>
    /// Replace all occurrences of chars in needles with the replaceWith character
    /// </summary>
    /// <param name="haystack"></param>
    /// <param name="needles"></param>
    /// <param name="replaceWith"></param>
    /// <returns></returns>
    public static string ReplaceAll(
        this string haystack,
        IEnumerable<char> needles,
        char replaceWith
    )
    {
        if (haystack is null)
        {
            throw new ArgumentNullException(nameof(haystack));
        }

        if (needles is null)
        {
            throw new ArgumentNullException(nameof(needles));
        }
        // we could do this:
        // return needles.Aggregate(
        //     haystack,
        //     (acc, cur) => acc.Replace(cur, replaceWith)
        // );
        // but that results in more string allocations
        // -> speed-wise, the approaches are about the same
        //    (on small-enough needle collections),
        //    but memory-wise, it makes a little difference
        //    - fewer strings to GC

        var arr = haystack.ToCharArray();
        var seek = new HashSet<char>(needles);
        for (var i = 0; i < arr.Length; i++)
        {
            if (seek.Contains(arr[i]))
            {
                arr[i] = replaceWith;
            }
        }

        return new String(arr);
    }

    /// <summary>
    /// Replace all occurrences of strings in needles with the replaceWith string
    /// </summary>
    /// <param name="haystack"></param>
    /// <param name="needles"></param>
    /// <param name="replaceWith"></param>
    /// <returns></returns>
    public static string ReplaceAll(
        this string haystack,
        IEnumerable<string> needles,
        string replaceWith
    )
    {
        if (haystack is null)
        {
            throw new ArgumentNullException(nameof(haystack));
        }

        if (needles is null)
        {
            throw new ArgumentNullException(nameof(needles));
        }

        return needles.Aggregate(
            haystack,
            (acc, cur) => acc.Replace(cur, replaceWith)
        );
    }

    /// <summary>
    /// Remove all the given characters from the string, returning a new
    /// string
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static string RemoveAll(
        this string subject,
        params char[] toRemove
    )
    {
        var seek = new HashSet<char>(toRemove);
        var result = new List<char>(subject.Length);
        foreach (var c in subject)
        {
            if (!seek.Contains(c))
            {
                result.Add(c);
            }
        }

        return new String(result.ToArray());
    }
    /// <summary>
    /// Remove all the given substrings from the string, returning a new
    /// string
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static string RemoveAll(
        this string subject,
        params string[] toRemove
    )
    {
        var result = subject;
        foreach (var item in toRemove)
        {
            result = result.Replace(item, "");
        }
        return result;
    }

    /// <summary>
    /// Converts an input string to snake_case
    /// </summary>
    /// <param name="input">string to convert</param>
    /// <returns>snake_cased_output</returns>
    public static string ToSnakeCase(
        this string input
    )
    {
        return input.ToWordsArray()
            .JoinWith("_")
            .ToLower();
    }

    /// <summary>
    /// Converts an input string to PascalCase
    /// </summary>
    /// <param name="input">string to convert</param>
    /// <returns>
    /// - pascalCasedOutput => PascalCasedOutput
    /// - pascal-cased-output => PascalCasedOutput
    /// - pascal_cased_output => PascalCasedOutput
    /// - pascal cased output => Pascal Cased Output
    /// </returns>
    public static string ToPascalCase(this string input)
    {
        return input.ToWordsArray()
            .Select(ToUpperCasedFirstLetter)
            .JoinWith("");
    }

    /// <summary>
    /// Alias for ToPascalCase
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToTitleCase(this string input)
    {
        return input.ToPascalCase();
    }

    /// <summary>
    /// Converts an input string to camelCase
    /// </summary>
    /// <param name="input">string to convert</param>
    /// <returns>camelCasedOutput</returns>
    public static string ToCamelCase(
        this string input
    )
    {
        return input.ToPascalCase()
            .ToLowerCasedFirstLetter();
    }

    /// <summary>
    /// Returns the input string in RaNdOMizEd case
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToRandomCase(
        this string input
    )
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        if (!AlphaNumericMatch.IsMatch(input))
        {
            return input;
        }

        var result = RandomizeCase(input);
        while (result == input)
        {
            result = RandomizeCase(input);
        }

        return result;
    }

    private static readonly Regex AlphaNumericMatch = new Regex("[a-z]+", RegexOptions.IgnoreCase);

    private static string RandomizeCase(string input)
    {
        return string.Join(
            "",
            input
                .Select(c => c.ToString())
                .Select(
                    c => RandomNumber.NextDouble() < 0.5
                        ? c.ToLowerInvariant()
                        : c.ToUpperInvariant()
                )
        );
    }

    /// <summary>
    /// Converts an input string to words, where possible
    /// eg: kebab-case => "kebab case"
    ///     snake_case => "snake case"
    ///     PascalCase => "pascal case"
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToWords(
        this string input
    )
    {
        return string.Join(
            " ",
            input.ToWordsArray()
        ).ToLower();
    }

    /// <summary>
    /// Breaks a string into an array of words without
    /// any modifications on casing. Ported from lodash's
    /// unicodeWords
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string[] ToWordsArray(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return new[]
            {
                input
            };
        }

        var result = new List<string>();
        foreach (Match match in UnicodeWords.Matches(input))
        {
            result.Add(match.Value);
        }

        return result.ToArray();
    }

    const string rsAstralRange = "\\ud800-\\udfff";
    const string rsComboMarksRange = "\\u0300-\\u036f";
    const string reComboHalfMarksRange = "\\ufe20-\\ufe2f";
    const string rsComboSymbolsRange = "\\u20d0-\\u20ff";
    const string rsComboMarksExtendedRange = "\\u1ab0-\\u1aff";
    const string rsComboMarksSupplementRange = "\\u1dc0-\\u1dff";

    const string rsComboRange = rsComboMarksRange + reComboHalfMarksRange + rsComboSymbolsRange +
        rsComboMarksExtendedRange + rsComboMarksSupplementRange;

    const string rsDingbatRange = "\\u2700-\\u27bf";
    const string rsLowerRange = "a-z\\xdf-\\xf6\\xf8-\\xff";
    const string rsMathOpRange = "\\xac\\xb1\\xd7\\xf7";
    const string rsNonCharRange = "\\x00-\\x2f\\x3a-\\x40\\x5b-\\x60\\x7b-\\xbf";
    const string rsPunctuationRange = "\\u2000-\\u206f";

    const string rsSpaceRange =
        " \\t\\x0b\\f\\xa0\\ufeff\\n\\r\\u2028\\u2029\\u1680\\u180e\\u2000\\u2001\\u2002\\u2003\\u2004\\u2005\\u2006\\u2007\\u2008\\u2009\\u200a\\u202f\\u205f\\u3000";

    const string rsUpperRange = "A-Z\\xc0-\\xd6\\xd8-\\xde";
    const string rsVarRange = "\\ufe0e\\ufe0f";
    const string rsBreakRange = rsMathOpRange + rsNonCharRange + rsPunctuationRange + rsSpaceRange;

    /** Used to compose unicode capture groups. */
    const string rsApos = "['\u2019]";

    const string rsBreak = $"[{rsBreakRange}]";
    const string rsCombo = $"[{rsComboRange}]";
    const string rsDigit = "\\d";
    const string rsDingbat = $"[{rsDingbatRange}]";
    const string rsLower = $"[{rsLowerRange}]";

    const string rsMisc =
        $"[^{rsAstralRange}{rsBreakRange + rsDigit + rsDingbatRange + rsLowerRange + rsUpperRange}]";

    const string rsFitz = "\\ud83c[\\udffb-\\udfff]";
    const string rsModifier = $"(?:{rsCombo}|{rsFitz})";
    const string rsNonAstral = $"[^{rsAstralRange}]";
    const string rsRegional = "(?:\\ud83c[\\udde6-\\uddff]){2}";
    const string rsSurrPair = "[\\ud800-\\udbff][\\udc00-\\udfff]";
    const string rsUpper = $"[{rsUpperRange}]";
    const string rsZWJ = "\\u200d";

    /** Used to compose unicode regexes. */
    const string rsMiscLower = $"(?:{rsLower}|{rsMisc})";

    const string rsMiscUpper = $"(?:{rsUpper}|{rsMisc})";
    const string rsOptContrLower = $"(?:{rsApos}(?:d|ll|m|re|s|t|ve))?";
    const string rsOptContrUpper = $"(?:{rsApos}(?:D|LL|M|RE|S|T|VE))?";
    const string reOptMod = $"{rsModifier}?";
    const string rsOptVar = $"[{rsVarRange}]?";

    static readonly string rsOptJoin =
        $"(?:{rsZWJ}(?:{string.Join("|", rsNonAstral, rsRegional, rsSurrPair)}){rsOptVar + reOptMod})*";

    const string rsOrdLower = "\\d*(?:1st|2nd|3rd|(?![123])\\dth)(?=\\b|[A-Z_])";
    const string rsOrdUpper = "\\d*(?:1ST|2ND|3RD|(?![123])\\dTH)(?=\\b|[a-z_])";
    static readonly string rsSeq = rsOptVar + reOptMod + rsOptJoin;
    static readonly string rsEmoji = $"(?:{string.Join("|", rsDingbat, rsRegional, rsSurrPair)}){rsSeq}";

    const string rsUUID = "[a-f\\d]{4}(?:[a-f\\d]{4}-){4}[a-f\\d]{12}";

    static readonly Regex UnicodeWords = new(
        string.Join(
            "|",
            rsUUID,
            $"{rsUpper}?{rsLower}+{rsOptContrLower}(?={string.Join("|", rsBreak, rsUpper, "$")})",
            $"{rsMiscUpper}+{rsOptContrUpper}(?={string.Join("|", rsBreak, rsUpper + rsMiscLower, "$")})",
            $"{rsUpper}?{rsMiscLower}+{rsOptContrLower}",
            $"{rsUpper}+{rsOptContrUpper}",
            rsOrdUpper,
            rsOrdLower,
            $"{rsDigit}+",
            rsEmoji
        ),
        RegexOptions.Compiled
    );

    /// <summary>
    /// Lower-cases the first letter in your string
    /// </summary>
    /// <param name="input">string to operate on</param>
    /// <returns>string with lower-cased first letter or null if input was null</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string ToLowerCasedFirstLetter(
        this string input
    )
    {
        return (input?.Length ?? 0) > 0
            // ReSharper disable once PossibleNullReferenceException
            ? $"{input[0].ToString().ToLower()}{input.Substring(1)}"
            : input;
    }

    /// <summary>
    /// Upper-cases the first letter in your string
    /// </summary>
    /// <param name="input">string to operate on</param>
    /// <returns>string with upper-cased first letter or null if input was null</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static string ToUpperCasedFirstLetter(
        this string input
    )
    {
        return (input?.Length ?? 0) > 0
            // ReSharper disable once PossibleNullReferenceException
            ? $"{input[0].ToString().ToUpper()}{input.Substring(1)}"
            : input;
    }

#if NETSTANDARD
    /// <summary>
    /// Provides an in-place shim for the ToLower method
    /// which is used from .net framework; the latter
    /// can accept a CultureInfo parameter, where .net standard
    /// cannot, so the parameter is just dropped
    /// </summary>
    public static string ToLower(this string input, CultureInfo _)
    {
        return input.ToLower();
    }
#endif

    private static string GetLeadingIntegerCharsFrom(
        string value
    )
    {
        var collected = new List<string>();
        var intMarker = 0;
        value.ForEach(
            c =>
            {
                if (intMarker > 1)
                {
                    return;
                }

                var asString = c.ToString();
                if ("1234567890".Contains(asString))
                {
                    intMarker = 1;
                    collected.Add(asString);
                    return;
                }

                if (intMarker == 1)
                {
                    intMarker++;
                }
            }
        );
        return collected.JoinWith(string.Empty);
    }

    /// <summary>
    /// Returns whether or not a string is an integer value
    /// </summary>
    /// <param name="str">string to test</param>
    /// <returns></returns>
    public static bool IsNumeric(
        this string str
    )
    {
        return !string.IsNullOrWhiteSpace(str) &&
            str.All(IsNumeric);
    }

    /// <summary>
    /// Tests if a string is Alphanumeric. Fails on null or whitespace too.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsAlphanumeric(
        this string str
    )
    {
        return !string.IsNullOrWhiteSpace(str) &&
            str.All(c => IsAlpha(c) || IsNumeric(c));
    }

    /// <summary>
    /// Tests if a string is Alphabetic only. Fails on null or whitespace too.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsAlpha(
        this string str
    )
    {
        return !string.IsNullOrWhiteSpace(str) &&
            str.All(IsAlpha);
    }

    /// <summary>
    /// Tests if a character is numeric (0-9)
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsNumeric(
        this char c
    )
    {
        return c >= '0' && c <= '9';
    }

    /// <summary>
    /// Tests if a character is alphabetic (a-z|A-Z)
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public static bool IsAlpha(
        this char c
    )
    {
        return (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z');
    }

    /// <summary>
    /// Convenience wrapper to provide a memory stream around a string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static MemoryStream ToMemoryStream(
        this string str
    )
    {
        var asBytes = str.AsBytes();
        return new MemoryStream(asBytes);
    }

    /// <summary>
    /// Surrounds a string with quotes if it contains any whitespace
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string QuoteIfSpaced(
        this string str
    )
    {
        if (str.IsNullOrEmpty())
        {
            return "";
        }

        return Whitespace.IsMatch(str)
            ? $"\"{str}\""
            : str;
    }

    /// <summary>
    /// Splits a commandline, respecting quoting
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[] SplitCommandline(
        this string str
    )
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return new string[0];
        }

        return CommandlinePartsMatcher
            .Matches(str)
            .OfType<Match>()
            .Select(m => m.Value.DeQuote())
            .ToArray();
    }

    /// <summary>
    /// "de-quotes" a string, only removes the outer-most, paired
    /// quotes, not just trimming, ie
    /// ""foo"" => "foo"
    /// "foo" => foo
    /// "foo => "foo
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string DeQuote(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        if (str == "\"")
        {
            // edge-case
            return str;
        }

        return str.StartsWith("\"") &&
            str.EndsWith("\"")
                ? str.Substring(1, str.Length - 2)
                : str;
    }

    /// <summary>
    /// tests if two string collections are identical, taking into account
    /// the provide comparison
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="comparison"></param>
    /// <returns>
    /// true if collections are of the same size and each item, in order,
    /// from the left item, matches the right one
    /// </returns>
    public static bool Matches(
        this IEnumerable<string> left,
        IEnumerable<string> right,
        StringComparison comparison
    )
    {
        return left.Matches(
            right,
            (a, b) =>
            {
                if (a is null && b is null)
                {
                    return true;
                }

                if (a is null || b is null)
                {
                    return false;
                }

                return a.Equals(b, comparison);
            }
        );
    }

    private static readonly Regex CommandlinePartsMatcher =
        new Regex("((\"[^\"]+\")|([^ ]+))");

    private static readonly Regex Whitespace =
        new Regex("\\s");

    /// <summary>
    /// Returns the substring of the given string from the given start
    /// Tolerates a start outside of the string - will return empty string
    /// Tolerates a start &lt; 0 - will start from the beginning of the string
    /// Tolerates null string - will return empty string
    /// </summary>
    /// <param name="str">string to operate on</param>
    /// <param name="start">desired starting point</param>
    /// <returns></returns>
    public static string Substr(
        this string str,
        int start
    )
    {
        return str.Substr(start, str?.Length ?? 0);
    }

    /// <summary>
    /// Returns the substring of the given string from the given start
    /// with the provided length
    /// Tolerates a start after of the string - will return empty string
    /// Tolerates a start &lt; 0 - will start from the beginning of the string
    /// Tolerates null string - will return empty string
    /// Tolerates a length out of bounds - will return all the string that it can
    /// Interprets a negative length as an offset from the end of the string
    /// </summary>
    /// <param name="str"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string Substr(
        this string str,
        int start,
        int length
    )
    {
        if (str is null ||
            start > str.Length)
        {
            return "";
        }

        if (start < 0)
        {
            start = 0;
        }

        if (length < 0)
        {
            length = str.Length - start + length;
        }

        if (start + length > str.Length)
        {
            length = str.Length - start;
        }

        return str.Substring(start, length);
    }

    /// <summary>
    /// Finds a window into a string, centered around the given
    /// centeredOn parameter, with maximum maxCharsLeftOrRight
    /// of the center.
    /// Eg: "12345".Window(2, 1) -> 234
    /// </summary>
    /// <param name="str"></param>
    /// <param name="centeredOn"></param>
    /// <param name="maxCharsLeftOrRight"></param>
    /// <returns></returns>
    public static string Window(
        this string str,
        int centeredOn,
        int maxCharsLeftOrRight
    )
    {
        return str.Window(
            centeredOn,
            maxCharsLeftOrRight,
            out _,
            out _
        );
    }

    /// <summary>
    /// Finds a window into a string, centered around the given
    /// centeredOn parameter, with maximum maxCharsLeftOrRight
    /// of the center.
    /// Eg: "12345".Window(2, 1) -> 234
    /// </summary>
    /// <param name="str"></param>
    /// <param name="centeredOn"></param>
    /// <param name="maxCharsLeftOrRight"></param>
    /// <param name="startedAt"></param>
    /// <param name="endedAt"></param>
    /// <returns></returns>
    public static string Window(
        this string str,
        int centeredOn,
        int maxCharsLeftOrRight,
        out int startedAt,
        out int endedAt
    )
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            startedAt = 0;
            endedAt = 0;
            return "";
        }

        if (centeredOn >= str.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(centeredOn),
                $"{nameof(centeredOn)} should be an index within the string"
            );
        }

        if (maxCharsLeftOrRight < 0)
        {
            maxCharsLeftOrRight = 0;
        }

        if (maxCharsLeftOrRight >= int.MaxValue / 2)
        {
            maxCharsLeftOrRight = int.MaxValue / 2 - 1;
        }

        startedAt = centeredOn - maxCharsLeftOrRight;
        if (startedAt < 0)
        {
            startedAt = 0;
        }

        var len = Math.Max(maxCharsLeftOrRight * 2 + 1, 1);
        endedAt = centeredOn + len;

        return str.Substr(
            startedAt,
            len
        );
    }

    /// <summary>
    /// Converts a base64 string back to the original string
    /// - assumes the original string is UTF8
    /// </summary>
    /// <param name="base64Data"></param>
    /// <returns></returns>
    public static byte[] UnBase64(this string base64Data)
    {
        return Convert.FromBase64String(base64Data.Base64Padded());
    }

    /// <summary>
    /// Converts a base64 string back to the original string
    /// using the provided encoding
    /// </summary>
    /// <param name="base64Data"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string UnBase64(
        this string base64Data,
        Encoding encoding
    )
    {
        var data = Convert.FromBase64String(base64Data.Base64Padded());
        return encoding.GetString(data);
    }

    /// <summary>
    /// Quick-decode for string base64 data
    /// T _must_ be a string, ie var str = base64.UnBase64&lt;string&gt;()
    /// - assumes UTF8 encoding
    /// </summary>
    /// <param name="base64Data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string UnBase64<T>(
        this string base64Data
    )
    {
        return base64Data.UnBase64<T>(Encoding.UTF8);
    }

    /// <summary>
    /// Quick-decode for string base64 data using the provided encoding
    /// T _must_ be a string, ie var str = base64.UnBase64&lt;string&gt;()
    /// </summary>
    /// <param name="base64Data"></param>
    /// <param name="encoding"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string UnBase64<T>(
        this string base64Data,
        Encoding encoding
    )
    {
        if (typeof(T) != typeof(string))
        {
            throw new NotImplementedException(
                $"UnBase64<T> without a deserializer Func only supports strings"
            );
        }

        return base64Data.UnBase64(DecodeBytes);

        string DecodeBytes(byte[] data)
        {
            return encoding.GetString(data);
        }
    }

    /// <summary>
    /// decode base64 string data &amp; deserialize to type T
    /// </summary>
    /// <param name="base64Data"></param>
    /// <param name="deserializer"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T UnBase64<T>(
        this string base64Data,
        Func<string, T> deserializer
    )
    {
        return base64Data.UnBase64(deserializer, Encoding.UTF8);
    }

    /// <summary>
    /// decode base64 string data &amp; deserialize to type T
    /// </summary>
    /// <param name="base64Data"></param>
    /// <param name="deserializer"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T UnBase64<T>(
        this string base64Data,
        Func<byte[], T> deserializer
    )
    {
        var byteData = Convert.FromBase64String(
            base64Data.Base64Padded()
        );
        return deserializer(byteData);
    }

    /// <summary>
    /// decode base64 string data &amp; deserialize to type T,
    /// assuming that the original data in the base64 string was
    /// a string (eg json), using the provided encoding
    /// </summary>
    /// <param name="base64Data"></param>
    /// <param name="deserializer"></param>
    /// <param name="encoding"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T UnBase64<T>(
        this string base64Data,
        Func<string, T> deserializer,
        Encoding encoding
    )
    {
        var unencoded = base64Data.UnBase64(encoding);
        return deserializer(unencoded);
    }

    /// <summary>
    /// Pads out a base64 string which is missing the base64 padding
    /// </summary>
    /// <param name="unpadded"></param>
    /// <returns></returns>
    public static string Base64Padded(
        this string unpadded
    )
    {
        return $"{unpadded}{Base64PaddingSuffixes[unpadded.Length % 4]}";
    }

    /// <summary>
    /// Removes base64 data padding (trailing '=' chars)
    /// - symmetrical with Base64Padded()
    /// </summary>
    /// <param name="base64Data"></param>
    /// <returns></returns>
    public static string Base64UnPadded(
        this string base64Data
    )
    {
        return $"{base64Data?.Trim('=')}";
    }

    private static readonly Dictionary<int, string> Base64PaddingSuffixes
        = new()
        {
            [0] = "",
            [1] = "===",
            [2] = "==",
            [3] = "="
        };

    /// <summary>
    /// Outdents a block to the first indentation.
    /// </summary>
    /// <param name="str">string to outdent</param>
    /// <returns></returns>
    public static string Outdent(this string str)
    {
        return str.Outdent(int.MaxValue);
    }

    /// <summary>
    /// Outdents a block of text at most to the given depth. Will
    /// stop as soon as any line is outdented completely.
    /// </summary>
    /// <param name="str">string to outdent</param>
    /// <param name="depth">depth to outdent to</param>
    /// <returns></returns>
    public static string Outdent(this string str, int depth)
    {
        str ??= "";
        var lineDelimiter = str.Contains(Environment.NewLine)
            ? Environment.NewLine
            : "\n";
        var lines = str.Split(
            new[]
            {
                lineDelimiter
            },
            StringSplitOptions.None
        );
        var outdented = lines.Outdent(depth);
        return outdented.JoinWith(lineDelimiter);
    }

    /// <summary>
    /// Splits text into lines, based on UNIX or DOS line endings,
    /// or a mixture of both
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string[] SplitIntoLines(
        this string str
    )
    {
        return str.MultiSplit("\r\n", "\n", "\r");
    }

    /// <summary>
    /// Splits a string based on one or more delimiters
    /// </summary>
    /// <param name="str"></param>
    /// <param name="delimiter"></param>
    /// <param name="moreDelimiters"></param>
    /// <returns></returns>
    public static string[] MultiSplit(
        this string str,
        string delimiter,
        params string[] moreDelimiters
    )
    {
        var allDelimiters = new[]
        {
            delimiter
        }.Concat(moreDelimiters).ToArray();
        return str.Split(allDelimiters, StringSplitOptions.None);
    }

    private static readonly Regex NewLines = new("(\r\n|\n|\r)+");

    /// <summary>
    /// Outdents a block of text at most to the given depth. Will
    /// stop as soon as any line is outdented completely.
    /// Attempts to determine the indent character from the first line with
    /// indenting.
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="depth">depth to outdent to</param>
    /// <returns></returns>
    public static string[] Outdent(this string[] lines, int depth)
    {
        var indentedWith = FindFirstIndentationSequence(lines);
        return lines.Outdent(indentedWith, depth, true);
    }

    private static string FindFirstIndentationSequence(string[] lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            foreach (var search in IndentationSearch)
            {
                if (line.StartsWith(search, StringComparison.InvariantCulture))
                {
                    return search;
                }
            }
        }

        return " ";
    }

    private static readonly string[] IndentationSearch = new[]
    {
        " ",
        "\t"
    };

    /// <summary>
    /// Outdents a block of text at most to the given depth. Will
    /// stop as soon as any line is outdented completely.
    /// </summary>
    /// <param name="lines"></param>
    /// <param name="indentedWith"></param>
    /// <param name="depth"></param>
    /// <param name="alsoTrimEnd">also right-trim lines, much like an auto-formatter would</param>
    /// <returns></returns>
    public static string[] Outdent(
        this string[] lines,
        string indentedWith,
        int depth,
        bool alsoTrimEnd
    )
    {
        if (string.IsNullOrEmpty(indentedWith))
        {
            return lines.ToArray(); // caller always gets a new copy
        }

        if (!string.IsNullOrWhiteSpace(indentedWith))
        {
            throw new ArgumentException(
                "non-whitespace indents are not supported",
                nameof(indentedWith)
            );
        }

        var minIndent = FindMinimumIndent(lines, indentedWith);
        var toOutdent = Math.Min(minIndent, depth);
        var toOutdentChars = toOutdent * indentedWith.Length;
        var result = new List<string>();
        foreach (var line in lines)
        {
            var thisLine = alsoTrimEnd
                ? line.TrimEnd()
                : line;
            if (thisLine.Length < toOutdentChars)
            {
                result.Add(thisLine);
            }
            else
            {
                result.Add(thisLine.Substring(toOutdentChars));
            }
        }

        return result.ToArray();
    }

    private static int FindMinimumIndent(
        string[] lines,
        string indentedWith
    )
    {
        var result = int.MaxValue;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var offset = 0;
            while (
                line.IndexOf(indentedWith, offset, StringComparison.InvariantCulture) == offset
            )
            {
                offset += indentedWith.Length;
            }

            var indent = offset / indentedWith.Length;
            if (indent < result)
            {
                result = indent;
            }
        }

        return result;
    }

    /// <summary>
    /// Attempt to parse a string as an integer encoded with hex
    /// </summary>
    /// <param name="str"></param>
    /// <param name="parsed"></param>
    /// <returns></returns>
    public static bool TryParseHex(
        this string str,
        out int parsed
    )
    {
        try
        {
            parsed = Convert.ToInt32(str, 16);
            return true;
        }
        catch (OverflowException)
        {
            parsed = Int32.MaxValue;
            return false;
        }
        catch (FormatException)
        {
            parsed = 0;
            return false;
        }
    }

    /// <summary>
    /// Split the string only once, eg so "foo:bar:qux" split on ":" returns [ "foo", "bar:qux" ]
    /// edge cases:
    /// null returns empty array
    /// no delimiter -> array with single element (original string)
    /// </summary>
    /// <param name="str"></param>
    /// <param name="splitOn"></param>
    /// <returns></returns>
    public static string[] SplitOnce(
        this string str,
        string splitOn
    )
    {
        if (str is null)
        {
            return Array.Empty<string>();
        }

        if (splitOn is null)
        {
            throw new ArgumentNullException(nameof(splitOn));
        }

        var idx = str.IndexOf(splitOn, StringComparison.InvariantCulture);
        if (idx < 0)
        {
            return new[]
            {
                str
            };
        }

        var first = str.Substring(0, idx);
        var second = str.Substring(idx + splitOn.Length);
        return new[]
        {
            first,
            second
        };
    }

    /// <summary>
    /// Splits a string as if it were a path, treating
    /// / and \ both as path delimiters
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[] SplitPath(
        this string str
    )
    {
        return str.SplitByRegex(PathSplitRegex);
    }

    private static Regex PathSplitRegex =>
        _pathSplitRegex ??= new Regex("[\\\\|/]", RegexOptions.Compiled);

    private static Regex _pathSplitRegex;

    /// <summary>
    /// Split a string by the provided C# regular expression
    /// </summary>
    /// <param name="str"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public static string[] SplitByRegex(
        this string str,
        string regex
    )
    {
        return str.SplitByRegex(regex, RegexOptions.None);
    }

    /// <summary>
    /// Split a string by the provided C# regular expression with
    /// the provided regex options
    /// </summary>
    /// <param name="str"></param>
    /// <param name="regex"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string[] SplitByRegex(
        this string str,
        string regex,
        RegexOptions options
    )
    {
        return str.SplitByRegex(
            new Regex(regex, options)
        );
    }

    /// <summary>
    /// Split a string by the provided Regex
    /// </summary>
    /// <param name="str"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public static string[] SplitByRegex(
        this string str,
        Regex regex
    )
    {
        if (str is null)
        {
            return Array.Empty<string>();
        }

        return regex.Split(str);
    }

    /// <summary>
    /// Resolves the result of a path containing .. and/or .
    /// </summary>
    /// <param name="path"></param>
    /// <param name="others"></param>
    /// <returns></returns>
    public static string ResolvePath(
        this string path,
        params string[] others
    )
    {
        if (path is null)
        {
            return "";
        }

        var joinWith = path.IndexOf('\\') > -1
            ? "\\"
            : "/";
        var all = new List<string>();
        foreach (var item in new[]
                 {
                     path
                 }.Concat(others).ToList())
        {
            if (item.IsAbsolutePath())
            {
                all.Clear();
            }

            all.Add(item);
        }

        path = string.Join(joinWith, all);

        var result = new List<string>();
        var parts = path.SplitPath();
        foreach (var part in parts)
        {
            switch (part)
            {
                case ".":
                    continue;
                case "..":
                    if (result.Count > 0)
                    {
                        if (result.Count > 1 || !IsRootPathElement(result[0]))
                        {
                            result.RemoveAt(result.Count - 1);
                        }
                    }

                    continue;
                default:
                    result.Add(part);
                    break;
            }
        }

        return string.Join(joinWith, result);

        bool IsRootPathElement(string str)
        {
            return str is "" or "/" ||
                WindowsDriveMatch.IsMatch(str);
        }
    }

    private static readonly Regex WindowsDriveMatch = new("^[A-Za-z]{1}:");

    /// <summary>
    /// Returns true if the given string looks like an absolute path,
    /// ie starts with one of:
    /// - /
    /// - \
    /// - windows drive (eg C:)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsAbsolutePath(
        this string str
    )
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        return str.StartsWith("/") ||
            str.StartsWith("\\") ||
            WindowsDriveMatch.IsMatch(str);
    }


#if !BUILD_PEANUTBUTTER_INTERNAL
        /// <summary>
        /// Determine the relative path from the string being operated on
        /// to the one passed in. Eg, if operating on the path
        /// '/foo/bar', provided '/foo', the result is 'bar'
        /// The aim is to provide a C# analogue to Node's path.relative(...)
        /// Paths are treated automatically via this overload:
        /// - split on any path delimiter (/ or \)
        /// - compared case-insensitive on windows, case-sensitive elsewhere
        /// </summary>
        /// <param name="path"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static string RelativeTo(
            this string path,
            string basePath
        )
        {
            return path.RelativeTo(basePath, PathType.Auto);
        }

        /// <summary>
        /// Determine the relative path from the string being operated on
        /// to the one passed in. Eg, if operating on the path
        /// '/foo/bar', provided '/foo', the result is 'bar'
        /// The aim is to provide a C# analogue to Node's path.relative(...)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="basePath"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public static string RelativeTo(
            this string path,
            string basePath,
            PathType pathType
        )
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (basePath is null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            var compare = pathType switch
            {
                PathType.Auto => Platform.IsWindows
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase,
                PathType.Unix => StringComparison.Ordinal,
                PathType.Windows => StringComparison.OrdinalIgnoreCase,
                _ => throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null)
            };
            if (path.Equals(basePath, compare))
            {
                return string.Empty;
            }

            var result = new List<string>();
            var pathParts = path.SplitPath();
            var baseParts = basePath.SplitPath();
            var end = Math.Max(pathParts.Length, baseParts.Length);
            var matching = true;

            for (var i = 0; i < end; i++)
            {
                if (i >= pathParts.Length)
                {
                    result.Insert(0, "..");
                }
                else if (i >= baseParts.Length)
                {
                    result.Add(pathParts[i]);
                }
                else
                {
                    matching = matching && pathParts[i].Equals(baseParts[i], compare);
                    if (matching)
                    {
                        continue;
                    }

                    result.Insert(0, "..");
                    result.Add(pathParts[i]);
                }
            }


            return result.JoinPath(pathType);
        }
#endif

    /// <summary>
    /// Given a path in any format (unix or dos),
    /// returns the path normalised for the current OS
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string AsPlatformPath(
        this string path
    )
    {
        return path
            .Replace("\\", DirectorySeparator)
            .Replace("/", DirectorySeparator);
    }

    private static readonly string DirectorySeparator
        = Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// Tests that the provided strings (needles) are found
    /// in order within haystack, eg
    /// "foo bar quux".ContainsInOrder("foo", "quux"); // returns true
    /// </summary>
    /// <param name="haystack">master string to search</param>
    /// <param name="needles">substrings to search for</param>
    /// <returns></returns>
    public static bool ContainsInOrder(
        this string haystack,
        params string[] needles
    )
    {
        return haystack.ContainsInOrder(
            StringComparison.CurrentCulture,
            needles
        );
    }

    /// <summary>
    /// Tests that the provided strings (needles) are found
    /// in order within haystack, using the provided comparison, eg
    /// "foo bar quux".ContainsInOrder(StringComparison.OrdinalIgnoreCase, "Foo", "qUUx"); // returns true
    /// </summary>
    /// <param name="haystack">master string to search</param>
    /// <param name="comparison"></param>
    /// <param name="needles">substrings to search for</param>
    /// <returns></returns>
    public static bool ContainsInOrder(
        this string haystack,
        StringComparison comparison,
        params string[] needles
    )
    {
        if (string.IsNullOrEmpty(haystack))
        {
            return false;
        }

        if (needles.Length == 0)
        {
            throw new ArgumentException(
                "No needles provided to search for in the haystack",
                nameof(needles)
            );
        }

        var from = 0;
        foreach (var needle in needles)
        {
            var idx = haystack.IndexOf(needle, from, comparison);
            if (idx < 0)
            {
                return false;
            }

            from = idx;
        }

        return true;
    }

    /// <summary>
    /// GZips the text as utf 8
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static byte[] GZip(this string str)
    {
        return str.GZip(Encoding.UTF8);
    }

    /// <summary>
    /// GZips the text with the provided encoding
    /// </summary>
    /// <param name="str"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static byte[] GZip(
        this string str,
        Encoding encoding
    )
    {
        var bytes = encoding.GetBytes(str);
        return bytes.GZip();
    }

    /// <summary>
    /// Appends the provided parts as path elements,
    /// using the path delimiter of the current platform,
    /// ensuring only a single path delimiter between
    /// elements whilst retaining any initial leading delimiter
    /// and any trailing delimiter, ie
    /// "foo/bar/".AppendPath("/part0", "part1", "/part2/", "final/")
    /// should produce
    /// foo/bar/part0/part1/part2/final/
    /// </summary>
    /// <param name="start"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string AppendPath(
        this string start,
        params string[] parts
    )
    {
        return start.AppendPath(
            Path.DirectorySeparatorChar,
            parts
        );
    }

    /// <summary>
    /// Appends the provided parts as path elements,
    /// using a forward-slash, as per web paths,
    /// ensuring only a single path delimiter between
    /// elements whilst retaining any initial leading delimiter
    /// and any trailing delimiter, ie
    /// "foo/bar/".AppendPath("/part0", "part1", "/part2/", "final/")
    /// should produce
    /// foo/bar/part0/part1/part2/final/
    /// </summary>
    /// <param name="start"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string AppendWebPath(
        this string start,
        params string[] parts
    )
    {
        return start.AppendPath(
            '/',
            parts
        );
    }

    /// <summary>
    /// Appends the provided parts as path elements,
    /// using the provided path delimiter, ensuring
    /// only a single path delimiter between elements
    /// whilst retaining any initial leading delimiter
    /// and any trailing delimiter, ie:
    /// "foo/bar/".AppendPath('/', "/part0", "part1", "/part2/", "final/")
    /// should produce
    /// foo/bar/part0/part1/part2/final/
    /// </summary>
    /// <param name="start"></param>
    /// <param name="delimiter"></param>
    /// <param name="parts"></param>
    /// <returns></returns>
    public static string AppendPath(
        this string start,
        char delimiter,
        params string[] parts
    )
    {
        if (parts.Length == 0)
        {
            return start;
        }

        var allParts = new List<string>()
        {
            start.TrimEnd(delimiter)
        };
        allParts.AddRange(
            parts.Select(
                s => s.TrimStart(delimiter).TrimEnd(delimiter)
            )
        );
        var lastPart = parts.Last();
        var hadTrailingDelimiter = lastPart.IndexOf(delimiter) == lastPart.Length - 1;
        var joined = allParts.JoinWith(delimiter);
        return hadTrailingDelimiter
            ? $"{joined}{delimiter}"
            : joined;
    }
}