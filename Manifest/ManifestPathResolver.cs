using CtxSignTool.Routing;

namespace CtxSignTool.Manifest;
/// <summary>
/// 
/// </summary>
public static class ManifestPathResolver
{
    /// <summary>
    /// Resolves the path to the manifest file based on the specified command context and root directory.
    /// </summary>
    /// <remarks>The method prioritizes user-specified parameters in the command context to determine the
    /// manifest path. If multiple parameters are present, the order of precedence is: 'manifest', 'in', 'name', and
    /// 'out'. If none are provided, the default manifest file 'cmsmanifest.json' in the root directory is
    /// used.</remarks>
    /// <param name="context">The command context containing parameters that influence how the manifest path is determined. Cannot be null.</param>
    /// <param name="root">The root directory used as a base for constructing the manifest path when a relative path or default is
    /// required. Cannot be null or empty. Use "." for base path</param>
    /// <returns>A string representing the resolved path to the manifest file. If no specific manifest path is provided in the
    /// context, returns 'cmsmanifest.json' located in the root directory.</returns>
    public static string ResolveManifestPath(CommandContext context, string root)
    {
        string manifestArg = context.Get("manifest", string.Empty);
        if (!Null(manifestArg) && !string.Equals(manifestArg, "true", StringComparison.OrdinalIgnoreCase))
            return manifestArg;

        string input = context.Get("in", string.Empty);
        if (!Null(input) && string.Equals(Path.GetExtension(input), ".json", StringComparison.OrdinalIgnoreCase))
            return input;

        string name = context.Get("name", string.Empty);
        if (!Null(name))
            return Path.IsPathRooted(name) ? name : Path.Combine(root, name);

        string outPath = context.Get("out", string.Empty);
        if (!Null(outPath) && string.Equals(Path.GetExtension(outPath), ".json", StringComparison.OrdinalIgnoreCase) && Null(context.Get("sig", string.Empty)))
            return outPath;

        return Path.Combine(root, "cmsmanifest.json");
    }
    /// <summary>
    /// Resolves the appropriate signature file path for a manifest based on the provided command context.
    /// </summary>
    /// <remarks>If a signature path is specified in the context, it is returned. If an output path is
    /// provided and does not have a '.json' extension, that path is used. Otherwise, the method appends '.sig' to the
    /// manifest path to form the signature file path.</remarks>
    /// <param name="context">The command context containing potential signature and output path values used to determine the signature file
    /// location.</param>
    /// <param name="manifestPath">The file path of the manifest, used as the base for constructing the signature file path if no explicit path is
    /// specified in the context.</param>
    /// <returns>A string representing the resolved signature file path. This may be a path specified in the context, an output
    /// path, or the manifest path with a '.sig' extension appended.</returns>
    public static string ResolveManifestSignaturePath(CommandContext context, string manifestPath)
    {
        string sigPath = context.Get("sig", string.Empty);
        if (!Null(sigPath))
            return sigPath;

        string outPath = context.Get("out", string.Empty);
        if (!Null(outPath) && !string.Equals(Path.GetExtension(outPath), ".json", StringComparison.OrdinalIgnoreCase))
            return outPath;

        return manifestPath + ".sig";
    }
}
