using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Manifest.GUI.Testing
{
	class ZAManifestCountrySpecificUserControlTest : TestCaseWithFactory
	{
		public void TestCaseNumberGridColumns()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			manifest.AMA_Nature = ShipmentTypeList.Codes.Export22;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.RFM);

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				var grid = asycudaManifestUserControl.Controls.Find("CaseNumberGrid", true)[0] as ZArchitecture.ZGrid;

				var columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Code);
				Assert("Case Type IsUnavailable", !columnStyle.IsUnavailable);
				columnStyle = grid.GetColumnStyle(CaseNumber.Schema.Description);
				Assert("Description IsUnavailable", !columnStyle.IsUnavailable);
				columnStyle = grid.GetColumnStyle(CaseNumber.Schema.CY_Data);
				Assert("Case Number IsUnavailable", !columnStyle.IsUnavailable);
			}
		}

		[TestDate(2019, 12, 03, 00, 00, 01)]
		public void TestCaseNumberVisibility()
		{
			#region RefData:

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCusCodeType("MFDOC", "Manifest Document Type Code");

			var docTypeCOM =
				testHelper.CreateCusCodeList(countryCode, "MFDOC", "COM", "Container Master", startDate, endDate);
			var docTypeCOH =
				testHelper.CreateCusCodeList(countryCode, "MFDOC", "COH", "Container House", startDate, endDate);
			var docTypeFWB =
				testHelper.CreateCusCodeList(countryCode, "MFDOC", "FWB", "Master Air Waybill", startDate, endDate);
			var docTypeHAB =
				testHelper.CreateCusCodeList(countryCode, "MFDOC", "HAB", "House Air Waybill", startDate, endDate);

			testHelper.CreateCusCodeListAttribute(docTypeCOM.PK, "BillLevelMessage", "false");
			testHelper.CreateCusCodeListAttribute(docTypeCOH.PK, "BillLevelMessage", "true");
			testHelper.CreateCusCodeListAttribute(docTypeFWB.PK, "BillLevelMessage", "false");
			testHelper.CreateCusCodeListAttribute(docTypeHAB.PK, "BillLevelMessage", "true");

			Factory.Save();

			#endregion RefData.

			#region Setup Test Scenarios:

			var effectiveDate = new ZDateTime(2019, 12, 01);
			var scenarios = new List<CaseNumberVisibilityTestScenario>
			{
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COM), ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Sea, effectiveDate, true),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COH), ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Sea, effectiveDate, false),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.FWB), ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Air, effectiveDate, true),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.HAB), ApplicationCodeTypeList.Codes.Consolidator, Core.Constants.TransportModes.Air, effectiveDate, false),
				new CaseNumberVisibilityTestScenario(nameof(ManifestDocumentType.COM), ApplicationCodeTypeList.Codes.ShippingLine, Core.Constants.TransportModes.Sea, new ZDateTime(2019, 12, 13), false)
			};

			#endregion Setup Test Scenarios.

			Action<CaseNumberVisibilityTestScenario> testAction = scenario =>
			{
				bool zaManifestCaseNumbersFeatureIsActive = true;
				if (!scenario.FunctionalityStartDate.Equals(effectiveDate))
				{
					zaManifestCaseNumbersFeatureIsActive = false;
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers,
					Core.Constants.CountryCodes.SouthAfrica, new ZDate(2019, 12, 03), zaManifestCaseNumbersFeatureIsActive))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var manifest = Factory.New<Business.AsycudaManifestHeader>();
					manifest.AMA_RN_NKCountry = countryCode;
					manifest.AMA_ApplicationCode = scenario.ManifestStyle;
					manifest.AMA_ManifestType = scenario.ManifestDocumentType;
					manifest.AMA_TransportMode = scenario.TransportMode;
					manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
					using (var form = new ManifestForm(manifest))
					{
						form.Show();
						form.Update();
						var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

						var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
						var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
						mainTabControl.SelectedTab = mainTabPage;
						var caseNumberControl = asycudaManifestUserControl.FindSingle<ZGroupBox>(nameof(ZAManifestControlBag.CaseNumberManifestHeaderGroupBox));

						var assertMsg = "Check Visibility of CaseNumberGroupBox for manifest type " + scenario.ManifestDocumentType;
						var expectedValue = scenario.ExpectedToBeVisible;
						AssertEquals(assertMsg, expectedValue, caseNumberControl.Visible);
					}
				}
			};
			scenarios.ForEach(testAction);
		}

		public void TestVesselCodeFindBox()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				AssertNotNull("VesselCodeFindBox", asycudaManifestUserControl.Controls.Find("VesselCodeFindBox", true)[0] as ZCodeFindBox);
				var vesselCodeFindBox = asycudaManifestUserControl.Controls.Find("VesselCodeFindBox", true)[0] as ZCodeFindBox;
				AssertType<ZCodeFindBox>("VesselCodeFindBox", vesselCodeFindBox);
				AssertEquals("VesselCodeFindBox Enabled", true, vesselCodeFindBox.Enabled);
				AssertEquals("VesselCodeFindBox ShowDescriptionBox", true, vesselCodeFindBox.Enabled);
				AssertEquals("VesselCodeFindBox PreBoundMaxLength", 35, vesselCodeFindBox.PreBoundMaxLength);
			}
		}

		public void TestRadioCallSignCodeFindBox()
		{
			var manifest = Factory.New<Business.AsycudaManifestHeader>();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.ALH);

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var mainTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "mainTabPage");
				mainTabControl.SelectedTab = mainTabPage;

				AssertNotNull("RadioCallSignCodeFindBox", asycudaManifestUserControl.Controls.Find("RadioCallSignCodeFindBox", true)[0] as ZCodeFindBox);
				var radioCallSignCodeFindBox = asycudaManifestUserControl.Controls.Find("RadioCallSignCodeFindBox", true)[0] as ZCodeFindBox;
				AssertType<ZCodeFindBox>("RadioCallSignCodeFindBox", radioCallSignCodeFindBox);
				AssertEquals("RadioCallSignCodeFindBox Enabled", true, radioCallSignCodeFindBox.Enabled);
				AssertEquals("RadioCallSignCodeFindBox ShowDescriptionBox", false, radioCallSignCodeFindBox.ShowDescriptionBox);
				AssertEquals("RadioCallSignCodeFindBox PreBoundMaxLength", 10, radioCallSignCodeFindBox.PreBoundMaxLength);
			}
		}
	}
}
