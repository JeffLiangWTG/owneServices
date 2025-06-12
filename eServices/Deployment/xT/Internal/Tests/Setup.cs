// Please do CH0/CHS/CHP, if want to restore the tests.

//using System.IO;
//using System.Reflection;
//using NuGet;
//using NUnit.Framework;

//namespace XT.Internal.API.Tests
//{
//	[SetUpFixture]
//	public class Setup
//	{
//		readonly string path = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

//		[OneTimeSetUp]
//		public void AddRuntimeAssemblies()
//		{
//			var repository = PackageRepositoryFactory.Default.CreateRepository("https://proget.wtg.zone/nuget/WTG-Internal/v3");
//			var packageManager = new PackageManager(repository, path);
//			packageManager.PackageInstalled += PackageManager_PackageInstalled;
//			var package = repository.FindPackage("xT.Deployment", SemanticVersion.Parse("5.1.1.61"));
//			if (package != null)
//			{
//				packageManager.InstallPackage(package, false, true);
//			}
//		}

//		private void PackageManager_PackageInstalled(object sender, PackageOperationEventArgs e)
//		{
//			foreach (var file in Directory.GetFiles(e.InstallPath, "*.dll", SearchOption.AllDirectories))
//			{
//				var fileName = Path.GetFileName(file);
//				var target = Path.Combine(path, fileName);
//				if (!File.Exists(target))
//				{
//					File.Copy(file, target);
//				}
//			}
//		}
//	}
//}


