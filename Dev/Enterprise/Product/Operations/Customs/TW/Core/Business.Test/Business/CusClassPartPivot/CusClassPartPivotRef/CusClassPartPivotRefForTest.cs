using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	class CusClassPartPivotRefForTest : CusClassPartPivotRef
	{
		public CusClassPartPivotRefForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ReferenceType => "ZZZ";
	}
}
