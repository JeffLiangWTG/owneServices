using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationFormTest_IPT : JobDeclarationFormAbstractTest
	{
		public void TestFormCaption()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKN;
				AssertEquals("Customs Declaration - IPT - BKT", form.FormCaption);
			}
		}

		public void TestAmountsCalculatedOnSaved()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.MessageInitiator = declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true)).PK;
				declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
				declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DNG;
				invoiceHeader.JZ_InvoiceAmount = 1000m;
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_InvoiceQuantity = 3000m;
				invoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.TNE;
				invoiceLine.JI_LinePrice = 1650000m;
				invoiceLine.JI_Tariff = "27101111";
				invoiceLine.SG_OuterPackQuantity = 30000;
				invoiceLine.SG_OuterPackQuantityUnit = UnitOfQuantityCodeList.Codes.DRM;
				invoiceLine.SG_UnitDutiableWGTVOLQTY = 10;
				invoiceLine.SG_UnitDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.DAL;
				invoiceLine.SG_TotalDutiableWGTVOLQTY = 300000;
				invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.DAL;
				AssertEquals("Pre-condition: Duty amount", 0m, declaration.TotalDutyPayable);
				AssertEquals("Pre-condition: Excise amount", 0m, declaration.TotalExcisePayable);
				AssertEquals("Pre-condition: Other Tax amount", 0m, declaration.TotalOtherTaxPayable);
				AssertEquals("Pre-condition: GST amount", 0m, declaration.TotalGSTPayable);
				form.FireSaveButton();
				AssertEquals("Duty amount", 0m, declaration.TotalDutyPayable);
				AssertEquals("Excise amount", 0m, declaration.TotalExcisePayable);
				AssertEquals("GST amount", 115500m, declaration.TotalGSTPayable);
			}
		}

		public void TestDocAddressRespectsRequirement()
		{
			declaration.JE_OH_HandlingAgent = Factory.New<OrgHeader>().PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var userControl = form.Controls[0] as CustomsBrokerageUserControl;
				var addressesTabPage = userControl.MainTabControl.GetTabPageByNameOrText("AddressesTabPage");
				userControl.MainTabControl.SelectedTab = addressesTabPage;
				var grid = addressesTabPage.FindSingle<ZGrid>("AddressGrid");
				grid.Select(0);
				var addressControl = addressesTabPage.FindSingle<ZDocAddressControl>("AddressControl");
				var docAddress = addressControl.CurrentDataItem as JobDocAddress;
				AssertEquals(DocAddressType.CarrierHandlingAgent, docAddress.DocAddressType);
				AssertEquals(false, docAddress.Requirement.CanOverride);
				var overrideCheckbox = addressControl.FindSingle<ZCheckBox>("OverrideAddressCheckbox");
				Assert(!overrideCheckbox.Visible);
			}
		}

		public override ZString MessageTypeForFormBashing => MessageTypeCodeList.Codes.IPT;

		protected override void SetUp()
		{
			base.SetUp();
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "Goods and Services Tax");
			Factory.Save();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
	}
}
