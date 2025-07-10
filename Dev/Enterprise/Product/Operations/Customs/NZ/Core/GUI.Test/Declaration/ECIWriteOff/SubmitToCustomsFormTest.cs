using System;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(SubmitToCustomsForm))]
	public class SubmitToCustomsFormTest : ZFormBasherTest
	{
		public void TestQueueForManifestingOnlyShowsWhereAppropriate()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			using (var form = new SubmitToCustomsForm(manager))
			{
				form.Show();
				Assert("Form.QueueForManifestingCheckBox.Visible", form.FindSingle<ZCheckBox>("QueueForManifestingCheckBox").Visible);
			}

			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			using (var form = new SubmitToCustomsForm(manager))
			{
				form.Show();
				Assert("Form.QueueForManifestingCheckBox.Visible", !form.FindSingle<ZCheckBox>("QueueForManifestingCheckBox").Visible);
			}
		}

		public void TestOkButton()
		{
			TestHelper.SetupMessagingEnvironment();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			using (var testForm = new SubmitToCustomsForm(manager))
			{
				testForm.OkButton_Click(null, new EventArgs());
				AssertEquals(testForm.DialogResult, System.Windows.Forms.DialogResult.OK);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			var manager = new MessageManager(declaration, MessageManager.OperationType.SubmitMessage);
			var testedForm = new SubmitToCustomsForm(manager);
			MissingResourceStringChecker.ExcludeFromTest(testedForm.RemarksTextBox);
			return testedForm;
		}
	}
}
