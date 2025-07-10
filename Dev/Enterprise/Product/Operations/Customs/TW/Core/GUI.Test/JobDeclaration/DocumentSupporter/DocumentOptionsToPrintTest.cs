using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.DocumentEngine.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(DocumentOptionsToPrint))]
	sealed class DocumentOptionsToPrintTest : TestCaseWithFactory
	{
		public void TestGetDocumentOptionsToPrintDocumentSupporterFromShipmentShouldBeForwardingShipmentDocumentSupporter()
		{
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Test";
			menuItem.SU_IsClientSpecific = true;
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			menuItem.SU_DeliveryRestrictionMacro = "\"<NotifyParty.OH_Code>\"!=\"\"";
			menuItem.SU_DeliveryRestrictionDescription = "Please check shipment's notify party";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = RefDocTypes.ShipmentNotes;
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = menuItem.PK;
			pivot.SI_RT_DocType = docType.PK;
			(menuItem.Documents as List<IStmMenuTemplatePivot>).Add(pivot);

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_Code = "XX";
			var address = notifyParty.Addresses.MainAddress;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = address.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			var documentSupporterDataState = shipment.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			CombineAssertions(() =>
			{
				AssertEquals("IsValid", true, documentSupporterDataState.IsValid);
				AssertEquals("ErrorMessage should not be: User defined delivery restriction condition is not met. \r\nPlease check shipment's notify party", string.Empty, documentSupporterDataState.ErrorMessage);
			});
		}

		public void TestGetDocumentOptionsToPrintCustomsDeclarationFromDeclarationModule()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			declaration.DocumentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
			AssertType<JobDeclarationDocumentOptionsForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestGetDocumentOptionsToPrintCustomsDeclarationWithMultipleDeclarations()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "EXP";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "EXP";
			var declarationList = declaration1.DocumentSupporter.JobDeclarationDocumentAddressConfig.Declarations;
			declarationList.Add(declaration1);
			declarationList.Add(declaration2);
			var invoiceLine = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration1.CustomsEntryInstructions.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			using (var form = new JobDeclarationForm(declaration1))
			{
				form.Show();
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				messageInitiator.AnswerToContinueWithAction = true;
				declaration1.MessageInitiator = messageInitiator;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (JobDeclarationDocumentOptionsForm)obj;
					var config = (JobDeclarationDocumentAddressConfig)dialog.BusinessEntity;
					config.DocumentName = "name1";
					config.HideEXPExporterTradChineseAddr = true;
					config.HideEXPExporterEnglishAddr = true;
					config.HideEXPBuyerTradChineseAddr = false;
					config.HideEXPBuyerEnglishAddr = true;
					config.HideIMPImporterTradChineseAddr = true;
					config.HideIMPImporterEnglishAddr = true;
					config.HideIMPSellerTradChineseAddr = true;
					config.CustomizeSectionBodyRow = "10";
					config.HideCustomizeSectionBodyRow = true;
					config.GoodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(x => x.Field == ExportDeclarationDocumentFieldList.Codes.GoodsDescription).Caption = "test desc";
					dialog.FindSingle<ZButton>("DeliverButton").PerformClick();
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = GetMenuItemForTesting(Business.CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument);
				declaration1.DocumentSupporter.GetDataStateBeforeRun(menuItem);
				CombineAssertions(() =>
				{
					var copiedConfig = declaration2.DocumentSupporter.JobDeclarationDocumentAddressConfig;
					AssertEquals("DocumentName", "name1", copiedConfig.DocumentName);
					AssertEquals("HideEXPExporterTradChineseAddr", true, copiedConfig.HideEXPExporterTradChineseAddr);
					AssertEquals("HideEXPExporterEnglishAddr", true, copiedConfig.HideEXPExporterEnglishAddr);
					AssertEquals("HideEXPBuyerTradChineseAddr", false, copiedConfig.HideEXPBuyerTradChineseAddr);
					AssertEquals("HideEXPBuyerEnglishAddr", true, copiedConfig.HideEXPBuyerEnglishAddr);
					AssertEquals("HideIMPImporterTradChineseAddr", true, copiedConfig.HideIMPImporterTradChineseAddr);
					AssertEquals("HideIMPImporterEnglishAddr", true, copiedConfig.HideIMPImporterEnglishAddr);
					AssertEquals("HideIMPSellerTradChineseAddr", true, copiedConfig.HideIMPSellerTradChineseAddr);
					AssertEquals("CustomizeSectionBodyRow", "10", copiedConfig.CustomizeSectionBodyRow);
					AssertEquals("HideCustomizeSectionBodyRow", true, copiedConfig.HideCustomizeSectionBodyRow);
					AssertEquals("GoodsDescriptionConfigs Count", 11, copiedConfig.GoodsDescriptionConfigs.Count);
					AssertEquals("'Goods Description' Caption", "test desc", copiedConfig.GoodsDescriptionConfigs.Cast<JobDeclarationDocumentGoodsDescriptionConfig>().FirstOrDefault(x => x.Field == ExportDeclarationDocumentFieldList.Codes.GoodsDescription).Caption);
				});
			}
		}

		public void TestGetDocumentOptionsToPrintCustomsDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.DocumentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				AssertType<JobDeclarationDocumentOptionsForm>(ZFormModaliser.LastFormShownDialogForTest);
			}

			var shipment = Factory.New<ForwardingShipment>();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			declaration.JE_JS = shipment.PK;
			using (var form = new ShipmentForm(shipment))
			{
				form.Show();
				shipment.DocumentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				AssertType<JobDeclarationDocumentOptionsForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestGetDocumentOptionsToPrintCustomsDeclarationWithoutEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var documentSupporter = declaration.DocumentSupporter;

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				messageInitiator.AnswerToContinueWithAction = false;
				declaration.MessageInitiator = messageInitiator;
				var documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				CombineAssertions("Do not Generate entries (Merge)", () =>
				{
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(declaration.EntryHeader);
				});

				messageInitiator.AnswerToContinueWithAction = true;
				documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				CombineAssertions("Generate entries (Merge), but Submit Type is not 'BLT'", () =>
				{
					AssertEquals(@"Cannot Generate Entries (Merge) when Submit Type is not 'BLT'. Please change the Submit Type to 'BLT'. 

If you cannot see the Submit Type, configure the Submit Type to 'BLT' or 'BTH' in Registry > Customs > Integration > Local Country Customs Interface.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(declaration.EntryHeader);
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				documentSupporterDataState = documentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				CombineAssertions("Generate entries (Merge) successfully.", () =>
				{
					AssertEquals("Generate Entry(Merge) successful, please save and send again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull(declaration.EntryHeader);
				});
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var newDeclaration2 = Factory.New<JobDeclaration>();
			newDeclaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new JobDeclarationForm(newDeclaration2))
			{
				form.Show();
				var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
				messageInitiator.AnswerToContinueWithAction = true;
				newDeclaration2.MessageInitiator = messageInitiator;
				var documentSupporterDataState = declaration.DocumentSupporter.GetDataStateBeforeRun(MenuItemForTesting);
				CombineAssertions("Generate entries (Merge) failed", () =>
				{
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(newDeclaration2.EntryHeader);
				});
			}
		}

		public void TestPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired()
		{
			foreach (var dataContext in ShouldDisplayOptionDialog)
			{
				AssertPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired(dataContext);
			}
			AssertPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired("mydataContext");
		}

		protected override void SetUp()
		{
			base.SetUp();
			documentEngineResourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));
		}

		protected override void TearDown()
		{
			if (documentEngineResourceRetriever.IsValueCreated)
			{
				documentEngineResourceRetriever.Value.Dispose();
			}
			base.TearDown();
		}

		Lazy<EmbeddedResourceRetriever> documentEngineResourceRetriever;

		void AssertPerformOnGetDocumentOptionsToPrintCustomsDeclarationRequired(ZString dataContext)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			ZFormModaliser.LastFormShownDialogForTest = null;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var docType = Factory.New<RefDocType>();
				docType.RT_DocType = RefDocTypes.EntryPrint;
				var menuItem2 = GetMenuItemForTesting(dataContext);
				declaration.DocumentSupporter.GetDataStateBeforeRun(menuItem2);
				if (ShouldDisplayOptionDialog.Contains(dataContext))
				{
					AssertType<JobDeclarationDocumentOptionsForm>(ZFormModaliser.LastFormShownDialogForTest);
				}
				else
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		IStmMenuItem MenuItemForTesting => menuItemForTesting ?? (menuItemForTesting = GetMenuItemForTesting(".ImportCustomsDeclarationDocument"));
		IStmMenuItem menuItemForTesting;

		IStmMenuItem GetMenuItemForTesting(ZString dataContext)
		{
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = "Test Customs Declaration";
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = RefDocTypes.MiscellaneousDocument;
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = menuItem.PK;
			pivot.SI_RT_DocType = docType.PK;

			var blankTemplate = Factory.New<StmTemplate>();
			blankTemplate.SO_Name = "B";
			blankTemplate.SO_Template = documentEngineResourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");
			blankTemplate.SO_DataContext = dataContext;
			pivot.SI_SO = blankTemplate.PK;
			(menuItem.Documents as List<IStmMenuTemplatePivot>).Add(pivot);
			return menuItem;
		}

		HashSet<ZString> ShouldDisplayOptionDialog => shouldDisplayOptionDialog ?? (shouldDisplayOptionDialog = new DocumentOptionsToPrint().ShouldDisplayOptionDialog);
		HashSet<ZString> shouldDisplayOptionDialog;
	}
}
