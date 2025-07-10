using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class DependencyProcessHelperFixture
	{
		[Test]
		public void GetDependencyProcessInfo()
		{
			var sourceData1 = new SourceData
			{
				SDA_SubSource = "dep",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var sourceData2 = new SourceData
			{
				SDA_SubSource = "dep",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetERRStatus()
			};
			var sourceData3 = new SourceData
			{
				SDA_SubSource = "dep",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var sourceData4 = new SourceData
			{
				SDA_SubSource = "dep",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var sourceData5 = new SourceData
			{
				SDA_SubSource = "dep",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetDUPStatus()
			};
			var dependency = new Dependency("dep", new DateTime(2017, 1, 2));

			var repo = new Mock<IStagingRepository>();
			using (var stagingDataProvider = new StagingDataProvider(repo.Object))
			{
				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { sourceData1 }.AsQueryable());
				var processInfo = DependencyProcessHelper.GetDependencyProcessInfo(stagingDataProvider, dependency);
				Assert.That(processInfo.Status, Is.EqualTo(ProcessStatus.Missing));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { sourceData1, sourceData2 }.AsQueryable());
				processInfo = DependencyProcessHelper.GetDependencyProcessInfo(stagingDataProvider, dependency);
				Assert.That(processInfo.Status, Is.EqualTo(ProcessStatus.Error));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { sourceData1, sourceData2, sourceData3 }.AsQueryable());
				processInfo = DependencyProcessHelper.GetDependencyProcessInfo(stagingDataProvider, dependency);
				Assert.That(processInfo.Status, Is.EqualTo(ProcessStatus.Merged));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { sourceData1, sourceData2, sourceData3, sourceData5 }.AsQueryable());
				processInfo = DependencyProcessHelper.GetDependencyProcessInfo(stagingDataProvider, dependency);
				Assert.That(processInfo.Status, Is.EqualTo(ProcessStatus.Merged));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { sourceData1, sourceData2, sourceData4 }.AsQueryable());
				processInfo = DependencyProcessHelper.GetDependencyProcessInfo(stagingDataProvider, dependency);
				Assert.That(processInfo.Status, Is.EqualTo(ProcessStatus.NotMerged));
			}
		}

		[Test]
		public void GetDependencyProcessResult()
		{
			var dep1SourceData1 = new SourceData
			{
				SDA_SubSource = "dep1",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var dep1SourceData2 = new SourceData
			{
				SDA_SubSource = "dep1",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetERRStatus()
			};
			var dep1SourceData3 = new SourceData
			{
				SDA_SubSource = "dep1",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var dep1SourceData4 = new SourceData
			{
				SDA_SubSource = "dep1",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var dep1SourceData5 = new SourceData
			{
				SDA_SubSource = "dep1",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetDUPStatus()
			};
			var dep2SourceData1 = new SourceData
			{
				SDA_SubSource = "dep2",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 1),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var dep2SourceData2 = new SourceData
			{
				SDA_SubSource = "dep2",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetERRStatus()
			};
			var dep2SourceData3 = new SourceData
			{
				SDA_SubSource = "dep2",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetMERStatus()
			};
			var dep2SourceData4 = new SourceData
			{
				SDA_SubSource = "dep2",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetPRSStatus()
			};
			var dep2SourceData5 = new SourceData
			{
				SDA_SubSource = "dep2",
				SDA_ContentType = DataSourceConstants.ContentType.UniversalXML,
				SDA_SourceTime = new DateTime(2017, 1, 3),
				SDA_Status = StatusProvider.GetDUPStatus()
			};

			var dependency1 = new Dependency("dep1", new DateTime(2017, 1, 2));
			var dependency2 = new Dependency("dep2", new DateTime(2017, 1, 2), DependencyType.Required);

			var repo = new Mock<IStagingRepository>();
			using (var stagingDataProvider = new StagingDataProvider(repo.Object))
			{
				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep2SourceData1 }.AsQueryable());
				var processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep1SourceData2 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep1SourceData5 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep2SourceData1, dep2SourceData2 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], false);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency2));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.WaitForDependency));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], true);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency2));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ReportError));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep1SourceData2, dep1SourceData3 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep1SourceData2, dep1SourceData3, dep1SourceData5 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep2SourceData1, dep2SourceData2, dep2SourceData3 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep2SourceData1, dep2SourceData2, dep2SourceData3, dep2SourceData5 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], false);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.Null);
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.ProcessPrimaryData));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep1SourceData1, dep1SourceData2, dep1SourceData4 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], false);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency1));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.WaitForDependency));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency1], true);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency1));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.WaitForDependency));

				repo.Setup(x => x.Get<SourceData>()).Returns(new SourceData[] { dep2SourceData1, dep2SourceData2, dep2SourceData4 }.AsQueryable());
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], false);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency2));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.WaitForDependency));
				processResult = DependencyProcessHelper.GetDependencyProcessResult(stagingDataProvider, [dependency2], true);
				Assert.That(processResult.Dependency, Is.EqualTo(dependency2));
				Assert.That(processResult.Action, Is.EqualTo(ProcessAction.WaitForDependency));

			}
		}
	}
}
