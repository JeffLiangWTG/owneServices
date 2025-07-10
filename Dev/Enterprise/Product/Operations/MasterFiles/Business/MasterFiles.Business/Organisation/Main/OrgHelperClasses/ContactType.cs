using System.Collections;
using System.ComponentModel;
using CargoWise.Common.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ImmutableObject(true)]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ContactType : IContactType
	{
		ContactType(string code, ContactBrandingType brandingType)
			: this(code, (NoResString)"", brandingType)
		{
		}

		ContactType(string code, MultilingualString defaultName, ContactBrandingType brandingType)
			: this(code, defaultName, code, "ALL", 0, brandingType)
		{
		}

		ContactType(string code, MultilingualString defaultName, string baseType, string parentType, int aggregateLevel, ContactBrandingType brandingType)
		{
			this.Code = code;
			this.DefaultName = defaultName;
			this.AggregateBaseType = baseType;
			this.AggregateParentType = parentType;
			this.AggregateLevel = aggregateLevel;
			this.BrandingType = brandingType;

			lock (CodeLookupTable)
			{
				CodeLookupTable.Add(code, this);
			}
		}

		#region Contact Types

		public static readonly ContactType All = new ContactType("ALL", ResString.GetMultilingualString("4c718ae5-103d-4da4-83a0-c575889f398a", "All Documents"), "", "", 0, ContactBrandingType.Client);
		public static readonly ContactType Receivables = new ContactType("A/R", ResString.GetMultilingualString("f2ccfd1f-ad05-48ad-992d-bf7fa013f4ed", "The Accounts Payable Manager"), ContactBrandingType.Client);
		public static readonly ContactType Payables = new ContactType("A/P", ResString.GetMultilingualString("a43d7466-964e-4fbb-9d22-3876edf8fb4e", "The Accounts Receivable Manager"), ContactBrandingType.Client);
		public static readonly ContactType Consignee = new ContactType("CNE", ResString.GetMultilingualString("610ed264-1360-44dd-9b3a-af7bd5b24488", "The Import Manager"), ContactBrandingType.Client);
		public static readonly ContactType Consignor = new ContactType("CNR", ResString.GetMultilingualString("ccc0b08a-ec2b-4ff1-abe0-305443e15bfe", "The Export Manager"), ContactBrandingType.Client);
		public static readonly ContactType TransportServices = new ContactType("TRC", ResString.GetMultilingualString("a0842977-d595-4e26-bbf1-691d16d4a1d8", "The Logistics Manager"), ContactBrandingType.Client);
		public static readonly ContactType Warehouse = new ContactType("WHS", ResString.GetMultilingualString("a0842977-d595-4e26-bbf1-691d16d4a1d8", "The Logistics Manager"), ContactBrandingType.Client);
		public static readonly ContactType Sales = new ContactType("SAL", ResString.GetMultilingualString("64487a42-ee6d-4046-af49-0bc2f623a4e3", "The Sales Manager"), ContactBrandingType.Client);
		public static readonly ContactType Marketing = new ContactType("MRK", ResString.GetMultilingualString("7a6b2e6b-1c1a-4daa-a0ef-c38908f26e86", "The Marketing Manager"), ContactBrandingType.Client);
		public static readonly ContactType FreightAgent = new ContactType("FWD", ResString.GetMultilingualString("a9446d74-7f42-47da-9e57-18d0e73269a1", "The Freight Manager"), "FWD", "ALL", 1, ContactBrandingType.Agent);
		public static readonly ContactType ExportFreightAgent = new ContactType("FWE", ResString.GetMultilingualString("e40e7d51-b1af-4737-a031-83c0f38594bb", "The Export Freight Manager"), "FWD", "FWD", 2, ContactBrandingType.Agent);
		public static readonly ContactType ImportFreightAgent = new ContactType("FWI", ResString.GetMultilingualString("f3e92226-b9ae-4cf2-99bc-d5724ad4a4b5", "The Import Freight Manager"), "FWD", "FWD", 2, ContactBrandingType.Agent);
		public static readonly ContactType ImportSeaFreightAgent = new ContactType("FIS", ResString.GetMultilingualString("ac203fa0-743b-4e5e-b890-ce4ead547459", "The Import Sea Freight Manager"), "FWD", "FWI", 3, ContactBrandingType.Agent);
		public static readonly ContactType ImportAirFreightAgent = new ContactType("FIA", ResString.GetMultilingualString("3bc2ff73-90f5-47f6-a69b-ddb8f89f54d8", "The Import Air Freight Manager"), "FWD", "FWI", 3, ContactBrandingType.Agent);
		public static readonly ContactType ExportSeaFreightAgent = new ContactType("FES", ResString.GetMultilingualString("4ca4acae-f115-448a-b6f8-e4b7a432bad1", "The Export Sea Freight Manager"), "FWD", "FWE", 3, ContactBrandingType.Agent);
		public static readonly ContactType ExportAirFreightAgent = new ContactType("FEA", ResString.GetMultilingualString("5b229492-8218-468a-8daa-80ecb65b397a", "The Export Air Freight Manager"), "FWD", "FWE", 3, ContactBrandingType.Agent);
		public static readonly ContactType ShippingLine = new ContactType("SHP", ResString.GetMultilingualString("d1e176d3-acad-4712-90bb-ad07bf1e9223", "The Freight Manager"), ContactBrandingType.Agent);
		public static readonly ContactType CTO = new ContactType("CTO", ResString.GetMultilingualString("90fa6ee6-3d60-4aad-a044-a323b958a210", "The Terminal Manager"), "CTO", "ALL", 1, ContactBrandingType.Agent);
		public static readonly ContactType Depot = new ContactType("CFS", ResString.GetMultilingualString("67cce40d-d39a-4742-b8d9-07da92f0a53e", "The Depot Manager"), "CFS", "ALL", 1, ContactBrandingType.Agent);
		public static readonly ContactType ExportDepot = new ContactType("PAK", ResString.GetMultilingualString("e40e7d51-b1af-4737-a031-83c0f38594bb", "The Export Freight Manager"), "CFS", "CFS", 2, ContactBrandingType.Agent);
		public static readonly ContactType ImportDepot = new ContactType("UNP", ResString.GetMultilingualString("f3e92226-b9ae-4cf2-99bc-d5724ad4a4b5", "The Import Freight Manager"), "CFS", "CFS", 2, ContactBrandingType.Agent);
		public static readonly ContactType ExportSeaDepot = new ContactType("PKS", ResString.GetMultilingualString("4ca4acae-f115-448a-b6f8-e4b7a432bad1", "The Export Sea Freight Manager"), "CFS", "PAK", 3, ContactBrandingType.Agent);
		public static readonly ContactType ExportAirDepot = new ContactType("PKA", ResString.GetMultilingualString("5b229492-8218-468a-8daa-80ecb65b397a", "The Export Air Freight Manager"), "CFS", "PAK", 3, ContactBrandingType.Agent);
		public static readonly ContactType ImportSeaDepot = new ContactType("UNS", ResString.GetMultilingualString("ac203fa0-743b-4e5e-b890-ce4ead547459", "The Import Sea Freight Manager"), "CFS", "UNP", 3, ContactBrandingType.Agent);
		public static readonly ContactType ImportAirDepot = new ContactType("UNA", ResString.GetMultilingualString("3bc2ff73-90f5-47f6-a69b-ddb8f89f54d8", "The Import Air Freight Manager"), "CFS", "UNP", 3, ContactBrandingType.Agent);
		public static readonly ContactType AirWholesaler = new ContactType("AIR", ResString.GetMultilingualString("b5fd95d7-0652-46f2-9d7f-fb12d23d4849", "The Air Freight Manager"), ContactBrandingType.Agent);
		public static readonly ContactType LocalTransport = new ContactType("TRN", ResString.GetMultilingualString("247c7e63-ef9c-4fa3-9053-413c4ed41a5f", "The Transport Manager"), ContactBrandingType.Agent);
		public static readonly ContactType Warehouse3PL = new ContactType("3PL", ResString.GetMultilingualString("4392f65c-b449-4695-b902-a5dc676810ac", "The Warehouse Manager"), ContactBrandingType.Agent);
		public static readonly ContactType CustomerService = new ContactType("CSV", ResString.GetMultilingualString("f80d5ab8-5e7d-4470-aae2-35003d0c7530", "The Customer Service Manager"), ContactBrandingType.Client);
		public static readonly ContactType Administration = new ContactType("ADM", ResString.GetMultilingualString("0ea5857c-4202-40cd-9961-fc3443c0bd43", "The Administration Manager"), ContactBrandingType.Client);
		public static readonly ContactType NotifyParty = new ContactType("NOT", ContactBrandingType.Client);
		public static readonly ContactType Miscellaneous = new ContactType("MSC", ContactBrandingType.Client);
		public static readonly ContactType NoContactType = new ContactType("NCT", ContactBrandingType.Client);
		public static readonly ContactType LocalClient = new ContactType("LOC", ResString.GetMultilingualString("cc9dfded-6c44-4a02-95ab-c47844d1a721", "Local Client"), ContactBrandingType.Client);
		public static readonly ContactType ExportBroker = new ContactType("BRE", ResString.GetMultilingualString("28434291-e7c3-4c66-82e6-752d45699d1b", "The Export Broker"), ContactBrandingType.Agent);
		public static readonly ContactType ImportBroker = new ContactType("BRI", ResString.GetMultilingualString("03071dfa-68b2-4f87-95ac-27e5a749ae08", "The Import Broker"), ContactBrandingType.Agent);
		public static readonly ContactType CommissionAgreementRecipient = new ContactType("CAR", ResString.GetMultilingualString("f6ab8d8b-a53a-48ac-a46e-05a7c532c65d", "The Commission Agreement Recipient"), ContactBrandingType.Client);
		public static readonly ContactType TransitWarehouse = new ContactType("TWH", ContactBrandingType.Agent);
		public static readonly ContactType VerifiedGrossWeightContact = new ContactType("VGM", ResString.GetMultilingualString("360d1a99-4351-4300-8812-e88f5e1e6bd2", "Verified Gross Weight Contact"), ContactBrandingType.Agent);
		public static readonly ContactType NettingParticipantStatement = new ContactType("NPS", ResString.GetMultilingualString("2b7f06b5-89c8-4b81-8201-17362fd43924", "Netting Participant Statement"), ContactBrandingType.Client);
		public static readonly ContactType NettingClearingJournal = new ContactType("NCJ", ResString.GetMultilingualString("2f1a054b-b0b6-41ee-9ea0-0a1806d894fe", "Netting Clearing Journal"), ContactBrandingType.Client);
		public static readonly ContactType ControllingCustomer = new ContactType("CCU", ResString.GetMultilingualString("b2b90876-3f6d-4982-8c1d-cd97a7d5c515", "Controlling Customer"), ContactBrandingType.Agent);
		public static readonly ContactType ControllingAgent = new ContactType("CAG", ResString.GetMultilingualString("3de3f7fd-7600-48bb-a3e0-c3dd96ab1f3b", "Controlling Agent"), ContactBrandingType.Agent);
		public static readonly ContactType Applicant = new ContactType("APP", ResString.GetMultilingualString("566FDA2B-6E59-43EA-A266-4F09D9ECA9A9", "Applicant"), ContactBrandingType.Agent);
		public static readonly ContactType Importer = new ContactType("IMP", ResString.GetMultilingualString("4CF5BF7A-5F4D-44D5-A45D-355D74FA3366", "Importer"), ContactBrandingType.Client);
		public static readonly ContactType Declarant = new ContactType("DEC", ResString.GetMultilingualString("084E6DF9-078A-4567-99F7-306FF0256FE8", "Declarant"), ContactBrandingType.Agent);
		public static readonly ContactType Principal = new ContactType("PRI", ResString.GetMultilingualString("9242543E-F02B-4D49-A13B-C79F37DEE5B3", "Principal"), ContactBrandingType.Client);

		#endregion

		public ContactType CalculateInitialContactType(string transportMode)
		{
			ContactType result;
			var modeIsAir = transportMode == Core.Constants.TransportModes.Air;

			if (this == ImportFreightAgent)
			{
				result = modeIsAir ? ImportAirFreightAgent : ImportSeaFreightAgent;
			}
			else if (this == ExportFreightAgent)
			{
				result = modeIsAir ? ExportAirFreightAgent : ExportSeaFreightAgent;
			}
			else if (this == ImportDepot)
			{
				result = modeIsAir ? ImportAirDepot : ImportSeaDepot;
			}
			else if (this == ExportDepot)
			{
				result = modeIsAir ? ExportAirDepot : ExportSeaDepot;
			}
			else
			{
				result = this;
			}
			return result;
		}

		#region Implementation

		public readonly string Code;
		public readonly MultilingualString DefaultName;
		public readonly string AggregateBaseType;
		public readonly string AggregateParentType;
		public readonly int AggregateLevel;
		public readonly ContactBrandingType BrandingType;

		static Hashtable CodeLookupTable
		{
			get { return codeLookupTable ?? (codeLookupTable = new Hashtable()); }
		}
		[SuppressThreadStaticFieldMessage]
		static Hashtable codeLookupTable;

		public static ContactType Find(string code)
		{
			lock (CodeLookupTable)
			{
				object result = CodeLookupTable[code];
				return result as ContactType;
			}
		}

		public static string[] FindRelatedAggregateTypes(ContactType type)
		{
			lock (CodeLookupTable)
			{
				ArrayList list = new ArrayList();
				foreach (string code in CodeLookupTable.Keys)
				{
					if (((ContactType)CodeLookupTable[code]).AggregateBaseType == type.AggregateBaseType)
					{
						list.Add(code);
					}
				}
				return (string[])list.ToArray(typeof(string));
			}
		}

		public override string ToString()
		{
			return Code;
		}

		public static implicit operator string(ContactType value)
		{
			return value == null ? null : value.Code;
		}

		#endregion

		#region IContactType Members

		ContactBrandingType IContactType.BrandingType
		{
			get { return BrandingType; }
		}

		#endregion

	}
}
