using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test
{
	[TestFixture]
	class ScriptClassGeneratorFixture
	{
		[Test]
		public void UpdaterInfosAreLatest()
		{
			var binPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
			var assembly = Assembly.LoadFrom(Path.Combine(binPath, @"..\Server\net8.0\CargoWise.RefDbRepo.RemoteDbManager.dll"));
			var updaterInfoClasses = assembly.GetTypes().Select(x => x.Name);

			var updaterInfos = UpdaterInfoProvider.GetUpdaterInfos;
			foreach (var info in updaterInfos)
			{
				var storageType = info.Item1.GetStorageType().Name.Substring(1);
				var storageTypeName = $"{storageType}UpdaterInfo_{info.Item2}";
				var storageTypeName_HigherVersion = $"{storageType}UpdaterInfo_{info.Item2 + 1}";
				Assert.True(updaterInfoClasses.Contains(storageTypeName), $"{storageTypeName} should exist.");
				Assert.False(updaterInfoClasses.Contains(storageTypeName_HigherVersion), $"{storageTypeName_HigherVersion} should not exist. Please update UpdaterInfoProvider.cs and use this tool to generate latest updaterinfo if you want to add a new version.");
			}
		}

		[Test]
		public void GenerateScriptcontent()
		{
			var scriptGen = new Mock<IScriptGenerator>();
			var generator = new ScriptClassGenerator(scriptGen.Object, new Mock<ISchemaInfo>().Object, new Mock<ISQLBuilder>().Object, new Mock<ICustomScriptProvider>().Object);
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetStorageType()).Returns(typeof(IRefAccTaxRate));
			scriptGen.Setup(x => x.GeneratePrepareTemporaryTablesScripts(info.Object)).Returns(new[] { ("AA", "BB"), ("CC", "DD") });
			scriptGen.Setup(x => x.GenerateMergeScript(info.Object)).Returns("Hello");
			scriptGen.Setup(x => x.GetPrerequisites(info.Object)).Returns(new[] { nameof(IDummy1) });

			var result = generator.GenerateScriptContent(info.Object, 1);
			StringAssert.Contains(@"using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefAccTaxRateUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public string DataSetName => ""RefAccTaxRate"";

		public Dictionary<string, string> PrepareTemporaryTablesScripts => new Dictionary<string, string>
		{
{ ""AA"", @""BB"" },
{ ""CC"", @""DD"" }
		};

		public string MergeScript => @""
Hello
"";

		public string[] Prerequisites => new [] { ""Dummy1"" };
		public int UpdaterVersion => 1;
	}
}", result);
		}

		[Test]
		public void GenerateScriptcontentWithOverriddenPrepareTemporaryTablesScripts()
		{
			var scriptGen = new Mock<IScriptGenerator>();
			var generator = new ScriptClassGenerator(scriptGen.Object, new Mock<ISchemaInfo>().Object, new Mock<ISQLBuilder>().Object, new Mock<ICustomScriptProvider>().Object);
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetStorageType()).Returns(typeof(IRefAccTaxRate));
			info.Setup(x => x.GetOverriddenPrepareTemporaryTablesScripts()).Returns("I AM NOT EMPTY");
			scriptGen.Setup(x => x.GeneratePrepareTemporaryTablesScripts(info.Object)).Returns(new[] { ("AA", "BB") });
			scriptGen.Setup(x => x.GenerateMergeScript(info.Object)).Returns("Hello");

			var result = generator.GenerateScriptContent(info.Object, 1);
			StringAssert.Contains(@"using System.Collections.Generic;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public partial class RefAccTaxRateUpdaterInfo_1 : IUpdaterScriptInfo
	{
		public string DataSetName => ""RefAccTaxRate"";

		public Dictionary<string, string> PrepareTemporaryTablesScripts => new Dictionary<string, string>
		{
I AM NOT EMPTY
		};

		public string MergeScript => @""
Hello
"";

		public string[] Prerequisites => new string[0];
		public int UpdaterVersion => 1;
	}
}", result);
		}

		[Test]
		public void WriteTo()
		{
			var scriptGen = new Mock<IScriptGenerator>();
			var generator = new ScriptClassGenerator(scriptGen.Object, new Mock<ISchemaInfo>().Object, new Mock<ISQLBuilder>().Object, new Mock<ICustomScriptProvider>().Object);
			var info = new Mock<IDataSetUpdaterInfo>();
			info.Setup(x => x.GetStorageType()).Returns(typeof(IRefAccTaxRate));
			var path = Path.GetTempPath();
			var content = "ABC\r\n123";
			ScriptClassGenerator.WriteTo(info.Object, 1, content, path);
			var filePath = Path.Combine(path, @"RefAccTaxRateUpdaterInfo_1.cs");
			File.Exists(filePath);
			Assert.AreEqual(content, File.ReadAllText(filePath));
		}
	}
}
