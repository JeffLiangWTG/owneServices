using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V3.Module;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		public void TestGetNewController()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var declaration = Factory.New<JobDeclaration>();
				var controller = module.GetNewController(declaration);
				Assert(controller is JobDeclarationController);
				declaration.JE_ApplicationCode = "";
				controller = module.GetNewController(declaration);
				Assert(controller is V3JobDeclarationController);
				declaration.JE_ApplicationCode = "4.1";
				controller = module.GetNewController(declaration);
				Assert(controller is JobDeclarationController);
				declaration.JE_ApplicationCode = "ITF";
				controller = module.GetNewController(declaration);
				Assert(controller is JobDeclarationController);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var module = new JobDeclarationModuleForTest())
			{
				var controller = module.GetNewController(null);
				Assert(controller is Customs.Module.JobDeclarationController);
			}
		}

		public void TestPrintPermits()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Permit"));
				var checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.Operations);
				checkpoint.IsAllowed = true;
				ExecuteAndAssert(module, "Print All Permits", 0);
				var declarations = new JobDeclaration[] { (JobDeclaration)module.GridCollection.AddNew(), (JobDeclaration)module.GridCollection.AddNew() };
				var cusEntryHeader = (CusEntryHeader)declarations[0].ActiveEntryHeaders.AddNew();
				AddEDIMessage(cusEntryHeader);
				var cusEntryHeader1 = (CusEntryHeader)declarations[1].ActiveEntryHeaders.AddNew();
				AddEDIMessage(cusEntryHeader1);
				ExecuteAndAssert(module, "Print All Permits", 2);
				ExecuteAndAssert(module, "Print Selected Permits", 0);
				module.selectedElements = declarations;
				ExecuteAndAssert(module, "Print Selected Permits", 2);
				checkpoint.IsAllowed = false;
				ExecuteAndAssert(module, "Print Selected Permits", 0);
			}
		}

		public void TestCopyDeclarationOnly_ExistsInContextMenu()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				AssertNull(module.FormActionMenu.FindByText(JobDeclarationModuleForTest.CopyDeclarationOnlyText));
			}
		}

		public void TestCopyDeclarationOnly()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				var declaration = Factory.New<JobDeclaration>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "TST001";
				declaration.JE_JS = shipment.PK;
				Factory.Save();
				module.SimulateContextMenuClick();
				AssertEquals("Information Please select one Declaration to copy.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("pre-condition", 0, OpenedFormCache.GetInstance().Count);
				module.selectedElements = new JobDeclaration[1];
				module.selectedElements[0] = declaration;
				module.SimulateContextMenuClick();

				var openedForm = OpenedFormCache.GetInstance();
				UserIdleWorker.Flush();
				AssertEquals(1, openedForm.Count);
				openedForm.CloseAllCachedForms();
			}
		}

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			var vessels = factory.GetCachedValue("BaseVesselDeclarationModuleTest", delegate
			{
				return factory.Load<RefVessel>(new ZQuery()
				{ MaximumRows = 6 });
			});
			var unlocos = factory.GetCachedValue("BaseUNLOCODeclarationModuleTest", delegate
			{
				return factory.Load<RefUNLOCO>(new ZQuery()
				{ MaximumRows = 11 });
			});
			var outwardCarrierAgent = factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.OutwardShippingLineForwarderPK = outwardCarrierAgent.PK;
			var twoDigitsNumber = i.ToString().PadLeft(2, '0');
			var transportModeCodeList = factory.GetCachedValue<TransportModeCodeList>();
			declaration.SG_OutwardTransportMode = transportModeCodeList[i % transportModeCodeList.Count].Code;
			declaration.SG_OutwardHAWB = "HBL" + twoDigitsNumber;
			declaration.SG_OutwardMAWB = "MBL" + twoDigitsNumber;
			var certificateTypeCodeList = CertificateTypesTestList;
			declaration.SG_ApplicationProductType = certificateTypeCodeList[i % certificateTypeCodeList.Count].Code;
			declaration.SG_OutwardVesselName = vessels[i % vessels.Length].RV_Code;
			declaration.SG_OutwardVoyageFlightNo = "OV" + twoDigitsNumber;
			declaration.JE_ContainerCount = (ZShort)i;
			declaration.JE_RL_NKPortOfArrival = unlocos[(i + 1) % unlocos.Length].RL_Code;
			declaration.JE_RL_NKPortOfFirstArrival = unlocos[(i + 2) % unlocos.Length].RL_Code;
			declaration.JE_RL_NKPortOfLoading = unlocos[(i + 3) % unlocos.Length].RL_Code;
			var container = declaration.CusContainers.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CertificateNumber = "CERT" + twoDigitsNumber;
			return declaration;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.JobDeclaration;

		void ExecuteAndAssert(JobDeclarationModuleForTest module, string menuItemName, int expectedDocumentCount)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			module.docPack = new DocumentPack();
			GetMenuItemByName(module.FormActionMenu.FindByText("&Actions").MenuItems, menuItemName).PerformClick();
			if (expectedDocumentCount == 0)
			{
				AssertEquals(0, module.docPack.Count);
				var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Permit"));
				var checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.Operations);
				if (!checkpoint.IsAllowed)
				{
					AssertEquals("Question " + checkpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
				}
				else
				{
					AssertEquals(0, module.docPack.Count);
					AssertEquals("Information There are no permits to print", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
			else
			{
				AssertEquals(expectedDocumentCount, module.docPack.Count);
				Assert(module.docPack.DeliveryInstructions.HasPrintedDocuments);
				AssertEquals("2 Cargo Clearance Permit(s) from - " + GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName, module.docPack.EmailSubjectForConsolidateReports);
			}
		}

		MenuItem GetMenuItemByName(Menu.MenuItemCollection menuItemsCollection, string menuItemName)
		{
			MenuItem result = null;
			foreach (MenuItem item in menuItemsCollection)
			{
				if (item.Text == menuItemName)
				{
					result = item;
					break;
				}
			}

			return result;
		}

		void AddEDIMessage(CusEntryHeader cusEntryHeader)
		{
			var message = cusEntryHeader.Messages.AddNew();
			message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
		}

		CodeDescriptionPairList CertificateTypesTestList
		{
			get
			{
				var cooList = new CodeDescriptionPairList();
				cooList.AddPair("1", "GSP Form A");
				cooList.AddPair("2", "GSP Form A under Cumulative ASEAN");
				cooList.AddPair("3", "Back-to-Back GSP Form A");
				cooList.AddPair("4", "Ordinary Certificate of Origin");
				cooList.AddPair("4A", "Certificate of Processing");
				cooList.AddPair("5", "Commonwealth Preference Certificate");
				cooList.AddPair("7", "Form W – reserve");
				cooList.AddPair("9", "Ordinary Certificate of Origin for textile products to EU countries only");
				cooList.AddPair("10", "Export Certificate for fresh cut orchids");
				cooList.AddPair("12", "Global System of Trade Preference (GSTP)");
				return cooList;
			}
		}

		sealed class JobDeclarationModuleForTest : JobDeclarationModule
		{
			public BusinessObject[] selectedElements;

			public DocumentPack docPack;

			public new ZController GetNewController(BusinessObject selectedBusinessObject) => base.GetNewController(selectedBusinessObject);

			public void SimulateContextMenuClick()
			{
				var menuItem = Grid.ContextMenu.MenuItems.FindByText("Actions").MenuItems.FindByText(CopyDeclarationOnlyText);
				menuItem.PerformClick();
			}

			protected override DocumentPack DocPack
			{
				get => docPack;
				set => docPack = value;
			}

			protected override BusinessObject[] GetSelectedElements() => selectedElements;
		}
	}
}
