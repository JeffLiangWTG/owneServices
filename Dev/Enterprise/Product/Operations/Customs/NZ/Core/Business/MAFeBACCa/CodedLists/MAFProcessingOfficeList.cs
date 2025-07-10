using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class MAFProcessingOfficeList : CodeDescriptionEnumList<BACCApplicationTypeDetailsMAFProcessingOffice>
	{
		public static class Codes
		{
			public const string Auckland = "AKL";
			public const string Christchurch = "CHC";
			public const string Dunedin = "DUD";
			public const string Gisborne = "GIS";
			public const string Hamilton = "HLZ";
			public const string Invercargill = "IVC";
			public const string Napier = "NPE";
			public const string Nelson = "NSN";
			public const string NewPlymouth = "NPL";
			public const string PalmerstonNorth = "PMR";
			public const string Tauranga = "TRG";
			public const string Timaru = "TIU";
			public const string Wellington = "WLG";
			public const string Whangarei = "WRE";
		}

		public static class Descriptions
		{
			public const string Auckland = "Auckland";
			public const string Christchurch = "Christchurch";
			public const string Dunedin = "Dunedin";
			public const string Gisborne = "Gisborne";
			public const string Hamilton = "Hamilton";
			public const string Invercargill = "Invercargill";
			public const string Napier = "Napier";
			public const string Nelson = "Nelson";
			public const string NewPlymouth = "New Plymouth";
			public const string PalmerstonNorth = "Palmerston North";
			public const string Tauranga = "Tauranga";
			public const string Timaru = "Timaru";
			public const string Wellington = "Wellington";
			public const string Whangarei = "Whangarei";
		}

		public MAFProcessingOfficeList()
		{
			AddPair(Codes.Auckland, Descriptions.Auckland, BACCApplicationTypeDetailsMAFProcessingOffice.Auckland);
			AddPair(Codes.Christchurch, Descriptions.Christchurch, BACCApplicationTypeDetailsMAFProcessingOffice.Christchurch);
			AddPair(Codes.Dunedin, Descriptions.Dunedin, BACCApplicationTypeDetailsMAFProcessingOffice.Dunedin);
			AddPair(Codes.Gisborne, Descriptions.Gisborne, BACCApplicationTypeDetailsMAFProcessingOffice.Gisborne);
			AddPair(Codes.Hamilton, Descriptions.Hamilton, BACCApplicationTypeDetailsMAFProcessingOffice.Hamilton);
			AddPair(Codes.Invercargill, Descriptions.Invercargill, BACCApplicationTypeDetailsMAFProcessingOffice.Invercargill);
			AddPair(Codes.Napier, Descriptions.Napier, BACCApplicationTypeDetailsMAFProcessingOffice.Napier);
			AddPair(Codes.Nelson, Descriptions.Nelson, BACCApplicationTypeDetailsMAFProcessingOffice.Nelson);
			AddPair(Codes.NewPlymouth, Descriptions.NewPlymouth, BACCApplicationTypeDetailsMAFProcessingOffice.NewPlymouth);
			AddPair(Codes.PalmerstonNorth, Descriptions.PalmerstonNorth, BACCApplicationTypeDetailsMAFProcessingOffice.PalmerstonNorth);
			AddPair(Codes.Tauranga, Descriptions.Tauranga, BACCApplicationTypeDetailsMAFProcessingOffice.Tauranga);
			AddPair(Codes.Timaru, Descriptions.Timaru, BACCApplicationTypeDetailsMAFProcessingOffice.Timaru);
			AddPair(Codes.Wellington, Descriptions.Wellington, BACCApplicationTypeDetailsMAFProcessingOffice.Wellington);
			AddPair(Codes.Whangarei, Descriptions.Whangarei, BACCApplicationTypeDetailsMAFProcessingOffice.Whangarei);
		}

		public ZString GetDefaultOfficeCodeFromUNLOCO(ZString unloco)
		{
			string possibleOfficeCode = unloco.SubstringSafe(2, 3);
			if (possibleOfficeCode.Length == 3 && ContainsCode(possibleOfficeCode))
			{
				return possibleOfficeCode;
			}
			return Codes.Auckland;
		}
	}
}
