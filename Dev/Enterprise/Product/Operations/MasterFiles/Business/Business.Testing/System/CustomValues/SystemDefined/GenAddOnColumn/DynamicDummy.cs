using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[SystemDefinedValues, UserDefinedValues]
	sealed class DynamicDummy : DummyBusinessObject
	{
		public DynamicDummy(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
