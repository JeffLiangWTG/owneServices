using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlBaseOnlyTest : OrgSupplierPartFormCustomsControlAbstractTest
	{
		public void TestShowBatchAuditDialog()
		{
			var staffDeniedAuditImport = Factory.NewWithValidTestData<GlbStaff>();
			ApplySecurityPermission(staffDeniedAuditImport, Env.Security.CustomsSupplierPartAuditImport, false);
			ApplySecurityPermission(staffDeniedAuditImport, Env.Security.CustomsSupplierPartAuditExport, true);

			var staffDeniedAuditExport = Factory.NewWithValidTestData<GlbStaff>();
			ApplySecurityPermission(staffDeniedAuditExport, Env.Security.CustomsSupplierPartAuditImport, true);
			ApplySecurityPermission(staffDeniedAuditExport, Env.Security.CustomsSupplierPartAuditExport, false);

			var staffDeniedAuditBoth = Factory.NewWithValidTestData<GlbStaff>();
			ApplySecurityPermission(staffDeniedAuditBoth, Env.Security.CustomsSupplierPartAudit, false);

			var staffAllowedAuditBoth = Factory.NewWithValidTestData<GlbStaff>();
			ApplySecurityPermission(staffAllowedAuditBoth, Env.Security.CustomsSupplierPartAudit, true);

			var part = Factory.NewWithValidTestData<Business.OrgSupplierPart>();
			var cusClassPivot = part.PivotsForBinding.AddNew();
			Factory.Save();

			using (var form = new ZForm(part))
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				AssertEquals("OrgSupplierPartFormCustomsControl", control.Name);

				form.Controls.Add(control);
				form.Show();

				var pivotsList = new BaseCusClassPartPivot[] { cusClassPivot };
				var classificationType = cusClassPivot.GetClassificationTypeProvider();
				var currentBranchPk = GlbBranch.CurrentBranch.PK.ToGuid();
				var currentDepartmentPk = GlbDepartment.CurrentDepartment.PK.ToGuid();

				using (Env.SetTemporaryUserContext(new UserContext(staffDeniedAuditImport.GS_LoginName, currentBranchPk, currentDepartmentPk)))
				{
					cusClassPivot.CI_ChildType = classificationType.HTICode;
					AssertDenied("HTI and DeniedAuditImport");

					cusClassPivot.CI_ChildType = classificationType.HTECode;
					AssertAllowed("HTE and DeniedAuditImport");

					cusClassPivot.CI_ChildType = classificationType.HTBCode;
					AssertDenied("HTB and DeniedAuditImport");

					cusClassPivot.CI_ChildType = "SHB";
					AssertIgnored();
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffDeniedAuditExport.GS_LoginName, currentBranchPk, currentDepartmentPk)))
				{
					cusClassPivot.CI_ChildType = classificationType.HTICode;
					AssertAllowed("HTI and DeniedAuditExport");

					cusClassPivot.CI_ChildType = classificationType.HTECode;
					AssertDenied("HTE and DeniedAuditExport");

					cusClassPivot.CI_ChildType = classificationType.HTBCode;
					AssertDenied("HTB and DeniedAuditExport");

					cusClassPivot.CI_ChildType = "SHB";
					AssertIgnored();
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffDeniedAuditBoth.GS_LoginName, currentBranchPk, currentDepartmentPk)))
				{
					cusClassPivot.CI_ChildType = classificationType.HTICode;
					AssertDenied("HTI and DeniedAuditBoth");

					cusClassPivot.CI_ChildType = classificationType.HTECode;
					AssertDenied("HTE and DeniedAuditBoth");

					cusClassPivot.CI_ChildType = classificationType.HTBCode;
					AssertDenied("HTB and DeniedAuditBoth");

					cusClassPivot.CI_ChildType = "SHB";
					AssertIgnored();
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffAllowedAuditBoth.GS_LoginName, currentBranchPk, currentDepartmentPk)))
				{
					cusClassPivot.CI_ChildType = classificationType.HTICode;
					AssertAllowed("HTI and AllowedAuditBoth");

					cusClassPivot.CI_ChildType = classificationType.HTECode;
					AssertAllowed("HTE and AllowedAuditBoth");

					cusClassPivot.CI_ChildType = classificationType.HTBCode;
					AssertAllowed("HTB and AllowedAuditBoth");

					cusClassPivot.CI_ChildType = "SHB";
					AssertIgnored();
				}

				void AssertDenied(ZString message)
				{
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					control.ShowBatchAuditDialog(pivotsList);
					AssertContains("Permission Denied when " + message, "You do not have the appropriate security rights to run this function", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				void AssertAllowed(ZString message)
				{
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.LastFormShownDialogForTest = null;
					control.ShowBatchAuditDialog(pivotsList);
					AssertEquals("No Permission Denied popup when " + message, ZString.Empty, (ZString)UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Audit Dialog Presented when " + message, "Audit Classification Lookup", ((ZForm)ZFormModaliser.LastFormShownDialogForTest).FormHeading);
					AssertEquals("Audit Dialog is generic WriteToLogForm", "WriteToLogForm", ((ZForm)ZFormModaliser.LastFormShownDialogForTest).Name);
				}

				void AssertIgnored()
				{
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.LastFormShownDialogForTest = null;
					control.ShowBatchAuditDialog(pivotsList);
					AssertEquals("No Permission Denied popup when not a controlled child type", ZString.Empty, (ZString)UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Audit Dialog is not presented when not a controlled child type", ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		protected override ZUserControl GetUserControl() => new OrgSupplierPartFormCustomsControl();

		protected override string UserControlName => "OrgSupplierPartFormCustomsControl";

		void ApplySecurityPermission(GlbStaff staff, SecurityCheckpoint checkpoint, bool granted)
		{
			var security = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			security.GU_SecurityRight = checkpoint.Code;
			security.GU_SecurityItemIsAllowed = granted;
			security.GU_GS = staff.PK;
		}
	}
}
