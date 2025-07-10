using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Registry.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	public class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.Duty, EntryChargeTypeList.Descriptions.Duty, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.Excise, EntryChargeTypeList.Descriptions.Excise, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.GST, EntryChargeTypeList.Descriptions.GST, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.OtherTax, EntryChargeTypeList.Descriptions.OtherTax, true, ZString.Empty);
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList() => new EntryChargeTypeList();

		protected override ZString CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
