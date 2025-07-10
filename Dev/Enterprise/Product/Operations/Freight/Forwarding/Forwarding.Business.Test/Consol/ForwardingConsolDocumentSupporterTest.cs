using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.CH;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolDocumentSupporter))]
	public class ForwardingConsolDocumentSupporterTest : DocumentSupporterTest
	{
		#region GetBODocDataProvidersNotFoundMessage

		public void TestGetBODocDataProvidersNotFoundMessage_NoAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var contextArray = new[]
			{
				Constants.DataContext.AWB,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This Consol is not associated with a AWB data.";

			AssertNotFoundMessage(consol, menu, contextArray, true, expectedMessage);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertNotFoundMessage(consol, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoContainers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Containers.RemoveAndDeleteAll();

			var contextArray = new[]
			{
				Constants.DataContext.ERA,
				Constants.DataContext.LTL,
				Constants.DataContext.IMO,
				Constants.DataContext.Container,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This consol does not have any containers.";

			AssertNotFoundMessage(consol, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoInvoice()
		{
			var receivingForwarder = Factory.New<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var contextArray = new[]
			{
				Constants.DataContext.ARInvoice,
				Constants.DataContext.GenericFreightJobInvoice,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "There is no invoice data associated with this consol.";

			AssertNotFoundMessage(consol, menu, contextArray, true, expectedMessage);

			var transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			transactionHeader.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			transactionHeader.AH_OH = receivingForwarder.PK;
			transactionHeader.AH_InvoiceDate = ZDateTime.Now;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;

			AssertNotFoundMessage(consol, menu, contextArray, false, ZString.Empty);
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var consol = Factory.New<ForwardingConsol>();
			var supporter = consol.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<ForwardingConsol>() }; }
		}

		#endregion

		public void TestPrintConsignmentSecurityDeclarationFormDefaultRecipient()
		{
			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, AWB.AWBActions.DocumentNames.ConsignmentSecurityDeclaration));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();

			var pack = new DocumentPack(menuItem);
			pack.DocumentSupporter = consol.DocumentSupporter;
			var instructions = new DeliveryInstructionsForTest(pack, new FactoryStrategy.PopulateButDoNotSave(Factory));
			var printTask = new PrintTask();
			printTask.DeliveryInstructionsDefaultPK = menuItem.PK;
			printTask.RunWithPartialInstructions(AllowedDeliveryOptions.All, instructions, Env.Security.None);

			AssertEquals("Delivery Method", Core.Constants.ContactNotifyModes.Print, instructions.LastDocDeliveryContactCollection[0].DeliveryMethod);
		}

		[Serializable]
		class DeliveryInstructionsForTest : DeliveryInstructions
		{
			public DeliveryInstructionsForTest() : base()
			{
			}

			public DeliveryInstructionsForTest(DocumentPack docPack, FactoryStrategy factoryStrategy) : base(docPack, factoryStrategy)
			{
			}

			public DocDeliveryContactCollection LastDocDeliveryContactCollection;

			protected override DocDeliveryContactCollection GetRecipientsCore(IStmMenuItem menuItem)
			{
				LastDocDeliveryContactCollection = base.GetRecipientsCore(menuItem);
				return LastDocDeliveryContactCollection;
			}
		}

		public void TestOnlySystemDefinedDocumentsUseQueryProvider()
		{
			DocumentEventsForTest documentEventsForTest = new DocumentEventsForTest(Factory);
			documentEventsForTest.menu.SU_MenuName = "Print Final Master";
			documentEventsForTest.menu.SU_IsSystemDefined = ZBool.False;

			ForwardingConsolDocumentSupporter supporter = new ForwardingConsolDocumentSupporter(Factory.New<ForwardingConsol>());
			supporter.Initialise(documentEventsForTest);

			documentEventsForTest.OnDocumentPrintRequested();
			Assert(!documentEventsForTest.DocumentCancelEventArgs.Cancel);

			documentEventsForTest.menu.SU_IsSystemDefined = ZBool.True;
			documentEventsForTest.OnDocumentPrintRequested();
			Assert(documentEventsForTest.DocumentCancelEventArgs.Cancel);
		}

		public void TestGetWrappersForARInvoice()
		{
			Consol.JK_UniqueConsignRef = "C00009999";
			Factory.Save();

			GlbCompany currentCompany = GlbCompany.CurrentCompany;
			GlbBranch currentCompanyBranch = currentCompany.Branches[0];

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "TST";
			GlbBranch otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";

			AccTransactionHeader invoiceCurrentCompany = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceCurrentCompany.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceCurrentCompany.AH_TransactionType = TransactionTypes.Invoice;
			invoiceCurrentCompany.AH_OH = org1.PK;
			invoiceCurrentCompany.AH_TransactionNum = "00001001";
			invoiceCurrentCompany.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoiceCurrentCompany.AH_GB = currentCompanyBranch.PK;
			invoiceCurrentCompany.AH_ConsolidatedInvoiceRef = "C00009999";

			AccTransactionHeader invoiceOtherCompany = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceOtherCompany.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceOtherCompany.AH_TransactionType = TransactionTypes.Invoice;
			invoiceOtherCompany.AH_OH = org1.PK;
			invoiceOtherCompany.AH_TransactionNum = "00001002";
			invoiceOtherCompany.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoiceOtherCompany.AH_GB = otherCompanyBranch.PK;
			invoiceOtherCompany.AH_ConsolidatedInvoiceRef = "C00009999";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = Consol;
			job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;
			Factory.Save();
			Assert("Consol.IsLegacyGateway", Consol.IsLegacyGateway);

			invoiceCurrentCompany.AH_JH = job.PK;
			invoiceOtherCompany.AH_JH = job.PK;

			ForwardingConsolDocumentSupporterForTest docSupporter = new ForwardingConsolDocumentSupporterForTest(Consol);

			Factory.Save();

			DocumentWrapper[] org1InvoiceWrappers = docSupporter.GetWrappersForARInvoice(org1.PK, Core.Constants.DataContext.ARInvoice, false);
			AssertNotNull("AR Invoice Wrappers for Org1 should not be null", org1InvoiceWrappers);
			AssertEquals("Number of AR Invoice Wrappers for Org1", 1, org1InvoiceWrappers.Length);
			ZGuid businessObjectPK = ((BusinessObject)org1InvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Wrapper BusinessObject", invoiceCurrentCompany.PK, businessObjectPK);

			DocumentWrapper[] org1GenericInvoiceWrappers = docSupporter.GetWrappersForARInvoice(org1.PK, Core.Constants.DataContext.GenericFreightJob, true);
			AssertNotNull("Generic Invoice Wrappers for Org1 should not be null", org1GenericInvoiceWrappers);
			AssertEquals("Number of Generic Invoice Wrappers for Org1", 1, org1GenericInvoiceWrappers.Length);
			businessObjectPK = ((BusinessObject)org1GenericInvoiceWrappers[0].WrappedObject).PK;
			AssertEquals("Generic Wrapper BusinessObject", Consol.PK, businessObjectPK);
		}

		public void TestConsolDocumentWrapperBansPrintingADSWhenNoP2P()
		{
			// Tested in GB because too little is visible forom here.
			//  C:\dev\Enterprise\Product\Operations\Customs\GB\Enterprise.Customs.GB.Chief.Test\ChiefExportConsolIntegration\ChiefExportConsolIntegrationTests.cs, method TestConsolDocumentWrapperBansPrintingADSWhenNoP2P()
			Assert(true);
		}

		public void TestQueryProvider()
		{
			ForwardingConsolDocumentSupporterForTest documentSupporter = new ForwardingConsolDocumentSupporterForTest(Factory.New<ForwardingConsol>());
			IForwardingConsolDocumentSupporterQueryProvider queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is ForwardingConsolDocumentSupporterQueryProvider);

			Factory.SetValue<IForwardingConsolDocumentSupporterQueryProvider, ForwardingConsolDocumentSupporterQueryProviderForTest>();

			documentSupporter = new ForwardingConsolDocumentSupporterForTest(Factory.New<ForwardingConsol>());
			queryProvider = documentSupporter.QueryProvider;
			AssertNotNull(queryProvider);
			Assert(queryProvider is ForwardingConsolDocumentSupporterQueryProviderForTest);

			IForwardingConsolDocumentSupporterQueryProvider queryProviderInSaveTransaction = null;
			Factory.Saving += _ => queryProviderInSaveTransaction = documentSupporter.QueryProvider;
			Factory.Save();
			AssertNotNull(queryProviderInSaveTransaction);
			Assert(queryProviderInSaveTransaction is ForwardingConsolDocumentSupporterQueryProvider);
		}

		#region BOL Printing

		[ExpectNoExceptions]
		public void TestMultipleShipmentsNoneRequireConfirmation()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 1000m))
			{
				var consol = Factory.New<ForwardingConsol>();

				ForwardingShipment shipment1 = consol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S1";
				shipment1.CustomsEntryNumber = "";
				PackLine line1 = shipment1.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 400;
				PackLine line2 = shipment1.OuterPackLines.AddNew();
				line2.JL_LinePrice = 400;
				line2.JL_HarmonisedCode = "c2";

				ForwardingShipment shipment2 = consol.Shipments.AddNew();
				shipment2.CustomsEntryNumber = "";
				shipment2.JS_UniqueConsignRef = "S2";
				PackLine line11 = shipment2.OuterPackLines.AddNew();
				line11.JL_HarmonisedCode = "c2";
				line11.JL_LinePrice = 100;
				PackLine line22 = shipment2.OuterPackLines.AddNew();
				line22.JL_LinePrice = 100;
				line22.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				DocumentEventsForTest documentEventsForTest = new DocumentEventsForTest(Factory);
				try
				{
					var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);
					Factory.SetValue(() => queryProvider.Object);

					queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()));

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
					var supporter = new ForwardingConsolDocumentSupporter(consol);
					supporter.Initialise(documentEventsForTest);
					documentEventsForTest.OnDocumentPrintRequested();

					queryProvider.Verify(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()), Times.Never);
				}
				finally
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
				}
			}
		}

		[ExpectNoExceptions]
		public void TestMultipleShipmentsOneRequireConfirmation()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 1000m))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();

				ForwardingShipment shipment1 = consol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S1";
				shipment1.CustomsEntryNumber = "";
				PackLine line1 = shipment1.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 400;
				PackLine line2 = shipment1.OuterPackLines.AddNew();
				line2.JL_LinePrice = 400;
				line2.JL_HarmonisedCode = "c2";

				ForwardingShipment shipment2 = GetShipmentWithRelatedOrg();
				shipment2.JS_UniqueConsignRef = "S2";
				consol.Shipments.Add(shipment2);
				shipment2.CustomsEntryNumber = "";
				PackLine line11 = shipment2.OuterPackLines.AddNew();
				line11.JL_HarmonisedCode = "c2";
				line11.JL_LinePrice = 1400;
				PackLine line22 = shipment2.OuterPackLines.AddNew();
				line22.JL_LinePrice = 1400;
				line22.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict) { CallBase = true };
					Factory.SetValue(() => queryProvider.Object);

					queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>())).Returns(false);
					queryProvider.Setup(m => m.PrintFinalMaster(consol, ZString.Empty));

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
					var supporter = new ForwardingConsolDocumentSupporter(consol);
					var documentEventsForTest = new DocumentEventsForTest(Factory);
					supporter.Initialise(documentEventsForTest);
					documentEventsForTest.OnDocumentPrintRequested();

					queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>())).Returns(false);
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
					supporter = new ForwardingConsolDocumentSupporter(consol);
					documentEventsForTest = new DocumentEventsForTest(Factory)
					{
						menu =
					{
						SU_MenuName = "Print Final Master", SU_IsSystemDefined = ZBool.True
					}
					};
					supporter.Initialise(documentEventsForTest);
					documentEventsForTest.OnDocumentPrintRequested();
					queryProvider.Verify(m => m.PrintFinalMaster(consol, ZString.Empty), Times.Once);
				}
				finally
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
				}
			}
		}

		[ExpectNoExceptions]
		public void TestMultipleShipmentsOneRequireConfirmation_IncorrectMenuName()
		{
			using (ObjectFactory.Get<US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 1000m))
			{
				ForwardingConsol consol = Factory.New<ForwardingConsol>();

				ForwardingShipment shipment1 = consol.Shipments.AddNew();
				shipment1.CustomsEntryNumber = "";
				PackLine line1 = shipment1.OuterPackLines.AddNew();
				line1.JL_HarmonisedCode = "c2";
				line1.JL_LinePrice = 400;
				PackLine line2 = shipment1.OuterPackLines.AddNew();
				line2.JL_LinePrice = 400;
				line2.JL_HarmonisedCode = "c2";

				ForwardingShipment shipment2 = GetShipmentWithRelatedOrg();
				consol.Shipments.Add(shipment2);
				shipment2.CustomsEntryNumber = "";
				PackLine line11 = shipment2.OuterPackLines.AddNew();
				line11.JL_HarmonisedCode = "c2";
				line11.JL_LinePrice = 1400;
				PackLine line22 = shipment2.OuterPackLines.AddNew();
				line22.JL_LinePrice = 1400;
				line22.JL_HarmonisedCode = "c2";

				ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				try
				{
					var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);
					Factory.SetValue(() => queryProvider.Object);

					queryProvider.Setup(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()));

					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
					var supporter = new ForwardingConsolDocumentSupporter(consol);
					var documentEventsForTest = new DocumentEventsForTest(Factory) { menu = { SU_MenuName = "doesnt start with BILL OF LADING" } };
					supporter.Initialise(documentEventsForTest);
					documentEventsForTest.OnDocumentPrintRequested();

					queryProvider.Verify(m => m.ConfirmBOLPrinting(It.IsAny<CommonShipment>()), Times.Never);
				}
				finally
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
				}
			}
		}

		ForwardingShipment GetShipmentWithRelatedOrg()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			OrgHeader consignor = Factory.New<OrgHeader>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			OrgHeader relatedParty = Factory.New<OrgHeader>();
			consignor.SetRelatedParty(relatedParty, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "SEA", "");
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = relatedParty.PK;
			return shipment;
		}

		#endregion

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_HouseBillOfLadingType = "FIA";

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_HouseBillOfLadingType = "FIA";
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.LCL;

			var shipment3 = Consol.Shipments.AddNew();
			shipment3.JS_HouseBillOfLadingType = "FIP";

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Malaysia);

			Consol.Containers.AddNew();

			Factory.Save();

			var deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent.OH_Code = "CCC";
			deliveryAgent.OH_FullName = "Delivery Agent";
			deliveryAgent.MainAddress.OA_Address1 = "Address 1";

			shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var accHeader1 = Factory.New<AccTransactionHeader>();
			accHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader1.AH_TransactionType = TransactionTypes.Invoice;
			accHeader1.AH_OH = deliveryAgent.PK;
			accHeader1.AH_InvoiceDate = ZDateTime.Now;
			accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader1.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;

			shipment2.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment2.JS_UniqueConsignRef = "S2";
			shipment2.JS_HouseBillOfLadingType = "FIA";
			shipment2.JS_NoCopyBills = 1;
			shipment2.JS_NoOriginalBills = 1;

			var accHeader2 = Factory.New<AccTransactionHeader>();
			accHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader2.AH_OH = deliveryAgent.PK;
			accHeader2.AH_InvoiceDate = ZDateTime.Now;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader2.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Delivery Agent Pack";

			Consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);

			return Consol;
		}

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable businessObject)
		{
			if (command.SU_MenuName == "MY Inward Manifest")
			{
				var shipment = ((ForwardingConsol)businessObject).Shipments.AddNew();

				var marksAndNumbers = "Creating Long Marks and numbers\n";

				for (int i = 0; i <= 40; i++)
				{
					marksAndNumbers += "Adding a line to Marks and numbers\n";
				}

				shipment.JS_MarksAndNumbers = marksAndNumbers;
			}
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return
				(documentCommand.SU_MenuName == "Print Final Master")
				|| documentCommand.SU_MenuName.StartsWith("Shipper Document Pack")
				|| documentCommand.SU_MenuPath.StartsWith("CMD")
				|| documentCommand.SU_MenuName.Contains("Cartage Advice")
				|| documentCommand.SU_MenuName.Contains("Profit Share Calculation Worksheet")
				|| documentCommand.SU_MenuName.Contains("Import Cargo Label")
				|| documentCommand.SU_MenuName == "Auto-Cost";

			//Doc Engine Bug - Remove Cartage Advice when doc engine bug is fixed
			//This Cartage Advice menu when run changes the parent of the menu to CommonContainer,
			//so then every other menu run after has CommonContainer as its parent BO when it should be
			//Fowarding Consol
		}

		public void TestSupportedDataContexts()
		{
			DataContextValueForTesting[] expectedDataContexts = new DataContextValueForTesting[]
			{
				new DataContextValueForTesting(Core.Constants.DataContext.BaseConsol),
				new DataContextValueForTesting(Core.Constants.DataContext.Consol),
				new DataContextValueForTesting(Core.Constants.DataContext.AWB),
				new DataContextValueForTesting(Core.Constants.DataContext.Container),
				new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice),
				new DataContextValueForTesting(Core.Constants.DataContext.Notes),
				new DataContextValueForTesting(Core.Constants.DataContext.ERA),
				new DataContextValueForTesting(Core.Constants.DataContext.LTL),
				new DataContextValueForTesting(Core.Constants.DataContext.IMO),
				new DataContextValueForTesting(Core.Constants.DataContext.ForwardingPreAdvice),
				new DataContextValueForTesting(Core.Constants.DataContext.NZCustoms),
				new DataContextValueForTesting(Core.Constants.DataContext.CHCustoms),
				new DataContextValueForTesting(Core.Constants.DataContext.ImportCargoLabel),
				new DataContextValueForTesting(Core.Constants.DataContext.LoadListDocument),
				new DataContextValueForTesting(Core.Constants.DataContext.ShippingOrder),
				new DataContextValueForTesting(Core.Constants.DataContext.CommonConsol),
				new DataContextValueForTesting(Core.Constants.DataContext.ForwardingConsol),
				new DataContextValueForTesting(Core.Constants.DataContext.CommonConsol),
				new DataContextValueForTesting(Core.Constants.DataContext.CartageAdvice),
				new DataContextValueForTesting(Core.Constants.DataContext.ForwardingShipmentAndConsol),
				new DataContextValueForTesting(Core.Constants.DataContext.RequestForMissingDocuments),
				new DataContextValueForTesting(Core.Constants.DataContext.TimeSlotRequest),
				new DataContextValueForTesting(Core.Constants.DataContext.CombinedCartageAdvice),
				new DataContextValueForTesting(Core.Constants.DataContext.ProfitShareDetail),
				new DataContextValueForTesting(Core.Constants.DataContext.ForwardingConsolGbADS),
				new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobInvoice),
				new DataContextValueForTesting(Core.Constants.DataContext.AsycudaManifestHeader),
				new DataContextValueForTesting(Core.Constants.DataContext.EuNcts)
			};

			DocumentSupporter documentSupporter = new ForwardingConsolDocumentSupporter(Factory.New<ForwardingConsol>());
			foreach (DataContextValueForTesting expectedDataContext in expectedDataContexts)
			{
				AssertEquals("Expecting to support " + expectedDataContext.FullDataContext, true, documentSupporter.IsDataContextSupported(expectedDataContext));
			}
		}

		public void TestGetDocumentWrappersInternalForGbAds()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDocumentSupporter = consol.DocumentSupporter;
			var gbWrappers = consolDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ForwardingConsolGbADS, null);
			AssertContains("When consol is asked for a doc of context ForwardingConsolGbADS, we see a non-null wrapper of the right type",
				"DocForwardingConsolGB", gbWrappers[0].GetType().FullName);
		}

		public void TestGetDocumentWrappersInternalForAWBHeader()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals("Prerequisite:", false, consol.IsAWBHeaderAccessible);
			var consolDocumentSupporter = consol.DocumentSupporter;
			var awbWrappers = consolDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AWB, null);
			AssertNull(awbWrappers);
		}

		public void TestGetDocumentWrappersInternalForAsycudaManifestHeader()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDocumentSupporter = consol.DocumentSupporter;
			var wrappers = consolDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AsycudaManifestHeader, null);
			AssertNull(wrappers);
			var manifest = (BusinessObject)Factory.New<ASYCUDA.IAsycudaManifestHeader>();
			manifest[AsycudaManifestHeaderSchema.AMA_ParentId] = consol.PK;
			manifest[AsycudaManifestHeaderSchema.AMA_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			wrappers = consolDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.AsycudaManifestHeader, null);
			AssertContains("Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeaderDocWrapper", wrappers[0].GetType().FullName);
		}

		public void TestGetDocumentWrappersInternalForCHCustoms()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDocumentSupporter = consol.DocumentSupporter;
			var wrappers = consolDocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CHCustoms, null);
			AssertEquals("count", 1, wrappers.Length);
			AssertEquals("DocForwardingConsol", "Enterprise.Customs.CH.Business.DocForwardingConsol", wrappers[0].GetType().FullName);
		}

		public void TestCanPrintChGroupDeliveryNoteForConsol() => CombineAssertions(() =>
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolDocumentSupporter = new ForwardingConsolDocumentSupporterForTest(consol);

			var menuItemMock = new Mock<IStmMenuItem>();
			menuItemMock.Setup(m => m.PK).Returns(new ZGuid(ForwardingConsolDocumentSupporter.ChGroupDeliveryNoteStmMenuItemPK));
			var eventArgs = new DocumentCancelEventArgs(menuItemMock.Object);

			var chHelperMock = new Mock<IForwardingConsolIntegrationHelper>();
			ObjectFactory.Substitute(chHelperMock.Object);

			eventArgs.Cancel = false;
			chHelperMock.Setup(h => h.CanPrintGroupDeliveryNoteForConsol(It.IsAny<IForwardingConsol>(), It.IsAny<IDocumentSupporterQueryProvider>())).Returns(false);
			consolDocumentSupporter.DocumentEventSource_DocumentPrintRequested_Exposed(null, eventArgs);
			chHelperMock.Verify(h => h.CanPrintGroupDeliveryNoteForConsol(consol, consolDocumentSupporter.QueryProvider));
			AssertEquals("Cannot print -> eventArgs.Cancel", true, eventArgs.Cancel);

			eventArgs.Cancel = false;
			chHelperMock.Invocations.Clear();
			chHelperMock.Setup(h => h.CanPrintGroupDeliveryNoteForConsol(It.IsAny<IForwardingConsol>(), It.IsAny<IDocumentSupporterQueryProvider>())).Returns(true);
			consolDocumentSupporter.DocumentEventSource_DocumentPrintRequested_Exposed(null, eventArgs);
			chHelperMock.Verify(h => h.CanPrintGroupDeliveryNoteForConsol(consol, consolDocumentSupporter.QueryProvider));
			AssertEquals("Can print  -> eventArgs.Cancel", false, eventArgs.Cancel);
		});

		#region Consol Agent Doc Pack

		public void TestGetChildCollectionMenuIsConsolAgentPack()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S0002001";
			shipment2.JS_UniqueConsignRef = "S0002002";
			shipment1.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment2.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment1;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			StmMenuItem consolAgentPackMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Consol Agent Pack"));
			consolAgentPackMenu.SU_MenuName = "Consol Agent Pack (Sea)";
			AssertChildCollectionsCount(consol, consolAgentPackMenu, 0);

			OrgHeader consolAgentOrg = Factory.New<OrgHeader>();
			consolAgentOrg.OH_FullName = "Consol Agent";
			consolAgentOrg.MainAddress.OA_Address1 = "Address 1";

			consol.JK_OA_ReceivingForwarderAddress = consolAgentOrg.MainAddress.PK;
			AssertChildCollectionsCount(consol, consolAgentPackMenu, 0);

			AccTransactionHeader accHeader = Factory.New<AccTransactionHeader>();
			accHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader.AH_OH = consolAgentOrg.PK;
			accHeader.AH_InvoiceDate = ZDateTime.Now;
			accHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader.AH_JH = job.PK;
			accHeader.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef;

			AssertChildCollectionsCount(consol, consolAgentPackMenu, 1);
		}

		void AssertChildCollectionsCount(ForwardingConsol consol, StmMenuItem consolAgentPackMenu, int expectedCount)
		{
			IDocumentSupportable[] docsForConsolAgentContext = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Expected number of shipments with consol agents context", expectedCount, docsForConsolAgentContext.Length);

			IDocumentSupportable[] docsForShipmentContext = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.Shipment, ShipmentDocBuilderMenuItem);
			AssertEquals("Expected number of shipments with shipment context", expectedCount, docsForShipmentContext.Length);
		}

		public void TestJobNumberOfGetChildCollectionMenuIsConsolAgentPack()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0002001";
			shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = shipment;
			consol.Shipments.Add(shipment);

			StmMenuItem consolAgentPackMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Consol Agent Pack"));
			consolAgentPackMenu.SU_MenuName = "Consol Agent Pack (Sea)";
			IDocumentSupportable[] docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 0, docs.Length);

			OrgHeader consolAgent = Factory.New<OrgHeader>();
			consolAgent.OH_FullName = "Consol Agent";
			consolAgent.MainAddress.OA_Address1 = "Address 1";

			consol.JK_OA_ReceivingForwarderAddress = consolAgent.MainAddress.PK;
			docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 0, docs.Length);

			AccTransactionHeader accHeader = Factory.New<AccTransactionHeader>();
			accHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader.AH_OH = consolAgent.PK;
			accHeader.AH_InvoiceDate = ZDateTime.Now;
			accHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader.AH_JH = job.PK;
			accHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 1, docs.Length);
			AssertEquals("Shipment should be found", shipment.PK, docs[0].DocumentSupporter.PK);

			accHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef + "/A";
			docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 1, docs.Length);
			AssertEquals("Shipment should be found", shipment.PK, docs[0].DocumentSupporter.PK);

			accHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef + "01";
			docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 0, docs.Length);

			accHeader.AH_JH = ZGuid.Empty;
			docs = consol.DocumentSupporter.GetChildCollection(consolAgentPackMenu, BusinessContext.ConsolAgent, null);
			AssertEquals("Shipments with consol agents", 0, docs.Length);
		}

		public void TestJobNumberOfGetChildCollectionMenuIsConsolAgentPack_ShipmentBusinesContext()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S0002001";
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0002002";

			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.Parent = shipment1;
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.Parent = shipment2;

			StmMenuItem consolMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Consol Agent Pack"));
			consolMenu.SU_MenuName = "Consol Agent Pack (Sea)";

			OrgHeader consolAgent = Factory.New<OrgHeader>();
			consolAgent.OH_FullName = "Consol Agent";
			consolAgent.MainAddress.OA_Address1 = "Address 1";
			consol.JK_OA_ReceivingForwarderAddress = consolAgent.MainAddress.PK;

			AccTransactionHeader accHeader1 = Factory.New<AccTransactionHeader>();
			accHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader1.AH_TransactionType = TransactionTypes.Invoice;
			accHeader1.AH_OH = consolAgent.PK;
			accHeader1.AH_InvoiceDate = ZDateTime.Now;
			accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader1.AH_JH = job1.PK;
			accHeader1.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef + "/A";

			AccTransactionHeader accHeader2 = Factory.New<AccTransactionHeader>();
			accHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = TransactionTypes.Invoice;
			accHeader2.AH_OH = consolAgent.PK;
			accHeader2.AH_InvoiceDate = ZDateTime.Now;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader2.AH_JH = job2.PK;
			accHeader2.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;

			IDocumentSupportable[] docs = consol.DocumentSupporter.GetChildCollection(consolMenu, BusinessContext.Shipment, ShipmentDocBuilderMenuItem);
			AssertNotNull("Should be able to load child collection from Congol Agent Pack Menu", docs);
			AssertEquals("Should find the attached documents", 2, docs.Length);
			AssertEquals("Document should belong to shipment 1", shipment1.PK, docs[0].DocumentSupporter.PK);
			AssertEquals("Document should belong to shipment 2", shipment2.PK, docs[1].DocumentSupporter.PK);
		}

		StmMenuItem ShipmentDocBuilderMenuItem
		{
			get
			{
				var query = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice");
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Shipment);

				return Factory.LoadTop1<StmMenuItem>(query);
			}
		}

		#endregion

		public void TestDeliveryAgentsFallbackOnConsolReceivingAgent()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Delivery Agent Pack";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingConsolDocumentSupporter documentSupporter = (ForwardingConsolDocumentSupporter)consol.DocumentSupporter;

			consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals(0, documentSupporter.DeliveryAgentsToPrint.Count);

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Consol Receiving Forwarder";
			receivingForwarder.MainAddress.OA_Address1 = "Address 1";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals(0, documentSupporter.DeliveryAgentsToPrint.Count);

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertContainsExactElementsInAnyOrder(new[] { receivingForwarder.PK }, documentSupporter.DeliveryAgentsToPrint.Select((agent) => agent.PK));

			OrgHeader deliveryAgent1 = Factory.New<OrgHeader>();
			deliveryAgent1.OH_FullName = "Shipment 1 Delivery Agent";
			deliveryAgent1.MainAddress.OA_Address1 = "Address 2";

			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;

			consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertContainsExactElementsInAnyOrder(new[] { receivingForwarder.PK, deliveryAgent1.PK }, documentSupporter.DeliveryAgentsToPrint.Select((agent) => agent.PK));

			OrgHeader deliveryAgent2 = Factory.New<OrgHeader>();
			deliveryAgent2.OH_FullName = "Shipment 1 Delivery Agent";
			deliveryAgent2.MainAddress.OA_Address1 = "Address 2";

			shipment2.JS_OH_DeliveryAgent = deliveryAgent2.PK;

			consol.DocumentSupporter.GetDataStateBeforeRun(menuItem);
			AssertContainsExactElementsInAnyOrder(new[] { deliveryAgent1.PK, deliveryAgent2.PK }, documentSupporter.DeliveryAgentsToPrint.Select((agent) => agent.PK));
		}

		public void TestGetDocBusinessObjectForGenericFreightJob()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotEquals("Wrappers with Consol", null, wrappers);
			AssertEquals("Wrappers.Length with Consol", 1, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Consol", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Consol", consol.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetDocBusinessObjectForConsolAgentPack()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Consol Agent Pack (for testing)";

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Consol Receiving Forwarder";
			receivingForwarder.MainAddress.OA_Address1 = "Address 1";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Consol Sending Forwarder";
			sendingForwarder.MainAddress.OA_Address1 = "Address 2";

			Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			Consol.JK_UniqueConsignRef = "C12345";

			DocumentWrapper[] wrappers = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);
			AssertNull("No invoices to wrap", wrappers);

			AccTransactionHeader accHeader1 = Factory.New<AccTransactionHeader>();
			accHeader1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader1.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader1.AH_OH = receivingForwarder.PK;
			accHeader1.AH_InvoiceDate = ZDateTime.Now;
			accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader1.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;

			wrappers = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("One wrapper", 1, wrappers.Length);

			AccTransactionHeader accHeader2 = Factory.New<AccTransactionHeader>();
			accHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader2.AH_OH = Consol.SendingForwarder.PK;
			accHeader2.AH_InvoiceDate = ZDateTime.Now;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader2.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;

			wrappers = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("Still one wrapper as invoice 2 is for the sending forwarder", 1, wrappers.Length);

			AccTransactionHeader accHeader3 = Factory.New<AccTransactionHeader>();
			accHeader3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader3.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader3.AH_OH = Consol.ReceivingForwarder.PK;
			accHeader3.AH_InvoiceDate = ZDateTime.Now;
			accHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader3.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader3.AH_ConsolidatedInvoiceRef = Consol.JK_UniqueConsignRef;

			wrappers = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("Two invoices for the receiving forwarder", 2, wrappers.Length);
		}

		public void TestHasUncontainerisedHazPackLines()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingConsolDocumentSupporterForTest documentSupporter = new ForwardingConsolDocumentSupporterForTest(consol);
			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			CommonContainer container = consol.Containers.AddNew();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			PackLine packLine2 = shipment1.OuterPackLines.AddNew();
			container.PackLines.Add(packLine1);
			container.PackLines.Add(packLine2);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine packLine3 = shipment2.OuterPackLines.AddNew();
			PackLine packLine4 = shipment2.OuterPackLines.AddNew();
			container.PackLines.Add(packLine3);
			container.PackLines.Add(packLine4);

			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			container.PackLines.Remove(packLine3);
			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			packLine3.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			packLine4.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			container.PackLines.Remove(packLine4);
			AssertEquals(true, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));

			container.PackLines.Add(packLine3);
			AssertEquals(false, consol.Shipments.Cast<ForwardingShipment>().Any(documentSupporter.HasUncontainerisedHazPackLines));
		}

		public void TestGetIMOWrappers()
		{
			var consol = Factory.New<ForwardingConsol>();

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict) { CallBase = true };

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions())
				.CallBase();

			var wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, null);
			AssertNull(wrappers);

			queryProvider.Verify(
				m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false), Times.Once);

			ForwardingContainer container = consol.Containers.AddNew();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			container.PackLines.Add(packLine1);

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_RH_NKCommodityCode = Constants.CargoTypes.Hazardous;
			container.PackLines.Add(packLine2);

			Factory.Save();

			queryProvider
				.SetupSequence(m =>
					m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false))
				.Returns(new ContainersToPrintOptions { ContainersToPrint = new[] { container } });

			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, null);
			AssertNotNull(wrappers);

			queryProvider.Verify(m =>
					m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), false),
				Times.Exactly(2));

			container.PackLines.Remove(packLine1);
			container.PackLines.Remove(packLine2);

			queryProvider
				.SetupSequence(
					m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), true))
				.Returns(new ContainersToPrintOptions { ContainersToPrint = new[] { container } });
			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.IMO, null);
			AssertNotNull(wrappers);

			queryProvider
				.Verify(m => m.GetContainersToPrint(It.IsAny<ContainerToSelectFromForPrintingCollection>(), true),
					Times.Once);
		}

		public void TestGetSupportedBOBusinessSource()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals("ForwardingConsol is supported", true, consol.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ForwardingConsol)));
		}

		#region Test GetChildCollection For ExportCartageAdvice

		public void TestGetChildCollectionForImportCartageAdvice()
		{
			ForwardingConsol forwardingConsol = Factory.New<ForwardingConsol>();

			DocumentZQuery query = new DocumentZQuery("Consol");
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");
			query.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotEqual, "HIDE=Y");
			query.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.ARV));

			var cartageAdviceMenu = Factory.LoadTop1<StmMenuItem>(query);

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);
			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false))
				.Returns(new ContainersToPrintOptions())
				.CallBase();

			var docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol", 0, docs.Length);

			queryProvider
				.Verify(m =>
						m.GetContainersToPrint(
							It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false),
					Times.Once);

			forwardingConsol.JK_RL_NKDischargePort = "USLAX";
			forwardingConsol.JK_RL_NKLoadPort = "AUSYD";

			queryProvider
				.SetupSequence(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false))
				.Returns(new ContainersToPrintOptions())
				.CallBase();

			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol", 0, docs.Length);

			queryProvider
				.Verify(m =>
						m.GetContainersToPrint(
							It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false),
					Times.Exactly(2));

			ForwardingContainer cont1 = forwardingConsol.Containers.AddNew();
			Factory.Save(); //Temp Factory Loads containers into ContainersToSelectFrom

			queryProvider.SetupSequence(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 1), false))
				.Returns(new ContainersToPrintOptions() { ContainersToPrint = new[] { cont1 } })
				.CallBase();
			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol should be 1", 1, docs.Length);

			queryProvider.Verify(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 1), false),
				Times.Once);

			ForwardingContainer cont2 = forwardingConsol.Containers.AddNew();
			Factory.Save(); //Temp Factory Loads containers into ContainersToSelectFrom

			queryProvider
				.SetupSequence(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false))
				.Returns(new ContainersToPrintOptions() { ContainersToPrint = new[] { cont1, cont2 } })
				.CallBase();

			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol should be 2", 2, docs.Length);

			queryProvider
				.Verify(m =>
						m.GetContainersToPrint(
							It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false),
					Times.Once);

			queryProvider.SetupSequence(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false))
				.Returns((ContainersToPrintOptions)null)
				.CallBase();

			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers should return 0 as continue is false", 0, docs.Length);

			queryProvider.Verify(m =>
					m.GetContainersToPrint(
						It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false),
				Times.Exactly(2));
		}

		public void TestGetChildCollectionForExportCartageAdvice()
		{
			ForwardingConsol forwardingConsol = Factory.New<ForwardingConsol>();

			DocumentZQuery query = new DocumentZQuery("Consol");
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Cartage Advice");
			query.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.NotEqual, "HIDE=Y");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, "Consol");
			query.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, SQLComparisonOperator.Equal, nameof(DocumentDirection.DEP));

			var cartageAdviceMenu = Factory.LoadTop1<StmMenuItem>(query);

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>();
			Factory.SetValue(() => queryProvider.Object);

			queryProvider.Setup(m => m.GetContainersToPrint(It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false))
				.Returns(new ContainersToPrintOptions());//.Repeat.Once();
			IDocumentSupportable[] docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol", 0, docs.Length);

			forwardingConsol.JK_RL_NKDischargePort = "AUSYD";
			forwardingConsol.JK_RL_NKLoadPort = "USLAX";

			queryProvider.Setup(m => m.GetContainersToPrint(It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 0), false))
				.Returns(new ContainersToPrintOptions());//.Repeat.Once();
			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol", 0, docs.Length);

			ForwardingContainer cont1 = forwardingConsol.Containers.AddNew();
			Factory.Save(); //Temp Factory Loads containers into ContainersToSelectFrom

			queryProvider.Setup(m => m.GetContainersToPrint(It.Is<ContainerToSelectFromForPrintingCollection>((c) => c != null && c.Count == 1), false))
				.Returns(new ContainersToPrintOptions() { ContainersToPrint = new[] { cont1 } });//.Repeat.Once();
			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol should be 1", 1, docs.Length);

			ForwardingContainer cont2 = forwardingConsol.Containers.AddNew();
			Factory.Save(); //Temp Factory Loads containers into ContainersToSelectFrom

			queryProvider.Setup(m => m.GetContainersToPrint(It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false))
				.Returns(new ContainersToPrintOptions() { ContainersToPrint = new[] { cont1, cont2 } });//.Repeat.Once();
			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers on consol should be 2", 2, docs.Length);

			queryProvider.Setup(m => m.GetContainersToPrint(It.Is<ContainerToSelectFromForPrintingCollection>(c => c != null && c.Count == 2), false))
				.Returns((ContainersToPrintOptions)null);//.Repeat.Once();
			docs = forwardingConsol.DocumentSupporter.GetChildCollection(cartageAdviceMenu, BusinessContext.ForwardingContainer, null);
			AssertEquals("Containers should return 0 as continue is false", 0, docs.Length);
		}

		#endregion

		public void TestGetFilterValue_JPAFR()
		{
			Consol.JK_RL_NKDischargePort = "JPTKI";
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKDischargePort = "";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKFirstForeignPort = "JPTKI";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKFirstForeignPort = "";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKLastForeignPort = "JPTKI";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKLastForeignPort = "";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKPortOfFirstArrival = "JPTKI";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_RL_NKPortOfFirstArrival = "";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTKI";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "";
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTZU";
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
			Consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			var testLeg = Consol.Transports.AddNew();
			testLeg.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testLeg.JW_RL_NKLoadPort = "JPTKI";
			testLeg.JW_RL_NKDiscPort = "JPTKY";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("is eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
			Consol.Transports.MostInterestingTransport.Delete();
			AssertEquals("is eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.JPAFR));
		}

		public void TestGetFilterValue_CAeManifest()
		{
			Consol.JK_RL_NKDischargePort = "CATOR";
			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("is eligable", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CAeManifest));
			Consol.JK_RL_NKLoadPort = "CABLO";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CAeManifest));
			Consol.JK_RL_NKDischargePort = "CNHSA";
			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("is not eligable", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CAeManifest));
		}

		public void TestAdditionalBODataSourceDocumentSupportersGetFilter()
		{
			AssertEquals("NCTSTransitDocumentsSupport", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var consol = Factory.New<ForwardingConsol>();
				var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = consol.PK;
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader[CusInBondHeaderSchema.Constants.BH_HeaderType] = "D";

				AssertEquals("NCTSTransitDocumentsSupport", "Y", consol.DocumentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));
			}
		}

		public void TestAdditionalBODataSourceDocumentSupportersGetFilterPhase5()
		{
			AssertEquals("NCTSTransitDocumentsSupport", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var consol = Factory.New<ForwardingConsol>();
				var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = consol.PK;
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader[CusInBondHeaderSchema.Constants.BH_HeaderType] = "D";

				AssertEquals("NCTSTransitDocumentsSupport", "Y", consol.DocumentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));
			}
		}

		public void TestMenuTemplateFilterValuesForPrintStandardDocument()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Consol, null)[0];
			ZString result = consol.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandard, wrapper);
			AssertEquals("Filter result for printing Standard Document", new ZString("Y"), result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecificDocument()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Consol, null)[0];
			ZString result = consol.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecific, wrapper);
			AssertEquals("Filter result for printing Client Specifc Document", new ZString("N"), result);
		}

		public void TestGetDocBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Consol, null);
			AssertEquals("Consol wrapper should be created", 1, wrapper.Length);
			AssertNotNull("Consol wrapper should be created", wrapper[0]);
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, null);
			AssertNull("Consol wrapper array is null", wrapper);

			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals("Consol wrapper should be created", 1, wrapper.Length);
			Assert("Consol wrapper for Notes data context", typeof(ForwardingConsol).IsAssignableFrom(wrapper[0].WrappedObject.GetType()));
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForDataContextCommonConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommonConsol, null);
			AssertEquals("Consol wrapper should be created", 1, wrapper.Length);
			AssertNotNull("Consol wrapper should be created", wrapper[0]);
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestUnContainerisedHAZShipments()
		{
			ForwardingConsolForTest consol = Factory.New<ForwardingConsolForTest>();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();

			CommonShipment shipment1 = consol.Shipments.AddNew();
			PackLine line1 = shipment1.OuterPackLines.AddNew();
			PackLine line2 = shipment1.OuterPackLines.AddNew();

			CommonShipment shipment2 = consol.Shipments.AddNew();
			PackLine line3 = shipment2.OuterPackLines.AddNew();
			PackLine line4 = shipment2.OuterPackLines.AddNew();

			CommonShipment shipment3 = consol.Shipments.AddNew();
			PackLine line5 = shipment3.OuterPackLines.AddNew();

			line1.JL_Description = "1";
			line2.JL_Description = "2";
			line3.JL_Description = "3";
			line4.JL_Description = "4";
			line5.JL_Description = "5";
			line1.JL_RH_NKCommodityCode = "GEN";
			line2.JL_RH_NKCommodityCode = "HAZ";
			line3.JL_RH_NKCommodityCode = "HAZ";
			line4.JL_RH_NKCommodityCode = "GEN";
			line5.JL_RH_NKCommodityCode = "GEN";

			line1.SetContainer(consol, container1);
			line2.SetContainer(consol, container2);
			line3.SetContainer(consol, null);
			line4.SetContainer(consol, null);
			line5.SetContainer(consol, null);

			AssertEquals("Should have one uncontainerised HAZ shipment", 1, consol.UnContainerisedHazShipments.Count);
		}

		public void TestGetDocBusinessObjectForARInvoice()
		{
			var receivingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			InvoiceCreationTestHelper invoiceCreator = new InvoiceCreationTestHelper(Factory);
			AccTransactionHeader header = invoiceCreator.SetupTransaction(GlbBranch.CurrentBranch, "00001001", null, consol.JK_UniqueConsignRef, ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);

			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertNull("ARInvoice wrapper array is null", wrapper);

			header.AH_OH = consol.ReceivingForwarderPK;
			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("ARInvoice wrapper should be created", 1, wrapper.Length);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[0]);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[0].GetType().Name);

			AccTransactionHeader header2 = invoiceCreator.SetupTransaction(GlbBranch.CurrentBranch, "00001001", receivingForwarder, consol.JK_UniqueConsignRef + "/A", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.CreditNote);
			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("ARInvoice wrapper should be created", 2, wrapper.Length);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[0]);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[1]);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[1].GetType().Name);
		}

		public void TestGetDocBusinessObjectForImportCargoLabel()
		{
			var consol = Factory.New<ForwardingConsol>();

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);

			Factory.SetValue(() => queryProvider.Object);

			queryProvider
				.SetupSequence(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()))
				.Returns((DocumentImportCargoLabel)null)
				.CallBase();
			DocumentWrapper[] wrapper = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ImportCargoLabel, null);
			AssertEquals("Import Cargo Label wrapper should be null", null, wrapper);

			queryProvider
				.Verify(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()), Times.Once);

			queryProvider
				.SetupSequence(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()))
				.Returns(new DocumentImportCargoLabel(consol))
				.CallBase();
			wrapper = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ImportCargoLabel, null);
			AssertEquals("Import Cargo Label wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
			queryProvider
				.Verify(m => m.GetImportCargoLabelToPrint(It.IsAny<DocumentImportCargoLabel>()), Times.Exactly(2));
		}

		public void TestGetDocBusinessObjectForRequestForMissingDocs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForMissingDocuments, null);
			AssertEquals("Consol Wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForTimeSlotRequest()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TimeSlotRequest, null);
			AssertEquals("Consol Wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocForwardingConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForCommonConsol()
		{
			var consol = Factory.New<ForwardingConsol>();

			var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>();

			Factory.SetValue(() => queryProvider.Object);

			queryProvider.Setup(m => m.GetConsolToPrint(It.IsAny<DocumentCommonConsol>())).Returns((DocumentCommonConsol)null);//.Repeat.Once();

			DocumentWrapper[] wrapper = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LoadListDocument, null);
			AssertEquals("Consol wrapper should be null", null, wrapper);
			queryProvider.Setup(m => m.GetConsolToPrint(It.IsAny<DocumentCommonConsol>())).Returns(new DocumentCommonConsol(consol, Core.Constants.DataContext.LoadListDocument));//.Repeat.Once();

			wrapper = Consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LoadListDocument, null);
			AssertEquals("Consol wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForShippingOrder()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShippingOrder, null);
			wrapper = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShippingOrder, null);
			AssertEquals("Consol wrapper should be created", 1, wrapper.Length);
			AssertEquals("Wrapper type should be DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
		}

		public void TestGetDocBusinessObjectForStandardShippingNote()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			DocumentWrapper[] wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ForwardingConsol, null);
			AssertEquals("Single wrapper created", 1, wrappers.Length);
			AssertEquals("Wrapper is of type DocForwardingConsol", "DocForwardingConsol", wrappers[0].GetType().Name);

			ForwardingContainer container1 = consol.Containers.AddNew();
			ForwardingContainer container2 = consol.Containers.AddNew();
			ForwardingContainer container3 = consol.Containers.AddNew();

			Factory.Save();

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "Standard Shipping Note";  // This is a document menu name
			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ForwardingConsol, menuItem);

			AssertEquals("Multiple wrappers created", 3, wrappers.Length);
			AssertEquals("First wrapper is of type DocForwardingConsol", "DocForwardingConsol", wrappers[0].GetType().Name);
			AssertEquals("Second wrapper is of type DocForwardingConsol", "DocForwardingConsol", wrappers[1].GetType().Name);
			AssertEquals("Third wrapper is of type DocForwardingConsol", "DocForwardingConsol", wrappers[2].GetType().Name);

			menuItem.SU_MenuName = "Some menu item";  // This is a document menu name
			wrappers = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ForwardingConsol, menuItem);

			AssertEquals("Single wrapper created", 1, wrappers.Length);
			AssertEquals("Wrapper is of type DocForwardingConsol", "DocForwardingConsol", wrappers[0].GetType().Name);
		}

		public void TestDeliveryAgentsToSelectFrom()
		{
			ForwardingConsolForTest consol = Factory.New<ForwardingConsolForTest>();

			ForwardingShipment shipment1 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			ForwardingShipment shipment2 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
			ForwardingShipment shipment3 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));

			AssertEquals("Delivery Agents to select from count", 0, consol.DeliveryAgentsToSelectFrom.Count);

			OrgHeader org1 = Factory.New<OrgHeader>();
			OrgHeader org2 = Factory.New<OrgHeader>();

			shipment1.JS_OH_DeliveryAgent = org1.PK;
			shipment2.JS_OH_DeliveryAgent = org2.PK;
			shipment3.JS_OH_DeliveryAgent = org2.PK;

			AssertEquals("Delivery Agents to select from count", 2, consol.DeliveryAgentsToSelectFrom.Count);
			Assert("Delivery Agents to select from does contain Org1", consol.DeliveryAgentsToSelectFrom.Contains(org1.PK));
			Assert("Delivery Agents to select from does contain Org2", consol.DeliveryAgentsToSelectFrom.Contains(org2.PK));
		}

		public void TestWhenDeliveryAgentIsTheSameAsReceivingForwarder()
		{
			var consol = Factory.New<ForwardingConsolForTest>();

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();

			AssertEquals("Delivery Agents to select from count", 0, consol.DeliveryAgentsToSelectFrom.Count);

			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;
			shipment1.JS_OH_DeliveryAgent = org1.PK;
			shipment2.JS_OH_DeliveryAgent = org2.PK;
			shipment3.JS_OH_DeliveryAgent = org2.PK;

			AssertEquals("Delivery Agents to select from count", 2, consol.DeliveryAgentsToSelectFrom.Count);
			Assert("Delivery Agents to select from does contain Org1", consol.DeliveryAgentsToSelectFrom.Contains(org1.PK));
			Assert("Delivery Agents to select from does contain Org2", consol.DeliveryAgentsToSelectFrom.Contains(org2.PK));
		}

		public void TestDeliveryAgentWhenConsolReceivingAgentIsSameAsShipmentDeliveryAgent()
		{
			var consol = Factory.New<ForwardingConsolForTest>();
			var shipment = consol.Shipments.AddNew();
			var agent = Factory.NewWithValidTestData<OrgHeader>();

			consol.JK_OA_ReceivingForwarderAddress = agent.Addresses[0].PK;
			shipment.JS_OH_DeliveryAgent = agent.PK;

			Assert("Precondition: Consol and Shipment to share delivery agent/receiving forwarder", shipment.DeliveryAgent == consol.ReceivingForwarder);
			Assert("Documents should be able to be delivered even if same agent on Consol and Shipment", consol.DeliveryAgentsToSelectFrom.Contains(shipment.DeliveryAgent));
			Assert("Delivery Agents to select from does contain Org2", consol.DeliveryAgentsToSelectFrom.Contains(agent.PK));
		}

		public void TestDeliveryAgentWhenConsolReceivingAgentIsEmpty()
		{
			var consol = Factory.New<ForwardingConsolForTest>();
			var shipment = consol.Shipments.AddNew();
			AssertEquals("Precondition: Consol has no Receiving Agent", 0, consol.DeliveryAgentsToSelectFrom.Count);

			shipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			Assert("Consol should be able to deliver to shipment's Delivery Agent", consol.DeliveryAgentsToSelectFrom.Contains(shipment.DeliveryAgent));
		}

		public void TestContainersToSelectFrom()
		{
			ForwardingConsolForTest consol = Factory.New<ForwardingConsolForTest>();

			CommonContainer con1 = consol.Containers.AddNew();
			con1.JC_ContainerNum = "C1";
			con1.JC_ContainerMode = Constants.ContainerModes.FCL;

			CommonContainer con2 = consol.Containers.AddNew();
			con2.JC_ContainerNum = "C2";
			con2.JC_ContainerMode = Constants.ContainerModes.LCL;
			Factory.Save();

			AssertEquals("Containers To select from count", 2, consol.ContainersToSelectFrom.Count);
			AssertEquals("Container To select from contains Con1", con1.PK, consol.ContainersToSelectFrom[0].Container.PK);
			AssertEquals("Container To select from contains Con2", con2.PK, consol.ContainersToSelectFrom[1].Container.PK);
		}

		public void TestBusinessContext()
		{
			AssertEquals("Business context should be 'Consol'", BusinessContext.Consol, Consol.DocumentSupporter.BusinessContext);
		}

		public void TestSupportedChildBusinessContexts()
		{
			AssertEquals(7, Consol.DocumentSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals(BusinessContext.Shipment, Consol.DocumentSupporter.SupportedChildBusinessContexts[0]);
			AssertEquals(BusinessContext.DeliveryAgent, Consol.DocumentSupporter.SupportedChildBusinessContexts[1]);
			AssertEquals(BusinessContext.ConsolAgent, Consol.DocumentSupporter.SupportedChildBusinessContexts[2]);
			AssertEquals(BusinessContext.ForwardingContainer, Consol.DocumentSupporter.SupportedChildBusinessContexts[3]);
			AssertEquals(BusinessContext.JPAFRHeader, Consol.DocumentSupporter.SupportedChildBusinessContexts[4]);
			AssertEquals(BusinessContext.CusMAWB, Consol.DocumentSupporter.SupportedChildBusinessContexts[5]);
			AssertEquals(BusinessContext.CAeManifest, Consol.DocumentSupporter.SupportedChildBusinessContexts[6]);
		}

		public void TestGetFilterValue()
		{
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("MOD filter should return 'AIR'", Enterprise.Core.Constants.TransportModes.Air, Consol.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("BCN filter should return 'N'", "N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.BCN));

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("BCN filter should return 'Y'", "Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.BCN));
		}

		public void TestMYDOFilterValue()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();

			shipment1.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment2.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			Consol.Shipments.Add(shipment1);
			Consol.Shipments.Add(shipment2);
			AssertEquals("N", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.MYDO));

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			GlbCompany.CurrentCompany.SetCountry("MY");
			AssertEquals("Y", Consol.DocumentSupporter.GetFilterValue(DocumentFilters.MYDO));
			GlbCompany.CurrentCompany.SetCountry(countryCode);
		}

		public void TestCTYFilterValue()
		{
			AssertEquals("CTY filter should return current country code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
		}

		public void TestCTYMODFilterValue()
		{
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("CTYMOD filter should return 'AUAIR'", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Enterprise.Core.Constants.TransportModes.Air, Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CTYMOD));

			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("CTYMOD filter should return 'AUSEA'", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Enterprise.Core.Constants.TransportModes.Sea, Consol.DocumentSupporter.GetFilterValue(DocumentFilters.CTYMOD));
		}

		public void TestDocumentMenuItemVisibilityDependingOnImportExportState()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "NZ";
			GlbBranch.CurrentBranch.SetCountry("NZ");
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals(Directions.Import, Consol.JobDirection);

			var documentCommands = new DocumentCommandCollection(Consol);
			documentCommands.Load();

			var documentCommand = documentCommands.Cast<DocumentCommand>().First(o => o.SU_MenuName == "MPI Application Cover Sheet");
			Assert(documentCommand.IsApplicable);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			documentCommands = new DocumentCommandCollection(Consol);

			documentCommands.Load();
			Assert(!documentCommand.IsApplicable);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			GlbBranch.CurrentBranch.SetCountry("AU");

			documentCommands = new DocumentCommandCollection(Consol);
			documentCommands.Load();

			AssertNotEquals(Directions.Import, Consol.JobDirection);

			documentCommand = documentCommands.Cast<DocumentCommand>().First(o => o.SU_MenuName == "MPI Application Cover Sheet");
			Assert(!documentCommand.IsApplicable);
		}

		public void TestGetChildCollectionForERA()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();

			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			shipment1.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			shipment2.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			DocumentZQuery filter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Export Receival Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AUBLK");

			var shipmentMenu = Factory.LoadTop1<StmMenuItem>(filter);
			IDocumentSupportable[] docs = consol.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("GetChildCollectionForERA should return 2 ForwardingShipments", 2, docs.Length);
			Assert("Returned Type should be ForwardingShipment", typeof(ForwardingShipment).IsAssignableFrom(docs[0].GetType()));
			AssertEquals("CurrentConsol should be Consol for Shipment 1", consol.PK, ((ForwardingShipment)docs[0]).CurrentConsolForDocuments.PK);
			AssertEquals("CurrentConsol should be Consol for Shipment 2", consol.PK, ((ForwardingShipment)docs[1]).CurrentConsolForDocuments.PK);
		}

		public void TestGetChildCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			var shipmentMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "Pre-Alert"));
			IDocumentSupportable[] docs = consol.DocumentSupporter.GetChildCollection(shipmentMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 2, docs.Length);

			var northKCTPortMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "NorthPort Delivery Order"));
			docs = consol.DocumentSupporter.GetChildCollection(northKCTPortMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 0, docs.Length);

			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Constants.ContainerModes.LCL;
			GlbCompany.CurrentCompany.SetCountry("MY");
			docs = consol.DocumentSupporter.GetChildCollection(northKCTPortMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);
			GlbCompany.CurrentCompany.SetCountry(countryCode);

			var northPKDPPortMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "NorthPort PKDP"));
			docs = consol.DocumentSupporter.GetChildCollection(northPKDPPortMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);

			var westPortMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "WestPort Delivery Order"));
			docs = consol.DocumentSupporter.GetChildCollection(westPortMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);

			var northWestPortContinationPageMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Contains, "North/WestPort"));
			docs = consol.DocumentSupporter.GetChildCollection(northWestPortContinationPageMenu, BusinessContext.Shipment, null);
			AssertEquals("Count", 1, docs.Length);

			var deliveryAgentPackMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Delivery Agent Pack (Air)"));
			consol.DocumentSupporter.GetDataStateBeforeRun(deliveryAgentPackMenu);
			docs = consol.DocumentSupporter.GetChildCollection(deliveryAgentPackMenu, BusinessContext.DeliveryAgent, null);
			AssertEquals("Count", 0, docs.Length);

			var deliveryAgent1 = Factory.New<DeliveryAgentOrgHeader>();
			var deliveryAgent2 = Factory.New<DeliveryAgentOrgHeader>();

			deliveryAgent1.OH_Code = "CCC1";
			deliveryAgent1.OH_FullName = "Delivery Agent1";
			deliveryAgent1.MainAddress.OA_Address1 = "Address 1";

			deliveryAgent2.OH_Code = "CCC2";
			deliveryAgent2.OH_FullName = "Delivery Agent2";
			deliveryAgent2.MainAddress.OA_Address1 = "Address 2";

			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;

			var accHeader1 = Factory.New<AccTransactionHeader>();
			accHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader1.AH_TransactionType = TransactionTypes.Invoice;
			accHeader1.AH_OH = deliveryAgent1.PK;
			accHeader1.AH_InvoiceDate = ZDateTime.Now;
			accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader1.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;

			shipment2.JS_OH_DeliveryAgent = deliveryAgent2.PK;
			shipment2.JS_UniqueConsignRef = "S2";
			shipment2.JS_HouseBillOfLadingType = "FIA";
			shipment2.JS_NoCopyBills = 1;
			shipment2.JS_NoOriginalBills = 1;

			var accHeader2 = Factory.New<AccTransactionHeader>();
			accHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = TransactionTypes.Invoice;
			accHeader2.AH_OH = deliveryAgent2.PK;
			accHeader2.AH_InvoiceDate = ZDateTime.Now;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader2.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;

			consol.DocumentSupporter.GetDataStateBeforeRun(deliveryAgentPackMenu);
			docs = consol.DocumentSupporter.GetChildCollection(deliveryAgentPackMenu, BusinessContext.DeliveryAgent, null);
			AssertEquals("Two Delivery Agents Pack", 2, docs.Length);

			var billOfLadingMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"));
			consol.DocumentSupporter.GetDataStateBeforeRun(billOfLadingMenu);

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 2, docs.Length);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 1, docs.Length);
			AssertEquals("DocumentSupporter.PK", shipment1.PK, docs[0].DocumentSupporter.PK);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 1, docs.Length);
			AssertEquals("DocumentSupporter.PK", shipment2.PK, docs[0].DocumentSupporter.PK);

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_PackingMode = Constants.ContainerModes.FCL;
			consol.Shipments.Add(shipment3);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.All;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 3, docs.Length);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.MastersOnly;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 2, docs.Length);

			consol.JK_PrintOptionForColoadsOnOtherDocs = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;
			docs = consol.DocumentSupporter.GetChildCollection(billOfLadingMenu, BusinessContext.Shipment, null);
			AssertEquals("Docs.Length", 2, docs.Length);
		}

		public void TestDepotContactOrganisation()
		{
			OrgHeader exportCTO = NewOrgHeader("EXPORTCTO", "EXPORTCTO", "address1");
			OrgHeader importCTO = NewOrgHeader("IMPORTCTO", "IMPORTCTO", "address2");
			OrgHeader packDepot = NewOrgHeader("PACKDEPOT", "PACKDEPOT", "address3");
			OrgHeader unpackDepot = NewOrgHeader("UNPACKDEPOT", "UNPACKDEPOT", "address4");

			AssertContactOrganisation(ContactType.Depot, DocumentDirection.DEP, null);
			AssertContactOrganisation(ContactType.Depot, DocumentDirection.ARV, null);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			Consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;
			Consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;

			AssertContactOrganisation(ContactType.Depot, DocumentDirection.DEP, exportCTO);
			AssertContactOrganisation(ContactType.ExportDepot, DocumentDirection.DEP, exportCTO);
			AssertContactOrganisation(ContactType.ExportSeaDepot, DocumentDirection.DEP, exportCTO);
			AssertContactOrganisation(ContactType.Depot, DocumentDirection.ARV, importCTO);
			AssertContactOrganisation(ContactType.ImportDepot, DocumentDirection.ARV, importCTO);
			AssertContactOrganisation(ContactType.ImportSeaDepot, DocumentDirection.ARV, importCTO);

			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertContactOrganisation(ContactType.ImportDepot, DocumentDirection.ARV, importCTO);

			Consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;
			Consol.JK_OA_UnpackDepotAddress = unpackDepot.MainAddress.PK;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertContactOrganisation(ContactType.Depot, DocumentDirection.DEP, packDepot);
			AssertContactOrganisation(ContactType.ExportDepot, DocumentDirection.DEP, packDepot);
			AssertContactOrganisation(ContactType.ExportSeaDepot, DocumentDirection.DEP, packDepot);

			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			Consol.JK_OA_PackDepotAddress = packDepot.MainAddress.PK;
			Consol.JK_OA_UnpackDepotAddress = unpackDepot.MainAddress.PK;
			AssertContactOrganisation(ContactType.Depot, DocumentDirection.DEP, packDepot);
			AssertContactOrganisation(ContactType.Depot, DocumentDirection.ARV, unpackDepot);
		}

		public void TestAPContactOrganisation()
		{
			OrgHeader sendingForwarder = NewOrgHeader("SNDFORD", "SNDFORD", "address1");
			AssertContactOrganisation(ContactType.Payables, DocumentDirection.ANY, null);
			Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			AssertContactOrganisation(ContactType.Payables, DocumentDirection.ANY, sendingForwarder);
		}

		void AssertContactOrganisation(IContactType contactType, DocumentDirection direction, IOrgHeader expected)
		{
			IDocumentDeliveryContact contact = Consol.DocumentSupporter.GetContactOrganisation("", contactType, direction);
			if (expected == null)
			{
				AssertNull("Contact should be null", contact);
			}
			else
			{
				AssertEquals("Contact should be " + expected.Code, expected.Code, contact.OrgHeader.Code);
			}
		}

		void Consol_OnGetDeliveryAgentsToPrint(object sender, DeliveryAgentsToPrintEventArgs e)
		{
			foreach (DeliveryAgentToSelectFromForPrinting currentDeliveryAgent in e.DeliveryAgentsToSelectFrom)
			{
				if (currentDeliveryAgent.OH_Calc_PrintDocumentForDeliveryAgent == ZBool.True)
				{
					var deliveryAgentToPrint = Factory.Load<DeliveryAgentOrgHeader>(currentDeliveryAgent.PK);
					e.DeliveryAgentsToPrint.Add(currentDeliveryAgent);
				}
			}
			e.CancelToPrint = false;
		}

		public void TestIsImport()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("IDocumentAutoDelivery IsImport", false, Consol.DocumentSupporter.IsImport);

			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("IDocumentAutoDelivery IsImport", true, Consol.DocumentSupporter.IsImport);
		}

		public void TestLocalPort()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";

			AssertEquals("IDocumentAutoDelivery LocalPort for CNR", "AUSYD", Consol.DocumentSupporter.LocalPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery LocalPort for CNE", "USLAX", Consol.DocumentSupporter.LocalPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery LocalPort for other contact type", "", Consol.DocumentSupporter.LocalPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestForeignPort()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";

			AssertEquals("IDocumentAutoDelivery ForeignPort for CNR", "USLAX", Consol.DocumentSupporter.ForeignPort(ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery ForeignPort for CNE", "AUSYD", Consol.DocumentSupporter.ForeignPort(ContactType.Consignee, DocumentDirection.ANY));
			AssertEquals("IDocumentAutoDelivery ForeignPort for other contact type", "", Consol.DocumentSupporter.ForeignPort(ContactType.Receivables, DocumentDirection.ANY));
		}

		public void TestGetContactOrganisation()
		{
			var factoryToSave = new BusinessObjectFactory();
			var sendingForwarder = factoryToSave.NewWithValidTestData<OrgHeader>();
			var shippingLine = factoryToSave.NewWithValidTestData<OrgHeader>();
			factoryToSave.Save();

			Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			Consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var contactOrg = Consol.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY);
			AssertEquals("IDocumentAutoDelivery ContactOrganisation for FWE", Consol.SendingForwarderPK, contactOrg.OrgHeader.PK);
			AssertEquals("IDocumentAutoDelivery RelatedParty for FWE", null, contactOrg.RelatedOrgHeader);

			contactOrg = Consol.DocumentSupporter.GetContactOrganisation("", ContactType.ExportAirFreightAgent, DocumentDirection.ANY);
			AssertEquals("IDocumentAutoDelivery ContactOrganisation for FWA", Consol.SendingForwarderPK, contactOrg.OrgHeader.PK);
			AssertEquals("IDocumentAutoDelivery RelatedParty for FWA", null, contactOrg.RelatedOrgHeader);

			contactOrg = Consol.DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY);
			AssertEquals("IDocumentAutoDelivery ContactOrganisation for FWS", Consol.SendingForwarderPK, contactOrg.OrgHeader.PK);
			AssertEquals("IDocumentAutoDelivery RelatedParty for FWS", null, contactOrg.RelatedOrgHeader);

			contactOrg = Consol.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY);
			AssertEquals("IDocumentAutoDelivery ContactOrganisation for SHP", Consol.ShippingLinePK, contactOrg.OrgHeader.PK);
			AssertEquals("IDocumentAutoDelivery RelatedParty for SHP", null, contactOrg.RelatedOrgHeader);

			contactOrg = Consol.DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("IDocumentAutoDelivery ContactOrg for other contact type", null, contactOrg);
		}

		public void TestMatchFilterValue()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestER = Factory.New<ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			manifestER.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			manifestER.AMA_ParentId = consol.PK;
			manifestER.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA.AMA_ParentId = consol.PK;
			manifestZA.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA2 = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA2.AMA_ParentId = consol.PK;
			manifestZA2.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA3 = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA3.AMA_ParentId = consol.PK;
			manifestZA3.AMA_ParentTableCode = consol.TablePrefix;

			manifestZA.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			manifestZA2.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(-1);
			manifestZA3.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(1);

			manifestER.AMA_TransportMode = "SEA";
			manifestZA.AMA_TransportMode = "AIR";
			manifestZA2.AMA_TransportMode = "SeA";
			manifestZA3.AMA_TransportMode = "SEA";
			manifestER.AMA_ManifestType = "ABC";
			manifestZA.AMA_ManifestType = "ABC";
			manifestZA2.AMA_ManifestType = "aBc";
			manifestZA3.AMA_ManifestType = "DEF";

			var documentSupporter = consol.DocumentSupporter;
			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMANMOD|ERABCSEA"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMANMOD|ERABC"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMAN|ERABC"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMAN|ER"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMOD|ERSEA"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMOD|ER"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTY|ER"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTY|VU"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMANMOD|ZAABCSEA"));
			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMANMOD|ZAABCAIR"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMANMOD|ZAABC"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMAN|ZAABC"));
			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMAN|ZADEF"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMAN|ZA"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMOD|ZASEA"));
			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTYMOD|ZAAIR"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTYMOD|ZA"));

			AssertEquals(true, documentSupporter.MatchFilterValue("ASYCTY|ZA"));
			AssertEquals(false, documentSupporter.MatchFilterValue("ASYCTY|VU"));
		}

		public void TestGetGlobalManifestMatch()
		{
			var consol = Factory.New<ForwardingConsol>();
			var manifestER = Factory.New<ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			manifestER.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			manifestER.AMA_ParentId = consol.PK;
			manifestER.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA.AMA_ParentId = consol.PK;
			manifestZA.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA2 = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA2.AMA_ParentId = consol.PK;
			manifestZA2.AMA_ParentTableCode = consol.TablePrefix;

			var manifestZA3 = Factory.New<ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA3.AMA_ParentId = consol.PK;
			manifestZA3.AMA_ParentTableCode = consol.TablePrefix;

			manifestZA.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			manifestZA2.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(-1);
			manifestZA3.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(1);

			manifestER.AMA_TransportMode = "SEA";
			manifestZA.AMA_TransportMode = "AIR";
			manifestZA2.AMA_TransportMode = "SeA";
			manifestZA3.AMA_TransportMode = "SEA";
			manifestER.AMA_ManifestType = "ABC";
			manifestZA.AMA_ManifestType = "ABC";
			manifestZA2.AMA_ManifestType = "aBc";
			manifestZA3.AMA_ManifestType = "DEF";

			var manifests = new ASYCUDA.IAsycudaManifestHeader[] { manifestER, manifestZA2, manifestZA, manifestZA3 };
			AssertEquals(manifestER, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMANMOD|ERABCSEA")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMANMOD|ERABC")));

			AssertEquals(manifestER, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMAN|ERABC")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMAN|ER")));

			AssertEquals(manifestER, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMOD|ERSEA")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMOD|ER")));

			AssertEquals(manifestER, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTY|ER")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTY|VU")));

			AssertEquals(manifestZA2, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMANMOD|ZAABCSEA")));
			AssertEquals(manifestZA, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMANMOD|ZAABCAIR")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMANMOD|ZAABC")));

			AssertEquals(manifestZA2, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMAN|ZAABC")));
			AssertEquals(manifestZA3, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMAN|ZADEF")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMAN|ZA")));

			AssertEquals(manifestZA2, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMOD|ZASEA")));
			AssertEquals(manifestZA, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMOD|ZAAIR")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTYMOD|ZA")));

			AssertEquals(manifestZA2, manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTY|ZA")));
			AssertNull(manifests.FirstOrDefault(ForwardingConsolDocumentSupporter.GetGlobalManifestMatch("ASYCTY|VU")));
		}

		public void TestCurrentConsolForDocumentsIsSetOnDocumentPrePrinted()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var documentEventsForTest = new DocumentEventsForTest(Factory);
			var supporter = new ForwardingConsolDocumentSupporter(consol);
			supporter.Initialise(documentEventsForTest);

			documentEventsForTest.OnDocumentPrePrinted();
			Assert("All consol shipments have CurrentConsolForDocuments", consol.Shipments.Cast<ForwardingShipment>().All(shipment => shipment.CurrentConsolForDocuments == consol));
		}

		public void TestCurrentConsolForDocumentsIsSetOnDocumentPrintRequested()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var documentEventsForTest = new DocumentEventsForTest(Factory);
			var supporter = new ForwardingConsolDocumentSupporter(consol);
			supporter.Initialise(documentEventsForTest);

			documentEventsForTest.OnDocumentPrintRequested();
			Assert("All consol shipments has CurrentConsolForDocuments",
				consol.Shipments.Cast<ForwardingShipment>().All(shipment => shipment.CurrentConsolForDocuments == consol));
		}

		public void TestAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms()
		{
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Constants.CountryCodes.Italy);
			AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(Constants.CountryCodes.Germany);
		}

		void AssertAdditionalBODataSourceDocumentSupportersGetsBOContextsFromCustoms(string countryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = Factory.New<ForwardingConsol>();
				var docSupporter = consol.DocumentSupporter;

				var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = consol.PK;
				nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
				var boDocDataProviders = docSupporter.GetBODocDataProviders(new DataContextValue("EuNcts"), null);
				AssertEquals("boDocDataProviders.Length with a nctsHeader", 1, boDocDataProviders.Length);
				AssertEquals("boDocDataProviders[0] with a nctsHeader", nctsHeader, boDocDataProviders[0].ParentBusinessObject);
			}
		}

		public void TestEuNctsIsSupportedAndWorking()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals("Is EuNcts DataContext supported", true, consol.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.EuNcts)));
			DocumentWrapper[] result = consol.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, commandBeingRun: null);
			AssertEquals("GetDocumentWrappers without NCTS header", null, result);

			var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader[CusInBondHeaderSchema.BH_ParentID] = consol.PK;
			var nctsDepartureMovement = (BusinessObject)Factory.New<EU.NCTS.IDepartureMovementHeader>();
			nctsDepartureMovement[CusInBondMoveHeaderSchema.BM_BH] = nctsHeader.PK;
			result = consol.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.EuNcts, commandBeingRun: null);
			AssertEquals("GetDocumentWrappers with a departure NCTS header", 1, result.Length);
		}

		public void TestSecurityMenuTemplateFilterValueForEuNcts()
		{
			var consol = Factory.New<ForwardingConsol>();
			var documentSupporter = consol.DocumentSupporter;
			var menuTemplateFilterValue = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.Security, dataProvider: null);
			AssertNull("When no linked NctsHeader, GetMenuTemplateFilterValue", menuTemplateFilterValue);

			BusinessObject nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = consol.PK;
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;
			var dataProvider = documentSupporter.GetBODocDataProviders(new DataContextValue("EuNcts"), null)[0];
			menuTemplateFilterValue = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.Security, dataProvider);
			AssertEquals("When linked NctsHeader without SafetyAndSecurityFlag, GetMenuTemplateFilterValue", "N", menuTemplateFilterValue);
		}

		public void TestAdditionalBODataSourceDocumentSupporters()
		{
			var consol = Factory.New<ForwardingConsol>();
			var documentSupporter = new ForwardingConsolDocumentSupporterForTest(consol);

			AssertEquals("AdditionalBODataSourceDocumentSupporters count", 1, documentSupporter.AdditionalBODataSourceDocumentSupportersExposed.Count);

			var nctsHeader = (BusinessObject)Factory.New<EU.NCTS.ICusInBondHeader>();
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ParentID] = consol.PK;
			nctsHeader[CusInBondHeaderSchema.Constants.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.NCTS4;

			AssertEquals("AdditionalBODataSourceDocumentSupporters count", 1, documentSupporter.AdditionalBODataSourceDocumentSupportersExposed.Count);
		}

		#region Print Final Master

		public void TestPrintFinalAWBMaster_WhentheConsigneeIsOnCreditHold()
		{
			var expectedMessage = @"Delivery of this document is restricted because:
       The Sending Agent, Receiving Agent or Consignee, Consignor, Local Client or any Debtors in any associated Shipment
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.";
			AssertIfConsigneeIsOnCreditHoldOrNot(true, false, expectedMessage);
		}

		public void TestPrintFinalAWBMaster_WhentheConsigneeIsNotOnCreditHold()
		{
			AssertIfConsigneeIsOnCreditHoldOrNot(false, true, "");
		}

		void AssertIfConsigneeIsOnCreditHoldOrNot(bool isOnCreditHold, bool isValid, string errorMsg)
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_IsDebtor = isOnCreditHold;
			orgCompanyData.OB_AROnCreditHold = isOnCreditHold;
			orgCompanyData.OB_OH = orgHeader.PK;
			Factory.Save();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = orgHeader.PK;

			var supporter = new ForwardingConsolDocumentSupporter(consol);

			var menuQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Print Final Master");
			menuQuery.AddToFilter(new ZQuery(StmMenuItemSchema.SU_MenuPath, "AWB"));
			var menuItem = Factory.LoadTop1<StmMenuItem>(menuQuery);
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			var dataState = supporter.GetDataStateBeforeRun(menuItem);
			CombineAssertions("If the Consignee has been put on Credit Hold, Should have this error.", delegate
			{
				AssertEquals(isValid, dataState.IsValid);
				AssertEquals(errorMsg, dataState.ErrorMessage);
			});
		}

		public void TestPrintFinalAWBMaster_SecurityRights()
		{
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Staff", "Print Final Master", runTestOnGroupPermissions: false);
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Group", "Print Final Master", runTestOnGroupPermissions: true);
		}

		public void TestAWBBarcodeLabel_SecurityRights()
		{
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Staff", "AWB Barcode Label", runTestOnGroupPermissions: false);
			RunPrintFinalAWBMaster_SecurityRightsTestOn("Group", "AWB Barcode Label", runTestOnGroupPermissions: true);
		}

		public void RunPrintFinalAWBMaster_SecurityRightsTestOn(string securityTargetName, string documentMenu, bool runTestOnGroupPermissions)
		{
			#region Setup

			var currentUser = Factory.NewWithValidTestData<GlbStaff>();
			currentUser.GS_IsOperational = true;

			BusinessObject securityTarget = currentUser;
			if (runTestOnGroupPermissions)
			{
				var group = currentUser.Groups.AddNew();
				group.GG_Code = "AAA";

				securityTarget = group;
			}

			var menuQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, documentMenu);
			menuQuery.AddToFilter(new ZQuery(StmMenuItemSchema.SU_MenuPath, "AWB"));

			var menuItem = Factory.LoadTop1<StmMenuItem>(menuQuery);
			var jobConsolModuleId = ModuleIDs.JobConsol;

			Factory.Save();

			#endregion

			using (var module = ObjectFactory.Get<IModuleFactory>().Create(jobConsolModuleId))
			{
				var supporter = new ForwardingConsolDocumentSupporter(Factory.New<ForwardingConsol>());
				var parentCheckpoint = Env.Security.FindOrCreateDocumentsCheckpoint(jobConsolModuleId, module.SecurityCheckpoint);
				var explicitCheckpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);

				void GetDataStateAndAssertError(string message, bool shouldHaveError)
				{
					using (Env.Instance.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
					{
						var dataState = supporter.GetDataStateBeforeRun(menuItem);
						var errorMessage = shouldHaveError ? explicitCheckpoint.ErrorMessageForNotAllowed : string.Empty;

						CombineAssertions(message, delegate
						{
							AssertEquals(!shouldHaveError, dataState.IsValid);
							AssertEquals(errorMessage, dataState.ErrorMessage);
						});
					}
				}

				GetDataStateAndAssertError("Pre-Condition - Default has no permissions", shouldHaveError: true);

				AddSecurityPermission(securityTarget, parentCheckpoint, true);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has implicit permission so should be able to use it (Implicit Yes)", shouldHaveError: false);

				UpdateSecurityPermission(securityTarget, parentCheckpoint, false);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has implicit permission so should be able to use it (Implicit No)", shouldHaveError: true);

				UpdateSecurityPermission(securityTarget, parentCheckpoint, true);
				AddSecurityPermission(securityTarget, explicitCheckpoint, false);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} has implicit permission but explicit takes priority so access is denied (Explicit No)", shouldHaveError: true);

				UpdateSecurityPermission(securityTarget, explicitCheckpoint, true);
				Factory.Save();

				GetDataStateAndAssertError($"{securityTargetName} now has explicit permission so should be able to use it (Explicit Yes)", shouldHaveError: false);
			}
		}

		void AddSecurityPermission(BusinessObject target, SecurityCheckpoint checkpoint, bool allowed)
		{
			var permission = Factory.New<GlbSecurity>();
			permission.GU_SecurityItemIsAllowed = allowed;
			permission.GU_SecurityRight = checkpoint.Code;
			permission.GU_ItemGUID = checkpoint.ItemGuid;

			if (target is GlbStaff)
			{
				permission.GU_GS = target.PK;
			}
			else if (target is GlbGroup)
			{
				permission.GU_GG = target.PK;
			}
		}

		void UpdateSecurityPermission(BusinessObject target, SecurityCheckpoint checkpoint, bool value)
		{
			var permissionQuery = new ZQuery(GlbSecuritySchema.GU_SecurityRight, checkpoint.Code);

			if (target is GlbStaff)
			{
				permissionQuery.AddToFilter(GlbSecuritySchema.GU_GS, target.PK);
			}
			else if (target is GlbGroup)
			{
				permissionQuery.AddToFilter(GlbSecuritySchema.GU_GG, target.PK);
			}

			var permission = Factory.LoadTop1<GlbSecurity>(permissionQuery);
			permission.GU_SecurityItemIsAllowed = value;
		}

		#endregion

		#region Implementation

		#region IDocumentEvents

		class DocumentEventsForTest : IDocumentEvents
		{
			public DocumentEventsForTest(BusinessObjectFactory factory)
			{
				menu = factory.New<StmMenuItem>();
				menu.SU_MenuName = "BILL OF LADING and something";
			}

			public StmMenuItem menu;

			#region IDocumentEvents Members

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "IDocumentEvents.DocumentPrePreviewed member from interface needs to be implemented; error CS0535 raised otherwise")]
			void OnDocumentPrePreviewed()
			{
				DocumentPrePreviewed(null, null);
			}

			public void OnDocumentPrePrinted()
			{
				DocumentPrePrinted(this, DocumentPrePrintedArgs);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "IDocumentEvents.DocumentPrinted member from interface needs to be implemented; error CS0535 raised otherwise")]
			void OnDocumentPrinted()
			{
				DocumentPrinted(null, null);
			}

			public void OnDocumentPrintRequested()
			{
				DocumentPrintRequested(this, DocumentCancelEventArgs);
			}

			public DocumentCancelEventArgs DocumentCancelEventArgs
			{
				get { return documentCancelEventArgs ?? (documentCancelEventArgs = new DocumentCancelEventArgs(menu)); }
			}
			DocumentCancelEventArgs documentCancelEventArgs;

			public DocumentPrintedEventArgs DocumentPrePrintedArgs
			{
				get { return documentPrePrintedArgs ?? (documentPrePrintedArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menu)); }
			}
			DocumentPrintedEventArgs documentPrePrintedArgs;

			public event DocumentPrintedEventHandler DocumentPrePreviewed;

			public event DocumentPrintedEventHandler DocumentPrePrinted;

			public event DocumentCancelEventHandler DocumentPrintRequested;

			public event DocumentPrintedEventHandler DocumentPrinted;

			#endregion
		}

		#endregion

		ForwardingConsolForTest Consol => consol ?? (consol = Factory.New<ForwardingConsolForTest>());
		ForwardingConsolForTest consol;

		OrgHeader NewOrgHeader(ZString code, ZString name, ZString address)
		{
			OrgHeader newOrgHeader = Factory.New<OrgHeader>();
			newOrgHeader.OH_Code = code;
			newOrgHeader.OH_FullName = name;
			newOrgHeader.MainAddress.OA_Address1 = address;
			return newOrgHeader;
		}

		class ForwardingConsolDocumentSupporterForTest : ForwardingConsolDocumentSupporter
		{
			public ForwardingConsolDocumentSupporterForTest(ForwardingConsol forwardingConsol)
				: base(forwardingConsol)
			{
			}

			public new bool HasUncontainerisedHazPackLines(ForwardingShipment shipment)
			{
				return base.HasUncontainerisedHazPackLines(shipment);
			}

			public new IForwardingConsolDocumentSupporterQueryProvider QueryProvider
			{
				get { return base.QueryProvider; }
			}

			public new DocumentWrapper[] GetWrappersForARInvoice(ZGuid orgHeaderForInvoices, Constants.DataContext dataContext, bool useDocBuilderInvoice)
			{
				return base.GetWrappersForARInvoice(orgHeaderForInvoices, dataContext, useDocBuilderInvoice);
			}

			public List<DocumentSupporter> AdditionalBODataSourceDocumentSupportersExposed => AdditionalBODataSourceDocumentSupporters;

			public void DocumentEventSource_DocumentPrintRequested_Exposed(object sender, DocumentCancelEventArgs e)
			{
				DocumentEventSource_DocumentPrintRequested(sender, e);
			}
		}

		#endregion
	}
}
