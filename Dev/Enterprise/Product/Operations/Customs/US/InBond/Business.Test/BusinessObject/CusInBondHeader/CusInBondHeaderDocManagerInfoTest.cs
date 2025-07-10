using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondHeaderDocManagerInfo))]
	sealed class CusInBondHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject() => Factory.New<CusInBondHeader>();

		public override BusinessObject GetPopulatedParentBusinessObject() => Factory.New<CusInBondHeader>();
	}
}
