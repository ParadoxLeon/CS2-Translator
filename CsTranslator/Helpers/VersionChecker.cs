using System.Threading.Tasks;
using Octokit;

public static class Version
{
    public const string CurrentVersion = "1.2.3";
}
public class VersionChecker
{
    public async Task<bool> CheckForUpdates()
    {
        var owner = "ParadoxLeon";
        var repoName = "CS2-Translator";

        var client = new GitHubClient(new ProductHeaderValue("CS2-Translator"));
        var releases = await client.Repository.Release.GetAll(owner, repoName);

        if (releases.Count > 0)
        {
            var latestRelease = releases[0];
            string latestVersion = latestRelease.TagName;

            if (IsNewerVersionAvailable(latestVersion))
            {
                // Return true to indicate that a newer version is available
                return true;
            }
        }

        // Return false to indicate that no newer version is available
        return false;
    }

    private bool IsNewerVersionAvailable(string latestVersion)
    {
        // Split the versions into parts
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
}
