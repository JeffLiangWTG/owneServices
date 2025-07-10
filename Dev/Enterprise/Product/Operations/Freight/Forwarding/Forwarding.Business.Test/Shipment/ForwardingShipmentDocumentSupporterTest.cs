using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentDocumentSupporter))]
	sealed class ForwardingShipmentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetChildCollectionWithNoExceptions_WhenNoGBCusHAWB_Issue01012458()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBillOfLadingType = "IFJ";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AELON";

			var supporter = new ForwardingShipmentDocumentSupporterForTest(shipment);
			AssertNotNull("Should not be null", supporter.GetChildCollection(null, BusinessContext.CusHAWB, null));
		}

		public void TestZADA306DocDataProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var docDataProviders = shipment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.ZADA306, Factory.New<IStmMenuItem>());
			AssertEquals(1, docDataProviders.Length);
		}

		public void TestGetSupportedDataContexts()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var documentSupporter = shipment.DocumentSupporter;
			var list = documentSupporter.ListOfSupportedDataContexts;

			CombineAssertions(() =>
			{
				Assert("Shipment", list.ContainsCode(Constants.DataContext.Shipment));
				Assert("AWB", list.ContainsCode(Constants.DataContext.AWB));
				Assert("Declaration", list.ContainsCode(Constants.DataContext.Declaration));
				Assert("NZCustoms", list.ContainsCode(Constants.DataContext.NZCustoms));
				Assert("DeclarationWithCusEntryHeaders", list.ContainsCode(Constants.DataContext.DeclarationWithCusEntryHeaders));
				Assert("Container", list.ContainsCode(Constants.DataContext.Container));
				Assert("Notes", list.ContainsCode(Constants.DataContext.Notes));
				Assert("CartageAdvice", list.ContainsCode(Constants.DataContext.CartageAdvice));
				Assert("CommercialInvoice", list.ContainsCode(Constants.DataContext.CommercialInvoice));
				Assert("ChargeSheet", list.ContainsCode(Constants.DataContext.ChargeSheet));
				Assert("CusEntryHeader", list.ContainsCode(Constants.DataContext.CusEntryHeader));
				Assert("ComInvoiceHeader", list.ContainsCode(Constants.DataContext.ComInvoiceHeader));
				Assert("PreAlert", list.ContainsCode(Constants.DataContext.PreAlert));
				Assert("ForwardingPreAdvice", list.ContainsCode(Constants.DataContext.ForwardingPreAdvice));
				Assert("ARInvoice", list.ContainsCode(Constants.DataContext.ARInvoice));
				Assert("ShipperDepartureNotice", list.ContainsCode(Constants.DataContext.ShipperDepartureNotice));
				Assert("LandedCostEntryHeaders", list.ContainsCode(Constants.DataContext.LandedCostEntryHeaders));
				Assert("LandedCostHeader", list.ContainsCode(Constants.DataContext.LandedCostHeader));
				Assert("ShipmentDeclaration", list.ContainsCode(Constants.DataContext.ShipmentDeclaration));
				Assert("FreightLabels", list.ContainsCode(Constants.DataContext.FreightLabels));
				Assert("ShippingOrder", list.ContainsCode(Constants.DataContext.ShippingOrder));
				Assert("ATD", list.ContainsCode(Constants.DataContext.ATD));
				Assert("EFTPaymentAdvice", list.ContainsCode(Constants.DataContext.EFTPaymentAdvice));
				Assert("LetterOfIndemnity", list.ContainsCode(Constants.DataContext.LetterOfIndemnity));
				Assert("CombinedCartageAdvice", list.ContainsCode(Constants.DataContext.CombinedCartageAdvice));
				Assert("ERA", list.ContainsCode(Constants.DataContext.ERA));
				Assert("CFSAndForwardingShipment", list.ContainsCode(Constants.DataContext.CFSAndForwardingShipment));
				Assert("RequestForMissingDocuments", list.ContainsCode(Constants.DataContext.RequestForMissingDocuments));
				Assert("Worksheet", list.ContainsCode(Constants.DataContext.Worksheet));
				Assert("ForwardingShipment", list.ContainsCode(Constants.DataContext.ForwardingShipment));
				Assert("IMO", list.ContainsCode(Constants.DataContext.IMO));
				Assert("RequestForService", list.ContainsCode(Constants.DataContext.RequestForService));
				Assert("ForwardingShipmentAndConsol", list.ContainsCode(Constants.DataContext.ForwardingShipmentAndConsol));
				Assert("Service", list.ContainsCode(Constants.DataContext.Service));
				Assert("TimeSlotRequest", list.ContainsCode(Constants.DataContext.TimeSlotRequest));
				Assert("GenericFreightJob", list.ContainsCode(Constants.DataContext.GenericFreightJob));
				Assert("GenericFreightJobRouting", list.ContainsCode(Constants.DataContext.GenericFreightJobRouting));
				Assert("GenericPickupDeliveryConfirm", list.ContainsCode(Constants.DataContext.GenericPickupDeliveryConfirm));
				Assert("GenericFreightJobByContainerIfFCL", list.ContainsCode(Constants.DataContext.GenericFreightJobByContainerIfFCL));
				Assert("GenericCommercialInvoice", list.ContainsCode(Constants.DataContext.GenericCommercialInvoice));
				Assert("SGPrintPermit", list.ContainsCode(Constants.DataContext.SGPrintPermit));
				Assert("SGRefundInfo", list.ContainsCode(Constants.DataContext.SGRefundInfo));
				Assert("EUR1", list.ContainsCode(Constants.DataContext.EUR1));
				Assert("SADH", list.ContainsCode(Constants.DataContext.SADH));
				Assert("ESSADH", list.ContainsCode(Constants.DataContext.ESSADH));
				Assert("LiquidationDetails", list.ContainsCode(Constants.DataContext.LiquidationDetails));
				Assert("GbTaxEstimator", list.ContainsCode(Constants.DataContext.GbTaxEstimator));
				Assert("GenericChargeSheet", list.ContainsCode(Constants.DataContext.GenericChargeSheet));
				Assert("GenericFreightJobFrmShipByContIfFCL", list.ContainsCode(Constants.DataContext.GenericFreightJobFrmShipByContIfFCL));
				Assert("InBond7512Departure", list.ContainsCode(Constants.DataContext.InBond7512Departure));
				Assert("GenericFreightJobByComInv", list.ContainsCode(Constants.DataContext.GenericFreightJobByComInv));
				Assert("GenericFreightJobInvoice", list.ContainsCode(Constants.DataContext.GenericFreightJobInvoice));
				Assert("GenericFreightJobByPackages", list.ContainsCode(Constants.DataContext.GenericFreightJobByPackages));
				Assert("GenericFreightJobByPackages1Doc", list.ContainsCode(Constants.DataContext.GenericFreightJobByPackages1Doc));
				Assert("GenericFreightJobBySelectedPackages", list.ContainsCode(Constants.DataContext.GenericFreightJobBySelectedPackages));
				Assert("GenericFreightJobBySelectedPkgs1Doc", list.ContainsCode(Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc));
				Assert("GenericImportCargoLabel", list.ContainsCode(Constants.DataContext.GenericImportCargoLabel));
				Assert("GbCcsuk", list.ContainsCode(Constants.DataContext.GbCcsuk));
				Assert("EuNcts", list.ContainsCode(Constants.DataContext.EuNcts));
				Assert("FRSADH", list.ContainsCode(Constants.DataContext.FRSADH));
				Assert("IEImportAccompanyingDocument", list.ContainsCode(Constants.DataContext.IEImportAccompanyingDocument));
				Assert("IEIADClearanceSlip", list.ContainsCode(Constants.DataContext.IEIADClearanceSlip));
				Assert("ZADA306", list.ContainsCode(Constants.DataContext.ZADA306));
			});
		}

		#region GetBODocDataProvidersNotFoundMessage

		public void TestGetBODocDataProvidersNotFoundMessage_NoAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var contextArray = new[]
			{
				Constants.DataContext.AWB,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This document requires AWB data.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var awbHeader = consol.AWBHeader;
			AssertNotNull(awbHeader);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader = shipment.AWBHeader;
			AssertNotNull(awbHeader);

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Core.Constants.DataContext.Declaration,
				Core.Constants.DataContext.NZCustoms,
				Core.Constants.DataContext.SGPrintPermit,
				Core.Constants.DataContext.SGRefundInfo,
				Core.Constants.DataContext.DeclarationWithCusEntryHeaders,
				Core.Constants.DataContext.CommercialInvoice,
				Core.Constants.DataContext.CusEntryHeader,
				Core.Constants.DataContext.ATD,
				Core.Constants.DataContext.EFTPaymentAdvice,
				Core.Constants.DataContext.ComInvoiceHeader,
				Core.Constants.DataContext.LandedCostEntryHeaders,
				Core.Constants.DataContext.LandedCostHeader,
				Core.Constants.DataContext.SADH,
				Core.Constants.DataContext.ESSADH,
				Core.Constants.DataContext.FRSADH,
				Core.Constants.DataContext.LiquidationDetails,
				Core.Constants.DataContext.GbTaxEstimator,
				Core.Constants.DataContext.GenericFreightJobByComInv,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This document is only available when this shipment has a Brokerage job.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoContainers()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Constants.DataContext.Container,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This document requires container data. Ensure the related Consol has containers.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;

			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoTransport()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobRouting
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any Transport information entered on the Routing tab.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			shipment.TransportsIncludingRelated.AddNew();

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoServices()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobServices
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any Services information entered within the Additional Details > Services grid.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			shipment.Services.AddNew();

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoPackLines()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 0;

			var contextArray = new[]
			{
				Constants.DataContext.GenericFreightJobByPackages,
				Constants.DataContext.GenericFreightJobByPackages1Doc,
				Constants.DataContext.GenericFreightJobBySelectedPackages,
				Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc,
				Constants.DataContext.GenericImportCargoLabel,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment doest not have any packlines.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;
			shipment.JS_OuterPacks = 1;

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoDebtor()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Constants.DataContext.ChargeSheet,
				Constants.DataContext.GenericChargeSheet,
			};

			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "DEP";

			var expectedMessage = "This document requires a debtor to be entered within the Billing tab.";

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoHAWBOnGBCustoms()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var contextArray = new[]
			{
				Constants.DataContext.GbCcsuk,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have any HAWB data about GB Customs.";

			var hasGBCusHAWBS = shipment.GBCusHAWBs != null && shipment.GBCusHAWBs.Any();
			AssertEquals(false, hasGBCusHAWBS);
			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_ARInvoice_Consignee()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			var contextArray = new[]
			{
				Constants.DataContext.ARInvoice,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have a Consignee.";

			menu.SU_ContactType = ContactType.Consignee.Code;
			shipment.ConsigneePK = ZGuid.Empty;

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			transactionHeader.AH_OH = consignee.PK;
			transactionHeader.AH_InvoiceDate = ZDateTime.Now;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_ARInvoice_Consignor()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			var contextArray = new[]
			{
				Constants.DataContext.ARInvoice,
			};

			var menu = Factory.New<StmMenuItem>();
			var expectedMessage = "This shipment does not have a Consignor.";

			menu.SU_ContactType = ContactType.Consignor.Code;
			shipment.ConsignorPK = ZGuid.Empty;

			AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			var transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			transactionHeader.AH_OH = consignor.PK;
			transactionHeader.AH_InvoiceDate = ZDateTime.Now;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			AssertNotFoundMessage(shipment, menu, contextArray, false, ZString.Empty);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_ARInvoice_LocalCharges()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			shipment.CreateJobHeaderWithMutex();

			var contextArray = new[]
			{
				Constants.DataContext.ARInvoice,
			};

			using (var job = shipment.Job)
			{
				var contactTypes = new[]
				{
					ContactType.All.Code,
					ContactType.NoContactType.Code,
					ContactType.Receivables.Code
				};

				var expectedMessage = "This shipment does not have a Local Client on the Billing tab page.";
				var menu = Factory.New<StmMenuItem>();

				var localCharges = Factory.NewWithValidTestData<OrgHeader>();

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				transactionHeader.AH_OH = localCharges.PK;
				transactionHeader.AH_InvoiceDate = ZDateTime.Now;
				transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
				transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
				transactionHeader.AH_JH = job.PK;
				transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

				foreach (var type in contactTypes)
				{
					menu.SU_ContactType = type;
					job.LocalChargesPK = ZGuid.Empty;

					AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

					job.LocalChargesPK = localCharges.PK;

					AssertNotFoundMessage(shipment, menu, contextArray, false, string.Empty);
				}
			}
		}

		public void TestGetBODocDataProvidersNotFoundMessage_ARInvoice_AgentCollect()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			shipment.CreateJobHeaderWithMutex();

			var contextArray = new[]
			{
				Constants.DataContext.ARInvoice,
			};

			using (var job = shipment.Job)
			{
				var expectedMessage = "This shipment does not have a Overseas Agent on the Billing tab page.";
				var menu = Factory.New<StmMenuItem>();

				var agentCollect = Factory.NewWithValidTestData<OrgHeader>();

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				transactionHeader.AH_OH = agentCollect.PK;
				transactionHeader.AH_InvoiceDate = ZDateTime.Now;
				transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
				transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
				transactionHeader.AH_JH = shipment.Job.PK;
				transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

				var contactTypes = new[]
				{
					ContactType.ImportAirFreightAgent.Code,
					ContactType.ImportSeaFreightAgent.Code,
					ContactType.ExportAirFreightAgent.Code,
					ContactType.ExportSeaFreightAgent.Code
				};

				foreach (var type in contactTypes)
				{
					menu.SU_ContactType = type;
					job.AgentCollectPK = ZGuid.Empty;

					AssertNotFoundMessage(shipment, menu, contextArray, true, expectedMessage);

					job.AgentCollectPK = agentCollect.PK;

					AssertNotFoundMessage(shipment, menu, contextArray, false, string.Empty);
				}
			}
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			if (menu.SU_MenuName.EqualsIgnoringCase("DocBuilder Invoice"))
			{
				return true;
			}

			var shipment = Factory.New<ForwardingShipment>();
			var supporter = shipment.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { return new[] { Factory.New<ForwardingShipment>() }; }
		}

		#endregion

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuPath.Contains("Customs", StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuPath.Contains("Cartage Advice", StringComparison.InvariantCultureIgnoreCase)
				|| documentCommand.SU_MenuName.EndsWith("Label", StringComparison.InvariantCultureIgnoreCase)
				|| base.ExcludeDocumentCommandTest(documentCommand);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBillOfLadingType = "IFJ";

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			var service = shipment.Services.AddNew();
			service.FillWithValidTestData();

			var gbMawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>();
			gbMawb.CM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.GbCcsuk;

			var gbHawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>();
			gbHawb.CS_JS = shipment.PK;
			gbHawb.CS_CM = ((BusinessObject)gbMawb).PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 5;
			packLine.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			var dg = packLine.UNDGs.AddNew();
			dg.FillWithValidTestData();

			var pickupConfirm = shipment.PickupConfirms.AddNew();
			pickupConfirm.FillWithValidTestData();

			var deliveryConfirm = shipment.DeliveryConfirms.AddNew();
			deliveryConfirm.FillWithValidTestData();

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";

			consol.Containers.AddNew();

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;

			var invoiceGroupHeader = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader>();
			invoiceGroupHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;

			var invoices = (IBusinessObjectCollection)ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>().GetProperty("Invoices").GetGetMethod().Invoke(declaration, null);

			var invoiceHeader = invoices.AddNew();
			invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration.PK;
			invoiceHeader[JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK] = invoiceGroupHeader.PK;

			return shipment;
		}
	}
}
