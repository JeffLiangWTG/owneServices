using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	partial class ISFBondActivityCodeList : Integration.Customs.US.ISF.IBondActivityCodeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static ZString GetCode(ZString declarationActiveCode)
		{
			ZString result = ZString.Empty;
			switch (declarationActiveCode)
			{
				case ActivityCodeList.Codes._1:
				case ActivityCodeList.Codes._1a1:
					result = Codes.ImporterOrBroker;
					break;
				case ActivityCodeList.Codes._2:
					result = Codes.CustodianOfBondedMerchandise;
					break;
				case ActivityCodeList.Codes._3:
				case ActivityCodeList.Codes._3a3:
					result = Codes.InternationalCarrier;
					break;
				case ActivityCodeList.Codes._4:
					result = Codes.ForeignTradeZoneOperator;
					break;
				case ActivityCodeList.Codes._16:
					result = Codes.ISFBond16;
					break;
			}
			return result;
		}

		public static List<ZString> GetEquivalentDeclarationActiveCodeList()
		{
			return new List<ZString>(new ZString[] { ActivityCodeList.Codes._1, ActivityCodeList.Codes._1a1, ActivityCodeList.Codes._2, ActivityCodeList.Codes._3, ActivityCodeList.Codes._3a3, ActivityCodeList.Codes._4 });
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList() => this;
	}
}
