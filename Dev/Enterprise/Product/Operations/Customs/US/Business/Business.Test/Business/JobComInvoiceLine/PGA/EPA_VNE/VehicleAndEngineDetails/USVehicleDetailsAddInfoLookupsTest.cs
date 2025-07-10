using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USVehicleDetailsAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			var vehicleDetails = vehicle.VehicleAndEngineDetails.AddNew();

			var lookups = vehicleDetails.AddInfoLookups;
			AssertEquals(typeof(MonthList), lookups.MonthList.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), lookups.IdentityQualifier.GetType());
			AssertEquals(typeof(ManufactureDateTypeList), lookups.DateTypes.GetType());
			Assert(!lookups.IdentityQualifier.ContainsCode(ItemIdentityNumberQualifierList.Codes.OfficialAnimalNumber));
			AssertEquals(2, lookups.IdentityQualifier.Count);
			Assert(lookups.IdentityQualifier.ContainsCode(ItemIdentityNumberQualifierList.Codes.SerialNumber));
			Assert(lookups.IdentityQualifier.ContainsCode(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN));
		}
	}
}
