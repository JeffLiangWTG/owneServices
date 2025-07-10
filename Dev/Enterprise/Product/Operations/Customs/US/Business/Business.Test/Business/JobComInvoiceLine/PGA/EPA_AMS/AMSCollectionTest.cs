using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AMSCollection))]
	public class AMSCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Test1";
			AssertEquals(true, invoiceLine.AMSLines.AllowNew);
			var amsLine = invoiceLine.AMSLines.AddNew();
			AssertEquals("Test1", amsLine.US_CommercialDescription);
		}

		[TestDate(2021, 12, 01)]
		public void TestSetDefaultUS_ProgramFromTariffAttribute()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0058580058";
			tariff.UE_DateFrom = new ZDateTime(2018, 09, 24);
			tariff.UE_DateTo = new ZDateTime(2079, 06, 06);

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0059590059";
			tariff2.UE_DateFrom = new ZDateTime(2018, 09, 24);
			tariff2.UE_DateTo = new ZDateTime(2079, 06, 06);
			Factory.Save();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff0058580058 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDate(2021, 01, 01), new ZDate(2079, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "XX1", tariff0058580058);
			var tariff0059590059 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff2.UE_Tariff, new ZDate(2021, 01, 01), new ZDate(2079, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "XX2,MO2,OR1", tariff0059590059);
			Factory.Save();

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			AssertEquals("XX1 is not a valid program code", ZString.Empty, amsLine.US_Program);
			var invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.US_NOPInd = OGAIndicatorList.Codes.Declared;
			var amsLine2 = invoiceLine2.AMSLines.AddNew();
			AssertEquals("OR1 is valid and defaulted.", AMSProgramList.Codes.OR1, amsLine2.US_Program);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.AMSLines;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.AMSLines;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			return new AMSCollection(invoiceLine);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
