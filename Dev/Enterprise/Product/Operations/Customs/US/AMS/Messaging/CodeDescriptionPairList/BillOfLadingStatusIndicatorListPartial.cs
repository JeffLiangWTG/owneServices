using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class BillOfLadingStatusIndicatorList
	{
		public static CodeDescriptionPairList GetCachedValue(BusinessObjectFactory factory, bool isNVOCC, bool isOceanBillType, bool isAMSHBREffective)
		{
			var type = "M";
			var extraSuffix = isAMSHBREffective ? "Y" : "N";
			if (isNVOCC)
			{
				type = isOceanBillType ? "O" : "N";
			}
			return factory.GetCachedValue("AMSBillOfLadingStatusIndicatorListNew" + type + extraSuffix, () =>
			{
				switch (type)
				{
					case "O":
						return GetOceanBillOfLadingList();
					case "N":
						return GetNonOceanBillOfLadingList(isAMSHBREffective);
					default:
						return GetNewACEList(isAMSHBREffective);
				}
			});
		}

		static CodeDescriptionPairList GetNewACEList(bool isAMSHBREffective)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.RegularBill, Descriptions.RegularBill);
			result.AddPair(Codes.EmptyEquipmentInstrumentsOfInternationalTrade, Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade);
			result.AddPair(Codes.SimpleForeignRetainedOnBoard, Descriptions.SimpleForeignRetainedOnBoard);
			result.AddPair(Codes.MasterBill, Descriptions.MasterBill);
			result.AddPair(Codes.HouseBill, Descriptions.HouseBill);
			result.AddPair(Codes.MasterFROB, Descriptions.MasterFROB);
			result.AddPair(Codes.FROB, Descriptions.FROB);
			result.AddPair(Codes.SimpleRegularBillFROBAndISF, Descriptions.SimpleRegularBillFROBAndISF);
			result.AddPair(Codes.HouseFROBAndISF, Descriptions.HouseFROBAndISF);
			result.AddPair(Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond);
			result.AddPair(Codes.SimpleRegularInBondType62_63WithISF, Descriptions.SimpleRegularInBondType62_63WithISF);
			AddAMSHBR(result, isAMSHBREffective);
			return result;
		}

		static CodeDescriptionPairList GetNonOceanBillOfLadingList(bool isAMSHBREffective)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.RegularBill, Descriptions.RegularBill);
			result.AddPair(Codes.EmptyEquipmentInstrumentsOfInternationalTrade, Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade);
			result.AddPair(Codes.SimpleForeignRetainedOnBoard, Descriptions.SimpleForeignRetainedOnBoard);
			result.AddPair(Codes.HouseBill, Descriptions.HouseBill);
			result.AddPair(Codes.FROB, Descriptions.FROB);
			result.AddPair(Codes.SimpleRegularBillFROBAndISF, Descriptions.SimpleRegularBillFROBAndISF);
			result.AddPair(Codes.HouseFROBAndISF, Descriptions.HouseFROBAndISF);
			result.AddPair(Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond);
			result.AddPair(Codes.SimpleRegularInBondType62_63WithISF, Descriptions.SimpleRegularInBondType62_63WithISF);
			AddAMSHBR(result, isAMSHBREffective);
			return result;
		}

		static void AddAMSHBR(CodeDescriptionPairList list, bool isAMSHBREffective)
		{
			if (isAMSHBREffective)
			{
				list.AddPair(Codes.Sec321SimpleRegularBill, Descriptions.Sec321SimpleRegularBill);
				list.AddPair(Codes.Sec321HouseBill, Descriptions.Sec321HouseBill);
				list.AddPair(Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond, Descriptions.HouseBillAndISFWhereHouseBOLIs62_63InBond);
			}
		}

		static CodeDescriptionPairList GetOceanBillOfLadingList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.MasterBill, Descriptions.MasterBill);
			result.AddPair(Codes.MasterFROB, Descriptions.MasterFROB);
			return result;
		}

		public static bool IsNVOCC(ZString code)
		{
			return code == Codes.HouseBill ||
				code == Codes.FROB ||
				code == Codes.HouseFROBAndISF ||
				code == Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond;
		}

		public static bool IsAMSHBR(ZString code)
		{
			return code == Codes.Sec321HouseBill ||
				code == Codes.Sec321SimpleRegularBill ||
				code == Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond;
		}

		public static bool IsISF(ZString code, bool isAMSHBREffective)
		{
			return code == Codes.SimpleRegularBillFROBAndISF ||
				code == Codes.HouseFROBAndISF ||
				code == Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond ||
				code == Codes.SimpleRegularInBondType62_63WithISF ||
				isAMSHBREffective && code == Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond;
		}

		public static bool IsTariffRequired(ZString code)
		{
			return code == Codes.SimpleRegularBillFROBAndISF
			|| code == Codes.HouseFROBAndISF
			|| code == Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond
			|| code == Codes.SimpleRegularInBondType62_63WithISF;
		}

		public static bool IsEligibleForPTT(ZString code)
		{
			return code == Codes.RegularBill ||
				code == Codes.MasterBill ||
				code == Codes.MasterFROB;
		}

		public static bool IsOceanBillOfLading(ZString code)
		{
			return code == Codes.MasterBill ||
				code == Codes.MasterFROB;
		}
	}
}
