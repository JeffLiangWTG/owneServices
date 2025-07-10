using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationUserControl))]
	class JobDeclarationUserControlTest : Customs.GUI.Testing.BaseCustomsDeclarationUserControlAbstractTest<JobDeclarationUserControl, JobDeclaration>
	{
		public void TestOrganizationControlsOrder()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				var shippingOrAirLineOrganisationControl = declarationUserControl.FindSingle<ZGuidFindBox>("ShippingOrAirLineOrganisationControl");
				var forwarderOrganisationControl = declarationUserControl.FindSingle<ZGuidFindBox>("ForwarderOrganisationControl");
				var containerTerminalOperatorAddressControl = declarationUserControl.FindSingle<ZDocAddressControl>("ContainerTerminalOperatorAddressControl");
				var depotAddressControl = declarationUserControl.FindSingle<ZDocAddressControl>("DepotAddressControl");
				var containerYardAddressControl = declarationUserControl.FindSingle<ZDocAddressControl>("ContainerYardAddressControl");
				var declarantOfficeAddressControl = declarationUserControl.FindSingle<ZAddressControl>("DeclarantOfficeAddressControl");
				var representativeAddressControl = declarationUserControl.FindSingle<ZAddressControl>("RepresentativeAddressControl");
				var notifyOrganisationControl = declarationUserControl.FindSingle<ZGuidFindBox>("NotifyOrganisationControl");
				var controllingAgentGuidFindBox = declarationUserControl.FindSingle<ZGuidFindBox>("ControllingAgentGuidFindBox");
				var controllingCustomerGuidFindBox = declarationUserControl.FindSingle<ZGuidFindBox>("ControllingCustomerGuidFindBox");
				var externalBrokerGuidFindBox = declarationUserControl.FindSingle<ZGuidFindBox>("ExternalBrokerGuidFindBox");
				AssertGreaterThan(forwarderOrganisationControl.Location.Y, shippingOrAirLineOrganisationControl.Location.Y);
				AssertGreaterThan(containerTerminalOperatorAddressControl.Location.Y, forwarderOrganisationControl.Location.Y);
				AssertGreaterThan(depotAddressControl.Location.Y, containerTerminalOperatorAddressControl.Location.Y);
				AssertGreaterThan(containerYardAddressControl.Location.Y, depotAddressControl.Location.Y);
				AssertGreaterThan(declarantOfficeAddressControl.Location.Y, containerYardAddressControl.Location.Y);
				AssertGreaterThan(representativeAddressControl.Location.Y, declarantOfficeAddressControl.Location.Y);
				AssertGreaterThan(notifyOrganisationControl.Location.Y, representativeAddressControl.Location.Y);
				AssertGreaterThan(controllingAgentGuidFindBox.Location.Y, notifyOrganisationControl.Location.Y);
				AssertGreaterThan(controllingCustomerGuidFindBox.Location.Y, controllingAgentGuidFindBox.Location.Y);
				AssertGreaterThan(externalBrokerGuidFindBox.Location.Y, controllingCustomerGuidFindBox.Location.Y);
			}
		}

		public void TestSetRightTabControlSelectTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("When open the declaration form, show Organizations as default.", control.OrganisationsTabPage, control.RightTabControl.SelectedTab);
			}
		}

		public void TestTotalCustomsQuantityCalcDropEdit()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				var control = (ZCalcDropEdit)declarationUserControl.Controls.Find("TotalCustomsQuantityCalcDropEdit", true).FirstOrDefault();
				AssertEquals(nameof(JobDeclaration.TotalCustomsQuantity), control.BindToAmount);
				AssertEquals(nameof(JobDeclaration.TotalCustomsQuantityUnit), control.BindToUnit);
			}
		}

		public void TestDescriptionBoxOfIncoTermDropEditIsInvisible()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				var control = (ZDropEdit)declarationUserControl.Controls.Find("IncoTermDropEdit", true).FirstOrDefault();
				AssertEquals(false, control.ShowDescriptionBox);
			}
		}

		public void TestLebelOfJE_ShipmentIncoTermPlaceTextBoxIsInvisible()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				var control = (ZTextBox)declarationUserControl.Controls.Find("JE_ShipmentIncoTermPlaceTextBox", true).FirstOrDefault();
				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(control));
			}
		}

		public void TestControlsExistence()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				TestHelper.AssertControlExists(declarationUserControl, "JE_EntrySubStyleDropEdit", "JE_EntrySubStyle");
				TestHelper.AssertControlExists(declarationUserControl, "JE_EntryDateForDutyDateEdit", "JE_EntryDateForDuty");
				TestHelper.AssertControlExists(declarationUserControl, "ZG_ShippingCountryFindBox", "ZG_ShippingCountry");
				TestHelper.AssertControlExists(declarationUserControl, "ZG_CountryOfSupplyFindBox", "ZG_CountryOfSupply");
				TestHelper.AssertControlExists(declarationUserControl, "NotifyOrganisationControl", "JE_OH_NotifyParty");
				TestHelper.AssertControlExists(declarationUserControl, "RepresentativeAddressControl", "JE_OA_Representative");
				TestHelper.AssertControlExists(declarationUserControl, "DutyPaymentTypeDropEdit", "JE_PaymentMethod");
				TestHelper.AssertControlExists(declarationUserControl, "JE_DeclarationDateDateEdit", "JE_DeclarationDate");
				TestHelper.AssertControlExists(declarationUserControl, "JE_DeclarationExchangeRateCalcEdit", "JE_DeclarationExchangeRate");
				TestHelper.AssertControlExists(declarationUserControl, "ZG_BankCodeFindBox", "ZG_BankCode");
				TestHelper.AssertControlExists(declarationUserControl, "JE_TransportMeansDropEdit", "JE_TransportMeans");
				TestHelper.AssertControlExists(declarationUserControl, "JE_SubLocationOfGoodsTextBox", "JE_SubLocationOfGoods");
				TestHelper.AssertControlExists(declarationUserControl, "GoodsAtCustomsAreaCheckBox", "GoodsAtCustomsArea");
				TestHelper.AssertControlExists(declarationUserControl, "OverTimePaymentCompletedCheckBox", "OverTimePaymentCompleted");
				TestHelper.AssertControlExists(declarationUserControl, "InspectionClerkTextBox", "InspectionClerk");
				TestHelper.AssertControlExists(declarationUserControl, "BondedWarehouseFindBox", "BondedWarehouseCode");
				TestHelper.AssertControlExists(declarationUserControl, "JE_ExportGoodsTypeDropEdit", "JE_ExportGoodsType");
				TestHelper.AssertControlExists(declarationUserControl, "ZG_NumberOfDocumentsCalcEdit", "ZG_NumberOfDocs");
				TestHelper.AssertControlExists(declarationUserControl, "TradeTypeDropEdit", "ZG_TradeType");
				TestHelper.AssertControlExists(declarationUserControl, "JE_ValuationDateDateEdit", "JE_ValuationDate");
			}
		}

		public void TestCustomsOfficesUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();
				var customsOfficesUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("CustomsOfficesUserControl");
				AssertType<CustomsOfficesUserControl>(customsOfficesUserControl.HostedControl);
			}
		}

		public void TestSetCaptions()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				AssertEquals("Load Port", declarationUserControl.PortOfLoadingFindBox.CaptionResourceString.Caption);
				AssertEquals("[9] Financial Rep.", declarationUserControl.FindSingle<ZAddressControl>("RepresentativeAddressControl").CaptionResourceString.Caption);
				AssertEquals("Registration No", declarationUserControl.ExportDeclarationNumberBoundTextBox.CaptionResourceString.Caption);
				AssertEquals("Valuation Date", declarationUserControl.JE_ValuationDateDateEdit.CaptionResourceString.Caption);
				AssertEquals("Trade Type", declarationUserControl.TradeTypeDropEdit.CaptionResourceString.Caption);
			}
		}

		public void TestUnnecessaryControlsHaveBeenHidden()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				control.JobDeclaration = declaration;
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					Assert("CTStatusIDDropEdit should be invisible", !control.CTStatusIDDropEdit.Visible);
					Assert("PortOfFirstArrivalFindBox should be invisible", !control.PortOfFirstArrivalFindBox.Visible);
					Assert("JE_UCRTextBox should be invisible", !control.JE_UCRTextBox.Visible);
					Assert("ZG_AgreedPlaceCodeDropEdit should be invisible", !control.ZG_AgreedPlaceCodeDropEdit.Visible);
					Assert("JE_DateOfFirstArrivalBoundDateEdit should be invisible", !control.JE_DateOfFirstArrivalBoundDateEdit.Visible);
				});
			}
		}

		public void TestFieldVisibilityForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "JE_CustomsDischargePortDropEdit", true);
					AssertControlVisibility(userControl, "JE_CustomsLoadPortDropEdit", false);
					AssertControlVisibility(userControl, "TradeTypeDropEdit", false);
				});

				declaration.JE_TransportMode = "ROA";
				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "JE_CustomsDischargePortDropEdit", false);
					AssertControlVisibility(userControl, "JE_CustomsLoadPortDropEdit", false);
				});
			}
		}

		public void TestFieldVisibilityForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";

			using (var form = new ZForm(declaration))
			using (var userControl = new JobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "JE_CustomsDischargePortDropEdit", false);
					AssertControlVisibility(userControl, "JE_CustomsLoadPortDropEdit", true);
					AssertControlVisibility(userControl, "TradeTypeDropEdit", true);
				});

				declaration.JE_TransportMode = "AIR";
				CombineAssertions(() =>
				{
					AssertControlVisibility(userControl, "JE_CustomsDischargePortDropEdit", false);
					AssertControlVisibility(userControl, "JE_CustomsLoadPortDropEdit", false);
				});
			}
		}

		void AssertControlVisibility(Control userControl, string controlName, bool isVisible)
		{
			AssertEquals(controlName, isVisible, userControl.FindSingle<Control>(controlName).Visible);
		}

		public void TestShowIsHighValueOvrdCheckBox()
		{
			CombineAssertions(() =>
			{
				using (var userControl = new JobDeclarationUserControl())
				{
					AssertEquals("When it is export", false, userControl.ShowIsHighValueOvrdCheckBox(false));
					AssertEquals("When it is import", false, userControl.ShowIsHighValueOvrdCheckBox(true));
				}
			});
		}

		public void TestZG_NumberOfDocumentsCalcEditRangeAndMaxLength()
		{
			using (var declarationUserControl = new JobDeclarationUserControl())
			{
				var control = (ZCalcEdit)declarationUserControl.Controls.Find("ZG_NumberOfDocumentsCalcEdit", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					AssertNotNull("ZG_NumberOfDocumentsCalcEdit is not found on the form", control);

					control.CalcValue = 0;
					AssertEquals("ZG_NumberOfDocumentsCalcEdit should accept value of 0", 0, Convert.ToInt32(control.CalcValue));

					control.CalcValue = 99;
					AssertEquals("ZG_NumberOfDocumentsCalcEdit should accept value of 99", 99, Convert.ToInt32(control.CalcValue));

					AssertEquals("ZG_NumberOfDocumentsCalcEdit MaxLength should be 2 as expected", 2, control.MaxLength);
				});
			}
		}
	}
}
