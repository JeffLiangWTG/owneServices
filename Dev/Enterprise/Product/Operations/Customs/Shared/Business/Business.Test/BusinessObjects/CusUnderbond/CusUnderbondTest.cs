using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondTest : TestCaseWithFactory
	{
		public void TestHeading()
		{
			Underbond.C4_SendersMessageReference = "123456";
			AssertEquals("Heading", "Underbond 123456", ((IDetailsTabPageHeadingProvider)Underbond).Heading);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Underbond", Underbond.HumanReadableName);
			Underbond.C4_SendersMessageReference = "3344";
			AssertEquals("Underbond 3344", Underbond.HumanReadableName);
		}

		public void TestCusEntryNumbers()
		{
			var underbond = Factory.New<CusUnderbondThatOverridesGetCanDoOutturn>();
			underbond.CanDoOutturnResult = false;
			AssertEquals("No CusEntryNumbers", 0, underbond.CusEntryNumbers.Length);
			Factory.Save();
			AssertEquals("1 created on saving", 1, underbond.CusEntryNumbers.Length);
			CusEntryNumber cusEntryNumber1 = underbond.CusEntryNumbers[0];
			Assert("CusEntryNumber not deleted", !cusEntryNumber1.IsDeleted);
			underbond.Delete();
			Assert("CusEntryNumber was deleted", cusEntryNumber1.IsDeleted);

			underbond = Factory.New<CusUnderbondThatOverridesGetCanDoOutturn>();
			underbond.CanDoOutturnResult = true;
			AssertEquals("No CusEntryNumbers", 0, underbond.CusEntryNumbers.Length);
			Factory.Save();
			AssertEquals("2 created on saving", 2, underbond.CusEntryNumbers.Length);
			cusEntryNumber1 = underbond.CusEntryNumbers[0];
			var cusEntryNumber2 = underbond.CusEntryNumbers[1];
			Assert("CusEntryNumber1 not deleted", !cusEntryNumber1.IsDeleted);
			Assert("CusEntryNumber2 not deleted", !cusEntryNumber2.IsDeleted);
			AssertEquals("Underbond status", CusEntryNumber.EntryType.UnderbondStatus, cusEntryNumber1.CE_EntryType);
			AssertEquals("Outturn status", CusEntryNumber.EntryType.OutturnStatus, cusEntryNumber2.CE_EntryType);
			underbond.Delete();
			Assert("CusEntryNumber1 was deleted", cusEntryNumber1.IsDeleted);
			Assert("CusEntryNumber2 was deleted", cusEntryNumber2.IsDeleted);

			underbond = Factory.New<CusUnderbondThatOverridesGetCanDoOutturn>();
			underbond.CanDoOutturnResult = true;
			Factory.Save();
			AssertEquals("2 created on saving", 2, underbond.CusEntryNumbers.Length);
			cusEntryNumber1 = underbond.CusEntryNumbers[0];
			cusEntryNumber2 = underbond.CusEntryNumbers[1];
			AssertEquals("Underbond status", CusEntryNumber.EntryType.UnderbondStatus, cusEntryNumber1.CE_EntryType);
			AssertEquals("Outturn status", CusEntryNumber.EntryType.OutturnStatus, cusEntryNumber2.CE_EntryType);
			underbond.CanDoOutturnResult = false;
			underbond.C4_FlightNo = "QF1";
			Factory.Save();
			AssertEquals("only 1 now", 1, underbond.CusEntryNumbers.Length);
			cusEntryNumber1 = underbond.CusEntryNumbers[0];
			AssertEquals("Underbond status", CusEntryNumber.EntryType.UnderbondStatus, cusEntryNumber1.CE_EntryType);
		}

		#region Implementation

		CusUnderbond underbond;
		CusUnderbond Underbond
		{
			get { return underbond ?? (underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>()); }
		}

		#endregion
	}
}
