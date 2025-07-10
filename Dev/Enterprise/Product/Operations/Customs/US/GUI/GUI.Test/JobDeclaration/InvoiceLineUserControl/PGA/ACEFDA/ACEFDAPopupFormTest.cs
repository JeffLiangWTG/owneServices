using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEFDAPopupForm))]
	sealed class ACEFDAPopupFormTest : ZFormBasherTest
	{
		public void TestScientificDetailsGridNotExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var constituentElement = fda.ProductConstituentElements.AddNew();
			constituentElement.US_GenusName = "NAME";
			using (var form = new ACEFDAPopupForm(fda))
			{
				form.Show();
				var detailsGroupBox = form.Controls.Find("DetailsGroupBox", true).Single();
				var controls = detailsGroupBox.Controls.Find("ConstGroupBox", true);
				AssertEquals("ConstGroupBox which ScientificDetailsGrid is belong to should has been removed", 0, controls.Length);
			}
		}

		public void TestFormProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			using (var form = new ACEFDAPopupForm(fda))
			{
				form.Show();
				AssertEquals("form.FormCaption", "FDA: Invoice: TEST1, LNO: 1", form.FormCaption);
			}
		}

		public void TestControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var constituentElement = fda.ProductConstituentElements.AddNew();
			constituentElement.US_GenusName = "NAME";
			using (var form = new ACEFDAPopupForm(fda))
			{
				form.Show();
				fda.US_ContainerDimType = CylindricalRectangularList.Codes.Cylindrical;
				AssertEquals("Diameter", form.US_ContainerDim1CalcEdit.CaptionResourceString.Caption);
				AssertEquals("Diameter", form.CanDim1InchCalcEdit.CaptionResourceString.Caption);
				fda.US_ContainerDimType = CylindricalRectangularList.Codes.Rectangular;
				AssertEquals("Width", form.US_ContainerDim1CalcEdit.CaptionResourceString.Caption);
				AssertEquals("Width", form.CanDim1InchCalcEdit.CaptionResourceString.Caption);
				fda.US_DimUQ = FDAMeasurementUnitList.Codes.InchesWithOneSixteenthDecimals;
				AssertEquals("Visible", true, form.InchPanel.Visible);
				fda.US_DimUQ = FDAMeasurementUnitList.Codes.Centimeters;
				AssertEquals("Not visible", false, form.InchPanel.Visible);
				var capName = (ZString)form.ConstGrid.GetColumnCaption(("US_PGAQuantityOfConstituentElement"));
				AssertEquals("visible", true, !capName.IsEmpty);
				AssertEquals(true, form.ForcePNCheckBox.Visible);
				AssertEquals(true, form.PNDisclCheckBox.Visible);
				AssertEquals(true, form.PNConfNoTextBox.Visible);
				AssertEquals(true, form.LicenseGroupBox.Visible);
				AssertEquals("Should be visiable below the Vehicle license Info grid.", new System.Drawing.Point(433, 390), form.LicenseGroupBox.Location);
			}
		}

		public void TestColumnVisibilityForProduct()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<Business.OrgSupplierPart>();
			product.OP_PartNum = "Test";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = importer.PK;
			relOrg.OU_Relationship = "OWN";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			var fdaOnProduct = pivot.ACEFDAs.AddNew();
			fdaOnProduct.US_ProductCode = "123456";
			var constituentElement = fdaOnProduct.ProductConstituentElements.AddNew();
			constituentElement.US_GenusName = "TEST";
			using (var form = new ACEFDAPopupForm(fdaOnProduct))
			{
				form.Show();
				var capName = (ZString)form.ConstGrid.GetColumnCaption(("US_PGAQuantityOfConstituentElement"));
				AssertEquals("visible", true, !capName.IsEmpty);
				AssertEquals(false, form.ForcePNCheckBox.Visible);
				AssertEquals(false, form.PNDisclCheckBox.Visible);
				AssertEquals(false, form.PNConfNoTextBox.Visible);
				Assert(form.FoodFacRegNoTextBox.Visible);
				Assert(form.ExemptDropEdit.Visible);
				Assert(form.PackGroupBox.Visible);
				AssertEquals("Should be visiable below the AOC grid.", new System.Drawing.Point(433, 360), form.LicenseGroupBox.Location);
			}
		}

		public void TestControlLabels()
		{
			using (var form = new ACEFDAPopupForm())
			{
				form.Show();
				var control = form.Controls.Find("US_UC_NKFDAProductionCodeFindBox", true)[0] as IResCaptionedControl;
				AssertEquals("US_UC_NKFDAProductionCodeFindBox label", "Growth/Prod. Ctry/Rgn.", control.CaptionResourceString.Caption);
				control = form.Controls.Find("zCodeFindBox1", true)[0] as IResCaptionedControl;
				AssertEquals("zCodeFindBox1 label", "Source Ctry/Rgn.", control.CaptionResourceString.Caption);
				control = form.Controls.Find("ShipCntryCodeFindBox", true)[0] as IResCaptionedControl;
				AssertEquals("ShipCntryCodeFindBox label", "Shipment Ctry/Rgn.", control.CaptionResourceString.Caption);
				control = form.Controls.Find("zCodeFindBox2", true)[0] as IResCaptionedControl;
				AssertEquals("zCodeFindBox2 label", "Refused Ctry/Rgn.", control.CaptionResourceString.Caption);
			}
		}

		public void TestLicensesGridColumnNames()
		{
			using (var control = new ACEFDAPopupForm())
			{
				var columnStyle = control.LicensesGrid.GetColumnStyle("US_CountryCode");
				AssertEquals("US_CountryCode caption", "Country/Region", columnStyle.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.Factory.Save();
			return new ACEFDAPopupForm(fda);
		}
	}
}
