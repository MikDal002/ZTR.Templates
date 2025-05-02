using Nuke.Common;
using Serilog;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ZtrTemplates.Configuration.Shared;

public partial class Build
{
    [Parameter("Base URL for Velopack updates served via the website")]
    readonly string UpdateBaseUrl = "https://frog02-20366.wykr.es";

    Target ConfigureAppSettings => _ => _
        .DependsOn(Clean) 
        .TriggeredBy(Clean)
        .Unlisted() 
        .Executes(() =>
        {
            Log.Information("Configuring appsettings.json for deployment...");

            var updateUrl = $"{UpdateBaseUrl}/{NameProjectDirectoryOnTheServer}/{DirectoryForReleases}";

            var appSettingsPath = ProjectToPublish.Directory / "appsettings.json";

            JsonNode? rootNode;

            if (File.Exists(appSettingsPath))
            {
                using var fileStream = File.OpenRead(appSettingsPath);
                try
                {
                    rootNode = JsonNode.Parse(fileStream);
                    Log.Information($"Read existing settings from '{appSettingsPath}'.");
                }
                catch (JsonException ex)
                {
                    Log.Warning($"Failed to parse existing appsettings.json at '{appSettingsPath}'. A new file will be created. Error: {ex.Message}");
                    rootNode = null;
                }
            }
            else
            {
                Log.Information($"File '{appSettingsPath}' not found. Creating a new settings file.");
                rootNode = null;
            }


            rootNode ??= new JsonObject();


            if (rootNode[nameof(UpdateOptions)] is not JsonObject updateOptionsNode)
            {
                updateOptionsNode = new JsonObject();
                rootNode[nameof(UpdateOptions)] = updateOptionsNode;
            }
            updateOptionsNode[nameof(UpdateOptions.UpdateUrl)] = updateUrl;


            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(appSettingsPath, rootNode.ToJsonString(jsonOptions));
            Log.Information($"Ensured '{nameof(UpdateOptions)}.{nameof(UpdateOptions.UpdateUrl)}' is set to '{updateUrl}' in '{appSettingsPath}'"); // Changed Log.Success to Log.Information
        });
}
