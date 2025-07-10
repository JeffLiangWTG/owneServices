using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderDocManagerInfo))]
	class CusInBondHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CusInBondHeader>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			return header;
		}
	}
}
