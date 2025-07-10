using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Registry.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	public class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.ACCFuelLevy, EntryChargeTypeList.Descriptions.ACCFuelLevy, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.ACCLevyCredit, EntryChargeTypeList.Descriptions.ACCLevyCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.ALACLevy, EntryChargeTypeList.Descriptions.ALACLevy, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.ALACLevyCredit, EntryChargeTypeList.Descriptions.ALACLevyCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[4], EntryChargeTypeList.Codes.AntiDumpingDuty, EntryChargeTypeList.Descriptions.AntiDumpingDuty, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[5], EntryChargeTypeList.Codes.CountervailingDuty, EntryChargeTypeList.Descriptions.CountervailingDuty, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[6], EntryChargeTypeList.Codes.DepositRefund, EntryChargeTypeList.Descriptions.DepositRefund, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[7], EntryChargeTypeList.Codes.Duty, EntryChargeTypeList.Descriptions.Duty, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[8], EntryChargeTypeList.Codes.DutyCredit, EntryChargeTypeList.Descriptions.DutyCredit, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[9], EntryChargeTypeList.Codes.EntryFee, EntryChargeTypeList.Descriptions.EntryFee, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[10], EntryChargeTypeList.Codes.EntryFeeGST, EntryChargeTypeList.Descriptions.EntryFeeGST, true, EntryChargeTypeList.Codes.EntryFee);
			AssertListElementIsCorrect(ChargeTypeList[11], EntryChargeTypeList.Codes.ExciseDutyCredit, EntryChargeTypeList.Descriptions.ExciseDutyCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[12], EntryChargeTypeList.Codes.GST, EntryChargeTypeList.Descriptions.GST, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[13], EntryChargeTypeList.Codes.GSTCredit, EntryChargeTypeList.Descriptions.GSTCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[14], EntryChargeTypeList.Codes.HERALevy, EntryChargeTypeList.Descriptions.HERALevy, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[15], EntryChargeTypeList.Codes.HERALevyCredit, EntryChargeTypeList.Descriptions.HERALevyCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[16], EntryChargeTypeList.Codes.PFMLFuelLevy, EntryChargeTypeList.Descriptions.PFMLFuelLevy, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[17], EntryChargeTypeList.Codes.PFMLFuelLevyCredit, EntryChargeTypeList.Descriptions.PFMLFuelLevyCredit, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[18], EntryChargeTypeList.Codes.SGGLevy, EntryChargeTypeList.Descriptions.SGGLevy, true, ZString.Empty);
		}

		public void TestIsLevy()
		{
			AssertEquals(true, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.ACCFuelLevy));
			AssertEquals(true, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.ALACLevy));
			AssertEquals(true, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.PFMLFuelLevy));
			AssertEquals(true, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.SGGLevy));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.ALACLevyCredit));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.AntiDumpingDuty));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.CountervailingDuty));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.DepositRefund));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.Duty));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.DutyCredit));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.EntryFee));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.EntryFeeGST));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.ExciseDutyCredit));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.GST));
			AssertEquals(false, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.GSTCredit));
			AssertEquals(true, EntryChargeTypeList.IsLevy(EntryChargeTypeList.Codes.HERALevy));
		}

		public void TestRemoveAllAction()
		{
			var allCodes = new EntryChargeTypeList();
			AssertEquals("Code exists", EntryChargeTypeList.Descriptions.ALACLevy, allCodes.GetDescriptionFromCode(EntryChargeTypeList.Codes.ALACLevy));

			allCodes.RemoveWhere(c => c.Code == EntryChargeTypeList.Codes.ALACLevy);
			AssertEquals("Code was removed", null, allCodes.GetDescriptionFromCode(EntryChargeTypeList.Codes.ALACLevy));
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new EntryChargeTypeList();
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.NewZealand;
	}

	public class EntryChargeTypeListFilterTest : TestCaseWithFactory
	{
		public void TestYouCantUseAGSTRollupChargeType()
		{
			var chargeTypeCollection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var testChargeType = new NZEntryChargeTypeSettingForTest(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory, chargeTypeCollection);
			testChargeType.ChargeType = EntryChargeTypeList.Codes.DutyCredit;
			AssertNoNotifications(testChargeType.ChargeTypeInfo);
			testChargeType.ChargeType = EntryChargeTypeList.Codes.EntryFeeGST;
			AssertHasWarningContaining(testChargeType.ChargeTypeInfo, string.Format(EntryChargeTypeSetting.ErrorUseANonGSTChargeType, EntryChargeTypeList.Codes.EntryFee));
			testChargeType.ChargeType = EntryChargeTypeList.Codes.GST;
			AssertNoNotifications(testChargeType.ChargeTypeInfo);
		}

		[XmlSerializerAssembly("Enterprise.Registry.Business.Test.XmlSerializers")]
		class NZEntryChargeTypeSettingForTest(FallbackLevel fallbackLevel, BusinessObjectFactory factory, EntryChargeTypeSettingCollection parentCollection) : EntryChargeTypeSetting(fallbackLevel, factory, parentCollection)
		{
			protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetChargeType_ListCore() => new EntryChargeTypeList();
		}
	}
}
