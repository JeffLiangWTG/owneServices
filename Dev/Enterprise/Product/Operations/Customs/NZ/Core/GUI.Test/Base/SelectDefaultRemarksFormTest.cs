using System;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Base.Testing
{
	[TestedType(typeof(SelectDefaultRemarksForm))]
	public class SelectDefaultRemarksFormTest : ZFormBasherTest
	{
		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "RemarksListBox")
			{
				return true;
			}

			return base.ShouldIgnoreMissingBindingMember(control);
		}

		public void TestCancelBtn_Click()
		{
			NZCustomsDataRegistry.Instance.DefaultResendingRemarks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new string[] { "Hello World", "Goodbye World" });
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			manager.EnteredRemarks = "What";
			using (SelectDefaultRemarksForm form = new SelectDefaultRemarksForm(manager))
			{
				form.Show();
				form.CancelBtn.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals("What", manager.EnteredRemarks);
			}
		}

		public void TestOkButton_Click()
		{
			NZCustomsDataRegistry.Instance.DefaultResendingRemarks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new string[] { "Hello World", "Goodbye World" });
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			manager.EnteredRemarks = "What";
			using (SelectDefaultRemarksForm form = new SelectDefaultRemarksForm(manager))
			{
				form.Show();
				form.RemarksListBox.SetSelected(0, true);
				form.OkButton.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals("Hello World", manager.EnteredRemarks);
			}
		}

		public void TestFullCaption()
		{
			Env.Registry.ShowBranchName = false;
			Env.Registry.ShowCompanyName = false;
			Env.Registry.ShowDepartmentName = false;
			Env.Registry.ShowUserName = false;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			using (SelectDefaultRemarksForm form = new SelectDefaultRemarksForm(manager))
			{
				form.Show();
				AssertEquals("Select Default Remarks", form.TextIncludingSuffix);
			}
		}

		protected override Form GetFormToBashCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			MessageManagerForClearance manager = new Business.MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageManager.OperationType.CancelMessage);
			return new SelectDefaultRemarksForm(manager);
		}
	}
}
