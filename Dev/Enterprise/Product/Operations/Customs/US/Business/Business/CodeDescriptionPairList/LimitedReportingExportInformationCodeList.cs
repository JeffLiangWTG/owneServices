
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class LimitedReportingExportInformationCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CR = ExportInformationCodeList.Codes.CR;
			public const string DD = ExportInformationCodeList.Codes.DD;
			public const string GP = ExportInformationCodeList.Codes.GP;
			public const string GS = ExportInformationCodeList.Codes.GS;
			public const string HH = ExportInformationCodeList.Codes.HH;
			public const string HV = ExportInformationCodeList.Codes.HV;
			public const string IS = ExportInformationCodeList.Codes.IS;
			public const string MS = ExportInformationCodeList.Codes.MS;
			public const string TE = ExportInformationCodeList.Codes.TE;
			public const string TL = ExportInformationCodeList.Codes.TL;
			public const string UG = ExportInformationCodeList.Codes.UG;
		}

		public static class Descriptions
		{
			public const string CR = ExportInformationCodeList.Descriptions.CR;
			public const string DD = ExportInformationCodeList.Descriptions.DD;
			public const string GP = ExportInformationCodeList.Descriptions.GP;
			public const string GS = ExportInformationCodeList.Descriptions.GS;
			public const string HH = ExportInformationCodeList.Descriptions.HH;
			public const string HV = ExportInformationCodeList.Descriptions.HV;
			public const string IS = ExportInformationCodeList.Descriptions.IS;
			public const string MS = ExportInformationCodeList.Descriptions.MS;
			public const string TE = ExportInformationCodeList.Descriptions.TE;
			public const string TL = ExportInformationCodeList.Descriptions.TL;
			public const string UG = ExportInformationCodeList.Descriptions.UG;
		}

		public LimitedReportingExportInformationCodeList()
		{
			AddPair(Codes.CR, Descriptions.CR);
			AddPair(Codes.DD, Descriptions.DD);
			AddPair(Codes.GP, Descriptions.GP);
			AddPair(Codes.GS, Descriptions.GS);
			AddPair(Codes.HH, Descriptions.HH);
			AddPair(Codes.HV, Descriptions.HV);
			AddPair(Codes.IS, Descriptions.IS);
			AddPair(Codes.MS, Descriptions.MS);
			AddPair(Codes.TE, Descriptions.TE);
			AddPair(Codes.TL, Descriptions.TL);
			AddPair(Codes.UG, Descriptions.UG);
		}
	}
}
