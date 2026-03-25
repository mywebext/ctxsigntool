using CtxSignTool.Contracts;
using CtxSignTool.Localization;
using CtxSignTool.Routing;

namespace CtxSignTool.Commands;
/// <summary>
/// Provides functionality to generate a manifest file based on the specified command context.
/// </summary>
/// <remarks>This class exposes methods for creating or updating a manifest file using parameters supplied via a
/// command context. It is intended for use in command-line scenarios where manifest generation is required. All members
/// are static and can be accessed without instantiating the class.</remarks>
public static class ManifestCommand
{
    /// <summary>
    /// Generates or updates a manifest file in the specified root directory based on the provided command context.
    /// </summary>
    /// <remarks>If the output manifest path is not specified, the method defaults to creating or updating
    /// 'cmsmanifest.json' in the root directory.</remarks>
    /// <param name="context">The context containing command-line arguments and options used to determine the root directory and output
    /// manifest path.</param>
    /// <returns>An integer status code indicating the result of the operation. Returns 0 if the manifest generation or update
    /// succeeds.</returns>
    /// <exception cref="CtxSignException">Thrown if the root directory is not specified in the context, indicating invalid usage.</exception>
    public static int Execute(CommandContext context)
    {
        string root = context.Get("root", context.Get("dir", string.Empty));
        
        if (Null(root))
        {
            string message = LanguageService.T("error.manifest.missingroot", "Missing --root <directory> for manifest generation.");
            throw new CtxSignException(ReturnCodes.InvalidUsage, HelpTarget.Manifest, message);
        }

        string outManifest = context.Get("out", string.Empty);
        if (Null(outManifest))
        {
            string name = context.Get("name", string.Empty);
            outManifest = !Null(name) ? Path.Combine(root, $"{name}.json") : Path.Combine(root, "cmsmanifest.json");
        }

        string W = LanguageService.T("word.wrote", "Wrote").ToUpper();
        string F = LanguageService.T("word.file", "File").ToUpper();
        int count = ManifestBuilder.BuildOrUpdate(root, outManifest);
        Console.WriteLine($"{W} : {outManifest}");
        Console.WriteLine($"{F} : {count}");
        return (int)ReturnCodes.Ok;
    }
}
