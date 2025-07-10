using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummySubRelatableActivity : DummyRelatableActivity, ISubRelatableActivity
	{
		public DummySubRelatableActivity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ISuperRelatableActivity SuperActivity
		{
			get;
			set;
		}
	}
}
