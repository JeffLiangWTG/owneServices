using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconOriginalDeclaration))]
	sealed class ReconOriginalDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDataRow()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertNotEquals("Row should not be the same as we don't want to catch the row in the factory against this businessobject", row, ((IBusinessObjectInternals)reconOriginal).Row);
		}

		public void TestIsInDatabase()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("This should be true to stop AddInfo from needing to serialise and save", true, reconOriginal.IsInDatabase);
		}

		public void TestEntrySummaryPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(entry.PK, reconOriginal.EntrySummaryPK);
		}

		public void TestEntrySummaryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, reconOriginal.EntrySummaryStatus);
		}

		public void TestJE_DeclarationReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.JE_DeclarationReference);
			declaration.JE_DeclarationReference = "B00000013";
			AssertEquals("B00000013", reconOriginal.JE_DeclarationReference);
		}

		public void TestUS_BondProducerAccNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.US_BondProducerAccNo);
			declaration.US_BondProducerAccNo = "21";
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("21", reconOriginal.US_BondProducerAccNo);
		}

		public void TestUS_EntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.US_EntryType);
			declaration.US_EntryType = "21";
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("21", reconOriginal.US_EntryType);
		}

		public void TestUS_OtherReconIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.US_OtherReconIndicator);
			declaration.US_OtherReconIndicator = "Y";
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("Y", reconOriginal.US_OtherReconIndicator);
		}

		public void TestUS_NAFTAReconIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZBool.False, reconOriginal.US_NAFTAReconIndicator);
			declaration.US_NAFTAReconIndicator = ZBool.True;
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZBool.True, reconOriginal.US_NAFTAReconIndicator);
		}

		public void TestUS_SuretyCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.US_SuretyCode);
			declaration.US_SuretyCode = "792";
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("792", reconOriginal.US_SuretyCode);
		}

		public void TestImporterOfRecordNumber()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var ein = importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "22342324", Core.Constants.CountryCodes.UnitedStates);
			var ssn = importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.SocialSecurityNumber, "3243242", Core.Constants.CountryCodes.UnitedStates);
			var cbp = importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "79865454", Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals(ZString.Empty, reconOriginal.ImporterOfRecordNumber);
			declaration.IOROrgPK = importer.PK;
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			AssertEquals("22342324", reconOriginal.ImporterOfRecordNumber);
			ein.Delete();
			AssertEquals("3243242", reconOriginal.ImporterOfRecordNumber);
			ssn.Delete();
			AssertEquals("79865454", reconOriginal.ImporterOfRecordNumber);
			cbp.Delete();
			AssertEquals(ZString.Empty, reconOriginal.ImporterOfRecordNumber);
		}

		public void TestPriorDisclosure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			Assert(!reconOriginal.US_PriorDisclosure);
			declaration.US_PriorDisclosure = true;
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			Assert(reconOriginal.US_PriorDisclosure);
		}

		public void TestNAFTA303ClaimStatement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			Assert(!reconOriginal.US_NAFTAClaimStat);
			declaration.US_NAFTAClaimStat = true;
			Factory.Save();
			reconOriginal = new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
			Assert(reconOriginal.US_NAFTAClaimStat);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var row = ((IBusinessObjectInternals)declaration).Row;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new ReconOriginalDeclaration(Factory, (IColumnIndexer)row, (IColumnIndexer)((IBusinessObjectInternals)entry).Row);
		}
	}
}
