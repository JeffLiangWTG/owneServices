using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeList()
		{
			CodeDescriptionPairList list = lookups.ChargeTypeList;
			AssertEquals(true, list.ContainsCode(USCustomsChargeTypeList.Codes.DisbursementCharge));
		}

		GroupInvoiceCharge groupInvoiceCharge;
		ChargeLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			groupInvoiceCharge = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].Charges.AddNew();
			lookups = new ChargeLookups(groupInvoiceCharge);
		}
	}
}
