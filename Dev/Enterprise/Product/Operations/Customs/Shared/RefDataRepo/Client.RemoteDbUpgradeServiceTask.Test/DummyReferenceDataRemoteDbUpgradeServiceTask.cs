using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using Moq;

namespace CargoWise.RefDataRepo.Ent.Client.RemoteDbUpgradeServiceTask.Test
{
	public class DummyReferenceDataRemoteDbUpgradeServiceTask : ReferenceDataRemoteDbUpgradeServiceTask
	{
		public DummyReferenceDataRemoteDbUpgradeServiceTask(Enterprise.Integration.ILogger logger, IErrorReportingClientWrapper errorReportingWrapper)
			: this(new DummyServiceTaskRemoteDbUpgraderManager(logger, errorReportingWrapper) { UpdateResult = true })
		{ }

		public DummyReferenceDataRemoteDbUpgradeServiceTask(DummyServiceTaskRemoteDbUpgraderManager manager)
		{
			this.manager = manager;
		}
		readonly DummyServiceTaskRemoteDbUpgraderManager manager;

		protected override ServiceTaskRemoteDbUpgraderManager GetUpgraderManager()
		{
			return manager;
		}

		protected override SharedDataSetUpdaterWrapper GetUpdateManagerWrapper()
		{
			var result = new Mock<SharedDataSetUpdaterWrapper>();
			result.Setup(x => x.DbInitializationCheck()).Returns(false);
			result.SetupGet(x => x.SavedDataSetCount).Returns(() => SavedDataSetCount);
			return result.Object;
		}

		public bool IsUpdateCalled
		{
			get
			{
				return manager.IsUpdateCalled;
			}
			set
			{
				manager.IsUpdateCalled = value;
			}
		}

		public int SavedDataSetCount { get; set; }
	}

	public class DummyServiceTaskRemoteDbUpgraderManager : ServiceTaskRemoteDbUpgraderManager
	{
		public DummyServiceTaskRemoteDbUpgraderManager(Enterprise.Integration.ILogger logger, IErrorReportingClientWrapper errorReportingWrapper) : base(logger, errorReportingWrapper, new[] { Db.ServerName }) { }
		public bool UpdateResult { get; set; }

		public bool IsUpdateCalled { get; set; }
		public override bool Update()
		{
			IsUpdateCalled = true;
			return UpdateResult;
		}
	}
}
