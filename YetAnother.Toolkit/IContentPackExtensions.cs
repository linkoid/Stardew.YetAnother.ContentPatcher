using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Utilities;
using StardewModdingAPI.Toolkit.Utilities.PathLookups;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linkoid.Stardew.YetAnother.Toolkit.Serialization;

namespace Linkoid.Stardew.YetAnother.Toolkit;

public static class IContentPackExtensions
{
    private static readonly YamlHelper YamlHelper = new YamlHelper();

    public static TModel? ReadYamlFile<TModel>(this IContentPack contentPack, string path)
        where TModel : class
    {
		ContentPack @this = contentPack as ContentPack ?? throw new ArgumentException("The given IContentPack is not an instance of ContentPack", nameof(contentPack));

		path = PathUtilities.NormalizePath(path);

        FileInfo file = @this.GetFile(path);
        return file.Exists && YamlHelper.ReadYamlFileIfExists(file.FullName, out TModel? model)
            ? model
            : null;
    }
}
