using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryHeader))]
	sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
	{
		public void TestStaticNewReturnsUSEntryHeader()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var entryHeader = Factory.New<CusEntryHeader>();
			var docEntryHeader = DocCusEntryHeader.New(entryHeader, Factory);
			AssertEquals(typeof(DocCusEntryHeader), docEntryHeader.GetType());
		}

		public void TestNew()
		{
			AssertNull("Created with null", DocCusEntryHeader.New(null, Factory));
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertNotNull("Created with a valild object", DocCusEntryHeader.New(entryHeader, Factory));
		}

		public void TestDeclaration()
		{
			AssertNotNull("Declaration", EntryHeader.Declaration);
			AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), EntryHeaderWrapperInternal.Declaration.GetType());
		}

		public void TestEntryLinesCollection()
		{
			AssertEquals("MergedLines.Count", 1, EntryHeader.MergedLines.Count);
			CusEntryLine line1 = EntryHeader.MergedLines[0];
			CusEntryLine line2 = EntryHeader.MergedLines.AddNew();
			CusEntryLine line3 = EntryHeader.MergedLines.AddNew();

			line1.CL_LineNumber = 3;
			line2.CL_LineNumber = 1;
			line3.CL_LineNumber = 2;

			AssertEquals(3, EntryHeaderWrapperInternal.EntryLines.Count);
			AssertEquals((short)1, EntryHeaderWrapperInternal.EntryLines[0].LineNumber);
			AssertEquals((short)2, EntryHeaderWrapperInternal.EntryLines[1].LineNumber);
			AssertEquals((short)3, EntryHeaderWrapperInternal.EntryLines[2].LineNumber);
		}

		public void TestForwarder()
		{
			AssertNotNull("ForwarderDocumentaryAddress", EntryHeader.Declaration.Forwarder);
			AssertEquals("Declaration is of type DocDeclaration", typeof(Enterprise.DocumentWrappers.DocOrganisation), EntryHeaderWrapperInternal.Forwarder.GetType());
			AssertEquals("Forwarder.Name", EntryHeader.Declaration.Forwarder.OH_FullName, EntryHeaderWrapperInternal.Forwarder.Name);
		}

		public void TestForwarderIdentificatioNumber()
		{
			AssertNotNull("ForwarderDocumentaryAddress.Organisation", EntryHeader.Declaration.Forwarder);
			AssertEquals("ForwarderIdentificatioNumber", new SEDIdentificationAndNumberDecider(EntryHeader.Declaration.Forwarder, EntryHeader.Declaration.Forwarder.MainAddress, null).IdentificationNumber, EntryHeaderWrapperInternal.ForwarderIdentificatioNumber);
		}

		public void TestUSPPI()
		{
			AssertNotNull(Invoice.US_USPPI);
			AssertNotNull(Invoice.US_USPPI.Organisation);
			AssertNotNull("EntryHeader.USPPI", EntryHeader.USPPI);
			AssertEquals("USPPI is of type DocDeclaration", typeof(DocUSOrganisation), EntryHeaderWrapperInternal.USPPI.GetType());
			AssertEquals("USPPI.Name", EntryHeader.USPPI.Organisation.OH_FullName, EntryHeaderWrapperInternal.USPPI.Name);
		}

		public void TestUSPPIAddress()
		{
			EntryHeader.RandomHeader.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
			Assert(EntryHeader.RandomHeader.SupplierPickupAddress.E2_OA_Address.IsEmpty);
			AssertEquals(typeof(Enterprise.DocumentWrappers.DocDocAddress), EntryHeaderWrapperInternal.USPPIAddress.GetType());
			AssertEquals(EntryHeader.RandomHeader.USPPIDocAddress.Address1, EntryHeaderWrapperInternal.USPPIAddress.Address1);

			var org = EntryHeader.Declaration.Supplier;
			var newAddress = org.Addresses.AddNew();
			newAddress.Address1 = "Test 20200521";
			EntryHeader.RandomHeader.SupplierPickupAddress.E2_OA_Address = newAddress.PK;
			Assert(!EntryHeader.RandomHeader.SupplierPickupAddress.E2_OA_Address.IsEmpty);
			AssertEquals(typeof(Enterprise.DocumentWrappers.DocDocAddress), EntryHeaderWrapperInternal.USPPIAddress.GetType());
			AssertEquals(EntryHeader.RandomHeader.SupplierPickupAddress.Address1, EntryHeaderWrapperInternal.USPPIAddress.Address1);
		}

		public void TestUSPPIIdentificatioNumber()
		{
			AssertNotNull("USPPI.Organisation", EntryHeader.USPPI.Organisation);
			AssertEquals("USPPIIdentificatioNumber", new SEDIdentificationAndNumberDecider(EntryHeader.USPPI.Organisation, EntryHeader.USPPI.Organisation.MainAddress, null).IdentificationNumber, EntryHeaderWrapperInternal.USPPIIdentificatioNumber);

			EntryHeader.USPPI.USOrganisationDocAddress.E2_AddressOverride = true;
			EntryHeader.USPPI.USOrganisationDocAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			EntryHeader.USPPI.USOrganisationDocAddress.E2_GovRegNum = "58-123456789";
			AssertEquals("USPPIIdentificatioNumber", "58123456789", EntryHeaderWrapperInternal.USPPIIdentificatioNumber);
		}

		public void TestExportUltimateConsignee()
		{
			AssertNotNull("EntryHeader.ExportUltimateConsignee", EntryHeader.ExportUltimateConsignee);
			AssertEquals("ExportUltimateConsignee is of type DocDeclaration", typeof(DocUSOrganisation), EntryHeaderWrapperInternal.ExportUltimateConsignee.GetType());
			AssertEquals("ExportUltimateConsignee.Name", EntryHeader.ExportUltimateConsignee.Organisation.OH_FullName, EntryHeaderWrapperInternal.ExportUltimateConsignee.Name);
		}

		public void TestIntermediateConsignee()
		{
			AssertNotNull("EntryHeader.IntermediateConsignee", EntryHeader.IntermediateConsignee);
			AssertEquals("IntermediateConsignee is of type DocDeclaration", typeof(DocUSOrganisation), EntryHeaderWrapperInternal.IntermediateConsignee.GetType());
			AssertEquals("IntermediateConsignee.Name", EntryHeader.IntermediateConsignee.Organisation.OH_FullName, EntryHeaderWrapperInternal.IntermediateConsignee.Name);
		}

		public void TestTransportationReferenceNumber()
		{
			AssertEquals("TransportationReferenceNumber", EntryHeader.TransportationReferenceNumber, EntryHeaderWrapperInternal.TransportationReferenceNumber);
		}

		public void TestIsTransactionsRelatedAndIsNotTransactionsRelated()
		{
			EntryHeader.RandomHeader.US_TransactionsRelated = US.Business.YesNoDefaultList.Codes.Yes;
			AssertEquals("IsTransactionsRelated", "X", EntryHeaderWrapperInternal.IsTransactionsRelated);
			AssertEquals("IsNotTransactionsRelated", "", EntryHeaderWrapperInternal.IsNotTransactionsRelated);
			EntryHeader.RandomHeader.US_TransactionsRelated = US.Business.YesNoDefaultList.Codes.No;
			AssertEquals("IsTransactionsRelated", "", EntryHeaderWrapperInternal.IsTransactionsRelated);
			AssertEquals("IsNotTransactionsRelated", "X", EntryHeaderWrapperInternal.IsNotTransactionsRelated);
		}

		public void TestPointOfOriginOrForeignTradeZone()
		{
			Declaration.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;
			Declaration.US_StateOfOrigin = "";
			Declaration.US_ForeignTradeZone = "";
			EntryHeader.RandomHeader.US_StateOfOrigin = "CA";
			EntryHeader.RandomHeader.US_ForeignTradeZone = "AAS";
			AssertEquals("IsForeignTradeZoneRequired", false, EntryHeader.IsForeignTradeZoneRequired);
			AssertEquals("PointOfOriginOrForeignTradeZone", "CA", EntryHeaderWrapperInternal.PointOfOriginOrForeignTradeZone);
			Declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertEquals("IsForeignTradeZoneRequired", true, EntryHeader.IsForeignTradeZoneRequired);
			AssertEquals("PointOfOriginOrForeignTradeZone", "AAS", EntryHeaderWrapperInternal.PointOfOriginOrForeignTradeZone);
		}

		public void TestCountryOfUltimateDestination()
		{
			AssertEquals("CountryOfUltimateDestination", EntryHeader.CountryOfUltimateDestination, EntryHeaderWrapperInternal.CountryOfUltimateDestination);
		}

		public void TestTransportModeDescription()
		{
			AssertEquals("ModeOfTransport", EntryHeaderWrapperInternal.Declaration.TransportModeDescription, EntryHeaderWrapperInternal.TransportModeDescription);
		}

		public void TestLoadingPier()
		{
			Declaration.JE_RL_NKPortOfLoading = helper.USCHI.Code;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("LoadingPier", ZString.Empty, EntryHeaderWrapperInternal.LoadingPier);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("LoadingPier", helper.USCHI.RL_PortName, EntryHeaderWrapperInternal.LoadingPier);
		}

		public void TestCarrierCode()
		{
			AssertEquals("CarrierCode", EntryHeader.CarrierCode, EntryHeaderWrapperInternal.CarrierCode);
		}

		public void TestExportingCarrier()
		{
			AssertEquals("ExportingCarrier", EntryHeader.ExportingCarrier, EntryHeaderWrapperInternal.ExportingCarrier);
		}

		public void TestPortOfExportName()
		{
			Declaration.US_RL_NKPortOfExport = helper.USCHI.Code;
			AssertEquals("PortOfExportName", helper.USCHI.RL_PortName, EntryHeaderWrapperInternal.PortOfExportName);

			Declaration.US_RL_NKPortOfExport = ZString.Empty;
			AssertEquals("PortOfExportName", ZString.Empty, EntryHeaderWrapperInternal.PortOfExportName);
		}

		public void TestImportEntryNumber()
		{
			AssertEquals("ImportEntryNumber", EntryHeader.ImportEntryNumber, EntryHeaderWrapperInternal.ImportEntryNumber);
		}

		public void TestIsHazardousCargoAndIsNotHazardousCargo()
		{
			EntryHeader.RandomHeader.US_HazardousCargo = US.Business.YesNoDefaultList.Codes.Yes;
			AssertEquals("IsHazardousCargo", "X", EntryHeaderWrapperInternal.IsHazardousCargo);
			AssertEquals("IsNotHazardousCargo", "", EntryHeaderWrapperInternal.IsNotHazardousCargo);
			EntryHeader.RandomHeader.US_HazardousCargo = US.Business.YesNoDefaultList.Codes.No;
			AssertEquals("IsHazardousCargo", "", EntryHeaderWrapperInternal.IsHazardousCargo);
			AssertEquals("IsNotHazardousCargo", "X", EntryHeaderWrapperInternal.IsNotHazardousCargo);
		}

		public void TestPortOfUnloadingName()
		{
			Declaration.JE_RL_NKPortOfArrival = helper.AUSYD.Code;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertNotEquals("PortOfUnloadingName", EntryHeaderWrapperInternal.Declaration.PortOfArrival.PortNameAndCountryName, EntryHeaderWrapperInternal.PortOfUnloadingName);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("PortOfUnloadingName", EntryHeaderWrapperInternal.Declaration.PortOfArrival.PortNameAndCountryName, EntryHeaderWrapperInternal.PortOfUnloadingName);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("PortOfUnloadingName", EntryHeaderWrapperInternal.Declaration.PortOfArrival.PortNameAndCountryName, EntryHeaderWrapperInternal.PortOfUnloadingName);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertNotEquals("PortOfUnloadingName", EntryHeaderWrapperInternal.Declaration.PortOfArrival.PortNameAndCountryName, EntryHeaderWrapperInternal.PortOfUnloadingName);
		}

		public void TestIsRoutedTransactionAndIsNotRoutedTransaction()
		{
			EntryHeader.RandomHeader.US_RoutedTransaction = US.Business.YesNoDefaultList.Codes.Yes;
			AssertEquals("IsRoutedTransaction", "X", EntryHeaderWrapperInternal.IsRoutedTransaction);
			AssertEquals("IsNotRoutedTransaction", "", EntryHeaderWrapperInternal.IsNotRoutedTransaction);
			EntryHeader.RandomHeader.US_RoutedTransaction = US.Business.YesNoDefaultList.Codes.No;
			AssertEquals("IsRoutedTransaction", "", EntryHeaderWrapperInternal.IsRoutedTransaction);
			AssertEquals("IsNotRoutedTransaction", "X", EntryHeaderWrapperInternal.IsNotRoutedTransaction);
		}

		public void TestInbondType()
		{
			AssertEquals("InbondType", EntryHeader.InbondType, EntryHeaderWrapperInternal.InbondType);
		}

		public void TestIsContainerizedAndIsNotContainerized()
		{
			EntryHeader.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerized", "X", EntryHeaderWrapperInternal.IsContainerized);
			AssertEquals("IsNotContainerized", "", EntryHeaderWrapperInternal.IsNotContainerized);
			EntryHeader.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerized", "", EntryHeaderWrapperInternal.IsContainerized);
			AssertEquals("IsNotContainerized", "X", EntryHeaderWrapperInternal.IsNotContainerized);
			EntryHeader.Declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerized", "X", EntryHeaderWrapperInternal.IsContainerized);
			AssertEquals("IsNotContainerized", "", EntryHeaderWrapperInternal.IsNotContainerized);
		}

		public void TestLicenseNumberAndLicenseException()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(EntryHeader.Factory, new ZString[] { USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C31 });

			AssertEquals(1, EntryHeader.MergedLines.Count);
			CusEntryLine line1 = EntryHeader.MergedLines[0];
			Declaration.US_LicenseType = "";
			InvoiceLine.US_LicenseType = "";
			InvoiceLine.US_LicenseNo = "";
			AssertEquals("PreCondition: Line1.LicenseType", "", line1.LicenseType);
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleLicenseDetails);
			AssertEquals("LicenseNumberAndLicenseException", "", EntryHeaderWrapperInternal.LicenseNumberAndLicenseException);

			InvoiceLine.US_LicenseType = USAESLicenseCode.Codes.C30;
			InvoiceLine.US_LicenseNo = "LIC23423";
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleLicenseDetails);
			AssertEquals("LicenseNumberAndLicenseException", "LIC23423", EntryHeaderWrapperInternal.LicenseNumberAndLicenseException);

			CusEntryLine line2 = EntryHeader.MergedLines.AddNew();
			JobComInvoiceLine invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C31;
			invoiceLine2.US_LicenseNo = "32323GD";
			invoiceLine2.JI_CL = line2.PK;

			AssertEquals(true, EntryHeaderWrapperInternal.HasMultipleLicenseDetails);
			AssertEquals("LicenseNumberAndLicenseException", "SEE ABOVE", EntryHeaderWrapperInternal.LicenseNumberAndLicenseException);

			EntryHeader.MergedLines.RemoveAndDeleteAll();
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleLicenseDetails);
			AssertEquals("LicenseNumberAndLicenseException", "", EntryHeaderWrapperInternal.LicenseNumberAndLicenseException);
		}

		public void TestECCN()
		{
			AssertEquals(1, EntryHeader.MergedLines.Count);
			CusEntryLine line1 = EntryHeader.MergedLines[0];
			Declaration.US_ECCN = "";
			InvoiceLine.US_ECCN = "";
			AssertEquals("PreCondition: Line1.ECCN", "", line1.ECCN);
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleECCN);
			AssertEquals("ECCN", "", EntryHeaderWrapperInternal.ECCN);

			InvoiceLine.US_ECCN = "ADESD";
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleECCN);
			AssertEquals("ECCN", "ADESD", EntryHeaderWrapperInternal.ECCN);

			CusEntryLine line2 = EntryHeader.MergedLines.AddNew();
			JobComInvoiceLine invoiceLine2 = Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_ECCN = "KHSDD";
			invoiceLine2.JI_CL = line2.PK;

			AssertEquals(true, EntryHeaderWrapperInternal.HasMultipleECCN);
			AssertEquals("ECCN", "SEE ABOVE", EntryHeaderWrapperInternal.ECCN);

			EntryHeader.MergedLines.RemoveAndDeleteAll();
			AssertEquals(false, EntryHeaderWrapperInternal.HasMultipleECCN);
			AssertEquals("ECCN", "", EntryHeaderWrapperInternal.ECCN);
		}

		public void TestIsShippedViaAir()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("", EntryHeaderWrapperInternal.IsShippedViaAir);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("X", EntryHeaderWrapperInternal.IsShippedViaAir);
		}

		public void TestIsShippedViaSea()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", EntryHeaderWrapperInternal.IsShippedViaSea);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("X", EntryHeaderWrapperInternal.IsShippedViaSea);
		}

		public void TestIsShippedViaRoad()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", EntryHeaderWrapperInternal.IsShippedViaRoad);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("X", EntryHeaderWrapperInternal.IsShippedViaRoad);
		}

		public void TestIsShippedViaRail()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", EntryHeaderWrapperInternal.IsShippedViaRail);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("X", EntryHeaderWrapperInternal.IsShippedViaRail);
		}

		public void TestIsShippedViaCourier()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("", EntryHeaderWrapperInternal.IsShippedViaCourier);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals("X", EntryHeaderWrapperInternal.IsShippedViaCourier);
		}

		public void TestIsDirect()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			AssertEquals("", EntryHeaderWrapperInternal.IsDirect);

			ForwardingConsol consol = shipment.Consols.AddNew();
			AssertEquals("", EntryHeaderWrapperInternal.IsDirect);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("X", EntryHeaderWrapperInternal.IsDirect);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("", EntryHeaderWrapperInternal.IsDirect);
		}

		public void TestIsConsolidate()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			AssertEquals("", EntryHeaderWrapperInternal.IsConsolidate);

			ForwardingConsol consol = shipment.Consols.AddNew();
			AssertEquals("X", EntryHeaderWrapperInternal.IsConsolidate);

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AssertEquals("", EntryHeaderWrapperInternal.IsConsolidate);

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			AssertEquals("X", EntryHeaderWrapperInternal.IsConsolidate);
		}

		public void TestIsPrepaid()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			Declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals("X", EntryHeaderWrapperInternal.IsPrepaid);

			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;
			AssertEquals("", EntryHeaderWrapperInternal.IsPrepaid);
		}

		public void TestIsCollect()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			Declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			AssertEquals("", EntryHeaderWrapperInternal.IsCollect);

			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;
			AssertEquals("X", EntryHeaderWrapperInternal.IsCollect);
		}

		#region Implementation

		protected override CusEntryHeader GetNewEntryHeader()
		{
			return EntryHeader;
		}

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
					fEntryHeader.CH_MessageType = Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.EntrySummary;
					AssertNotNull(EntryLine);
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					fEntryLine = EntryHeader.MergedLines.AddNew();
					InvoiceLine.JI_CL = EntryLine.PK;
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = helper.CreateSeaExportDeclaration();
					declaration.US_EntryFilerCode = "ABC";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		protected override Enterprise.Customs.Business.BaseJobDeclaration BaseDeclaration
		{
			get
			{
				if (baseDeclaration == null)
				{
					baseDeclaration = Factory.New<JobDeclaration>();
				}
				((JobDeclaration)baseDeclaration).US_EntryFilerCode = "ABC";
				return baseDeclaration;
			}
		}
		Enterprise.Customs.Business.BaseJobDeclaration baseDeclaration;

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Declaration.Invoices[0];
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = Invoice.JobComInvoiceLines[0];
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;
		DeclarationTestHelper helper;

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override void SetUp()
		{
			helper = new DeclarationTestHelper(Factory);
			base.SetUp();
		}

		protected override void TearDown()
		{
			fEntryHeader = null;
			fEntryLine = null;
			fInvoice = null;
			fInvoiceLine = null;
			base.TearDown();
		}

		protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
		{
			return DocCusEntryHeader.New(entryHeader, Factory);
		}

		#endregion
	}
}
