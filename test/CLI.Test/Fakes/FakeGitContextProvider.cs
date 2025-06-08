using Core.Services;

namespace CLI.Test.Fakes
{
    public class FakeGitContextProvider(string fakeRepoPath, bool shouldSucceed = true, string? workingDirectory = null) : IGitContextProvider
    {
        public string GetWorkingDirectory()
        {
            return workingDirectory ?? Directory.GetCurrentDirectory();
        }

        public bool TryGetRepositoryRoot(out string root)
        {
            if (shouldSucceed)
            {
                root = fakeRepoPath;
                return true;
            }

            root = string.Empty;
            return false;
        }
    }
}
