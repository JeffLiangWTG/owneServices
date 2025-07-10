namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENSMessageBlockTest : NUnit.Framework.TestCase
	{
		public void TestENS88AlwaysZeroFillsIfNotUsed()
		{
			AENS88 aens88 = new AENS88();
			AssertEquals("Each field has an instruction, fills with zero if not used. If not followed, it will be rejected", "8800000000000 00000000000 00000000000 00000000000                               ", aens88.Serialise());
			aens88.TotalBondedADDutyAmount = 1000m;
			AssertEquals("8800000100000 00000000000 00000000000 00000000000                               ", aens88.Serialise());
		}

		public void TestENS89AlwaysZeroFillsIfNotUsed()
		{
			AENS89 aens89 = new AENS89();
			aens89.AccountingClassCode1 = "111";
			aens89.TotalFeeAmount1 = 0m;
			aens89.AccountingClassCode2 = "222";
			aens89.TotalFeeAmount2 = 0m;
			AssertEquals("zeros should fill only when assignment of value has happened", "891110000000000022200000000000                                                  ", aens89.Serialise());
		}

		public void TestENS90AlwaysZeroFillsIfNotUsed()
		{
			AENS90 aens90 = new AENS90();
			AssertEquals("Each field has an instruction, fills with zero if not used. If not followed, it will be rejected", "9000000000000 00000000000 00000000000 00000000000 00000000000 00000000000       ", aens90.Serialise());
		}

		public void TestAENSE1IsError()
		{
			var aens1 = new AENSE1();
			aens1.DispositionTypeCode = "R";
			aens1.SeverityCode = "F";
			AssertEquals(false, (aens1 as I7501Errors).IsError);
			aens1.DispositionTypeCode = string.Empty;
			aens1.SeverityCode = "F";
			AssertEquals(true, (aens1 as I7501Errors).IsError);
			aens1.SeverityCode = "W";
			AssertEquals(true, (aens1 as I7501Errors).IsError);
			aens1.SeverityCode = "P";
			AssertEquals(true, (aens1 as I7501Errors).IsError);
			aens1.SeverityCode = "I";
			AssertEquals(true, (aens1 as I7501Errors).IsError);
			aens1.SeverityCode = " ";
			AssertEquals(false, (aens1 as I7501Errors).IsError);
		}
	}
}
