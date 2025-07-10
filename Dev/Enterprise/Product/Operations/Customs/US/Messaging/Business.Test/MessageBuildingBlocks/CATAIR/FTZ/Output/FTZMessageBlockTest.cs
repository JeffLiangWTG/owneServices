using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Testing
{
	sealed class FTZMessageBlockTest : NUnit.Framework.TestCase
	{
		public void TestFTZNF20DataIsNotTrimmed()
		{
			string message = "20 10APLUAPL BAHRAIN            1213           2014052120140521250120140521     ";
			var ftzNF20 = new FTZNF20();
			ftzNF20.Deserialise(message);
			AssertNoExceptionThrown(delegate
			{
				((ISerialiserSupporter)ftzNF20).Serialise(true, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, null);
			});
		}

		public void TestFTZNF10WithFTZFZ10_01()
		{
			string message = "10A0610010A122SAS05184Y4909NB3M         66-053115000LBY066-094189600            ";
			var ftzNF10 = new FTZNF10_01();
			AssertNoExceptionThrown(() => ftzNF10.Deserialise(message));
			AssertNoExceptionThrown(() => ((ISerialiserSupporter)ftzNF10).Serialise(true, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, null));
		}

		public void TestMASKSSNOnFTZFZ10_01()
		{
			Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			var message = "10A0610010A122SAS05184Y4909NB3M         555-99-7777 LBY066-094189600            ";
			var ftzFT10 = new FTZFT10_01();
			ftzFT10.Deserialise(message);
			AssertEquals("***-**-****", ftzFT10.ZoneOperatorIdentifierformerlyIRSIdentifier);
			AssertEquals("66-094189600", ftzFT10.ApplicantForAdmission);
		}
	}
}
