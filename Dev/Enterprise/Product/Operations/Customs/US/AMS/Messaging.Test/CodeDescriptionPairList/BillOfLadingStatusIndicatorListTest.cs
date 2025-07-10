using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class BillOfLadingStatusIndicatorListTest : TestCaseWithFactory
	{
		public void TestGetCachedValue()
		{
			var list1 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, false);
			var list2 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, false);
			AssertEquals(true, Object.ReferenceEquals(list1, list2));
			var expectedList = new Tuple<string, string>[] {
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.RegularBill, BillOfLadingStatusIndicatorList.Descriptions.RegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade, BillOfLadingStatusIndicatorList.Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, BillOfLadingStatusIndicatorList.Descriptions.SimpleForeignRetainedOnBoard),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterBill, BillOfLadingStatusIndicatorList.Descriptions.MasterBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBill, BillOfLadingStatusIndicatorList.Descriptions.HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterFROB, BillOfLadingStatusIndicatorList.Descriptions.MasterFROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.FROB, BillOfLadingStatusIndicatorList.Descriptions.FROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularBillFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.HouseFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularInBondType62_63WithISF)
			};
			AssertEquals("No of elments don't match", expectedList.Length, list1.Count);
			foreach (var pair in expectedList)
			{
				AssertEquals(pair.Item1, pair.Item2, list1.GetDescriptionFromCode(pair.Item1));
			}

			list1 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, false);
			list2 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, false);
			AssertEquals(true, Object.ReferenceEquals(list1, list2));
			AssertEquals("ACE list is not the default list", false, Object.ReferenceEquals(list1, Factory.GetCachedValue<BillOfLadingStatusIndicatorList>()));
			expectedList = new Tuple<string, string>[] {
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.RegularBill, BillOfLadingStatusIndicatorList.Descriptions.RegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade, BillOfLadingStatusIndicatorList.Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, BillOfLadingStatusIndicatorList.Descriptions.SimpleForeignRetainedOnBoard),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBill, BillOfLadingStatusIndicatorList.Descriptions.HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.FROB, BillOfLadingStatusIndicatorList.Descriptions.FROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularBillFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.HouseFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularInBondType62_63WithISF)
			};
			AssertEquals("No of elments don't match", expectedList.Length, list1.Count);
			foreach (var pair in expectedList)
			{
				AssertEquals(pair.Item1, pair.Item2, list1.GetDescriptionFromCode(pair.Item1));
			}

			list1 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, true, false);
			list2 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, true, false);
			AssertEquals(true, Object.ReferenceEquals(list1, list2));
			AssertEquals("ACE list is not the default list", false, Object.ReferenceEquals(list1, Factory.GetCachedValue<BillOfLadingStatusIndicatorList>()));
			expectedList = new Tuple<string, string>[] {
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterBill, BillOfLadingStatusIndicatorList.Descriptions.MasterBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterFROB, BillOfLadingStatusIndicatorList.Descriptions.MasterFROB),
			};
			AssertEquals("No of elments don't match", expectedList.Length, list1.Count);
			foreach (var pair in expectedList)
			{
				AssertEquals(pair.Item1, pair.Item2, list1.GetDescriptionFromCode(pair.Item1));
			}
		}

		public void TestAMSHBREIsPartOfCachingKey()
		{
			var nvoccList = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, true);
			var voccList = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, true);
			AssertEquals("NVOCC List should have code when AMSHBRE is active", BillOfLadingStatusIndicatorList.Descriptions.Sec321SimpleRegularBill, nvoccList.GetDescriptionFromCode(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill));
			AssertEquals("VOCC List should have code when AMSHBRE is active", BillOfLadingStatusIndicatorList.Descriptions.Sec321SimpleRegularBill, voccList.GetDescriptionFromCode(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill));

			nvoccList = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, false);
			voccList = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, false);
			AssertEquals("NVOCC List should not have code when AMSHBRE is not active", false, nvoccList.ContainsCode(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill));
			AssertEquals("VOCC List should not have code when AMSHBRE is not active", false, voccList.ContainsCode(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill));
		}

		public void TestIsTariffRequiredwhenQRST()
		{
			AssertEquals("Q", true, BillOfLadingStatusIndicatorList.IsTariffRequired(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF));
			AssertEquals("S", true, BillOfLadingStatusIndicatorList.IsTariffRequired(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond));
			AssertEquals("R", true, BillOfLadingStatusIndicatorList.IsTariffRequired(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF));
			AssertEquals("T", true, BillOfLadingStatusIndicatorList.IsTariffRequired(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF));
			AssertEquals("B", false, BillOfLadingStatusIndicatorList.IsTariffRequired(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard));
		}

		public void TestIsHouseBillofLoadingWhenTypeAMSHBREIsActive()
		{
			var list1 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, true);
			var list2 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, false, false, true);
			AssertEquals(true, Object.ReferenceEquals(list1, list2));
			var expectedList = new Tuple<string, string>[] {
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.RegularBill, BillOfLadingStatusIndicatorList.Descriptions.RegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade, BillOfLadingStatusIndicatorList.Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, BillOfLadingStatusIndicatorList.Descriptions.SimpleForeignRetainedOnBoard),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterBill, BillOfLadingStatusIndicatorList.Descriptions.MasterBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBill, BillOfLadingStatusIndicatorList.Descriptions.HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.MasterFROB, BillOfLadingStatusIndicatorList.Descriptions.MasterFROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.FROB, BillOfLadingStatusIndicatorList.Descriptions.FROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularBillFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.HouseFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularInBondType62_63WithISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill, BillOfLadingStatusIndicatorList.Descriptions.Sec321SimpleRegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.Sec321HouseBill, BillOfLadingStatusIndicatorList.Descriptions.Sec321HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereHouseBOLIs62_63InBond)
				};
			AssertEquals("No of elments don't match", expectedList.Length, list1.Count);
			foreach (var pair in expectedList)
			{
				AssertEquals(pair.Item1, pair.Item2, list1.GetDescriptionFromCode(pair.Item1));
			}

			list1 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, true);
			list2 = BillOfLadingStatusIndicatorList.GetCachedValue(Factory, true, false, true);
			AssertEquals(true, Object.ReferenceEquals(list1, list2));
			AssertEquals("ACE list is not the default list", false, Object.ReferenceEquals(list1, Factory.GetCachedValue<BillOfLadingStatusIndicatorList>()));
			expectedList = new Tuple<string, string>[] {
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.RegularBill, BillOfLadingStatusIndicatorList.Descriptions.RegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade, BillOfLadingStatusIndicatorList.Descriptions.EmptyEquipmentInstrumentsOfInternationalTrade),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard, BillOfLadingStatusIndicatorList.Descriptions.SimpleForeignRetainedOnBoard),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBill, BillOfLadingStatusIndicatorList.Descriptions.HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.FROB, BillOfLadingStatusIndicatorList.Descriptions.FROB),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularBillFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Descriptions.HouseFROBAndISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereMasterBOLIs62_63InBond),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF, BillOfLadingStatusIndicatorList.Descriptions.SimpleRegularInBondType62_63WithISF),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.Sec321SimpleRegularBill, BillOfLadingStatusIndicatorList.Descriptions.Sec321SimpleRegularBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.Sec321HouseBill, BillOfLadingStatusIndicatorList.Descriptions.Sec321HouseBill),
				new Tuple<string, string>(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Descriptions.HouseBillAndISFWhereHouseBOLIs62_63InBond)
				};
			AssertEquals("No of elments don't match", expectedList.Length, list1.Count);
			foreach (var pair in expectedList)
			{
				AssertEquals(pair.Item1, pair.Item2, list1.GetDescriptionFromCode(pair.Item1));
			}
		}

		public void TestIsOceanBillOfLading()
		{
			var list = new BillOfLadingStatusIndicatorList();
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.MasterBill);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.MasterFROB);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(pair.Code + ": " + pair.Description, ZBool.False, BillOfLadingStatusIndicatorList.IsOceanBillOfLading(pair.Code));
			}

			foreach (var code in new[] { BillOfLadingStatusIndicatorList.Codes.MasterBill,
				BillOfLadingStatusIndicatorList.Codes.MasterFROB })
			{
				AssertEquals(code, ZBool.True, BillOfLadingStatusIndicatorList.IsOceanBillOfLading(code));
			}
		}

		public void TestIsISF()
		{
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsISF(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, false));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsISF(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, false));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsISF(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, false));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsISF(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF, false));

			var list = new BillOfLadingStatusIndicatorList();
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(ZBool.False, BillOfLadingStatusIndicatorList.IsISF(pair.Code, false));
			}

			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsISF(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereHouseBOLIs62_63InBond, true));
		}

		public void TestIsNVOCC()
		{
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsNVOCC(BillOfLadingStatusIndicatorList.Codes.HouseBill));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsNVOCC(BillOfLadingStatusIndicatorList.Codes.FROB));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsNVOCC(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsNVOCC(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond));

			var list = new BillOfLadingStatusIndicatorList();
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.HouseBill);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.FROB);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF);
			list.RemoveCode(BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond);
			foreach (ICodeDescription pair in list)
			{
				AssertEquals(ZBool.False, BillOfLadingStatusIndicatorList.IsNVOCC(pair.Code));
			}
		}

		public void TestIsEligibleForPTT()
		{
			AssertEquals(ZBool.False, BillOfLadingStatusIndicatorList.IsEligibleForPTT(BillOfLadingStatusIndicatorList.Codes.HouseBill));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsEligibleForPTT(BillOfLadingStatusIndicatorList.Codes.RegularBill));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsEligibleForPTT(BillOfLadingStatusIndicatorList.Codes.MasterBill));
			AssertEquals(ZBool.True, BillOfLadingStatusIndicatorList.IsEligibleForPTT(BillOfLadingStatusIndicatorList.Codes.MasterFROB));
		}
	}
}
