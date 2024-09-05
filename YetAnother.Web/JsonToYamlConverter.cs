using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Schemas;
using System.Diagnostics.CodeAnalysis;

namespace YetAnother.Web;

public class JsonToYamlConverter
{
    public string? JsonString { get; set; }
    public string? YamlString { get; set; }

    private JsonLoadSettings jsonLoadSettings = new JsonLoadSettings()
    {
        CommentHandling = CommentHandling.Load,
        LineInfoHandling = LineInfoHandling.Load,
        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
    };

    private EmitterSettings emitterSettings = new EmitterSettings(
        bestIndent: 2, 
        bestWidth: 120,
        isCanonical: false,
        maxSimpleKeyLength: 120,
        skipAnchorName: false,
        indentSequences: false,
        newLine: null)
    {
        
    };

    public JToken ParseJson(string? json)
    {
        return JToken.Parse(json ?? "", jsonLoadSettings);
    }

    public string ConvertToYaml(JToken token)
    {
        using var stringWriter = new StringWriter();
        var emitter = new Emitter(stringWriter, emitterSettings);
        emitter.Emit(new StreamStart());
        emitter.Emit(new DocumentStart());

        try
        {
            EmitJTokenToYaml(emitter, token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }
        
        emitter.Emit(new DocumentEnd(true));
        emitter.Emit(new StreamEnd());
        return stringWriter.ToString();
    }

    private void EmitJTokenToYaml(IEmitter emitter, JToken token, JTokenReader? reader = null)
    {
        reader ??= new JTokenReader(token);

        int previousLine = -1;
        while (reader.Read())
        {
            bool isOnNewLine = true;
            if (reader.CurrentToken.TryGetLineNumber(out var currentLineNumber))
            {
                isOnNewLine = previousLine != currentLineNumber;
                previousLine = currentLineNumber;
            }

            TagName tag = TagName.Empty;

            switch (reader.TokenType)
            {
                case JsonToken.None:
                    break;

                case JsonToken.StartObject:
                    tag = FailsafeSchema.Tags.Map;
                    JContainer jobject = (JContainer?)reader.CurrentToken ?? throw new JsonSerializationException();
                    MappingStyle mappingStyle = MappingStyle.Any;
                    if (jobject.Last?.GetLineNumber() == currentLineNumber)
                    {
                        mappingStyle = MappingStyle.Flow;
                    }
                    emitter.Emit(new MappingStart(AnchorName.Empty, tag, true, mappingStyle));
                    break;

                case JsonToken.StartArray:
                    tag = FailsafeSchema.Tags.Seq;
                    JContainer jarray = (JContainer?)reader.CurrentToken ?? throw new JsonSerializationException();
                    SequenceStyle sequenceStyle = SequenceStyle.Any;
                    if (jarray.Last?.GetLineNumber() == currentLineNumber)
                    {
                        sequenceStyle = SequenceStyle.Flow;
                    }
                    emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, sequenceStyle));
                    break;

                case JsonToken.StartConstructor:
                    throw new NotSupportedException(reader.TokenType.ToString());
                    break;

                case JsonToken.PropertyName:
                    tag = FailsafeSchema.Tags.Str;
                    EmitPropertyNameToYaml(emitter, reader.Value as string ?? string.Empty, reader.CurrentToken!);
                    break;

                case JsonToken.Comment:
                    emitter.Emit(new Comment(reader.Value?.ToString() ?? string.Empty, !isOnNewLine));
                    break;

                case JsonToken.Raw:
                    throw new NotSupportedException(reader.TokenType.ToString());
                    break;

                case JsonToken.String:
                    EmitStringToYaml(emitter, reader.Value as string ?? string.Empty, reader.CurrentToken!);
                    break;

                case JsonToken.Integer:
                    tag = JsonSchema.Tags.Int;
                    goto case JsonToken.Undefined;
                case JsonToken.Float:
                    tag = JsonSchema.Tags.Float;
                    goto case JsonToken.Undefined;
                case JsonToken.Boolean:
                    tag = JsonSchema.Tags.Bool;
                    goto case JsonToken.Undefined;
                case JsonToken.Null:
                    tag = JsonSchema.Tags.Null;
                    goto case JsonToken.Undefined;
                case JsonToken.Date:
                    tag = DefaultSchema.Tags.Timestamp;
                    goto case JsonToken.Undefined;
                case JsonToken.Undefined:
                    emitter.Emit(new Scalar(AnchorName.Empty, tag, reader.Value?.ToString() ?? string.Empty,
                        ScalarStyle.Any, isPlainImplicit: true, isQuotedImplicit: true));
                    break;

                case JsonToken.EndObject:
                    emitter.Emit(new MappingEnd());
                    break;

                case JsonToken.EndArray:
                    emitter.Emit(new SequenceEnd());
                    break;

                case JsonToken.EndConstructor:
                case JsonToken.Bytes:
                    throw new NotSupportedException(reader.TokenType.ToString());
                    break;

            }
        }
    }

    private void EmitPropertyNameToYaml(IEmitter emitter, string str, JToken token)
    {
        var style = ScalarStyle.Plain;

        if (token.IsDescendantOfAny(2*2, "ConfigSchema", "AliasTokenNames", "DynamicTokens", "Changes"))
        {
            style = ScalarStyle.ForcePlain;
        }

        if (token.IsDescendantOfAny(1*2, "When", "Fields", "Entries"))
        {
            style = ScalarStyle.DoubleQuoted;
        }

        emitter.Emit(new Scalar(AnchorName.Empty, FailsafeSchema.Tags.Str, str, 
            style, isPlainImplicit: true, isQuotedImplicit: true));
    }

    private void EmitStringToYaml(IEmitter emitter, string str, JToken token)
    {
        if (GuessIsEventString(str))
        {
            EmitEventStringToYaml(emitter, str);
            return;
        }

        ScalarStyle style = ScalarStyle.Any;
        Console.WriteLine($"path: {token.Path}");

        if (str.Length > emitterSettings.BestWidth && str.Contains(' '))
        {
            style = ScalarStyle.Folded;
            if (!str.EndsWith('\n')) str += "\n";
        }
        else if (string.IsNullOrWhiteSpace(str) 
            || str.Any(@"#[]{}\""`':".Contains)
            || str.StartsWith(@"&*!|>\%@"))
        {
            style = ScalarStyle.DoubleQuoted;
        }
        else
        {
            if (token.IsDescendantOfAny(2 * 2 + 1, "ConfigSchema", "AliasTokenNames"))
                style = ScalarStyle.Plain;

            if (token.IsDescendantOfAny("Entries", "MoveEntries", "TargetField"))
                style = ScalarStyle.DoubleQuoted;

            if (token.IsDescendantOfAny(1 * 2 + 1, "FromArea", "ToArea"))
                style = ScalarStyle.Plain;

            if (token.IsDescendantOfAny(1 * 2 + 1, "When", "SetProperties", "MapProperties"))
                style = ScalarStyle.DoubleQuoted;

            if (token.IsDescendantOfAny(0 * 2 + 1, "Type"))
                style = ScalarStyle.Plain;

            if (token.IsDescendantOfAny(0 * 2 + 1, JTokenType.Array))
                style = ScalarStyle.DoubleQuoted;

            if (token.IsDescendantOfAny(0 * 2 + 1, "FromFile"))
                style = ScalarStyle.DoubleQuoted;
        }

        emitter.Emit(new Scalar(AnchorName.Empty, FailsafeSchema.Tags.Str, str ?? "",
            style, isPlainImplicit: true, isQuotedImplicit: true));

        if (false)
        {
            if (token.IsDescendantOfAny(2 * 2 + 1, "ConfigSchema", "AliasTokenNames"))
                emitter.Emit(new Comment($"Descendant of {"ConfigSchema"}", true));

            if (token.IsDescendantOfAny("Entries", "MoveEntries", "TargetField"))
                emitter.Emit(new Comment($"Descendant of {"Entries"}", true));

            if (token.IsDescendantOfAny(1 * 2 + 1, "FromArea", "ToArea"))
                emitter.Emit(new Comment($"Descendant of {"FromArea"}", true));

            if (token.IsDescendantOfAny(1 * 2 + 1, "When", "SetProperties", "MapProperties"))
                emitter.Emit(new Comment($"Descendant of {"When"}", true));

            if (token.IsDescendantOfAny(0 * 2 + 1, "Type"))
                emitter.Emit(new Comment($"Descendant of {"Type"}", true));

            if (token.IsDescendantOfAny(0 * 2 + 1, JTokenType.Array))
            {
                emitter.Emit(new Comment($"Descendant of {JTokenType.Array}", true));
                emitter.Emit(new Comment($"TokenType was {token.Type}", false));
                emitter.Emit(new Comment($"Parent TokenType was {token.Parent.Type}", false));
            }

            if (token.IsDescendantOfAny(0 * 2 + 1, "FromFile"))
                emitter.Emit(new Comment($"Descendant of {"FromFile"}", true));
        }
    }

    private bool GuessIsEventString(string str)
    {
        return str.Contains("/viewport")
            || str.Contains("/faceDirection")
            || str.Contains("/move farmer")
            || str.Contains("/pause");
    }

    private void EmitEventStringToYaml(IEmitter emitter, string str)
    {
        var style = ScalarStyle.Folded;

        if (!str.Contains('\n'))
            str = string.Join("/", str.Split('/'));
        if (!str.EndsWith('\n'))
            str += "\n";

        emitter.Emit(new Scalar(AnchorName.Empty, FailsafeSchema.Tags.Str, str));
    }
}

file static class JTokenExtensions
{
    public static bool TryGetLineNumber(this JToken? token, out int lineNumber)
    {
        var lineInfo = token as IJsonLineInfo;
        if (lineInfo?.HasLineInfo() ?? false)
        {
            lineNumber = lineInfo.LineNumber;
            return true;
        }
        lineNumber = default;
        return false;
    }

    public static int? GetLineNumber(this JToken? token)
    {
        var lineInfo = token as IJsonLineInfo;
        if (lineInfo?.HasLineInfo() ?? false)
        {
            return lineInfo.LineNumber;
        }
        return null;
    }

    public static int GetDepth(this JToken token)
    {
        return token.Ancestors().Count();
    }

    public static bool IsDescendantOf(this JToken token, in JTokenMatcher match, int maxDepth = int.MaxValue)
    {
        while ((token = token.Parent) != null && maxDepth-- > 0)
        {
            if (token == match)
                return true;
        }
        return false;
    }

    public static bool IsDescendantOfAny(this JToken token, params ReadOnlySpan<JTokenMatcher> matches)
    {
        return IsDescendantOfAny(token, matches, int.MaxValue);
    }
    internal static bool IsDescendantOfAny(this JToken token, int maxDepth, params ReadOnlySpan<JTokenMatcher> matches)
    {
        return IsDescendantOfAny(token, matches, maxDepth);
    }
    public static bool IsDescendantOfAny(this JToken token, in ReadOnlySpan<JTokenMatcher> matches, int maxDepth)
    {
        while ((token = token.Parent) != null && maxDepth-- > 0)
        {
            foreach (var match in matches)
            {
                if (token == match) return true;
            }
        }
        return false;
    }
    public static bool IsDescendantOfAny(this JToken token, IEnumerable<JTokenMatcher> matches, int maxDepth = int.MaxValue)
    {
        while ((token = token.Parent) != null && maxDepth-- > 0)
        {
            foreach (var match in matches)
            {
                if (match.Equals(token)) return true;
            }
        }
        return false;
    }
}

file readonly struct JTokenMatcher : IEquatable<JToken>
{
    private readonly MatchType matchType;
    private readonly JToken? reference;
    private readonly string? name;
    private readonly JTokenType type;

    public bool Equals(JToken? token)
    {
        if (token == null) return false;

        return matchType switch
        {
            MatchType.None => false,
            MatchType.Reference => token == reference,
            MatchType.Name => token is JProperty jproperty
                                && string.Equals(jproperty.Name, name, StringComparison.OrdinalIgnoreCase),
            MatchType.Type => token.Type == type,
            _ => false,
        };
    }

    public JTokenMatcher(JToken reference)
    {
        matchType = MatchType.Reference;
        this.reference = reference;
    }
    public JTokenMatcher(string name)
    {
        matchType = MatchType.Name;
        this.name = name;
    }
    public JTokenMatcher(JTokenType type)
    {
        matchType = MatchType.Type;
        this.type = type;
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is JToken token)
            return Equals(token);
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(matchType, reference, name, type);
    }
    public static implicit operator JTokenMatcher(JToken reference) => new(reference);
    public static implicit operator JTokenMatcher(string name) => new(name);
    public static implicit operator JTokenMatcher(JTokenType type) => new(type);
    public static bool operator ==(JTokenMatcher matcher, JToken token) => matcher.Equals(token);
    public static bool operator !=(JTokenMatcher matcher, JToken token) => !matcher.Equals(token);
    public static bool operator ==(JToken token, JTokenMatcher matcher) => matcher.Equals(token);
    public static bool operator !=(JToken token, JTokenMatcher matcher) => !matcher.Equals(token);
    private enum MatchType
    {
        None,
        Reference,
        Name,
        Type
    }
}
