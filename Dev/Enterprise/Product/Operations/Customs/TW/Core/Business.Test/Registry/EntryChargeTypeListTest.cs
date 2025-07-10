using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	sealed class EntryChargeTypeListTest : Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.A10, EntryChargeTypeList.Descriptions.A10, true, "");
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.A19, EntryChargeTypeList.Descriptions.A19, true, "");
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.A20, EntryChargeTypeList.Descriptions.A20, true, "");
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.A30, EntryChargeTypeList.Descriptions.A30, true, "");
			AssertListElementIsCorrect(ChargeTypeList[4], EntryChargeTypeList.Codes.A40, EntryChargeTypeList.Descriptions.A40, true, "");
			AssertListElementIsCorrect(ChargeTypeList[5], EntryChargeTypeList.Codes.A50, EntryChargeTypeList.Descriptions.A50, true, "");
			AssertListElementIsCorrect(ChargeTypeList[6], EntryChargeTypeList.Codes.B10, EntryChargeTypeList.Descriptions.B10, true, "");
			AssertListElementIsCorrect(ChargeTypeList[7], EntryChargeTypeList.Codes.B19, EntryChargeTypeList.Descriptions.B19, true, "");
			AssertListElementIsCorrect(ChargeTypeList[8], EntryChargeTypeList.Codes.B29, EntryChargeTypeList.Descriptions.B29, true, "");
			AssertListElementIsCorrect(ChargeTypeList[9], EntryChargeTypeList.Codes.B31, EntryChargeTypeList.Descriptions.B31, true, "");
			AssertListElementIsCorrect(ChargeTypeList[10], EntryChargeTypeList.Codes.B32, EntryChargeTypeList.Descriptions.B32, true, "");
			AssertListElementIsCorrect(ChargeTypeList[11], EntryChargeTypeList.Codes.B40, EntryChargeTypeList.Descriptions.B40, true, "");
			AssertListElementIsCorrect(ChargeTypeList[12], EntryChargeTypeList.Codes.B49, EntryChargeTypeList.Descriptions.B49, true, "");
			AssertListElementIsCorrect(ChargeTypeList[13], EntryChargeTypeList.Codes.B51, EntryChargeTypeList.Descriptions.B51, true, "");
			AssertListElementIsCorrect(ChargeTypeList[14], EntryChargeTypeList.Codes.B52, EntryChargeTypeList.Descriptions.B52, true, "");
			AssertListElementIsCorrect(ChargeTypeList[15], EntryChargeTypeList.Codes.B59, EntryChargeTypeList.Descriptions.B59, true, "");
			AssertListElementIsCorrect(ChargeTypeList[16], EntryChargeTypeList.Codes.B60, EntryChargeTypeList.Descriptions.B60, true, "");
			AssertListElementIsCorrect(ChargeTypeList[17], EntryChargeTypeList.Codes.B69, EntryChargeTypeList.Descriptions.B69, true, "");
			AssertListElementIsCorrect(ChargeTypeList[18], EntryChargeTypeList.Codes.B79, EntryChargeTypeList.Descriptions.B79, true, "");
			AssertListElementIsCorrect(ChargeTypeList[19], EntryChargeTypeList.Codes.B89, EntryChargeTypeList.Descriptions.B89, true, "");
			AssertListElementIsCorrect(ChargeTypeList[20], EntryChargeTypeList.Codes.C10, EntryChargeTypeList.Descriptions.C10, true, "");
			AssertListElementIsCorrect(ChargeTypeList[21], EntryChargeTypeList.Codes.C20, EntryChargeTypeList.Descriptions.C20, true, "");
			AssertListElementIsCorrect(ChargeTypeList[22], EntryChargeTypeList.Codes.C21, EntryChargeTypeList.Descriptions.C21, true, "");
			AssertListElementIsCorrect(ChargeTypeList[23], EntryChargeTypeList.Codes.C22, EntryChargeTypeList.Descriptions.C22, true, "");
			AssertListElementIsCorrect(ChargeTypeList[24], EntryChargeTypeList.Codes.C23, EntryChargeTypeList.Descriptions.C23, true, "");
			AssertListElementIsCorrect(ChargeTypeList[25], EntryChargeTypeList.Codes.C24, EntryChargeTypeList.Descriptions.C24, true, "");
			AssertListElementIsCorrect(ChargeTypeList[26], EntryChargeTypeList.Codes.C25, EntryChargeTypeList.Descriptions.C25, true, "");
			AssertListElementIsCorrect(ChargeTypeList[27], EntryChargeTypeList.Codes.C31, EntryChargeTypeList.Descriptions.C31, true, "");
			AssertListElementIsCorrect(ChargeTypeList[28], EntryChargeTypeList.Codes.C32, EntryChargeTypeList.Descriptions.C32, true, "");
			AssertListElementIsCorrect(ChargeTypeList[29], EntryChargeTypeList.Codes.C33, EntryChargeTypeList.Descriptions.C33, true, "");
			AssertListElementIsCorrect(ChargeTypeList[30], EntryChargeTypeList.Codes.C34, EntryChargeTypeList.Descriptions.C34, true, "");
			AssertListElementIsCorrect(ChargeTypeList[31], EntryChargeTypeList.Codes.D10, EntryChargeTypeList.Descriptions.D10, true, "");
			AssertListElementIsCorrect(ChargeTypeList[32], EntryChargeTypeList.Codes.F10, EntryChargeTypeList.Descriptions.F10, true, "");
			AssertListElementIsCorrect(ChargeTypeList[33], EntryChargeTypeList.Codes.F11, EntryChargeTypeList.Descriptions.F11, true, "");
			AssertListElementIsCorrect(ChargeTypeList[34], EntryChargeTypeList.Codes.F12, EntryChargeTypeList.Descriptions.F12, true, "");
			AssertListElementIsCorrect(ChargeTypeList[35], EntryChargeTypeList.Codes.F13, EntryChargeTypeList.Descriptions.F13, true, "");
			AssertListElementIsCorrect(ChargeTypeList[36], EntryChargeTypeList.Codes.F14, EntryChargeTypeList.Descriptions.F14, true, "");
			AssertListElementIsCorrect(ChargeTypeList[37], EntryChargeTypeList.Codes.F15, EntryChargeTypeList.Descriptions.F15, true, "");
			AssertListElementIsCorrect(ChargeTypeList[38], EntryChargeTypeList.Codes.F16, EntryChargeTypeList.Descriptions.F16, true, "");
			AssertListElementIsCorrect(ChargeTypeList[39], EntryChargeTypeList.Codes.F20, EntryChargeTypeList.Descriptions.F20, true, "");
			AssertListElementIsCorrect(ChargeTypeList[40], EntryChargeTypeList.Codes.F21, EntryChargeTypeList.Descriptions.F21, true, "");
			AssertListElementIsCorrect(ChargeTypeList[41], EntryChargeTypeList.Codes.F22, EntryChargeTypeList.Descriptions.F22, true, "");
			AssertListElementIsCorrect(ChargeTypeList[42], EntryChargeTypeList.Codes.F23, EntryChargeTypeList.Descriptions.F23, true, "");
			AssertListElementIsCorrect(ChargeTypeList[43], EntryChargeTypeList.Codes.F30, EntryChargeTypeList.Descriptions.F30, true, "");
			AssertListElementIsCorrect(ChargeTypeList[44], EntryChargeTypeList.Codes.F31, EntryChargeTypeList.Descriptions.F31, true, "");
			AssertListElementIsCorrect(ChargeTypeList[45], EntryChargeTypeList.Codes.F40, EntryChargeTypeList.Descriptions.F40, true, "");
			AssertListElementIsCorrect(ChargeTypeList[46], EntryChargeTypeList.Codes.F50, EntryChargeTypeList.Descriptions.F50, true, "");
			AssertListElementIsCorrect(ChargeTypeList[47], EntryChargeTypeList.Codes.F51, EntryChargeTypeList.Descriptions.F51, true, "");
			AssertListElementIsCorrect(ChargeTypeList[48], EntryChargeTypeList.Codes.F52, EntryChargeTypeList.Descriptions.F52, true, "");
			AssertListElementIsCorrect(ChargeTypeList[49], EntryChargeTypeList.Codes.F55, EntryChargeTypeList.Descriptions.F55, true, "");
			AssertListElementIsCorrect(ChargeTypeList[50], EntryChargeTypeList.Codes.F56, EntryChargeTypeList.Descriptions.F56, true, "");
			AssertListElementIsCorrect(ChargeTypeList[51], EntryChargeTypeList.Codes.F57, EntryChargeTypeList.Descriptions.F57, true, "");
			AssertListElementIsCorrect(ChargeTypeList[52], EntryChargeTypeList.Codes.F58, EntryChargeTypeList.Descriptions.F58, true, "");
			AssertListElementIsCorrect(ChargeTypeList[53], EntryChargeTypeList.Codes.F88, EntryChargeTypeList.Descriptions.F88, true, "");
			AssertListElementIsCorrect(ChargeTypeList[54], EntryChargeTypeList.Codes.F99, EntryChargeTypeList.Descriptions.F99, true, "");
			AssertListElementIsCorrect(ChargeTypeList[55], EntryChargeTypeList.Codes.X00, EntryChargeTypeList.Descriptions.X00, true, "");
		}

		protected override Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList() => new EntryChargeTypeList();

		protected override ZString CountryCode => Core.Constants.CountryCodes.Taiwan;
	}
}
