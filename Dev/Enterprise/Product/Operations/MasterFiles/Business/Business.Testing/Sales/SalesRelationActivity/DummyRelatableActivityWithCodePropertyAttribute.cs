using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	[CodeProperty("CodeProperty")]
	sealed class DummyRelatableActivityWithCodePropertyAttribute : DummyRelatableActivity
	{
		public DummyRelatableActivityWithCodePropertyAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CodeProperty
		{
			get;
			set;
		}
	}
}
