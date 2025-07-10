using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WarningAcknowledgementTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		[RequiresSTA]
		public void TestLogsCreation()
		{
			var warning = Factory.NewWithValidTestData<GenCustomAddOnRuleAck>();
			warning.XK_ParentID = Guid.NewGuid();
			warning.XK_ParentTableCode = "GS";
			warning.XK_SystemCreateTimeUtc = DateTime.UtcNow;
			warning.XK_SystemCreateUser = "E";
			warning.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;

			Factory.Save();

			using (var form = new WarningAcknowledgementForm(warning))
			{
				form.Show();
				AssertEquals("Warning has been cancelled and saved to DB", false, warning.XK_IsCancelled);

				form.isCancelledCheckbox.Checked = true;
				form.FireSaveButton();
				Factory.Save();

				AssertEquals("Warning has been cancelled and saved to DB", true, warning.XK_IsCancelled);

				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_Table, "GenCustomAddOnRuleAck");
				var logs = Factory.Load<StmALog>(query);
				AssertEquals("Only one log should exist if there are more lost likely tests leave rubbish behind", 1, logs.Length);
			}
		}

		[UseSnapshotProtection]
		public void TestOpenRelatedParent()
		{
			// this tests will be written once i change phonevalidation because otherwise i will need to re-write them
			Assert(true);
		}

		[UseSnapshotProtection]
		public void TestOpenComplexRelatedParent()
		{
			// this tests will be written once i change phonevalidation because otherwise i will need to re-write them
			Assert(true);
		}

		//[UseSnapshotProtection]
		public void TestEntireFormReadOnlyExceptIsCancelled()
		{
			// this tests will be written once i change phonevalidation because otherwise i will need to re-write them
			Assert(true);
		}
	}
}
