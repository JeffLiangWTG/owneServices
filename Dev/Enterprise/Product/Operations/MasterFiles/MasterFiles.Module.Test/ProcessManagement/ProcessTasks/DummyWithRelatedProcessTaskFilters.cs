using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyWithRelatedProcessTaskFilters : DummyWithRelatedFilters
	{
		public DummyWithRelatedProcessTaskFilters(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ModuleIdentifier ModuleIdentifier => ModuleIDs.ProcessTasks;
	}
}
