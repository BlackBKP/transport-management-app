namespace TMS.Services
{
    public class ApplicationStorage
    {
        public string DataDirectory { get; }

        public string KeysDirectory
        {
            get { return Path.Combine(DataDirectory, "keys"); }
        }

        public ApplicationStorage(IWebHostEnvironment environment, IConfiguration configuration)
        {
            string? configuredDirectory = configuration["Tms:DataDirectory"];

            DataDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
                ? Path.Combine(environment.ContentRootPath, "App_Data")
                : Path.GetFullPath(configuredDirectory, environment.ContentRootPath);

            Directory.CreateDirectory(DataDirectory);
        }
    }
}
