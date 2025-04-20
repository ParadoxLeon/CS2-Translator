using System;
using System.Net.Http;
using System.Threading.Tasks;

public static class Version
{
    public const string CurrentVersion = "1.4.0";
}

public class VersionChecker
{
    private const string GitHubApiUrl = "https://api.github.com/repos/ParadoxLeon/CS2-Translator/releases/latest";

    public async Task<bool> CheckForUpdates()
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "CS2-Translator");

            HttpResponseMessage response = await httpClient.GetAsync(GitHubApiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                // extract the latest version.
                string latestVersion = GetJsonValue(json, "tag_name");

                if (IsNewerVersionAvailable(latestVersion))
                {
                    // Return true to indicate that a newer version is available
                    return true;
                }
            }

            // Return false to indicate that no newer version is available or if there was an error
            return false;
        }
    }

    private bool IsNewerVersionAvailable(string latestVersion)
    {
        var currentParts = Version.CurrentVersion.Split('.');
        var latestParts = latestVersion.Split('.');

        for (int i = 0; i < currentParts.Length; i++)
        {
            int current = int.Parse(currentParts[i]);
            int latest = int.Parse(latestParts[i]);

            if (current < latest)
            {
                return true; // A newer version is available
            }
            else if (current > latest)
            {
                return false; // Current version is newer
            }
        }

        // If we reach this point, the versions are equal
        return false;
    }

    private string GetJsonValue(string json, string key)
    {
        int index = json.IndexOf($"\"{key}\":", StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            int startIndex = json.IndexOf("\"", index + key.Length + 3) + 1;
            int endIndex = json.IndexOf("\"", startIndex);

            if (startIndex != -1 && endIndex != -1)
            {
                return json.Substring(startIndex, endIndex - startIndex);
            }
        }

        return null;
    }
}
