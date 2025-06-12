using Microsoft.Build.Framework;
using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Utilities;
using System.Linq;
using System;
using XH.Framework.Deployment.XT.ConfigurationTypes;

namespace XT.REST.Deployment.Tasks

{
    public class FileExtractor : Microsoft.Build.Utilities.Task
    {
        public override bool Execute()
        {
            var success = false;
            Log.LogMessage(MessageImportance.High, "Extracting File.");
            try
            {
                var workingDirectory = Path.Combine(Shared.ConfigurationManager.Profile.WorkingDirectory, "Files");
                var files = Shared.ConfigurationManager.Interface.Files;

                var fileItems = files.Select(file =>
                {
                    Log.LogMessage(MessageImportance.High, $"source:{file.Source}, destination:{file.Destination}");
                    var item = new TaskItem(file.Source);
                    item.SetMetadata("Source", Path.Combine(workingDirectory, Path.GetFileName(file.Source)));
                    item.SetMetadata("Destination", file.Destination);
                    return item;
                }).ToArray();

                ExtractedFiles = fileItems;
                success = true;
            }
            finally
            {
                Shared.LogResult(success, Log);
            }
            return true;
        }

        [Output]
        public ITaskItem[] ExtractedFiles { get; private set; } = new TaskItem[0];

    }
}
