using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	public class TSWMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetXMLMessageSerializesToUTF8EncodingAndLineBreakersTabsRemoved()
		{
			var expectedSerializationEncoding = @"<?xml version=""1.0"" encoding=""utf-8""?>";
			string xmlGenerated = TSWMessageBuilder.GetXMLMessage();
			AssertEquals("xml generated must be utf-8 encoded", true, xmlGenerated.StartsWith(expectedSerializationEncoding));
			AssertNotContains("xml generated should Contains", @"0			00
0	9
9	0
	8
C", xmlGenerated);
			AssertContains("xml generated should Contains", "<ID>0   00  0 9  9 0   8  C</ID>", xmlGenerated);
		}

		#region Implementation
		OCRMessageBuilder TSWMessageBuilder
		{
			get
			{
				if (fTSWMessageBuilder == null)
				{
					fTSWMessageBuilder = new OCRMessageBuilder(new OCRConsolWrapper(Consol, AdditionalInfo, null, "", "", ""), TSWTransactionTypes.Original, @"0			00
0	9
9	0
	8
C");
				}

				return fTSWMessageBuilder;
			}
		}
		protected OCRMessageBuilder fTSWMessageBuilder;

		protected ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<ForwardingConsol>();
				}

				return fConsol;
			}
		}
		ForwardingConsol fConsol;

		protected DummyAdditionalMessageInformation AdditionalInfo
		{
			get
			{
				if (fAdditionalInfo == null)
				{
					fAdditionalInfo = new DummyAdditionalMessageInformation(TSWTransactionTypes.Original, Consol.Factory);
				}

				return fAdditionalInfo;
			}
		}
		DummyAdditionalMessageInformation fAdditionalInfo;

		public class DummyAdditionalMessageInformation : IAdditionalInformation
		{
			public DummyAdditionalMessageInformation(TSWTransactionTypes transactionType, BusinessObjectFactory factory)
			{
			}

			public DummyAdditionalMessageInformation(TSWTransactionTypes transactionType, BusinessObjectFactory factory, string cancellationChangeReason)
			{
				this.cancelChangeReason = cancellationChangeReason;
			}

			readonly string cancelChangeReason;
			public ZString FreeText
			{
				get
				{
					return "";
				}
			}

			public ZBool OverrideIndicator
			{
				get
				{
					return false;
				}
			}

			public ZString ManualOverrideText
			{
				get
				{
					return "";
				}
			}

			public ZString AdditionalStatementText
			{
				get
				{
					return cancelChangeReason;
				}
			}

			public IEnumerable<ITSWAttachment> SupportingDocuments => Enumerable.Empty<ITSWAttachment>();
			public ZBool QueueForManifesting
			{
				get
				{
					return false;
				}
			}

			public ZBool SendManifest
			{
				get
				{
					return false;
				}
			}
		}

		protected BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				}

				return jobDeclaration;
			}
		}
		BaseJobDeclaration jobDeclaration;

		protected void CreateExportSeaJob()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BSES002932";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_PaymentMethod = MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "SGSIN";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateImportSeaJob(string messageSubType = MessageTypeList.Codes.I10)
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = messageSubType;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FMKU00329847";
			container1.CO_ContainerSize = "40";
			container1.CO_Weight = 7.8m;
			container1.CO_WeightUQ = "T";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateImportSeaJobWithNoCharges()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FMKU00329847";
			container1.CO_ContainerSize = "40";
			container1.CO_Weight = 7.8m;
			container1.CO_WeightUQ = "T";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 0;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateImportAirJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS AIRFREIGHT";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = "I10";
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 150.752m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_GoodsDescription = "NEWS PAPER";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "0810049584";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PCS";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 6, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateCompletionJob()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS AIRFREIGHT";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = "COM";
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.JE_TotalWeight = 150m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "QF108";
			JobDeclaration.JE_DateOfArrival = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_DeclarationReference = "BIS00002309";
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 7, 19);
			JobDeclaration.JE_GoodsDescription = "NEWS PAPER";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "0810049584";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "PCS";
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 6, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected void CreateJobWithMultipleValues()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "ANL SHIPPING";
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_FullName = "SMITH & SONS";
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			supplier2.OH_FullName = "BOUNTY INDUSTRIES";
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.I10;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 15m;
			JobDeclaration.JE_TotalWeightUnit = "T";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_DeclarationReference = "BIS2309MULTI";
			JobDeclaration.JE_ExportDate = ZDateTime.Today.AddDays(-14);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_HouseBill = "HB92027";
			JobDeclaration.JE_MasterBill = "OB293042-24902";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			JobDeclaration.JE_RL_NKFinalDestination = "NZAKL";
			JobDeclaration.JE_RL_NKOrigin = "SGSIN";
			JobDeclaration.JE_RL_NKPortOfArrival = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfLoading = "SGSIN";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 15;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			JobDeclaration.CusContainers.RemoveAndDeleteAll();
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "FMKU00329847";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_ContainerSize = "40";
			container1.CO_Weight = 7.8m;
			container1.CO_WeightUQ = "T";
			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "NKFU00734293";
			container2.CO_FCL_LCL_AIR = "FCL";
			container2.CO_ContainerSize = "40";
			container2.CO_Weight = 8.3m;
			container2.CO_WeightUQ = "T";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 20;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 15;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier1.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 7, 10);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 7200m;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			var invoice2 = JobDeclaration.Invoices.AddNew();
			invoice2.JZ_OH_Supplier = supplier2.PK;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice2.JZ_InvoiceDate = new ZDateTime(2013, 7, 14);
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4201.00.00.01B";
			invoiceLine2.JI_LinePrice = 3500m;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = JobDeclaration.ActiveEntryHeaders[0];
			var entryLine1 = entry.MergedLines[0];
			if (entryLine1.CL_AdValoremTariff == "4201.00.00.01B")
			{
				entry.MergedLines[1].CL_LineNumber = 1;
				entry.MergedLines[0].CL_LineNumber = 2;
				entry.MergedLines.CustomSort();
				var entryInv1 = entry.InvoiceHeaders[0];
				var entryInv2 = entry.InvoiceHeaders[1];
				if (entryInv1.Supplier.OH_FullName == "BOUNTY INDUSTRIES")
				{
					entry.InvoiceHeaders[0] = entryInv2;
					entry.InvoiceHeaders[1] = entryInv1;
				}

				Factory.Save();
			}
		}

		protected void CreateExampleDJob()
		{
			JobDeclaration.JE_MessageType = "EXP";
			JobDeclaration.JE_ApplicationCode = "TSW";
			JobDeclaration.JE_DeclarationReference = "BA00000001";
			JobDeclaration.JE_MessageSubType = MessageTypeList.Codes.E40;
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_TotalWeight = 1500m;
			JobDeclaration.JE_TotalWeightUnit = "KG";
			JobDeclaration.JE_VoyageFlightNo = "227W";
			JobDeclaration.JE_ContainerMode = "FCL";
			JobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			JobDeclaration.JE_ExportDate = new ZDateTime(2013, 2, 28);
			JobDeclaration.JE_GoodsDescription = "CHEMICALS";
			JobDeclaration.JE_GS_NKCusAgent = "JKS";
			JobDeclaration.JE_MasterBill = "123456";
			JobDeclaration.JE_HouseBill = "COS12345678";
			JobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			JobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			JobDeclaration.JE_RL_NKOrigin = "NZAKL";
			JobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			JobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			JobDeclaration.JE_VesselName = "HYOGO MARU";
			JobDeclaration.JE_TotalNoOfPacks = 1;
			JobDeclaration.JE_TotalNoOfPacksPackType = "CNT";
			var houseBill1 = JobDeclaration.Bills[1];
			var houseBill2 = JobDeclaration.Bills.AddNew();
			houseBill2.CU_BillType = "HB";
			houseBill2.CU_BillNum = "XYZ99887766";
			var container1 = JobDeclaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CAXU2968920";
			container1.CO_ContainerSize = "40";
			container1.CO_FCL_LCL_AIR = "FCL";
			container1.CO_Weight = 1500m;
			container1.CO_WeightUQ = "KG";
			var container2 = JobDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "UUXU99203930";
			container2.CO_ContainerSize = "40";
			container2.CO_FCL_LCL_AIR = "FCL";
			container2.CO_Weight = 1500m;
			container2.CO_WeightUQ = "KG";
			var container3 = JobDeclaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "ZZUU9283929";
			container3.CO_ContainerSize = "40";
			container3.CO_FCL_LCL_AIR = "FCL";
			container3.CO_Weight = 1500m;
			container3.CO_WeightUQ = "KG";
			var container4 = JobDeclaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "ZZXU2968920";
			container4.CO_ContainerSize = "40";
			container4.CO_FCL_LCL_AIR = "FCL";
			container4.CO_Weight = 1500m;
			container4.CO_WeightUQ = "KG";
			var packLine1 = JobDeclaration.Packages[0];
			packLine1.CW_PackQty = 14;
			packLine1.CW_PackType = "CT";
			packLine1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			var packLine2 = JobDeclaration.Packages[1];
			packLine2.CW_PackQty = 8;
			packLine2.CW_PackType = "BX";
			packLine2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			var packLine3 = JobDeclaration.Packages.AddNew();
			packLine3.CW_HouseBill = houseBill2.CU_BillNum;
			packLine3.CW_PackQty = 8;
			packLine3.CW_PackType = "BX";
			packLine3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
			var packLine4 = JobDeclaration.Packages.AddNew();
			packLine4.CW_HouseBill = houseBill2.CU_BillNum;
			packLine4.CW_PackQty = 5;
			packLine4.CW_PackType = "BX";
			packLine4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;
			var packingGroup1 = JobDeclaration.PackingGroups[0];
			packingGroup1.CR_CU_HouseBill = houseBill1.PK;
			var packingGroup2 = JobDeclaration.PackingGroups[1];
			packingGroup2.CR_CU_HouseBill = houseBill1.PK;
			var packingGroup3 = JobDeclaration.PackingGroups[2];
			packingGroup3.CR_CU_HouseBill = houseBill2.PK;
			var packingGroup4 = JobDeclaration.PackingGroups[3];
			packingGroup4.CR_CU_HouseBill = houseBill2.PK;
			var invoice = JobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);
			var invoiceLine = JobDeclaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			Factory.Save();
			JobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		#region Interface Wrappers Mocked
		protected class MockCusEntryHeaderWrapper : IImportDeclaration, IExportDeclaration, IGoodsShipment, ITSWSubmitter
		{
			public MockCusEntryHeaderWrapper(CusEntryHeader entryHeader)
			{
				this.entryHeader = entryHeader;
				declaration = entryHeader.Declaration;
			}

			public MockCusEntryHeaderWrapper(CusEntryHeader entryHeader, bool intendedUseDesc)
			{
				this.entryHeader = entryHeader;
				declaration = entryHeader.Declaration;
				this.intendedUseDesc = intendedUseDesc;
			}

			readonly CusEntryHeader entryHeader;
			readonly BaseJobDeclaration declaration;
			readonly bool intendedUseDesc;
			#region IImportDeclaration Implementation
			ZBool IDeclaration.IsSea
			{
				get
				{
					return declaration.IsSea;
				}
			}

			ZBool IDeclaration.IsAir
			{
				get
				{
					return declaration.IsAir;
				}
			}

			ZBool IDeclaration.IsMail
			{
				get
				{
					return declaration.IsPost;
				}
			}

			ZBool IDeclaration.IsContainerised
			{
				get
				{
					return declaration.CusContainers.Count > 0;
				}
			}

			ZBool IDeclaration.IsCompletionEntry
			{
				get
				{
					return declaration.JE_MessageSubType == "COM";
				} //Dummied up for mock
			}

			ZBool IDeclaration.HasContainersOrPallets
			{
				get
				{
					return declaration.CusContainers.Count > 0;
				}
			}

			ZBool IImportDeclaration.IsPeriodicImport
			{
				get
				{
					return false;
				}
			}

			ZString IDeclaration.MessageType
			{
				get
				{
					return declaration.JE_MessageSubType;
				}
			}

			ZString IDeclaration.SenderReferenceNumber
			{
				get
				{
					PopulateCH_BGMReferenceIfNeeded();
					return entryHeader.CH_BGMReference.Replace("/", "");
				}
			}

			public void PopulateCH_BGMReferenceIfNeeded()
			{
				if (entryHeader.CH_BGMReference.IsEmpty && declaration != null)
				{
					string messageReferenceNumber = declaration.JE_DeclarationReference;
					entryHeader.CH_BGMReference = messageReferenceNumber;
				}
			}

			ZString IDeclaration.TSWReferenceNumber
			{
				get
				{
					return "734929209";
				}
			}

			IAdditionalInformation IDeclaration.AdditionalInformation
			{
				get
				{
					return null;
				} // AdditionalInformation; }
			}

			IEnumerable<IOtherInfo> IDeclaration.OtherInfoCodes
			{
				get
				{
					yield return new OtherInfo("PDO", ZString.Empty);
				}
			}

			IEnumerable<ZString> IDeclaration.Permits
			{
				get
				{
					yield return "POD,123456";
				}
			}

			IEnumerable<IOtherInfo> IDeclaration.OtherReferencedDocuments
			{
				get
				{
					yield return new OtherInfo("CER", "Test Certificate");
				}
			}

			ZString IDeclaration.HandlingInformation
			{
				get
				{
					return GoodsHandling;
				}
			}

			ZString IDeclaration.MPIAccountDetails
			{
				get
				{
					return ZString.Empty;
				}
			}

			ZInt IDeclaration.TransactionType
			{
				get
				{
					return 9;
				}
			}

			ZDecimal IDeclaration.TotalGrossWeightInKGM
			{
				get
				{
					return declaration.GrossWeight.InKilogramsSafe;
				}
			}

			ZString IDeclaration.TotalGrossWeightUnit
			{
				get
				{
					return "KGM";
				}
			}

			ZDateTime IImportDeclaration.DateOfImport
			{
				get
				{
					return declaration.JE_DateOfArrival;
				}
			}

			ZDateTime IImportDeclaration.ImportPeriod
			{
				get
				{
					return declaration.JE_EntryAuthorisationDate;
				}
			}

			ZString IDeclaration.BrokerCode
			{
				get
				{
					return "0092178J";
				}
			}

			ZString IDeclaration.PremiseID
			{
				get
				{
					return declaration.IsAir ? "25001" : "";
				}
			}

			ZString IDeclaration.CraftName
			{
				get
				{
					return declaration.JE_VesselName;
				}
			}

			ZString IDeclaration.LloydsNo
			{
				get
				{
					var result = ZString.Empty;
					if (declaration.IsSea && !declaration.JE_VesselName.IsEmpty)
					{
						var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, declaration.JE_VesselName);
						var vessel = declaration.Factory.LoadTop1<RefVessel>(vesselQuery);
						if (vessel != null)
						{
							result = vessel.RV_LloydsNumber;
						}

						if (result.IsEmpty)
						{
							result = declaration.JE_LloydsIMO;
						}
					}

					return result;
				}
			}

			ZString IDeclaration.VoyageNo
			{
				get
				{
					return declaration.JE_VoyageFlightNo.Left(8);
				}
			}

			ZString IDeclaration.FlightNo
			{
				get
				{
					return declaration.JE_VoyageFlightNo;
				}
			}

			ZDateTime IDeclaration.DepartureDate
			{
				get
				{
					return declaration.JE_ExportDate;
				}
			}

			IOrganisationSimple IDeclaration.Carrier
			{
				get
				{
					return OrgHeaderWrapper.New(declaration.ShippingLine);
				}
			}

			IEnumerable<ICurrency> IDeclaration.ExchangeRates
			{
				get
				{
					var currencyList = new List<ZString>();
					foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
					{
						string indicatorCode = "F";
						if (!currencyList.Contains(invoice.JZ_RX_NKInvoice_Currency))
						{
							currencyList.Add(invoice.JZ_RX_NKInvoice_Currency);
							yield return new Currency(invoice.JZ_RX_NKInvoice_Currency, invoice.JZ_InvoiceCurrExRate, indicatorCode);
						}
					}
				}
			}

			IDeclarant IDeclaration.Declarant
			{
				get
				{
					return new TSWGlbStaffWrapper(GlbStaff.CurrentUser);
				}
			}

			ZString IDeclaration.PaymentType
			{
				get
				{
					var result = ZString.Empty;
					switch (declaration.JE_PaymentMethod)
					{
						case MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker:
						case MasterFiles.Business.PaymentMethodList.Codes.CashPaidByClient:
							result = "C";
							break;
						case MasterFiles.Business.PaymentMethodList.Codes.BrokerDeferred:
							result = "B";
							break;
						case MasterFiles.Business.PaymentMethodList.Codes.ClientDeferred:
							result = "D";
							break;
					}

					return result;
				}
			}

			IEnumerable<IDutyTaxFee> IDeclaration.DutyTaxFees
			{
				get
				{
					if (entryHeader.TotalDutyAmount > 0)
					{
						var dutyTaxFee = new DutyTaxFee(entryHeader.TotalDutyAmount, DutyTaxFeeTypeList.Codes.CUD, Core.Constants.CurrencyCodes.NewZealand);
						yield return dutyTaxFee;
					}

					if (entryHeader.GSTAmount > 0)
					{
						var dutyTaxFee = new DutyTaxFee(entryHeader.GSTAmount, DutyTaxFeeTypeList.Codes.GST, Core.Constants.CurrencyCodes.NewZealand);
						yield return dutyTaxFee;
					}

					if (entryHeader.TotalAmountPayable > 0)
					{
						var dutyTaxFee = new DutyTaxFee(entryHeader.TotalAmountPayable, DutyTaxFeeTypeList.Codes.TOT, Core.Constants.CurrencyCodes.NewZealand);
						yield return dutyTaxFee;
					}
				}
			}

			ZBool IImportDeclaration.IsMiscImporter
			{
				get
				{
					return IsMiscellaneousImporter;
				}
			}

			Integration.IJobDocAddress IImportDeclaration.MiscImporterAddress
			{
				get
				{
					return IsMiscellaneousImporter && declaration.Shipment != null && declaration.Shipment.ConsigneeDocumentaryAddress != null ? declaration.Shipment.ConsigneeDocumentaryAddress : null;
				}
			}

			protected ZBool IsMiscellaneousImporter
			{
				get
				{
					return declaration.Importer != null && declaration.Importer.OH_FullName == "MISC";
				} // Dummied for Mock
			}

			IOrganisation IImportDeclaration.Importer
			{
				get
				{
					return OrgHeaderWrapper.New(declaration.Importer);
				}
			}

			IEnumerable<IMasterBillTransportDocument> IDeclaration.MasterBills
			{
				get
				{
					foreach (Bill billDetails in declaration.Bills)
					{
						if (billDetails.CU_BillType == Customs.Business.BillTypeList.Codes.MasterBill)
						{
							yield return new MockBillNumberWrapper(billDetails);
						}
					}
				}
			}

			IEnumerable<IAssociatedTransportDocument> IDeclaration.AllBills
			{
				get
				{
					foreach (Bill billDetails in declaration.LowestBills)
					{
						yield return new MockBillNumberWrapper(billDetails);
					}
				}
			}

			IEnumerable<ITransportEquipment> IDeclaration.Equipment
			{
				get
				{
					foreach (BasePackingGroup packGroup in declaration.PackingGroups)
					{
						if (packGroup.Container != null)
						{
							yield return new MockContainerWrapper(packGroup);
						}
					}
				}
			}

			IEnumerable<IPackaging> IDeclaration.Packaging
			{
				get
				{
					foreach (BasePackage package in declaration.Packages)
					{
						ZString houseBill = package.Bill != null ? package.Bill.CU_HouseBill : ZString.Empty;
						var packaging = new Packaging(package, houseBill);
						yield return packaging;
					}
				}
			}

			ZString IDeclaration.PreviousDocumentNo
			{
				get
				{
					return "76423920";
				} // Dummied up for Mock
			}

			ZString IDeclaration.PreviousDocumentType
			{
				get
				{
					return PreviousDocumentTypeList.Codes.I51;
				}
			}

			IGoodsShipment IImportDeclaration.GoodsShipment
			{
				get
				{
					return this;
				}
			}

			#endregion
			#region IGoodsShipment Implementation
			ZString IGoodsShipment.ShipmentOrigin
			{
				get
				{
					return declaration.JE_RL_NKOrigin.SubstringSafe(0, 2);
				}
			}

			ZString IGoodsShipment.NatureOfTransaction
			{
				get
				{
					return "10";
				}
			}

			ZBool IGoodsShipment.MAFContainerDeclaration
			{
				get
				{
					return declaration.IsContainerised;
				} // Dummied for Mock
			}

			IEnumerable<ZString> IGoodsShipment.MAFContainerStatements
			{
				get
				{
					yield return declaration.IsContainerised ? "YYNNNN" : ""; // Dummied for Mock
				}
			}

			IEnumerable<ZString> IGoodsShipment.MPIApprovedSystemNumbers
			{
				get
				{
					yield return ZString.Empty;
				}
			}

			ZString IGoodsShipment.LocationOfGoods
			{
				get
				{
					var result = ZString.Empty;
					if (declaration.WarehouseAddress != null)
					{
						result = declaration.WarehouseAddress.LocalControlledPremisesID;
					}
					else if (declaration.JE_RL_NKPortOfArrival == "NZWSZ")
					{
						result = declaration.JE_RL_NKPortOfArrival; // mocked for test case
					}

					return result;
				}
			}

			ZString IGoodsShipment.PortOfLoading
			{
				get
				{
					return declaration.JE_RL_NKPortOfLoading;
				}
			}

			ZString IGoodsShipment.PortOfDischarge
			{
				get
				{
					return declaration.JE_RL_NKPortOfArrival;
				}
			}

			ZDecimal IGoodsShipment.FreightCostsInNZD
			{
				get
				{
					var result = ZDecimal.Zero;
					return result;
				}
			}

			ZString IGoodsShipment.FreightApportionmentMethod
			{
				get
				{
					return ZString.Empty;
				}
			}

			IOrganisation IGoodsShipment.DeliverToParty
			{
				get
				{
					return OrgHeaderWrapper.New(declaration.ImporterDeliveryAddress.Organisation, declaration.ImporterDeliveryAddress);
				}
				/*
						in reality this value is coming from Declaration.DeliveryDestinationPartyDocAddress,
						but that field is not in the BaseJobDeclaration and cannot be mocked here.
						Use of ImporterDeliveryAddress for testing purposes only.
					*/
			}

			IEnumerable<IGoodsItems> IGoodsShipment.Items
			{
				get
				{
					foreach (CusEntryLine entryLine in entryHeader.MergedLines)
					{
						yield return new MockCusEntryLineWrapper(entryLine, intendedUseDesc);
					}
				}
			}

			IEnumerable<IInvoice> IGoodsShipment.Invoices
			{
				get
				{
					foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
					{
						yield return new InvoiceHeader(invoice.JZ_InvoiceNumber, invoice.IncoTerm, invoice.JZ_InvoiceDate);
					}
				}
			}

			IEnumerable<IOrganisationSimple> IGoodsShipment.NotifyParties
			{
				get
				{
					foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
					{
						yield return OrgHeaderWrapper.New(declaration.NotifyPartyDocumentaryAddress.Organisation, declaration.NotifyPartyDocumentaryAddress);
					}
				}
			}

			IEnumerable<ZString> IGoodsShipment.NotifyPartyCodes => Enumerable.Empty<ZString>();
			IEnumerable<IOrganisation> IGoodsShipment.Sellers
			{
				get
				{
					foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
					{
						yield return OrgHeaderWrapper.New(invoice.Supplier);
					}
				}
			}

			IEnumerable<IOrganisation> IGoodsShipment.StuffingEstablishments
			{
				get
				{
					if (declaration.JE_DeclarationReference == "BIS2309MULTI") // mocked for unit testing
					{
						foreach (BaseCusContainer container in entryHeader.Declaration.CusContainers)
						{
							yield return OrgAddressWrapper.New(entryHeader.Factory.New<OrgAddress>());
						}
					}
				}
			}

			ZString IGoodsShipment.CustomsControlledArea
			{
				get
				{
					var result = ZString.Empty;
					var ccaOrg = declaration.WarehouseDocAddress.Organisation;
					if (ccaOrg != null)
					{
						result = GetCustomsCode(ccaOrg, OrgCusCode.CodeTypes.ControlledPremisesID);
					}

					return result;
				}
			}

			ZString GetCustomsCode(OrgHeader organisation, params ZString[] codeTypes)
			{
				return organisation == null ? ZString.Empty : organisation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.NewZealand, codeTypes);
			}

			IEnumerable<IOrganisation> IGoodsShipment.Suppliers
			{
				get
				{
					foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
					{
						yield return OrgHeaderWrapper.New(invoice.Supplier);
					}
				}
			}

			#endregion
			#region ITSWSubmitter Implementation
			ZString ITSWSubmitter.SubmitterCode
			{
				get
				{
					return "00009908C";
				}
			}

			#endregion
			#region DutyTaxFee
			class DutyTaxFee : IDutyTaxFee
			{
				public DutyTaxFee(ZDecimal value, ZString typeCode, ZString currencyCode)
				{
					this.value = value;
					this.typeCode = typeCode;
					this.currencyCode = currencyCode;
				}

				readonly ZDecimal value;
				readonly ZString typeCode;
				readonly ZString currencyCode;
				public ZDecimal Amount
				{
					get
					{
						return value.Round(2);
					}
				}

				public ZString DutyTaxFeeType
				{
					get
					{
						return typeCode;
					}
				}

				public ZString CurrencyCode
				{
					get
					{
						return currencyCode;
					}
				}
			}

			#endregion
			#region Currency
			class Currency : ICurrency
			{
				public Currency(ZString code, ZDecimal rate, ZString indicator)
				{
					this.code = code;
					this.rate = rate;
					this.indicator = indicator;
				}

				readonly ZString code;
				readonly ZDecimal rate;
				readonly ZString indicator;
				public ZString CurrencyCode
				{
					get
					{
						return code;
					}
				}

				public ZDecimal ExchangeRate
				{
					get
					{
						return rate;
					}
				}

				public ZString ExchangeRateIndicator
				{
					get
					{
						return indicator;
					}
				}
			}

			#endregion
			#region InvoiceHeader
			class InvoiceHeader : IInvoice
			{
				public InvoiceHeader(ZString invNumber, ZString incoTerm, ZDateTime invoiceDate)
				{
					this.invNumber = invNumber;
					this.incoTerm = incoTerm;
					this.invoiceDate = invoiceDate;
				}

				readonly ZString invNumber;
				readonly ZString incoTerm;
				readonly ZDateTime invoiceDate;
				public ZString InvoiceNumber
				{
					get
					{
						return invNumber;
					}
				}

				public ZString IncoTerms
				{
					get
					{
						return incoTerm;
					}
				}

				public ZDateTime InvoiceDate
				{
					get
					{
						return invoiceDate;
					}
				}
			}

			#endregion
			#region OtherInfo
			class OtherInfo : IOtherInfo
			{
				public OtherInfo(ZString code, ZString data)
				{
					this.code = code;
					this.data = data;
				}

				readonly ZString code;
				readonly ZString data;
				public ZString Code
				{
					get
					{
						return code;
					}
				}

				public ZString Data
				{
					get
					{
						return data;
					}
				}
			}

			#endregion
			#region Goods Handling
			public ZString GoodsHandling
			{
				get
				{
					StmNote goodsHandlingNote = GetGoodsHandlingNote();
					return goodsHandlingNote != null ? goodsHandlingNote.ST_NoteText.Replace("\r\n", " ") : ZString.Empty;
				}
			}

			protected StmNote GetGoodsHandlingNote()
			{
				StmNote[] handlingInstructionsNotes = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
				return handlingInstructionsNotes.Length > 0 ? handlingInstructionsNotes[0] : null;
			}

			#endregion
			IOrganisation IExportDeclaration.Exporter
			{
				get
				{
					return OrgHeaderWrapper.New(declaration.Supplier);
				}
			}

			ZDateTime IExportDeclaration.DateOfExport
			{
				get
				{
					return declaration.JE_ExportDate;
				}
			}

			IGoodsShipment IExportDeclaration.GoodsShipment
			{
				get
				{
					return this;
				}
			}

			IOrganisation IExportDeclaration.Importer
			{
				get
				{
					return OrgHeaderWrapper.New(declaration.Importer);
				}
			}
		}

		class MockBillNumberWrapper : IMasterBillTransportDocument
		{
			public MockBillNumberWrapper(Bill bill)
			{
				this.bill = bill;
			}

			readonly Bill bill;
			public IEnumerable<ZGuid> ChildBills
			{
				get
				{
					foreach (Bill childBill in (ChildBillCollection<Bill, BaseJobDeclaration>)bill.ChildBills)
					{
						yield return childBill.PK;
					}
				}
			}

			public ZString BillNumber
			{
				get
				{
					return bill.CU_BillNum;
				}
			}

			public ZString BillType
			{
				///	BM = Bill of Lading
				///	MB = Master Bill
				///	HWB = House Way Bill
				///	ABU = Parcel Number
				get
				{
					var result = bill.CU_BillType;
					switch (bill.CU_BillType)
					{
						case Customs.Business.BillTypeList.Codes.MasterBill:
							result = Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.MB;
							break;
						case Customs.Business.BillTypeList.Codes.HouseBill:
							result = bill.Declaration.IsPost ? Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.ABU : bill.Declaration.IsSea ? Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.BM : Enterprise.Customs.NZ.TradeSingleWindow.BillTypeList.Codes.HWB;
							break;
					}

					return result;
				}
			}

			public IEnumerable<ZGuid> RelatedPackages
			{
				get
				{
					foreach (BasePackingGroup packGroup in bill.PackingGroups)
					{
						if (packGroup.Container == null)
						{
							foreach (BasePackage package in packGroup.Packages)
							{
								if (package.Bill != null)
								{
									yield return package.PK;
								}
							}
						}
					}
				}
			}

			public IEnumerable<ZGuid> RelatedEquipment
			{
				get
				{
					foreach (BasePackingGroup packGroup in bill.PackingGroups)
					{
						if (packGroup.Container != null)
						{
							yield return packGroup.PK;
						}
					}
				}
			}

			public ZInt MessageSequence
			{
				get
				{
					return fMessageSequence;
				}

				set
				{
					fMessageSequence = value;
				}
			}
			ZInt fMessageSequence;

			public ZGuid PK
			{
				get
				{
					return bill.PK;
				}
			}
		}

		class MockContainerWrapper : ITransportEquipment
		{
			public MockContainerWrapper(BasePackingGroup packGroup)
			{
				this.packGroup = packGroup;
				this.container = packGroup.Container;
			}

			readonly BasePackingGroup packGroup;
			readonly BaseCusContainer container;
			#region ITransportEquipment Methods
			public ZString ContainerNumber
			{
				get
				{
					return container.CO_ContainerNumber;
				}
			}

			public ZString ContainerMode
			{
				get
				{
					return BaseCusContainer.ContainerModes.FullContainerLoad;
				}
			}

			public ZString Status
			{
				get
				{
					var result = ZString.Empty;
					if (container.CO_FCL_LCL_AIR == "FCL")
					{
						result = ContainerStatusList.Codes.C5;
					}
					else if (container.CO_FCL_LCL_AIR == "LCL")
					{
						result = ContainerStatusList.Codes.C7;
					}
					else if (container.CO_FCL_LCL_AIR == "BLK")
					{
						result = ContainerStatusList.Codes.C8;
					}

					if (container.CO_FCL_LCL_AIR == "EMT")
					{
						result = ContainerStatusList.Codes.C4;
					}

					return result;
				}
			}

			public ZString Size
			{
				get
				{
					return container.CO_ContainerSize;
				}
			}

			public IEnumerable<ZString> SealNumbers
			{
				get
				{
					if (!container.CO_Seal.IsEmpty)
					{
						yield return container.CO_Seal;
					}

					if (!container.CO_SecondSeal.IsEmpty)
					{
						yield return container.CO_SecondSeal;
					}
				}
			}

			public ZString SealingParty
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZString AttachedEquipmentCode
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZString StowPosition
			{
				get
				{
					return container.JobContainer != null && !container.JobContainer.JC_StowagePosition.IsEmpty ? container.JobContainer.JC_StowagePosition.PadLeft(7, '0') : ZString.Empty;
				}
			}

			ZBool ITransportEquipment.IsPallet
			{
				get
				{
					return ContainerNumberIsValidPalletNumber();
				}
			}

			public ZInt MessageSequence
			{
				get
				{
					return fMessageSequence;
				}

				set
				{
					fMessageSequence = value;
				}
			}
			ZInt fMessageSequence;

			public IEnumerable<ZGuid> RelatedPackages
			{
				get
				{
					foreach (BasePackage package in packGroup.Packages)
					{
						yield return package.PK;
					}
				}
			}

			ZGuid ITransportEquipment.StuffingLocation
			{
				get
				{
					return ZGuid.Empty;
				}
			}

			public ZGuid PK
			{
				get
				{
					return packGroup.PK;
				}
			}

			#region ContainerNumberIsValidPalletNumber
			public bool ContainerNumberIsValidPalletNumber()
			{
				var containerNo = container.CO_ContainerNumber;
				return containerNo.SubstringSafe(0, 1) == "P" && ((containerNo.Length == 2 && containerNo.SubstringSafe(1, 1).KeepChars("1234567890").Length == 1) || (containerNo.Length == 3 && containerNo.SubstringSafe(1, 2).KeepChars("1234567890").Length == 2));
			}
			#endregion
			#endregion
		}

		#region Packaging
		internal class Packaging : IPackaging
		{
			public Packaging(BasePackage package, ZString billNo)
			{
				this.package = package;
				this.billNo = billNo;
			}

			readonly BasePackage package;
			readonly ZString billNo;
			public ZString ShippingMarks
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZInt NumberOfPackages
			{
				get
				{
					return package.CW_PackQty;
				}
			}

			public ZString PackageType
			{
				get
				{
					return package.CW_PackType;
				}
			}

			public ZString PackingMaterialDesc
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZDecimal PackageVolumeInMTQ
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			public ZInt MessageSequence
			{
				get
				{
					return fMessageSequence;
				}

				set
				{
					fMessageSequence = value;
				}
			}

			ZInt fMessageSequence;
			public ZString RelatedHB
			{
				get
				{
					return billNo;
				}
			}

			public ZString RelatedContainer
			{
				get
				{
					return package.CW_ContainerNoOrEquipmentNo;
				}
			}

			public ZGuid PK
			{
				get
				{
					return package.PK;
				}
			}
		}

		#endregion
		class MockCusEntryLineWrapper : IGoodsItems
		{
			public MockCusEntryLineWrapper(CusEntryLine entryLine, bool useIntendedUseDesc)
			{
				this.entryLine = entryLine;
				intendedUseDesc = useIntendedUseDesc;
			}

			readonly CusEntryLine entryLine;
			readonly bool intendedUseDesc;
			#region IGoodsItems
			public ZString VendorIdentifier => "SUPPLIERGSTNO";
			public ZString IsGSTPrePaid => YesNoList.Codes.Yes;
			public ZDecimal ValueForDutyInNZD
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			public ZString TransitionalFacilityCode
			{
				get
				{
					return "7176F";
				} // Dummied for Mock
			}

			public ZString GoodsDescription
			{
				get
				{
					return entryLine.Description;
				}
			}

			public ZString LotNumber
			{
				get
				{
					return "LN000100999";
				} // Dummied for Mock
			}

			public ZDate DateMarking
			{
				get
				{
					return new ZDate(2014, 06, 30);
				} // Dummied for Mock
			}

			public ZBool HasForeignCurrency
			{
				get
				{
					return entryLine.RandomLine.JI_RX_NKLinePriceCurr != Core.Constants.CurrencyCodes.NewZealand;
				}
			}

			public ZDecimal ValueInForeignCurrency
			{
				get
				{
					return HasForeignCurrency ? entryLine.FOB.Amount : ZDecimal.Zero;
				}
			}

			public ZString ForeignCurrencyCode
			{
				get
				{
					return HasForeignCurrency ? entryLine.FOB.Currency.Code : string.Empty;
				}
			}

			public ZString IntendedUse // Dummied for Mock
			{
				get
				{
					if (intendedUseDesc)
					{
						return @"MOVE LOGISTICS
30 HIGHBROOK DRIVE
EAST TAMAKI
AUCKLAND 2013

CONTACT NAME: KARAN SIAN
CONTACT :  090265-4225";
					}
					else
					{
						return IntendedUseCodeList.Codes.HC;
					}
				}
			}

			public ZString IntendedUseCode // Dummied for Mock
			{
				get
				{
					return intendedUseDesc ? "" : "SP";
				}
			}

			public IEnumerable<IClassification> Classifications
			{
				get
				{
					if (!entryLine.CL_AdValoremTariff.IsEmpty)
					{
						yield return new ICRClassification(entryLine.CL_AdValoremTariff, TradeSingleWindow.ClassificationTypeList.Codes.HS);
					}
				}
			}

			public IEnumerable<IConstituent> Constituents => Enumerable.Empty<IConstituent>();
			public ZString PreferenceClaimed
			{
				get
				{
					return PreferenceClaimedList.Codes.NML;
				}
			}

			public IEnumerable<IDutyTaxFee> LineDutyTaxFees
			{
				get
				{
					if (entryLine.DutyAmount > 0)
					{
						var dutyTaxFee = new DutyTaxFee(entryLine.DutyAmount, DutyTaxFeeTypeList.Codes.CUD, Core.Constants.CurrencyCodes.NewZealand);
						yield return dutyTaxFee;
					}

					if (entryLine.GSTVATAmount > 0)
					{
						var dutyTaxFee = new DutyTaxFee(entryLine.GSTVATAmount, DutyTaxFeeTypeList.Codes.GST, Core.Constants.CurrencyCodes.NewZealand);
						yield return dutyTaxFee;
					}
				}
			}

			public IOrganisation Grower
			{
				get
				{
					return null;
				}
			}

			public IEnumerable<ZString> RoutingCountryCodes
			{
				get
				{
					// Dummied for Mock
					if (entryLine.Declaration.Transports.Count > 1)
					{
						var originalLoadPort = entryLine.Declaration.JE_RL_NKPortOfLoading;
						foreach (ITransport leg in entryLine.Declaration.Transports)
						{
							if (leg.JW_RL_NKLoadPort != originalLoadPort)
							{
								yield return leg.JW_RL_NKLoadPort.SubstringSafe(0, 2);
							}
						}
					}
				}
			}

			public IOrganisation Manufacturer
			{
				get
				{
					// Dummied for Mock
					var manufacturer = entryLine.Factory.NewWithValidTestData<OrgHeader>();
					manufacturer.OH_FullName = "PURESTEEL FABRICATORS";
					manufacturer.MainAddress.OA_Address1 = "UNIT 18, 100 MAIN ST.";
					manufacturer.MainAddress.OA_Address2 = "WOLLOOMOOLOO";
					manufacturer.MainAddress.OA_City = "SYDNEY";
					manufacturer.MainAddress.OA_PostCode = "2009";
					manufacturer.MainAddress.OA_State = "NSW";
					return OrgHeaderWrapper.New(manufacturer);
				}
			}

			public IOrganisation Producer
			{
				get
				{
					return null;
				}
			}

			public IEnumerable<IProduct> Products => Enumerable.Empty<IProduct>();
			public ZString BrandName
			{
				get
				{
					return "BONDS";
				} // Dummied for Mock
			}

			public ZString CommonName
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZString RegisteredName
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZString TradeName
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZBool UsedGoods
			{
				get
				{
					return false;
				}
			}

			public ZBool GeneticallyModified
			{
				get
				{
					return false;
				}
			}

			public ZString ExportCountry
			{
				get
				{
					return entryLine.CountryCode;
				}
			}

			public ITemperatureRequirements Temperatures
			{
				get
				{
					return null;
				}
			}

			public IEnumerable<ZString> ContainerNumbers
			{
				get
				{
					if (entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly.Count > 0)
					{
						foreach (NonPersistentCusContainer lineContainer in entryLine.RandomLine.ContainersForInvoiceLinesForBindingOnly)
						{
							if (lineContainer.IsForInvoiceLine)
							{
								yield return lineContainer.ContainerNumber;
							}
						}
					}
				}
			}

			public IOrganisationSimple TreatmentProvider
			{
				get
				{
					// Dummied for Mock
					var treatmentProvider = entryLine.Factory.NewWithValidTestData<OrgHeader>();
					treatmentProvider.OH_FullName = "ABC Fumigators Pty Ltd.";
					return OrgHeaderWrapper.New(treatmentProvider);
				}
			}

			public ZDecimal ItemGrossWeightInKGM
			{
				get
				{
					return entryLine.EffectiveGrossWeight.InKilograms;
				}
			}

			public ZDecimal ItemNetWeightInKGM
			{
				get
				{
					return entryLine.EffectiveNetWeight.InKilograms;
				}
			}

			public ZDecimal StatisticalQty
			{
				get
				{
					if (entryLine.RandomLine.JI_Tariff == "9019.10.09.00B")
					{
						return entryLine.RandomLine.JI_CustomsQuantity;
					}
					else
					{
						return 1500m; // Dummied for Mock
					}
				}
			}

			public ZString StatisticalQtyUnit
			{
				get
				{
					if (entryLine.RandomLine.JI_Tariff == "9019.10.09.00B")
					{
						return entryLine.RandomLine.JI_CustomsUnitQty;
					}
					else
					{
						return "KGM"; // Dummied for Mock
					}
				}
			}

			public ZDecimal SupplementaryQty
			{
				get
				{
					return ZDecimal.Zero;
				}
			}

			public ZString SupplementaryQtyUnit
			{
				get
				{
					return ZString.Empty;
				}
			}

			public ZString OriginCountry
			{
				get
				{
					return entryLine.CountryCode;
				}
			}

			public ZString OriginRegion
			{
				get
				{
					return ZString.Empty;
				}
			}

			public IEnumerable<IPackaging> Packaging
			{
				get
				{
					// Dummied for Mock
					var packaging = new LinePackaging(Core.Constants.ContainerMarking.NoMarks, entryLine.InvoiceQuantity.ToZInt(), entryLine.InvoiceUQ, "STRAW", entryLine.EffectiveVolume.InCubicMetres);
					yield return packaging;
				}
			}

			public IEnumerable<IValuationAdjustment> Adjustments
			{
				get
				{
					// Dummied for Mock
					//151 = Freight
					var adjustment = new ValuationAdjustment(200m, ValuationAdjustmentTypeList.Codes.V151);
					yield return adjustment;
					//150 = Insurance
					adjustment = new ValuationAdjustment(55m, ValuationAdjustmentTypeList.Codes.V150);
					yield return adjustment;
				}
			}

			public IEnumerable<ZString> Permits
			{
				get
				{
					if (entryLine.CL_LineNumber > 2) // Dummied for test cases
					{
						yield return "MEL,123456";
					}
				}
			}

			public IEnumerable<ZString> ProhibitedCodes
			{
				get
				{
					if (entryLine.CL_LineNumber > 2) // Dummied for test cases
					{
						yield return "HWA";
					}
				}
			}

			public IEnumerable<IOtherInfo> OtherInfoCodes
			{
				get
				{
					if (entryLine.CL_LineNumber > 2) // Dummied for test cases
					{
						yield return new OtherInfo("FSA", "12345"); //Dummied for mock
					}
				}
			}

			public ZString RelationshipIndicator
			{
				get
				{
					return "135";
				} // Dummied for Mock
			}

			public ZInt SupplierLineIsRelatedTo
			{
				get
				{
					var result = 1;
					var relatedInvoiceSupplier = entryLine.RandomLine.Supplier;
					var supplierPosition = 0;
					foreach (BaseJobComInvoiceHeader invoice in entryLine.Declaration.Invoices)
					{
						supplierPosition++;
						if (relatedInvoiceSupplier == invoice.Supplier)
						{
							result = supplierPosition;
							break;
						}
					}

					return result;
				}
			}

			public ZBool IsPartsRelated
			{
				get
				{
					return false;
				}
			}

			#endregion
			#region DutyTaxFee
			class DutyTaxFee : IDutyTaxFee
			{
				public DutyTaxFee(ZDecimal value, ZString typeCode, ZString currencyCode)
				{
					this.value = value;
					this.typeCode = typeCode;
					this.currencyCode = currencyCode;
				}

				readonly ZDecimal value;
				readonly ZString typeCode;
				readonly ZString currencyCode;
				public ZDecimal Amount
				{
					get
					{
						return value.Round(2);
					}
				}

				public ZString DutyTaxFeeType
				{
					get
					{
						return typeCode;
					}
				}

				public ZString CurrencyCode
				{
					get
					{
						return currencyCode;
					}
				}
			}

			#endregion
			#region Packaging
			class LinePackaging : IPackaging
			{
				public LinePackaging(ZString marks, ZInt numberOfPackages, ZString packageType, ZString packingMaterial, ZDecimal packageVolume)
				{
					this.shippingMarks = marks;
					this.numberOfPackages = numberOfPackages;
					this.packageType = packageType;
					this.packingMaterialDesc = packingMaterial;
					this.packageVolumeInMTQ = packageVolume;
				}

				readonly ZString shippingMarks;
				readonly ZInt numberOfPackages;
				readonly ZString packageType;
				readonly ZString packingMaterialDesc;
				readonly ZDecimal packageVolumeInMTQ;
				public ZString ShippingMarks
				{
					get
					{
						return shippingMarks;
					}
				}

				public ZInt NumberOfPackages
				{
					get
					{
						return numberOfPackages;
					}
				}

				public ZString PackageType
				{
					get
					{
						return packageType;
					}
				}

				public ZString PackingMaterialDesc
				{
					get
					{
						return packingMaterialDesc;
					}
				}

				public ZDecimal PackageVolumeInMTQ
				{
					get
					{
						return packageVolumeInMTQ;
					}
				}

				public ZInt MessageSequence
				{
					get
					{
						return fMessageSequence;
					}

					set
					{
						fMessageSequence = value;
					}
				}
				ZInt fMessageSequence;

				public ZString RelatedHB
				{
					get
					{
						return ZString.Empty;
					}
				}

				public ZString RelatedContainer
				{
					get
					{
						return ZString.Empty;
					}
				}

				public ZGuid PK
				{
					get
					{
						return ZGuid.Empty;
					}
				}
			}

			#endregion
			#region DutyTaxFee
			class ValuationAdjustment : IValuationAdjustment
			{
				public ValuationAdjustment(ZDecimal amount, ZString code)
				{
					this.amount = amount;
					this.code = code;
				}

				readonly ZDecimal amount;
				readonly ZString code;
				public ZString AdjustmentQualifier
				{
					get
					{
						return code;
					}
				}

				public ZDecimal AdjustmentAmountInNZD
				{
					get
					{
						return amount;
					}
				}
			}

			#endregion
			#region OtherInfo
			class OtherInfo : IOtherInfo
			{
				public OtherInfo(ZString code, ZString data)
				{
					this.code = code;
					this.data = data;
				}

				readonly ZString code;
				readonly ZString data;
				public ZString Code
				{
					get
					{
						return code;
					}
				}

				public ZString Data
				{
					get
					{
						return data;
					}
				}
			}
			#endregion
		}
		#endregion
		#endregion

		protected string GetEmbeddedResourcePath(string filename) => "Enterprise.Customs.NZ.TradeSingleWindow.Testing.MessageBuilders.TestFiles." + filename;
	}
}
