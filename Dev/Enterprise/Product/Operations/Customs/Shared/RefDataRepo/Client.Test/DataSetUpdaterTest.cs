using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class DataSetUpdaterTest : TestCase
	{
		public void TestReportWhenNoTimestampRecord()
		{
			var serverStreamProxyMock = new Mock<IServerStreamProxy>();
			var proxyMockNew = new Mock<IServerProxy>();
			proxyMockNew.Setup(x => x.CreateHttpClient()).Returns(new HttpClientWrapper(new HttpClient()));
			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns<RefDbVersionControl>(null);

			var updater = new DummyDataSetUpdater(proxyMockNew.Object, dbHelperMock.Object, versionControl.Object);

			Task.Run(() => updater.ReportAsync("any", "any", "any", true)).Wait();
			proxyMockNew.Verify(x => x.ReportAsync(nameof(Dummy), It.IsAny<IHttpClient>(), "any", "any", null, null, "any", true, It.IsAny<UpdaterDurationLog>()), Times.AtLeast(1));
			Assert(true);
		}

		public void TestSavePerBatch()
		{
			var serverStreamProxyMock = new Mock<IServerStreamProxy>();
			var dummyObj1 = new Dummy();
			dummyObj1.Checkpoint = "123456789";
			var dummyObj2 = new Dummy();
			serverStreamProxyMock.SetupSequence(x => x.Read<Dummy>())
				.Returns(Task.FromResult(dummyObj1))
				.Returns(Task.FromResult(dummyObj2))
				.Returns(Task.FromResult<Dummy>(null));

			var proxyMockNew = new Mock<IServerProxy>();
			proxyMockNew.Setup(x => x.OpenServerDataStream(nameof(Dummy), It.IsAny<IHttpClient>(), It.IsAny<DataSetGet>(), It.IsAny<StringBuilder>()))
				.Returns(Task.FromResult(serverStreamProxyMock.Object));

			var updater = new DummyDataSetUpdater(proxyMockNew.Object, dbHelperMock.Object, versionControl.Object);
			updater.TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(DummyStorage)) };
			updater.MergeText = "MERGE DummyStorage Now";
			updater.addServerData = (s, x) =>
			{
				s.ToArray()[0].Rows.Add(Guid.NewGuid(), string.Empty, 0, DateTime.Now);
			};

			dbHelperMock.Setup(x => x.GetReferencedForeignKeys()).Returns(new List<ForeignKeyRelationship>());
			var trans = new Mock<IDbTransaction>();
			Task.Run(() => updater.UpdateAsync(dummyDataSetVersion, null, null)).Wait();

			dbHelperMock.Verify(x => x.ExecuteNonQuery(It.Is<string>(y => y.Contains("MERGE DummyStorage Now")),
			It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<Func<IDbCommand, IDbDataParameter>[]>()), Times.Exactly(2));
			Assert(true);
		}

		public void TestCleanupTemporaryTables()
		{
			var dbHelper = new Mock<IDBHelper>();
			var trans = new Mock<IDbTransaction>();
			dbHelper.Setup(x => x.BeginTransaction()).Returns(trans.Object);
			var updater = new DummyDataSetUpdater(new Mock<IServerProxy>().Object, dbHelper.Object, new Mock<IRefVersionControlManager>().Object);
			updater.TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(string)), Tuple.Create(typeof(IDummyDependentStorage), typeof(string)) };
			updater.CleanupTemporaryTables_Exposed(DataSetName);
			dbHelper.Verify(x => x.DeleteTemporaryTable<IDummyStorage>(DataSetName, trans.Object));
			dbHelper.Verify(x => x.DeleteTemporaryTable<IDummyDependentStorage>(DataSetName, trans.Object));
			Assert(true);
		}

		public void TestResumeDataSetGet()
		{
			((DummyDataSetUpdater)updater).TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(string)) };
			dbHelperMock.Setup(x => x.GetReferencedForeignKeys()).Returns(new List<ForeignKeyRelationship>());
			var dataSetGet = new DataSetGet(null, dummyDataSetVersion.Timestamp, "1.0", DataSetName);
			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, JsonSerializer.Serialize(dataSetGet), 1));
			Task.Run(() => updater.ResumeAsync(dummyDataSetVersion, null)).Wait();
			proxyMock.Verify(x => x.OpenServerDataStream(nameof(Dummy), It.IsAny<IHttpClient>(), It.IsAny<DataSetGet>(), It.IsAny<StringBuilder>()));
			dbHelperMock.Verify(x => x.DeleteTemporaryTable<IDummyStorage>(DataSetName, It.IsAny<IDbTransaction>()));
			Assert(true);
		}

		public void TestCleanResumeData()
		{
			((DummyDataSetUpdater)updater).TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(string)) };
			var dataSetGet = new DataSetGet(null, DateTime.UtcNow, "1.0", "notnull");
			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, JsonSerializer.Serialize(dataSetGet), 1));
			Task.Run(() => updater.ResumeAsync(dummyDataSetVersion, null)).Wait();
			Assert(true);
		}

		public void TestUpdate()
		{
			((DummyDataSetUpdater)updater).TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(string)) };
			dbHelperMock.Setup(x => x.GetReferencedForeignKeys()).Returns(new List<ForeignKeyRelationship>());
			var trans = new Mock<IDbTransaction>();
			dbHelperMock.Setup(x => x.BeginTransaction()).Returns(trans.Object);
			Task.Run(() => updater.UpdateAsync(dummyDataSetVersion, null, null)).Wait();
			proxyMock.Verify(x => x.OpenServerDataStream(nameof(Dummy), It.IsAny<IHttpClient>(), It.Is<DataSetGet>(p => p.UpperTimestamp == dummyDataSetVersion.Timestamp), It.IsAny<StringBuilder>()));
			dbHelperMock.Verify(x => x.CreateTemporaryTable<IDummyStorage>(DataSetName, It.IsAny<IEnumerable<Tuple<string, string>>>(), It.IsAny<IDbTransaction>()));
			dbHelperMock.Verify(x => x.DeleteTemporaryTable<IDummyStorage>(DataSetName, trans.Object), Times.Exactly(1));
			Assert(true);
		}

		public void TestResetTimestampIfNeeded()
		{
			versionControl.Setup(x => x.ResetLastUpdatedUTC(DataSetName, It.IsAny<IDbTransaction>()));
			updater.ResetTimestampIfNeeded(DataSetName);
			versionControl.Verify(x => x.ResetLastUpdatedUTC(DataSetName, It.IsAny<IDbTransaction>()), Times.Never);

			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, string.Empty, 3));
			((DummyDataSetUpdater)updater).dummyVersion = 2;
			updater.ResetTimestampIfNeeded(DataSetName);
			versionControl.Verify(x => x.ResetLastUpdatedUTC(DataSetName, It.IsAny<IDbTransaction>()), Times.Never);

			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, string.Empty, 1));
			updater.ResetTimestampIfNeeded(DataSetName);
			versionControl.Verify(x => x.ResetLastUpdatedUTC(DataSetName, It.IsAny<IDbTransaction>()), Times.Once);
			Assert(true);
		}

		public void TestShouldRun()
		{
			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, string.Empty, 1));
			((DummyDataSetUpdater)updater).dummyVersion = 2;
			AssertEquals(true, ((DummyDataSetUpdater)updater).ShouldRun(DataSetName));

			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, string.Empty, 2));
			AssertEquals(true, ((DummyDataSetUpdater)updater).ShouldRun(DataSetName));

			versionControl.Setup(x => x.GetVersionControl(It.IsAny<string>(), It.IsAny<IDbTransaction>())).Returns(new RefDbVersionControl(DataSetName, null, string.Empty, 3));
			AssertEquals(false, ((DummyDataSetUpdater)updater).ShouldRun(DataSetName));
		}

		public void TestDataSetGetSavedCorrectly()
		{
			List<DataSetGet> dataSetGetsSaved = new List<DataSetGet>();
			versionControl.Setup(x => x.SaveVersionControl(
				It.IsAny<IDbTransaction>(),
				It.IsAny<RefDbVersionControl>(),
				It.IsAny<RefDbVersionControl>()
				)).Callback((IDbTransaction tran, RefDbVersionControl versionControl, RefDbVersionControl oldVersionControl) => dataSetGetsSaved.Add(versionControl.DataSetGet));

			var serverStreamProxyMock = new Mock<IServerStreamProxy>();
			var dummyObj1 = new Dummy();
			dummyObj1.Checkpoint = "123456789";
			var dummyObj2 = new Dummy();
			serverStreamProxyMock.SetupSequence(x => x.Read<Dummy>())
				.Returns(Task.FromResult(dummyObj1))
				.Returns(Task.FromResult(dummyObj2))
				.Returns(Task.FromResult<Dummy>(null));

			var proxyMockNew = new Mock<IServerProxy>();
			proxyMockNew.Setup(x => x.OpenServerDataStream(nameof(Dummy), It.IsAny<IHttpClient>(), It.IsAny<DataSetGet>(), It.IsAny<StringBuilder>()))
				.Returns(Task.FromResult(serverStreamProxyMock.Object));

			var updaterNew = new DummyDataSetUpdater(proxyMockNew.Object, dbHelperMock.Object, versionControl.Object);
			updaterNew.TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(DummyStorage)) };
			updaterNew.addServerData = (s, x) =>
			{
				s.ToArray()[0].Rows.Add(Guid.NewGuid(), string.Empty, 0, DateTime.Now);
			};

			((DummyDataSetUpdater)updater).TypesByInsertOrder = new[] { Tuple.Create(typeof(IDummyStorage), typeof(Dummy)) };
			dbHelperMock.Setup(x => x.GetReferencedForeignKeys()).Returns(new List<ForeignKeyRelationship>());
			var trans = new Mock<IDbTransaction>();
			Task.Run(() => updaterNew.UpdateAsync(dummyDataSetVersion, null, null)).Wait();

			AssertEquals(2, dataSetGetsSaved.Count);
			AssertEquals("123456789", dataSetGetsSaved[0].Checkpoint);
			AssertEquals(DataSetName, dataSetGetsSaved[0].DataSet);
			AssertEquals(null, dataSetGetsSaved[1]);
		}

		Mock<IDBHelper> dbHelperMock;
		Mock<IRefVersionControlManager> versionControl;
		Mock<IServerProxy> proxyMock;
		IDataSetUpdater updater;
		const string DataSetName = "DummyDataSet";
		readonly DataSetVersion dummyDataSetVersion = new DataSetVersion(DataSetName, DateTime.UtcNow);

		protected override void SetUp()
		{
			base.SetUp();
			proxyMock = new Mock<IServerProxy>();
			dbHelperMock = new Mock<IDBHelper>();
			dbHelperMock.Setup(x => x.BeginTransaction()).Returns(new Mock<IDbTransaction>().Object);
			versionControl = new Mock<IRefVersionControlManager>();
			proxyMock.Setup(x => x.OpenServerDataStream(nameof(Dummy), It.IsAny<IHttpClient>(), It.IsAny<DataSetGet>(), It.IsAny<StringBuilder>()))
				.Returns(Task.FromResult(new Mock<IServerStreamProxy>().Object));
			updater = new DummyDataSetUpdater(proxyMock.Object, dbHelperMock.Object, versionControl.Object);
		}
	}
}
