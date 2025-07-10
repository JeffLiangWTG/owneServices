using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(IssuerAndBillOfLading))]
	public class IssuerAndBillOfLadingTest : Customs.Business.Testing.CusCodeDataTest<IssuerAndBillOfLading>
	{
		public void TestCY_TypeIsSet()
		{
			var billOfLading = Factory.New<IssuerAndBillOfLading>();
			AssertEquals(IssuerAndBillOfLading.IssuerAndBillOfLadingTypeCode, billOfLading.CY_Type);
		}

		public void TestParent()
		{
			var billOfLading = Factory.New<IssuerAndBillOfLading>();
			AssertNull(billOfLading.Parent);

			var moveHeader = Factory.New<CusInBondMoveHeader>();
			billOfLading.CY_ParentID = moveHeader.PK;
			billOfLading.CY_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			AssertEquals(moveHeader, billOfLading.Parent);
		}

		public void TestLoadIssuerCodeBOLStatusFromCY_Data()
		{
			var billOfLading = Factory.New<IssuerAndBillOfLading>();
			billOfLading.CY_ParentID = ZGuid.NewZGuid();
			billOfLading.CY_Data = "AAAA632409-35668";
			billOfLading.CY_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix;
			AssertEquals("AAAA", billOfLading.CY_IssuerCode);
			AssertEquals("632409-35668", billOfLading.CY_BillOfLading);

			billOfLading.CY_IssuerCode = "";
			AssertEquals("", billOfLading.CY_IssuerCode);

			billOfLading.CY_BillOfLading = "8734587";
			AssertEquals("8734587", billOfLading.CY_BillOfLading);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var billOfLadingLoaded = factory2.Load<IssuerAndBillOfLading>(billOfLading.PK);
			AssertEquals("", billOfLadingLoaded.CY_IssuerCode);
			AssertEquals("8734587", billOfLadingLoaded.CY_BillOfLading);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			var billOfLading = factory.New<IssuerAndBillOfLading>();
			billOfLading.Parent = moveHeader;
			billOfLading.CY_Data = "AAAA632409-35668";
			return billOfLading;
		}
	}
}
