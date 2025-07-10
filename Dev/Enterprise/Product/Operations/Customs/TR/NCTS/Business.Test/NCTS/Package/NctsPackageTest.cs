using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackage))]
	public class NctsPackageTest : CusInvPackTest<NctsDepartureCargoDesc>
	{
		public void TestValidation()
		{
			AssertType<NctsPackageValidation>(package.Validation);
		}

		public void TestPhase5Validation()
		{
			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader2.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var bills = nctsHeader2.MovementHeader.Header.Bills.AddNew();
			var goodsitems = bills.GoodsItems.AddNew();
			package = goodsitems.Packages.AddNew();

			AssertType<NctsPackagePhase5Validation>(package.Validation);
		}

		protected override NctsDepartureCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader.GoodsItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => package;

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			package = goodsItem.Packages.AddNew();
		}

		NctsDepartureCargoDesc goodsItem;
		NctsPackage package;
	}
}
