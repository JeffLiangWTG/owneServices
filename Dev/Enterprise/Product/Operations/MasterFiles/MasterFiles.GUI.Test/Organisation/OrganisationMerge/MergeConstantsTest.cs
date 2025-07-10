using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class MergeConstantsTest : TestCase
	{
		public void TestMergeWarningTitle()
		{
			AssertEquals("CRITICAL WARNING", MergeConstants.MergeWarningTitle);
		}

		public void TestMergeConfirmationMessage()
		{
			AssertEquals("THIS PROCESS IS IRREVERSIBLE", MergeConstants.MergeConfirmationMessage);
		}

		public void TestGetMergeWarningSingle()
		{
			var expected = @"You are about to merge organization aaa into organization bbb. This will switch all jobs in the system to bbb that previously belonged to aaa, and will delete aaa. If merging causes Rate Entries to overlap, then only Rates belonging to bbb will be kept, the others will be deleted. This action is irreversible and cannot be undone.

If you select this option CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?";

			AssertEquals(expected, MergeConstants.GetMergeWarningSingle("aaa", "bbb"));
		}

		public void TestGetMergeWarningManyToOne()
		{
			var expected = @"You are about to merge organization(s) xxx into organization yyy. This will switch all jobs in the system to yyy that previously belonged to any of these organizations, and will delete these organizations. If merging causes Rate Entries to overlap, then only Rates belonging to yyy will be kept, the others will be deleted. This action is irreversible and cannot be undone.

If you select this option CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?";

			AssertEquals(expected, MergeConstants.GetMergeWarningManyToOne("xxx", "yyy"));
		}

		public void TestGetMergeWarningManyToMany()
		{
			AssertEquals("Every Organization you have selected will be merged with any other similar Organizations found. This operation is time consuming.This action is irreversible and cannot be undone even if you cancel merging process in the middle. If you select this option it is irreversible and CargoWise cannot provide a reversing process or manual reversal. Are you sure you want to proceed?", MergeConstants.MergeWarningManyToMany);
		}
	}
}
