using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	[TestedType(typeof(WDFEDIInterchange))]
	public class WDFEDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<WDFEDIInterchange>();
			AssertEquals(true, interchange.EI_IsActive);
			AssertEquals(EDIInterchange.ApplicationCodes.WarehouseDocket, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.WarehouseDocket, interchange.EI_InterchangeType);
			AssertEquals(ZString.Empty, interchange.EI_FooterText);
			AssertEquals(GlbBranch.CurrentBranch.PK, interchange.EI_GB);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New(GetExpectedBusinessObjectType());
	}
}
