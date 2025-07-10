using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseJobDeclarationDocumentSupportTest : DocumentSupporterTest
	{
		public virtual void TestGetBODocDataProvidersNotFoundMessage()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals("Invoice Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CommercialInvoice), null));
			AssertEquals("Invoice Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice), null));
			AssertEquals("Invoice Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.ComInvoiceHeader), null));
			AssertEquals("Landed Costing Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.LandedCostEntryHeaders), null));
			AssertEquals("Landed Costing Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.LandedCostHeader), null));
			AssertEquals("Entry Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.CusEntryHeaderENS), null));
			AssertEquals("Invoice Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJob), null));
			AssertEquals("There is no container data associated with this declaration.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJobByContainerIfFCL), null));
			AssertEquals("Invoice Header cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.GenericFreightJobByComInv), null));
		}

		public virtual void TestShowReasonForNotPrinting()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.PreAlert, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.CartageAdvice, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.RequestForService, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.Declaration, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.Notes, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.ChargeSheet, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.ShipperDepartureNotice, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.ShipmentDeclaration, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.CombinedCartageAdvice, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.RequestForMissingDocuments, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.Worksheet, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.Service, null));
			AssertEquals(false, declaration.DocumentSupporter.ShowReasonForNotPrinting(Enterprise.Core.Constants.DataContext.TimeSlotRequest, null));
		}

		public void TestTransportMode()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocumentSupporter docSupporter = declaration.DocumentSupporter;

			declaration.JE_TransportMode = "";
			declaration.JE_ContainerMode = "";
			AssertEquals("docSupporter.TransportMode", "", docSupporter.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("docSupporter.TransportMode", Core.Constants.TransportModes.Air, docSupporter.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("docSupporter.TransportMode", Core.Constants.TransportModes.Road, docSupporter.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("docSupporter.TransportMode", Core.Constants.TransportModes.Rail, docSupporter.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Empty;
			AssertEquals("docSupporter.TransportMode", Core.Constants.TransportModes.Sea, docSupporter.TransportMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL; // JE_ContainerMode field should be ignored for all Transport Modes but Sea.
			AssertEquals("docSupporter.TransportMode", Core.Constants.TransportModes.Air, docSupporter.TransportMode);
		}

		public void TestContainerMode()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocumentSupporter docSupporter = declaration.DocumentSupporter;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("docSupporter.ContainerModes", string.Empty, docSupporter.ContainerMode);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("docSupporter.ContainerModes", Core.Constants.ContainerModes.FCL, docSupporter.ContainerMode);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("docSupporter.ContainerModes", Core.Constants.ContainerModes.LCL, docSupporter.ContainerMode);
		}

		public void TestGetDocBusinessObject_ForGenericCommercialInvoice()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocumentWrapper[] wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotEquals("Wrappers with Declaration with no Invoices", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with no Invoices", 0, wrappers.Length);

			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();

			wrappers = invoiceHeader.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericCommercialInvoice, null);
			AssertNotEquals("Wrappers with Declaration with a Commercial Invoice", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with a Commercial Invoice", 1, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Declaration with a Commercial Invoice", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Declaration with a Commercial Invoice", invoiceHeader.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetDocBusinessObject_ForGenericFreightJobByComInv()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			DocumentWrapper[] wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByComInv, null);
			AssertNotEquals("Wrappers with Declaration with no Invoices", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with no Invoices", 0, wrappers.Length);

			BaseJobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceHeader invoiceHeader1 = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoiceHeader2 = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();

			wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByComInv, null);
			AssertNotEquals("Wrappers with Declaration with a Commercial Invoice", null, wrappers);
			AssertEquals("Wrappers.Length with Declaration with a Commercial Invoice", 2, wrappers.Length);
			AssertNotEquals("Wrappers[0] with Declaration with a Commercial Invoice", null, wrappers[0]);
			AssertEquals("((BusinessObject)wrappers[0].WrappedObject).PK with Declaration with a Commercial Invoice", declaration.PK, ((BusinessObject)wrappers[0].WrappedObject).PK);
		}

		public void TestGetBODocDataProviderForLandedCostHeader()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			IBODocDataProvider[] docProviders = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".LandedCostHeader"), null);
			AssertEquals("docProviders for LandedCosting", 0, docProviders.Length);

			BusinessObject landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			docProviders = declaration.DocumentSupporter.GetBODocDataProviders(new DataContextValue(".LandedCostHeader"), null);
			AssertEquals("docProviders for LandedCosting", 1, docProviders.Length);
		}

		[ExpectNoExceptions()]
		public void TestWhenCalledFromPrintARInvoiceInAccounting()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.DocumentSupporter.GetBODocDataProviders(new DataContextValue(nameof(DataContext.GenericFreightJobInvoice)), null);
		}

		public void TestGetDocBusinessObjectForInvoices()
		{
			Db.Connection.BeginTransaction();// system barfs if this is not in a transaction.

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_DeclarationReference = "B11092008";

			var job = new JobHeader.Loader(dec).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoiceConsignee = CreateInvoice("00001001", job.PK);
			var invoiceConsignor = CreateInvoice("00001002", job.PK);
			var invoiceLocalClient = CreateInvoice("00001003", job.PK);
			var invoiceAgent = CreateInvoice("00001004", job.PK);

			Factory.Save();

			dec.JE_OH_Importer = invoiceConsignee.Header.PK;
			dec.JE_OH_Supplier = invoiceConsignor.Header.PK;
			job.JH_OA_LocalChargesAddr = invoiceLocalClient.Header.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = invoiceAgent.Header.MainAddress.PK;

			AssertCorrectJobInvoiceObjects(dec, ContactType.NoContactType, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(dec, ContactType.Consignee, invoiceConsignee);
			AssertCorrectJobInvoiceObjects(dec, ContactType.Consignor, invoiceConsignor);
			AssertCorrectJobInvoiceObjects(dec, ContactType.ExportBroker, invoiceAgent);
			AssertCorrectJobInvoiceObjects(dec, ContactType.ImportBroker, invoiceAgent);
			AssertCorrectJobInvoiceObjects(dec, ContactType.All, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(dec, ContactType.Receivables, invoiceLocalClient);
			AssertCorrectJobInvoiceObjects(dec, null, invoiceLocalClient);

			job.JH_OA_LocalChargesAddr = ZGuid.Empty;
			AssertCorrectJobInvoiceObjects(dec, ContactType.Receivables, null);

			Db.Connection.RollbackTransaction();
		}

		void AssertCorrectJobInvoiceObjects(BaseJobDeclaration dec, ContactType contactType, AccTransactionHeader expectedInvoice)
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = contactType != null ? contactType.Code : "";

			DocumentWrapper[] wrappers = dec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menuItem);

			if (expectedInvoice == null)
			{
				AssertNull(wrappers);
			}
			else
			{
				AssertEquals(1, wrappers.Length);
				AssertEquals(expectedInvoice.PK, ((IBODocDataProvider)wrappers[0]).BusinessObjectToLogAgainst.PK);
			}

			wrappers = dec.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, menuItem);

			if (expectedInvoice == null)
			{
				AssertNull(wrappers);
			}
			else
			{
				AssertEquals(1, wrappers.Length);
				AssertEquals(expectedInvoice.PK, ((IBODocDataProvider)wrappers[0]).BusinessObjectToLogAgainst.PK);
			}
		}

		AccTransactionHeader CreateInvoice(ZString invoiceNumber, ZGuid jobPK)
		{
			var result = Factory.NewWithValidTestData<AccTransactionHeader>();
			result.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			result.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			result.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.AH_TransactionNum = invoiceNumber;
			result.AH_RX_NKTransactionCurrency = Declaration.LocalCurrencyCode;
			result.AH_GB = GlbBranch.CurrentBranch.PK;
			result.AH_ConsolidatedInvoiceRef = "B11092008";
			result.AH_TransactionReference = invoiceNumber;
			result.AH_JH = jobPK;

			return result;
		}

		public virtual void TestSupportedDataContexts()
		{
			AssertEquals("DataContext.Declaration is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Declaration)));
			AssertEquals("DataContext.Notes is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Notes)));
			AssertEquals("DataContext.CommercialInvoice is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CommercialInvoice)));
			AssertEquals("DataContext.ChargeSheet is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ChargeSheet)));
			AssertEquals("DataContext.ComInvoiceHeader is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ComInvoiceHeader)));
			AssertEquals("DataContext.PreAlert is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.PreAlert)));
			AssertEquals("DataContext.CusEntryHeader is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusEntryHeader)));
			AssertEquals("DataContext.CusEntryHeaderENS is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CusEntryHeaderENS)));
			AssertEquals("DataContext.DeclarationWithCusEntryHeaders is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.DeclarationWithCusEntryHeaders)));
			AssertEquals("DataContext.ShipperDepartureNotice is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ShipperDepartureNotice)));
			AssertEquals("DataContext.LandedCostHeader is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.LandedCostHeader)));
			AssertEquals("DataContext.LandedCostEntryHeaders is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.LandedCostEntryHeaders)));
			AssertEquals("DataContext.ShipmentDeclaration is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ShipmentDeclaration)));
			AssertEquals("DataContext.CartageAdvice is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CartageAdvice)));
			AssertEquals("DataContext.CombinedCartageAdvice is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.CombinedCartageAdvice)));
			AssertEquals("DataContext.RequestForMissingDocuments is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.RequestForMissingDocuments)));
			AssertEquals("DataContext.Worksheet is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Worksheet)));
			AssertEquals("DataContext.RequestForService is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.RequestForService)));
			AssertEquals("DataContext.Service is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.Service)));
			AssertEquals("DataContext.TimeSlotRequest is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.TimeSlotRequest)));
			AssertEquals("DataContext.GenericFreightJobByComInv is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GenericFreightJobByComInv)));
			AssertEquals("DataContext.GenericFreightJobByContainerIfFCL is Supported", true, Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GenericFreightJobByContainerIfFCL)));
			Assert(Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.GenericFreightJobInvoice)));
			Assert(Declaration.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(DataContext.ARInvoice)));
		}

		protected virtual ZString GetMainNameSpace()
		{
			return "Enterprise.DocumentWrappers.Customs." + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + ".";
		}

		public virtual void TestGetDocBusinessObjects()
		{
			ZString mainNameSpace = GetMainNameSpace();
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObject();

			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Notes, null);
			AssertEquals("Document wrapper for data context of notes is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShipperDepartureNotice, null);
			AssertEquals("Document wrapper for data context of notes is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Worksheet, null);
			AssertEquals("Document wrapper for data context of notes is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CommercialInvoice, null);
			AssertEquals("Document wrapper for data context of CommercialInvoice is of type DocJobComInvoiceHeader", mainNameSpace + "DocJobComInvoiceHeader", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ComInvoiceHeader, null);
			AssertEquals("Document wrapper for data context of ComInvoiceHeader is of type DocJobComInvoiceHeader", mainNameSpace + "DocJobComInvoiceHeader", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ChargeSheet, null);
			AssertEquals("Document wrapper for data context of Charge sheet is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LandedCostEntryHeaders, null);
			AssertEquals("Document wrapper for data context of Charge sheet is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ShipmentDeclaration, null);
			AssertEquals("Document wrapper for data context of ShipmentDeclaration is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CombinedCartageAdvice, null);
			AssertEquals("Document wrapper for data context of CombinedCartageAdvice is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Service, null);
			AssertEquals("Document wrapper for data context of Service is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.TimeSlotRequest, null);
			AssertEquals("Document wrapper for data context of TimeSlotRequest is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			BusinessObject landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.LandedCostHeader, null);
			AssertEquals("Document wrapper for data context of Landed Cost Header is of type LandedCostHeader", "Enterprise.DocumentWrappers.DocLandedCostHeader", result[0].GetType().ToString());

			declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObject();

			declaration.OnGetCusContainersToPrint += new CusContainersToPrintEventHandler(Declaration_OnGetCusContainersToPrint);

			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("Only one document wrapper should be created", 1, result.Length);
			AssertEquals("Document wrapper for data context of cartage advice with no containers is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			BaseCusContainer cusContainer1 = declaration.CusContainers.Count >= 1 ? declaration.CusContainers[0] : declaration.CusContainers.AddNew();
			BaseCusContainer cusContainer2 = declaration.CusContainers.Count >= 2 ? declaration.CusContainers[1] : declaration.CusContainers.AddNew();

			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("Only one document wrapper should be created", 1, result.Length);
			AssertEquals("Document wrapper for export data context of cartage advice with containers is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("Only one document wrapper should be created", 1, result.Length);
			AssertEquals("Document wrapper for import data context of cartage advice with LCL containers is of type DocDeclaration", mainNameSpace + "DocDeclaration", result[0].GetType().ToString());

			declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("A document wrapper should be created for each container", 2, result.Length);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			foreach (DocumentWrapper docWrapper in result)
			{
				hashtable.Add(((BusinessObject)docWrapper.WrappedObject).PK, docWrapper);
			}
			Assert("FCL Container 1", hashtable.ContainsKey(cusContainer1.PK));
			Assert("FCL Container 2", hashtable.ContainsKey(cusContainer2.PK));

			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("A document wrapper should be created for each container", 2, result.Length);

			hashtable = new System.Collections.Hashtable();

			foreach (DocumentWrapper docWrapper in result)
			{
				hashtable.Add(((BusinessObject)docWrapper.WrappedObject).PK, docWrapper);
			}
			Assert("FCL Container 1", hashtable.ContainsKey(cusContainer1.PK));
			Assert("FCL Container 2", hashtable.ContainsKey(cusContainer2.PK));

			declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			cusContainer1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			cusContainer2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CartageAdvice, null);
			AssertEquals("A document wrapper should be created for each container", 2, result.Length);

			hashtable = new System.Collections.Hashtable();

			foreach (DocumentWrapper docWrapper in result)
			{
				hashtable.Add(((BusinessObject)docWrapper.WrappedObject).PK, docWrapper);
			}
			Assert("FCL Container 1", hashtable.ContainsKey(cusContainer1.PK));
			Assert("FCL Container 2", hashtable.ContainsKey(cusContainer2.PK));

			TestGetDocBusinessObjectsForRequestForService(declaration);
		}

		#region TestGetDocBusinessObjectForRequestForService

		void TestGetDocBusinessObjectsForRequestForService(BaseJobDeclaration declaration)
		{
			var servicesSelectionProvider = new Mock<IServicesSelectionProvider>(MockBehavior.Strict);

			Factory.SetValue(() => servicesSelectionProvider.Object);

			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>((parent) => parent == declaration.DocsAndCartage)))
				.Returns((JobService[])null);

			DocumentWrapper[] wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertNull(wrappers);

			AssertEquals("Prerequisite - First service was already added by GetDocumentSupportBusinessObject", 1, declaration.DocsAndCartage.Services.Count);
			servicesSelectionProvider.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>((parent) => parent == declaration.DocsAndCartage)), Times.Once());

			JobService service1 = declaration.DocsAndCartage.Services[0];
			JobService service2 = declaration.DocsAndCartage.Services.AddNew();

			servicesSelectionProvider.Setup(m => m.GetServicesToPrint(It.Is<IHaveServices>((parent) => parent == declaration.DocsAndCartage)))
				.Returns(new JobService[] { service1, service2 });

			wrappers = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.RequestForService, null);
			AssertEquals("Service Wrappers should be created", 2, wrappers.Length);
			AssertEquals("Wrapper type should be DocService", "DocService", wrappers[0].GetType().Name);
			servicesSelectionProvider.Verify(m => m.GetServicesToPrint(It.Is<IHaveServices>((parent) => parent == declaration.DocsAndCartage)), Times.Exactly(2));
		}

		#endregion

		void Declaration_OnGetCusContainersToPrint(object sender, CusContainersToPrintEventArgs e)
		{
			e.ContinueToPrint = true;
		}

		public virtual void TestBusinessContext()
		{
			AssertEquals("Declaration has a business context of customs", BusinessContext.Customs, Declaration.DocumentSupporter.BusinessContext);
		}

		public void TestGetFilterValueMOD()
		{
			//Filter = MOD
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MOD' result is 'SEA'", Core.Constants.TransportModes.Sea, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MOD));
		}

		public virtual void TestGetMenuTemplateFilterValue()
		{
			AssertEquals("Menu Filter", "No", Declaration.DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.AUCountry, null));
		}

		public void TestGetFilterValueMSC()
		{
			//Filter = MSC
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("For filter 'MSC' result is 'EXPIMP'", "EXPIMP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("For filter 'MSC' result is 'EXPIMP'", "EXPIMP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSC));
		}

		public void TestGetFilterValueHIDE()
		{
			//Filter = HIDE
			AssertEquals("For filter 'HIDE=Y' result is Y for default with no exclusions", "Y", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.HIDE));
		}

		public void TestGetFilterValueLGR()
		{
			//Filter = LGR
			AssertEquals("For filter 'LGR' result is ''", "", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.LGR));
		}

		public void TestGetFilterValueCTY()
		{
			//Filter = CTY
			AssertEquals("For filter 'CTY' result is current country code", Declaration.CountryCode, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.CTY));
		}

		public virtual void TestGetFilterValueMSGBKR()
		{
			//Filter = MSGBKR
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'EXP'", "EXP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("For filter 'MSGBKR' result is 'DRW'", "DRW", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'IMP'", "IMP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			MakeDeclarationExWarehouse(Declaration);
			AssertEquals("For filter 'MSGBKR' result is 'IMP'", "IMP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
		}

		public virtual void TestGetFilterValueMSGBKRCTY()
		{
			//Filter = MSGBKRCTY
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Declaration.CountryCode);
			AssertEquals("For filter 'MSGBKR' result is 'EXP' + Current Country Code", "EXP" + customsCountry, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			AssertEquals("For filter 'MSGBKR' result is 'DRW' + Current Country Code", "DRW" + customsCountry, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'IMP' + Current Country Code", "IMP" + customsCountry, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			MakeDeclarationExWarehouse(Declaration);
			AssertEquals("For filter 'MSGBKR' result is 'IMP' + Current Country Code", "IMP" + customsCountry, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			using (Declaration.Company.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("For filter 'MSGBKR' result is 'IMPUS'", "IMPUS", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
			}
		}

		public virtual void TestGetFilterValueMSGBKRCTYMOD()
		{
			//Filter = MSGBKRCTYMOD
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Declaration.CountryCode);
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country Code + \"AIR\"", "EXP" + customsCountry + "AIR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			Declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Drawback;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'DRW' + Current Country Code + \"SEA\"", "DRW" + customsCountry + "SEA", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + \"AIR\"", "IMP" + customsCountry + "AIR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			MakeDeclarationExWarehouse(Declaration);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + \"SEA\"", "IMP" + customsCountry + "SEA", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			using (Declaration.Company.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMPSEA'", "IMPUSSEA", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
			}
		}

		public void TestGetFilterValueASYCUDA()
		{
			//Filter = ASYCUDA
			var isAsycudaCountry = ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(Declaration.CountryCode);
			if (isAsycudaCountry)
			{
				AssertEquals("For filter 'ASYCUDA' result is Y for Asycuda country", "Y", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.ASYCUDA));
			}
			else
			{
				AssertEquals("For filter 'ASYCUDA' result is N", "N", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.ASYCUDA));
			}
		}

		protected virtual void MakeDeclarationExWarehouse(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
		}

		public virtual void TestGetFilterValueMSGBKRCTYAPP()
		{
			//Filter = MSGBKRCTYAPP
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Result should be 'EXP' + Current Country Code + 'CMR'", "EXP" + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "CMR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));

			using (Declaration.Company.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Result should be 'EXPUSCMR'", "EXPUSCMR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYAPP));
			}
		}

		public virtual void TestGetFilterValueBKRCTYAPP()
		{
			//Filter = BKRCTYAPP
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Result should be Current Country Code + 'CMR'", Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) + "CMR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));

			using (Declaration.Company.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("Result should be 'USCMR'", "USCMR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));
			}
		}

		public virtual void TestGetFilterValueBKRCTY()
		{
			//Filter = BKRCTY
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(Declaration.CountryCode);
			AssertEquals("For filter 'BKRCTY' result is Customs Country Code", customsCountry, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTY));

			using (Declaration.Company.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				AssertEquals("For filter 'BKRCTY' result is 'US'", "US", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTY));
			}
		}

		public virtual void TestGetFilterValueEXPBKRLIC()
		{
			//Filter = EXPBKRLIC
			AssertEquals("For filter 'EXPBKRLIC' result is 'Y'", "Y", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.EXPBKRLIC));
		}

		public virtual void TestGetContactOrganisation()
		{
			Declaration.JE_OH_Forwarder = ZGuid.Empty;
			AssertNull("GetContactOrganisation on FWE doesnt blow up on null Forwarder", Declaration.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);

			var fWD = Factory.NewWithValidTestData<OrgHeader>();
			Declaration.JE_OH_Forwarder = fWD.PK;
			AssertEquals("GetContactOrganisation(FWE) returning the Forwarder", fWD, Declaration.DocumentSupporter.GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals("GetContactOrganisation(FWI) returning the Forwarder", fWD, Declaration.DocumentSupporter.GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY).OrgHeader);

			var cTOOrg = Factory.NewWithValidTestData<OrgHeader>();
			var cTOAddress = cTOOrg.Addresses.AddNew();
			Declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = cTOAddress.PK;
			AssertEquals("Contact Type should be CTO", cTOOrg.PK, Declaration.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.CTO, DocumentDirection.ANY).OrgHeader.PK);

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			Declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertEquals("GetContactOrganisation(TIP) returning the Carrier", shippingLine, Declaration.DocumentSupporter.GetContactOrganisation("", ContactType.ShippingLine, DocumentDirection.ANY).OrgHeader);

			var lOCOrg = Factory.NewWithValidTestData<OrgHeader>();
			var lOCAddress = lOCOrg.Addresses.AddNew();
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = Declaration.PK;
			Declaration.Job.JH_OA_LocalChargesAddr = lOCAddress.PK;
			AssertEquals("Contact Type should be LOC", lOCOrg.PK, Declaration.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalClient, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestCartageAdvicePrintedForPrint()
		{
			var declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();

			var filter = new DocumentZQuery("Customs", "Pre-Alert");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			var docoSupporter = new JobDeclarationDocumentSupporterForTest(declaration);
			docoSupporter.DocumentEventSource_DocumentPrintedTestMethod(this, eventArgs);
			Assert("Cartage Advised time is not filled in", declaration.JP_Calc_CartageAdvised.IsEmpty);

			filter = new DocumentZQuery("Customs", "Cartage Advice");
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem);
			docoSupporter = new JobDeclarationDocumentSupporterForTest(declaration);
			docoSupporter.DocumentEventSource_DocumentPrintedTestMethod(this, eventArgs);
			Assert("Cartage Advised time is not filled in", declaration.JP_Calc_CartageAdvised.IsEmpty);

			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem);
			docoSupporter = new JobDeclarationDocumentSupporterForTest(declaration);
			docoSupporter.DocumentEventSource_DocumentPrintedTestMethod(this, eventArgs);
			declaration = (new BusinessObjectFactory()).Load<BaseJobDeclaration>(declaration.PK);
			Assert("Cartage Advised time is filled in", !declaration.JP_Calc_CartageAdvised.IsEmpty);
		}

		public void TestCartageAdvicePrintedForEmailAndFax()
		{
			var declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.Factory.Save();

			var filter = new DocumentZQuery("Customs", "Cartage Advice");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);
			var docoSupporter = new JobDeclarationDocumentSupporterForTest(declaration);
			docoSupporter.DocumentEventSource_DocumentPrintedTestMethod(this, eventArgs);
			declaration = (new BusinessObjectFactory()).Load<BaseJobDeclaration>(declaration.PK);
			Assert("Cartage Advised time is filled in", !declaration.JP_Calc_CartageAdvised.IsEmpty);
		}

		public void TestCallingGetDataStateBeforeRunWithNull()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			JobDeclarationDocumentSupporterForTest supporter = new JobDeclarationDocumentSupporterForTest(declaration);
			AssertNoExceptionThrown(delegate
			{ supporter.GetDataStateBeforeRun(null); });
		}

		public void TestLandedCostingMenu()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			DocumentZQuery filter = new DocumentZQuery("Customs", "Landed Costing");
			var menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			DocumentPrintedEventArgs eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);

			JobDeclarationDocumentSupporterForTest docSupporter = new JobDeclarationDocumentSupporterForTest(declaration);
			DocumentSupporterDataState result = docSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Not valid yet", false, result.IsValid);
			AssertEquals("Error message", JobDeclarationDocumentSupporterForTest.LandedCostingErrorMessage, result.ErrorMessage);

			filter = new DocumentZQuery("Customs", "Landed Costing - Old");
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);

			result = docSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Valid for Landed Costing - Old", true, result.IsValid);
			AssertEquals("Error message", true, string.IsNullOrEmpty(result.ErrorMessage));

			filter = new DocumentZQuery("Customs", "Landed Costing");
			menuItem = Factory.LoadTop1<StmMenuItem>(filter);
			eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.TakenFromContact, menuItem);

			BusinessObject landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";
			result = docSupporter.GetDataStateBeforeRun(menuItem);
			AssertEquals("Landed Costing Lines dont exist", false, result.IsValid);
		}

		public void TestMasterBillContextIsSupported()
		{
			Bill masterBill1 = Declaration.Bills.AddNew();
			masterBill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "HELLO";
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "GoodBye";
			Bill masterBill2 = Declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "SAILOR";

			BaseJobDeclarationDocumentSupporter supporter = new BaseJobDeclarationDocumentSupporter(Declaration);

			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".MasterBill")));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(".MasterBill"), null);

			AssertEquals(2, result.Length);
			AssertEquals(masterBill1, BODocDataProvider.GetBusinessObject(result[0]));
			AssertEquals(masterBill2, BODocDataProvider.GetBusinessObject(result[1]));
		}

		public void TestCusEntryHeaderIsSupported()
		{
			Declaration.CustomsEntryHeaders.RemoveAll();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			BaseJobDeclarationDocumentSupporter supporter = new BaseJobDeclarationDocumentSupporter(Declaration);
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValue(".CusEntryHeader")));

			IBODocDataProvider[] result = supporter.GetBODocDataProviders(new DataContextValue(".CusEntryHeader"), null);

			AssertEquals(2, result.Length);
			AssertEquals(entry, BODocDataProvider.GetBusinessObject(result[0]));
			AssertEquals(entry2, BODocDataProvider.GetBusinessObject(result[1]));
		}

		public void TestStorageDocsAreEditableIfInRelated()
		{
			AssertEquals("Storage docs should be modifyable if the business object is a related business object and not a top level business object", true, Declaration.DocumentSupporter.StorageDocsAreEditableIfInRelated);
		}

		#region Implementation

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObjectForRunningDocuments();
				var messageTypeList = new CodeDescriptionPairList(declaration.Lookups.MessageTypeList);
				RemoveMessageTypesNotInvolvedInTesting(messageTypeList, declaration.JE_MessageType);
				yield return declaration;
				foreach (ICodeDescription messageType in messageTypeList)
				{
					declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObjectForRunningDocuments();
					declaration.JE_MessageType = messageType.Code;
					declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
					yield return declaration;
				}
			}
		}

		protected virtual void RemoveMessageTypesNotInvolvedInTesting(CodeDescriptionPairList messageTypeList, ZString msgType)
		{
			messageTypeList.RemoveCode(msgType);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return GetDocumentSupportableDeclaration();
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObjectForRunningDocuments()
		{
			var declaration = (BaseJobDeclaration)GetDocumentSupportableBusinessObject();

			declaration.Bills.AddNew();
			declaration.Bills.AddNew();
			declaration.Importer?.CustomsCodes.AddNew();
			declaration.Importer?.CustomsCodes.AddNew();

			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			invoiceHeader1.JZ_ValuationDateOverride = ZDate.Today;
			var invoice1Line1 = invoiceHeader1.JobComInvoiceLines[0];
			invoice1Line1.JI_Tariff = "11";

			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.Charges.AddNew();
			invoiceHeader2.Charges.AddNew();
			invoiceHeader2.JZ_ValuationDateOverride = ZDate.Today.AddDays(-1);

			var invoice2Line1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_Tariff = "21";
			var invoice2Line2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_Tariff = "22";

			declaration.CusContainers.AddNew();
			var container = declaration.CusContainers.AddNew();
			var invoiceLine1ToContainer = invoice2Line1.ContainersPivot.AddNew();
			invoiceLine1ToContainer.C2_CO = container.PK;
			var invoiceLine2ToContainer = invoice2Line2.ContainersPivot.AddNew();
			invoiceLine2ToContainer.C2_CO = container.PK;

			// Assuming headers merge on valuation date and lines merge on tariff.
			// Should get 2 entries with 1 and 2 lines respectively
			DoMerge(declaration);

			var entryNum = 0;
			foreach (CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				try
				{
					entryHeader.EntryNumber = "EN" + (entryNum += 111);
				}
				catch (Exception) { }

				entryHeader.Charges.AddNew();
				entryHeader.Charges.AddNew();

				foreach (CusEntryLine entryLine in entryHeader.MergedLines)
				{
					entryLine.Fees.AddNew();
					entryLine.Fees.AddNew();
				}
			}

			return declaration;
		}

		protected virtual BaseJobDeclaration GetDocumentSupportableDeclaration()
		{
			var declaration = GetJobDeclaration();
			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			DoMerge(declaration);
			declaration.DocsAndCartage.Services.AddNew();
			return declaration;
		}

		protected BaseJobDeclaration GetDocumentSupportBusinessObjectWithLandedCosting()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader1.JZ_ValuationCode = "V1";
			var invLine11 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_Tariff = "1134123456";
			var invLine12 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invLine12.JI_Tariff = "1234123456";

			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_ValuationCode = "V2";
			var invLine21 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invLine21.JI_Tariff = "2134123456";
			var invLine22 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invLine22.JI_Tariff = "2234123456";

			DoMerge(declaration);

			if (declaration.CustomsEntryHeaders.FirstOrDefault() is CusEntryHeader entryHeader)
			{
				entryHeader.CH_BGMReference = "B00010000/1";
			}

			var consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consignee.OH_RL_NKClosestPort = "AUSYD";
			declaration.JE_OH_Importer = consignee.PK;

			var landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			var landedCostHistory = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHistory>();
			landedCostHistory[LandedCostHistorySchema.LH_LT.Name] = landedCostHeader.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentID.Name] = invLine11.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentTableCode.Name] = JobComInvoiceLineSchema.Constants.Prefix;

			declaration.DocsAndCartage.Services.AddNew();
			return declaration;
		}

		protected virtual void DoMerge(BaseJobDeclaration declaration)
		{
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
		}

		//Go to Country-specific JobDeclarationSupporterTest and override there rather using Country Constants
		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			ZBool result = documentCommand.SU_MenuName.StartsWith("Landed Costing");

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Australia)
			{
				if (documentCommand.SU_MenuName.StartsWith("Standard Landed Costing")
					|| documentCommand.SU_MenuName.StartsWith("Entry Print"))
				{
					result = ZBool.True;
				}
			}

			if (documentCommand.SU_MenuName.Contains("Cartage Advice") || documentCommand.SU_MenuName.Contains("DocBuilder Invoice"))
			{
				result = ZBool.True;
			}

			return result;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = new Dictionary<string, int>();

				maxHits["CusAddInfo"] = 2;
				maxHits["CusContainer"] = 2;
				maxHits["CusContainerInvoiceLinePivot"] = 2;
				maxHits["CusDecHouseContainerPivot"] = 2;
				maxHits["CusEntryHeader"] = 2;
				maxHits["CusEntryLine"] = 2;
				maxHits["CusEntryNum"] = 2;
				maxHits["CusInBondHeader"] = 2;
				maxHits["GenPivot"] = 2;
				maxHits["JobComInvoiceHeader"] = 3;
				maxHits["JobComInvoiceHeaderRefs"] = 4;
				maxHits["JobComInvHeaderCharge"] = 2;
				maxHits["JobDocAddress"] = 2;
				maxHits["OrgAddress"] = 3;
				maxHits["OrgContact"] = 2;
				maxHits["OrgCountryData"] = 2;
				maxHits["OrgDocument"] = 2;
				maxHits["OrgHeader"] = 2;
				maxHits["StmALog"] = 2;
				maxHits["StmNote"] = 2;
				
				return maxHits;
			}
		}

		protected BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = GetJobDeclaration()); }
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected virtual ZString MessageTypeCodeForImportDeclaration
		{
			get { return Enterprise.Customs.Business.JobMessageTypeList.Codes.Import; }
		}

		protected virtual ZString MessageTypeCodeForExportDeclaration
		{
			get { return Enterprise.Customs.Business.JobMessageTypeList.Codes.Export; }
		}
		#endregion
	}
}
