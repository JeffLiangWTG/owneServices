using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client.ServiceTask.Test
{
	public class DummyReferenceDataUpdateServiceTask : ReferenceDataUpdateServiceTask
	{
		public DummyReferenceDataUpdateServiceTask()
			: this(new DummyDataSetUpdaterManagerWrapper { UpdateResult = true })
		{ }

		public DummyReferenceDataUpdateServiceTask(SharedDataSetUpdaterWrapper managerWrapper)
		{
			this.managerWrapper = managerWrapper;
		}
		readonly SharedDataSetUpdaterWrapper managerWrapper;

		protected override SharedDataSetUpdaterWrapper GetUpdateManagerWrapper()
		{
			return managerWrapper;
		}

		public bool IsUpdateAllCalled
		{
			get
			{
				var wrapper = (DummyDataSetUpdaterManagerWrapper)managerWrapper;
				return wrapper.IsUpdateAllCalled;
			}
			set
			{
				var wrapper = (DummyDataSetUpdaterManagerWrapper)managerWrapper;
				wrapper.IsUpdateAllCalled = value;
			}
		}
	}

	public class DummyDataSetUpdaterManagerWrapper : SharedDataSetUpdaterWrapper
	{
		public new int SavedDataSetCount
		{
			get { return savedDataSetCount; }
			set { savedDataSetCount = value; }
		}
		public bool UpdateResult { get; set; }

		public bool IsUpdateAllCalled { get; set; }

		public new void SetLogger(ILogger logger)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public override void LogSystemNotRegisteredInfo()
		{
			Console.WriteLine("Terminate for test");
		}

		public new bool DbInitializationCheck()
		{
			return true;
		}

		public override IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetAllDataSetUpdater()
		{
			throw new NotImplementedException();
		}

		public override bool Update(string tableName, string dataSet)
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{ }

		public override bool UpdateAll()
		{
			IsUpdateAllCalled = true;
			return UpdateResult;
		}
	}
}
