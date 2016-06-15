using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigrator.Core.Util;

namespace DbMigrator.ConsoleApp
{
    internal class FolderTrimmer
    {
        internal void TrimFolder(string folder, string filter)
        {
            filter = filter.ToLowerInvariant();
            var allFiles = System.IO.Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories);
            FlagFilter flagFilter = new FlagFilter(filter);
            System.Uri rootFolder = new Uri(folder);
            foreach (var filePath in allFiles)
            {
                System.Uri fileUri = new Uri(filePath);
                Uri relativeUri = rootFolder.MakeRelativeUri(fileUri);
                var parts = relativeUri.ToString().ToLowerInvariant().Split('/');
                var flags = parts.Skip(1).Take(parts.Length-2).ToArray();
                if (!flagFilter.Test(flags))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
