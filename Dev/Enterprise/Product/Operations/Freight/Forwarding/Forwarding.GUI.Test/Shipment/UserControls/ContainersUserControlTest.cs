using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal class ContainersUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestWhenCustomAttributesAreNotUsed()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "";

			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControl testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);

				testUserControl.SetDataBinding(shipment, "");

				testForm.Show();
			}
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestWhenCustomAttributesAreUsed()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Col1";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "Col2";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "Col3";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption = "Col4";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption = "Col5";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption = "Col6";
			Env.Registry.Freight.PackLine.PackLineCustomDate1Caption = "Col7";
			Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption = "Col8";

			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControl testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);

				testUserControl.SetDataBinding(shipment, "");

				testForm.Show();
			}
		}

		[ExpectNoExceptions()]
		[RequiresSTA]
		public void TestCustomFieldsAttributesHintIsSet()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Hint = "Hint1";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Hint = "Hint2";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Hint = "Hint3";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute4Hint = "Hint4";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal1Hint = "Hint5";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Hint = "Hint6";
			Env.Registry.Freight.PackLine.PackLineCustomFlag1Hint = "Hint7";
			Env.Registry.Freight.PackLine.PackLineCustomFlag2Hint = "Hint8";
			Env.Registry.Freight.PackLine.PackLineCustomDate1Hint = "Hint9";
			Env.Registry.Freight.PackLine.PackLineCustomDate2Hint = "Hint10";
			var shipment = Factory.New<ForwardingShipment>();
			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControl testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);
				testUserControl.SetDataBinding(shipment, "");
				testForm.Show();
				AssertEquals("Hint1", ((testUserControl.Controls.Find("JL_CustomAttrib1TextBox", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint2", ((testUserControl.Controls.Find("JL_CustomAttrib2TextBox", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint3", ((testUserControl.Controls.Find("JL_CustomAttrib3TextBox", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint4", ((testUserControl.Controls.Find("JL_CustomAttrib4TextBox", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint5", ((testUserControl.Controls.Find("JL_CustomDecimal1CalcEdit", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint6", ((testUserControl.Controls.Find("JL_CustomDecimal2", true)[0]) as ZTextBox).CaptionResourceString.Caption);
				AssertEquals("Hint7", ((testUserControl.Controls.Find("JL_CustomFlag1CheckBox", true)[0]) as ZCheckBox).CaptionResourceString.Caption);
				AssertEquals("Hint8", ((testUserControl.Controls.Find("JL_CustomFlag2CheckBox", true)[0]) as ZCheckBox).CaptionResourceString.Caption);
				AssertEquals("Hint9", ((testUserControl.Controls.Find("CustomDate1DateEdit", true)[0]) as ZDateEdit).CaptionResourceString.Caption);
				AssertEquals("Hint10", ((testUserControl.Controls.Find("CustomDate2DateEdit", true)[0]) as ZDateEdit).CaptionResourceString.Caption);
			}
		}

		public void TestCustomCaptions()
		{
			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "Zayden";
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControl testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);
				testUserControl.SetDataBinding(shipment, "");
				testForm.Show();

				AssertEquals("Overidden", "Zayden", testUserControl.Controls.Find("JL_CustomAttrib1TextBox", true)[0].GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Not Overridden", "Custom Text 2", testUserControl.Controls.Find("JL_CustomAttrib2TextBox", true)[0].GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		[RequiresSTA]
		public void TestBindToListOfDGContactGuidFindBox()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (var testForm = new ZForm(shipment))
			{
				var testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);
				testUserControl.SetDataBinding(shipment, "");
				testForm.Show();

				AssertEquals("OuterPackLines.UNDGs+Contacts", ((ZGuidFindBox)testUserControl.Controls.Find("DGContactGuidFindBox", true)[0]).BindToList);
			}
		}

		public void TestSetNMFCVisibility()
		{
			string columnName = "CommodityCode+RH_FN_NKNMFC";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			ContainersUserControlForTest containerUserControl = null;
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (ZForm testForm = new ZForm(shipment))
				{
					containerUserControl = new ContainersUserControlForTest();
					testForm.Controls.Add(containerUserControl);
					containerUserControl.SetDataBinding(shipment, "");
					testForm.Show();

					Assert(containerUserControl.JobPackLinesGrid.GetColumnStyle(columnName).IsUnavailable);

					shipment.JS_TransportMode = Core.Constants.TransportModes.Road;

					Assert(containerUserControl.JobPackLinesGrid.GetColumnStyle(columnName).IsUnavailable);
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

				using (ZForm testForm = new ZForm(shipment))
				{
					containerUserControl = new ContainersUserControlForTest();
					testForm.Controls.Add(containerUserControl);
					containerUserControl.SetDataBinding(shipment, "");
					testForm.Show();

					Assert(containerUserControl.JobPackLinesGrid.GetColumnStyle(columnName).IsUnavailable);

					shipment.JS_TransportMode = Core.Constants.TransportModes.Road;

					Assert(!containerUserControl.JobPackLinesGrid.GetColumnStyle(columnName).IsUnavailable);
					Assert(containerUserControl.JobPackLinesGrid.GetColumnStyle(columnName).IsVisible);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(country);
			}
		}

		public void TestSetImperialWeightAndVolumeVisibility()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControlForTest containerUserControl = new ContainersUserControlForTest();
				testForm.Controls.Add(containerUserControl);
				containerUserControl.SetDataBinding(shipment, "");
				shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
				shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

				Env.Registry.FreightWeightUnit = "KG";
				Env.Registry.FreightVolumeUnit = "M3";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "AUBNE";

				testForm.Show();
				AssertEquals("Imperial Weight should not be visible", containerUserControl.JS_ActualWeight_ImperialTextBox.Visible, false);

				shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
				testForm.Show();
				AssertEquals("Imperial Weight should be visible", containerUserControl.JS_ActualWeight_ImperialTextBox.Visible, true);

				shipment.JS_UnitOfWeight = Core.Constants.Weight.MetricCarat;
				testForm.Show();
				AssertEquals("Imperial Weight should be visible", containerUserControl.JS_ActualWeight_ImperialTextBox.Visible, false);
			}
		}

		[RequiresSTA]
		public void TestSetLoadingMetersVisibility()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.New<ForwardingShipment>();
			using (ZForm form = new ZForm(shipment))
			{
				ContainersUserControlForTest containerControl = new ContainersUserControlForTest();
				form.Controls.Add(containerControl);
				containerControl.SetDataBinding(shipment, "");

				form.Show();

				shipment.JS_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);
				AssertEquals(true, containerControl.JL_LoadingMetersCalcDropEdit.Visible);

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);
				AssertEquals(false, containerControl.JL_LoadingMetersCalcDropEdit.Visible);
			}
		}

		public void TestColumnsStyles()
		{
			Enterprise.Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (ContainersUserControlForTest control = new ContainersUserControlForTest())
			{
				Assert("Column LocationWhsGuid should be in grid", CheckColumnIsInGrid(control.LocationGrid, "LocationWhsGuid"));
				Assert("Column LocationString should be in grid", CheckColumnIsInGrid(control.LocationGrid, "LocationString"));
				Assert("Column JQ_WarehouseLocation should not be in grid", !CheckColumnIsInGrid(control.LocationGrid, PackLocation.Schema.JQ_WarehouseLocation));
			}

			Enterprise.Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (ContainersUserControlForTest control = new ContainersUserControlForTest())
			{
				Assert("Column LocationWhsGuid should not be in grid", !CheckColumnIsInGrid(control.LocationGrid, "LocationWhsGuid"));
				Assert("Column LocationString should not be in grid", !CheckColumnIsInGrid(control.LocationGrid, "LocationString"));
				Assert("Column JQ_WarehouseLocation should not be in grid", CheckColumnIsInGrid(control.LocationGrid, PackLocation.Schema.JQ_WarehouseLocation));
			}
		}

		public void TestSetRefNumberColumnHeaderTextAndCaption()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControlForTest containerUserControl = new ContainersUserControlForTest();
				testForm.Controls.Add(containerUserControl);
				containerUserControl.SetDataBinding(shipment, "");

				Assert("Column RefNumber should be in grid", CheckColumnIsInGrid(containerUserControl.JobPackLinesGrid, "JL_RefNumber"));

				testForm.Show();

				var columns = containerUserControl.JobPackLinesGrid.Columns;
				AssertEquals("HeaderText under FCL mode", "Ref Number", columns["JL_RefNumber"].ColumnStyle.HeaderText);

				var textControlCollection = containerUserControl.Controls.Find("zTextBox1", true);
				AssertEquals("zTextBox1 should be found", 1, textControlCollection.Length);
				var captionRender = ((ZTextBox)textControlCollection[0]).GetExtension<ILabelCaptionRenderer>();
				AssertEquals("zTextBox1 caption under FCL mode", "Ref Number", captionRender.Caption);

				shipment.JS_PackingMode = "ROR";
				AssertEquals("HeaderText under ROR mode", "VIN", columns["JL_RefNumber"].ColumnStyle.HeaderText);
				AssertEquals("zTextBox1 caption under ROR mode", "VIN", captionRender.Caption);
			}
		}

		public void TestHSCodeFindBoxUseCustomsCountryOfJurisdiction()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			using (ZForm form = new ZForm(shipment))
			{
				ContainersUserControlForTest containerControl = new ContainersUserControlForTest();
				form.Controls.Add(containerControl);
				containerControl.SetDataBinding(shipment, "");

				form.Show();

				var outPackLine = shipment.OuterPackLines.AddNew();
				outPackLine.JL_PackageCount = 1;

				containerControl.HSCountryFindBox.CodeBox.Text = Core.Constants.CountryCodes.Liechtenstein;
				var codeFindBox = containerControl.HSCodeFindBox;
				AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Liechtenstein, codeFindBox.GetCountryCode());
				AssertEquals("GetDataGrouping", Core.Constants.CountryCodes.Switzerland, codeFindBox.GetDataGrouping());
			}
		}
		[RequiresSTA]
		public void TestNoExceptionOnTariffFindBox()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			using (ZForm form = new ZForm(shipment))
			{
				ContainersUserControlForTest containerControl = new ContainersUserControlForTest();
				form.Controls.Add(containerControl);
				containerControl.SetDataBinding(shipment, "");

				form.Show();

				var outPackLine = shipment.OuterPackLines.AddNew();
				outPackLine.JL_PackageCount = 1;

				containerControl.HSCountryFindBox.CodeBox.Text = Core.Constants.CountryCodes.SouthAfrica;
				containerControl.HSCodeFindBox.CodeBox.Text = "A";
				containerControl.HSCodeFindBox.ErrorForUnsupportedCountry = ZString.Empty;

				containerControl.JobPackLinesGrid.SelectSingleElement(outPackLine);
				Assert(containerControl.HSCodeFindBox.Enabled);

				AssertNoExceptionThrown(containerControl.HSCodeFindBox.PopupButton.PerformClick);
			}
		}

		[RequiresSTA]
		public void TestVehicleColumnsByPackingMode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			using (ZForm testForm = new ZForm(shipment))
			{
				ContainersUserControlForTest containerUserControl = new ContainersUserControlForTest();
				testForm.Controls.Add(containerUserControl);
				containerUserControl.SetDataBinding(shipment, "");
				testForm.Show();

				var vehicleColumnList = new string[]
				{
					AutoJobPackLines.Schema.JL_VehicleMake,
					AutoJobPackLines.Schema.JL_VehicleModel,
					AutoJobPackLines.Schema.JL_VehicleYear,
					AutoJobPackLines.Schema.JL_VehicleColor,
					AutoJobPackLines.Schema.JL_VehicleNumberOfDoors,
					AutoJobPackLines.Schema.JL_VehicleTransmission
				};

				foreach (var vehicleColumn in vehicleColumnList)
				{
					Assert("Vehicle Column should be not availabe in grid", !CheckColumnAvailableInGrid(containerUserControl.JobPackLinesGrid, vehicleColumn));
				}

				shipment.JS_PackingMode = "ROR";
				foreach (var vehicleColumn in vehicleColumnList)
				{
					Assert("Vehicle Column should be available in grid", CheckColumnAvailableInGrid(containerUserControl.JobPackLinesGrid, vehicleColumn));
				}
			}
		}

		public void TestFlashPointControlIMO_IsNotReadonly_WhenDIIsCombustible()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var packline = shipment.OuterPackLines.AddNew();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var imoSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a").FirstOrDefault();
			imoSubstance.DG_FlashPoint = "23 c.c";
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var dg = packline.UNDGs.AddNew();
			dg.DI_DG = subs.PK;

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				dg.DI_IsCombustible = false;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("DIIsCombustible is false so should be readonly regardless of the standard type code is IMO", flashPointCalcEdit.ReadOnly);
				}
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("DIIsCombustible so should be editable regardless of the standard type code is IMO", !flashPointCalcEdit.ReadOnly);
				}
			}
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("Substance attached so should be readonly regardless of the standard type code is IMO", !flashPointCalcEdit.ReadOnly);
				}
				dg.DI_DG = Guid.Empty;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("No substance attached so should be readonly regardless of the standard type code is IMO", flashPointCalcEdit.ReadOnly);
				}
			}
		}

		[RequiresSTA]
		public void TestFlashPointControlIAT_IsNotReadonlyWhenDIIsCombustible()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var packline = shipment.OuterPackLines.AddNew();

			var iatSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			iatSubstance.DG_Standard = "IAT";
			iatSubstance.DG_FlashPoint = "23 c.c";

			var dg = packline.UNDGs.AddNew();
			dg.DI_DG = iatSubstance.PK;

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				dg.DI_IsCombustible = false;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("DIIsCombustible is false so should be readonly regardless of the standard type code", flashPointCalcEdit.ReadOnly);
				}
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("DIIsCombustible so should be editable regardless of the standard type code", !flashPointCalcEdit.ReadOnly);
				}
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("Substance attached so should be readonly regardless of the standard type code", !flashPointCalcEdit.ReadOnly);
				}
				dg.DI_DG = Guid.Empty;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("No substance attached so should be readonly regardless of the standard type code", flashPointCalcEdit.ReadOnly);
				}
			}
		}

		public void TestFlashPointControl_IsNotReadonlyWhenNoFlashPointButDIIsCombustible()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			var packline = shipment.OuterPackLines.AddNew();

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var imoSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a").FirstOrDefault();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var dg = packline.UNDGs.AddNew();
			dg.DI_DG = imoSubstance.PK;

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("No flash point but DIIsCombustible substance so should be editable", !flashPointCalcEdit.ReadOnly);
				}

				dg.DI_IsCombustible = false;
				Factory.Save();

				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("Has substance but is not combustible so should be readonly", flashPointCalcEdit.ReadOnly);
				}
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				dg.DI_IsCombustible = true;
				Factory.Save();
				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("Has substance so should be editable", !flashPointCalcEdit.ReadOnly);
				}

				dg.DI_DG = Guid.Empty;
				Factory.Save();

				using (var testForm = new ZForm(shipment))
				{
					var flashPointCalcEdit = GetFlashPointControl(shipment, testForm);
					AssertNotNull("Precondition: control found", flashPointCalcEdit);
					Assert("Has no substance so should be readonly", flashPointCalcEdit.ReadOnly);
				}
			}
		}

		static ZCalcEdit GetFlashPointControl(ForwardingShipment shipment, ZForm testForm)
		{
			var containerUserControl = new ContainersUserControlForTest();
			testForm.Controls.Add(containerUserControl);
			containerUserControl.SetDataBinding(shipment, "");
			testForm.Show();
			var packLinesGrid = (ZGrid)testForm.Controls.Find("packLinesGrid", true).FirstOrDefault();
			packLinesGrid.Select(0);

			var flashPointCalcEdit = (ZCalcEdit)testForm.Controls.Find("FlashPointCalcEdit", true).FirstOrDefault();
			return flashPointCalcEdit;
		}

		[RequiresSTA]
		public void TestCountWeightAndVolumeLabelsExist()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var testForm = new ZForm(shipment))
			{
				var testUserControl = new ContainersUserControl();
				testForm.Controls.Add(testUserControl);
				testUserControl.SetDataBinding(shipment, "");
				testForm.Show();

				AssertNotNull("CountLabel should exist.", testUserControl.Controls.Find("CountLabel", true).FirstOrDefault());
				AssertNotNull("WeightLabel should exist.", testUserControl.Controls.Find("WeightLabel", true).FirstOrDefault());
				AssertNotNull("VolumeLabel should exist.", testUserControl.Controls.Find("VolumeLabel", true).FirstOrDefault());
			}
		}

		public void TestHarmonisedCodeFindBox_DecimalIsRemoved()
		{
			var expectedDescription = string.Empty;

			using (var tariffFindBox = new TariffFindBox())
			{
				tariffFindBox.GetDataGrouping = () => Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
				var listProvider = ((IFindBox)tariffFindBox).ListProvider;
				expectedDescription = listProvider.DescriptionFromCode("392051");
			}

			var shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();

			using (var testForm = new ZForm(shipment))
			{
				var containerUserControl = new ContainersUserControlForTest();
				testForm.Controls.Add(containerUserControl);
				containerUserControl.SetDataBinding(shipment, "");
				testForm.Show();
				var packLinesGrid = (ZGrid)testForm.Controls.Find("packLinesGrid", true).FirstOrDefault();
				packLinesGrid.Select(0);
				var harmonisedCodeFindBox = (TariffFindBox)testForm.Controls.Find("HarmonisedCodeFindBox", true).FirstOrDefault();
				AssertNotNull("Precondition: control found", harmonisedCodeFindBox);
				harmonisedCodeFindBox.CurrentCode = "3920.51";
				UserIdleWorker.Flush();
				AssertEquals("Description", expectedDescription, harmonisedCodeFindBox.DescriptionBox.Text);
			}
		}

		#region Implementation

		ZBool CheckColumnIsInGrid(ZGrid grid, ZString columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.ColumnName == columnName);
		}

		ZBool CheckColumnAvailableInGrid(ZGrid grid, ZString columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(info => info.ColumnName == columnName && !info.IsUnavailable);
		}

		public class ContainersUserControlForTest : ContainersUserControl
		{
			public ZGrid LocationGrid
			{
				get { return locationsControl.Grid; }
			}

			public ZGrid JobPackLinesGrid
			{
				get { return packLinesContol.Grid; }
			}

			public ZCalcEdit JS_ActualWeight_ImperialTextBox
			{
				get { return JS_ActualWeight_ImperialBoundTextBox; }
			}

			public new ZCalcEdit JL_LoadingMetersCalcDropEdit
			{
				get { return base.JL_LoadingMetersCalcDropEdit; }
			}
		}

		#endregion
	}
}
