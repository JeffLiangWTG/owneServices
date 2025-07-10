using System.Drawing;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrganisationTaxRateFileImport))]
	sealed class OrganisationTaxRateFileImportTest : NonPersistentBusinessObjectTestCase
	{
		public void Test_LoadTaxConfigurationObject()
		{
			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, taxRateSource: TaxRateSources.OrganisationOnly.Code);

			var collection = orgTaxRateFileImport.TaxConfigurations;
			orgTaxRateFileImport.TaxConfiguration = collection[0].PK;

			var expectedObject = orgTaxRateFileImport.TaxConfigurationObject;
			AssertEquals(expectedObject.PK, orgTaxRateFileImport.TaxConfiguration);
		}

		public void Test_LoadTaxConfigurationObjectEmptyWhenPKIsNotInTaxConfigurationsCollection()
		{
			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, taxRateSource: TaxRateSources.OrganisationOnly.Code);

			var collection = orgTaxRateFileImport.TaxConfigurations;
			orgTaxRateFileImport.TaxConfiguration = ZGuid.NewZGuid();

			var expectedObject = orgTaxRateFileImport.TaxConfigurationObject;
			AssertEquals(expectedObject, null);

			orgTaxRateFileImport.TaxConfiguration = collection[0].PK;
			expectedObject = orgTaxRateFileImport.TaxConfigurationObject;
			AssertEquals(expectedObject.PK, orgTaxRateFileImport.TaxConfiguration);
		}

		public void Test_LoadTaxConfigurations()
		{
			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			var mockHelper = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();
			mockHelper.Setup(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(() => null);
			AssertNull(orgTaxRateFileImport.TaxConfigurations);

			mockHelper.Verify(x => x.GetTaxConfigurationThatSupportsOrganisationRates(orgTaxRateFileImport.Factory, GlbCompany.CurrentCompany), Times.Once);
			mockHelper.Reset();

			var taxConfigurationCollection = new AccTaxConfigurationCollection(Factory);
			mockHelper.Setup(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(taxConfigurationCollection);

			var expectedCollection = orgTaxRateFileImport.TaxConfigurations;
			AssertEquals(0, expectedCollection.Count);
			mockHelper.Verify(x => x.GetTaxConfigurationThatSupportsOrganisationRates(orgTaxRateFileImport.Factory, GlbCompany.CurrentCompany), Times.Once);
			mockHelper.Reset();

			var taxConfig1 = taxConfigurationCollection.AddNew();
			var taxConfig2 = taxConfigurationCollection.AddNew();
			mockHelper.Setup(x => x.GetTaxConfigurationThatSupportsOrganisationRates(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>())).Returns(taxConfigurationCollection);
			orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			expectedCollection = orgTaxRateFileImport.TaxConfigurations;
			AssertEquals(2, expectedCollection.Count);
			mockHelper.Verify(x => x.GetTaxConfigurationThatSupportsOrganisationRates(orgTaxRateFileImport.Factory, GlbCompany.CurrentCompany), Times.Once);
			mockHelper.Reset();

			expectedCollection = orgTaxRateFileImport.TaxConfigurations;
			mockHelper.Verify(x => x.GetTaxConfigurationThatSupportsOrganisationRates(orgTaxRateFileImport.Factory, GlbCompany.CurrentCompany), Times.Never);
		}

		public void Test_LoadRateSources()
		{
			var originList = new RateSourceMethods();

			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			AssertNotNull(orgTaxRateFileImport.RateSourceList);
			AssertEquals(originList.Count, orgTaxRateFileImport.RateSourceList.Count);
		}

		public void Test_ListAttributeOf()
		{
			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			AssertHasCustomAttribute<ListAttribute>(orgTaxRateFileImport.GetType(), nameof(orgTaxRateFileImport.TaxConfiguration), false, a => a.ListDataSourceMember == nameof(orgTaxRateFileImport.TaxConfigurations));
			AssertHasCustomAttribute<ListAttribute>(orgTaxRateFileImport.GetType(), nameof(orgTaxRateFileImport.RateSource), false, s => s.ListDataSourceMember == nameof(orgTaxRateFileImport.RateSourceList));
		}

		public void TestImportLinesIsRegisteredEditableChildObject()
		{
			var orgTaxRateFileImport = new OrganisationTaxRateFileImport();
			Assert("IsRegisteredEditableChildObject", orgTaxRateFileImport.IsRegisteredEditableChildObject(orgTaxRateFileImport.ImportLines));
		}

		public void TestNoteHasCorrectValues()
		{
			SetupAndCreateNote();

			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("GetAllNotes return only one note.", 1, notes.Count);

			AssertEquals("Organization Tax Configuration Rates Import Log", notes[0].ST_Description);
			AssertEquals("INT", notes[0].ST_NoteType);
			AssertEquals(false, notes[0].ST_IsPopupLog);
			AssertEquals(false, notes[0].ST_NoteData.IsEmpty);

			AssertEquals("Default value must be set as false by default.", false, notes[0].ST_IsTextOnly);
			AssertEquals("Default value must be set as true by default.", true, notes[0].IsReadOnlyAfterAdd);

			AssertEquals(GlbCompany.GetCurrentCompany(Factory).PK, notes[0].ST_GC_RelatedCompany);
		}

		public void TestNoteDataAsText()
		{
			SetupAndCreateNote();

			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();

			var expectedContent = @"System Created Organization Rates Import Log
Tax: X6-7LPDX81ZF7-GOGQ2QVQOF-AR
Start Date: 01-Jun-22
End Date: 15-Jun-22
Rate Source: 
Importing User: CWSupport

Summary: 1 rate records added.

Updated Organization details:
Organization Code: ARORG; Organization Name: ORG NAME 1; Reg. Code: 123; Rate Numerator: 1; Rate Denominator: 10
";

			AssertMultilineASCIIEquals(expectedContent, notes[0].ST_NoteDataAsText);
		}

		public void TestNoteDataAsRtf()
		{
			SetupAndCreateNote();

			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();

			AssertFormattedRtfString(notes[0].ST_NoteData.ToUTF8());

			void AssertFormattedRtfString(ZString zBlobRtf)
			{
				using (Font fontBold = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold))
				using (Font fontRegular = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular))
				{
					FormattedRtfString rtf = new FormattedRtfString();

					rtf += new FormattedRtfPart("System Created Organization Rates Import Log" + System.Environment.NewLine, fontBold);
					rtf += new FormattedRtfPart("Tax: X6-7LPDX81ZF7-GOGQ2QVQOF-AR" + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("Start Date: 01-Jun-22" + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("End Date: 15-Jun-22" + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("Rate Source: " + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("Importing User: CWSupport" + System.Environment.NewLine + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("Summary: ", fontBold);
					rtf += new FormattedRtfPart("1 rate records added." + System.Environment.NewLine + System.Environment.NewLine, fontRegular);
					rtf += new FormattedRtfPart("Updated Organization details:" + System.Environment.NewLine, fontBold);
					rtf += new FormattedRtfPart("Organization Code: ARORG; Organization Name: ORG NAME 1; Reg. Code: 123; Rate Numerator: 1; Rate Denominator: 10" + System.Environment.NewLine, fontRegular);

					string rtfString = rtf.ToRtf();

					ORtfTextUtilTest.AssertRtfTextsEqualLanguageIndependent(zBlobRtf, rtfString);
				}
			}
		}

		public void TestFactorySaveSecondSavingBecauseStmNoteNonUniqueDescriptionIsIncluded()
		{
			SetupAndCreateNote();
			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			Factory.Save();

			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("Saving the factory add new Note", 1, notes.Count);

			var orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(Factory);
			orgTaxRateFileImportLine.OrganizationCode = "ARORG";
			orgTaxRateFileImportLine.OrganizationName = "ORG NAME 1";
			orgTaxRateFileImportLine.RegistrationCode = "123";
			orgTaxRateFileImportLine.RateSource = "MOV";

			orgTaxRateFileImportLine.StartDate = new ZDate(2022, 06, 01);
			orgTaxRateFileImportLine.EndDate = new ZDate(2022, 06, 15);
			orgTaxRateFileImportLine.RateNumerator = 1;
			orgTaxRateFileImportLine.RateDenominator = 10;
			orgTaxRateFileImportLine.TaxConfigurationPK = OrgTaxConfig.PK;

			OrgTaxRateFileImport.ImportLines.Add(orgTaxRateFileImportLine);
			OrgTaxRateFileImport.Factory.Save();

			notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("Second saving with same data now should be saved because we include in StmNoteNonUniqueDescriptions to StmNote table view.", 2, notes.Count);
		}

		public void TestFactorySaveCalledTwiceSaveJustTheFirstTimeImportLine()
		{
			SetupAndCreateNote();
			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			Factory.Save();

			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("Saving the factory add new Note", 1, notes.Count);
			var expectedLine = "Organization Code: ARORG; Organization Name: ORG NAME 1; Reg. Code: 123; Rate Numerator: 1; Rate Denominator: 10";
			AssertContains(expectedLine, notes[0].ST_NoteDataAsText);

			OrgTaxRateFileImport.ImportLines[0].OrganizationName = "SAMPLE ORG";
			Factory.Save();

			notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("Saving the factory add new Note", 1, notes.Count);
			var nonExpectedLine = "Organization Code: ARORG; Organization Name: SAMPLE ORG; Reg. Code: 123; Rate Numerator: 1; Rate Denominator: 10";
			AssertNotContains(nonExpectedLine, notes[0].ST_NoteDataAsText);
		}

		void SetupAndCreateNote(bool singleLine = true)
		{
			OrgTaxRateFileImport = new OrganisationTaxRateFileImport();
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_Code = "AR-TAXCONFIGCODE-AR";
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = orgHeader.PK;

			OrgTaxConfig = Factory.NewWithValidTestData<AccOrgTaxConfiguration>();
			OrgTaxConfig.OTC_OB = orgCompanyData.PK;
			OrgTaxConfig.OTC_ETC = taxConfiguration.PK;
			OrgTaxRateFileImport.TaxConfiguration = taxConfiguration.PK;

			Factory.Save();

			AddImportLines(OrgTaxConfig.PK, singleLine);
		}

		void AddImportLines(ZGuid orgTaxConfiguration, bool singleLine = true)
		{
			var orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(Factory);
			orgTaxRateFileImportLine.OrganizationCode = "ARORG";
			orgTaxRateFileImportLine.OrganizationName = "ORG NAME 1";
			orgTaxRateFileImportLine.RegistrationCode = "123";
			orgTaxRateFileImportLine.RateSource = "MOV";

			orgTaxRateFileImportLine.StartDate = new ZDate(2022, 06, 01);
			orgTaxRateFileImportLine.EndDate = new ZDate(2022, 06, 15);
			orgTaxRateFileImportLine.RateNumerator = 1;
			orgTaxRateFileImportLine.RateDenominator = 10;
			orgTaxRateFileImportLine.TaxConfigurationPK = orgTaxConfiguration;

			OrgTaxRateFileImport.ImportLines.Add(orgTaxRateFileImportLine);

			if (!singleLine)
			{
				var orgTaxRateFileImportLine2 = new OrganisationTaxRateFileImportLine(Factory);
				orgTaxRateFileImportLine2.OrganizationCode = "ARORG2";
				orgTaxRateFileImportLine2.OrganizationName = "ORG NAME 2";
				orgTaxRateFileImportLine2.RegistrationCode = "456";
				orgTaxRateFileImportLine2.RateSource = "QUA";

				orgTaxRateFileImportLine2.StartDate = new ZDate(2022, 07, 01);
				orgTaxRateFileImportLine2.EndDate = new ZDate(2022, 07, 15);
				orgTaxRateFileImportLine2.RateNumerator = 5;
				orgTaxRateFileImportLine2.RateDenominator = 1;
				orgTaxRateFileImportLine2.TaxConfigurationPK = orgTaxConfiguration;

				OrgTaxRateFileImport.ImportLines.Add(orgTaxRateFileImportLine2);
			}

			OrgTaxRateFileImport.Factory.Save();
		}

		public void TestAdditionalNoteCanBeSavedForUserOfDifferentLanguage()
		{
			var englishStaff = Factory.NewWithValidTestData<GlbStaff>();
			englishStaff.GS_LoginName = "en-user";
			englishStaff.GS_WorkingLanguage = SharedConstants.Languages.English;

			var britishStaff = Factory.NewWithValidTestData<GlbStaff>();
			britishStaff.GS_LoginName = "gb-user";
			britishStaff.GS_WorkingLanguage = SharedConstants.Languages.EnglishBritish;

			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(englishStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				SetupAndCreateNote();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(britishStaff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(Factory);
				orgTaxRateFileImportLine.OrganizationCode = "ARORG";
				orgTaxRateFileImportLine.OrganizationName = "ORG NAME 1";
				orgTaxRateFileImportLine.RegistrationCode = "123";
				orgTaxRateFileImportLine.RateSource = "MOV";

				orgTaxRateFileImportLine.StartDate = new ZDate(2022, 06, 01);
				orgTaxRateFileImportLine.EndDate = new ZDate(2022, 06, 15);
				orgTaxRateFileImportLine.RateNumerator = 1;
				orgTaxRateFileImportLine.RateDenominator = 10;
				orgTaxRateFileImportLine.TaxConfigurationPK = OrgTaxConfig.PK;

				OrgTaxRateFileImport.ImportLines.Add(orgTaxRateFileImportLine);
				AssertNoExceptionThrown(() => OrgTaxRateFileImport.Factory.Save());

				var orgTaxRateFileImportLine1 = new OrganisationTaxRateFileImportLine(Factory);
				orgTaxRateFileImportLine1.OrganizationCode = "ARORG";
				orgTaxRateFileImportLine1.OrganizationName = "ORG NAME 1";
				orgTaxRateFileImportLine1.RegistrationCode = "123";
				orgTaxRateFileImportLine1.RateSource = "MOV";

				orgTaxRateFileImportLine.StartDate = new ZDate(2022, 06, 01);
				orgTaxRateFileImportLine1.EndDate = new ZDate(2022, 06, 15);
				orgTaxRateFileImportLine1.RateNumerator = 1;
				orgTaxRateFileImportLine1.RateDenominator = 10;
				orgTaxRateFileImportLine1.TaxConfigurationPK = OrgTaxConfig.PK;

				OrgTaxRateFileImport.ImportLines.Add(orgTaxRateFileImportLine1);
				AssertNoExceptionThrown(() => OrgTaxRateFileImport.Factory.Save());
			}

			var orgProxy = GlbCompany.GetCurrentCompany(Factory).OrgProxy;
			var notes = (StmNoteCollection)orgProxy.Notes.GetAllNotes();
			AssertEquals("All Different Language Notes Are in English Language Descripton", 3, notes.Cast<StmNote>().Count(e => e.ST_Description == "Organization Tax Configuration Rates Import Log"));
		}

		AccOrgTaxConfiguration OrgTaxConfig;
		OrganisationTaxRateFileImport OrgTaxRateFileImport;
	}
}
