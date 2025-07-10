using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[SystemDefinedValues]
	sealed class DummyForTest : DummyBusinessObject
	{
		public DummyForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
