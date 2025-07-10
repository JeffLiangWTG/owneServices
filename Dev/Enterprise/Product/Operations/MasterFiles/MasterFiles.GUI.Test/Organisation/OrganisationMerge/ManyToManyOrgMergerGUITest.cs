using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ManyToManyOrgMergerGUITest : TransactionedTestCase
	{
		public void TestMergeSelectedOrgs()
		{
			bool previousOrganisationMerging = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;
			try
			{
				ManyToManyOrgMergerGUIForTest merger = new ManyToManyOrgMergerGUIForTest(new ZQuery(OrgHeaderSchema.OH_FullName, "~muhaha dont find me~"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				merger.Merge();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Organization transferred successfully.\r\nNumber of Organizations processed is 0.\r\nTime taken:"));

				merger.ReturnErrorForMergeSelectedOrgsCore = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				merger.Merge();
				AssertEquals("hello", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("OH_Code", merger.ExportQuery_Exposed.OrderBy);
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousOrganisationMerging;
			}
		}

		public void TestCancel()
		{
			bool previousOrganisationMerging = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;
			try
			{
				ManyToManyOrgMergerGUIForTest merger = new ManyToManyOrgMergerGUIForTest(new ZQuery(OrgHeaderSchema.OH_FullName, "~muhaha dont find me~"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				merger.Cancel = true;
				merger.Merge();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Operation canceled by the user.\r\nNumber of Organizations processed is 0.\r\nTime taken:"));
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousOrganisationMerging;
			}
		}

		public void TestAllowOpenedFormWhenMerging()
		{
			bool previousOrganisationMerging = Env.Security.OrgDuplicateDetectionMerge.IsAllowed;
			var cacheForm = new ZForm();
			OpenedFormCache.GetInstance().Add(Guid.NewGuid(), cacheForm, "test");
			try
			{
				ManyToManyOrgMergerGUIForTest merger = new ManyToManyOrgMergerGUIForTest(new ZQuery(OrgHeaderSchema.OH_FullName, "~muhaha dont find me~"));
				merger.Merge();

				AssertNotEquals("Please close open forms before you use this feature.", UnitTestUserNotification.Instance.LastMessage.Text);
				cacheForm.Close();
			}
			finally
			{
				Env.Security.OrgDuplicateDetectionMerge.IsAllowed = previousOrganisationMerging;
			}
		}
	}
}
