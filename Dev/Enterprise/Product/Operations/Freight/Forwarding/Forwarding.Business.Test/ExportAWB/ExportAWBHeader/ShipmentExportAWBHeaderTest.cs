using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.AWB;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentExportAWBHeader))]
	public class ShipmentExportAWBHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsHarmonizedCodeMissing()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment.PK)) as BusinessObject;
			Factory.Save();

			var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
			consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;
			consignment[HVLVConsignmentSchema.HVC_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

			var item = Factory.New<IHVLVItem>() as BusinessObject;
			item[HVLVItemSchema.HVI_HVC_Consignment] = consignment.PK;
			item[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipment.PK;
			item[HVLVItemSchema.HVI_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];

			var itemLine = Factory.New<IHVLVItemLine>() as BusinessObject;
			itemLine[HVLVItemLineSchema.HVS_HVI_HVLVItem] = item.PK;
			itemLine[HVLVItemLineSchema.HVS_ClusterKey] = header[HVLVConsignmentHeaderSchema.HCH_ClusterKey];
			itemLine[HVLVItemLineSchema.HVS_Quantity] = (ZShort)1;
			itemLine[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode] = "AU";
			Factory.Save();

			var loadPort = "AUSYD";
			AssertEquals(true, shipment.IsHarmonizedCodeMissing(loadPort));

			itemLine[HVLVItemLineSchema.HVS_DestinationTariff] = "12345";
			Factory.Save();
			AssertEquals(false, shipment.IsHarmonizedCodeMissing(loadPort));
		}

		public void TestGetDGCodesFromParentBO()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "MMM";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var shipment = AWBHeader.Shipment;
			var outerPackLine = shipment.OuterPackLines.AddNew();
			var dgLine = outerPackLine.UNDGs.AddNew();
			dgLine.LinkDefault(subs);

			var item = Factory.New<IHVLVItem>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "HHH";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var dgLine2 = Factory.New<UNDGDataItem>();
			dgLine2.DI_ParentTableCode = HVLVItemSchema.Constants.Prefix;
			dgLine2.DI_ParentID = item.PK;
			dgLine2.DI_DG = subs2.PK;

			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			var dgCodes = shipment.GetDGCodes();

			CombineAssertions("only get DGCodes from ForwardingPackLine when shipment type is not HVL", () =>
			{
				AssertEquals(1, dgCodes.Count());
				Assert(dgCodes.Contains("UNMMM"));
			});

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			dgCodes = shipment.GetDGCodes();

			CombineAssertions("get DGCodes from both ForwardingPackLine and HVLVItemLine when shipment type is HVL", () =>
			{
				AssertEquals(2, dgCodes.Count());
				Assert(dgCodes.Contains("UNMMM"));
				Assert(dgCodes.Contains("HHH"));
			});
		}

		public void TestDGVariantNotShown() =>
			CheckDGVariant("UN9434");

		void CheckDGVariant(params ZString[] expected)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var undgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
				undgSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				undgSubstance.DG_Class = "7";
				undgSubstance.DG_UNNO = "9434";
				undgSubstance.DG_Variant = "A";

				var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
				dangerousGood.DI_DG = undgSubstance.PK;
				dangerousGood.DI_PackageCount = 1;
				dangerousGood.DI_F3_NKPackType = "BOX";

				var dangerousShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				dangerousShipment.JS_UniqueConsignRef = "DangerousShipment1";
				dangerousShipment.OuterPackLines.AddNew().UNDGs.Add(dangerousGood);

				var header = Factory.New<ShipmentExportAWBHeader>();
				header.EH_ParentID = dangerousShipment.PK;

				header.Populate();

				var array = header.DGCodes.ToArray();
				Array.Sort(array);
				Array.Sort(expected);
				AssertArrayEqualsByElements("DG Codes", expected, array);
			}
		}

		#region Loader

		public void TestLoadOrCreate()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => ShipmentExportAWBHeader.LoadOrCreate(null));

			var shipment1 = Factory.New<ForwardingShipment>();
			var header1 = Factory.New<ShipmentExportAWBHeader>();
			header1.EH_ParentID = shipment1.PK;
			header1.EH_Table = JobShipmentSchema.Constants.TableName;

			AssertEquals(header1, ShipmentExportAWBHeader.LoadOrCreate(shipment1));

			var shipment2 = Factory.New<ForwardingShipment>();
			var header2 = ShipmentExportAWBHeader.LoadOrCreate(shipment2);

			AssertEquals(true, typeof(ShipmentExportAWBHeader).IsAssignableFrom(header2.GetType()));
			AssertEquals(shipment2.PK, header2.EH_ParentID);
			AssertEquals(JobShipmentSchema.Constants.TableName, header2.EH_Table);

			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_OverrideWaybillDefaults = true;

			AssertEquals("Precondition", true, header2.IsSavedByFactory);
			Factory.Save();

			var shipment2InAnotherFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment2.PK);
			AssertEquals(header2.PK, ShipmentExportAWBHeader.LoadOrCreate(shipment2InAnotherFactory).PK);
		}

		#endregion

		#region Extra Customizable Text

		public void TestMultilineExtraAccountionInfoMacrosSplitToMultipleLines()
		{
			SetUpForDeparturePort("AUSYD");

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "CARGOWISE";
			org.MainAddress.OA_Address1 = "72";
			org.MainAddress.OA_Address2 = "O'RIORDAN";

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			ExportAWBHeader awb = shipment.AWBHeader;

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList
			{
				new CodeDescriptionPair("CNN", "FIRST DESCRIPTION"),
				new CodeDescriptionPair("BBC", "<DocShipment.BillOfLading.NotifyParty>"),
				new CodeDescriptionPair("ABC", "LAST DESCRIPTION")
			});
			awb.Populate();

			ZGuid pk = awb.AWBAccountingInformations[0].PK;

			AssertEquals(6, awb.AWBAccountingInformations.Count);
			AssertEquals("CNN", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("FIRST DESCRIPTION", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("CARGOWISE", awb.AWBAccountingInformations[1].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[2].EA_InformationID);
			AssertEquals("72", awb.AWBAccountingInformations[2].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[3].EA_InformationID);
			AssertEquals("O'RIORDAN", awb.AWBAccountingInformations[3].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[4].EA_InformationID);
			AssertEquals("AUSTRALIA", awb.AWBAccountingInformations[4].EA_Information);
			AssertEquals("ABC", awb.AWBAccountingInformations[5].EA_InformationID);
			AssertEquals("LAST DESCRIPTION", awb.AWBAccountingInformations[5].EA_Information);

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList
			{
				new CodeDescriptionPair("CNN", "FIRST DESCRIPTION"),
				new CodeDescriptionPair("CBS", ""),
				new CodeDescriptionPair("BBC", "<DocShipment.BillOfLading.NotifyParty>"),
				new CodeDescriptionPair("ABC", "LAST DESCRIPTION")
			});
			awb.Populate();

			AssertEquals(7, awb.AWBAccountingInformations.Count);
			AssertEquals("First accounting info has not changed and should be reused", pk, awb.AWBAccountingInformations[0].PK);
			AssertEquals("CNN", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("FIRST DESCRIPTION", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("CBS", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("", awb.AWBAccountingInformations[1].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[2].EA_InformationID);
			AssertEquals("CARGOWISE", awb.AWBAccountingInformations[2].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[3].EA_InformationID);
			AssertEquals("72", awb.AWBAccountingInformations[3].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[4].EA_InformationID);
			AssertEquals("O'RIORDAN", awb.AWBAccountingInformations[4].EA_Information);
			AssertEquals("BBC", awb.AWBAccountingInformations[5].EA_InformationID);
			AssertEquals("AUSTRALIA", awb.AWBAccountingInformations[5].EA_Information);
			AssertEquals("ABC", awb.AWBAccountingInformations[6].EA_InformationID);
			AssertEquals("LAST DESCRIPTION", awb.AWBAccountingInformations[6].EA_Information);
		}

		public void TestDocumentContextIsInitialisedDuringMacroReplacement()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.Consols.AddNew();
			shipment.Consols[0].JK_UniqueConsignRef = "C000065432";

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "<DocShipment.Consol.ConsolNumber>") });
			ExportAWBHeader awb = shipment.AWBHeader;
			awb.Populate();

			AssertEquals("C000065432", awb.AWBAccountingInformations[0].EA_Information);
		}

		public void TestEmptyExtraTextLinesAreExcluded()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Real Handling Info");
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_INCO = "CIF";

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "<AirportOfDestinationCode>"), new CodeDescriptionPair("bla", "some information") });

			ExportAWBHeader awb = shipment.AWBHeader;
			awb.Populate();
			AssertEquals(2, awb.AWBAccountingInformations.Count);
			AssertEquals((ZByte)1, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("BOM", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals((ZByte)2, awb.AWBAccountingInformations[1].EA_Sequence);
			AssertEquals("bla", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("some information", awb.AWBAccountingInformations[1].EA_Information);

			shipment.JS_RL_NKDestination = ZString.Empty;

			awb.Populate();
			AssertEquals(1, awb.AWBAccountingInformations.Count);
			AssertEquals("bla", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals((ZByte)2, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("some information", awb.AWBAccountingInformations[0].EA_Information);
		}

		public void TestExtraCustomizableText()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Real Handling Info");
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_INCO = "CIF";

			ExportAWBHeader awb = shipment.AWBHeader;
			awb.Populate();
			Assert(!awb.HasChanges);
			AssertEquals("No Accounting Infos", 0, awb.AWBAccountingInformations.Count);
			AssertEquals("Default nature and qty of goods", "No Dimensions Available\n\n\n\n\n\n\n\n\n\n\n", awb.NatureAndQtyOfGoods);
			AssertEquals("No optional info", "TERMS: CIF", awb.EH_OptionalShippingInformation);
			AssertEquals("No optional info", "", awb.EH_OptionalShippingInformation2);
			AssertEquals("Handling info from note only", "Real Handling Info", awb.EH_HandlingInformation);

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("SRN", "<AirportOfDestinationCode>"), new CodeDescriptionPair("bla", "some information") });
			FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Handling <AirportOfDestinationText>");
			FreightDataRegistry.Instance.HAWBNatureAndQtyOfGoodsExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Nature <INCO>");
			FreightDataRegistry.Instance.HAWBOptionalShippingInfoOneExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1<CompanyCountry>");
			FreightDataRegistry.Instance.HAWBOptionalShippingInfoTwoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2<CompanyCode>");

			awb.Populate();
			Assert(!awb.HasChanges);
			AssertEquals("1 Accounting Infos", 2, awb.AWBAccountingInformations.Count);
			AssertEquals("Accounting Info 1", (ZByte)1, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("Accounting Info 1", "SRN", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("Accounting Info 1", "BOM", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("Accounting Info 2", (ZByte)2, awb.AWBAccountingInformations[1].EA_Sequence);
			AssertEquals("Accounting Info 2", "bla", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("Accounting Info 2", "some information", awb.AWBAccountingInformations[1].EA_Information);
			AssertEquals("Nature and qty of goods", "Nature CIF\nNo Dimensions Available\n\n\n\n\n\n\n\n\n\n", awb.NatureAndQtyOfGoods);
			AssertEquals("Optional info 1", "1" + GlbCompany.CurrentCompany.Country.RN_Desc, awb.EH_OptionalShippingInformation);
			AssertEquals("Optional info 2", "2" + GlbCompany.CurrentCompany.GC_Code, awb.EH_OptionalShippingInformation2);
			AssertEquals("Handling info", "Real Handling Info\r\nHandling MUMBAI (EX BOMBAY)", awb.EH_HandlingInformation);

			FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("SRN", "<AirportOfDestinationCode>"), new CodeDescriptionPair("bbb", "new info") });
			awb.Populate();
			Assert(!awb.HasChanges);
			AssertEquals("1 Accounting Infos", 2, awb.AWBAccountingInformations.Count);
			AssertEquals("Accounting Info 1", (ZByte)1, awb.AWBAccountingInformations[0].EA_Sequence);
			AssertEquals("Accounting Info 1", "SRN", awb.AWBAccountingInformations[0].EA_InformationID);
			AssertEquals("Accounting Info 1", "BOM", awb.AWBAccountingInformations[0].EA_Information);
			AssertEquals("Old info needs to be rewritten here", (ZByte)2, awb.AWBAccountingInformations[1].EA_Sequence);
			AssertEquals("Old info needs to be rewritten here", "bbb", awb.AWBAccountingInformations[1].EA_InformationID);
			AssertEquals("Old info needs to be rewritten here", "new info", awb.AWBAccountingInformations[1].EA_Information);
		}

		public void TestExtraCustomizableText_AccountingInfoExtraText_VariableValues_DoesNotTriggerHasChanges()
		{
			Test_AccountingInfoExtraText_VariableValues("<DateTimeStart>");
			Test_AccountingInfoExtraText_VariableValues("TestWithSpaces  ");

			void Test_AccountingInfoExtraText_VariableValues(string accountingInfoExtraText)
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "USLAX";

				FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					new CodeDescriptionPairList { new CodeDescriptionPair("", accountingInfoExtraText) });
				ExportAWBHeader awb = shipment.AWBHeader;
				awb.Populate();
				Assert(!awb.HasChanges);

				awb.EH_AreRateLinesOverridden = true;
				awb.Populate();
				Assert(awb.HasChanges);

				Factory.Save();
				Assert(!awb.HasChanges);

				awb.Populate();
				Assert(!awb.HasChanges);
				AssertEquals("1 Accounting Infos", 1, awb.AWBAccountingInformations.Count);

				FreightDataRegistry.Instance.HAWBAccountingInfoExtraText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionPairList { new CodeDescriptionPair("", "Test") });
				awb.Populate();
				Assert(!awb.HasChanges);
				AssertEquals("1 Accounting Infos", 1, awb.AWBAccountingInformations.Count);
			}
		}

		#endregion

		#region Currency

		[DisableZeroExchangeRateOverriding]
		public void TestGetAmountInHAWBCurrency()
		{
			SetUpForDeparturePort("AUSYD");

			RefCurrency audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			AssertNoExceptionThrown(() =>
				{
					AssertNull("Precondition: provider of currency converter", AWBHeader.InvoiceJobHeader);
					AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), ZGuid.Empty, CostSell.Revenue);
				});

			Func<string, decimal, RefExchangeRate> createExchangeRate = (currencyCode, sellRate) =>
				{
					RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
					exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
					exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
					exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
					exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					exchangeRate.RE_RX_NKExCurrency = currencyCode;
					exchangeRate.RE_SellRate = sellRate;
					return exchangeRate;
				};

			RefExchangeRate usdExchangeRate = createExchangeRate("USD", 2m);
			RefExchangeRate nzdExchangeRate = createExchangeRate("NZD", 4m);

			Factory.Save();

			JobHeaderWithFakeCurrencyConverter jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;

			AssertEquals("Precondition", jobHeader.PK, AWBHeader.InvoiceJobHeader.PK);
			AssertEquals("Precondition: HAWB currency", "AUD", AWBHeader.EH_Currency);

			RefCurrency usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefCurrency nzdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			RefCurrency mdlCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "MDL");

			AssertEquals(100m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), ZGuid.Empty, CostSell.Revenue));
			AssertEquals(50m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, usdCurrency), ZGuid.Empty, CostSell.Revenue));
			AssertEquals(25m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, nzdCurrency), ZGuid.Empty, CostSell.Revenue));
			AssertEquals(0m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, mdlCurrency), ZGuid.Empty, CostSell.Revenue));
		}

		public void TestCurrency()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			var job = CreateJobHeader(AWBHeader.Shipment.PK, client, debtor);

			var charge1 = AddCharge(job, Factory.New<AccChargeCode>().PK);
			charge1.JR_RX_NKSellCurrency = "UAH";
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = AddCharge(job, Env.Registry.FreightChargeCode);
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = AddCharge(job, Env.Registry.FreightChargeCode);
			charge3.JR_RX_NKSellCurrency = "AFN";
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var invoice1Line = CreateInvoiceAndLine("INR", new ZDateTime(2007, 1, 1));
			var invoice2Line = CreateInvoiceAndLine("EUR", new ZDateTime(2008, 1, 1));

			FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AWBHeader.Populate();
			AssertEquals("Company currency as registry off", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AWBHeader.EH_Currency);

			FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AWBHeader.Populate();
			AssertEquals("If no freight charge found, fallback to local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AWBHeader.EH_Currency);

			charge3.JR_OH_SellAccount = debtor.PK;
			charge3.JR_RX_NKSellCurrency = "AFN";
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AWBHeader.Populate();
			AssertEquals("First freight charge currency", "AFN", AWBHeader.EH_Currency);

			charge2.JR_OH_SellAccount = debtor.PK;
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AWBHeader.Populate();
			AssertEquals("First freight charge currency", "USD", AWBHeader.EH_Currency);

			charge2.JR_AL_ARLine = invoice2Line.PK;
			charge3.JR_AL_ARLine = invoice1Line.PK;
			AWBHeader.Populate();
			AssertEquals("Earliest freight collect invoice currency", "INR", AWBHeader.EH_Currency);

			charge3.JR_OH_SellAccount = client.PK;
			AWBHeader.Populate();
			AssertEquals("Earliest freight collect invoice currency", "EUR", AWBHeader.EH_Currency);

			FreightDataRegistry.Instance.SetHAWBCurrencyToFirstPrepaidInvoiceCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AWBHeader.Populate();
			AssertEquals("Earliest freight prepaid invoice currency", "INR", AWBHeader.EH_Currency);

			charge2.Delete();
			charge3.Delete();
			AWBHeader.Populate();
			AssertEquals("If no freight charge found, fallback to local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AWBHeader.EH_Currency);
		}

		JobHeader CreateJobHeader(ZGuid shipmentPK, OrgHeader client, OrgHeader debtor)
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipmentPK;
			job.JH_JobNum = "Phony number";
			job.JH_OA_AgentCollectAddr = debtor != null ? debtor.MainAddress.PK : ZGuid.Empty;
			job.JH_OA_LocalChargesAddr = client != null ? client.MainAddress.PK : ZGuid.Empty;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return job;
		}

		JobCharge AddCharge(JobHeader job, ZGuid chargeCodePK)
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_GB = job.JH_GB;
			charge.JR_GE = job.JH_GE;

			return charge;
		}

		AccTransactionLines CreateInvoiceAndLine(ZString currency, ZDateTime postDate, string lineType = TransactionLineTypes.Revenue)
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_RX_NKTransactionCurrency = currency;
			invoice.AH_InvoiceDate = postDate;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = invoice.PK;
			line.AL_LineType = lineType;
			line.AL_PostDate = postDate;

			return line;
		}

		#endregion

		#region Currency With Organisation

		[DisableZeroExchangeRateOverriding]
		public void TestGetAmountInHAWBCurrency_WithOrganisation()
		{
			SetUpForDeparturePort("AUSYD");

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			AssertNoExceptionThrown(() =>
			{
				AssertNull("Precondition: provider of currency converter", AWBHeader.InvoiceJobHeader);
				AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), ZGuid.Empty, CostSell.Revenue);
			});

			CreateReferenceExchangeRate("USD", 2m);
			CreateReferenceExchangeRate("NZD", 4m);

			var jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			Factory.Save();

			AssertEquals("Precondition", jobHeader.PK, AWBHeader.InvoiceJobHeader.PK);
			AssertEquals("Precondition: HAWB currency", "AUD", AWBHeader.EH_Currency);

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var nzdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			var mdlCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "MDL");

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			CreateExchangeRate("USD", 2.5, "DEB", debtor.PK, jobHeader.PK);
			CreateExchangeRate("NZD", 5, "DEB", debtor.PK, jobHeader.PK);
			CreateExchangeRate("USD", 4, "CRD", creditor.PK, jobHeader.PK);
			CreateExchangeRate("NZD", 10, "CRD", creditor.PK, jobHeader.PK);

			Factory.Save();

			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			AssertNotNull(freightChargeCode);
			CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 200, 445, debtor, creditor);

			AssertEquals(100m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), debtor.PK, CostSell.Revenue));
			AssertEquals(40m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, usdCurrency), debtor.PK, CostSell.Revenue));
			AssertEquals(20m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, nzdCurrency), debtor.PK, CostSell.Revenue));
			AssertEquals(0m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, mdlCurrency), debtor.PK, CostSell.Revenue));

			AssertEquals(100m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), creditor.PK, CostSell.Cost));
			AssertEquals(25m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, usdCurrency), creditor.PK, CostSell.Cost));
			AssertEquals(10m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, nzdCurrency), creditor.PK, CostSell.Cost));
			AssertEquals(0m, AWBHeader.GetAmountInHAWBCurrency(new Money(100m, mdlCurrency), creditor.PK, CostSell.Cost));
		}

		public void TestSellAmount_CostAmount_TaxAmount_InHAWBCurrency()
		{
			SetUpForDeparturePort("AUSYD");

			var audCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			AssertNoExceptionThrown(() =>
			{
				AssertNull("Precondition: provider of currency converter", AWBHeader.InvoiceJobHeader);
				AWBHeader.GetAmountInHAWBCurrency(new Money(100m, audCurrency), ZGuid.Empty, CostSell.Revenue);
			});

			CreateReferenceExchangeRate("USD", 2m);

			var jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			Factory.Save();

			AssertEquals("Precondition", jobHeader.PK, AWBHeader.InvoiceJobHeader.PK);
			AssertEquals("Precondition: HAWB currency", "AUD", AWBHeader.EH_Currency);

			Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRateNumerator_ForTestOnly(10);
			rate.AT_Type = "RAT";

			CreateExchangeRate("USD", 2.5, "DEB", debtor.PK, jobHeader.PK);
			CreateExchangeRate("NZD", 5, "DEB", debtor.PK, jobHeader.PK);
			CreateExchangeRate("USD", 4, "CRD", creditor.PK, jobHeader.PK);
			CreateExchangeRate("NZD", 10, "CRD", creditor.PK, jobHeader.PK);

			var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
			AssertNotNull(freightChargeCode);
			var invoice1Line = CreateInvoiceAndLine("USD", new ZDateTime(2018, 11, 1), TransactionLineTypes.WIP);
			Factory.Save();

			var charge = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 200, 400, debtor, creditor, rate);
			charge.JR_AL_ARLine = invoice1Line.PK;

			FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(500m, AWBHeader.GetSellAmountInHAWBCurrency(charge));
			AssertEquals(1600m, AWBHeader.GetCostAmountInHAWBCurrency(charge));
			AssertEquals(50m, AWBHeader.GetTaxAmountInHAWBCurrency(charge));
		}

		public void TestTaxTotalsFromFreightCharges()
		{
			var oldRegistryValue = Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax;

			try
			{
				Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;

				using (FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CreateReferenceExchangeRate("USD", 2m);

					var jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
					jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
					Factory.Save();

					var creditor = Factory.NewWithValidTestData<OrgHeader>();
					var debtor = new JobHeader.Loader(AWBHeader.Shipment).TryLoadOrCreate();
					debtor.JH_GE = GlbDepartment.CurrentDepartment.PK;
					debtor.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
					debtor.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

					var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
					rate.SetRateNumerator_ForTestOnly(10);
					rate.AT_Type = "RAT";

					var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
					var invoiceLine = CreateInvoiceAndLine("USD", ZDateTime.Today, TransactionLineTypes.WIP);

					var prepaidCharge1 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 10, 0, debtor.LocalCharges, creditor, rate);
					var prepaidCharge2 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 20, 0, debtor.LocalCharges, creditor, rate);
					var collectCharge1 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 30, 0, debtor.AgentCollect, creditor, rate);
					var collectCharge2 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 40, 0, debtor.AgentCollect, creditor, rate);

					prepaidCharge1.JR_AL_ARLine = invoiceLine.PK;
					prepaidCharge2.JR_AL_ARLine = invoiceLine.PK;
					collectCharge1.JR_AL_ARLine = invoiceLine.PK;
					collectCharge2.JR_AL_ARLine = invoiceLine.PK;

					AssertEquals(prepaidCharge1.JR_Calc_LocalSellTaxAmt, Factory.Load<ICharge>(prepaidCharge1.PK).LocalSellGSTAmt);
					AssertEquals(prepaidCharge2.JR_Calc_LocalSellTaxAmt, Factory.Load<ICharge>(prepaidCharge2.PK).LocalSellGSTAmt);
					AssertEquals(collectCharge1.JR_Calc_LocalSellTaxAmt, Factory.Load<ICharge>(collectCharge1.PK).LocalSellGSTAmt);
					AssertEquals(collectCharge2.JR_Calc_LocalSellTaxAmt, Factory.Load<ICharge>(collectCharge2.PK).LocalSellGSTAmt);

					using (FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						AWBHeader.Populate();

						AssertEquals(2m, AWBHeader.GetTaxAmountInHAWBCurrency(prepaidCharge1));
						AssertEquals(4m, AWBHeader.GetTaxAmountInHAWBCurrency(prepaidCharge2));
						AssertEquals(6m, AWBHeader.GetTaxAmountInHAWBCurrency(collectCharge1));
						AssertEquals(8m, AWBHeader.GetTaxAmountInHAWBCurrency(collectCharge2));

						AssertEquals(6m, AWBHeader.EH_TaxesPPD);
						AssertEquals(14m, AWBHeader.EH_TaxesCOL);
					}
				}
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = oldRegistryValue;
			}
		}

		public void TestTaxAmount_GST()
		{
			var oldRegistryValue = Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax;

			try
			{
				Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;

				using (FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CreateReferenceExchangeRate("USD", 2m);

					var jobHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
					jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
					jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
					Factory.Save();

					var creditor = Factory.NewWithValidTestData<OrgHeader>();
					var debtor = new JobHeader.Loader(AWBHeader.Shipment).TryLoadOrCreate();
					debtor.JH_GE = GlbDepartment.CurrentDepartment.PK;
					debtor.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK;
					debtor.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

					var rateGST = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
					rateGST.SetRate_ForTestOnly(50, 10);
					rateGST.SetExtraRate_ForTestOnly(50, 10);
					rateGST.AT_Type = "RAT";
					rateGST.AT_ExtraTaxRateType = "STA";

					var freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
					var invoiceLine = CreateInvoiceAndLine("USD", ZDateTime.Today, TransactionLineTypes.WIP);

					var prepaidCharge1 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 10, 0, debtor.LocalCharges, creditor, rateGST);
					var prepaidCharge2 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 20, 0, debtor.LocalCharges, creditor, rateGST);
					var collectCharge1 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 30, 0, debtor.AgentCollect, creditor, rateGST);
					var collectCharge2 = CreateShipmentCharge(AWBHeader.Shipment, freightChargeCode, 40, 0, debtor.AgentCollect, creditor, rateGST);

					prepaidCharge1.JR_AL_ARLine = invoiceLine.PK;
					prepaidCharge2.JR_AL_ARLine = invoiceLine.PK;
					collectCharge1.JR_AL_ARLine = invoiceLine.PK;
					collectCharge2.JR_AL_ARLine = invoiceLine.PK;

					AssertEquals("Rate: 5%, extra rate： 5 %, exchange rate: 2. Tax amount should be 10 * 2 * (5+5) / 100 = 2", prepaidCharge1.JR_Calc_LocalSellTaxAmt * 2, Factory.Load<ICharge>(prepaidCharge1.PK).LocalSellGSTAmt);
					AssertEquals("Rate: 5%, extra rate： 5 %, exchange rate: 2. Tax amount should be 20 * 2 * (5+5) / 100 = 4", prepaidCharge2.JR_Calc_LocalSellTaxAmt * 2, Factory.Load<ICharge>(prepaidCharge2.PK).LocalSellGSTAmt);
					AssertEquals("Rate: 5%, extra rate： 5 %, exchange rate: 2. Tax amount should be 30 * 2 * (5+5) / 100 = 6", collectCharge1.JR_Calc_LocalSellTaxAmt * 2, Factory.Load<ICharge>(collectCharge1.PK).LocalSellGSTAmt);
					AssertEquals("Rate: 5%, extra rate： 5 %, exchange rate: 2. Tax amount should be 40 * 2 * (5+5) / 100 = 8", collectCharge2.JR_Calc_LocalSellTaxAmt * 2, Factory.Load<ICharge>(collectCharge2.PK).LocalSellGSTAmt);

					using (FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
					{
						AWBHeader.Populate();

						AssertEquals(2m, AWBHeader.GetTaxAmountInHAWBCurrency(prepaidCharge1));
						AssertEquals(4m, AWBHeader.GetTaxAmountInHAWBCurrency(prepaidCharge2));
						AssertEquals(6m, AWBHeader.GetTaxAmountInHAWBCurrency(collectCharge1));
						AssertEquals(8m, AWBHeader.GetTaxAmountInHAWBCurrency(collectCharge2));

						AssertEquals(6m, AWBHeader.EH_TaxesPPD);
						AssertEquals(14m, AWBHeader.EH_TaxesCOL);
					}
				}
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = oldRegistryValue;
			}
		}

		JobCharge CreateShipmentCharge(ForwardingShipment shipment,
			AccChargeCode chargeCode, ZDecimal sell, ZDecimal cost,
			OrgHeader sellAccount = null,
			OrgHeader costAccount = null,
			AccTaxRate rate = null)
		{
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AC = chargeCode != null ? chargeCode.PK : ZGuid.Empty;
			charge.JR_JH = shipment.Job.PK;
			charge.JR_OH_SellAccount = sellAccount != null ? sellAccount.PK : ZGuid.Empty;
			charge.JR_OH_CostAccount = costAccount != null ? costAccount.PK : ZGuid.Empty;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.Company.GC_IsReciprocal = true;
			charge.JR_LocalSellAmt = sell;
			charge.JR_OSSellAmt = sell;

			if (rate != null)
			{
				charge.JR_AT_SellGSTRate = rate.PK;
			}

			charge.JR_LocalCostAmt = cost;
			charge.JR_OSCostAmt = cost;

			return charge;
		}

		void CreateExchangeRate(string currencyCode, ZDecimal baseRate, string orgType, ZGuid orgHeaderPK, ZGuid jobHeaderPK)
		{
			var exRate = (BusinessObject)Factory.New<IExchangeRate>();
			exRate["JF_RX_NKRateCurrency"] = currencyCode;
			exRate["JF_BaseRate"] = baseRate;
			exRate["JF_OH_Org"] = orgHeaderPK;
			exRate["JF_OrgType"] = orgType;
			exRate["JF_JH"] = jobHeaderPK;
		}

		void CreateReferenceExchangeRate(string currencyCode, ZDecimal sellRate)
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_RX_NKExCurrency = currencyCode;
			exchangeRate.RE_SellRate = sellRate;
		}

		#endregion

		#region Rate Lines

		public void TestRateLineNoPieces()
		{
			AWBHeader.Shipment.JS_OuterPacks = 50;
			AWBHeader.Populate();
			AssertEquals("50", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);

			AWBHeader.Shipment.JS_OuterPacks = 9999;
			AWBHeader.Populate();
			AssertEquals("9999", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);

			AWBHeader.Shipment.JS_OuterPacks = 10000;
			AWBHeader.Populate();
			AssertEquals("9999", AWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
		}

		public void TestRateLineGrossWeightUnit()
		{
			AWBHeader.Shipment.JS_ActualWeight = 30M;
			AWBHeader.Shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			AWBHeader.Populate();
			AssertEquals(30M, AWBHeader.RateLineGrossWeight);

			AWBHeader.Shipment.JS_UnitOfWeight = Core.Constants.Weight.Ounces;
			AWBHeader.Populate();
			AssertEquals(1.875m, AWBHeader.RateLineGrossWeight);

			AWBHeader.Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AWBHeader.Populate();
			AssertEquals(30m, AWBHeader.RateLineGrossWeight);

			AWBHeader.Shipment.JS_UnitOfWeight = "WA";
			AWBHeader.Populate();
			AssertEquals(0M, AWBHeader.RateLineGrossWeight);
		}

		public void TestRateLineWeightUnit()
		{
			AWBHeader.Shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			AWBHeader.Populate();
			AssertEquals("K", AWBHeader.RateLineWeightUnit);

			AWBHeader.Shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			AWBHeader.Populate();
			AssertEquals("L", AWBHeader.RateLineWeightUnit);

			AWBHeader.Shipment.JS_UnitOfWeight = Constants.Weight.Grams;
			AWBHeader.Populate();
			AssertEquals("K", AWBHeader.RateLineWeightUnit);

			AWBHeader.Shipment.JS_UnitOfWeight = Constants.Weight.Ounces;
			AWBHeader.Populate();
			AssertEquals("L", AWBHeader.RateLineWeightUnit);
		}

		#endregion

		#region Shipping Load

		public void TestShippingLoadAndCount()
		{
			AWBHeader.Shipment.JS_TotalPackageCount = 0;
			AWBHeader.Shipment.JS_OuterPacks = 9;
			AWBHeader.Populate();
			AssertEquals(0, AWBHeader.EH_ShippingLoadAndCount);

			AWBHeader.Shipment.JS_TotalPackageCount = 4;
			AWBHeader.Populate();
			AssertEquals(4, AWBHeader.EH_ShippingLoadAndCount);
		}

		public void TestShippingLoadAndCountWhenBiggerThanZShortRange()
		{
			AWBHeader.Shipment.JS_TotalPackageCount = 40000;
			AWBHeader.Populate();
			AssertEquals(40000, AWBHeader.EH_ShippingLoadAndCount);
		}

		#endregion

		#region Prepaid/Collect

		public void TestEH_TotalWeightPPDAndEH_TotalWeightCOL_WhenPrepaidWeight()
		{
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount = 120;
			AWBHeader.AWBRateLines[0].ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false; // results in a call to Populate()
			AssertEquals(0m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(120m, AWBHeader.EH_TotalWeightCOL);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(120m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(0m, AWBHeader.EH_TotalWeightCOL);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jobHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var freightChargeCode = Factory.New<AccChargeCode>();
			freightChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			var freightCharge1 = NewJobCharge(jobHeader, 11.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Consol.SendingForwarder, 1.1m);
			var freightCharge2 = NewJobCharge(jobHeader, 1252.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Shipment.Consignor, 1.1m);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals(11.0m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(1252.0m, AWBHeader.EH_TotalWeightPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AWBHeader.AWBRateLines[0].ER_Total = 120m;
			AssertEquals(120m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(0m, AWBHeader.EH_TotalWeightCOL);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(1252.0m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(11.0m, AWBHeader.EH_TotalWeightCOL);
		}

		public void TestEH_TotalWeightPPDAndEH_TotalWeightCOL_WhenCollectWeight()
		{
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount = 120;
			AWBHeader.AWBRateLines[0].ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false; // results in a call to Populate()
			AssertEquals(0m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(120m, AWBHeader.EH_TotalWeightPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(120m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(0m, AWBHeader.EH_TotalWeightPPD);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jobHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var freightChargeCode = Factory.New<AccChargeCode>();
			freightChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			var freightCharge1 = NewJobCharge(jobHeader, 11.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Consol.SendingForwarder, 1.1m);
			var freightCharge2 = NewJobCharge(jobHeader, 1252.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Shipment.Consignor, 1.1m);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals(11.0m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(1252.0m, AWBHeader.EH_TotalWeightPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AWBHeader.AWBRateLines[0].ER_Total = 120m;
			AssertEquals(120m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(0m, AWBHeader.EH_TotalWeightPPD);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(1252.0m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(11.0m, AWBHeader.EH_TotalWeightCOL);
		}

		public void TestEH_TotalWeightPPDAndEH_TotalWeightCOL_WhenPrepaidCollectWeight()
		{
			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount = 120;
			AWBHeader.AWBRateLines[0].ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AWBHeader.AWBRateLines[1].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[1].ER_RateChargeOrDiscount = 350;
			AWBHeader.AWBRateLines[1].ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(120m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(350m, AWBHeader.EH_TotalWeightCOL);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jobHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var freightChargeCode = Factory.New<AccChargeCode>();
			freightChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			var freightCharge1 = NewJobCharge(jobHeader, 11.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Consol.SendingForwarder, 1.1m);
			var freightCharge2 = NewJobCharge(jobHeader, 1252.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Shipment.Consignor, 1.1m);

			AWBHeader.EH_AreRateLinesOverridden = false;
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals(11.0m, AWBHeader.EH_TotalWeightCOL);
			AssertEquals(1252.0m, AWBHeader.EH_TotalWeightPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertEquals(0m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(0m, AWBHeader.EH_TotalWeightCOL);

			AWBHeader.AWBRateLines[0].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount = 120;
			AWBHeader.AWBRateLines[0].ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AWBHeader.AWBRateLines[1].ER_ChargeableWeight = 1;
			AWBHeader.AWBRateLines[1].ER_RateChargeOrDiscount = 350;
			AWBHeader.AWBRateLines[1].ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals(120m, AWBHeader.EH_TotalWeightPPD);
			AssertEquals(350m, AWBHeader.EH_TotalWeightCOL);
		}

		#endregion

		#region Properties

		public void TestExportAWBHeaderConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USNYC";

			CommonConsol consolA = shipment.Consols.AddNew();
			consolA.JK_TransportMode = Constants.TransportModes.Air;
			consolA.JK_RL_NKLoadPort = "AUSYD";
			consolA.JK_RL_NKDischargePort = "AUBNE";

			CommonConsol consolB = shipment.Consols.AddNew();
			consolB.JK_TransportMode = Constants.TransportModes.Air;
			consolB.JK_RL_NKLoadPort = "AUBNE";
			consolB.JK_RL_NKDischargePort = "USNYC";

			ExportAWBHeader awb = shipment.AWBHeader;
			awb.Populate();

			AssertEquals(awb.Consol, consolA);

			consolA.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(awb.Consol, consolB);

			consolB.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(awb.Consol, consolA);

			consolB.JK_TransportMode = Constants.TransportModes.Air;
			consolB.JK_RL_NKDischargePort = "USORD";
			CommonConsol consolC = shipment.Consols.AddNew();
			consolC.JK_TransportMode = Constants.TransportModes.Air;
			consolC.JK_RL_NKLoadPort = "USORD";
			consolC.JK_RL_NKDischargePort = "USNYC";
			AssertEquals(awb.Consol, consolB);
		}

		public void TestConsolNumber()
		{
			AssertEquals("Consol Number is empty", "", AWBHeader.EH_ConsolNumber);

			AWBHeader.Consol.JK_UniqueConsignRef = "C000099";
			AssertEquals("C000099", AWBHeader.EH_ConsolNumber);
		}

		public void TestRateClass()
		{
			AWBHeader.Shipment.JS_ActualWeight = 10M;
			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.NormalCharge, AWBHeader.AWBRateLines[0].ER_RateClass);

			AWBHeader.Shipment.JS_ActualWeight = 45M;
			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, AWBHeader.AWBRateLines[0].ER_RateClass);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;

			var chargeCode = Factory.New<AccChargeCode>();

			var freightCharge = Factory.New<JobCharge>();
			freightCharge.JR_AC = chargeCode.PK;
			freightCharge.JR_JH = jobHeader.PK;
			freightCharge.JR_LocalSellAmt = 99.0m;

			var oldCharge = Env.Registry.FreightChargeCode;
			Env.Registry.FreightChargeCode = chargeCode.PK.ToGuid();

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, AWBHeader.AWBRateLines[0].ER_RateClass);

			var costBasis = Factory.New<JobPaymentBasis>();
			costBasis.PBS_JR = freightCharge.PK;
			costBasis.PBS_IsCost = true;
			costBasis.PBS_RateReference = "MIN";

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, AWBHeader.AWBRateLines[0].ER_RateClass);

			var sellBasis = Factory.New<JobPaymentBasis>();
			sellBasis.PBS_JR = freightCharge.PK;
			sellBasis.PBS_IsCost = false;
			sellBasis.PBS_RateReference = "MIN";

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.MinimumCharge, AWBHeader.AWBRateLines[0].ER_RateClass);

			var anotherFreightCharge = Factory.New<JobCharge>();
			anotherFreightCharge.JR_AC = chargeCode.PK;
			anotherFreightCharge.JR_JH = jobHeader.PK;
			anotherFreightCharge.JR_LocalSellAmt = 9157.0m;

			var freightCharges = AWBHeader.GetFreightCharges();
			AssertEquals(2, freightCharges.Length);

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, AWBHeader.AWBRateLines[0].ER_RateClass);

			anotherFreightCharge.JR_JH = ZGuid.Empty;
			freightCharges = AWBHeader.GetFreightCharges();
			AssertEquals(1, freightCharges.Length);

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.MinimumCharge, AWBHeader.AWBRateLines[0].ER_RateClass);

			Env.Registry.FreightChargeCode = oldCharge;
			freightCharges = AWBHeader.GetFreightCharges();
			AssertEquals("There should be no charge matching the defined Freight charge", 0, freightCharges.Length);

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.RateClass.QuantityRate, AWBHeader.AWBRateLines[0].ER_RateClass);
		}

		public void TestAgentIATACode()
		{
			AWBHeader.Populate();
			AssertEquals(((ZString)Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode), AWBHeader.EH_AgentIATACodeFormatted);
		}

		public void TestAgentAccountNo()
		{
			AWBHeader.Populate();
			AssertEquals(Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber, AWBHeader.EH_AgentAccountNo);
		}

		public void TestPrepaidCollect()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.Populate();
			AssertEquals("FOB is Freight Collect", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_WeightPrepaidCollect);
			AssertEquals("FOB is Origin Prepaid", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_OtherPrepaidCollect);
			AssertEquals("FOB is Freight Collect, Origin Prepaid", ExportAWBHeader.Constants.ChargeCodes.DestinationCollectCash, AWBHeader.EH_ChargesCode);

			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			AWBHeader.Populate();
			AssertEquals("EXW is Freight Collect", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_WeightPrepaidCollect);
			AssertEquals("EXW is Origin Collect", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_OtherPrepaidCollect);
			AssertEquals("EXW is all collect", ExportAWBHeader.Constants.ChargeCodes.AllChargesCollect, AWBHeader.EH_ChargesCode);

			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AWBHeader.Populate();
			AssertEquals("DDP is Freight Prepaid", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_WeightPrepaidCollect);
			AssertEquals("DDP is Origin Prepaid", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_OtherPrepaidCollect);
			AssertEquals("DDP is all Prepaid", ExportAWBHeader.Constants.ChargeCodes.AllChargesPrepaidCash, AWBHeader.EH_ChargesCode);
		}

		public void TestEH_WeightPrepaidCollect()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_WeightPrepaidCollect);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from shipment - FOB is Collect Freight charges", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_WeightPrepaidCollect);
		}

		public void TestRateClassLabelText()
		{
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals("EH_RateClassLabelText", "Rate Class", AWBHeader.EH_RateClassLabelText);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals("EH_RateClassLabelText", "Rate Class", AWBHeader.EH_RateClassLabelText);

			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertEquals("EH_RateClassLabelText", "Prepaid/Collect", AWBHeader.EH_RateClassLabelText);
		}

		public void TestEH_OtherPrepaidCollect()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect, AWBHeader.EH_OtherPrepaidCollect);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from AWB shipment - FOB is Prepaid Origin charges", ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.EH_OtherPrepaidCollect);
		}

		public void TestEH_WeightPPD()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //PPD
			AWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from shipment", ZBool.True, AWBHeader.EH_WeightPPD);
		}

		public void TestEH_WeightCOL()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //COL
			AWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_WeightCOL);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from shipment", ZBool.True, AWBHeader.EH_WeightCOL);
		}

		public void TestEH_WeightBTH()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //PPD
			AWBHeader.EH_WeightVPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_WeightBTH);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("prepaid/collect status from shipment", ZBool.False, AWBHeader.EH_WeightBTH);
		}

		public void TestDomesticPaymentTerms_Prepaid()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNAAT";
			shipment.JS_RL_NKDestination = "CNSHA";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;

			awbHeader.Populate();

			AssertEquals("PPD is Prepaid", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, awbHeader.EH_WeightVPPDCOL);
			AssertEquals("PPD is Prepaid", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, awbHeader.EH_OtherPPDCOL);
		}

		public void TestDomesticPaymentTerms_Collect()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNAAT";
			shipment.JS_RL_NKDestination = "CNSHA";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;

			awbHeader.Populate();

			AssertEquals("CLT is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_WeightVPPDCOL);
			AssertEquals("CLT is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_OtherPPDCOL);
		}

		public void TestDomesticPaymentTerms_CollectCOD()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNAAT";
			shipment.JS_RL_NKDestination = "CNSHA";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.CollectCOD;

			awbHeader.Populate();

			AssertEquals("FCD is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_WeightVPPDCOL);
			AssertEquals("FCD is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_OtherPPDCOL);
		}

		public void TestDomesticPaymentTerms_CollectThirdParty()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CNAAT";
			shipment.JS_RL_NKDestination = "CNSHA";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.CollectThirdParty;

			awbHeader.Populate();

			AssertEquals("C3P is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_WeightVPPDCOL);
			AssertEquals("C3P is Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, awbHeader.EH_OtherPPDCOL);
		}

		public void TestEH_OtherPPD()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //PPD
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.False, AWBHeader.EH_OtherPPD);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AWBHeader.Populate();
			AssertEquals("prepaid/collect status from AWB shipment", ZBool.True, AWBHeader.EH_OtherPPD);
		}

		public void TestEH_OtherCOL()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_OtherCOL);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AWBHeader.Populate();
			AssertEquals("prepaid/collect status from shipment - Origin Charges prepaid for FOB", ZBool.False, AWBHeader.EH_OtherCOL);
		}

		public void TestEH_OtherBTH()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //PPD
			AWBHeader.EH_OtherPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("prepaid/collect status from AWB header", ZBool.True, AWBHeader.EH_OtherBTH);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			AWBHeader.Populate();
			AssertEquals("prepaid/collect status from shipment", ZBool.False, AWBHeader.EH_OtherBTH);
		}

		public void TestWeightVPPDCol()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //Prepaid
			AWBHeader.Populate();
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, AWBHeader.EH_WeightVPPDCOL);

			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Collect
			AWBHeader.Populate();
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, AWBHeader.EH_WeightVPPDCOL);
		}

		public void TestAgentAgentName()
		{
			AWBHeader.Populate();
			AssertEquals(Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, AWBHeader.EH_AgentName);
		}

		public void TestAgentPlace()
		{
			AWBHeader.Populate();
			AssertEquals(Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity, AWBHeader.EH_AgentPlace);
		}

		public void TestSpecialHandlingCode()
		{
			RunSpecialHandlingCodeCTTest("DEHAM", "USLAX", true);
			RunSpecialHandlingCodeCTTest("ESMAD", "ESBCN", true);
			RunSpecialHandlingCodeCTTest("AUSYD", "AUMEL", false);
		}

		void RunSpecialHandlingCodeCTTest(ZString loadPort, ZString dischargePort, bool shouldBePopulated)
		{
			AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
			AWBHeader.Shipment.JS_RL_NKDestination = dischargePort;
			AWBHeader.Populate();
			AssertEquals(ZString.Empty, AWBHeader.EH_SpecialHandlingCode);

			AWBHeader.Shipment.JS_CommunityTransitStatus = "";
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_SpecialHandlingCode);

			AWBHeader.Shipment.JS_CommunityTransitStatus = "C";
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_SpecialHandlingCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(loadPort.Left(2)))
			{
				AWBHeader.Shipment.JS_RL_NKOrigin = loadPort;

				AWBHeader.Shipment.JS_CommunityTransitStatus = "C";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "C" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "F";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "F" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T1";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T1" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2F";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2F" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2L";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2L" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2LF";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2LF" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2LSM";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2LSM" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T2SM";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T2SM" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "TD";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "TD" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "TF";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "TF" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "X";
				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "X" : "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "T";
				AWBHeader.Populate();
				AssertEquals("T is not a currently allowed special handling code", "", AWBHeader.EH_SpecialHandlingCode);

				AWBHeader.Shipment.JS_CommunityTransitStatus = "HELLO";
				AWBHeader.Populate();
				AssertEquals("HELLO is not a currently allowed special handling code", "", AWBHeader.EH_SpecialHandlingCode);
			}
		}

		public void TestSpecialHandlingCode_Transshipment()
		{
			RunSpecialHandlingCodeCTTest("INDEL", "DEHAM", "USLAX", true);
			RunSpecialHandlingCodeCTTest("INDEL", "AUSYD", "NZAKL", false);
		}

		void RunSpecialHandlingCodeCTTest(ZString loadPort, ZString dischargePort, ZString destinationPort, bool shouldBePopulated)
		{
			AWBHeader.Shipment.JS_RL_NKOrigin = loadPort;
			AWBHeader.Shipment.JS_RL_NKDestination = destinationPort;

			var transport1 = AWBHeader.Shipment.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportCodes.Air;
			transport1.JW_RL_NKLoadPort = loadPort;
			transport1.JW_RL_NKDiscPort = dischargePort;

			var transport2 = AWBHeader.Shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportCodes.Air;
			transport2.JW_RL_NKLoadPort = dischargePort;
			transport2.JW_RL_NKDiscPort = destinationPort;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(dischargePort.Left(2)))
			{
				AWBHeader.Shipment.JS_CommunityTransitStatus = "T1";

				AWBHeader.Populate();
				AssertEquals(shouldBePopulated ? "T1" : string.Empty, AWBHeader.EH_SpecialHandlingCode);
			}
		}

		#endregion

		#region Address Tests

		#region Consignee Address

		public void TestConsigneeDocumentaryAddress()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Shipment.ConsigneeDocumentaryAddress, AWBHeader.ConsigneeDocumentaryAddress);
		}

		public void TestDefaultConsigneeAddressType()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			Env.Registry.Freight.AirWaybill.ConsigneeAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultConsigneeAddressType);
			AWBHeader.Shipment.Consignee.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Delivery;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Delivery, AWBHeader.DefaultConsigneeAddressType);
		}

		public void TestConsigneeOfficeAddress()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Shipment.Consignee.MainAddress, AWBHeader.ConsigneeOfficeAddress);
		}

		public void TestConsigneeDeliveryAddress()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Shipment.Consignee, OrgConstants.AddressType.Delivery);
			AssertEquals("COMPANYNAME", AWBHeader.ConsigneeDeliveryAddress.OA_CompanyNameOverride);
		}

		public void TestDefaultConsigneeCompanyName()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			AWBHeader.Shipment.Consignee.OH_FullName = "DEFAULTCONSIGNEECOMPANYNAME";
			AssertEquals("DEFAULTCONSIGNEECOMPANYNAME", AWBHeader.DefaultConsigneeCompanyName);
		}

		public void TestGetconsigneeAddreses()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Shipment.Consignee, OrgConstants.AddressType.Delivery);
			AssertEquals(AWBHeader.Shipment.Consignee.Addresses.Count, AWBHeader.GetConsigneeAddresses.Count);
		}

		#endregion

		#region Shipper Address

		public void TestShipperDocumentaryAddress()
		{
			AssertEquals(AWBHeader.Shipment.ConsignorDocumentaryAddress, AWBHeader.ShipperDocumentaryAddress);
		}

		public void TestDefaultShipperAddressType()
		{
			Env.Registry.Freight.AirWaybill.ShipperAddressDefaultsTo = OrgConstants.AddressType.Documentary;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Documentary, AWBHeader.DefaultShipperAddressType);
			AWBHeader.Shipment.Consignor.MiscServ.OM_EXDocumentAddressPreference = OrgConstants.AddressType.Pickup;
			AssertEquals(ExportAWBHeader.DefaultAddressTypes.Pickup, AWBHeader.DefaultShipperAddressType);
		}

		public void TestShipperOfficeAddress()
		{
			AWBHeader.Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Shipment.Consignor.MainAddress, AWBHeader.ShipperOfficeAddress);
		}

		public void TestShipperDeliveryAddress()
		{
			AWBHeader.Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Shipment.Consignor, OrgConstants.AddressType.Pickup);
			AssertEquals("COMPANYNAME", AWBHeader.ShipperPickupAddress.OA_CompanyNameOverride);
		}

		public void TestDefaultShipperCompanyName()
		{
			AWBHeader.Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Shipment.Consignor.OH_FullName = "DEFAULTSHIPPERCOMPANYNAME";
			AssertEquals("DEFAULTSHIPPERCOMPANYNAME", AWBHeader.DefaultShipperCompanyName);
		}

		public void TestGetShipperAddreses()
		{
			AWBHeader.Shipment.ConsignorPK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Shipment.Consignor, OrgConstants.AddressType.Pickup);
			AssertEquals(AWBHeader.Shipment.Consignor.Addresses.Count, AWBHeader.GetShipperAddresses.Count);
		}

		#endregion

		#region Notify Address

		public void TestNotifyPartyDocumentaryAddress()
		{
			AWBHeader.Shipment.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertEquals(AWBHeader.Shipment.NotifyPartyDocumentaryAddress, AWBHeader.NotifyPartyDocumentaryAddress);
		}

		public void TestGetAlsoNotifyAddreses()
		{
			AWBHeader.Shipment.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AddAddress(AWBHeader.Shipment.NotifyParty, OrgConstants.AddressType.PickupAndDelivery);
			AssertEquals(AWBHeader.Shipment.NotifyParty.Addresses.Count, AWBHeader.GetAlsoNotifyAddresses.Count);
		}

		#endregion

		#region Addresses Common

		void AddAddress(OrgHeader org, ZString addressType)
		{
			OrgAddress address = org.Addresses.AddNew();
			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.OA_CompanyNameOverride = "COMPANYNAME";
		}

		#endregion

		#endregion

		public void TestRateLinesLength()
		{
			var packLine1 = AWBHeader.Shipment.OuterPackLines.AddNew();
			packLine1.JL_Length = 10;
			packLine1.JL_Width = 11;
			packLine1.JL_Height = 12;
			packLine1.JL_UnitOfDimension = Constants.Length.Inches;
			packLine1.JL_PackageCount = 13;

			var packLine2 = AWBHeader.Shipment.OuterPackLines.AddNew();
			packLine2.JL_Length = 20;
			packLine2.JL_Width = 21;
			packLine2.JL_Height = 22;
			packLine2.JL_UnitOfDimension = Constants.Length.Inches;
			packLine2.JL_PackageCount = 23;

			AWBHeader.Populate();

			AssertEquals("First line is NOT limited to 15 chars", "DIMS 10x11x12 IN x 13", AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text);
			AssertEquals("Subsequent lines aren't limited", "DIMS 20x21x22 IN x 23", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text = "more than 15 chars";

			AssertNoMessageErrors(AWBHeader.AWBRateLine1.NatureAndQtyOfGoods.TextInfo);
		}

		public void TestShipperAccount()
		{
			AssertEquals(consignorCode, AWBHeader.EH_ShipperAccount);

			AWBHeader.Shipment.ConsignorDocumentaryAddress.Organisation.OH_Code = "TEST";
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Populate();
			AssertEquals("TEST", AWBHeader.EH_ShipperAccount);

			AWBHeader.Shipment.ConsignorDocumentaryAddress.Organisation.CountryData.OV_EXExportPermissionDetails = "EXPerm";
			AWBHeader.Populate();
			AssertEquals("TEST", AWBHeader.EH_ShipperAccount);
		}

		public void TestConsigneeAccount()
		{
			AssertEquals("", AWBHeader.EH_ConsigneeAccount);
			AWBHeader.Shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AWBHeader.Shipment.ConsigneeDocumentaryAddress.Organisation.OH_Code = "TESTCODE";
			AWBHeader.Populate();
			AssertEquals("TESTCODE", AWBHeader.EH_ConsigneeAccount);
		}

		public void TestAirlinePrefix()
		{
			AWBHeader.Consol.JK_MasterBillNum = "2";
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_AirlinePrefix);

			AWBHeader.Consol.JK_MasterBillNum = "081";
			AWBHeader.Populate();
			AssertEquals("081", AWBHeader.EH_AirlinePrefix);

			AWBHeader.Consol.JK_MasterBillNum = "256 5456544";
			AWBHeader.Populate();
			AssertEquals("256", AWBHeader.EH_AirlinePrefix);
		}

		void AddOtherCharges()
		{
			JobHeader jHeader = Factory.NewJobForTesting<JobHeader>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			//this is a collect charge
			JobCharge jCharge = Factory.New<JobCharge>();
			AccChargeCode charge1 = Factory.New<AccChargeCode>();
			charge1.AC_Desc = "Desc 1";
			jCharge.JR_AC = charge1.PK;
			jCharge.JR_JH = jHeader.PK;
			jCharge.JR_LocalSellAmt = 11.0m;
			jCharge.JR_Desc = "This is some value that is over 35 chars in length to test that only 35 chars are copied into the othercharges field";
			jCharge.JR_OH_SellAccount = AWBHeader.Consol.SendingForwarderPK;

			//this is a prepaid charge
			jCharge = Factory.New<JobCharge>();
			AccChargeCode charge2 = Factory.New<AccChargeCode>();
			charge2.AC_Desc = "Desc 2";
			jCharge.JR_AC = charge2.PK;
			jCharge.JR_JH = jHeader.PK;
			jCharge.JR_LocalSellAmt = 99.0m;
			jCharge.JR_Desc = "This is some value that is over 35 chars in length to test that only 35 chars are copied into the othercharges field";
			jCharge.JR_OH_SellAccount = AWBHeader.Shipment.ConsignorPK;
		}

		public void TestHandlingInformationWithVATNumberForArgentina()
		{
			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, Constants.CountryCodes.Argentina, Constants.CountryCodes.Argentina, 1, "CUIT", "CUIT", "HAW");

			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

			AWBHeader.Shipment.JS_RL_NKDestination = "ARBUE";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMDocumentAddressPreference = OrgConstants.AddressType.Office;

			var taxCode = consignee.CustomsCodes.AddNew();
			taxCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;
			taxCode.OK_CustomsRegNo = "1234";

			AWBHeader.Shipment.ConsigneePK = consignee.PK;
			AWBHeader.Populate();
			AssertEquals("CUIT: 1234", (string)AWBHeader.EH_HandlingInformation);

			AWBHeader.Shipment.JS_RL_NKDestination = "CNSHA";
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithRUCNumberForBrazil()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);
			var shipment = AWBHeader.Shipment;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "BRSAO";
			shipment.JS_RL_NKDestination = "AUSYD";
			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum.CE_RN_NKCountryCode = CountryCodes.Brazil;
			entryNum.CE_EntryNum = "6AU123456789D0VAHK11N";
			AWBHeader.Populate();
			AssertEquals("RUC:6AU123456789D0VAHK11N", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithDUENumberForBrazil()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);
			var shipment = AWBHeader.Shipment;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "BRSAO";
			shipment.JS_RL_NKDestination = "AUSYD";
			var cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			cusEntryNumber.CE_RN_NKCountryCode = CountryCodes.Brazil;
			cusEntryNumber.CE_EntryNum = "69BR5689541979";
			AWBHeader.Populate();
			AssertEquals("DUE:69BR5689541979", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithRUCNumberAndDUENumberForBrazil()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);
			var shipment = AWBHeader.Shipment;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "BRSAO";
			shipment.JS_RL_NKDestination = "AUSYD";
			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryType = BrazilAdditionalReferenceNumberTypes.Codes.RUC;
			entryNum.CE_RN_NKCountryCode = CountryCodes.Brazil;
			entryNum.CE_EntryNum = "6AU123456789D0VAHK11N";
			var cusEntryNumber = shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			cusEntryNumber.CE_RN_NKCountryCode = CountryCodes.Brazil;
			cusEntryNumber.CE_EntryNum = "69BR5689541979";
			AWBHeader.Populate();
			AssertEquals("RUC:6AU123456789D0VAHK11N\r\nDUE:69BR5689541979", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformationWithACINumberForEgypt()
		{
			AWBHeader.Populate();
			AssertEquals(string.Empty, AWBHeader.EH_HandlingInformation);

			var shipment = AWBHeader.Shipment;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "EGCAI";

			var entryNum = shipment.Numbers.AddNew();
			entryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum.CE_RN_NKCountryCode = CountryCodes.Egypt;
			entryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

			var entryNum2 = shipment.Numbers.AddNew();
			entryNum2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			entryNum2.CE_RN_NKCountryCode = CountryCodes.Egypt;
			entryNum2.CE_EntryNum = "6AU123456789D0VAHK11O";

			AWBHeader.Populate();
			AssertEquals("ACID Number:6AU123456789D0VAHK11N,6AU123456789D0VAHK11O", AWBHeader.EH_HandlingInformation);
		}

		public void TestHandlingInformation_RegistryExtraTextWithNewLineDelimiter()
		{
			using (FreightDataRegistry.Instance.HAWBHandlingInformationExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "HAWB REGISTRY TEXT"))
			{
				var shipment = AWBHeader.Shipment;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "EGCAI";
				shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "SHIPMENT NOTE");

				var entryNum = shipment.Numbers.AddNew();
				entryNum.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
				entryNum.CE_RN_NKCountryCode = CountryCodes.Egypt;
				entryNum.CE_EntryNum = "6AU123456789D0VAHK11N";

				AWBHeader.Populate();
				AssertEquals("SHIPMENT NOTE\r\nACID Number:6AU123456789D0VAHK11N\r\nHAWB REGISTRY TEXT", AWBHeader.EH_HandlingInformation);
			}
		}

		public void TestEH_OtherChargesDueCarrierCOL()
		{
			AddOtherCharges();
			AWBHeader.Populate();
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierCOL);
		}

		public void TestEH_OtherChargesDueCarrierPPD()
		{
			AddOtherCharges();
			AWBHeader.Populate();
			AssertEquals(0M, AWBHeader.EH_OtherChargesDueCarrierPPD);
		}

		public void TestEH_OtherChargesDueAgentPPD()
		{
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AddOtherCharges();
			AWBHeader.Populate();
			AssertEquals(99M, AWBHeader.EH_OtherChargesDueAgentPPD);
		}

		public void TestEH_OtherChargesDueAgentCOL()
		{
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AddOtherCharges();
			AWBHeader.Populate();
			AssertEquals(11M, AWBHeader.EH_OtherChargesDueAgentCOL);
		}

		public void TestHouseBill()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "PPHDIV00000012";

			ShipmentExportAWBHeader header = Factory.New<ShipmentExportAWBHeader>();
			header.EH_ParentID = shipment.PK;

			AssertEquals("DIV00000012", header.HouseBill);

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "PPHDIV000012";

			var header1 = Factory.New<ShipmentExportAWBHeader>();
			header1.EH_ParentID = shipment1.PK;

			AssertEquals("PPHDIV000012", header1.HouseBill);
		}

		#region Test PopulateOtherCharges

		public void TestPopulateOtherCharges()
		{
			var jHeader = Factory.NewJobForTesting<JobHeader>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var chargedCodeAC = Factory.New<AccChargeCode>();
			chargedCodeAC.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			var chargeCodeTV = Factory.New<AccChargeCode>();
			chargeCodeTV.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.TV;
			var description = "TV charge code description";
			chargedCodeAC.AC_Desc = description;
			chargeCodeTV.AC_Desc = description;

			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// prepaid charge
			var jCharge = NewJobCharge(jHeader, 99.0m, chargeCodeTV, description, AWBHeader.Shipment.Consignor, 9.9m);
			AWBHeader.Populate();
			AssertEquals(1, AWBHeader.AWBOtherCharges.Count);
			AssertEquals(9.9m, AWBHeader.EH_TaxesPPD);
			AssertChargeExists(AWBHeader.AWBOtherCharges, 99m, "TV charge code description", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, Core.Constants.AWB.ChargeCodes.TV);

			// collect charge
			jCharge = NewJobCharge(jHeader, 11.0m, chargedCodeAC, description, AWBHeader.Consol.SendingForwarder, 1.1m);
			AWBHeader.Populate();
			AssertEquals(2, AWBHeader.AWBOtherCharges.Count);
			AssertEquals(1.1m, AWBHeader.EH_TaxesCOL);
			AssertChargeExists(AWBHeader.AWBOtherCharges, 11m, "TV charge code description", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, Core.Constants.AWB.ChargeCodes.AC);

			// profit share
			var profitShare = Factory.Load<AccChargeCode>(ObjectFactory.Get<IAccounting>().ProfitShareChargeCode);
			jCharge = NewJobCharge(jHeader, 5m, profitShare, "Crap", AWBHeader.Shipment.Consignor, 0.5m);
			AWBHeader.Populate();
			AssertEquals("Profit Share is not added", 2, AWBHeader.AWBOtherCharges.Count);

			var displayCollection = AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB);
			displayCollection[Core.Constants.AWB.ChargeCodes.TV].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AWBHeader.Populate();
			AssertEquals("collect display option shouldn't hide prepaid charge", 2, AWBHeader.AWBOtherCharges.Count);

			displayCollection[Core.Constants.AWB.ChargeCodes.TV].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.HAWBPrepaidDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AWBHeader.Populate();
			AssertEquals("collect charge should be hidden", 1, AWBHeader.AWBOtherCharges.Count);
			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, AWBHeader.AWBOtherCharges[0].EO_ChargeCode);

			displayCollection[Core.Constants.AWB.ChargeCodes.AC].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			displayCollection[Core.Constants.AWB.ChargeCodes.TV].Visibility = nameof(AWBDisplayOptionVisibility.Hide);
			ExportAWBRegistry.Instance.HAWBCollectDisplayOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, displayCollection);

			AWBHeader.Populate();
			AssertEquals(0, AWBHeader.AWBOtherCharges.Count);
		}

		public void TestPopulateOtherChargesWithoutJobHeader()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Air, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "FRPAR";
				shipment1.JS_RL_NKDestination = "NZAKL";

				shipment1.ConsignorPK = consignor.PK;
				shipment1.ConsigneePK = consignee.PK;

				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertNull("Precondition: JobHeader should be null", shipment1.JobHeader);
					var awbHeader = Factory.New<ShipmentExportAWBHeader>();

					AssertNoExceptionThrown(() => {
						awbHeader.EH_ParentID = shipment1.PK;
						awbHeader.Populate();
					});
				}
			}
		}

		public void TestHAWBOtherChargesHasOverseasChargesWithRegistrySetting()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Air, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				CreateAccChargeCode("DDOC", nzCompany.PK);
				CreateAccChargeCode("521", nzCompany.PK);
				CreateAccChargeCode("CAF", nzCompany.PK);
				var frt = CreateAccChargeCode("FRT", nzCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					nzOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = TransportModes.Air;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor2.PK;
					shipment1.ConsigneePK = consignee2.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor2.PK;
					header.AgentCollectPK = nzOrgProxy2.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

					factory2.Save();
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory3 = new BusinessObjectFactory();

					var consignor3 = factory3.Load<OrgHeader>(consignor.PK);
					var consignee3 = factory3.Load<OrgHeader>(consignee.PK);
					var frOrgProxy3 = factory3.Load<OrgHeader>(frOrgProxy.PK);
					consignor3.CompanyData.OB_IsDebtor = true;
					consignee3.CompanyData.OB_IsDebtor = true;
					frOrgProxy3.CompanyData.OB_IsDebtor = true;

					var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);

					var loader2 = new JobHeader.Loader(shipment2);
					var header2 = loader2.TryLoadOrCreateWithMutex();
					header2.LocalChargesPK = consignee3.PK;
					header2.AgentCollectPK = frOrgProxy3.PK;

					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 40m, "DDOC", "NZD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.AgentCollectPK, 30m, "CAF", "AUD", factory3);
					CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 20m, "521", "NZD", factory3);

					factory3.Save();
				}

				void CheckAllCharges(ForwardingShipment shipment)
				{
					var awbHeader = Factory.New<ShipmentExportAWBHeader>();
					awbHeader.EH_ParentID = shipment.PK;
					var chargeDescriptions = awbHeader.AWBOtherCharges.Select(c => c.EO_ChargeDescription);

					AssertContainsExactElementsInAnyOrder(new[] { "International Freight", "DDOC Desc", "521 Desc" }, chargeDescriptions);

					var frtChargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "International Freight").First();
					AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, frtChargeLine.EO_PPDCLT);
					AssertEquals(15m, frtChargeLine.Amount);
					AssertEquals("AUD", frtChargeLine.Currency);

					var ddochargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "DDOC Desc").First();
					AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, ddochargeLine.EO_PPDCLT);
					AssertEquals(40m, ddochargeLine.Amount);
					AssertEquals("NZD", ddochargeLine.Currency);

					var chargeLine521 = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "521 Desc").First();
					AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, chargeLine521.EO_PPDCLT);
					AssertEquals(20m, chargeLine521.Amount);
					AssertEquals("NZD", chargeLine521.Currency);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Env.Registry.FreightChargeCode = frt.PK.ToGuid();
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);
					CheckAllCharges(shipment3);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Env.Registry.FreightChargeCode = frt.PK.ToGuid();
					var factory5 = new BusinessObjectFactory();
					var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);
					CheckAllCharges(shipment4);
				}
			}
		}

		public void TestHAWBOtherChargesHasLocalChargesWithoutRegistrySetting()
		{
			var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
			var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");

			var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
			var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);

			var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
			consignor.OH_IsConsignor = true;
			consignor.OH_IsDebtor = true;

			var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
			consignee.OH_IsConsignee = true;
			consignee.OH_IsDebtor = true;

			CreateAccChargeCode("DDOC", nzCompany.PK);
			CreateAccChargeCode("521", nzCompany.PK);
			CreateAccChargeCode("CAF", nzCompany.PK);
			var frt = CreateAccChargeCode("FRT", nzCompany.PK);

			Factory.Save();

			ZGuid shipmentPK;

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();

				var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
				var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
				var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
				consignor2.CompanyData.OB_IsDebtor = true;
				consignee2.CompanyData.OB_IsDebtor = true;
				nzOrgProxy2.CompanyData.OB_IsDebtor = true;

				var shipment1 = factory2.New<ForwardingShipment>();
				shipmentPK = shipment1.PK;

				shipment1.JS_TransportMode = TransportModes.Air;
				shipment1.JS_RL_NKOrigin = "FRPAR";
				shipment1.JS_RL_NKDestination = "NZAKL";
				shipment1.JS_UniqueConsignRef = "S00005000";

				shipment1.ConsignorPK = consignor2.PK;
				shipment1.ConsigneePK = consignee2.PK;

				var loader = new JobHeader.Loader(shipment1);
				var header = loader.TryLoadOrCreateWithMutex();
				header.LocalChargesPK = consignor2.PK;
				header.AgentCollectPK = nzOrgProxy2.PK;

				CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", factory2);
				CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

				factory2.Save();
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factory3 = new BusinessObjectFactory();

				var consignor3 = factory3.Load<OrgHeader>(consignor.PK);
				var consignee3 = factory3.Load<OrgHeader>(consignee.PK);
				var frOrgProxy3 = factory3.Load<OrgHeader>(frOrgProxy.PK);
				consignor3.CompanyData.OB_IsDebtor = true;
				consignee3.CompanyData.OB_IsDebtor = true;
				frOrgProxy3.CompanyData.OB_IsDebtor = true;

				var shipment2 = factory3.Load<ForwardingShipment>(shipmentPK);

				var loader2 = new JobHeader.Loader(shipment2);
				var header2 = loader2.TryLoadOrCreateWithMutex();
				header2.LocalChargesPK = consignee3.PK;
				header2.AgentCollectPK = frOrgProxy3.PK;

				CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 40m, "DDOC", "NZD", factory3);
				CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.AgentCollectPK, 30m, "CAF", "AUD", factory3);
				CreateLineCharge(shipment2.JobHeader, shipment2.JobHeader.LocalChargesPK, 20m, "521", "NZD", factory3);

				factory3.Save();
			}

			void CheckAllCharges(ForwardingShipment shipment)
			{
				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;
				var chargeDescriptions = awbHeader.AWBOtherCharges.Select(c => c.EO_ChargeDescription);

				AssertContainsExactElementsInAnyOrder(new[] { "International Freight", "Bunker Adjustment Factor" }, chargeDescriptions);

				var frtChargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "International Freight").First();
				AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, frtChargeLine.EO_PPDCLT);
				AssertEquals(15m, frtChargeLine.Amount);
				AssertEquals("AUD", frtChargeLine.Currency);

				var ddochargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "Bunker Adjustment Factor").First();
				AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, ddochargeLine.EO_PPDCLT);
				AssertEquals(5m, ddochargeLine.Amount);
				AssertEquals("NZD", ddochargeLine.Currency);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Registry.FreightChargeCode = frt.PK.ToGuid();
				var factory4 = new BusinessObjectFactory();
				var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);
				CheckAllCharges(shipment3);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Env.Registry.FreightChargeCode = frt.PK.ToGuid();
				var factory5 = new BusinessObjectFactory();
				var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);
				CheckAllCharges(shipment4);
			}
		}

		public void TestHAWBOtherChargesHasLocalChargesWithoutOverseas()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Air, "FR", "NZ");

			using (FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
				var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");

				var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
				var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);

				var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
				consignor.OH_IsConsignor = true;
				consignor.OH_IsDebtor = true;

				var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");
				consignee.OH_IsConsignee = true;
				consignee.OH_IsDebtor = true;

				CreateAccChargeCode("DDOC", nzCompany.PK);
				CreateAccChargeCode("521", nzCompany.PK);
				CreateAccChargeCode("CAF", nzCompany.PK);
				var frt = CreateAccChargeCode("FRT", nzCompany.PK);

				Factory.Save();

				ZGuid shipmentPK;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					var factory2 = new BusinessObjectFactory();

					var consignor2 = factory2.Load<OrgHeader>(consignor.PK);
					var consignee2 = factory2.Load<OrgHeader>(consignee.PK);
					var nzOrgProxy2 = factory2.Load<OrgHeader>(nzOrgProxy.PK);
					consignor2.CompanyData.OB_IsDebtor = true;
					consignee2.CompanyData.OB_IsDebtor = true;
					nzOrgProxy2.CompanyData.OB_IsDebtor = true;

					var shipment1 = factory2.New<ForwardingShipment>();
					shipmentPK = shipment1.PK;

					shipment1.JS_TransportMode = TransportModes.Air;
					shipment1.JS_RL_NKOrigin = "FRPAR";
					shipment1.JS_RL_NKDestination = "NZAKL";
					shipment1.JS_UniqueConsignRef = "S00005000";

					shipment1.ConsignorPK = consignor2.PK;
					shipment1.ConsigneePK = consignee2.PK;

					var loader = new JobHeader.Loader(shipment1);
					var header = loader.TryLoadOrCreateWithMutex();
					header.LocalChargesPK = consignor2.PK;
					header.AgentCollectPK = nzOrgProxy2.PK;

					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.LocalChargesPK, 15m, "FRT", "AUD", factory2);
					CreateLineCharge(shipment1.JobHeader, shipment1.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

					factory2.Save();
				}

				void CheckAllCharges(ForwardingShipment shipment)
				{
					var awbHeader = Factory.New<ShipmentExportAWBHeader>();
					awbHeader.EH_ParentID = shipment.PK;
					var chargeDescriptions = awbHeader.AWBOtherCharges.Select(c => c.EO_ChargeDescription);

					AssertContainsExactElementsInAnyOrder(new[] { "International Freight", "Bunker Adjustment Factor" }, chargeDescriptions);

					var frtChargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "International Freight").First();
					AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, frtChargeLine.EO_PPDCLT);
					AssertEquals(15m, frtChargeLine.Amount);
					AssertEquals("AUD", frtChargeLine.Currency);

					var ddochargeLine = awbHeader.AWBOtherCharges.Find(c => c.EO_ChargeDescription == "Bunker Adjustment Factor").First();
					AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, ddochargeLine.EO_PPDCLT);
					AssertEquals(5m, ddochargeLine.Amount);
					AssertEquals("NZD", ddochargeLine.Currency);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Env.Registry.FreightChargeCode = frt.PK.ToGuid();
					var factory4 = new BusinessObjectFactory();
					var shipment3 = factory4.Load<ForwardingShipment>(shipmentPK);
					CheckAllCharges(shipment3);
				}

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, nzCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Env.Registry.FreightChargeCode = frt.PK.ToGuid();
					var factory5 = new BusinessObjectFactory();
					var shipment4 = factory5.Load<ForwardingShipment>(shipmentPK);
					CheckAllCharges(shipment4);
				}
			}
		}

		public void TestLoadAWBHeaderDbHits()
		{
			var collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection();
			CreatePrintChargesBilledToLocalClientAtDestAsCollect(collection, Core.Constants.TransportModes.Air, "FR", "NZ");

			FreightDataRegistry.Instance.PrintChargesBilledToLocalClientAtDestAsCollect.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var frOrgProxy = CreateOrg("FRPROORG", "FRProxy", "FRProxy Address", "FR", "FRPAR");
			var nzOrgProxy = CreateOrg("NZPROORG", "NZProxy", "NZProxy Address", "NZ", "NZAKL");

			var frCompany = CreateCompany("C#1", "FR Test", "B#1", "Brach 1", "FRPAR", "AUD", frOrgProxy.PK);
			var nzCompany = CreateCompany("C#2", "NZ Test", "B#2", "Brach 2", "NZAKL", "NZD", nzOrgProxy.PK);

			var consignor = CreateOrg("CONSIGNOR", "Consignor", "Consignor Address", "FR", "FRPAR");
			var consignee = CreateOrg("CONSIGNEE", "Consignee", "Consignee Address", "NZ", "NZAKL");

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, frCompany.Branches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var factory2 = new BusinessObjectFactory();

				var shipment = factory2.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "NZAKL";
				shipment.JS_UniqueConsignRef = "S00005000";
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;

				var loader = new JobHeader.Loader(shipment);
				var header = loader.TryLoadOrCreateWithMutex();
				header.LocalChargesPK = consignor.PK;
				header.AgentCollectPK = nzOrgProxy.PK;

				CreateLineCharge(shipment.JobHeader, shipment.JobHeader.AgentCollectPK, 5m, "BAF", "NZD", factory2);

				factory2.Save();

				var factory3 = new BusinessObjectFactory();
				shipment = factory3.Load<ForwardingShipment>(shipment.PK);
				AssertNotNull(shipment.AWBHeader);
				AssertMaxDbHits(37, factory3);
			}
		}

		static PrintChargesBilledToLocalClientAtDestAsCollect CreatePrintChargesBilledToLocalClientAtDestAsCollect(PrintChargesBilledToLocalClientAtDestAsCollectCollection collection, ZString transportMode, ZString exportCountry, ZString importCountry)
		{
			var setting = collection.AddNew();

			setting.TransportMode = transportMode;
			setting.ExportCountry = exportCountry;
			setting.ImportCountry = importCountry;

			return setting;
		}

		OrgHeader CreateOrg(string code, string fullName, string address1, string countryCode, string closestPort)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.Address1 = address1;
			org.MainAddress.OA_RN_NKCountryCode = countryCode;

			return org;
		}

		GlbCompany CreateCompany(string companyCode, string companyName, string branchCode, string branchName, string homePort, string currency, ZGuid orgProxyPK, ZGuid orgBranchProxyPK = default)
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = companyCode;
			newCompany.GC_Name = companyName;
			newCompany.GC_RN_NKCountryCode = homePort.Substring(0, 2);
			newCompany.GC_OH_OrgProxy = orgProxyPK;
			newCompany.GC_RX_NKLocalCurrency = currency;

			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = branchCode;
			newBranch.GB_BranchName = branchName;
			newBranch.GB_RL_NKHomePort = homePort;
			newBranch.GB_OH_OrgProxy = orgBranchProxyPK.IsEmpty ? orgProxyPK : orgBranchProxyPK;

			return newCompany;
		}

		AccChargeCode CreateAccChargeCode(string code, ZGuid companyPK)
		{
			var accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = code;
			accChargeCode.AC_Desc = code + " Desc";
			accChargeCode.AC_ChargeGroup = code.Substring(0, AccChargeCodeSchema.AC_ChargeGroup.MaxLength);
			accChargeCode.AC_GC = companyPK;

			accChargeCode.SetGLAccountDataForTesting(accChargeCode.GLAccountForTesting);

			return accChargeCode;
		}

		void CreateLineCharge(JobHeader header, ZGuid sellAccountPK, ZDecimal osSellAmount, ZString chargeCode, ZString currencyCode, BusinessObjectFactory otherFactory = default)
		{
			var factory = otherFactory ?? Factory;

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, SQLComparisonOperator.Equal, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.Equal, header.JH_GC);

			var accChargeCode = factory.LoadTop1<AccChargeCode>(query);

			AssertNotNull($"prerequisite: charge code '{chargeCode}' was found", accChargeCode);

			var lineCharge = factory.New<JobCharge>();
			lineCharge.JR_JH = header.PK;
			lineCharge.JR_GE = header.JH_GE;
			lineCharge.JR_GB = header.JH_GB;
			lineCharge.JR_AC = accChargeCode.PK;
			lineCharge.JR_OH_SellAccount = sellAccountPK;
			lineCharge.JR_RX_NKSellCurrency = currencyCode;
			lineCharge.JR_OSSellAmt = osSellAmount;
			lineCharge.JR_Desc = accChargeCode.AC_Desc;
		}

		public void TestRateLineAndOtherChargeCurrencyConversion()
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 34m;
			exchangeRate.RE_RX_NKExCurrency = "INR";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			JobHeaderWithFakeCurrencyConverter jHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode1.AC_Desc = "Blah";

			JobCharge jCharge = NewJobCharge(jHeader, 11.0m, chargeCode1, "Blah", AWBHeader.Shipment.Consignor);

			AWBHeader.Populate();
			AssertEquals(1, AWBHeader.AWBOtherCharges.Count);
			AssertChargeExists(AWBHeader.AWBOtherCharges, 11m, "Blah", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, Core.Constants.AWB.ChargeCodes.AC);

			SetAWBCurrency(jHeader, "INR", AWBHeader.Consol.SendingForwarder);
			AWBHeader.Populate();
			AssertEquals(1, AWBHeader.AWBOtherCharges.Count);
			AssertChargeExists(AWBHeader.AWBOtherCharges, 11m * 34m, "Blah", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, Core.Constants.AWB.ChargeCodes.AC);
			AssertEquals(30m * 34m, AWBHeader.AWBRateLines[0].ER_Total);
		}

		public void TestRateLineAndOtherChargeCurrencyConversion_ForeignCurrency()
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 7.75m;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			JobHeaderWithFakeCurrencyConverter jHeader = Factory.New<JobHeaderWithFakeCurrencyConverter>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			AccChargeCode chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			chargeCode1.AC_Desc = "Blah";

			var jCharge = NewJobCharge(jHeader, 0m, chargeCode1, "Blah", AWBHeader.Consol.SendingForwarder);
			jCharge.JR_OH_SellAccount = jHeader.LocalChargesAddr.Header.PK;
			jCharge.JR_RX_NKSellCurrency = exchangeRate.RE_RX_NKExCurrency;
			jCharge.JR_OSSellAmt = 74.10M;

			SetAWBCurrency(jHeader, "USD", AWBHeader.Consol.SendingForwarder);
			AssertEquals(1, AWBHeader.AWBOtherCharges.Count);
			AssertChargeExists(AWBHeader.AWBOtherCharges, 74.10m, "Blah", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, Core.Constants.AWB.ChargeCodes.AC);
		}

		void SetAWBCurrency(JobHeader job, ZString currency, OrgHeader sellAccount)
		{
			FreightDataRegistry.Instance.SetHAWBCurrencyToFirstCollectInvoiceCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var charge = AddCharge(job, Env.Registry.FreightChargeCode);
			charge.JR_LocalSellAmt = 30m;
			charge.JR_OH_SellAccount = sellAccount.PK;
			charge.JR_AL_ARLine = CreateInvoiceAndLine(currency, new ZDateTime(2007, 1, 1)).PK;
			AWBHeader.Populate();
			AssertEquals("Earliest freight collect invoice currency", currency, AWBHeader.EH_Currency);
		}

		class JobHeaderWithFakeCurrencyConverter : JobHeader
		{
			public JobHeaderWithFakeCurrencyConverter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override CurrencyConverter CurrencyConverter
			{
				get { return CurrencyConverter.New(Factory, ZDateTime.Now, Enterprise.ZArchitecture.Core.ExchangeRateType.Sell, 0); }
			}

			public OrgHeader Agent
			{
				get { return AgentCollect; }
			}

			public OrgHeader BillTo
			{
				get { return LocalCharges; }
			}

			protected override bool IsChargesCollectionLoaded
			{
				get { return false; }
			}
		}

		void AssertChargeExists(ExportAWBOtherChargesCollection otherCharges, decimal expectedAmount, string expectedDescription, string expectedPrepaidCollect, string expectedChargeCode)
		{
			bool found = false;

			foreach (ExportAWBOtherCharges charge in otherCharges)
			{
				if (charge.EO_Amount == expectedAmount &&
					charge.EO_ChargeDescription == expectedDescription &&
					charge.EO_PPDCLT == expectedPrepaidCollect &&
					charge.EO_ChargeCode == expectedChargeCode)
				{
					found = true;
					break;
				}
			}

			Assert(found);
		}

		public void TestPopulateOtherChargesWhenThereAreMoreThanMaxOtherChargesCount()
		{
			Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;

			var jHeader = Factory.NewJobForTesting<JobHeader>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			chargeCode.AC_Desc = "Test Charge";

			for (int i = 0; i < AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB; i++)
			{
				NewJobCharge(jHeader, (ZDecimal)(i + 1), chargeCode, "Desc" + i, AWBHeader.Shipment.Consignor);

				AWBHeader.Populate();

				AssertEquals(i + 1, AWBHeader.AWBOtherCharges.Count);

				var expectedAmounts = new List<ZDecimal>();

				for (int j = 0; j <= i; j++)
				{
					expectedAmounts.Add(i - j + 1);
				}

				AssertContainsExactElementsInAnyOrder(expectedAmounts,
					AWBHeader.AWBOtherCharges.Cast<ExportAWBOtherCharges>().Select(charge => charge.EO_Amount));

				AssertEquals("Test Charge", AWBHeader.AWBOtherCharges[i].EO_ChargeDescription.Trim());
				AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, AWBHeader.AWBOtherCharges[i].EO_PPDCLT);
				AssertEquals(Core.Constants.AWB.ChargeCodes.DB, AWBHeader.AWBOtherCharges[i].EO_ChargeCode);
			}

			for (int i = 0; i < 5; i++)
			{
				NewJobCharge(jHeader, 10M, chargeCode, "New Desc", AWBHeader.Consol.SendingForwarder);
			}

			AWBHeader.Populate();
			var lastOtherCharges = AWBHeader.AWBOtherCharges[AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB - 1];
			AssertEquals("Should not exceed MaxOtherChargesCount", AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB, AWBHeader.AWBOtherCharges.Count);
			AssertEquals("Other Misc Charges", lastOtherCharges.EO_ChargeDescription);
			AssertEquals(Core.Constants.AWB.ChargeCodes.MB, lastOtherCharges.EO_ChargeCode);
			AssertEquals("Amount should be accumulated", (ZDecimal)(11 + AWBHeader.AWBOtherCharges.MaxOtherChargesWithMissingChargeCodeThatCouldFitOnPrintedAWB), lastOtherCharges.EO_Amount);
		}

		public void TestPopulateOtherChargesWhenThereAreMoreThanMaxOtherChargesCount_ButManyZero()
		{
			var jHeader = Factory.NewJobForTesting<JobHeader>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			chargeCode.AC_Desc = "Test Charge";

			for (int i = 0; i < AWBHeader.AWBOtherCharges.MaxOtherChargesThatCouldFitOnPrintedAWB + 10; i++)
			{
				NewJobCharge(jHeader, 0m, chargeCode, "Desc" + i, AWBHeader.Shipment.Consignor);
			}

			NewJobCharge(jHeader, 50m, chargeCode, "Real", AWBHeader.Shipment.Consignor);

			AWBHeader.Populate();
			AssertEquals(1, AWBHeader.AWBOtherCharges.Count);
			AssertEquals(50m, AWBHeader.AWBOtherCharges[0].EO_Amount);
			AssertEquals("Test Charge", AWBHeader.AWBOtherCharges[0].EO_ChargeDescription.Trim());
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, AWBHeader.AWBOtherCharges[0].EO_PPDCLT);
			AssertEquals(Core.Constants.AWB.ChargeCodes.DB, AWBHeader.AWBOtherCharges[0].EO_ChargeCode);
		}

		JobCharge NewJobCharge(JobHeader jHeader, ZDecimal localSellAmt, AccChargeCode chargeCode, ZString desc, OrgHeader sellAccount)
		{
			return NewJobCharge(jHeader, localSellAmt, chargeCode, desc, sellAccount, 0);
		}

		JobCharge NewJobCharge(JobHeader jHeader, ZDecimal localSellAmt, AccChargeCode chargeCode, ZString desc, OrgHeader sellAccount, ZDecimal taxAmount)
		{
			JobCharge jCharge = Factory.New<JobCharge>();
			jCharge.JR_JH = jHeader.PK;
			jCharge.JR_AC = chargeCode.PK;
			jCharge.JR_Desc = desc;
			jCharge.JR_OH_SellAccount = sellAccount.PK;
			jCharge.JR_LocalSellAmt = localSellAmt;

			AccTaxRate accTaxRate = Factory.New<AccTaxRate>();
			accTaxRate.AT_Code = "TEST";
			accTaxRate.AT_Type = "RAT";
			accTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			accTaxRate.SetRateNumerator_ForTestOnly(10);
			jCharge.JR_AT_SellGSTRate = accTaxRate.PK;

			return jCharge;
		}

		#endregion

		public void TestRateLineRateChargeOrDiscount()
		{
			AssertEquals("got the correct rate charge from shipment", 0m, AWBHeader.RateLineRateChargeOrDiscount);
		}

		public void TestPopulateIssuedBy()
		{
			OrgHeader companyOrganisation = GlbCompany.CurrentCompany.OrgProxy;
			if (companyOrganisation == null)
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.Factory.New(typeof(OrgHeader)).PK;
				companyOrganisation = GlbCompany.CurrentCompany.OrgProxy;
				companyOrganisation.MainAddress.OA_Address1 = "10 HUTCHESON STREET";
				companyOrganisation.MainAddress.OA_Address2 = "ALBION  QLD";
				companyOrganisation.MainAddress.OA_City = "";
				companyOrganisation.MainAddress.OA_PostCode = "4010";
				companyOrganisation.OH_RL_NKClosestPort = "AUSYD";
				companyOrganisation.MainAddress.OA_State = "";
			}

			OrgHeader organisation = GlbBranch.CurrentBranch.OrgProxy;
			if (organisation == null)
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.Factory.New(typeof(OrgHeader)).PK;
				organisation = GlbBranch.CurrentBranch.OrgProxy;
			}

			AssertNotNull("Precondition : Organisation must not be null", organisation);
			organisation.MainAddress.OA_Address1 = "10 HUTCHESON STREET";
			organisation.MainAddress.OA_Address2 = "ALBION  QLD";
			organisation.MainAddress.OA_City = "";
			organisation.MainAddress.OA_PostCode = "4010";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.MainAddress.OA_State = "";

			AWBHeader.PopulateIssuedBy();

			BusinessObjectFactory fat = new BusinessObjectFactory();
			RefUNLOCO loco = fat.New<RefUNLOCO>();
			loco.RL_Code = "COD";
			GlbBranch newBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			OrgHeader newHeader = fat.New<OrgHeader>();
			newBranch.GB_OH_OrgProxy = newHeader.PK;

			newHeader.OH_FullName = "fulname";
			newHeader.MainAddress.OA_Address1 = "sck";
			newHeader.MainAddress.OA_Address2 = "QLD";
			newHeader.MainAddress.OA_City = "";
			newHeader.MainAddress.OA_PostCode = "4";
			newHeader.OH_RL_NKClosestPort = "AUS";
			newHeader.MainAddress.OA_State = "nsw";
			newHeader.OH_Code = "anan";
			newBranch.GB_Phone = "0415";

			newBranch.GB_RL_NKHomePort = loco.RL_Code;
			AWBHeader.Shipment.JS_RL_NKOrigin = loco.RL_Code;

			fat.Save();
			AWBHeader.Populate();

			AssertEquals("NAME FROM BRANCH", GlbBranch.CurrentBranch.OrgProxy.OH_FullName.ToUpper(), AWBHeader.EH_IssuingAgentName);
			AssertEquals("10 HUTCHESON STREET", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ALBION  QLD, 4010, AUSTRALIA", AWBHeader.EH_IssuingAgentAddress2);

			FreightDataRegistry.Instance.IssuedByDetailsUseShipmentOrigin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.Populate();

			AssertEquals("NAME FROM BRANCH", "fulname".ToUpper(), AWBHeader.EH_IssuingAgentName);
			AssertEquals("SCK", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("QLD, NSW, 4", AWBHeader.EH_IssuingAgentAddress2);

			newHeader.MainAddress.OA_Language = "ZH-CN";
			var englishAddress = newHeader.MainAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = "EN";
			englishAddress.OTA_Address1 = "Address1";
			englishAddress.OTA_Address2 = "Address2";
			englishAddress.OTA_City = "City";
			englishAddress.OTA_PostCode = "PostCode";
			englishAddress.OTA_State = "State";
			fat.Save();

			AWBHeader.Populate();
			AssertEquals("ADDRESS1", AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals("ADDRESS2, CITY, STATE, POSTCODE", AWBHeader.EH_IssuingAgentAddress2);

			ForwardingConfigurationRegistry.Instance.PrintForwarderBranchDetailsHAWBIssuedBySection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AWBHeader.Populate();
			AssertEquals("EMPTY NAME", ZString.Empty, AWBHeader.EH_IssuingAgentName);
			AssertEquals(ZString.Empty, AWBHeader.EH_IssuingAgentAddress1);
			AssertEquals(ZString.Empty, AWBHeader.EH_IssuingAgentAddress2);
		}

		public void TestOtherPPDCol()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			AWBHeader.Populate();
			AssertEquals("EXW - Origin charges Collect", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, AWBHeader.OtherPPDCOL);

			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			AWBHeader.Populate();
			AssertEquals("FOB - Origin Charges Prepaid", ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, AWBHeader.OtherPPDCOL);
		}

		public void TestGoodsDescription()
		{
			AWBHeader.Shipment.JS_GoodsDescription = "Short Goods Desc";
			AssertEquals("Short Goods Desc", AWBHeader.GoodsDescription);

			AWBHeader.Shipment.JS_MarksAndNumbers = "Shipment Marks and Numbers";
			AssertEquals("Short Goods Desc\r\nShipment Marks and Numbers", AWBHeader.GoodsDescription);

			StmNote note = AWBHeader.Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			note.ST_NoteText = "Detailed Goods Description";
			AssertEquals("Detailed Goods Description\r\nShipment Marks and Numbers", AWBHeader.GoodsDescription);
		}

		public void TestPopulateShortGoodsDescriptionforFHL()
		{
			AWBHeader.Shipment.JS_GoodsDescription = "Short Goods";
			AWBHeader.PopulateShortGoodsDescriptionforFHL();
			AssertEquals("Short Goods", AWBHeader.EH_ManifestDescriptionOfGoods);

			AWBHeader.Shipment.JS_GoodsDescription = "Short Goods Description";
			AWBHeader.PopulateShortGoodsDescriptionforFHL();
			AssertEquals("Short Goods Des", AWBHeader.EH_ManifestDescriptionOfGoods);

			AWBHeader.Shipment.JS_GoodsDescription = string.Empty;
			AWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "Long Goods";
			AWBHeader.PopulateShortGoodsDescriptionforFHL();
			AssertEquals("Long Goods", AWBHeader.EH_ManifestDescriptionOfGoods);

			AWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "Long Description which is more than 15 charactors";
			AWBHeader.PopulateShortGoodsDescriptionforFHL();
			AssertEquals("Long Descriptio", AWBHeader.EH_ManifestDescriptionOfGoods);
		}

		public void TestNatureAndQtyOfGoods()
		{
			AWBHeader.NatureAndQtyOfGoods = "1Some text that\n2Spans more\n3Than 1 line\n4on the form\n5Some text that\n6Spans more\n7Than 1 line\n8on the form\n9Some text that\n10Spans more\n11Than 1 line\n12on the form\n13th line";
			AssertEquals("1Some text that", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("2Spans more", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("3Than 1 line", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("4on the form", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("5Some text that", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("6Spans more", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("7Than 1 line", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("8on the form", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("9Some text that", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("10Spans more", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("11Than 1 line", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("12on the form13th line", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AWBHeader.NatureAndQtyOfGoods = AWBHeader.TextToNatureAndQtyOfGoodsLines("1Some text that is a bit longer than the max length for a line\n2Spans more\n3Than 1 line\n4on the form\n5Some text that\n6Spans more\n7Than 1 line\n8on the form\n9Some text that\n10Spans more\n11Than 1 line\n12on the form\n13th line");
			AssertEquals("1Some text that is a bit longer", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("than the max length for a line", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("2Spans more", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("3Than 1 line", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("4on the form", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("5Some text that", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("6Spans more", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("7Than 1 line", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("8on the form", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("9Some text that", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("10Spans more", AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals("11Than 1 line12on the form13t", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);

			AWBHeader.NatureAndQtyOfGoods = AWBHeader.TextToNatureAndQtyOfGoodsLines("ABIGAS1ABIGAS2 ABIGAS3 ABIGAS4ABIGAS5\nABIGAS1 ABIGAS2ABIGAS3 ABIGAS4ABIGAS5\nABIGAS1ABIGAS2ABIGAS3ABIGAS4ABIGAS5\nABIGAS1ABIGAS2 ABIGAS3ABIGAS4 ABIGAS5");
			AssertEquals("ABIGAS1ABIGAS2 ABIGAS3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS4ABIGAS5", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS1 ABIGAS2ABIGAS3", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS4ABIGAS5", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS1ABIGAS2ABIGAS3ABIGAS", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("4ABIGAS5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS1ABIGAS2", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("ABIGAS3ABIGAS4 ABIGAS5", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals(ZString.Empty, AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals(ZString.Empty, AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals(ZString.Empty, AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription);
			AssertEquals(ZString.Empty, AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods1()
		{
			AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "1";
			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "2";
			AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "3";
			AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "4";
			AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "5";
			AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "6";
			AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "7";
			AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "8";
			AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "9";
			AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription = "10";
			AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription = "11";
			AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "12";

			AssertEquals("1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12", AWBHeader.NatureAndQtyOfGoods);
		}

		public void TestAllowRecogniseAndUpdateNatureAndQtyOfGoodsTypeFromText()
		{
			AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "VOL 1.000 M3";
			AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "VOL 1.000 M3";
			AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "VOL 1.000 M3";
			AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "VOL 1.000 M3";
			AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "DIMS 1x2x3 IN x 4";
			AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsDescription = "AAA";
			AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription = "AAA";

			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine11.NatureAndQtyOfGoodsType);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsType);
		}

		public void TestHandlingInformation()
		{
			StmNote note = AWBHeader.Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteDataAsText = "SHIPMENT NOTE";
			AWBHeader.Populate();
			AssertEquals("SHIPMENT NOTE", AWBHeader.EH_HandlingInformation);

			note.ST_NoteDataAsText = new string('X', 500);
			AWBHeader.Populate();
			AssertEquals(new string('X', 65 * 3), AWBHeader.EH_HandlingInformation);

			note.Delete();

			var consignee = Factory.New<OrgHeader>();
			var consigneeNote = consignee.Notes.AddNew();
			consigneeNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			consigneeNote.ST_NoteDataAsText = "CONSIGNEE NOTE";
			AWBHeader.Shipment.ConsigneePK = consignee.PK;
			AWBHeader.Populate();
			AssertEquals("CONSIGNEE NOTE", AWBHeader.EH_HandlingInformation);

			consigneeNote.Delete();
			AWBHeader.Populate();
			AssertEquals("", AWBHeader.EH_HandlingInformation);

			Transport transport = AWBHeader.Shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "QW132";
			transport.JW_ETD = new ZDateTime(2007, 8, 1);

			transport = AWBHeader.Shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "QW232";
			transport.JW_ETD = new ZDateTime(2007, 8, 2);

			AssertEquals("", AWBHeader.HandlingInformationForTest);

			transport = AWBHeader.Shipment.Transports.AddNew();
			transport.JW_VoyageFlight = "QW332";
			transport.JW_ETD = new ZDateTime(2007, 8, 3);

			AssertEquals("QW332/3", AWBHeader.HandlingInformationForTest);

			transport.JW_VoyageFlight = "";
			transport.JW_ETD = new ZDateTime(2007, 8, 3);
			AssertEquals("/3", AWBHeader.HandlingInformationForTest);

			transport.JW_VoyageFlight = "QF123";
			transport.JW_ETD = ZDateTime.Empty;
			AssertEquals("QF123", AWBHeader.HandlingInformationForTest);
		}

		public void TestDepartureFlight1()
		{
			// should always get the first flight from the Consol
			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Shipment.Transports.RemoveAndDeleteAll();
			AssertEquals("AWBHeader.Consol.Transports.Count", 1, AWBHeader.Consol.Transports.Count);

			Transport consolFL1 = AWBHeader.Consol.Transports[0];
			consolFL1.JW_TransportMode = "";
			AssertNull("DepartureFlight1", AWBHeader.DepartureFlight1);

			consolFL1.JW_TransportMode = Core.Constants.TransportModes.Air;
			consolFL1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			AssertNotNull("DepartureFlight1", AWBHeader.DepartureFlight1);

			consolFL1.JW_RL_NKLoadPort = "SGSIN";
			consolFL1.JW_RL_NKDiscPort = "USNYC";
			AssertEquals("SGSIN", AWBHeader.DepartureFlight1.JW_RL_NKLoadPort);

			Transport shipmentFL1 = AWBHeader.Shipment.Transports.AddNew();
			shipmentFL1.JW_TransportMode = Core.Constants.TransportModes.Air;
			shipmentFL1.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			shipmentFL1.JW_RL_NKLoadPort = "USLAX";
			AssertEquals("SGSIN", AWBHeader.DepartureFlight1.JW_RL_NKLoadPort);

			AWBHeader.Populate();
			AssertEquals("NYC", AWBHeader.EH_To1st);
		}

		public void TestEH_AgentApprovedExporterNumber()
		{
			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_Code = "ABC";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var addressCountryData = sendingForwarder.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = sendingForwarder.PK;
				addressCountryData.OV_OA_ApprovedLocation = sendingForwarder.MainAddress.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalNumber = "RA12345";
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				AWBHeader.EH_ParentID = shipment.PK;
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;

				AssertEquals("RA12345", AWBHeader.EH_AgentApprovedExporterNumber);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				AssertEquals("RA12345", AWBHeader.EH_AgentApprovedExporterNumber);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertEquals("RA12345", AWBHeader.EH_AgentApprovedExporterNumber);

				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertEquals("Approval has expired", string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;
				AssertEquals(string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalNumber = "RA12345";
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				AssertEquals("the sending forwarder has NOT been set.", string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;
				sendingForwarder.MainAddress.KnownShipperDetails.RemoveFromRelationship(addressCountryData);
				AssertEquals("There is NO known shipper details", string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;
				sendingForwarder.MainAddress.KnownShipperDetails.Add(addressCountryData);
				var childShipment1 = shipment.CoLoadShipments.AddNew();
				childShipment1.JS_InspectionTypeCode = "APP";
				var childShipment2 = shipment.CoLoadShipments.AddNew();
				childShipment2.JS_InspectionTypeCode = "UNK";

				AssertEquals("one of related Shipments of the CLD Master Shipment is UNK", "UNK", AWBHeader.EH_AgentApprovedExporterNumber);

				childShipment2.JS_InspectionTypeCode = "APP";
				AssertEquals("RA12345", AWBHeader.EH_AgentApprovedExporterNumber);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				shipment.CoLoadShipments.RemoveAll();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				AWBHeader.EH_ParentID = shipment.PK;
				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;

				AssertEquals("NOT for Hong Kong.", string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<ForwardingShipment>();
				AWBHeader.EH_ParentID = shipment.PK;
				shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = sendingForwarder.MainAddress.PK;

				AssertEquals("The registry item: EnableSupplyChainSecurity_HK is false.", string.Empty, AWBHeader.EH_AgentApprovedExporterNumber);
			}
		}

		public void TestDepartureFlight2()
		{
			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Shipment.Transports.RemoveAndDeleteAll();
			AssertNull(AWBHeader.DepartureFlight2);

			Transport fL2 = AWBHeader.Consol.Transports.AddNew();
			fL2.JW_TransportMode = Core.Constants.TransportModes.Air;
			fL2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			fL2.JW_RL_NKLoadPort = "SGSIN";

			AssertEquals("SGSIN", AWBHeader.DepartureFlight2.JW_RL_NKLoadPort);

			fL2 = AWBHeader.Shipment.Transports.AddNew();
			fL2.JW_TransportMode = Core.Constants.TransportModes.Air;
			fL2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			fL2.JW_RL_NKLoadPort = "USLAX";
			fL2.JW_RL_NKDiscPort = "USNYC";
			AssertEquals("USLAX", AWBHeader.DepartureFlight2.JW_RL_NKLoadPort);
			AWBHeader.Populate();
			AssertEquals("NYC", AWBHeader.EH_To2nd);
		}

		public void TestDepartureFlight3()
		{
			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Shipment.Transports.RemoveAndDeleteAll();
			AssertNull(AWBHeader.DepartureFlight3);

			Transport fL3 = AWBHeader.Consol.Transports.AddNew();
			fL3.JW_TransportMode = Core.Constants.TransportModes.Air;
			fL3.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
			fL3.JW_RL_NKLoadPort = "SGSIN";

			AssertEquals("SGSIN", AWBHeader.DepartureFlight3.JW_RL_NKLoadPort);

			fL3 = AWBHeader.Shipment.Transports.AddNew();
			fL3.JW_TransportMode = Core.Constants.TransportModes.Air;
			fL3.JW_TransportType = Core.Constants.TransportPlanningType.Flight3;
			fL3.JW_RL_NKLoadPort = "USLAX";
			fL3.JW_RL_NKDiscPort = "USNYC";
			AssertEquals("USLAX", AWBHeader.DepartureFlight3.JW_RL_NKLoadPort);
			AWBHeader.Populate();
			AssertEquals("NYC", AWBHeader.EH_To3rd);

			AWBHeader.Shipment.Transports.RemoveAndDeleteAll();

			// if there is a flight 2 on the shipment and a flight 3 on the consol then flight 3 on the consol should be ignored
			Transport fL2 = AWBHeader.Shipment.Transports.AddNew();
			fL2.JW_TransportMode = Core.Constants.TransportModes.Air;
			fL2.JW_TransportType = Core.Constants.TransportPlanningType.Flight2;
			fL2.JW_RL_NKLoadPort = "USLAX";
			AssertNull(AWBHeader.DepartureFlight3);
		}

		public void TestOptionalShippingInfo()
		{
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			AWBHeader.Populate();
			AssertEquals("TERMS: EXW", AWBHeader.EH_OptionalShippingInformation);

			AWBHeader.Shipment.JS_INCO = "";
			AWBHeader.Populate();
			AssertEquals("TERMS:", AWBHeader.EH_OptionalShippingInformation);
		}

		public void TestOriginCode()
		{
			AWBHeader.Shipment.JS_RL_NKOrigin = ZString.Empty;
			AssertEquals("Prerequisite", "AUMEL", AWBHeader.Consol.JK_RL_NKLoadPort);
			AWBHeader.Populate();
			AssertEquals("Origin code comes from the consol load port", "MEL", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Consol.JK_RL_NKLoadPort = ZString.Empty;
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Populate();
			AssertEquals("Origin code comes from transport", "SYD", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Shipment.JS_RL_NKOrigin = "KRWJU";
			AWBHeader.Populate();
			AssertEquals("Origin code comes from shipment's origin", "WJU", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Shipment.JS_RL_NKOrigin = "SGSIN";
			AWBHeader.Populate();
			AssertEquals("Origin code comes from shipment's origin", "SIN", AWBHeader.EH_AWBOriginCode);

			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Shipment.JS_RL_NKOrigin = ZString.Empty;
			AWBHeader.Populate();
			AssertEquals("Origin code", ZString.Empty, AWBHeader.EH_AWBOriginCode);
		}

		public void TestOriginCode_FSA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.SeaAir;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "KRWJU";

			shipment.AWBHeader.Populate();
			AssertEquals("Origin code comes from first Air consol's load port", "MEL", shipment.AWBHeader.EH_AWBOriginCode);
		}

		public void TestAirportOfDeparture()
		{
			AWBHeader.Shipment.JS_RL_NKOrigin = ZString.Empty;
			AssertEquals("Prerequisite", "AUMEL", AWBHeader.Consol.JK_RL_NKLoadPort);
			AWBHeader.Populate();
			AssertEquals("AirportOfDeparture comes from the consol load port", "Melbourne", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Consol.JK_RL_NKLoadPort = ZString.Empty;
			AWBHeader.Consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Populate();
			AssertEquals("AirportOfDeparture comes from transport", "Sydney", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Shipment.JS_RL_NKOrigin = "SGSIN";
			AWBHeader.Populate();
			AssertEquals("AirportOfDeparture comes from shipment's load port", "Singapore", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Shipment.JS_RL_NKOrigin = "HKHKG";
			AWBHeader.Populate();
			AssertEquals("AirportOfDeparture comes from shipment's load port", "Hong Kong", AWBHeader.EH_AirportOfDepartureAndRequestRouteText);

			AWBHeader.Consol.Transports.RemoveAndDeleteAll();
			AWBHeader.Shipment.JS_RL_NKOrigin = ZString.Empty;
			AWBHeader.Populate();
			AssertEquals("AirportOfDeparture", ZString.Empty, AWBHeader.EH_AirportOfDepartureAndRequestRouteText);
		}

		public void TestAirportOfDeparture_FSA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.SeaAir;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "KRWJU";

			shipment.AWBHeader.Populate();
			AssertEquals("AirportOfDeparture comes from first AIR consol's load port", "Melbourne", shipment.AWBHeader.EH_AirportOfDepartureAndRequestRouteText);
		}

		public void TestAWBDestinationCode_FSA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.SeaAir;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "KRWJU";

			var consol4 = shipment.Consols.AddNew();
			consol4.JK_TransportMode = TransportModes.Sea;
			consol4.JK_RL_NKLoadPort = "KRWJU";
			consol4.JK_RL_NKDischargePort = "USSEA";

			var consol5 = shipment.Consols.AddNew();
			consol5.JK_TransportMode = TransportModes.Air;
			consol5.JK_RL_NKLoadPort = "USSEA";
			consol5.JK_RL_NKDischargePort = "USLAX";

			var consol6 = shipment.Consols.AddNew();
			consol6.JK_TransportMode = TransportModes.Air;
			consol6.JK_RL_NKLoadPort = "CNBJS";
			consol6.JK_RL_NKDischargePort = "CNSHA";

			shipment.AWBHeader.Populate();
			AssertEquals("AWBDestinationCode comes from last consecutive Air consol's discharge port", "WJU", shipment.AWBHeader.EH_AirportOfDestinationCode);
		}

		public void TestAWBDestinationText_FSA()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.SeaAir;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = TransportModes.Air;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			var consol3 = shipment.Consols.AddNew();
			consol3.JK_TransportMode = TransportModes.Air;
			consol3.JK_RL_NKLoadPort = "SGSIN";
			consol3.JK_RL_NKDischargePort = "KRWJU";

			var consol4 = shipment.Consols.AddNew();
			consol4.JK_TransportMode = TransportModes.Sea;
			consol4.JK_RL_NKLoadPort = "KRWJU";
			consol4.JK_RL_NKDischargePort = "USSEA";

			var consol5 = shipment.Consols.AddNew();
			consol5.JK_TransportMode = TransportModes.Air;
			consol5.JK_RL_NKLoadPort = "USSEA";
			consol5.JK_RL_NKDischargePort = "USLAX";

			var consol6 = shipment.Consols.AddNew();
			consol6.JK_TransportMode = TransportModes.Air;
			consol6.JK_RL_NKLoadPort = "CNBJS";
			consol6.JK_RL_NKDischargePort = "CNSHA";

			shipment.AWBHeader.Populate();
			AssertEquals("AWBDestinationText comes from last consecutive Air consol's discharge port", "Wonju", shipment.AWBHeader.EH_AirportOfDestinationText);
		}

		#region Nature And Quantity of Goods

		#region Default Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoVolume_WithGoodsDescription()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;

			AWBHeader.Shipment.JS_GoodsDescription = "Short Desc.";
			AWBHeader.Populate();
			AssertEquals("Short Desc.", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);

			AWBHeader.Shipment.JS_GoodsDescription = string.Empty;
			AWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "Some Relatively Long Description\nWith a new line";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("Some Relatively Long Description", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("With a new line", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_LargeDescriptionNoSpaces()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;

			AWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "Longlonglonglonglonglonglonglonglongotcha\nDescription";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("Longlonglonglonglonglonglonglonglon", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("gotcha", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("Description", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoVolume()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;

			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_Volume()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_Default_NoDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_Default_EmptyDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			PackLine outerPackLine = AWBHeader.Shipment.OuterPackLines.AddNew();
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_Default_Dimensions()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_Default_Dimensions_DimensionsDontFit()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 8, "CM");
			AddOuterPackLine(6, 7, 8, 7, "CM");
			AddOuterPackLine(6, 7, 8, 6, "CM");
			AddOuterPackLine(6, 7, 8, 5, "CM");
			AddOuterPackLine(6, 7, 8, 4, "CM");
			AddOuterPackLine(6, 7, 8, 3, "CM");
			AddOuterPackLine(6, 7, 8, 2, "CM");
			AddOuterPackLine(6, 7, 8, 1, "CM");
			AddOuterPackLine(6, 7, 7, 1, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9\nDIMS 6x7x8 CM x 8\nDIMS 6x7x8 CM x 7\nDIMS 6x7x8 CM x 6\nDIMS 6x7x8 CM x 5\nDIMS 6x7x8 CM x 4\nDIMS 6x7x8 CM x 3\nDIMS 6x7x8 CM x 2\nDIMS 6x7x8 CM x 1\nDIMS 6x7x7 CM x 1", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9\nDIMS 6x7x8 CM x 8\nDIMS 6x7x8 CM x 7\nDIMS 6x7x8 CM x 6\nDIMS 6x7x8 CM x 5\nDIMS 6x7x8 CM x 4\nDIMS 6x7x8 CM x 3\nDIMS 6x7x8 CM x 2\nDIMS 6x7x8 CM x 1\nDIMS 6x7x7 CM x 1\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_ALL_DimensionsDontFit_VolumeAndDimensionForFollowOnPage()
		{
			AWBHeader.Consol.JK_AgentType = Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Constants.AWB.Dimensions.ALL;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			var goodsDescription = AWBHeader.Shipment.Notes.AddNew();
			goodsDescription.ST_Description = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
			goodsDescription.ST_NoteText = @"Spicy jalapeno bacon ipsum dolor amet alcatra tri-tip hamburger, short ribs frankfurter pancetta chicken. Porchetta filet mignon doner, alcatra rump salami tri-tip pork sausage tenderloin. Bacon andouille filet mignon pancetta. Swine beef doner pork loin pork chop jowl fatback ham hock buffalo turkey hamburger boudin sirloin porchetta.";

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("Dimensions should be included", "DIMS 6x7x8 CM x 9\nVOL 10.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQuantityOfGoodsVolume_OverriddenDecimalNumber()
		{
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_AWBWeight = collection.AddNew();
			defaultNumberOfDecimals_AWBWeight.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_AWBWeight.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_AWBWeight.NumberOfDecimals = 1;
			defaultNumberOfDecimals_AWBWeight.RoundingMode = RoundingModes.Up;
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			FreightConfigurationRegistry.Instance.UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AWBHeader.Populate();
			AssertEquals("VOL 10.0 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_NoDimensionsAvailable()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.NDA;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestNatureAndQtyOfGoods_NoDimensionsAvailable_WhenPackageCountIsZero()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 0, "M");
			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		#endregion

		#region VOL selected - Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_VOL_NoVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.M3;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 12;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(1, 2, 3, 4, "M");
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_VOL_VolumeSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.M3;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(1, 2, 3, 4, "M");
			AWBHeader.Populate();

			AssertEquals("VOL 10.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 10.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		#endregion

		#region PKS selected - Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoVolume_WithGoodsDescription()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "Some Relatively Long Description\nWith a new line";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("Some Relatively Long Description", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("With a new line", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_Volume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_OuterPacks = 0;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_NoDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_OuterPacks = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_EmptyDimensions_NoOfOuterPacksSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_OuterPacks = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			PackLine outerPackLine = AWBHeader.Shipment.OuterPackLines.AddNew();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_Dimensions()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_PKS_Dimensions_DimensionsDontFit()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.PKS;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 10M;
			AWBHeader.Shipment.JS_UnitOfVolume = "M3";
			AWBHeader.Shipment.JS_TotalPackageCount = 20;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();

			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 8, "CM");
			AddOuterPackLine(6, 7, 8, 7, "CM");
			AddOuterPackLine(6, 7, 8, 6, "CM");
			AddOuterPackLine(6, 7, 8, 5, "CM");
			AddOuterPackLine(6, 7, 8, 4, "CM");
			AddOuterPackLine(6, 7, 8, 3, "CM");
			AddOuterPackLine(6, 7, 8, 2, "CM");
			AddOuterPackLine(6, 7, 8, 1, "CM");
			AddOuterPackLine(6, 7, 7, 1, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9\nDIMS 6x7x8 CM x 8\nDIMS 6x7x8 CM x 7\nDIMS 6x7x8 CM x 6\nDIMS 6x7x8 CM x 5\nDIMS 6x7x8 CM x 4\nDIMS 6x7x8 CM x 3\nDIMS 6x7x8 CM x 2\nDIMS 6x7x8 CM x 1\nDIMS 6x7x7 CM x 1", AWBHeader.VolumeAndDimensionForFollowOnPage);

			AddOuterPackLine(6, 7, 8, 9, "CM");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 9", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 8", AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 7", AWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 6", AWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 5", AWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 4", AWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 3", AWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 2", AWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 6x7x8 CM x 1", AWBHeader.AWBRateLine10.NatureAndQtyOfGoodsDescription);
			AssertEquals("20 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8\nDIMS 6x7x8 CM x 9\nDIMS 6x7x8 CM x 8\nDIMS 6x7x8 CM x 7\nDIMS 6x7x8 CM x 6\nDIMS 6x7x8 CM x 5\nDIMS 6x7x8 CM x 4\nDIMS 6x7x8 CM x 3\nDIMS 6x7x8 CM x 2\nDIMS 6x7x8 CM x 1\nDIMS 6x7x7 CM x 1\nDIMS 6x7x8 CM x 9", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		#endregion

		#region ALL selected - Behaviour of Nature and Qty of Goods

		public void TestNatureAndQtyOfGoods_ALL_NoVolumeButDimensions()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_TotalPackageCount = 8;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(5, 6, 7, 8, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("8 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 500x600x700 CM x 8", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_ALL_NoDimensionsButVolume()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 999M;
			AWBHeader.Shipment.JS_TotalPackageCount = 12;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals("VOL 999.000 M3", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("12 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 999.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_ALL_DimensionsAndVolumeSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 89M;
			AWBHeader.Shipment.JS_TotalPackageCount = 200;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AddOuterPackLine(1, 2, 3, 20, "M");
			AWBHeader.Populate();

			AssertEquals("DIMS 100x200x300 CM x 20", AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals("VOL 89.000 M3", AWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription);
			AssertEquals("200 SLAC", AWBHeader.AWBRateLine12.NatureAndQtyOfGoodsDescription);
			AssertEquals("DIMS 100x200x300 CM x 20\nVOL 89.000 M3", AWBHeader.VolumeAndDimensionForFollowOnPage);
		}

		public void TestNatureAndQtyOfGoods_ALL_DimensionsAndVolumeAreNotSpecified()
		{
			AWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Direct;
			AWBHeader.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			AWBHeader.Shipment.JS_GoodsDescription = "";
			AWBHeader.Shipment.JS_ActualVolume = 0M;
			AWBHeader.Shipment.JS_OuterPacks = 12;
			AWBHeader.Shipment.OuterPackLines.RemoveAndDeleteAll();
			AWBHeader.Populate();

			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
			AssertEquals(ExportAWBHeader.Constants.NoDimensionsAvailable, AWBHeader.VolumeAndDimensionForFollowOnPage);
		}
		#endregion

		void AddOuterPackLine(int length, int width, int height, int packageCount, string unitOfDimension)
		{
			PackLine outerPackLine = AWBHeader.Shipment.OuterPackLines.AddNew();

			outerPackLine.JL_Length = length;
			outerPackLine.JL_Width = width;
			outerPackLine.JL_Height = height;
			outerPackLine.JL_PackageCount = packageCount;
			outerPackLine.JL_UnitOfDimension = unitOfDimension;
		}

		public void TestExportStatementIsAddedToNatureAndQtyOfGoods()
		{
			const string statement = "STATEMENT FOR TESTING";

			SetUpForDeparturePort("AUSYD");

			ExportStatementSetting.UseOnHawb = true;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Shipment.DocsAndCartage.JP_ExportStatement = ExportStatementSetting.Code;
			AWBHeader.Populate();
			AssertContains("NatureAndQtyOfGoods", statement, AWBHeader.NatureAndQtyOfGoods);

			ExportStatementSetting.UseOnHawb = false;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Populate();
			AssertNotContains("NatureAndQtyOfGoods", statement, AWBHeader.NatureAndQtyOfGoods);

			ExportStatementSetting.UseOnHawb = true;
			UpdateCountryStatementSettingsToRegistry();
			AWBHeader.Shipment.DocsAndCartage.JP_ExportStatement = "";
			AWBHeader.Populate();
			AssertNotContains("NatureAndQtyOfGoods", statement, AWBHeader.NatureAndQtyOfGoods);

			AWBHeader.Shipment.DocsAndCartage.JP_ExportStatement = ExportStatementSetting.Code;
			AWBHeader.Populate();
			AssertContains("NatureAndQtyOfGoods", statement, AWBHeader.NatureAndQtyOfGoods);
		}

		#endregion

		public void TestIsTaxAutoCalculated()
		{
			AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
			AWBHeader.Shipment.JS_RL_NKDestination = "AUBNE";
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Shipment.JS_OverrideWaybillDefaults = false;
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			AWBHeader.Shipment.JS_RL_NKDestination = "USCHI";

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AWBHeader.IsTaxAutoCalculated);

			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = true;
			FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AWBHeader.IsTaxAutoCalculated);
		}

		public void TestDefaultingCustomsValue_ControlledByRegistry()
		{
			AWBHeader.Shipment.JS_GoodsValue = 1m;
			AWBHeader.Shipment.JS_RL_NKDestination = "AUSYD";

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBHeader.Populate();
				AssertEquals("If registry is set to 'No', the CustomsValue should return 0.", 0m, AWBHeader.EH_CustomsValue);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBHeader.Populate();
				AssertEquals("If registry is set to 'Yes', the CustomsValue should return 1. ", 1m, AWBHeader.EH_CustomsValue);
			}
		}

		public void TestDefaultingCustomsValue_DefaultsShipmentGoodsValue_WhenBangladeshImport_RegardlessOfRegistryValue()
		{
			AWBHeader.Shipment.JS_GoodsValue = 1m;
			AWBHeader.Shipment.JS_RL_NKDestination = "BDKHL";

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AWBHeader.Consol.JK_AgentType = AgentType.Direct;
				AWBHeader.Populate();
				AssertEquals("The CustomsValue should return shipment's goods value.", 1m, AWBHeader.EH_CustomsValue);

				AWBHeader.Consol.JK_AgentType = AgentType.Agent;
				AWBHeader.Populate();
				AssertEquals("The CustomsValue should return shipment's goods value.", 1m, AWBHeader.EH_CustomsValue);
			}

			using (FreightDataRegistry.Instance.DefaultShipmentGoodsValueToHAWBAndDirectMAWB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AWBHeader.Consol.JK_AgentType = AgentType.Direct;
				AWBHeader.Populate();
				AssertEquals("The CustomsValue should return shipment's goods value.", 1m, AWBHeader.EH_CustomsValue);

				AWBHeader.Consol.JK_AgentType = AgentType.Agent;
				AWBHeader.Populate();
				AssertEquals("The CustomsValue should return shipment's goods value.", 1m, AWBHeader.EH_CustomsValue);
			}
		}

		public void TestConsigneeAddressOverrideDefault()
		{
			AWBHeader.Shipment.ConsigneePK = Factory.New<OrgHeader>().PK;
			OrgAddress consigneeAddress = AWBHeader.Shipment.Consignee.MainAddress;
			consigneeAddress.OA_Address1 = "ConsigneeAddress";
			AssertEquals("Consignee pickup address added, so should be ConsigneeAddress", "ConsigneeAddress", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);

			OrgAddress consigneeDocumentAddress = AWBHeader.Shipment.Consignee.Addresses.AddNew();
			consigneeDocumentAddress.OA_Address1 = "ConsigneeDocumentAddress";
			consigneeDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Consignee postal address added, Still should be ConsigneeAddress", "ConsigneeAddress", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);

			AWBHeader.Shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentAddress.PK;
			AssertEquals("ConsigneeDocumentaryAddress set, should be ConsigneeDocumentAddress", "ConsigneeDocumentAddress", AWBHeader.ConsigneeDocumentaryAddress.E2_Address1);
		}

		public void TestShipperAddressOverrideDefault()
		{
			OrgAddress shipperAddress = AWBHeader.Shipment.Consignor.MainAddress;
			shipperAddress.OA_Address1 = "ShipperAddress";
			AssertEquals("Shipper pickup address added, so should be ShipperAddress", "ShipperAddress", AWBHeader.ShipperDocumentaryAddress.E2_Address1);

			OrgAddress shipperDocumentAddress = AWBHeader.Shipment.Consignor.Addresses.AddNew();
			shipperDocumentAddress.OA_Address1 = "ShipperDocumentAddress";
			shipperDocumentAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			AssertEquals("Shipper postal address added, Still should be ShipperAddress", "ShipperAddress", AWBHeader.ShipperDocumentaryAddress.E2_Address1);

			AWBHeader.Shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipperDocumentAddress.PK;
			AssertEquals("ShipperDocumentaryAddress set, should be ShipperDocumentAddress", "ShipperDocumentAddress", AWBHeader.ShipperDocumentaryAddress.E2_Address1);
		}

		public void TestExtraCarrierInfoLine2()
		{
			AssertEquals("Carrier line info should be blank", "", AWBHeader.ExtraCarrierInfoLine2);

			var airline1 = Factory.New<RefAirline>();
			airline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
			airline1.RM_TwoCharacterCode = "ZZ";
			airline1.RM_AirlineName1 = "AIR ZZ";

			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "ZOrganisation 1";
			org1.OH_Code = "ZZZZ";
			org1.MainAddress.OA_Address1 = "Addr1";
			org1.MiscServ.OM_RM_Airline = airline1.PK;
			AWBHeader.Consol.JK_MasterBillNum = "6663434";

			Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText = "Test Carrier Text";
			AssertEquals("Extra Carrier Info should equal", "Test Carrier Text: AIR ZZ", AWBHeader.ExtraCarrierInfoLine2);

			Env.Registry.Freight.AirWaybill.HAWBDefaultCarrierText = "Carrier Text that is longer than 64 characters blah blah this";
			AssertEquals("Extra Carrier Info should equal", "Carrier Text that is longer than 64 characters blah blah this: A", AWBHeader.ExtraCarrierInfoLine2);
		}

		public void TestExtraShipperInfoLine1()
		{
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine1);

			Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Test Shipper Text";
			AssertEquals("Extra Carrier Info should equal", "Test Shipper Text", AWBHeader.ExtraShipperInfoLine1);

			Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Shipper Text that is longer than 64 characters blah blah.trumpet this should not appear";
			AssertEquals("Extra Carrier Info should equal", "Shipper Text that is longer than 64 characters blah blah.trumpet", AWBHeader.ExtraShipperInfoLine1);
		}

		public void TestExtraShipperInfoLine2()
		{
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Test Shipper Text less than 64 char";
			AssertEquals("Shipper line info should be blank", "", AWBHeader.EH_ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Shipper Text that is longer than 64 characters blah blah.trumpetthis should appear line 2";
			AssertEquals("Extra Carrier Info should equal", "this should appear line 2", AWBHeader.ExtraShipperInfoLine2);

			Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText += " Some more characters to push the string over 128 characters";
			AssertEquals("Extra Carrier Info should equal", "this should appear line 2 Some more characters to push the strin", AWBHeader.ExtraShipperInfoLine2);
		}

		public void TestRegistrationNumber()
		{
			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil, Constants.CountryCodes.Brazil, 1, "CNPJ", "CNPJ", "HAW");

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var shipment = Factory.New<ForwardingShipment>();
			AWBHeader.EH_ParentID = shipment.PK;
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = brazil.Code;
			code1.OK_CustomsRegNo = "REG1111";

			var code2 = consignee.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.RebateUserCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code2.OK_CustomsRegNo = "REG2222";
			AWBHeader.Populate();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			shipment.JS_RL_NKOrigin = "ARBUE";
			shipment.JS_RL_NKDestination = "BRRIO";
			AWBHeader.Populate();
			AssertEquals("Registration Number", "CNPJ: REG1111", AWBHeader.RegistrationNumber);
		}

		public void TestRegistrationNumber_BRViaEU()
		{
			CreateRefDocOrgCusCode(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil, Constants.CountryCodes.Brazil, 1, "CNPJ", "CNPJ", "HAW");

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "BRSAO";
			AWBHeader.EH_ParentID = shipment.PK;
			AssertEquals("Registration Number", ZString.Empty, AWBHeader.RegistrationNumber);

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;

			var leg1 = consol.Transports[0];
			leg1.JW_TransportMode = TransportModes.Air;
			leg1.JW_RL_NKLoadPort = "AUBNE";
			leg1.JW_RL_NKDiscPort = "DEFRA";
			leg1.JW_ETD = ZDateTime.Now.AddDays(1);

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = TransportModes.Air;
			leg2.JW_RL_NKLoadPort = "DEFRA";
			leg2.JW_RL_NKDiscPort = "BRSAO";
			leg2.JW_ETD = ZDateTime.Now.AddDays(2);

			var consignee = Factory.New<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			code1.OK_RN_NKCodeCountry = CountryCodes.Brazil;
			code1.OK_CustomsRegNo = "REG1111";

			AWBHeader.Populate();
			AssertEquals("Registration Number", "CNPJ: REG1111", AWBHeader.RegistrationNumber);
		}

		public void TestEORITaxNumber_DischargeInEU_Consignee()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPAR";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("DE123456789", awbHeader.EH_ConsigneeTraderNo);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestMVATaxNumber_DischargeInNorway_Consignee()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NOGRI";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NOGRI";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.NorwayCodeTypes.MVA, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("NO123456789", awbHeader.EH_ConsigneeTraderNo);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestUIDTaxNumber_DischargeInSwitzerland_Consignee()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CHALE";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "CHALE";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", awbHeader.EH_ConsigneeTraderNo);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestUIDTaxNumber_DischargeInLiechtenstein_Consignee()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "LIBAZ";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "LIBAZ";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Liechtenstein;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", awbHeader.EH_ConsigneeTraderNo);

			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.SwissCodeTypes.UID, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("CH123456789", awbHeader.EH_ConsigneeTraderNo);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestICETaxNumber_DischargeInMorocco_Consignee()
		{
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "HAW");
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MACAS";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "MACAS";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			code1.OK_CustomsRegNo = "111111111111111";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(code1.OK_CustomsRegNo, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestShipperTradeNoHasNoCountryCodePrefix()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NOGRI";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NOGRI";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;

			var customCode = shipment.Consignor.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			customCode.OK_CustomsRegNo = "XI123456789";

			awbHeader.Populate();
			AssertEquals("XI123456789", awbHeader.EH_ShipperTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			awbHeader.Populate();
			AssertEquals("DEA123456789", awbHeader.EH_ShipperTraderNo);

			customCode.OK_CustomsRegNo = "2";
			awbHeader.Populate();
			AssertEquals("DE2", awbHeader.EH_ShipperTraderNo);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_EOR()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NOGRI";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NOGRI";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsigneePK = consignee.PK;

			var customCode = shipment.Consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			customCode.OK_CustomsRegNo = "XI123456789";

			awbHeader.Populate();
			AssertEquals("XI123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			awbHeader.Populate();
			AssertEquals("CHA123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			awbHeader.Populate();
			AssertEquals("CH2", awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestShipperAndConsigneeEmail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ID6DI";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Indonesia Consignee";
			consignee.OH_RL_NKClosestPort = "ID6DI";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "ID";
			consignee.MainAddress.OA_Email = "consignee@wtg.com";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "AU Shipper";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.City = "Sydney City";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";
			shipper.MainAddress.OA_Email = "shipper@wtg.com";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = shipper.PK;
			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			AssertEquals(consignee.MainAddress.OA_Email, awbHeader.EH_ConsigneeContactEmail);
			AssertEquals(shipper.MainAddress.OA_Email, awbHeader.EH_ShipperContactEmail);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_MVA()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NOGRI";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "NOGRI";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsigneePK = consignee.PK;

			var customCode = shipment.Consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.NorwayCodeTypes.MVA;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Norway;
			customCode.OK_CustomsRegNo = "XX123456789";

			awbHeader.Populate();
			AssertEquals("XX123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "X123456789";
			awbHeader.Populate();
			AssertEquals("NOX123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			awbHeader.Populate();
			AssertEquals("NO2", awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestConsigneeTraderNoHasNoCountryCodePrefix_UID()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "LIBAZ";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "LIBAZ";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsigneePK = consignee.PK;

			var customCode = shipment.Consignee.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Liechtenstein;
			customCode.OK_CustomsRegNo = "XX123456789";

			awbHeader.Populate();
			AssertEquals("XX123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "X123456789";
			awbHeader.Populate();
			AssertEquals("CHX123456789", awbHeader.EH_ConsigneeTraderNo);

			customCode.OK_CustomsRegNo = "2";
			awbHeader.Populate();
			AssertEquals("CH2", awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestCCCTaxNumber_DischargeInCanada_AlsoNotify()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "HAW");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CA2KS";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CA2KS";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			awbHeader.Populate();

			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNo);

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			code1.OK_CustomsRegNo = "678910";
			awbHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("678910", awbHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestCCCTaxNumber_DischargeInCanada_Consignee()
		{
			CreateRefDocOrgCusCode("CCC", CountryCodes.Canada, CountryCodes.Canada, 1, "CCC", "CCC", "HAW");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CA2KS";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CA2KS";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			awbHeader.Populate();

			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);

			var code1 = consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Canada;
			code1.OK_CustomsRegNo = "678910";
			awbHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("678910", awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestCUITaxNumber_Discharge_Argentina()
		{
			CreateRefDocOrgCusCode("CUI", CountryCodes.Argentina, CountryCodes.Argentina, 1, "CUIT", "Tax Identification Number", "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ARBUE";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Argentina;
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.AWBHeader.Populate();

			AssertEquals("No Tax type should be displayed when no tax number.", string.Empty, shipment.AWBHeader.EH_ConsigneeTraderNoType);
			AssertHasWarning(shipment.AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertHasWarning(shipment.AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");

			var taxCode = consignee.CustomsCodes.AddNew();
			taxCode.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			taxCode.OK_RN_NKCodeCountry = CountryCodes.Argentina;
			taxCode.OK_CustomsRegNo = "12364";
			shipment.AWBHeader.Populate();

			AssertEquals("CUIT", shipment.AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12364", shipment.AWBHeader.EH_ConsigneeTraderNo);
			AssertNoWarning(shipment.AWBHeader.EH_ConsigneeTraderNoInfo, "CUI (CUIT) is required for Argentina imports.");
			AssertNoWarning(AWBHeader.EH_ConsigneeTraderNoTypeInfo, "CUI (CUIT) is required for Argentina imports.");
		}

		public void TestPNNNumberRequired_Discharge_Indonesia()
		{
			CreateRefDocOrgCusCode("PPN", CountryCodes.Indonesia, CountryCodes.Indonesia, 1, "NPWP", "NPWP tax identification number", "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "ID6DI";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Indonesia Consignee";
			consignee.OH_RL_NKClosestPort = "ID6DI";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "ID";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedErrorMessageConsignee = "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017";
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.IndonesiaCodeTypes.PPN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			ainCode.OK_CustomsRegNo = "123PPN";

			awbHeader.Populate();

			AssertEquals("NPWP", shipment.AWBHeader.EH_ConsigneeTraderNoType);
			AssertEquals("123PPN", shipment.AWBHeader.EH_ConsigneeTraderNo);
			AssertNoWarning(shipment.AWBHeader.EH_ConsigneeTraderNoInfo, "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017");
		}

		public void TestPopulateConsigneeAddress_ShouldHaveNoTaxInfoForDirectConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			consol.ReceivingForwarderWithContact.OrgPK = consignee.PK;
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "HAW");

			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "HAW");

			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			AssertEquals("", awbHeader.EH_ConsigneeTraderNo);
			AssertEquals("", awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("", awbHeader.EH_ConsigneeTraderNoCountryCode);
			AssertEquals("", awbHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("", awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("", awbHeader.EH_AlsoNotifyTraderNoCountryCode);
		}

		public void TestPopulateConsigneeAddress_ShouldUseBINOnTaxInfoForBangladeshNonDirectConsol()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "HAW");

			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedErrorMessageConsignee = "The Consignee VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			string expectedErrorMessageNotifyParty = "The Notify Party VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			AssertHasMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertHasMessageError(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertNoMessageError(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);
			AssertEquals("123BIN", awbHeader.EH_ConsigneeTraderNo);
			AssertEquals("BIN", awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("123BIN", awbHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("BIN", awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BD", awbHeader.EH_AlsoNotifyTraderNoCountryCode);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForNonDirectConsolToBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BOLPB";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BOLPB";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bolivia Consignee";
			consignee.OH_RL_NKClosestPort = "BOLPB";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 A Very Nice Street";
			consignee.MainAddress.City = "La Paz";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedErrorMessageConsignee = "The Consignee's NIT number is required for shipments to Bolivia.";
			AssertHasMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);

			var nitCode = consignee.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertEquals("997755331", awbHeader.EH_ConsigneeTraderNo);
			AssertEquals("NIT", awbHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldUseNITOnTaxInfoForNonDirectConsolFromBolivia()
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Bolivia, CountryCodes.Bolivia, 1, "NIT", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "BOLPB";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "BOLPB";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "Bolivia shipper";
			shipper.OH_RL_NKClosestPort = "BOLPB";
			shipper.MainAddress.Address1 = "Unit 717";
			shipper.MainAddress.Address2 = "11 Hard To Find Street";
			shipper.MainAddress.City = "La Paz";
			shipper.MainAddress.Postcode = "2215";
			shipper.MainAddress.OA_RN_NKCountryCode = "BO";

			shipment.ConsignorPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedErrorMessageShipper = "The Consignor's NIT number is required for shipments from Bolivia.";
			AssertHasMessageError(awbHeader.EH_ShipperTraderNoInfo, expectedErrorMessageShipper);

			var nitCode = shipper.CustomsCodes.AddNew();
			nitCode.OK_CodeType = OrgCusCode.BoliviaCodeTypes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bolivia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ShipperTraderNoInfo, expectedErrorMessageShipper);
			AssertEquals("997755331", awbHeader.EH_ShipperTraderNo);
			AssertEquals("NIT", awbHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldNotHaveCANForConsolFromIndia()
		{
			CreateRefDocOrgCusCode("CAN", CountryCodes.India, CountryCodes.India, 1, "CAN", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "INDEL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			var code = consignor.CustomsCodes.AddNew();
			code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			code.OK_CustomsRegNo = "997755331";

			shipment.ConsignorPK = consignor.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			AssertEquals("", AWBHeader.EH_ShipperTraderNo);
			AssertEquals("", AWBHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldNotHaveCANForConsolToIndia()
		{
			CreateRefDocOrgCusCode("CAN", CountryCodes.India, CountryCodes.India, 1, "CAN", ZString.Empty, "AWB");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "INDEL";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var code = consignee.CustomsCodes.AddNew();
			code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.CAN;
			code.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
			code.OK_CustomsRegNo = "997755331";

			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			AssertEquals("", AWBHeader.EH_ConsigneeTraderNo);
			AssertEquals("", AWBHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForDirectConsolToColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(Core.Constants.AgentType.Direct);
		}

		public void TestPopulateConsigneeAddress_ShouldUseNITOnTaxInfoForNonDirectConsolToColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(Core.Constants.AgentType.Other);
		}

		void SetupAndAssertShouldUseNITOnTaxInfoForConsolToColombia(string agentType)
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Colombia, CountryCodes.Colombia, 1, "NIT", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "CO8SG";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = agentType;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CO8SG";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Colombian shipper";
			consignee.OH_RL_NKClosestPort = "CO8SG";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Hard To Find Street";
			consignee.MainAddress.City = "Bogota";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "CO";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedWarningMessage = "NIT is required for Colombia imports.";
			AssertHasWarning(awbHeader.EH_ConsigneeTraderNoInfo, expectedWarningMessage);

			var nitCode = consignee.CustomsCodes.AddNew();
			nitCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedWarningMessage);
			AssertEquals(nitCode.OK_CustomsRegNo, awbHeader.EH_ConsigneeTraderNo);
			AssertEquals(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, awbHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateShipperAddress_ShouldUseNITOnTaxInfoForDirectConsolFromColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(Core.Constants.AgentType.Direct);
		}

		public void TestPopulateShipperAddress_ShouldUseNITOnTaxInfoForNonDirectConsolFromColombia()
		{
			SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(Core.Constants.AgentType.Other);
		}

		void SetupAndAssertShouldUseNITOnTaxInfoForConsolFromColombia(string agentType)
		{
			CreateRefDocOrgCusCode("NIT", CountryCodes.Colombia, CountryCodes.Colombia, 1, "NIT", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "CO8SG";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = agentType;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "CO8SG";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_FullName = "Colombian shipper";
			shipper.OH_RL_NKClosestPort = "CO8SG";
			shipper.MainAddress.Address1 = "Unit 717";
			shipper.MainAddress.Address2 = "11 Hard To Find Street";
			shipper.MainAddress.City = "Bogota";
			shipper.MainAddress.Postcode = "2215";
			shipper.MainAddress.OA_RN_NKCountryCode = "CO";

			shipment.ConsignorPK = shipper.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedWarningMessage = "NIT is required for Colombia exports.";
			AssertHasWarning(awbHeader.EH_ShipperTraderNoInfo, expectedWarningMessage);

			var nitCode = shipper.CustomsCodes.AddNew();
			nitCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			nitCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Colombia;
			nitCode.OK_CustomsRegNo = "997755331";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ShipperTraderNoInfo, expectedWarningMessage);
			AssertEquals(nitCode.OK_CustomsRegNo, awbHeader.EH_ShipperTraderNo);
			AssertEquals(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, awbHeader.EH_ShipperTraderNoType);
		}

		public void TestPopulateConsigneeAddress_ShouldUseBINOnTaxInfoForBangladeshNoConsol()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "BIN", ZString.Empty, "HAW");

			CreateRefDocOrgCusCode("AIN", CountryCodes.Bangladesh, CountryCodes.Bangladesh, 1, "AIN", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Factory.Save();

			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			string expectedErrorMessageConsignee = "The Consignee VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			string expectedErrorMessageNotifyParty = "The Notify Party VAT (BIN Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			AssertHasMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertHasMessageError(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();

			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertNoMessageError(awbHeader.EH_AlsoNotifyTraderNoInfo, expectedErrorMessageNotifyParty);
			AssertEquals("123BIN", awbHeader.EH_ConsigneeTraderNo);
			AssertEquals("BIN", awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("123BIN", awbHeader.EH_AlsoNotifyTraderNo);
			AssertEquals("BIN", awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("BD", awbHeader.EH_AlsoNotifyTraderNoCountryCode);
		}

		public void TestPopulateConsigneeAddress_ShouldUseRTNOnTaxInfoForNonDirectConsolToHonduras()
		{
			CreateRefDocOrgCusCode("RTN", CountryCodes.Honduras, CountryCodes.Honduras, 1, "RTN", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HNTGU";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HNTGU";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Honduras Consignee";
			consignee.OH_RL_NKClosestPort = "HNTGU";
			consignee.MainAddress.Address1 = "Unit 717";
			consignee.MainAddress.Address2 = "11 Beautiful Street";
			consignee.MainAddress.City = "Honduras City";
			consignee.MainAddress.Postcode = "32110";
			consignee.MainAddress.OA_RN_NKCountryCode = CountryCodes.Honduras;

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			Factory.Save();
			var awbHeader = shipment.AWBHeader;
			awbHeader.Populate();

			var expectedErrorMessageConsignee = "The Consignee's RTN number is required for inbound shipments to Honduras.";
			AssertHasMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);

			var rtnCode = consignee.CustomsCodes.AddNew();
			rtnCode.OK_CodeType = OrgCusCode.HondurasCodeTypes.RTN;
			rtnCode.OK_RN_NKCodeCountry = CountryCodes.Honduras;
			rtnCode.OK_CustomsRegNo = "5678999";
			Factory.Save();
			awbHeader.Populate();

			AssertNoMessageError(awbHeader.EH_ConsigneeTraderNoInfo, expectedErrorMessageConsignee);
			AssertEquals("5678999", awbHeader.EH_ConsigneeTraderNo);
			AssertEquals("RTN", awbHeader.EH_ConsigneeTraderNoType);
		}

		public void TestPopulateConsignorAddress_ShouldNotUseRTNOnTaxInfoForNonDirectConsolFromHonduras()
		{
			CreateRefDocOrgCusCode("RTN", CountryCodes.Honduras, CountryCodes.Honduras, 1, "RTN", ZString.Empty, "HAW");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "HNTGU";
			shipment.JS_RL_NKDestination = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "HNTGU";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Honduras Consignor";
			consignor.OH_RL_NKClosestPort = "HNTGU";
			consignor.MainAddress.Address1 = "Unit 717";
			consignor.MainAddress.Address2 = "11 Beautiful Street";
			consignor.MainAddress.City = "Honduras City";
			consignor.MainAddress.Postcode = "32110";
			consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Honduras;

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignor.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var awbHeader = shipment.AWBHeader;

			var rtnCode = consignor.CustomsCodes.AddNew();
			rtnCode.OK_CodeType = OrgCusCode.HondurasCodeTypes.RTN;
			rtnCode.OK_RN_NKCodeCountry = CountryCodes.Honduras;
			rtnCode.OK_CustomsRegNo = "5678999";
			Factory.Save();
			awbHeader.Populate();

			AssertEquals(string.Empty, awbHeader.EH_ShipperTraderNo);
			AssertEquals(string.Empty, awbHeader.EH_ShipperTraderNoType);
		}

		public void TestAlsoNotifyTraderNoHasNoCountryCodePrefix()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPAR";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEHAM";

			awbHeader.EH_ParentID = shipment.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			var customCode = notifyParty.Organisation.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			customCode.OK_CustomsRegNo = "XI123456789";

			awbHeader.Populate();
			AssertEquals("XI123456789", awbHeader.EH_AlsoNotifyTraderNo);

			customCode.OK_CustomsRegNo = "A123456789";
			awbHeader.Populate();
			AssertEquals("DEA123456789", awbHeader.EH_AlsoNotifyTraderNo);

			customCode.OK_CustomsRegNo = "2";
			awbHeader.Populate();
			AssertEquals("DE2", awbHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestEORITaxNumber_DischargeInEU_ForConsolLegs()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPAR";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("DE123456789", awbHeader.EH_ConsigneeTraderNo);

			consol.Transports[0].JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestEORIShouldNotShowOnTransitEU()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "DEHAM";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "USLAX";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestEORITaxNumber_DischargeInEU_AlsoNotify()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPAR";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "DEHAM";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "DEHAM";
			transport2.JW_RL_NKDiscPort = "FRPAR";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			code1.OK_CustomsRegNo = "123456789";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("DE123456789", awbHeader.EH_AlsoNotifyTraderNo);

			transport1.JW_RL_NKDiscPort = "HKHKG";
			awbHeader.Populate();
			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestICETaxNumber_DischargeInMorocco_AlsoNotify()
		{
			CreateRefDocOrgCusCode("ICE", "MA", "MA", 1, "ICE", "ICE", "HAW");
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MAAGA";
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "MACAS";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "MACAS";
			transport2.JW_RL_NKDiscPort = "MAAGA";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			code1.OK_CustomsRegNo = "111111111111111";

			awbHeader.Populate();
			AssertEquals(OrgCusCode.MoroccoCodeTypes.ICE, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(code1.OK_CustomsRegNo, awbHeader.EH_AlsoNotifyTraderNo);
		}

		public void TestVATTaxNumber_DiscInIL_Consignee()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "HAW");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IL2LL";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "IL2LL";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			awbHeader.Populate();

			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_ConsigneeTraderNo);

			var code1 = shipment.Consignee.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "12345";
			awbHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, awbHeader.EH_ConsigneeTraderNoType);
			AssertEquals("12345", awbHeader.EH_ConsigneeTraderNo);
		}

		public void TestVATTaxNumber_DiscInIL_AlsoNotify()
		{
			CreateRefDocOrgCusCode("VAT", CountryCodes.Israel, CountryCodes.Israel, 1, "VAT", "VAT", "HAW");

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CCCRRRCCC";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CCCEEECCC";
			consignee.OH_IsConsignee = true;

			Factory.Save();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IL2LL";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "IL2LL";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var notifyParty = shipment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
			notifyParty.OrganisationPK = Factory.New<OrgHeader>().PK;
			awbHeader.Populate();

			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals(ZString.Empty, awbHeader.EH_AlsoNotifyTraderNo);

			var code1 = notifyParty.Organisation.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			code1.OK_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			code1.OK_CustomsRegNo = "678910";
			awbHeader.Populate();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, awbHeader.EH_AlsoNotifyTraderNoType);
			AssertEquals("678910", awbHeader.EH_AlsoNotifyTraderNo);
		}

		#region Goods Declaration Reference Number

		public void TestGoodsDeclarationReferenceNumbers_TrimLeadingEntryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "CHBSL";
				shipment.JS_RL_NKDestination = "USCHI";

				var cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum1.CE_EntryNum = "GDRN11111";
				cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum2.CE_EntryNum = "GDR22222";
				cusEntryNum2.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum3.CE_EntryNum = "AAA33333";
				cusEntryNum3.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				var cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum4.CE_EntryNum = "PMT333";
				cusEntryNum4.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;

				var header = Factory.New<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				AssertContainsExactElementsInAnyOrder("Goods Declaration Reference Numbers",
					new[]
					{
						"CH|EXP|11111, 22222, AAA33333"
					},
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));

				cusEntryNum4.CE_EntryType = CusEntryNumberTypes.Switzerland.GoodsDeclarationReferenceNumber;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"CH|EXP|11111, 22222, AAA33333, PMT333"
					},
					header.GoodsDeclarationReferenceNumbers.Select(FormatGoodsDeclarationReferenceNumber));
			}
		}

		string FormatGoodsDeclarationReferenceNumber(GoodsDeclarationReferenceNumber goodsDeclarationReferenceNumber)
		{
			return string.Format("{0}|{1}|{2}", goodsDeclarationReferenceNumber.CountryOfIssue, goodsDeclarationReferenceNumber.MovementCode, string.Join(", ", goodsDeclarationReferenceNumber.Numbers));
		}

		#endregion

		#region Movement Reference Numbers

		public void TestMovementReferenceNumbers_TrimLeadingEntryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";

				var cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum1.CE_EntryNum = "MRN11111";
				cusEntryNum1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum2.CE_EntryNum = "MRN22222";
				cusEntryNum2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				var cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum3.CE_EntryNum = "PMT333";
				cusEntryNum3.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;

				var header = Factory.New<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				AssertEquals(ZString.Empty, shipment.CustomsEntryNumberType);
				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111, 22222*HWB|HBL11111"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));

				cusEntryNum3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				AssertEquals(CusEntryNumberTypes.Standard.MovementReferenceNumber, shipment.CustomsEntryNumberType);
				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					new[]
					{
						"HU|EXP|11111, 22222, PMT333*HWB|HBL11111"
					},
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		public void TestMovementReferenceNumbers_DoNotPopulateForNonEUCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "HBL11111";
				shipment.JS_RL_NKOrigin = "HUBUD";
				shipment.JS_RL_NKDestination = "PLWRO";
				shipment.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				shipment.CustomsEntryNumber = "MRN11111";

				var header = Factory.New<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				AssertContainsExactElementsInAnyOrder("Movement Reference Numbers",
					Array.Empty<string>(),
					header.MovementReferenceNumbers.Select(FormatMovementReferenceNumber));
			}
		}

		string FormatMovementReferenceNumber(MovementReferenceNumber number)
		{
			var result = string.Format("{0}|{1}|{2}", number.CountryOfIssue, number.MovementCode, string.Join(", ", number.Numbers));

			if (number.RelatedNumbers != null)
			{
				var related = number
					.RelatedNumbers
					.Select(n => string.Format("{0}|{1}", n.Type, n.Number))
					.ToArray();

				if (related.Any())
				{
					result = string.Concat(result, "*", string.Join("*", related));
				}
			}

			return result;
		}

		#endregion

		public void TestExtraShipperData()
		{
			CreateRefDocOrgCusCode(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, Constants.CountryCodes.Argentina, Constants.CountryCodes.Argentina, 1, "CUIT", "CUIT", "HAW");

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			AssertEquals("Extra Shipper Data", ZString.Empty, AWBHeader.ExtraShipperData);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ARBUE";
			shipment.JS_RL_NKDestination = "BRRIO";

			AWBHeader.EH_ParentID = shipment.PK;
			AssertEquals("Extra Shipper Data", ZString.Empty, AWBHeader.ExtraShipperData);

			var consignor = Factory.New<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			var code1 = consignor.CustomsCodes.AddNew();
			code1.OK_CodeType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Argentina;
			code1.OK_CustomsRegNo = "REG1111";

			var code2 = consignor.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.HKCodeTypes.KnownConsignorNumber;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.HongKong;
			code2.OK_CustomsRegNo = "REG2222";

			AWBHeader.Populate();
			AssertEquals("VAT number for shipper", "CUIT: REG1111", AWBHeader.ExtraShipperData);

			consignor.OH_RL_NKClosestPort = "HKHKG";
			AssertEquals("Extra Shipper Data", "KC: REG2222 CUIT: REG1111", AWBHeader.ExtraShipperData);
		}

		void CreateRefDocOrgCusCode(ZString code, ZString regulatingCountry, ZString codeCountry, ZByte priority, ZString shortLabel, ZString longLabel, ZString documentType)
		{
			var orgCusCode = Factory.New<RefDocOrgCusCode>();
			orgCusCode.DOC_CodeType = code;
			orgCusCode.DOC_RN_NKRegulatingCountry = regulatingCountry;
			orgCusCode.DOC_RN_NKCodeCountry = codeCountry;
			orgCusCode.DOC_Priority = priority;
			orgCusCode.DOC_ShortLabel = shortLabel;
			orgCusCode.DOC_LongLabel = longLabel;
			orgCusCode.DOC_DocumentType = documentType;
		}

		#region TestEH_AWBIssueDate

		[TestDate(2013, 5, 5)]
		public void TestPopulateIssueDate()
		{
			var today = ZDateTime.Today;
			var houseBillIssueDate = today.AddDays(3);
			var masterBillIssueDate = today.AddDays(-6);
			var consolLCLCutOffDate = today.AddDays(-4);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();

			Factory.Save();

			AssertEquals("Pre-condition: registry should default the HAWB from today", FreightConfigurationRegistry.Instance.AWBIssueDateIsTodaysDate, FreightConfigurationRegistry.Instance.AWBIssueDate.Value);
			AssertEquals("Pre-condition: shipment issue date should default as empty", ZDateTime.Empty, shipment.JS_HouseBillIssueDate);
			AssertAWBIssueDate("Pre-condition: HAWB issue date should default as today", today, shipment);

			shipment.JS_HouseBillIssueDate = houseBillIssueDate;

			AssertAWBIssueDate("HAWB issue date should always use the shipment issue date when it is set", houseBillIssueDate, shipment);

			FreightConfigurationRegistry.Instance.AWBIssueDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightConfigurationRegistry.Instance.AWBIssueDateIsCutOffDate);
			consol.JK_MasterBillIssueDate = masterBillIssueDate;

			AssertAWBIssueDate("HAWB issue date should always use the shipment issue date when it is set", houseBillIssueDate, shipment);

			shipment.JS_HouseBillIssueDate = ZDateTime.Empty;

			AssertAWBIssueDate("HAWB issue date should now fall back to consol's master bill issue date when the registry is set to default from Consol's AWB issue date", masterBillIssueDate, shipment);

			consol.JK_MasterBillIssueDate = ZDateTime.Empty;

			AssertAWBIssueDate("HAWB issue date has no alternative but to fall back to today's date", today, shipment);

			consol.Transports[0].JW_DepotCutOff = consolLCLCutOffDate;

			AssertAWBIssueDate("HAWB issue date should fall back to the consol's main transport leg cut off date when there is no shipment issue date or master bill issue date on the consol and the registry is set to default from Consol's AWB issue date", consolLCLCutOffDate, shipment);

			shipment.JS_HouseBillIssueDate = houseBillIssueDate;

			AssertAWBIssueDate("HAWB issue date should always use the shipment issue date when it is set", houseBillIssueDate, shipment);
		}

		void AssertAWBIssueDate(ZString message, ZDateTime expectedDate, ForwardingShipment shipment)
		{
			shipment.IsAWBValuesOverriddenProperty = false;
			var hawb = shipment.AWBHeader;
			hawb.Populate();

			AssertEquals(message, expectedDate, hawb.EH_AWBIssueDate);

			shipment.IsAWBValuesOverriddenProperty = true;
			hawb.Populate();

			AssertEquals(message, expectedDate, hawb.EH_AWBIssueDate);
		}

		#endregion

		public void TestHumanReadableName()
		{
			AssertEquals("House Air Waybill for " + AWBHeader.Shipment.HumanReadableName, AWBHeader.HumanReadableName);

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			AssertEquals("House Air Waybill", AWBHeader.HumanReadableName);
		}

		public void TestCustomsEntryNumber()
		{
			SetUpForDeparturePort("AUSYD");

			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			AWBHeader.Shipment.CustomsEntryNumberType = "ZUB";
			AWBHeader.Shipment.CustomsEntryNumber = "456";
			AssertEquals("ZUB: 456", AWBHeader.EH_ECNCRNNumber);

			AWBHeader.Shipment.CustomsEntryNumberType = Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.Get3CharCode(Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.EXSP.Code);
			AWBHeader.Shipment.CustomsEntryNumber = "987";
			AssertEquals("CAN: EXSP", AWBHeader.EH_ECNCRNNumber);
		}

		public void TestMultipleCustomsEntryNumbers()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			ForwardingShipment shipment = AWBHeader.Shipment;

			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "CUS";

			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "222";
			cusEntryNum2.CE_EntryType = "NUM";

			CusEntryNumber cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "BER";

			CusEntryNumber cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum4.CE_EntryNum = "";
			cusEntryNum4.CE_EntryType = "X";

			AssertEquals("CustomsEntryNumbers formatted", "CUS: 111,\r\nNUM: 222,\r\nBER: 333", AWBHeader.EH_ECNCRNNumber);

			AssertContainsExactElementsInAnyOrder("CustomsEntryNumbers",
				new[]
				{
					"CUS|111",
					"NUM|222",
					"BER|333"
				},
				AWBHeader.CustomsEntryNumbers.Select(n => string.Format("{0}|{1}", n.Type, n.Number)));
		}

		public void TestUnitedStatesCustomsEntryNumber()
		{
			SetUpForDeparturePort("AUSYD");

			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AWBHeader.Shipment.CustomsEntryNumberType = "ITN";
				AWBHeader.Shipment.CustomsEntryNumber = "456";
				AssertEquals("Custom entry number with 'ITN' type in United States should show 'AES' instead in AWB", "AES: 456", AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestMultipleCustomsEntryNumbersNotShowingMoreThan4()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.France);
			ForwardingShipment shipment = AWBHeader.Shipment;

			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = "111";
			cusEntryNum1.CE_EntryType = "CUS";

			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "222";
			cusEntryNum2.CE_EntryType = "NUM";

			CusEntryNumber cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum3.CE_EntryNum = "333";
			cusEntryNum3.CE_EntryType = "BER";

			CusEntryNumber cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum4.CE_EntryNum = "444";
			cusEntryNum4.CE_EntryType = "CAN";

			CusEntryNumber cusEntryNum5 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum5.CE_EntryNum = "555";
			cusEntryNum5.CE_EntryType = "CCN";

			AssertEquals("CustomsEntryNumbers formatted", "", AWBHeader.EH_ECNCRNNumber);

			AssertContainsExactElementsInAnyOrder("CustomsEntryNumbers",
				new[]
				{
					"CUS|111",
					"NUM|222",
					"BER|333",
					"CAN|444",
					"CCN|555"
				},
				AWBHeader.CustomsEntryNumbers.Select(n => string.Format("{0}|{1}", n.Type, n.Number)));
		}

		public void TestKenyaCustomsEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Kenya))
			{
				var header = Factory.New<ShipmentExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();
				header.EH_ParentID = shipment.PK;

				var cusEntryNum = shipment.CusEntryNumbers.AddNew();
				cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				cusEntryNum.CE_EntryNum = "123456789";

				var entryNum = header.CustomsEntryNumbers.FirstOrDefault();
				AssertNotNull(entryNum);
				AssertEquals(CusEntryNumberTypes.Standard.ClearancePermitNumber, entryNum.Type);
				AssertEquals("123456789", entryNum.Number);
			}
		}

		public void TestKenya_EH_ECNCRNNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Kenya))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
				shipment.JS_RL_NKOrigin = "KENBO";
				shipment.JS_RL_NKDestination = "AUSYD";

				var header = Factory.New<ShipmentExportAWBHeader>();
				header.EH_ParentID = shipment.PK;

				var cusEntryNum = shipment.CusEntryNumbers.AddNew();
				cusEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				cusEntryNum.CE_EntryNum = "123456789";

				var subShipment1 = shipment.CoLoadShipments.AddNew();
				subShipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;

				var cusEntryNum2 = subShipment1.CusEntryNumbers.AddNew();
				cusEntryNum2.CE_EntryType = CusEntryNumberTypes.Standard.ClearancePermitNumber;
				cusEntryNum2.CE_EntryNum = "987654321";

				AssertEquals("123456789 987654321", header.EH_ECNCRNNumber);
			}
		}

		public void TestHongKongCustomsEntryNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.HongKong))
			{
				var shipment = AWBHeader.Shipment;

				var cusEntryNum = shipment.CusEntryNumbers.AddNew();
				cusEntryNum.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
				cusEntryNum.CE_EntryNum = "1111111";

				var cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum2.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
				cusEntryNum2.CE_EntryNum = "2222222";

				AssertEquals("CustomsEntryNumbers formatted", "E/L: 1111111,2222222", AWBHeader.EH_ECNCRNNumber);

				var cusEntryNum3 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum3.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
				cusEntryNum3.CE_EntryNum = "3333333";

				var cusEntryNum4 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum4.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
				cusEntryNum4.CE_EntryNum = "4444444";

				var cusEntryNum5 = shipment.CusEntryNumbers.AddNew();
				cusEntryNum5.CE_EntryType = CusEntryNumberTypes.HongKong.ExportLicense;
				cusEntryNum5.CE_EntryNum = "5555555";

				AssertEquals("CustomsEntryNumbers formatted", "", AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestCustomsEntryNumberIgnoresEmptyElements()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			ForwardingShipment shipment = AWBHeader.Shipment;
			shipment.CusEntryNumbers.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);
			CusEntryNumber cusEntryNum1 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum1.CE_EntryNum = ZString.Empty;
			cusEntryNum1.CE_EntryType = ZString.Empty;
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);
			CusEntryNumber cusEntryNum2 = shipment.CusEntryNumbers.AddNew();
			cusEntryNum2.CE_EntryNum = "666";
			cusEntryNum2.CE_EntryType = "NOB";
			AssertEquals("NOB: 666", AWBHeader.EH_ECNCRNNumber);
		}

		public void TestAWBRateLinesType()
		{
			AssertEquals(typeof(ShipmentExportAWBRateLineCollection), AWBHeader.AWBRateLines.GetType());
		}

		public void TestUpdateRateClasses()
		{
			AWBHeader.Shipment.JS_OverrideWaybillDefaults = true;
			AWBHeader.Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight; //PPD
			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AWBHeader.AWBRateLines[0].ER_RateClass = Core.Constants.AWB.RateClass.MinimumCharge;

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			AssertEquals(Core.Constants.AWB.RateClass.MinimumCharge, AWBHeader.AWBRateLines[0].ER_RateClass);

			AWBHeader.EH_WeightPrepaidCollect = ShipmentExportAWBHeader.Constants.PrepaidCollect1CharCodes.Both;
			AssertEquals(ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid, AWBHeader.AWBRateLines[0].ER_RateClass);

			AWBHeader.EH_WeightPrepaidCollect = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			AssertEquals(Core.Constants.AWB.RateClass.MinimumCharge, AWBHeader.AWBRateLines[0].ER_RateClass);
		}

		public void TestGetNewValidation()
		{
			AssertEquals("Type of Validation", typeof(ShipmentExportAWBHeaderValidation), AWBHeader.Validation.GetType());
		}

		public void TestCalculationLogsAnalyzerType()
		{
			AssertEquals(typeof(ShipmentCalculationLogsAnalyzer), AWBHeader.CalculationLogsAnalyzer.GetType());
		}

		public void TestSaving()
		{
			var newFactory = new BusinessObjectFactory();

			var shipment = newFactory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var aWB = shipment.AWBHeader;
			AssertEquals("Not overridden new AWB is never saved", false, aWB.IsSavedByFactory);
			newFactory.Save();
			AssertEquals(false, aWB.IsInDatabase);

			shipment.JS_OverrideWaybillDefaults = true;
			shipment.JS_OverrideWaybillDefaults = false;
			aWB.EH_AreRateLinesOverridden = false;
			AssertEquals("Not overridden new AWB is never saved even if 'override' was changed", false, aWB.IsSavedByFactory);

			aWB.ForceSavingByFactory = true;
			AssertEquals("Forced AWB is always saved", true, aWB.IsSavedByFactory);
			newFactory.Save();
			AssertEquals(true, aWB.IsInDatabase);

			aWB.ForceSavingByFactory = false;
			AssertEquals("Once saved but not overridden is not saved next time", false, aWB.IsSavedByFactory);

			shipment.JS_OverrideWaybillDefaults = true;
			AssertEquals("Saved when 'Override' is ticked", true, aWB.IsSavedByFactory);
			newFactory.Save();

			AssertEquals("Overridden is not saved if doesn't have changes", false, aWB.IsSavedByFactory);

			aWB.HasChanges = true;
			AssertEquals("Overridden is saved when has changes", true, aWB.IsSavedByFactory);
			newFactory.Save();

			shipment.JS_OverrideWaybillDefaults = false;
			AssertEquals("Overridden is saved when 'Override' has changes and already in the database", true, aWB.IsSavedByFactory);
			newFactory.Save();

			aWB.HasChanges = true;
			AssertEquals("Not overridden is not saved when has changes", false, aWB.IsSavedByFactory);

			shipment.JS_OverrideWaybillDefaults = true;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Non-air transport modes not saved", false, aWB.IsSavedByFactory);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			aWB.EH_AreRateLinesOverridden = true;
			AssertEquals("Saved when 'Override Rate Section' has changed", true, aWB.IsSavedByFactory);
		}

		public void TestEH_ECNCRNNumber()
		{
			SetUpForDeparturePort("CATOR");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var aUCompany = Factory.New<GlbCompany>();
				aUCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				var aUBranch = Factory.New<GlbBranch>();
				aUBranch.GB_GC = aUCompany.PK;

				var aUdeclaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
				aUdeclaration.JE_JS = AWBHeader.Shipment.PK;
				aUdeclaration.JE_MessageType = "EXP";
				aUdeclaration.JE_GB = aUBranch.PK;
			}

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CATOR";
			AWBHeader.Shipment.JS_RL_NKOrigin = "CATOR";
			AWBHeader.Shipment.JS_RL_NKDestination = "AUSYD";

			var num = AWBHeader.Shipment.Numbers.AddNew();
			num.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			num.CE_EntryNum = "01682031001";
			num.CE_ParentTable = AWBHeader.Shipment.TableName;
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			var declaration = Factory.New<Enterprise.Integration.Customs.CA.IJobDeclaration>();
			declaration.JE_JS = AWBHeader.Shipment.PK;
			declaration.JE_MessageType = "EXP";

			var number = Factory.New<CusEntryNumber>();
			number.CE_EntryNum = "RC1792201232000019";
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CTN;
			number.CE_ParentID = declaration.PK;
			number.CE_ParentTable = declaration.TableName;
			AWBHeader.Shipment.ResetCusEntryNumbers();
			Factory.Save();
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			var otherCACompany = Factory.New<GlbCompany>();
			otherCACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			otherCACompany.GC_Code = "!CA";
			var otherCACompanyBranch = Factory.New<GlbBranch>();
			otherCACompanyBranch.GB_Code = "!VA";
			otherCACompanyBranch.GB_GC = otherCACompany.PK;

			var anotherCAdeclaration = Factory.New<Enterprise.Integration.Customs.CA.IJobDeclaration>();
			anotherCAdeclaration.JE_MessageType = "EXP";
			anotherCAdeclaration.JE_GB = otherCACompanyBranch.PK;
			anotherCAdeclaration.JE_JS = AWBHeader.Shipment.PK;
			AWBHeader.Shipment.ResetCusEntryNumbers();
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			declaration.JE_MessageType = "IMP";
			number.CE_EntryNum = ZString.Empty;
			AWBHeader.Shipment.ResetCusEntryNumbers();
			Factory.Save();
			AWBHeader.Shipment.ResetCusEntryNumbers();
			AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AWBHeader.Shipment.ResetCusEntryNumbers();
				AssertEquals(ZString.Empty, AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestEH_ECNCRNNumber_AU()
		{
			AWBHeader.Shipment.CustomsEntryNumberType = "ZZZ";
			AWBHeader.Shipment.CustomsEntryNumber = "123";

			GlbCompany.CurrentCompany.SetCountry("AU");
			AssertEquals("ZZZ: 123", AWBHeader.EH_ECNCRNNumber);
		}

		public void TestEH_ECNCRNNumber_US()
		{
			AWBHeader.Shipment.CustomsEntryNumberType = "ZZZ";
			AWBHeader.Shipment.CustomsEntryNumber = "123";

			foreach (var countryCode in Core.Constants.CountryCodes.UsaAndTerritoriesList)
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				AssertEquals("ZZZ: 123", AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestEH_ECNCRNNumber_EU()
		{
			var europeanUnionCountries = new ZString[]
			{
				Core.Constants.CountryCodes.Austria,
				Core.Constants.CountryCodes.Belgium,
				Core.Constants.CountryCodes.Bulgaria,
				Core.Constants.CountryCodes.Cyprus,
				Core.Constants.CountryCodes.CzechRepublic,
				Core.Constants.CountryCodes.Denmark,
				Core.Constants.CountryCodes.Estonia,
				Core.Constants.CountryCodes.Finland,
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Greece,
				Core.Constants.CountryCodes.Hungary,
				Core.Constants.CountryCodes.Ireland,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.Latvia,
				Core.Constants.CountryCodes.Lithuania,
				Core.Constants.CountryCodes.Luxembourg,
				Core.Constants.CountryCodes.Malta,
				Core.Constants.CountryCodes.Netherlands,
				Core.Constants.CountryCodes.Poland,
				Core.Constants.CountryCodes.Portugal,
				Core.Constants.CountryCodes.Romania,
				Core.Constants.CountryCodes.Slovakia,
				Core.Constants.CountryCodes.Slovenia,
				Core.Constants.CountryCodes.Spain,
				Core.Constants.CountryCodes.Sweden,
				Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.CountryCodes.Monaco,
				Core.Constants.CountryCodes.IsleOfMan,
				Core.Constants.CountryCodes.Croatia
			};

			AWBHeader.Shipment.CustomsEntryNumberType = "ZZZ";
			AWBHeader.Shipment.CustomsEntryNumber = "111";

			foreach (var countryCode in europeanUnionCountries)
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
				AssertEquals("ZZZ: 111", AWBHeader.EH_ECNCRNNumber);
			}
		}

		public void TestFreightCharges()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jobHeader.JH_JobNum = "Phony number";
			jobHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jobHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var freightChargeCode = Factory.New<AccChargeCode>();
			freightChargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;

			Env.Registry.FreightChargeCode = freightChargeCode.PK.ToGuid();

			var freightCharge1 = NewJobCharge(jobHeader, 11.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Consol.SendingForwarder, 1.1m);
			var freightCharge2 = NewJobCharge(jobHeader, 1252.0m, freightChargeCode, "Freight Charge Description", AWBHeader.Shipment.Consignor, 1.1m);
			AWBHeader.Populate();

			var freightCharges = AWBHeader.GetFreightCharges();

			AssertNotNull(freightCharges);
			AssertEquals(2, freightCharges.Length);
			AssertCollectionContains(freightCharge1, freightCharges);
			AssertCollectionContains(freightCharge2, freightCharges);
		}

		public void TestFollowOnVolumeAndDimension()
		{
			var shipmentHeader1 = Factory.New<ShipmentExportAWBHeader>();
			var shipment1 = Factory.New<ForwardingShipment>();
			shipmentHeader1.EH_ParentID = shipment1.PK;

			shipmentHeader1.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			shipmentHeader1.Shipment.JS_GoodsDescription = "";
			shipmentHeader1.Shipment.JS_ActualVolume = 10M;
			shipmentHeader1.Shipment.JS_UnitOfVolume = "M3";
			shipmentHeader1.Shipment.JS_OuterPacks = 0;
			shipmentHeader1.Shipment.OuterPackLines.RemoveAndDeleteAll();
			shipmentHeader1.Shipment.IsAWBValuesOverriddenProperty = false;
			shipmentHeader1.PopulateIfNotOverridden();

			var shipmentHeader2 = Factory.New<ShipmentExportAWBHeader>();
			var shipment2 = Factory.New<ForwardingShipment>();
			shipmentHeader2.EH_ParentID = shipment2.PK;

			shipmentHeader2.Shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.DEF;
			shipmentHeader2.Shipment.JS_GoodsDescription = "";
			shipmentHeader2.Shipment.JS_ActualVolume = 10M;
			shipmentHeader2.Shipment.JS_UnitOfVolume = "M3";
			shipmentHeader2.Shipment.JS_OuterPacks = 0;
			shipmentHeader2.Shipment.OuterPackLines.RemoveAndDeleteAll();
			shipmentHeader2.Shipment.IsAWBValuesOverriddenProperty = true;
			shipmentHeader2.PopulateIfNotOverridden();

			AssertEquals("Shipments should be equal regardless of override", shipmentHeader1.VolumeAndDimensionForFollowOnPage, shipmentHeader2.VolumeAndDimensionForFollowOnPage);
		}

		ExportAWBAccountingInformation NewAccountingInformation(string informationID, string information)
		{
			var accountingInformation = Factory.NewWithValidTestData<ExportAWBAccountingInformation>();
			accountingInformation.EA_EH = AWBHeader.PK;
			accountingInformation.EA_InformationID = informationID;
			accountingInformation.EA_Information = information;

			return accountingInformation;
		}

		public void TestCalculateACASOverrideFields()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			forwarder.OH_IsConsignor = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKDischargePort = "USLAX";
			shipment.ConsignorPK = forwarder.PK;

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			AWBHeader.EH_ParentID = shipment.PK;
			AWBHeader.EH_ShipperContactEmail = "";
			AWBHeader.EH_ConsigneeContactEmail = "";
			AWBHeader.EH_WeightVPPDCOL = "PPD";

			var acasCountryHandler = AWBHeader.GetACASCountryHandler();
			acasCountryHandler.GetCustomerAccountHolderAndName(out var accountHolder, out var accountName);
			acasCountryHandler.GetCustomerAccountIssuerAndNumber(out var accountIssuer, out var accountNumber);
			var customerShippingFrequency = acasCountryHandler.GetCustomerAccountShippingFrequency();
			var verifiedKnownConsignor = acasCountryHandler.IsVerifiedKnownConsignor();
			acasCountryHandler.GetCustomerAccountEstablishmentDate(out var establishmentDate);
			acasCountryHandler.GetCustomerAccountBillingType(out var billingType);
			(_, ZString idType, ZString idIssuer, ZString idNumber) = acasCountryHandler.GetBiographicData();

			var changeVerifiedConsignor = !verifiedKnownConsignor;

			CombineAssertions("Assert Preconditions", () =>
			{
				Assert(!AWBHeader.EH_Calculated_ACASInfoOverridden);
				AssertEquals(AWBHeader.EH_ShipperContactEmail, AWBHeader.EH_Calculated_ACASShipperEmail);
				AssertEquals(AWBHeader.EH_ConsigneeContactEmail, AWBHeader.EH_Calculated_ACASConsigneeEmail);
				AssertEquals(accountName, AWBHeader.EH_Calculated_ACASCustomerAccountName);
				AssertEquals(accountIssuer, AWBHeader.EH_Calculated_ACASCustomerAccountIssuer);
				AssertEquals(accountNumber, AWBHeader.EH_Calculated_ACASCustomerAccountNumber);
				AssertEquals(accountHolder, AWBHeader.EH_Calculated_ACASCustomerAccountHolder);
				AssertEquals(customerShippingFrequency, AWBHeader.EH_Calculated_ACASCustomerAccountShippingFrequency);
				AssertEquals(verifiedKnownConsignor, AWBHeader.EH_Calculated_ACASVerifiedKnownConsignor);
				AssertEquals(establishmentDate, AWBHeader.EH_Calculated_ACASCustomerAccountEstablishmentDate.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture));
				AssertEquals(billingType, AWBHeader.EH_Calculated_ACASCustomerAccountBillingType);
				AssertEquals(idType, AWBHeader.EH_Calculated_ACASBiographicDataType);
				AssertEquals(idIssuer, AWBHeader.EH_Calculated_ACASBiographicDataCountry);
				AssertEquals(idNumber, AWBHeader.EH_Calculated_ACASBiographicDataNumber);
			});

			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.SPE, "shipper@abc.com.au"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.ANM, "Customer Name"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.ASF, "X"));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.VKC, changeVerifiedConsignor.ToString()));
			AWBHeader.AWBAccountingInformations.Add(NewAccountingInformation(AccountingCodes.AED, ZDate.Today.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture)));

			CombineAssertions("ACAS Calculated Properties should have been changed", () =>
			{
				Assert(AWBHeader.EH_Calculated_ACASInfoOverridden);
				AssertEquals("shipper@abc.com.au", AWBHeader.EH_Calculated_ACASShipperEmail);
				AssertEquals("Customer Name", AWBHeader.EH_Calculated_ACASCustomerAccountName);
				AssertEquals("X", AWBHeader.EH_Calculated_ACASCustomerAccountShippingFrequency);
				AssertEquals(changeVerifiedConsignor, AWBHeader.EH_Calculated_ACASVerifiedKnownConsignor);
				AssertEquals(ZDate.Today, AWBHeader.EH_Calculated_ACASCustomerAccountEstablishmentDate);
			});
		}

		#region AsAgreed

		public void TestAsAgreed1stAnd2nd_DefaultToNonForBrazil()
		{
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.All;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = Core.Constants.AWB.AsAgreedTypes.Codes.All;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DKBLL";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var awbHeader = Factory.New<ShipmentExportAWBHeaderForTest>();
			awbHeader.EH_ParentID = shipment.PK;

			AssertEquals("Precondition: Should still default to All for DK discharge", Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Precondition: Should still default to All for DK discharge", Core.Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			shipment.JS_RL_NKDestination = "BRRIO";
			awbHeader.Populate();

			AssertEquals("Precondition: Failed to set shipment discharge port to Brazil", "BRRIO", shipment.JS_RL_NKDestination);

			AssertEquals("As Agreed 1st failed to default to None with default value set to All",
				Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("As Agreed 2nd failed to default to None with default value set to All",
				Core.Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
		}

		public void TestAsAgreedDefaultsFromRegistryWhenShipmentUnchanged()
		{
			var oldFirst = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB;
			var oldSecond = Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB;
			try
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = Constants.AWB.AsAgreedTypes.Codes.All;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = Constants.AWB.AsAgreedTypes.Codes.All;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Precondition: Expected consol field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, shipment.JS_HBLAWBChargesDisplay);

				var awbHeader = shipment.AWBHeader as ShipmentExportAWBHeader;
				AssertNotNull(awbHeader);
				awbHeader.Populate();
				AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
				AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetHAWB = oldFirst;
				Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetHAWB = oldSecond;
			}
		}

		public void TestAsAgreedSynchronisedFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			AssertEquals("Precondition: Expected shipment field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, shipment.JS_HBLAWBChargesDisplay);

			var awbHeader = shipment.AWBHeader as ShipmentExportAWBHeader;
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertEquals("Precondition: Expected shipment field to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, shipment.JS_HBLAWBChargesDisplay);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.Collect, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.Prepaid, awbHeader.EH_AsAgreed2nd);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertEquals("Precondition: Expected shipment field to be NON", ChargesApplyHelper.ChargesApplyConstants.NON, shipment.JS_HBLAWBChargesDisplay);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.None, awbHeader.EH_AsAgreed2nd);
		}

		public void TestAsAgreedNotSynchronisedFromShipmentWhenOverriden()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			AssertEquals("Precondition: Expected shipment field to be ALL", ChargesApplyHelper.ChargesApplyConstants.ALL, shipment.JS_HBLAWBChargesDisplay);

			var awbHeader = shipment.AWBHeader as ShipmentExportAWBHeader;
			AssertNotNull(awbHeader);
			awbHeader.Populate();
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			shipment.IsAWBValuesOverriddenProperty = true;
			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertEquals("Precondition: Expected shipment field to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, shipment.JS_HBLAWBChargesDisplay);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertEquals("Precondition: Expected shipment field to be NON", ChargesApplyHelper.ChargesApplyConstants.NON, shipment.JS_HBLAWBChargesDisplay);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed1st);
			AssertEquals("Expected awb header to not be defaulted", Constants.AWB.AsAgreedTypes.Codes.All, awbHeader.EH_AsAgreed2nd);
		}

		#endregion

		public void TestIsImportCountryChina()
		{
			AWBHeader.Shipment.JS_RL_NKDestination = "AUSYD";
			Assert(!AWBHeader.IsImportToChina);

			AWBHeader.Shipment.JS_RL_NKDestination = "CNSHA";
			Assert(AWBHeader.IsImportToChina);
		}

		public void TestIsTransitingThroughChina()
		{
			var transport = AWBHeader.Shipment.Transports.AddNew();

			AWBHeader.Shipment.JS_RL_NKOrigin = "CNSHA";
			AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "CNSHA";
			AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(!AWBHeader.IsTransitingThroughChina);

			AWBHeader.Shipment.JS_RL_NKOrigin = "AUSYD";
			AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(!AWBHeader.IsTransitingThroughChina);

			AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "CNSHA";
			AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "AUSYD";
			Assert(AWBHeader.IsTransitingThroughChina);

			AWBHeader.Shipment.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			AWBHeader.Shipment.Transports[0].JW_RL_NKDiscPort = "CNSHA";
			Assert(AWBHeader.IsTransitingThroughChina);
		}

		public void TestIsTransitingThroughChina_RelatedTransports()
		{
			var shipment = AWBHeader.Shipment;

			var consol = AWBHeader.Shipment.Consols.AddNew();
			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";

			shipment.JS_RL_NKOrigin = "CNSHA";
			Assert(!AWBHeader.IsTransitingThroughChina);

			shipment.JS_RL_NKOrigin = "USLAX";
			Assert(AWBHeader.IsTransitingThroughChina);

			shipment.JS_RL_NKDestination = "CNSHA";
			Assert(!AWBHeader.IsTransitingThroughChina);

			shipment.JS_RL_NKDestination = "USLAX";
			Assert(AWBHeader.IsTransitingThroughChina);
		}

		#region TestMasterBillNumber_DRTShipmentWithPreCarriageOnForwardingAGTConsol

		public void TestMasterBillNumber_DRTShipmentWithPreCarriageOnForwardingAGTConsol()
		{
			var today = ZDateTime.Today;
			var shipment = AWBHeader.Shipment;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "DRT shipment";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;

			var agentPreCarriageConsol = FreightTestHelper.GetConsol<ForwardingConsol>("AGT Pre-carriage", "AIR", "AGT", "USCHI", "USLAX", today, today.AddDays(1), Factory, shipment);
			agentPreCarriageConsol.JK_MasterBillNum = "11111111111";
			var directDepartureConsol = FreightTestHelper.GetConsol<ForwardingConsol>("DRT Departure", "AIR", "DRT", "USLAX", "SGSIN", today.AddDays(2), today.AddDays(3), Factory, shipment);
			directDepartureConsol.JK_MasterBillNum = "22222222222";
			var directArrivalConsol = FreightTestHelper.GetConsol<ForwardingConsol>("DRT Arrival", "AIR", "DRT", "SGSIN", "AUSYD", today.AddDays(4), today.AddDays(5), Factory, shipment);
			directArrivalConsol.JK_MasterBillNum = "33333333333";
			var agentOnForwardingConsol = FreightTestHelper.GetConsol<ForwardingConsol>("AGT On-forwarding", "AIR", "AGT", "AUSYD", "AUMEL", today.AddDays(6), today.AddDays(7), Factory, shipment);
			agentOnForwardingConsol.JK_MasterBillNum = "44444444444";

			AssertEquals(directDepartureConsol, AWBHeader.Consol);
			AssertEquals("22222222", AWBHeader.EH_AWBSerialNo);
		}

		#endregion

		#region Harmonized Code

		public void TestHarmonizedCode()
		{
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var packLines = AWBHeader.Shipment.OuterPackLines;
			var packLine = packLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_HarmonisedCode = "1000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription, AWBHeader.AWBRateLine3.NatureAndQtyOfGoodsType);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_HarmonisedCode = "2000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "2000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 2000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 3;
			packLine.JL_HarmonisedCode = "2000";

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 4;

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 5;
			packLine.JL_HarmonisedCode = "3000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "2000", "3000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 2000, 3000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
		}

		public void TestHarmonizedCode_DoNotFillAvailableRateLinesWhenNoTariffFound()
		{
			var packLines = AWBHeader.Shipment.OuterPackLines;
			for (var i = 1; i < 30; i++)
			{
				var packLine = packLines.AddNew();
				packLine.JL_PackageCount = i;
				packLine.JL_HarmonisedCode = i.ToString();
			}

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine6.NatureAndQtyOfGoods.Text);

			AssertEquals("HS Codes: 1, 2, 3, 4, 5, 6, 7, 8,", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals("9, 10, 11, 12, 13, 14, 15, 16, 17,", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("18, 19, 20, 21, 22, 23, 24, 25, 26,", AWBHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
			AssertEquals("27, 28, 29", AWBHeader.AWBRateLine5.NatureAndQtyOfGoods.Text);
		}

		public void TestHarmonizedCode_CountrySpecificHSCode()
		{
			HarmonisedCodeHelperTest.LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var packLines = AWBHeader.Shipment.OuterPackLines;
			var packLine = packLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_HarmonisedCode = "1000";

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 2;

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 3;
			packLine.JL_HarmonisedCode = "3000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "3000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 3000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			var hc1 = packLine.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "JM";
			hc1.JLH_Code = "3333";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "3333" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 3333", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			packLine = packLines.AddNew();
			packLine.JL_PackageCount = 4;
			packLine.JL_HarmonisedCode = "4000";
			var hc2 = packLine.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "SG";
			hc2.JLH_Code = "4444";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "3333", "4000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 3333, 4000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			hc1.JLH_Code = "100000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "4000" }, AWBHeader.GetAvailableHarmonisedCodes());

			AWBHeader.Populate();

			AssertEquals("No Dimensions Available", AWBHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS codes", "HS Codes: 100000, 4000", AWBHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
		}

		public void TestHarmonizedCode_CountrySpecificHSCode_MultipleCountries()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";
			shipment.JS_RL_NKLoadPort = "JMKIN";

			awbHeader.EH_ParentID = shipment.PK;

			var packLines = awbHeader.Shipment.OuterPackLines;

			var packLine1 = packLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_HarmonisedCode = "100000";

			var hc1 = packLine1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "HK";
			hc1.JLH_Code = "200000";

			var packLine2 = packLines.AddNew();
			packLine2.JL_PackageCount = 2;

			var packLine3 = packLines.AddNew();
			packLine3.JL_PackageCount = 3;
			packLine3.JL_HarmonisedCode = "300000";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "300000" }, awbHeader.GetAvailableHarmonisedCodes());

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 300000", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			var hc2 = packLine3.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "JM";
			hc2.JLH_Code = "333333";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "333333" }, awbHeader.GetAvailableHarmonisedCodes());

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 333333", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			var hc3 = packLine3.HarmonisedCodes.AddNew();
			hc3.JLH_RN_NKCountry = "JM";
			hc3.JLH_Code = "444444";

			var hc4 = packLine3.HarmonisedCodes.AddNew();
			hc4.JLH_RN_NKCountry = "SG";
			hc4.JLH_Code = "555555";

			AssertContainsExactElementsInAnyOrder(new[] { "100000", "333333", "444444" }, awbHeader.GetAvailableHarmonisedCodes());

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 333333, 444444", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);

			var packLine4 = packLines.AddNew();
			packLine4.JL_PackageCount = 4;
			packLine4.JL_HarmonisedCode = "800000";

			for (int i = 0; i < 12; i++)
			{
				var hc = packLine4.HarmonisedCodes.AddNew();
				hc.JLH_RN_NKCountry = "JM";
				hc.JLH_Code = (900000 + i).ToString();
			}

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"100000", "333333", "444444", "900000", "900001", "900002",
				"900003", "900004", "900005", "900006", "900007", "900008",
				"900009", "900010", "900011"
			}, awbHeader.GetAvailableHarmonisedCodes());

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine6.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000, 333333, 444444,", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals("900000, 900001, 900002, 900003,", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("900004, 900005, 900006, 900007,", awbHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
			AssertEquals("900008, 900009, 900010, 900011", awbHeader.AWBRateLine5.NatureAndQtyOfGoods.Text);

			Assert(!awbHeader.AWBRateLine1.IsHSCodeLine);
			Assert(awbHeader.AWBRateLine2.IsHSCodeLine);
			Assert(awbHeader.AWBRateLine3.IsHSCodeLine);
			Assert(awbHeader.AWBRateLine4.IsHSCodeLine);
			Assert(awbHeader.AWBRateLine5.IsHSCodeLine);
		}

		public void TestHarmonizedCode_CountrySpecificHSCode_MultipleCountries_Validation()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";
			shipment.JS_RL_NKLoadPort = "JMKIN";

			awbHeader.EH_ParentID = shipment.PK;
			AssertEquals(false, awbHeader.EH_AreRateLinesOverridden);

			var packLines = shipment.OuterPackLines;

			var packLine1 = packLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_HarmonisedCode = "100000";

			var hc1 = packLine1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "HK";
			hc1.JLH_Code = "200000";

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 100000", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertNoWarnings(awbHeader.AWBRateLine2.NatureAndQtyOfGoods.TextInfo);

			var packLine2 = packLines.AddNew();
			packLine2.JL_PackageCount = 2;

			for (int i = 0; i < 42; i++)
			{
				var hc = packLine2.HarmonisedCodes.AddNew();
				hc.JLH_RN_NKCountry = "JM";
				hc.JLH_Code = (900001 + i).ToString();
			}

			awbHeader.Populate();

			AssertEquals("HS Codes: 100000, 900001, 900002,", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			AssertEquals("900003, 900004, 900005, 900006,", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertEquals("900007, 900008, 900009, 900010,", awbHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
			AssertEquals("900011, 900012, 900013, 900014,", awbHeader.AWBRateLine5.NatureAndQtyOfGoods.Text);
			AssertEquals("900015, 900016, 900017, 900018,", awbHeader.AWBRateLine6.NatureAndQtyOfGoods.Text);
			AssertEquals("900019, 900020, 900021, 900022,", awbHeader.AWBRateLine7.NatureAndQtyOfGoods.Text);
			AssertEquals("900023, 900024, 900025, 900026,", awbHeader.AWBRateLine8.NatureAndQtyOfGoods.Text);
			AssertEquals("900027, 900028, 900029, 900030,", awbHeader.AWBRateLine9.NatureAndQtyOfGoods.Text);
			AssertEquals("900031, 900032, 900033, 900034,", awbHeader.AWBRateLine10.NatureAndQtyOfGoods.Text);
			AssertEquals("900035, 900036, 900037, 900038,", awbHeader.AWBRateLine11.NatureAndQtyOfGoods.Text);
			AssertEquals("900039, 900040, 900041, 900042", awbHeader.AWBRateLine12.NatureAndQtyOfGoods.Text);
			AssertNoWarnings(awbHeader.AWBRateLine12.NatureAndQtyOfGoods.TextInfo);

			var hc2 = packLine2.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "JM";
			hc2.JLH_Code = "900080";

			awbHeader.Populate();

			var message = "More HS Codes exist but cannot be shown due to lack of space.";
			AssertNoWarnings(awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);
			AssertNoWarnings(awbHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo);
			AssertHasWarning(awbHeader.AWBRateLine12.NatureAndQtyOfGoods.TextInfo, message);

			awbHeader.EH_AreRateLinesOverridden = true;
			AssertNoWarnings("No warning when EH_AreRateLinesOverridden is true", awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);
			AssertNoWarnings("No warning when EH_AreRateLinesOverridden is true", awbHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo);
			AssertNoWarnings("No warning when EH_AreRateLinesOverridden is true", awbHeader.AWBRateLine12.NatureAndQtyOfGoods.TextInfo);

			awbHeader.EH_AreRateLinesOverridden = false;
			AssertNoWarnings("Only last HS Code line has the warning when EH_AreRateLinesOverridden is false", awbHeader.AWBRateLine10.NatureAndQtyOfGoods.TextInfo);
			AssertNoWarnings("Only last HS Code line has the warning when EH_AreRateLinesOverridden is false", awbHeader.AWBRateLine11.NatureAndQtyOfGoods.TextInfo);
			AssertHasWarning("Only last HS Code line has the warning when EH_AreRateLinesOverridden is false", awbHeader.AWBRateLine12.NatureAndQtyOfGoods.TextInfo, message);
		}

		public void TestHarmonizedCode_IsHSCodeLine()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "JMKIN";
			shipment.JS_RL_NKLoadPort = "JMKIN";

			awbHeader.EH_ParentID = shipment.PK;

			var packLines = shipment.OuterPackLines;

			var packLine1 = packLines.AddNew();
			packLine1.JL_PackageCount = 1;
			packLine1.JL_HarmonisedCode = "1.";
			AssertNoErrors(packLine1.JL_HarmonisedCodeInfo);

			var hc1 = packLine1.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "JM";
			hc1.JLH_Code = "123456789012345";

			var hc2 = packLine1.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "JM";
			hc2.JLH_Code = "123456789012346";

			var hc3 = packLine1.HarmonisedCodes.AddNew();
			hc3.JLH_RN_NKCountry = "JM";
			hc3.JLH_Code = "1...";
			AssertNoErrors(hc3.JLH_CodeInfo);

			awbHeader.Populate();

			AssertEquals("No Dimensions Available", awbHeader.AWBRateLine4.NatureAndQtyOfGoods.Text);
			AssertEquals("HS Codes: 123456789012345,", awbHeader.AWBRateLine2.NatureAndQtyOfGoods.Text);
			Assert(awbHeader.AWBRateLine2.IsHSCodeLine);

			AssertEquals("123456789012346, 1...", awbHeader.AWBRateLine3.NatureAndQtyOfGoods.Text);
			AssertNoMessageErrors(awbHeader.AWBRateLine3.NatureAndQtyOfGoods.TextInfo);
			Assert(awbHeader.AWBRateLine3.IsHSCodeLine);
		}

		#endregion

		#region Override Airline IATA code

		public void TestOverrideIATACode()
		{
			var jHeader = Factory.NewJobForTesting<JobHeader>();
			jHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jHeader.JH_ParentID = AWBHeader.Shipment.PK;
			jHeader.JH_JobNum = "Phony number";
			jHeader.JH_OA_AgentCollectAddr = AWBHeader.Consol.SendingForwarder.MainAddress.PK;
			jHeader.JH_OA_LocalChargesAddr = AWBHeader.Shipment.Consignor.MainAddress.PK;

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_IATA_ChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			chargeCode.AC_Desc = "Test Charge";
			NewJobCharge(jHeader, 10m, chargeCode, "Fake", AWBHeader.Shipment.Job.LocalCharges);
			NewJobCharge(jHeader, 50m, chargeCode, "Real", AWBHeader.Consol.SendingForwarder);

			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.ChargeCodes.DB, AWBHeader.AWBOtherCharges.Where(c => c.EO_Amount == 50m).First().EO_ChargeCode);

			var company = Factory.New<OrgHeader>();
			var miscServ = company.MiscServ;
			company.OH_RL_NKClosestPort = "USLAX";
			company.OH_FullName = "TestCompany";
			company.MainAddress.OA_Address1 = "TestAddress";
			company.OH_IsShippingProvider = true;
			company.OH_IsAirLine = true;
			miscServ.OM_RM_Airline = Factory.LoadFromNaturalKey<RefAirline>(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "081").PK;

			AWBHeader.Consol.JK_OA_ShippingLineAddress = company.MainAddress.PK;
			var airoverride = chargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airoverride.ACI_OH_Carrier = company.PK;
			airoverride.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			AWBHeader.Populate();
			AssertEquals(Core.Constants.AWB.ChargeCodes.AC, AWBHeader.AWBOtherCharges.Where(c => c.EO_Amount == 50m).First().EO_ChargeCode);
		}

		#endregion

		#region TestPopulateRateLinesSectionInfo_DefaultingWhenOverriden

		public void TestPopulateIfNotOverriden_DoesNotSetVolumeAndDimensionsTwice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = SetupShipment();
				shipment.JS_TotalPackageCount = 0;
				shipment.IsAWBValuesOverriddenProperty = true;

				var awbHeader = shipment.AWBHeader;
				awbHeader.EH_AreRateLinesOverridden = false;

				var expectedNatureAndQtyOfGoods =
@"GOODS DESCRIPTION
UDF statement
DIMS 3x3x3 CM x 1
VOL 3.590 M3" + string.Concat(Enumerable.Repeat("\r\n", 8));

				awbHeader.PopulateIfNotOverridden();
				AssertNatureAndQtyOfGoods("Volume and Dimensions lines should only occur once.", expectedNatureAndQtyOfGoods, awbHeader);

				expectedNatureAndQtyOfGoods =
@"UDF statement
No Dimensions Available" + string.Concat(Enumerable.Repeat("\r\n", 10));

				shipment.JS_GoodsDescription = "";
				shipment.JS_ActualVolume = 0M;
				shipment.JS_OuterPacks = 0;
				shipment.OuterPackLines.RemoveAndDeleteAll();
				awbHeader.PopulateIfNotOverridden();
				AssertNatureAndQtyOfGoods("No Dimensions Available line should only occur once.", expectedNatureAndQtyOfGoods, awbHeader);
			}
		}

		public void TestPopulateRateLinesSectionInfo_ConsistentDefaultingWithOverrides()
		{
			var shipment = SetupShipment();
			var awbHeader = shipment.AWBHeader;

			const string expectedNatureAndQtyOfGoods =
@"GOODS DESCRIPTION
UDF statement
DIMS 3x3x3 CM x 1
VOL 3.590 M3







4 SLAC";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				RepopulateAndAssertNatureAndQtyOfGoods("Populated (1)", expectedNatureAndQtyOfGoods, awbHeader);

				shipment.IsAWBValuesOverriddenProperty = true;
				awbHeader.EH_AreRateLinesOverridden = true;
				RepopulateAndAssertNatureAndQtyOfGoods("NOT populated (1)", expectedDummyNatureAndQtyOfGoods, awbHeader);

				shipment.IsAWBValuesOverriddenProperty = false;
				awbHeader.EH_AreRateLinesOverridden = true;
				RepopulateAndAssertNatureAndQtyOfGoods("NOT populated (2)", expectedDummyNatureAndQtyOfGoods, awbHeader);

				shipment.IsAWBValuesOverriddenProperty = true;
				awbHeader.EH_AreRateLinesOverridden = false;
				RepopulateAndAssertNatureAndQtyOfGoods("Populated (2)", expectedNatureAndQtyOfGoods, awbHeader);
			}
		}

		public void TestPopulateRateLinesSectionInfo_BcnShipment_ShouldPopulateWeightFromTheShipmentItSelf()
		{
			var shipment = SetupShipment();
			shipment.JS_PackingMode = ContainerModes.BuyersConsol;
			shipment.JS_ActualWeight = 400;
			shipment.JS_ActualChargeable = 500;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;

			var jobCharge1 = Factory.New<JobCharge>();
			jobCharge1.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge1.JR_JH = jobHeader.PK;
			jobCharge1.JR_OSSellAmt = 1000;

			var calculationLog = new CalculationLog();
			calculationLog.CalculatorCode = "UNT";
			calculationLog.Weight = 100;
			calculationLog.Chargeable = 200;
			calculationLog.Unit = "KG";
			calculationLog.AddPerUnitCalculation(200, "KG", 1);

			var logsWrapper = new CalculationLogsWrapper();
			logsWrapper.Logs.Add(calculationLog);

			CalculationLogsLoader.Save(jobCharge1, logsWrapper);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				// Non BCN Shipment
				shipment.JS_PackingMode = ContainerModes.FCL;
				var awbHeader = shipment.AWBHeader;
				awbHeader.Populate();

				var line = awbHeader.AWBRateLines[0];
				CombineAssertions("Amounts should come from from calculation", () =>
				{
					AssertEquals(nameof(line.ER_GrossWeight), 100m, line.ER_GrossWeight);
					AssertEquals(nameof(line.ER_ChargeableWeight), 200m, line.ER_ChargeableWeight);
					AssertEquals(nameof(line.ER_RateChargeOrDiscount), 1m, line.ER_RateChargeOrDiscount);
					AssertEquals(nameof(line.ER_Total), 200m, line.ER_Total);
				});

				// BCN shipment
				shipment.JS_PackingMode = ContainerModes.BuyersConsol;
				awbHeader = shipment.AWBHeader;
				awbHeader.Populate();

				line = awbHeader.AWBRateLines[0];
				CombineAssertions("Amounts should come from the shipment itself rather than from calculation", () =>
				{
					AssertEquals(nameof(line.ER_GrossWeight), 400m, line.ER_GrossWeight);
					AssertEquals(nameof(line.ER_ChargeableWeight), 500m, line.ER_ChargeableWeight);
					AssertEquals(nameof(line.ER_RateChargeOrDiscount), 2m, line.ER_RateChargeOrDiscount);
					AssertEquals(nameof(line.ER_Total), 1000m, line.ER_Total);
				});
			}
		}

		ForwardingShipment SetupShipment()
		{
			CountryExportStatementSettingCollection defaultValue = new CountryExportStatementSettingCollection();
			CountryExportStatementSetting sEDSetting = defaultValue.AddNew();
			sEDSetting.CountryCode = Core.Constants.CountryCodes.Australia;
			sEDSetting.Statements.Add(new ExportStatementSetting(sEDSetting, "GBH", "UDF statement", "", "", "", "UDF", true, true, true, true, true, true));
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			shipment.JS_TotalPackageCount = 4;
			shipment.JS_OuterPacks = 50;
			shipment.JS_ActualWeight = 4.4m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "GOODS DESCRIPTION";

			shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = Core.Constants.AWB.Dimensions.ALL;
			shipment.DocsAndCartage.JP_ExportStatement = "GBH";
			shipment.JS_ActualVolume = 3.59m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			ExportAWBHeaderTest.AddPackLineDimension(shipment.OuterPackLines);

			shipment.JS_HBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.ALL;
			return shipment;
		}

		#endregion

		#region TestHasValidScenarioToShowEOR

		public void TestHasValidScenarioToShowEOR()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "FRPAR";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "FRPAR";
			transport2.JW_RL_NKDiscPort = "DEHAM";

			awbHeader.Populate();

			Assert(awbHeader.IsImportToICS2Zone);

			shipment.JS_RL_NKDestination = "USLAX";
			transport2.JW_RL_NKDiscPort = "USLAX";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);

			transport2.JW_RL_NKDiscPort = "HKHKG";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);
		}

		public void TestHasNotValidScenarioToShowEOR()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_RL_NKOrigin = "FRPAR";
			shipment.JS_RL_NKDestination = "DEHAM";

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "FRPAR";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "DEHAM";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);
		}

		public void TestHasValidScenarioToShowEOR_TransferBySeaToIcs2Country()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_RL_NKDestination = "DEHAM";

			awbHeader.EH_ParentID = shipment.PK;

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USNYC";

			var transport2 = shipment.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "USNYC";
			transport2.JW_RL_NKDiscPort = "DEHAM";

			awbHeader.Populate();

			Assert(!awbHeader.IsImportToICS2Zone);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.Populate();

			Assert(awbHeader.IsImportToICS2Zone);
		}

		public void TestHasDestinationInIcs2Zone()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			awbHeader.EH_ParentID = shipment.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			Assert(awbHeader.HasDestinationInIcs2Zone);

			SetupNorthernIrelandZone();
			shipment.JS_RL_NKDestination = "GBBEL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			shipment.JS_RL_NKDestination = "CHBSL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			shipment.JS_RL_NKDestination = "NOOSL";
			Assert(awbHeader.HasDestinationInIcs2Zone);

			shipment.JS_RL_NKDestination = "HKHKG";
			Assert(!awbHeader.HasDestinationInIcs2Zone);
		}

		void SetupNorthernIrelandZone()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "NORTHERN IRELAND";
			}
		}

		#endregion

		#region Transform State For Japan

		public void TestTransformShipperStateForJapan()
		{
			var consignor = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.ConsignorPK = consignor.PK;
				awbHeader.EH_ParentID = shipment.PK;

				awbHeader.Populate();
				AssertEquals(consignor.MainAddress.PK, awbHeader.EH_OA_ShipperAddress);
				AssertEquals("Tokyo", awbHeader.EH_ShipperState);

				consignor.MainAddress.OA_State = "OSAKA-FU";
				awbHeader.Populate();
				AssertEquals("OSAKA-FU", awbHeader.EH_ShipperState);
			}
		}

		public void TestTransformConsigneeStateForJapan()
		{
			var consignee = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.ConsigneePK = consignee.PK;
				awbHeader.EH_ParentID = shipment.PK;

				awbHeader.Populate();
				AssertEquals(consignee.MainAddress.PK, awbHeader.EH_OA_ConsigneeAddress);
				AssertEquals("Tokyo", awbHeader.EH_ConsigneeState);

				consignee.MainAddress.OA_State = "OSAKA-FU";
				awbHeader.Populate();
				AssertEquals("OSAKA-FU", awbHeader.EH_ConsigneeState);
			}
		}

		public void TestTransformAlsoNotifyStateForJapan()
		{
			var notifyParty = SetupJPOrg();

			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Japanese))
			{
				var awbHeader = Factory.New<ShipmentExportAWBHeader>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
				awbHeader.EH_ParentID = shipment.PK;

				awbHeader.Populate();
				AssertEquals("Tokyo", awbHeader.EH_AlsoNotifyState);

				notifyParty.MainAddress.OA_State = "OSAKA-FU";
				awbHeader.Populate();
				AssertEquals("OSAKA-FU", awbHeader.EH_AlsoNotifyState);
			}
		}

		OrgHeader SetupJPOrg()
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_Code, "13"), JoinCondition.And,
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, CountryCodes.Japan)));

			var translateJapanese = Factory.New<RefLanguageText>();
			translateJapanese.RLT_ColumnName = "RW_Description";
			translateJapanese.RLT_Language = SharedConstants.Languages.Japanese;
			translateJapanese.RLT_ParentId = state.PK;
			translateJapanese.RLT_ParentTableCode = "RW";
			translateJapanese.RLT_Text = "東京都";

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST 1";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_City = "Tokyo City";
			org.MainAddress.OA_State = "13";
			org.OH_RL_NKClosestPort = "JPTYO";

			return org;
		}

		#endregion

		#region Transform State For China

		public void TestTransformShipperStateForChina()
		{
			var consignor = SetupCNOrg();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.Populate();
			AssertEquals(consignor.MainAddress.PK, awbHeader.EH_OA_ShipperAddress);
			AssertEquals("BEIJING", awbHeader.EH_ShipperState);

			consignor.MainAddress.OA_State = "SHANG-ZN";
			awbHeader.Populate();
			AssertEquals("SHANG-ZN", awbHeader.EH_ShipperState);
		}

		public void TestTransformConsigneeStateForChina()
		{
			var consignee = SetupCNOrg();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsigneePK = consignee.PK;
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.Populate();
			AssertEquals(consignee.MainAddress.PK, awbHeader.EH_OA_ConsigneeAddress);
			AssertEquals("BEIJING", awbHeader.EH_ConsigneeState);

			consignee.MainAddress.OA_State = "SHANG-ZN";
			awbHeader.Populate();
			AssertEquals("SHANG-ZN", awbHeader.EH_ConsigneeState);
		}

		public void TestTransformAlsoNotifyStateForChina()
		{
			var notifyParty = SetupCNOrg();

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
			awbHeader.EH_ParentID = shipment.PK;

			awbHeader.Populate();
			AssertEquals("BEIJING", awbHeader.EH_AlsoNotifyState);

			notifyParty.MainAddress.OA_State = "SHANG-ZN";
			awbHeader.Populate();
			AssertEquals("SHANG-ZN", awbHeader.EH_AlsoNotifyState);
		}

		public void TestTransformStateForChinaShouldNotChangeDescriptionInRefCountryStates()
		{
			var consignor = SetupCNOrg();
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "110";
			state.RW_Description = "Beij";
			state.RW_RN_NKCountryCode = Constants.CountryCodes.China;
			consignor.MainAddress.OA_State = "110";
			Factory.Save();
			AssertEquals("Beij", state.RW_Description);

			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.ConsignorPK = consignor.PK;
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.Populate();
			AssertEquals(consignor.MainAddress.PK, awbHeader.EH_OA_ShipperAddress);
			AssertEquals("BEIJ", awbHeader.EH_ShipperState);
			Factory.Save();

			AssertEquals("Beij", state.RW_Description);
		}

		OrgHeader SetupCNOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST 1";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_City = "Beijing City";
			org.MainAddress.OA_State = "11";
			org.OH_RL_NKClosestPort = "CNBJS";

			return org;
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestAgentShipperSignature_ForHongKongExportBorrowedMAWB()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "HKC";
			company.GC_Name = "HongKong Company";
			company.GC_RN_NKCountryCode = "HK";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = "AUBNE";
			branch.GB_Code = "HKB";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var borrowedFrom = Factory.NewWithValidTestData<OrgHeader>();
				borrowedFrom.OH_Code = "BRW";
				borrowedFrom.OH_FullName = "borrowed-from company full name";

				var mawb = Factory.NewWithValidTestData<JobMawb>();
				mawb.JM_Airline3DigitPrefix = "176";
				mawb.JM_MAWB = "10000001";
				mawb.JM_GB = branch.PK;
				mawb.JM_ServiceLevel = "STD";
				mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
				mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_AgentType = "AGT";
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.MasterBillAirlinePrefix = "176";
				consol.JK_IsNeutralMaster = true;

				mawb.JM_ParentID = consol.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = "PHS";

				Factory.Save();
				var initialUserContext = Env.CurrentUserContext;

				try
				{
					Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Issuing Carrier Name";
					shipment.PopulateAWB();
					AssertEquals("HAWB ShippersSignature should be login username for borrowed MAWB cross trade when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, shipment.AWBHeader.EH_ShippersSignature);

					shipment.JS_RL_NKDestination = "HKHKG";
					shipment.PopulateAWB();
					AssertEquals("HAWB ShippersSignature should be login username for Hong Kong import borrowed MAWB when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, shipment.AWBHeader.EH_ShippersSignature);

					shipment.JS_RL_NKOrigin = "HKHKG";
					shipment.JS_RL_NKDestination = "USCHI";
					consol.JK_RL_NKLoadPort = "HKHKG";
					consol.JK_RL_NKDischargePort = "NZAKL";
					mawb.JM_ParentID = consol.PK;
					shipment.PopulateAWB();
					AssertEquals("HAWB ShippersSignature should be login username for Hong Kong export borrowed MAWB when login branch is Hong Kong", GlbStaff.CurrentUser.GS_FullName, shipment.AWBHeader.EH_ShippersSignature);
					AssertEquals("HAWB AgentSignature should be issuing carrier name for Hong Kong export borrowed MAWB when login branch is Hong Kong", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, shipment.AWBHeader.EH_AWBAgentsSignature);
					AssertEquals("MAWB ShippersSignature should be issuing carrier for Hong Kong export borrowed MAWB when login branch is Hong Kong", Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName, consol.AWBHeader.EH_ShippersSignature);
					AssertEquals("MAWB AgentSignature should be borrow-from company full name for Hong Kong export borrowed MAWB when login branch is Hong Kong", borrowedFrom.OH_FullName.ToUpper(), consol.AWBHeader.EH_AWBAgentsSignature);

					mawb.JM_OA_From = ZGuid.Empty;
					shipment.PopulateAWB();
					AssertEquals("HAWB ShippersSignature should be login username for non-borrowed MAWB Hong Kong export when login branch is Hong Kong.", GlbStaff.CurrentUser.GS_FullName, shipment.AWBHeader.EH_ShippersSignature);

					var dgnCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
					dgnCertificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.DGN;
					dgnCertificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddYears(1);
					dgnCertificate.XZ_RefNumber = "XYZ578 TRU 324 Z53466576";
					shipment.PopulateAWB();
					const string expectedSignature = "CargoWise Support XYZ578 TRU 324 Z5";
					AssertEquals("Expected value's length should be equal to the max length of EH_ShippersSignature", 35, expectedSignature.Length);
					AssertEquals("HAWB ShippersSignature should be login username + DGN for non-borrowed MAWB Hong Kong export when login branch is Hong Kong.", expectedSignature, shipment.AWBHeader.EH_ShippersSignature);
				}
				finally
				{
					Env.SetUserContext(initialUserContext);
				}
			}
		}

		public void TestHAWBEH_ExtraShipperInfoLine2_HongKong()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_IsNeutralMaster = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_InspectionTypeCode = "PHS";

			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "176";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = consol.PK;

			var borrowedFrom = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var knownShipperDetails = borrowedFrom.MainAddress.KnownShipperDetails.AddNew();
			knownShipperDetails.OV_EXApprovedOrMajorExporter = "RAN";
			knownShipperDetails.OV_EXApprovalNumber = "RA12345";
			knownShipperDetails.OV_OH_OrgHeader = borrowedFrom.PK;
			mawb.JM_OA_From = borrowedFrom.MainAddress.PK;

			Factory.Save();

			string originalRegistryText = Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText;
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
				{
					shipment.AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();

					shipment.JS_InspectionTypeCode = "PHS";

					Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Short text < 64 chars";
					shipment.AWBHeader.Populate();
					AssertEquals("Approval code is shown on line 2", "SPX", shipment.AWBHeader.EH_ExtraShipperInfoLine2);

					shipment.JS_InspectionTypeCode = "UNK";
					Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Test Shipper Text greater than 64 characters but less than 128 characters";
					shipment.AWBHeader.Populate();
					AssertEquals("No approval", "haracters UNK", shipment.AWBHeader.EH_ExtraShipperInfoLine2);

					shipment.JS_InspectionTypeCode = "PHS";
					shipment.AWBHeader.Populate();
					AssertEquals("SPX is appended", "haracters SPX", shipment.AWBHeader.EH_ExtraShipperInfoLine2);

					Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = "Test Shipper Text greater than 64 characters and also greater than 128 characters so that it needs to be trimmed in order to append the security code";
					shipment.AWBHeader.Populate();
					AssertEquals("Line is trimmed", "an 128 characters so that it needs to be trimmed in order to SPX", shipment.AWBHeader.EH_ExtraShipperInfoLine2);
				}

				shipment.AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();
				shipment.AWBHeader.Populate();
				AssertEquals("If not Hong Kong then SPX is not appended", "an 128 characters so that it needs to be trimmed in order to app", shipment.AWBHeader.EH_ExtraShipperInfoLine2);
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.HAWBDefaultShipperText = originalRegistryText;
			}
		}

		public void TestPopulateSpecialHandlingItems_ConsolSpecialMax_WithoutUserNotification()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			consol.SecurityStatusCode = "SCO";
			var handling1 = consol.AWBSpecialHandlingItems.AddNew();
			var handling2 = consol.AWBSpecialHandlingItems.AddNew();
			var handling3 = consol.AWBSpecialHandlingItems.AddNew();
			var handling4 = consol.AWBSpecialHandlingItems.AddNew();
			var handling5 = consol.AWBSpecialHandlingItems.AddNew();
			var handling6 = consol.AWBSpecialHandlingItems.AddNew();
			var handling7 = consol.AWBSpecialHandlingItems.AddNew();
			var handling8 = consol.AWBSpecialHandlingItems.AddNew();
			var handling9 = consol.AWBSpecialHandlingItems.AddNew();

			handling1.JKH_Code = "ACT";
			handling2.JKH_Code = "AOG";
			handling3.JKH_Code = "PEB";
			handling4.JKH_Code = "RDS";
			handling5.JKH_Code = "BUP";
			handling6.JKH_Code = "ICE";
			handling7.JKH_Code = "CAO";
			handling8.JKH_Code = "CAT";
			handling9.JKH_Code = "CIC";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.AWBHeader.Populate();

			AssertNotContains("A maximum of nine Special Handling Codes is possible for the FWB message", UnitTestUserNotification.Instance.LastMessage.Text);

			var sqlText = $@"select * from ExportAWBHeader inner join JobShipment on EH_ParentID = JS_PK
inner join ExportAWBSpecialHandling on EH_PK = EP_EH";
			var count = 0;
			using (var cmd = Db.Connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					count = reader.GetInt32(0);
				}
			}
			AssertEquals(0, count);
		}

		public void TestGetTaxCodeInformationForBangladesh_MultipleVATTaxInfo_NoExceptionThrown()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			CreateRefDocOrgCusCode("VAT", CountryCodes.Australia, CountryCodes.Australia, 1, "VAT", ZString.Empty, "HAW");

			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				var awbHeader = shipment.AWBHeader;
			});
		}

		public void TestUniversalCopyShouldIgnoreInvoiceJobHeaderWhenCopyShipmentExportAWBHeader()
		{
			var ignoreElementAttributes = (UniversalCopyIgnoreElementAttribute[])(typeof(ShipmentExportAWBHeader).GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), false));
			AssertCollectionContains("InvoiceJobHeader", ignoreElementAttributes[0].ElementNames);
		}

		#region Load port is in EU, Discharge port is not in EU

		public void TestShipperTraderNoWhenTypeIsEORIAndIsNotImport2ICSMember()
		{
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "PTLIS";
			shipment.JS_RL_NKDestination = "AUSYD";

			awbHeader.EH_ParentID = shipment.PK;
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Full Name";
			consignor.MainAddress.OA_Address1 = "Address 1";
			consignor.MainAddress.OA_Address2 = "Address 2";
			consignor.MainAddress.OA_City = "City";
			consignor.OH_RL_NKClosestPort = "PTLIS";
			consignor.MainAddress.OA_PostCode = "Post Code";
			var customCode = consignor.CustomsCodes.AddNew();
			customCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Portugal;
			customCode.OK_CustomsRegNo = "XI123456789";
			shipment.ConsignorPK = consignor.PK;

			var code = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code.DOC_DocumentType = "HAW";
			code.DOC_Direction = "BTH";
			code.DOC_RN_NKCodeCountry = Constants.CountryCodes.Portugal;
			code.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Portugal;
			code.DOC_ShortLabel = "EORI NO.";
			code.DOC_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			Factory.Save();
			awbHeader.Populate();

			AssertEquals(string.Empty, awbHeader.EH_ShipperTraderNo);
			AssertEquals(string.Empty, awbHeader.EH_ShipperTraderNoType);
		}

		#endregion

		public void TestListExportStatements_CargoIMP()
		{
			TestListExportStatements_CargoIMP(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");
			void TestListExportStatements_CargoIMP(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var defaultValue = new CountryExportStatementSettingCollection();
					var exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
						true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW",
						"NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";
					ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					var header = Factory.New<ConsolExportAWBHeader>();
					header.EH_ParentID = consol.PK;

					var shipment = header.Consol.Shipments.AddNew();
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;

					shipment.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";

					var exportStatements = header.ExportStatements_CargoIMP;
					AssertEquals("X20100101987654", exportStatements[0].Statement.Trim());
					AssertEquals("PRF", exportStatements[0].Code);

					shipment.DocsAndCartage.JP_ExportStatement = "PDU";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment.ConsignorPK = consignor.PK;

					exportStatements = header.ExportStatements_CargoIMP;
					AssertEquals("PDU", exportStatements[0].Code);
					AssertEquals("12345678912 20101001", exportStatements[0].Statement.Trim());

					shipment.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);

					exportStatements = header.ExportStatements_CargoIMP;
					AssertEquals("DWN", exportStatements[0].Code);
					AssertEquals("111111111 20101001", exportStatements[0].Statement.Trim());

					shipment.DocsAndCartage.JP_ExportStatement = "LOW";
					exportStatements = header.ExportStatements_CargoIMP;
					AssertEquals("LOW", exportStatements[0].Code);
					AssertEquals(String.Empty, exportStatements[0].Statement.Trim());
				}
			}
		}

		public void TestListExportStatements_CargoIMP_EntryFilerIdNotSet()
		{
			TestListExportStatements_CargoIMP(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestListExportStatements_CargoIMP(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");
			void TestListExportStatements_CargoIMP(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var consol = Factory.New<ForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					var header = Factory.New<ConsolExportAWBHeader>();
					header.EH_ParentID = consol.PK;

					var shipment = header.Consol.Shipments.AddNew();
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = destination;

					shipment.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var exportStatements = header.ExportStatements_CargoIMP;
					AssertEquals("DWN", exportStatements[0].Code);
					AssertEquals("20101001", exportStatements[0].Statement.Trim());
				}
			}
		}

		#region Advance Cargo Reporting Self-Filer

		public void TestPopulateEH_IsConsigneeDeclarantForAdvancedCargoReporting()
		{
			AWBHeader.Populate();
			Assert(!AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			var shipment = AWBHeader.Shipment;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";

			var consol = AWBHeader.Consol;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "HKHKG";

			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var misc = receivingForwarder.MiscServ;

			AWBHeader.Populate();
			Assert(!AWBHeader.HasInboundToICS2Zone);

			misc.OM_FWAdvanceCargoReportingSelfFiler = true;
			Assert(consol.IsAdvanceCargoReportingSelfFiler);
			Assert("Should return false when HasInboundToICS2Zone is false", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			shipment.JS_RL_NKDestination = "DEHAM";
			transport.JW_RL_NKDiscPort = "DEHAM";
			AWBHeader.Populate();
			Assert(AWBHeader.HasInboundToICS2Zone);

			AWBHeader.Populate();
			Assert("Should get value from consol.IsAdvanceCargoReportingSelfFiler when is not direct shipment and HasInboundToICS2Zone is true", AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			misc.OM_FWAdvanceCargoReportingSelfFiler = false;
			Assert(!consol.IsAdvanceCargoReportingSelfFiler);
			AWBHeader.Populate();
			Assert("Should get value from consol.IsAdvanceCargoReportingSelfFiler when is not direct shipment and HasInboundToICS2Zone is true", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			AWBHeader.Consol.JK_AgentType = AgentType.Direct;
			shipment = AWBHeader.Consol.DirectShipment;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			misc = consignee.MiscServ;

			misc.OM_IMAdvanceCargoReportingSelfFiler = true;
			Assert(shipment.IsAdvanceCargoReportingSelfFiler);
			AWBHeader.Populate();
			Assert("Should get value from Directshipment.IsAdvanceCargoReportingSelfFiler when is direct shipment and HasInboundToICS2Zone is true", AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			misc.OM_IMAdvanceCargoReportingSelfFiler = false;
			Assert(!shipment.IsAdvanceCargoReportingSelfFiler);
			AWBHeader.Populate();
			Assert("Should get value from Directshipment.IsAdvanceCargoReportingSelfFiler when is direct shipment and HasInboundToICS2Zone is true", !AWBHeader.EH_IsConsigneeDeclarantForAdvanceCargoReporting);
		}

		#endregion

		#region Assertion helpers

		const string expectedDummyNatureAndQtyOfGoods =
@"custom1
custom2
custom3
custom4
custom5
custom6
custom7
custom8
custom9
custom10
custom11
custom12";

		void SetDummyNatureAndQtyOfGoods(ExportAWBHeader awbHeader)
		{
			for (int i = 0; i < 12; ++i)
			{
				awbHeader.AWBRateLines[i].NatureAndQtyOfGoodsDescription = "custom" + (i + 1);
			}
		}

		void RepopulateAndAssertNatureAndQtyOfGoods(string message, string expected, ExportAWBHeader awbHeader)
		{
			SetDummyNatureAndQtyOfGoods(awbHeader);
			awbHeader.Populate();
			AssertNatureAndQtyOfGoods(message, expected, awbHeader);
		}

		void AssertNatureAndQtyOfGoods(string message, string expected, ExportAWBHeader awbHeader)
		{
			AssertNotNull("header should not be null", awbHeader);

			var natureAndQtyOfGoods = awbHeader
				.AWBRateLines
				.Cast<ExportAWBRateLine>()
				.Select(line => string.Format("{0}", line.NatureAndQtyOfGoodsDescription));

			AssertEquals($"NatureAndQtyOfGoods: {message}",
				expected,
				string.Join("\r\n", natureAndQtyOfGoods),
				true);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			AWBHeader.EH_ParentID = shipment.PK;

			return AWBHeader;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		ZGuid PreviousOrgHeader;

		ZString previousCountryCode;

		protected override void SetUp()
		{
			PreviousOrgHeader = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			previousCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			base.SetUp();

			SetUpForDeparturePort("JMKIN");
		}

		void SetUpForDeparturePort(ZString departurePort)
		{
			var countryCode = departurePort.Left(2);

			GlbCompany.CurrentCompany.SetCountry(countryCode);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = departurePort;

			AWBHeader = Factory.New<ShipmentExportAWBHeaderForTest>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUMEL";
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = departurePort;

			var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consignorCode = consignor.OH_Code;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = departurePort;

			CountryStatementSettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
			CountryExportStatementSetting countrySetting = CountryStatementSettingCollection.AddNew();
			countrySetting.CountryCode = countryCode;
			ExportStatementSetting = countrySetting.Statements.AddNew();
			ExportStatementSetting.Code = "NDR";
			ExportStatementSetting.Statement = "STATEMENT FOR TESTING";
			ExportStatementSetting.Visibility = "UDF";
			UpdateCountryStatementSettingsToRegistry();

			AWBHeader.EH_ParentID = shipment.PK;
			var sendingForwarder = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignor.PK));
			sendingForwarder.OH_FullName = "Name";
			sendingForwarder.MainAddress.OA_Address1 = "Address1";
			sendingForwarder.MainAddress.OA_Address2 = "Address2";
			AWBHeader.Consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			AWBHeader.AWBOtherCharges.AddNew();
			AWBHeader.AWBOtherCharges.AddNew();

			AWBHeader.AWBAccountingInformations.AddNew();
			AWBHeader.AWBAccountingInformations.AddNew();
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = PreviousOrgHeader;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = previousCountryCode;
			base.TearDown();
		}

		void UpdateCountryStatementSettingsToRegistry()
		{
			FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CountryStatementSettingCollection);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;

			var header = factory.New<ShipmentExportAWBHeader>();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			AWBHeader.EH_ParentID = shipment.PK;

			return header;
		}

		ShipmentExportAWBHeaderForTest AWBHeader;
		CountryExportStatementSettingCollection CountryStatementSettingCollection;
		ExportStatementSetting ExportStatementSetting;
		ZString consignorCode;

		protected class ShipmentExportAWBHeaderForTest : ShipmentExportAWBHeader
		{
			public ShipmentExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public new void PopulateShipperContactDetails(IDocAddress address)
			{
				base.PopulateShipperContactDetails(address);
			}

			protected ZString ParentPrepaidCollect
			{
				get
				{
					return fPPDCLT;
				}
			}

			public void OverridePPDCLT(ZString pPDCLT)
			{
				fPPDCLT = pPDCLT;
			}
			ZString fPPDCLT;

			public ZString HandlingInformationForTest
			{
				get { return base.HandlingInformation; }
			}

			public new Transport DepartureFlight2
			{
				get { return base.DepartureFlight2; }
			}

			public new Transport DepartureFlight3
			{
				get { return base.DepartureFlight3; }
			}

			public new Transport DepartureFlight1
			{
				get { return base.DepartureFlight1; }
			}

			public new ZString OtherPPDCOL
			{
				get { return base.OtherPPDCOL; }
			}

			public new void PopulateIssuedBy()
			{
				base.PopulateIssuedBy();
			}

			public new void PopulateShortGoodsDescriptionforFHL()
			{
				base.PopulateShortGoodsDescriptionforFHL();
			}

			public new ZDecimal RateLineRateChargeOrDiscount
			{
				get { return base.RateLineRateChargeOrDiscount; }
			}

			public new ZBool IsConsigneeAdvanceCargoReportingSelfFilerSet
			{
				get { return base.IsConsigneeAdvanceCargoReportingSelfFilerSet; }
			}

			public new bool IsTaxAutoCalculated
			{
				get { return base.IsTaxAutoCalculated; }
			}

			public new JobDocAddress ConsigneeDocumentaryAddress
			{
				get { return base.ConsigneeDocumentaryAddress; }
			}

			public new JobDocAddress ShipperDocumentaryAddress
			{
				get { return base.ShipperDocumentaryAddress; }
			}

			public new ZString ExtraCarrierInfoLine2
			{
				get { return base.ExtraCarrierInfoLine2; }
			}

			public new ZString ExtraShipperInfoLine1
			{
				get { return base.ExtraShipperInfoLine1; }
			}

			public new ZString ExtraShipperInfoLine2
			{
				get { return base.ExtraShipperInfoLine2; }
			}

			public new DefaultAddressTypes DefaultConsigneeAddressType
			{
				get { return base.DefaultConsigneeAddressType; }
			}

			public new DefaultAddressTypes DefaultShipperAddressType
			{
				get { return base.DefaultShipperAddressType; }
			}

			public new OrgAddress ConsigneeOfficeAddress
			{
				get { return base.ConsigneeOfficeAddress; }
			}

			public new OrgAddress ConsigneeDeliveryAddress
			{
				get { return base.ConsigneeDeliveryAddress; }
			}

			public new ZString DefaultConsigneeCompanyName
			{
				get { return base.DefaultConsigneeCompanyName; }
			}

			public new List<OrgAddress> GetConsigneeAddresses
			{
				get { return base.GetConsigneeAddresses(); }
			}

			public new OrgAddress ShipperOfficeAddress
			{
				get { return base.ShipperOfficeAddress; }
			}

			public new OrgAddress ShipperPickupAddress
			{
				get { return base.ShipperPickupAddress; }
			}

			public new ZString DefaultShipperCompanyName
			{
				get { return base.DefaultShipperCompanyName; }
			}

			public new List<OrgAddress> GetShipperAddresses
			{
				get { return base.GetShipperAddresses(); }
			}

			public new JobDocAddress NotifyPartyDocumentaryAddress
			{
				get { return base.NotifyPartyDocumentaryAddress; }
			}

			public new List<OrgAddress> GetAlsoNotifyAddresses
			{
				get { return base.GetAlsoNotifyAddresses(); }
			}

			public new ZDecimal RateLineGrossWeight
			{
				get { return base.RateLineGrossWeight; }
			}

			public new ZString RateLineWeightUnit
			{
				get { return base.RateLineWeightUnit; }
			}
		}

		#endregion
	}
}
