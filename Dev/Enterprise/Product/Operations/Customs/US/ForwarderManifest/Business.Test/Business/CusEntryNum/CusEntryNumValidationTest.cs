using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(CusEntryNumValidation))]
	public class CusEntryNumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryNum()
		{
			var messageError = "The number must start with the letter \"X\", followed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;

			entryNumber1.CE_EntryNum = "a1234";
			AssertHasMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			entryNumber1.CE_EntryNum = "X20120112901245";
			AssertNoMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			entryNumber1.CE_EntryNum = "X20121312901245";
			AssertHasMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			entryNumber1.CE_EntryNum = "0123456789012345";
			AssertHasMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			messageError = "In-Bond Number should be 9 digits.";
			entryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.InBond;
			entryNumber1.CE_EntryNum = "0123456789";
			AssertHasMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			entryNumber1.CE_EntryNum = "012345678";
			AssertNoMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);

			entryNumber1.CE_EntryNum = "a12345678";
			AssertHasMessageErrorContaining(entryNumber1.CE_EntryNumInfo, messageError);
		}
	}
}
