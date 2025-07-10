using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Report.Testing
{
	class TestFactoryParent
	{
		protected BusinessObjectFactory factory;

		public TestFactoryParent(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
	}

	sealed class SupplierFactory : TestFactoryParent
	{
		readonly string alphaCode = "SUP";
		int codeCounter;

		public SupplierFactory(BusinessObjectFactory factory) : base(factory) { }

		public OrgHeader CreateSupplier()
		{
			var supplier = factory.New<OrgHeader>();
			supplier.OH_Code = alphaCode + (codeCounter++).ToString("0#", CultureInfo.InvariantCulture);
			supplier.OH_FullName = supplier.OH_Code + "_FullName";
			return supplier;
		}
	}

	sealed class ImporterFactory : TestFactoryParent
	{
		readonly string alphaCode = "IMP";
		int codeCounter;

		public ImporterFactory(BusinessObjectFactory factory) : base(factory) { }

		public OrgHeader CreateImporter()
		{
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = alphaCode + (codeCounter++).ToString("0#", CultureInfo.InvariantCulture);
			importer.OH_FullName = importer.OH_Code + "_FullName";
			return importer;
		}
	}

	sealed class CommercialInvoiceHeaderFactory : TestFactoryParent
	{
		public CommercialInvoiceHeaderFactory(BusinessObjectFactory factory) : base(factory) { }

		public JobComInvoiceHeader CreateInvoice_with_Lines(int noOfLines, CommercialInvoiceLineFactory invoiceLineFactory)
		{
			var invoice = factory.New<JobComInvoiceHeader>();

			for (int a = 0; a < noOfLines; a++)
			{
				invoiceLineFactory.CreateInvoiceLine(invoice);
			}
			return invoice;
		}
	}

	sealed class CommercialInvoiceLineFactory : TestFactoryParent
	{
		int lineCounter;
		readonly decimal customsQuantity = 1111;
		readonly decimal customsSecondQuantity = 2222;
		readonly decimal customsThirdQuantity = 3333;
		readonly string customsUnitQty = "U";
		readonly string customsSecondUnitQty = "V";
		readonly string customsThirdUnitQty = "W";
		readonly string orderNumber = "Order-";
		readonly string partNo = "PartNo-";
		readonly CountryProvider countryProvider = new CountryProvider();

		public CommercialInvoiceLineFactory(BusinessObjectFactory factory) : base(factory) { }

		public JobComInvoiceLine CreateInvoiceLine(JobComInvoiceHeader invoice)
		{
			lineCounter++;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CustomsQuantity = customsQuantity + lineCounter;
			invoiceLine.JI_CustomsSecondQuantity = customsSecondQuantity + (2 * lineCounter);
			invoiceLine.JI_CustomsThirdQuantity = customsThirdQuantity + (3 * lineCounter);
			invoiceLine.JI_CustomsUnitQty = customsUnitQty + lineCounter;
			invoiceLine.JI_CustomsSecondUnitQty = customsSecondUnitQty + lineCounter;
			invoiceLine.JI_CustomsThirdUnitQty = customsThirdUnitQty + lineCounter;
			invoiceLine.JI_OrderNumber = orderNumber + lineCounter;
			invoiceLine.JI_PartNo = partNo + lineCounter;
			invoiceLine.JI_CountryOfOrigin = countryProvider.GetCountryCode(lineCounter);

			return invoiceLine;
		}
	}

	sealed class DeclarationFactory : TestFactoryParent
	{
		int decCounter;
		int containerCounter;
		readonly string vesselName = "BlueOcean";
		readonly string voyageFlightNo = "BA";
		readonly ZDate eTA = new ZDate(2019, 10, 25);
		readonly string clientRef = "ClientRef-";
		readonly string transportDocNo = "TxDoc-";
		readonly string houseBill = "HouseBill-";
		readonly string customsOffice = "OF";
		readonly ZDate jobRegistrationDate = new ZDate(2019, 03, 28);
		readonly string containerPrefix = "NNN-";

		public DeclarationFactory(BusinessObjectFactory factory) : base(factory) { }

		public JobDeclaration CreateDeclaration_with_CusContainers(string transportMode, string messageType, int noOfContainers)
		{
			decCounter++;

			var dec = factory.New<JobDeclaration>();
			dec.JE_TransportMode = transportMode;
			dec.JE_MessageType = messageType;
			dec.JE_VesselName = vesselName + decCounter;
			dec.JE_VoyageFlightNo = voyageFlightNo + decCounter.ToString("00#", CultureInfo.InvariantCulture);
			dec.JE_DateOfArrival = eTA.AddDays(decCounter);
			dec.JE_OwnerRef = clientRef + decCounter;
			dec.JE_MasterBill = transportDocNo + decCounter;
			dec.JE_HouseBill = houseBill + decCounter;
			dec.JE_CustomsOffice = customsOffice + decCounter;
			dec.JE_SystemCreateTimeUtc = jobRegistrationDate.AddDays(3 * decCounter);

			for (int a = 0; a < noOfContainers; a++)
			{
				containerCounter++;
				var container = dec.CusContainers.AddNew();
				container.CO_ContainerNumber = containerPrefix + containerCounter.ToString("00#", CultureInfo.InvariantCulture);
			}

			return dec;
		}
	}

	sealed class EntryHeaderFactory : TestFactoryParent
	{
		int entryCounter;
		readonly ZDate submissionDate = new ZDate(2017, 07, 31);
		readonly ZDate assessmentDate = new ZDate(2017, 12, 31);
		readonly ZDate entryReleaseDate = new ZDate(2018, 12, 31);
		readonly string[] messageStatusList = { "ACK", "AWA", "AWO", "CC", "ERR" };

		public EntryHeaderFactory(BusinessObjectFactory factory) : base(factory) { }

		public CusEntryHeader CreateEntry_with_Lines(JobDeclaration dec, short noOfLines, EntryLineFactory lineFactory, string customsOfficeOverride = "")
		{
			entryCounter++;

			var cusEntryInstr = factory.New<CusEntryInstruction>();
			cusEntryInstr.CEI_JE = dec.PK;
			cusEntryInstr.CEI_Style = "A" + entryCounter.ToString("0#", CultureInfo.InvariantCulture);
			cusEntryInstr.CEI_DateForDuty = assessmentDate.AddDays(3 * entryCounter);
			cusEntryInstr.CEI_CustomsOfficeOverride = customsOfficeOverride;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = cusEntryInstr.PK;
			entry.CH_EntrySubmittedDate = submissionDate.AddDays(3 * entryCounter);
			entry.CH_EntryReleaseDate = entryReleaseDate.AddDays(3 * entryCounter);
			entry.CH_MessageType = dec.JE_MessageType;
			entry.CH_Status = messageStatusList[(entryCounter - 1) % 5];
			entry.CH_EntryStatus = entryCounter.ToString(CultureInfo.InvariantCulture);

			for (short a = 0; a < noOfLines; a++)
			{
				lineFactory.CreateEntryLine(entry, (short)(a + 1));
			}

			var entryNum = factory.New<CusEntryNumber>();
			entryNum.CE_EntryNum = "MRN-" + entryCounter.ToString("0#", CultureInfo.InvariantCulture);
			entryNum.CE_RN_NKCountryCode = "ZA";
			entryNum.CE_EntryType = "MRN";
			entryNum.CE_ParentID = entry.PK;
			entryNum.CE_ParentTable = "CusEntryHeader";

			return entry;
		}
	}

	sealed class EntryLineFactory : TestFactoryParent
	{
		int lineCounter;
		readonly decimal customsValue = 5678;
		readonly string tariffCode = "8.8.8.";

		public EntryLineFactory(BusinessObjectFactory factory) : base(factory) { }

		public CusEntryLine CreateEntryLine(CusEntryHeader entry, short lineNumber)
		{
			lineCounter++;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = lineNumber;
			entryLine.CL_CustomsValue = customsValue + lineCounter;
			entryLine.CL_AdValoremTariff = tariffCode + lineCounter;
			entryLine.CL_SystemCreateTimeUtc = entryLine.Declaration.JE_SystemCreateTimeUtc;

			return entryLine;
		}

		public CusEntryLine CreateEntryLine(CusEntryHeader entry, int clusterKey)
		{
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CH = entry.PK;
			entryLine.CL_ClusterKey = clusterKey;
			entryLine.CL_SystemCreateTimeUtc = entryLine.Declaration.JE_SystemCreateTimeUtc;
			return entryLine;
		}
	}

	sealed class OrganizationFactory : TestFactoryParent
	{
		int codeCounter;

		public OrganizationFactory(BusinessObjectFactory factory) : base(factory) { }

		public OrgHeader CreateOrganization(string alphaCode)
		{
			var supplier = factory.New<OrgHeader>();
			supplier.OH_Code = $"{alphaCode}{codeCounter++.ToString("0#", CultureInfo.InvariantCulture)}";
			supplier.OH_FullName = $"{supplier.OH_Code}_FullName";
			return supplier;
		}
	}

	sealed class WarehouseFactory : TestFactoryParent
	{
		readonly string alphaCode = "WHS";
		int codeCounter;

		public WarehouseFactory(BusinessObjectFactory factory) : base(factory) { }

		public OrgHeader CreateWareHouse()
		{
			var warehouse = factory.New<OrgHeader>();
			var code = $"{alphaCode}{codeCounter++.ToString("0#", CultureInfo.InvariantCulture)}";
			warehouse.OH_Code = code;
			warehouse.OH_FullName = $"{code}_FullName";
			warehouse.OH_RL_NKClosestPort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			warehouse.MainAddress.OA_Address1 = $"{code} ADDRESS";
			warehouse.MainAddress.LocalControlledPremisesID = "23423";
			warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			return warehouse;
		}
	}

	sealed class EntryLineFeeFactory : TestFactoryParent
	{
		public EntryLineFeeFactory(BusinessObjectFactory factory) : base(factory) { }

		public CusEntryLineFee CreateEntryLineFee(CusEntryLine entryLine, string chargeType, float chargeAmount, int clusterKey, string source = "CW1")
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_ChargeType = chargeType;
			lineFee.CF_ChargeAmount = chargeAmount;
			lineFee.CF_Source = source;
			lineFee.CF_IsLandedCostOnly = false;
			lineFee.CF_ClusterKey = clusterKey;
			return lineFee;
		}
	}

	sealed class EntryNumFactory : TestFactoryParent
	{
		public EntryNumFactory(BusinessObjectFactory factory) : base(factory) { }

		public CusEntryNumber CreateEntryNum(ZGuid parentID, string parentTable, string entryNum, string entryType, string category, string countryCode)
		{
			var cusEntryNum = factory.New<CusEntryNumber>();
			cusEntryNum.CE_ParentID = parentID;
			cusEntryNum.CE_ParentTable = parentTable;
			cusEntryNum.CE_EntryNum = entryNum;
			cusEntryNum.CE_EntryType = entryType;
			cusEntryNum.CE_Category = category;
			cusEntryNum.CE_RN_NKCountryCode = countryCode;
			return cusEntryNum;
		}
	}

	sealed class CountryProvider
	{
		readonly string[] countryCodes =
		{
			"AD", "AE", "AF", "AG", "AI", "AL", "AM", "AN", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX", "AZ", "BA", "BB", "BD", "BE", "BF", "BG", "BH",
			"BI", "BJ", "BL", "BM", "BN", "BO", "BQ", "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK", "CL", "CM",
			"CN", "CO", "CR", "CS", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", "FI",
			"FJ", "FK", "FM", "FO", "FR", "GA", "GB", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS", "GT", "GU", "GW", "GY",
			"HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE", "KG", "KH", "KI",
			"KM", "KN", "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MC", "MD", "ME", "MF", "MG",
			"MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ", "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI", "NL",
			"NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RS",
			"RU", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS", "ST", "SV", "SX", "SY", "SZ", "TC",
			"TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO", "TR", "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE",
			"VG", "VI", "VN", "VU", "WF", "WS", "XK", "XZ", "YE", "YT", "ZA", "ZM", "ZW"
		};

		public string GetCountryCode(int index)
		{
			return countryCodes[index];
		}
	}
}
