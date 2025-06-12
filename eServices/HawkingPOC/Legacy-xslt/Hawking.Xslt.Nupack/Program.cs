using System.IO;
using Unity;
using Hawking.Unity;
using Microsoft.Extensions.Configuration;
using Hawking.Xslt.Nupack.Builders;

namespace Hawking.Xslt.Nupack
{
    class Program
    {
        static void Main(string[] commandlineArgs)
        {
            var args = Args.Parse(commandlineArgs);
            var nupack = new PackageBuilder(args);

            ConfigUnity();

            nupack.Pack();
        }

        static void ConfigUnity()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = configurationBuilder.Build();
            DependencyFactory.Container.RegisterInstance<IConfigurationRoot>(config);
            DependencyFactory.Container.RegisterType<INuspecInfo, NuspecInfo>();
        }
    }
}
