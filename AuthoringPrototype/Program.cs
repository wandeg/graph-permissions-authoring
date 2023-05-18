using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Kibali;

namespace AuthoringPrototype;

public class Program
{
    public static async Task Main(string[] args)
    {
        await MergeAndConvert(args);
    }

    private static async Task MergeAndConvert(string[] args)
    {
        var inputFile = args[0];
        var outputDir = args[1];
        using var reader = new StreamReader(inputFile);
        string json = reader.ReadToEnd();
        var input = JsonSerializer.Deserialize<AuthoringInput>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var convertedDocument = ConvertToExpectedOutput(input);
        await MergeGeneratedPermissionsWithExistingAsync(convertedDocument, outputDir);
    }

    private static async Task MergeGeneratedPermissionsWithExistingAsync(PermissionsDocument newDocument, string outputDir)
    {
        var groups = newDocument.Permissions.GroupBy(p => Prefix(p.Key, '.'));
        foreach (var group in groups)
        {
            // Merge path with output dir
            var outputPath = Path.Combine(outputDir, $"{group.Key}.json");
            var existingDocument = new PermissionsDocument();
            if (File.Exists(outputPath))
            {
                using var reader = new StreamReader(outputPath);
                string json = reader.ReadToEnd();
                existingDocument = JsonSerializer.Deserialize<PermissionsDocument>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            var mergedDocument = PermissionsMerger.MergePermissionsDocument(existingDocument, newDocument);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = { new JsonStringEnumConverter(), },
                WriteIndented = true,
                MaxDepth = 100,
            };
            var newDocumentOutput = JsonSerializer.Serialize<PermissionsDocument>(mergedDocument, options);
            File.WriteAllText(outputPath, newDocumentOutput.Replace(@"\u0027", "'").Replace(@"\u2019", "'").Replace(@"\u00A0", " "));
            using (var outStream = new FileStream(outputPath, FileMode.Create))
            {
                await mergedDocument.WriteAsync(outStream);
            }
        }
    }

    private static string Prefix(string key, char separator)
    {
        var index = key.IndexOf(separator);
        if (index == -1)
        {
            return key;
        }
        return key.Substring(0, index);
    }

    private static PermissionsDocument ConvertToExpectedOutput(AuthoringInput input)
    {
        var document = new PermissionsDocument() { Permissions = new() };
        if (input == null)
        {
            return document;
        }

        foreach (var entry in input.Paths)
        {
            var path = entry.Key;
            foreach (var permissionData in entry.Value)
            {
                var permissionNames = permissionData.Permissions;
                var methods = permissionData.Methods;
                foreach (var permissionName in permissionNames)
                {
                    if (!document.Permissions.TryGetValue(permissionName, out var permission))
                    {
                        permission = new Permission();
                        document.Permissions.Add(permissionName, permission);
                    }
                    foreach (var schemeType in permissionData.Schemes)
                    {
                        var newScheme = new Scheme();
                        if (!permission.Schemes.TryGetValue(schemeType, out var _))
                        {
                            permission.Schemes.Add(schemeType, newScheme);
                        }
                    }

                    var pathSet = GetOrCreatePathSet(permission, methods.ToHashSet(), permissionData.Schemes.ToHashSet());
                    pathSet.Paths.Add(path, string.Empty);
                }
            }
            
            
        }
        return document;
    }

    private static PathSet GetOrCreatePathSet(Permission permission, HashSet<string> methods, HashSet<string> schemes)
    {
        foreach (var pathSet in permission.PathSets)
        {
            if (pathSet.SchemeKeys.SetEquals(schemes) && pathSet.Methods.SetEquals(methods))
            {
                return pathSet;
            }
        }

        var newPathSet = new PathSet
        {
            Methods = methods,
            SchemeKeys = schemes,
        };

        permission.PathSets.Add(newPathSet);
        return newPathSet;
    }

    private static string Prefix(string path, params char[] delimiters)
    {
        var pathDelimiters = new[] { '/', '\\' };
        var index = path?.IndexOfAny(delimiters?.Length > 0 ? delimiters : pathDelimiters) ?? -1;
        return index >= 0 ? path[..index] : path;
    }
}