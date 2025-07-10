using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	[UserDefinedValues, SystemDefinedValues]
	sealed class SystemAndUserDefined : DummyBusinessObject
	{
		public SystemAndUserDefined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
