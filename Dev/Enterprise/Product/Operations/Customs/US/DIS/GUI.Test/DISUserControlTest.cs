using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DIS.Business;
using Enterprise.Customs.US.DIS.Business.Testing;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.DIS.GUI.Testing
{
	sealed class DISUserControlTest : TestCaseWithFactory
	{
		public void TestOptionalDataTopPanelVisibility()
		{
			var helper = new DISUniversalReferenceHelper(Factory);

			var code1 = helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "OptionalData", "PCK");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "OptionalData", "INV");

			var code2 = helper.CreateDisCodeEntry("EPA01", "EPA01");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "COM");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "PER");
			Factory.Save();

			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_AddInfo = Xml1;

			var addInfo2 = requiredDocument.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo2.EX_AddInfo = Xml1;

			AssertEquals("Precondition", 2, HostWrapper.DISDocuments.Count);

			using (DISForm form = new DISForm(HostWrapper))
			{
				form.Show();

				var control = form.Controls.Find("disUserControl1", true)[0];
				var tabControl = (ZTabControl)control.Controls.Find("OptionalDataTabControl", true)[0];

				var optionalDataTabPage = (ZTabPage)tabControl.Controls.Find("OptionalDataTabPage", true)[0];
				tabControl.SelectedTab = optionalDataTabPage;

				var noOptionalDataAvailableLabel = optionalDataTabPage.Controls.Find("OptionalDataUnavailableLabel", true)[0];
				var invoiceGroupBox = optionalDataTabPage.Controls.Find("InvoiceGroupBox", true)[0];
				var commodityGroupBox = optionalDataTabPage.Controls.Find("CommodityGroupBox", true)[0];
				var bondDataGroupBox = optionalDataTabPage.Controls.Find("BondDataGroupBox", true)[0];
				var toxicSubstanceGroupBox = optionalDataTabPage.Controls.Find("ToxicSubstanceGroupBox", true)[0];
				var permitGroupBox = optionalDataTabPage.Controls.Find("PermitGroupBox", true)[0];
				var certificateGroupBox = optionalDataTabPage.Controls.Find("CertificateGroupBox", true)[0];
				var packingListGroupBox = optionalDataTabPage.Controls.Find("PackingListGroupBox", true)[0];

				Assert(noOptionalDataAvailableLabel.Visible);
				AssertEquals(DISDocument.EnterAFormTypeMessage, noOptionalDataAvailableLabel.Text);
				Assert(!invoiceGroupBox.Visible);
				Assert(!commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(!permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(!packingListGroupBox.Visible);

				var grid = (ZGrid)control.Controls.Find("DocumentsGrid", true)[0];
				grid.ListManager.Position = 0;
				var disDocument = (DISDocument)grid.ListManager.GetCurrent();

				disDocument.DocumentLabel = "EPA01";
				Assert(!noOptionalDataAvailableLabel.Visible);
				Assert(!invoiceGroupBox.Visible);
				Assert(commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(!packingListGroupBox.Visible);

				grid.ListManager.Position = 1;

				Assert(noOptionalDataAvailableLabel.Visible);
				Assert(!invoiceGroupBox.Visible);
				Assert(!commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(!permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(!packingListGroupBox.Visible);

				disDocument = (DISDocument)grid.ListManager.GetCurrent();
				disDocument.DocumentLabel = "CBP01";
				Assert(!noOptionalDataAvailableLabel.Visible);
				Assert(invoiceGroupBox.Visible);
				Assert(!commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(!permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(packingListGroupBox.Visible);
			}
		}

		public void TestWhenInvoiceIsTheOnlyOptionalDataRelevant()
		{
			var helper = new DISUniversalReferenceHelper(Factory);

			var code1 = helper.CreateDisCodeEntry("CBP01", "PACKING_LIST");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "OptionalData", "PCK");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, "OptionalData", "INV");

			var code2 = helper.CreateDisCodeEntry("NMF02", "APH_STAT");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "COM");
			helper.DataHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, "OptionalData", "PER");

			Factory.Save();

			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_AddInfo = Xml1;

			var addInfo2 = requiredDocument.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo2.EX_AddInfo = Xml1;

			var disDocument = HostWrapper.DISDocuments[0];
			disDocument.DocumentLabel = "CBP01";
			var disDocument2 = HostWrapper.DISDocuments[1];
			disDocument2.DocumentLabel = "NMF02";

			using (DISForm form = new DISForm(HostWrapper))
			{
				form.Show();

				var control = form.Controls.Find("disUserControl1", true)[0];
				var tabControl = (ZTabControl)control.Controls.Find("OptionalDataTabControl", true)[0];

				var optionalDataTabPage = (ZTabPage)tabControl.Controls.Find("OptionalDataTabPage", true)[0];
				tabControl.SelectedTab = optionalDataTabPage;

				var noOptionalDataAvailableLabel = optionalDataTabPage.Controls.Find("OptionalDataUnavailableLabel", true)[0];
				var invoiceGroupBox = optionalDataTabPage.Controls.Find("InvoiceGroupBox", true)[0];
				var commodityGroupBox = optionalDataTabPage.Controls.Find("CommodityGroupBox", true)[0];
				var bondDataGroupBox = optionalDataTabPage.Controls.Find("BondDataGroupBox", true)[0];
				var toxicSubstanceGroupBox = optionalDataTabPage.Controls.Find("ToxicSubstanceGroupBox", true)[0];
				var permitGroupBox = optionalDataTabPage.Controls.Find("PermitGroupBox", true)[0];
				var certificateGroupBox = optionalDataTabPage.Controls.Find("CertificateGroupBox", true)[0];
				var packingListGroupBox = optionalDataTabPage.Controls.Find("PackingListGroupBox", true)[0];
				var optionalDataTopPanel = optionalDataTabPage.Controls.Find("OptionalDataTopPanel", true)[0];
				var invoiceCommodityPanel = optionalDataTabPage.Controls.Find("InvoiceCommodityPanel", true)[0];

				var grid = (ZGrid)control.Controls.Find("DocumentsGrid", true)[0];
				grid.ListManager.Position = 0;

				Assert(!noOptionalDataAvailableLabel.Visible);
				Assert(invoiceGroupBox.Visible);
				Assert(!commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(!permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(packingListGroupBox.Visible);
				Assert("top section should be visible for the first document", optionalDataTopPanel.Visible);

				grid.ListManager.Position = 1;
				Assert(!noOptionalDataAvailableLabel.Visible);
				Assert(!invoiceGroupBox.Visible);
				Assert(commodityGroupBox.Visible);
				Assert(!bondDataGroupBox.Visible);
				Assert(!toxicSubstanceGroupBox.Visible);
				Assert(permitGroupBox.Visible);
				Assert(!certificateGroupBox.Visible);
				Assert(!packingListGroupBox.Visible);
				Assert("top section should be invisible and InvoiceGroupBox should occupy all the space", optionalDataTopPanel.Visible);
				Assert("bottom section should be visible for the second document", invoiceCommodityPanel.Visible);
			}
		}

		public void TestVisibilityForExportDeclaration()
		{
			JobDeclaration["JE_MessageType"] = "EXP";
			var jobDeclaration = (IUSDISHost)JobDeclaration;
			var requiredDocument = jobDeclaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo.EX_AddInfo = Xml1;

			var addInfo2 = requiredDocument.AddInfos.AddNew();
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			addInfo2.EX_AddInfo = Xml1;

			var disDocument = HostWrapper.DISDocuments[0];
			disDocument.DocumentLabel = "CBP01";
			var disDocument2 = HostWrapper.DISDocuments[1];
			disDocument2.DocumentLabel = "NMF02";

			using (DISForm form = new DISForm(HostWrapper))
			{
				form.Show();

				var control = form.Controls.Find("disUserControl1", true)[0];
				var tabControl = (ZTabControl)control.Controls.Find("OptionalDataTabControl", true)[0];
				var detailsTabPage = (ZTabPage)tabControl.Controls.Find("DetailsTabPage", true)[0];
				var shipmentNoDropEdit = detailsTabPage.Controls.Find("ShipmentNoDropEdit", true)[0];
				Assert(shipmentNoDropEdit.Visible);
			}
		}

		const string Xml1 = @"<DISDocument>
    <DocumentID>1234354337</DocumentID>
  </DISDocument>";

		DISHostWrapper HostWrapper
		{
			get { return hostWrapper ?? (hostWrapper = new DISHostWrapper((IUSDISHost)JobDeclaration)); }
		}
		DISHostWrapper hostWrapper;

		BusinessObject JobDeclaration
		{
			get { return jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration()); }
		}
		BusinessObject jobDeclaration;
	}
}
