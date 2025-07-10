using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;
using TransportTypeList = Enterprise.Customs.US.Business.TransportTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(USJobDeclarationUserControl))]
	sealed class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<USJobDeclarationUserControl, JobDeclaration>
	{
		public void TestOrgAddressControlsUnderUSOrganisationTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new USJobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				foreach (var messageType in declaration.Lookups.MessageTypeList.GetAllCodes().Where(x => x != JobMessageTypeList.Codes.Export))
				{
					declaration.JE_MessageType = messageType;
					AssertTestOrgAddressControlsUnderOrgTab(control.USOrganisationsTabPage);
				}
			}
		}

		public void TestControlsVisibilityWhenMessageTypeChanged()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var declaration = Factory.New<JobDeclaration>();
				using (var form = new ZForm(declaration))
				using (var control = new USJobDeclarationUserControl())
				{
					control.JobDeclaration = declaration;
					form.Controls.Add(control);
					form.Show();
					AssertEquals("PSCCheckBox.Visible", false, control.PSCCheckBox.Visible);
					AssertEquals("EnableENSCheckBox.Text", "Enable 7501", control.EnableENSCheckBox.Text);
					AssertEquals("US_EnableCRLCheckBox.Text", "Enable 3461", control.US_EnableCRLCheckBox.Text);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("PSCCheckBox.Visible", true, control.PSCCheckBox.Visible);
					AssertEquals("EnableENSCheckBox.Text", "Enable Ent Sum", control.EnableENSCheckBox.Text);
					AssertEquals("US_EnableCRLCheckBox.Text", "Enable Cargo Rel", control.US_EnableCRLCheckBox.Text);
				}
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			PrepareData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				AssertComponentVisible(form);

				declaration.JE_RL_NKPortOfArrival = "TEST1";
				AssertComponentVisible(form, true);

				declaration.US_RL_NKPortOfExport = "TEST1";
				AssertComponentVisible(form, true, false, true, false);

				declaration.JE_RL_NKPortOfLoading = "TEST1";
				AssertComponentVisible(form, true, true, true, false);

				declaration.JE_TransportMode = "AIR";
				AssertComponentVisible(form, true);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertComponentVisible(form, false, true, false, false);

				declaration.JE_TransportMode = "SEA";
				AssertComponentVisible(form, true, true, false, false);
			}
		}

		void AssertComponentVisible(JobDeclarationForm form, bool dischargeTypeIsDropEdit = false, bool loadingTypeIsDropEdit = false, bool exportDropEditVisible = false, bool exportFindBoxVisible = true)
		{
			AssertEquals(dischargeTypeIsDropEdit, form.FindSingle<ZDropEdit>("PortOfDischargeSchDDropEdit").Visible);
			AssertEquals(!dischargeTypeIsDropEdit, form.FindSingle<ZCodeFindBox>("PortOfDischargeSchDFindBox").Visible);

			AssertEquals(loadingTypeIsDropEdit, form.FindSingle<ZDropEdit>("PortOfLoadingSchDDropEdit").Visible);
			AssertEquals(!loadingTypeIsDropEdit, form.FindSingle<ZCodeFindBox>("PortOfLoadingSchDFindBox").Visible);

			AssertEquals(exportDropEditVisible, form.FindSingle<ZDropEdit>("PortOfExportSchDCodeDropEdit").Visible);
			AssertEquals(exportFindBoxVisible, form.FindSingle<ZCodeFindBox>("PortOfExportSchDCodeFindBox").Visible);
		}

		void PrepareData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "4001 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "4002 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "Test Name", startDate, endDate);
			Factory.Save();

			CreateLocoIfNotExists("TEST1");
			CreateLocoMapIfNotExists("4001", "TEST1", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("4002", "TEST1", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists("60001", "TEST1", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("60002", "TEST1", USLocoMapSystemUsageList.Codes.SCK);
		}

		void CreateLocoIfNotExists(string locoCode, string country = "US")
		{
			var codeFilter = new ZQuery(RefUNLOCOSchema.RL_Code, locoCode);
			var unLoco = Factory.LoadTop1<RefUNLOCO>(codeFilter);
			if (unLoco == null)
			{
				var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
				testUSLoco.RL_Code = locoCode;
				testUSLoco.RL_PortName = "TEST Port - " + locoCode;
				testUSLoco.RL_IsSystem = true;
				testUSLoco.RL_HasAirport = true;
				testUSLoco.RL_HasSeaport = true;
				testUSLoco.RL_RN_NKCountryCode = country;
				Factory.Save();
			}
		}

		RefLocoMap CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}

			return locoMap;
		}

		public void TestDeleteDeclarationOnCargoReleaseTypeChanging()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_IsInvoiceByRequest = ZBool.True;
			declaration.US_EnableAII = ZBool.True;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				Assert(declaration.US_IsInvoiceByRequest);
				Assert(declaration.US_EnableAII);
				Assert(control.US_EnableAIICheckBox.Visible);
				Assert(control.InvoiceByRequestCheckBox.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				Assert(declaration.US_IsInvoiceByRequest);
				Assert(declaration.US_EnableAII);
				Assert(!control.US_EnableAIICheckBox.Visible);
				Assert(!control.InvoiceByRequestCheckBox.Visible);
			}
		}

		public void TestControlLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				AssertEquals("US_UC_NKCountryOfExportCodeFindBox", "Country/Region of Export", control.US_UC_NKCountryOfExportCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("US_UC_NKCountryOfExportCodeFindBox Short Caption", "Ctry/Rgn. of Export", control.US_UC_NKCountryOfExportCodeFindBox.CaptionResourceString.ShortCaption);
				AssertEquals("US_RN_NKCountryOfDestinationCodeFindBox", "Country/Region of Dest.", control.US_RN_NKCountryOfDestinationCodeFindBox.CaptionResourceString.Caption);
				AssertEquals("US_RN_NKCountryOfDestinationCodeFindBox Short Caption", "Ctry/Rgn. Dest.", control.US_RN_NKCountryOfDestinationCodeFindBox.CaptionResourceString.ShortCaption);
			}
		}

		public void TestControlsVisibilityForRail()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("IsRail", true, declaration.IsRail);
				AssertEquals("JE_MasterBillForSeaBoundTextBox visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestControlsVisibilityForHandCarry()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("JE_MasterBillForSeaBoundTextBox visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
				AssertEquals("JE_MasterBillForSeaBoundTextBox visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestControlsVisiblityForECCNNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCodeC58 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C58, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC58PK = refCusCodeC58.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC58PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired, "Mandatory");

			var refCusCodeC32 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "1C351", USAESLicenseCode.Codes.C32, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC32PK = refCusCodeC32.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC32PK, RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C32);

			var refCusCodeC30 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "C2343", USAESLicenseCode.Codes.C30, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC30PK = refCusCodeC30.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C30);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ECCNRequired, "Mandatory");

			Factory.Save();

			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33 });

			var declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

				declaration.US_LicenseType = USAESLicenseCode.Codes.C30;
				AssertEquals("ECCNDropEdit visibility", true, control.ECCNCodeFindBox.Visible);
				AssertEquals("ECCNTextBox visibility", false, control.ECCNTextBox.Visible);

				declaration.US_LicenseType = USAESLicenseCode.Codes.C33;
				AssertEquals("ECCNDropEdit visibility", false, control.ECCNCodeFindBox.Visible);
				AssertEquals("ECCNTextBox visibility", true, control.ECCNTextBox.Visible);
			}
		}

		public void TestControlsVisibilityForTruck()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("IsTruck", true, declaration.IsTruck);
				AssertEquals("JE_MasterBillForSeaBoundTextBox visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", false, control.JE_VoyageFlightNoBoundTextBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility for ACE", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestControlsVisibilityForRoad()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_TransportMode = TransportTypeList.Codes.Road;
				AssertEquals("IsRoad", true, declaration.IsRoad);
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestControlsVisibilityForBWB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
				AssertEquals("IsBorderWaterBone", true, declaration.IsBorderWaterBorne);
				AssertEquals("JE_MasterBillForSeaBoundTextBox visibility", true, control.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("JE_VoyageFlightNoBoundTextBox visibility", true, control.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestEnableAIIAndMonthlyFilingVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				Assert("For ACE the Enable AII tick box can be removed except when the Cargo Release Type = â€˜ACSâ€™", !control.US_EnableAIICheckBox.Visible);
				Assert("MonthlyFilingCheckBox Visible should dispaly for 'FIX'", control.US_MonthlyFilingCheckBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				Assert("MonthlyFilingCheckBox Visible should not dispaly for 'FIX'", !control.US_MonthlyFilingCheckBox.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				Assert(control.US_EnableAIICheckBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				Assert(control.US_EnableAIICheckBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("MonthlyFilingCheckBox Visible should not dispaly for 'EXP'", !control.US_MonthlyFilingCheckBox.Visible);
			}
		}

		public void TestEnableInvoiceByRequestCheckBoxVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				Assert("For ACE the Enable Inv By Req tick box can be removed except when the Cargo Release Type = â€˜ACSâ€™", !control.InvoiceByRequestCheckBox.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				Assert(control.InvoiceByRequestCheckBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				Assert(control.InvoiceByRequestCheckBox.Visible);
			}
		}

		public void TestEnableCBPBrokerOrgControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControl as USJobDeclarationUserControl;
				var miscOrgControl = control.JobMiscOrgsControl;
				form.Show();
				Assert("For ACE the CBP Broker Org Control can be removed except when the Cargo Release Type = ‘ACS’", !miscOrgControl.CBPBrokerOrgControl.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				Assert(control.InvoiceByRequestCheckBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				Assert(control.InvoiceByRequestCheckBox.Visible);
			}
		}

		public void TestWarehouseWithdrawalPanelControlsVisibilityAndCaption()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				AssertEquals("Caption is Warehouse Withdrawal", "Warehouse Withdrawal", control.WarehouseWithdrawalLabel.Text);
				AssertEquals("Withdrawal panel not visible", false, control.WarehouseWithdrawalPanel.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
				AssertEquals("Caption is Re-Warehouse", "Re-Warehouse", control.WarehouseWithdrawalLabel.Text);
				AssertEquals("Withdrawal panel visible", true, control.WarehouseWithdrawalPanel.Visible);
				AssertEquals("Warehouse District Port not visible", false, control.WHSDistrictPortCodeFindBox.Visible);
				AssertEquals("Final Withdrawal not visible", false, control.IsFinalWHSCheckBox.Visible);
				AssertEquals("Qty in W/H not visible", false, control.QtyInWhBeforeWithdrawalCalcEdit.Visible);
				AssertEquals("W/draw Qty not visible", false, control.QtyBeingWithdrawnCalcEdit.Visible);
				AssertEquals("Balance not visible", false, control.QtyInWHAfterWithdrawalCalcEdit.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
				AssertEquals("Caption is Warehouse Withdrawal", "Warehouse Withdrawal", control.WarehouseWithdrawalLabel.Text);
				AssertEquals("Withdrawal panel visible", true, control.WarehouseWithdrawalPanel.Visible);
				AssertEquals("Warehouse District Port visible", true, control.WHSDistrictPortCodeFindBox.Visible);
				AssertEquals("Final Withdrawal visible", true, control.IsFinalWHSCheckBox.Visible);
				AssertEquals("Qty in W/H visible", true, control.QtyInWhBeforeWithdrawalCalcEdit.Visible);
				AssertEquals("W/draw Qty visible", true, control.QtyBeingWithdrawnCalcEdit.Visible);
				AssertEquals("Balance visible", true, control.QtyInWHAfterWithdrawalCalcEdit.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
				AssertEquals("Warehouse District Port not visible", false, control.WHSDistrictPortCodeFindBox.Visible);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals("Warehouse District Port visible", true, control.WHSDistrictPortCodeFindBox.Visible);
			}
		}

		public void TestMasterBillForAirAllowCharPrefix()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MasterBill = "AMF12345678";
			declaration.JE_MasterBillIssuerSCAC = "Z~";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				AssertEquals("IsAir", true, declaration.IsAir);
				AssertEquals("Sea Text box should be invisible", false, control.JE_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("Air text box is visible", true, control.JE_MasterBillForAirBoundTextBox.Visible);
				AssertEquals("AIR TextBox takes alpha letters", "AMF1234 5678", control.JE_MasterBillForAirBoundTextBox.MasterBillText);
			}
		}

		public void TestSetRightTabControlSelectTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				AssertEquals("RightTabControl.SelectedTab", control.USOrganisationsTabPage, control.RightTabControl.SelectedTab);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("RightTabControl.SelectedTab", control.OrganisationsTabPage, control.RightTabControl.SelectedTab);
			}
		}

		public void TestSplitShipmentReleasePanelVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals(true, control.SEReleasePanel.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				AssertEquals(false, control.SEReleasePanel.Visible);
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				AssertEquals(true, control.SEReleasePanel.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals(false, control.SEReleasePanel.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				AssertEquals(false, control.SEReleasePanel.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				declaration.US_EnableCRL = true;
				declaration.US_EnableSPN = true;
				AssertEquals(false, control.SEReleasePanel.Visible);
				AssertEquals(true, control.SEReleaseAndStandAlonePriorNoticePanel.Visible);
				AssertEquals(true, control.SplitShipmentReleaseCodeForSPNDropEdit.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals(false, control.SEReleasePanel.Visible);
				AssertEquals(true, control.SEReleaseAndStandAlonePriorNoticePanel.Visible);
				AssertEquals(false, control.SplitShipmentReleaseCodeForSPNDropEdit.Visible);
			}
		}

		public void TestShowOrHideControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				AssertEquals("MessageSubType is not relevant for US", false, control.JE_MessageSubTypeBoundDropDownEdit.Visible);
				Assert(control.IncoTermDropEdit.Visible);
				Assert(control.IncoTermExplainButton.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertShowOrHideExportControls(control, true);
				AssertShowOrHideNonExportControls(control, false);
				AssertEquals("Carrier SCAC should now be visible for Export jobs as well", control.CarrierSCACCodeFindBox.Visible, true);
			}
		}

		public void TestAllocateImportEntryNumberButtonVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(true, control.AllocateImportEntryNumberButton.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				AssertEquals(false, control.AllocateImportEntryNumberButton.Visible);
			}
		}

		public void TestControlVisibilityForExWarehouse()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("HMFPanel should be hidden for Export", false, control.HMFPanel.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("HMFPanel should be Shown for Import", true, control.HMFPanel.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
				AssertEquals("HMFPanel should be hidden for ExWarehouse", false, control.HMFPanel.Visible);
			}
		}

		public void TestControlVisibliityForACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				AssertEquals("PSC For ACS", false, control.PSCCheckBox.Visible);
				AssertEquals("AllocateEntryNumber button For ACS", true, control.AllocateImportEntryNumberButton.Visible);
				AssertEquals("AccLiqCheckBox", false, control.AccLiqCheckBox.Visible);
				AssertEquals("Enable 7501", control.EnableENSCheckBox.Text);
				AssertEquals("Enable 3461", control.US_EnableCRLCheckBox.Text);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				AssertEquals("PSC For ACE", true, control.PSCCheckBox.Visible);
				AssertEquals("AllocateEntryNumber button For ACE", true, control.AllocateImportEntryNumberButton.Visible);
				AssertEquals("AccLiqCheckBox", false, control.AccLiqCheckBox.Visible);
				declaration.US_PSC = true;
				AssertEquals("AllocateEntryNumber button For ACE PSC", false, control.AllocateImportEntryNumberButton.Visible);
				AssertEquals("AccLiqCheckBox", true, control.AccLiqCheckBox.Visible);
				AssertEquals("Enable Ent Sum", control.EnableENSCheckBox.Text);
				AssertEquals("Enable Cargo Rel", control.US_EnableCRLCheckBox.Text);
			}
		}

		public void TestServiceLebelVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				AssertEquals("JE_RS_NKServiceLevelBoundFindBox should be invisible.", false, control.FindSingle<ZCodeFindBox>("JE_RS_NKServiceLevelBoundFindBox").Visible);
			}
		}

		public void TestTIBMotorVehicleContolsVisibilityWhenTIBEntry()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "AUTO");
			Factory.Save();

			var tariff35 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98130035", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff35.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			var tariff75 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "98130075", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff75.PK, "", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			#endregion

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				AssertEquals("No TIB entry", false, control.TIBMotorVehiclesDropEdit.Visible);
				AssertEquals("should be visible whenTIB Motor Vehiels is checked on", false, control.TIBMVNonConformingCheckBox.Visible);

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_SupTariff = "98130075";

				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
				AssertEquals("TIB entry", true, control.TIBMotorVehiclesDropEdit.Visible);

				declaration.US_TIBMotorVehicles = String.Empty;
				control.TIBMotorVehiclesDropEdit.SelectItem("");
				AssertEquals("TIBMVNonConforming should be visible whenTIB Motor Vehiels is checked on", false, control.TIBMVNonConformingCheckBox.Visible);
				declaration.US_TIBMotorVehicles = YesNoList.Codes.No;
				control.TIBMotorVehiclesDropEdit.SelectItem(YesNoList.Codes.No);
				AssertEquals("TIBMVNonConforming should be visible whenTIB Motor Vehiels is checked on", false, control.TIBMVNonConformingCheckBox.Visible);

				control.TIBMotorVehiclesDropEdit.SelectItem(YesNoList.Codes.Yes);
				declaration.US_TIBMotorVehicles = YesNoList.Codes.Yes;
				AssertEquals("TIBMVNonConformingCheckBox should be visible", true, control.TIBMVNonConformingCheckBox.Visible);
			}
		}

		public void TestControlsVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				Assert("Journey text box should not be visible - only for Rail", !control.JourneyTextBox.Visible);
				Assert("Carrier Name text box should not be visible - only for Rail or Truck", !control.CarrierNameTextBox.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				Assert(control.FTZTextBox.Visible);
				Assert(control.MasterBillForFTZTextBox.Visible);
				Assert(!control.JE_MasterBillForSeaBoundTextBox.Visible);
				Assert(control.DestinationStateDropEdit.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				Assert(!control.FTZTextBox.Visible);
				Assert(!control.MasterBillForFTZTextBox.Visible);
				Assert(control.JE_MasterBillForSeaBoundTextBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				Assert(!control.FTZTextBox.Visible);
				Assert(!control.MasterBillForFTZTextBox.Visible);
				Assert(control.JE_MasterBillForSeaBoundTextBox.Visible);
				Assert(!control.DestinationStateDropEdit.Visible);
				Assert(!control.JE_ContainerCountCalcEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
				AssertEquals("TransportDetailsGroupBox", true, control.TransportDetailsGroupBox.Visible);
				AssertEquals("ShipmentDetailsGroupBox", true, control.ShipmentDetailsGroupBox.Visible);
				AssertEquals("ContainerModeBoundDropDownEdit", true, control.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals(true, control.SupplierOrganisationControl.Visible);
				AssertEquals(true, control.ImporterOrganisationControl.Visible);
				AssertEquals("IsExport", false, declaration.IsExport);
				AssertEquals("Docs should be shown", true, control.DocsTabPageInternal.TabVisible);
				AssertEquals("Orders should not be disposed", false, control.OrdersTabPageInternal.IsDisposed);
				AssertEquals("Shipment Custom Fields should be shown", true, control.ShipmentCustomFieldsPage.TabVisible);
				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("IsExport", true, declaration.IsExport);
				AssertEquals("Docs should be shown", true, control.DocsTabPageInternal.TabVisible);
				AssertEquals("Orders should not be disposed", false, control.OrdersTabPageInternal.IsDisposed);
				AssertEquals("Shipment Custom Fields should be shown", true, control.ShipmentCustomFieldsPage.TabVisible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(!control.JE_MasterBillForSeaBoundTextBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(control.JE_MasterBillForSeaBoundTextBox.Visible);
				Assert("Journey text box should not be visible - only for Rail", !control.JourneyTextBox.Visible);
				Assert("Carrier Name text box should not be visible - only for Rail or Truck", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				Assert("Journey text box should not be visible - only for Rail", !control.JourneyTextBox.Visible);
				AssertEquals("TIB Panel should not be visible", false, control.TIBPanel.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				AssertEquals("TIB Panel should be visible for TIB entry type", true, control.TIBPanel.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = ZString.Empty;
				Assert("Carrier Name text box should not be visible if carrierScac is not UNKN", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = "APLU";
				Assert("Carrier Name text box should not be visible if carrierScac is not UNKN", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_UI_NKCarrierSCAC = "UNKN";
				Assert("Carrier Name text box should not be visible if messagetype is not export", !control.CarrierNameTextBox.Visible);
				declaration.US_EnableENS = false;
				declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = "UNKN";
				Assert("Carrier Name text box should be visible if Truck transport mode", control.CarrierNameTextBox.Visible);
				// making sure that change of shiping line triggers update
				declaration.JE_OH_ShippingLine = Factory.New<OrgHeader>().PK;
				AssertEquals("Carrier Name text box should not be visible if there is a shipping line", false, control.CarrierNameTextBox.Visible);
				declaration.JE_OH_ShippingLine = ZGuid.Empty;
				AssertEquals("Carrier Name text box should be visible if Truck transport mode", true, control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("TIB Panel should not be visible", false, control.TIBPanel.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = ZString.Empty;
				Assert("Carrier Name text box should not be visible if carrierScac is not UNKN", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = "APLU";
				Assert("Carrier Name text box should not be visible if carrierScac is not UNKN", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_UI_NKCarrierSCAC = "UNKN";
				Assert("Carrier Name text box should not be visible if messagetype is not export", !control.CarrierNameTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_UI_NKCarrierSCAC = "UNKN";
				Assert("Carrier Name text box should be visible if Rail transport mode", control.CarrierNameTextBox.Visible);
				// making sure that change of transport mode triggers update
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Journey text box should not be visible for SEA", false, control.JourneyTextBox.Visible);
			}

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				AssertEquals("IsExport", false, declaration.IsExport);
				AssertEquals("Docs should be shown", true, control.DocsTabPageInternal.TabVisible);
				AssertEquals("Orders should not be disposed", true, control.OrdersTabPageInternal.IsDisposed);
				AssertEquals("Shipment Custom Fields should be shown", true, control.ShipmentCustomFieldsPage.TabVisible);
				control.RightTabControl.SelectedTab = control.OrganisationsTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("IsExport", true, declaration.IsExport);
				AssertEquals("Docs should not be shown", false, control.DocsTabPageInternal.TabVisible);
				AssertEquals("Orders should be disposed", true, control.OrdersTabPageInternal.IsDisposed);
				AssertEquals("Shipment Custom Fields should be shown", true, control.ShipmentCustomFieldsPage.TabVisible);
			}
		}

		public void TestResizeRightTabControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				form.Show();
				foreach (CodeDescriptionPair jobMessageType in declaration.Lookups.MessageTypeList)
				{
					declaration.JE_MessageType = jobMessageType.Code;
					if (jobMessageType.Code == JobMessageTypeList.Codes.Export)
					{
						AssertEquals("RightTabControl should not overlap DeclarationDetailsGroupBox when declaration type is Export", control.DeclarationDetailsGroupBox.Top, control.RightTabControl.Bottom);
					}
					else
					{
						AssertEquals("RightTabControl should overlaps DeclarationDetailsGroupBox when declaration type is not Export", control.ShipmentTypeGroupBox.Bottom, control.RightTabControl.Bottom);
					}
				}
			}
		}

		public void TestBondedWarehouseDocAddressControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals(true, control.BondedWarehouseDocAddressControl.Visible);
			}
		}

		public void TestBox29Button_Click()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				USJobDeclarationUserControl control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				AssertEquals(false, control.Box29Button.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				AssertEquals(true, control.Box29Button.Visible);
				control.Box29Button.PerformClick();
				Form activeForm = ZFormModaliser.ActiveForm;
				AssertEquals("Box29 form is shown", typeof(Box29Form), activeForm.GetType());
			}
		}

		public void TestMessageModeControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Message Mode should be invisible as it is export", false, userControl.ApplicationCodeDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Message Mode should be visible as it is import", true, userControl.ApplicationCodeDropEdit.Visible);
			}
		}

		public void TestJE_ApplicationCodeBoundDropEditVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				var applicationCodeBoundDropEdit = userControl.FindSingleOrDefault<ZDropEdit>("JE_ApplicationCodeBoundDropEdit");
				AssertEquals("JE_ApplicationCodeBoundDropEdit should be visible as it is export", true, applicationCodeBoundDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("JE_ApplicationCodeBoundDropEdit should be invisible as it is import", false, applicationCodeBoundDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals("JE_ApplicationCodeBoundDropEdit should be visible as it is FTZ", true, applicationCodeBoundDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
				AssertEquals("JE_ApplicationCodeBoundDropEdit should be invisible as it is Miscellaneous", false, applicationCodeBoundDropEdit.Visible);
			}
		}

		public void TestVisibleChangedWithDeletedDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var control = form.CustomsBrokerageUserControl.DeclarationUserControlForTesting as USJobDeclarationUserControl;
				form.Show();
				control.Visible = false;
				Assert("control.JobDeclaration is not deleted", !control.USJobDeclaration.IsDeleted);
				declaration.Delete();
				if (ErrorReporter.LastKeyReported == "Declaration should not be deleted")
				{
					ErrorReporter.Clear();
				}

				Assert("control.JobDeclaration is deleted", control.USJobDeclaration.IsDeleted);
				Assert("control.Visible before changed", !control.Visible);
				control.Visible = true;
				Assert("control.Visible after changed", control.Visible);
			}
		}

		public void TestChangeFixedTransportModeControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.US_EnableCRL = true;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Pipeline Name should be visible", true, userControl.PipelineNameTextBox.Visible);
				AssertEquals("Batch/Ticket No. should be visible", true, userControl.BatchTicketTextBox.Visible);
				AssertEquals("Non-AMS checkbox should be visible", true, userControl.NonAMSCheckBox.Visible);
				declaration.US_EntryType = "06";
				AssertEquals("Batch/Ticket No. should be not visible", false, userControl.BatchTicketTextBox.Visible);
				declaration.US_EntryType = "01";
				AssertEquals("Batch/Ticket No. should be visible", true, userControl.BatchTicketTextBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Pipeline Name should be invisible", false, userControl.PipelineNameTextBox.Visible);
				AssertEquals("Batch/Ticket No. should be invisible", false, userControl.BatchTicketTextBox.Visible);
				AssertEquals("Non-AMS checkbox should be visible", true, userControl.NonAMSCheckBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				AssertEquals("Non-AMS checkbox should be invisible", false, userControl.NonAMSCheckBox.Visible);
			}
		}

		public void TestFTZStandAlonePriorNoticeControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Stand Alone Prior Notice ID Type should be invisible", false, userControl.FTZSPNIDTypeDropEdit.Visible);
				declaration.US_EnableSPN = true;
				declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.O;
				AssertEquals("Stand Alone Prior Notice ID Type should be invisible", false, userControl.FTZSPNIDTypeDropEdit.Visible);
				declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
				AssertEquals("Stand Alone Prior Notice ID Type should be visible", true, userControl.FTZSPNIDTypeDropEdit.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Stand Alone Prior Notice ID Type should be invisible", false, userControl.FTZSPNIDTypeDropEdit.Visible);
			}
		}

		public void TestShowOrHideOrganizationTabPage()
		{
			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "IMPORTER";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerOrg.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Import organizations tab page should be visible", true, userControl.USOrganisationsTabPage.TabVisible);
				AssertEquals("Export organizations tab page should be invisible", false, userControl.OrganisationsTabPage.TabVisible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Import organizations tab page should be invisible", false, userControl.USOrganisationsTabPage.TabVisible);
				AssertEquals("Export organizations tab page should be visible", true, userControl.OrganisationsTabPage.TabVisible);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Import organizations tab page should be visible", true, userControl.USOrganisationsTabPage.TabVisible);
				AssertEquals("Export organizations tab page should be invisible", false, userControl.OrganisationsTabPage.TabVisible);
			}
		}

		public void TestFTZAllocateButtonVisibility()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var stmNums = Factory.New<OrganisationViewStmNums>();
			stmNums.SN_Type = OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber;
			stmNums.SN_ZoneIDPrefix = "111DD22";
			stmNums.SN_ClientPrefix = "AAA";
			stmNums.SN_Owner = orgHeader.PK;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.IOROrgPK = orgHeader.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals(false, userControl.AllocateButton.Visible);
				declaration.FTZZoneID = "111DD22";
				AssertEquals(true, userControl.AllocateButton.Visible);
				declaration.IOROrgPK = ZGuid.Empty;
				AssertEquals(false, userControl.AllocateButton.Visible);
				declaration.IOROrgPK = orgHeader.PK;
				AssertEquals(true, userControl.AllocateButton.Visible);
				declaration.FTZControlNumber = "AAA00112";
				AssertEquals(true, userControl.AllocateButton.Visible);
				declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;
				AssertEquals(true, userControl.AllocateButton.Visible);
			}
		}

		public void TestExpressTrackingUserControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals(true, userControl.ExpressTrackingCheckBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals(false, userControl.ExpressTrackingCheckBox.Visible);
				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals(true, userControl.ExpressTrackingCheckBox.Visible);
			}
		}

		public void TestInsuranceValueControlVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("EXP, Insurance Value is invisible", false, userControl.InsuranceValueCalcFindBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("IMP, Insurance Value is visible", true, userControl.InsuranceValueCalcFindBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals("FTZ, Insurance Value is visible", true, userControl.InsuranceValueCalcFindBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Switch to EXP, Insurance Value is invisible", false, userControl.InsuranceValueCalcFindBox.Visible);
			}

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("S file, EXP, Insurance Value is invisible", false, userControl.InsuranceValueCalcFindBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("S file, IMP, Insurance Value is invisible", false, userControl.InsuranceValueCalcFindBox.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				AssertEquals("S file, FTZ, Insurance Value is invisible", false, userControl.InsuranceValueCalcFindBox.Visible);
			}
		}

		public void TestScreeningControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Screening status is visible", true, userControl.ScreeningStatusDropEdit.Visible);
				AssertEquals("Screening button is visible", true, userControl.ScreenButton.Visible);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				AssertEquals("Screening status is not visible", false, userControl.ScreeningStatusDropEdit.Visible);
				AssertEquals("Screening button is not visible", false, userControl.ScreenButton.Visible);
			}
		}

		public void TestScreeningControlsVisibleWithCompliance()
		{
			AssertScreeningControlsVisible(true, false);
			AssertScreeningControlsVisible(false, true);

			void AssertScreeningControlsVisible(bool enableCompliance, bool expectedVisible)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				var complianceWiseFeatureRule = new ComplianceRiskFeatureControlRule { Enabled = true };
				var featureDataMock = new Mock<IFeatureData>();
				var featureControlMock = new Mock<IFeatureControlManager>();
				featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out complianceWiseFeatureRule)).Returns(enableCompliance);
				featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.ComplianceWiseCustomsDeclarationModule, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
				using (ObjectFactory.Substitute(featureControlMock.Object))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
					AssertEquals(expectedVisible, userControl.ScreeningStatusDropEdit.Visible);
					AssertEquals(expectedVisible, userControl.ScreenButton.Visible);
				}
			}
		}

		public void TestScreeningControlsAreHidden_WhenNotIsDrawback_AndHasShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = (USJobDeclarationUserControl)form.CustomsBrokerageUserControl.DeclarationUserControlForTesting;
				AssertEquals("Screening status is not visible", false, userControl.ScreeningStatusDropEdit.Visible);
				AssertEquals("Screening button is not visible", false, userControl.ScreenButton.Visible);
			}
		}

		protected override IEnumerable<Action> SetupDeclarationForTestScenarios(BaseJobDeclaration declaration)
		{
			yield return () => declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		void AssertShowOrHideExportControls(USJobDeclarationUserControl control, bool isVisible)
		{
			AssertEquals("InbondTypeDropEdit.Visible", isVisible, control.InbondTypeDropEdit.Visible);
			AssertEquals("ImportEntryNoTextBox.Visible", isVisible, control.ImportEntryNoTextBox.Visible);
			AssertEquals("InbondTypeDropEdit.Visible", isVisible, control.InbondTypeDropEdit.Visible);
			AssertEquals("ImportEntryNoTextBox.Visible", isVisible, control.ImportEntryNoTextBox.Visible);
			AssertEquals("TransportReferenceTextBox.Visible", isVisible, control.TransportReferenceTextBox.Visible);
			AssertEquals("ForeignTradeZoneTextBox.Visible", isVisible, control.ForeignTradeZoneTextBox.Visible);
			AssertEquals("StateOfOriginDropEdit.Visible", isVisible, control.StateOfOriginDropEdit.Visible);
			AssertEquals("ECCNTextBox.Visible", isVisible, control.ECCNTextBox.Visible);
			AssertEquals("LicenseNoTextBox.Visible", isVisible, control.LicenseNoTextBox.Visible);
			AssertEquals("LicenseTypeDropEdit.Visible", isVisible, control.LicenseTypeCodeFindBox.Visible);
			AssertEquals("ExportCodeDropEdit.Visible", isVisible, control.ExportCodeDropEdit.Visible);
			AssertEquals("PortOfExportPanel.Visible", isVisible, control.PortOfExportPanel.Visible);
			AssertEquals("PortOfExportCodeFindBox.Visible", isVisible, control.PortOfExportCodeFindBox.Visible);
			AssertEquals("DateOfExportDateEdit.Visible", isVisible, control.DateOfExportDateEdit.Visible);
			Assert(control.IncoTermDropEdit.Visible);
			Assert(control.IncoTermExplainButton.Visible);
		}

		void AssertShowOrHideNonExportControls(USJobDeclarationUserControl control, bool isVisible)
		{
			AssertEquals("EntryTypeDropEdit.Visible", isVisible, control.EntryTypeDropEdit.Visible);
			AssertEquals("MasterBillIssuerGuidFindBox", isVisible, control.MasterBillIssuerSCACFindBox.Visible);
			AssertEquals("ImportShipmentTypePanel", isVisible, control.ImportShipmentTypePanel.Visible);
			AssertEquals("ConsolidatedSummaryCheckBox", isVisible, control.ConsolidatedSummaryCheckBox.Visible);
			AssertEquals("house bill issuer", isVisible, control.HouseBillIssuerSCACFindBox.Visible);
			AssertEquals("PrimaryITNumberTextBox", isVisible, control.PrimaryITNumberTextBox.Visible);
			AssertEquals("ITDateDateEdit", isVisible, control.ITDateDateEdit.Visible);
			Assert(control.IncoTermDropEdit.Visible);
			Assert(control.IncoTermExplainButton.Visible);
		}
	}
}
