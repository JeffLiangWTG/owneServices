using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class SubmissionTypeListTest : TestCase
	{
		public void TestIsISF10Entry()
		{
			var nonISF5SubmissionTypes = new SubmissionTypeList();
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.ISF10);
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.ISF5ToISF10);
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.LateISF10);
			AssertEquals(true, SubmissionTypeList.IsISF10Entry(SubmissionTypeList.Codes.ISF10));
			AssertEquals(true, SubmissionTypeList.IsISF10Entry(SubmissionTypeList.Codes.ISF5ToISF10));
			AssertEquals(true, SubmissionTypeList.IsISF10Entry(SubmissionTypeList.Codes.LateISF10));
			foreach (ICodeDescription pair in nonISF5SubmissionTypes)
			{
				AssertEquals(false, SubmissionTypeList.IsISF10Entry(pair.Code));
			}
		}

		public void TestISF5Entry()
		{
			var nonISF5SubmissionTypes = new SubmissionTypeList();
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.ISF5);
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.ISF10ToISF5);
			nonISF5SubmissionTypes.RemoveCode(SubmissionTypeList.Codes.LateISF5);
			AssertEquals(true, SubmissionTypeList.IsISF5Entry(SubmissionTypeList.Codes.ISF5));
			AssertEquals(true, SubmissionTypeList.IsISF5Entry(SubmissionTypeList.Codes.ISF10ToISF5));
			AssertEquals(true, SubmissionTypeList.IsISF5Entry(SubmissionTypeList.Codes.LateISF5));
			foreach (ICodeDescription pair in nonISF5SubmissionTypes)
			{
				AssertEquals(false, SubmissionTypeList.IsISF5Entry(pair.Code));
			}
		}

		public void TestIsLateEntry()
		{
			var nonLateSubmissionTypes = new SubmissionTypeList();
			nonLateSubmissionTypes.RemoveCode(SubmissionTypeList.Codes.LateISF10);
			nonLateSubmissionTypes.RemoveCode(SubmissionTypeList.Codes.LateISF5);
			AssertEquals(true, SubmissionTypeList.IsLateEntry(SubmissionTypeList.Codes.LateISF10));
			AssertEquals(true, SubmissionTypeList.IsLateEntry(SubmissionTypeList.Codes.LateISF5));
			foreach (ICodeDescription pair in nonLateSubmissionTypes)
			{
				AssertEquals(false, SubmissionTypeList.IsLateEntry(pair.Code));
			}
		}

		public void TestICodeDescriptionPairListProviderMembers()
		{
			var list = new SubmissionTypeList();
			AssertEquals(list, list.GetCodeDescriptionPairList());
		}
	}
}
