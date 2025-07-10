using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitShareDetailsLookups : AutoOrgProfitShareDetailsLookups
	{
		public OrgProfitShareDetailsLookups(AutoOrgProfitShareDetails parent) : base(parent)
		{
		}

		#region Locations

		public LocationCollection Locations =>
			Factory.GetCachedValue("LocationCollectionWithZones", () => new LocationCollection(Factory, true));

		#endregion

		#region Agreement Types

		public const string AgreementTypeFreight = "PCF";
		public const string AgreementTypeFreightChargeOnly = "FRT";
		public const string AgreementTypeCollectFreight = "CLF";
		public const string AgreementTypeOrigin = "ORG";
		public const string AgreementTypeDestination = "DST";
		public const string AgreementTypeAll = "ALL";
		public const string AgreementTypeFreightOrigin = "FOR";
		public const string AgreementTypeFreightDestination = "FDS";
		public const string AgreementTypeCustom = "CUS";
		public const string AgreementTypeUserDefined = "USR";

		public CodeDescriptionPairList AgreementTypes
		{
			get { return Factory.GetCachedValue("OrgProfitShareDetailsLookups.AgreementTypes", () => new AgreementTypesList()); }
		}

		public class AgreementTypesList : CodeDescriptionPairList
		{
			public AgreementTypesList()
			{
				AddPair(AgreementTypeFreight, Res.GetString("90de83ba-592e-4058-8018-8e406fd5e040", "Prepaid and Collect Freight Charges"));
				AddPair(AgreementTypeFreightChargeOnly, Res.GetString("ae1bdfc8-40a8-447c-b56c-babc3b2add52", "Freight Charge Only"));
				AddPair(AgreementTypeCollectFreight, Res.GetString("164c0dd5-d20b-4e1e-a9de-0455fabd867d", "Collect Freight"));
				AddPair(AgreementTypeOrigin, Res.GetString("efc6291c-5a72-44d8-bebe-f17a800539e8", "Origin Charges"));
				AddPair(AgreementTypeDestination, Res.GetString("46ed6511-7102-43df-8fa5-5a751adee809", "Destination Charges"));
				AddPair(AgreementTypeAll, Res.GetString("10ad07e4-9423-492e-b9e8-9f00ec03a50d", "All Charges"));
				AddPair(AgreementTypeFreightOrigin, Res.GetString("08d24809-40ec-4ce4-ab3d-dc833e4f1b4b", "Prepaid and Collect Freight and Origin"));
				AddPair(AgreementTypeFreightDestination, Res.GetString("3cf845f5-47b3-48fc-a8bc-0e1169e13390", "Prepaid and Collect Freight and Destination"));
				AddPair(AgreementTypeCustom, Res.GetString("789b5d2e-d583-45e1-a4f3-34387c7f4ce9", "Custom List of Charge Codes from Registry"));
				AddPair(AgreementTypeUserDefined, Res.GetString("b2216929-3e70-4cf2-8a02-3d82928df172", "User Defined List of Charge Codes"));
			}
		}

		#endregion

		#region Freight Modes

		public CodeDescriptionPairList FreightModes
		{
			get { return Factory.GetCachedValue("OrgProfitShareDetailsLookups.FreightModesList", () => new FreightModesList()); }
		}

		public class FreightModesList : CodeDescriptionPairList
		{
			public FreightModesList()
			{
				Add(ALL);
				Add(AIR);
				Add(ULD);
				Add(LSE);
				Add(SEA);
				Add(RAI);
				Add(ROA);
				Add(FCL);
				Add(LCL);
				Add(BLK);
				Add(LQD);
				Add(BBK);
				Add(ROR);
				Add(FTL);
				Add(LTL);
				Add(GRP);
				Add(BCN);
				Add(OTH);
			}

			public static CodeDescriptionPair ALL { get { return new CodeDescriptionPair("ALL", Res.GetString("8d512582-bfb7-4e8a-b32d-6f8397e03ed7", "All Transport Modes")); } }
			public static CodeDescriptionPair AIR { get { return new CodeDescriptionPair("AIR", Res.GetString("c58ddd6c-49b4-4def-9641-a5693a23e91d", "Air (ULD, LSE, BCN)")); } }
			public static CodeDescriptionPair ULD { get { return new CodeDescriptionPair("ULD", Res.GetString("808CE873-F8FD-4A2C-9C80-2526FD1091B4", "Unit Load Device (Air)")); } }
			public static CodeDescriptionPair LSE { get { return new CodeDescriptionPair("LSE", Res.GetString("d38a0750-8cb0-4442-b3d8-81b983482962", "Loose (Air)")); } }
			public static CodeDescriptionPair SEA { get { return new CodeDescriptionPair("SEA", Res.GetString("491a4898-a21a-4b35-b2bc-863f7272e1b7", "Sea (FCL, LCL, BLK, LQD, BBK, ROR, GRP, BCN)")); } }
			public static CodeDescriptionPair RAI { get { return new CodeDescriptionPair("RAI", Res.GetString("f5356f6e-fce5-493d-b4e4-39613024c472", "Rail (FCL, LCL, BLK, LQD, BBK, GRP, BCN)")); } }
			public static CodeDescriptionPair ROA { get { return new CodeDescriptionPair("ROA", Res.GetString("bcdea5be-5be2-487a-8c1a-45a609e81659", "Road (FCL, LCL, FTL, LTL, GRP, BCN)")); } }
			public static CodeDescriptionPair FCL { get { return new CodeDescriptionPair("FCL", Res.GetString("c7b85453-49eb-4be5-b82d-58984bfd6874", "Full Container Load (Sea/Rail/Road)")); } }
			public static CodeDescriptionPair LCL { get { return new CodeDescriptionPair("LCL", Res.GetString("515ca518-d763-4c80-93e6-08d75072c56b", "Less Container Load (Sea/Rail/Road)")); } }
			public static CodeDescriptionPair BLK { get { return new CodeDescriptionPair("BLK", Res.GetString("ddfd2576-6581-4174-a0ae-8a617e7aa697", "Bulk (Sea/Rail/Road)")); } }
			public static CodeDescriptionPair LQD { get { return new CodeDescriptionPair("LQD", Res.GetString("5333af06-2383-461d-bd40-3a319d956d34", "Liquid (Sea/Rail)")); } }
			public static CodeDescriptionPair BBK { get { return new CodeDescriptionPair("BBK", Res.GetString("5206e819-161b-472d-99bd-a358837e4b31", "Break Bulk (Sea/Rail)")); } }
			public static CodeDescriptionPair ROR { get { return new CodeDescriptionPair("ROR", Res.GetString("7e794036-f743-4e6e-a48d-a7e6eef32903", "Roll On/Roll Off (Sea)")); } }
			public static CodeDescriptionPair FTL { get { return new CodeDescriptionPair("FTL", Res.GetString("05adc46b-fd47-47cc-89bd-80ff66ff8354", "Full Truck Load (Road)")); } }
			public static CodeDescriptionPair LTL { get { return new CodeDescriptionPair("LTL", Res.GetString("9a09ba85-ffb9-4bd7-9db5-8453cf511244", "Less Truck Load (Road)")); } }
			public static CodeDescriptionPair OTH { get { return new CodeDescriptionPair("OTH", Res.GetString("6f8f450b-a34b-4250-88d7-463803bb533d", "Other (Air/Sea/Rail/Road)")); } }
			public static CodeDescriptionPair GRP { get { return new CodeDescriptionPair("GRP", Res.GetString("823f1a4f-5806-45f1-96b2-2d615ccdbdc1", "Groupage/Freight All Kinds (Sea/Rail/Road)")); } }
			public static CodeDescriptionPair BCN { get { return new CodeDescriptionPair("BCN", Res.GetString("49e8faa7-82bd-4942-8308-51de12be6aca", "Buyer’s Consolidation (Air/Sea/Rail/Road)")); } }
		}

		#endregion

		#region OrgOverrideTypes 

		public CodeDescriptionPairList OrgOverrideTypes
		{
			get { return Factory.GetCachedValue("OrgProfitShareDetailsLookups.OrgOverrideTypesList", () => new OrgOverrideTypesList()); }
		}

		public class OrgOverrideTypesList : CodeDescriptionPairList
		{
			public OrgOverrideTypesList()
			{
				Add(LOC);
				Add(CNE);
				Add(CNR);
				Add(PUA);
				Add(IBR);
				Add(EBR);
				Add(ALL);
			}

			public static CodeDescriptionPair LOC { get { return new CodeDescriptionPair("LOC", Res.GetString("8f8a3608-a6e9-4b9e-9601-5c74bb28ed50", "Local Client")); } }
			public static CodeDescriptionPair CNE { get { return new CodeDescriptionPair("CNE", Res.GetString("ab39c898-4ab5-49e9-b517-b0cc331c06e8", "Consignee")); } }
			public static CodeDescriptionPair CNR { get { return new CodeDescriptionPair("CNR", Res.GetString("d227be78-7a3d-4080-a146-946752f9e033", "Consignor")); } }
			public static CodeDescriptionPair PUA { get { return new CodeDescriptionPair("PUA", Res.GetString("f6e52e13-863b-4ede-9366-5fed4dbeccbb", "Pick-up Agent")); } }
			public static CodeDescriptionPair IBR { get { return new CodeDescriptionPair("IBR", Res.GetString("586781a8-4cd2-462c-8cfc-e796ec118cbb", "Import Broker")); } }
			public static CodeDescriptionPair EBR { get { return new CodeDescriptionPair("EBR", Res.GetString("1c3a5a85-ab15-458f-a328-bbd6f08f03a9", "Export Broker")); } }
			public static CodeDescriptionPair ALL { get { return new CodeDescriptionPair("ALL", Res.GetString("ee42c67f-400e-4eb6-868f-04851600f52e", "All")); } }
		}

		#endregion

		#region User Charge Codes

		public AccChargeCodeCollection CurrentCompanyChargeCodes
		{
			get
			{
				return Factory.GetCachedValue("CurrentCompanyChargeCodes", () =>
					new AccChargeCodeCollection(Factory, new ZQuery(), GlbCompany.CurrentCompany.PK.ToGuid()));
			}
		}

		#endregion

		#region Job Types

		public CodeDescriptionPairList JobTypes =>
			Factory.GetCachedValue("OrgProfitShareDetailsLookups|JobTypes", () => new JobTypesList());

		#endregion

		#region Gateway Agent Type conditions

		public CodeDescriptionPairList GatewayAgentTypes =>
			Factory.GetCachedValue("OrgProfitShareDetailsLookups|GatewayAgentTypes", () => new GatewayAgentTypesList());

		#endregion

		#region Gateway Profit Apportionment Method

		public CodeDescriptionPairList GatewayProfitApportionmentMethods =>
			Factory.GetCachedValue("OrgProfitShareDetailsLookups|GatewayProfitApportionmentMethods", () => new GatewayProfitApportionmentMethodList());

		#endregion
	}
}
