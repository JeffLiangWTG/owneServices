using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class SanctionsUserControlTest : TestCaseWithFactory
	{
		public void TestCopyNMFSDataToSanctions_Click()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_VesselName = "Declaration Vessel Name";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var header = declaration.Invoices.AddNew();
			var invoiceLine1 = header.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0301930000";
			invoiceLine1.US_UC_NKCountryOfOrigin = "RU";

			var nmfsLine1 = invoiceLine1.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			nmfsLine1.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			var harvesting1 = nmfsLine1.HarvestingDetails.AddNew();
			harvesting1.US_VesselCountry = "CA";
			harvesting1.US_HarvestedCountry = "CA";
			AssertEquals(0, invoiceLine1.FishingInformations.Count);

			using (var frm = new JobDeclarationForm(declaration))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				frm.Show();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)frm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.SanctionsTabPage;

				var copyNMFSDataToSanctionsButton = userControl.SanctionsUserControl.FindSingle<ZButton>("CopyNMFSDataToSanctionsButton");
				AssertNotNull(copyNMFSDataToSanctionsButton);
				copyNMFSDataToSanctionsButton.PerformClick();
				AssertEquals(1, invoiceLine1.FishingInformations.Count);
			}
		}

		public void TestGridVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0301930000";
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			using (var frm = new JobDeclarationForm(declaration))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.Sanctions, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				frm.Show();
				frm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = frm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)frm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.SanctionsTabPage;
				AssertNotNull(userControl.SanctionsUserControl);
				var fishingInformationGroupBox = userControl.SanctionsUserControl.FindSingle<ZGroupBox>("FishingInformationGroupBox");
				AssertNotNull(fishingInformationGroupBox);
				AssertEquals(true, fishingInformationGroupBox.Visible);

				var fishingInformationGrid = userControl.SanctionsUserControl.FindSingle<ZGrid>("FishingInformationGrid");
				AssertNotNull(fishingInformationGrid);
				AssertEquals(true, fishingInformationGrid.Visible);

				var miningInformationGroupBox = userControl.SanctionsUserControl.FindSingle<ZGroupBox>("MiningInformationGroupBox");
				AssertNotNull(miningInformationGroupBox);
				AssertEquals(false, miningInformationGroupBox.Visible);

				var miningInformationGrid = userControl.SanctionsUserControl.FindSingle<ZGrid>("MiningInformationGrid");
				AssertNotNull(miningInformationGrid);
				AssertEquals(false, miningInformationGrid.Visible);

				invoiceLine.JI_Tariff = "7102310000";
				fishingInformationGroupBox = userControl.SanctionsUserControl.FindSingle<ZGroupBox>("FishingInformationGroupBox");
				AssertNotNull(fishingInformationGroupBox);
				AssertEquals(false, fishingInformationGroupBox.Visible);

				fishingInformationGrid = userControl.SanctionsUserControl.FindSingle<ZGrid>("FishingInformationGrid");
				AssertNotNull(fishingInformationGrid);
				AssertEquals(false, fishingInformationGrid.Visible);

				miningInformationGroupBox = userControl.SanctionsUserControl.FindSingle<ZGroupBox>("MiningInformationGroupBox");
				AssertNotNull(miningInformationGroupBox);
				AssertEquals(true, miningInformationGroupBox.Visible);

				miningInformationGrid = userControl.SanctionsUserControl.FindSingle<ZGrid>("MiningInformationGrid");
				AssertNotNull(miningInformationGrid);
				AssertEquals(true, miningInformationGrid.Visible);
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing);
			var conditionType2 = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0301930000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType1.PK, tariff1.PK, "Fishing Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7102310000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType2.PK, tariff2.PK, "Mining Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
