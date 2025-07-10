using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MessageSendingFormWithValidationDetails))]
	sealed class JobDeclarationMessageSendingFormTest : MessageSendingObjectFormTest
	{
		public void TestControls()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetails(parent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.Controls.Find("SendWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;

				Assert("Visibility for SendWithValidationErrorsCheckBox", !sendWithValidationErrorsCheckBox.Visible);
				Assert("Visibility for SendWithAdditionalWarningCheckBox", !sendWithAdditionalWarningCheckBox.Visible);
				Assert("No Message select", !sendButton.Enabled);

				parent.SendingObjectsCollection[0].ShouldSend = true;
				parent.SendingObjectsCollection[1].ShouldSend = true;

				Assert("There are message errors", sendWithValidationErrorsCheckBox.Visible);
				AssertEquals("Continue to send even though the selected message(s) contains validation errors?", sendWithValidationErrorsCheckBox.Text);
				Assert("There are no additional warnings", !sendWithAdditionalWarningCheckBox.Visible);
				Assert("SendWithValidationErrorsCheckBox unticked", !sendButton.Enabled);

				sendWithValidationErrorsCheckBox.Checked = true;
				Assert("SendWithValidationErrorsCheckBox ticked", sendButton.Enabled);
			}

			parent.AdditionalWarningsForTesting = "Additional Warnings";
			parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First().ShouldSend = false;
			using (var form = new MessageSendingFormWithValidationDetails(parent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.Controls.Find("SendWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;

				parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First().ShouldSend = true;

				Assert("There are message errors", sendWithValidationErrorsCheckBox.Visible);
				Assert("There are additional warnings", sendWithAdditionalWarningCheckBox.Visible);
				Assert("SendWithValidationErrorsCheckBox & SendWithAdditionalWarningCheckBox unticked", !sendButton.Enabled);

				sendWithValidationErrorsCheckBox.Checked = true;
				Assert("SendWithAdditionalWarningCheckBox unticked", !sendButton.Enabled);

				sendWithAdditionalWarningCheckBox.Checked = true;
				Assert("SendWithAdditionalWarningCheckBox ticked", sendButton.Enabled);
			}

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			parent.AdditionalWarningsForTesting = "";
			using (var form = new MessageSendingFormWithValidationDetails(parent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.Controls.Find("SendWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;

				Assert("There are message errors", sendWithValidationErrorsCheckBox.Visible);
				Assert("User does not have security right", !sendWithValidationErrorsCheckBox.Enabled);
				AssertEquals("You don't have security rights to send with message errors", sendWithValidationErrorsCheckBox.Text);
				Assert("SendWithAdditionalWarningCheckBox disabled", !sendButton.Enabled);
			}

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "CNT";
			parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First().ShouldSend = false;
			using (var form = new MessageSendingFormWithValidationDetails(parent))
			{
				form.Show();

				var sendWithValidationErrorsCheckBox = form.Controls.Find("SendWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				var sendWithAdditionalWarningCheckBox = form.Controls.Find("SendWithAdditionalWarningCheckBox", true)[0] as ZCheckBox;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				parent.SendingObjectsCollection.Cast<JobDeclarationMessageSendingObject>().First().ShouldSend = true;

				Assert("There are no message errors", !sendWithValidationErrorsCheckBox.Visible);
				Assert("User does not have security right", !sendWithValidationErrorsCheckBox.Enabled);
				AssertEquals("You don't have security rights to send with message errors", sendWithValidationErrorsCheckBox.Text);
				Assert("SendWithAdditionalWarningCheckBox enabled", sendButton.Enabled);
			}
		}

		public void TestDropEditColumnStyleIsAddedCorrectly()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetails(parent))
			{
				form.Show();

				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				AssertNotNull("MessageSendingObjectsGrid", grid);
				AssertEquals("Grid has a drop edit", typeof(ZDropEditColumnStyleInfo), grid.ColumnStyles[4].GetType());
			}
		}

		public void TestGridColumns_WhenMessageSendingGridColumnLayoutProviderIsSpecified()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetailsForTest(parent))
			{
				form.Show();

				var grid = form.Controls.Find("MessageSendingObjectsGrid", true)[0] as ZGrid;
				AssertNotNull("MessageSendingObjectsGrid", grid);
				AssertEquals("Grid ColumnStyles Count", 1, grid.ColumnStyles.Count);
				AssertEquals("Grid has a drop edit", typeof(ZDropEditColumnStyleInfo), grid.ColumnStyles[0].GetType());
			}
		}

		public void TestPreviewMessageCheckbox_WhenHidden()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetailsForTest(parent, false, false))
			{
				form.Show();
				var previewMessageCheckBox = form.Controls.Find("PreviewMessageCheckBox", true)[0] as ZCheckBox;

				parent.SendingObjectsCollection[0].ShouldSend = true;
				parent.SendingObjectsCollection[1].ShouldSend = false;

				CombineAssertions(() =>
				{
					Assert("PreviewMessageCheckBox is hidden", !previewMessageCheckBox.Visible);
					Assert("PreviewMessageCheckBox is always disabled", !previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is always unchecked", !previewMessageCheckBox.Checked);
				});
			}
		}

		public void TestPreviewMessageCheckbox_WhenSendingAMessagePerObject()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetailsForTest(parent, true, false))
			{
				form.Show();
				var previewMessageCheckBox = form.Controls.Find("PreviewMessageCheckBox", true)[0] as ZCheckBox;

				CombineAssertions(() => {
					parent.SendingObjectsCollection[0].ShouldSend = false;
					parent.SendingObjectsCollection[1].ShouldSend = false;
					Assert("PreviewMessageCheckBox is disabled when no object is selected", !previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is unchecked when no object is selected", !previewMessageCheckBox.Checked);

					parent.SendingObjectsCollection[0].ShouldSend = true;
					parent.SendingObjectsCollection[1].ShouldSend = false;
					Assert("PreviewMessageCheckBox is enabled when only one object is selected", previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is checked when only one object is selected", previewMessageCheckBox.Checked);

					parent.SendingObjectsCollection[0].ShouldSend = true;
					parent.SendingObjectsCollection[1].ShouldSend = true;
					Assert("PreviewMessageCheckBox is disabled when multiple objects are selected", !previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is unchecked when multiple objects are selected", !previewMessageCheckBox.Checked);
				});
			}
		}

		public void TestPreviewMessageCheckbox_WhenSendingASingleMessageForMultipleObjects()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var parent = new JobDeclarationMessageSendingObjectParentForTest(declaration);
			using (var form = new MessageSendingFormWithValidationDetailsForTest(parent, true, true))
			{
				form.Show();
				var previewMessageCheckBox = form.Controls.Find("PreviewMessageCheckBox", true)[0] as ZCheckBox;

				CombineAssertions(() =>
				{
					parent.SendingObjectsCollection[0].ShouldSend = true;
					parent.SendingObjectsCollection[1].ShouldSend = false;
					Assert("PreviewMessageCheckBox is enabled when one object is selected", previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is checked when one object is selected", previewMessageCheckBox.Checked);

					parent.SendingObjectsCollection[0].ShouldSend = false;
					parent.SendingObjectsCollection[1].ShouldSend = false;
					Assert("PreviewMessageCheckBox is disabled when no object is selected", !previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is unchecked when no object is selected", !previewMessageCheckBox.Checked);

					parent.SendingObjectsCollection[0].ShouldSend = true;
					parent.SendingObjectsCollection[1].ShouldSend = true;
					Assert("PreviewMessageCheckBox is enabled when multiple objects are selected", previewMessageCheckBox.Enabled);
					Assert("PreviewMessageCheckBox is checked when multiple objects are selected", previewMessageCheckBox.Checked);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new MessageSendingFormWithValidationDetails(new JobDeclarationMessageSendingObjectParentForTest(declaration));
		}

		sealed class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObjectForTest>
		{
			public JobDeclarationMessageSendingObjectParentForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			public ZString AdditionalWarningsForTesting { get; set; }

			protected override ZString GetAdditionalWarningsCore() => AdditionalWarningsForTesting;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900298. Form this is used in requires ResourceString.")]
			public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => base.MessageSendingObjectProperties.Append(
				new MessageSendingObjectProperty("CodeProperty", true, 200, Res.GetData("AAAFD518-4DA1-4F9D-BF6E-82E1D51383F4", "Code"))
			);

			protected override JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(CusEntryHeader header)
			{
				return new JobDeclarationMessageSendingObjectForTest(header);
			}

			protected override NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObjectForTest> GetSendingObjectsCollectionCore()
			{
				var result = new JobDeclarationMessageSendingObjectCollectionForTest(Factory);
				foreach (CusEntryHeader entry in ParentDeclaration.ActiveEntryHeaders)
				{
					result.Add(CreateNewJobDeclarationMessageSendingObject(entry));
				}
				return result;
			}
		}

		sealed class JobDeclarationMessageSendingObjectForTest : JobDeclarationMessageSendingObject
		{
			public JobDeclarationMessageSendingObjectForTest(CusEntryHeader entry) : base(entry)
			{
			}

			[List("CodePropertyList")]
			public ZString CodeProperty { get; set; }

			public ZPropertyInfo CodePropertyInfo => GetZPropertyInfo(nameof(CodeProperty));
			public CodeDescriptionPairList CodePropertyList
			{
				get
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("Y", "Yes");
					result.AddPair("N", "No");
					return result;
				}
			}
		}

		sealed class JobDeclarationMessageSendingObjectCollectionForTest : JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObjectForTest>
		{
			public JobDeclarationMessageSendingObjectCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new JobDeclarationMessageSendingObjectForTest this[int index] => base[index];
		}

		sealed class JobDeclarationMessageSendingGridColumnLayoutProviderForTest : IGridColumnLayoutProvider
		{
			public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
			IGridColumnLayout layout;

			IGridColumnLayout CreateLayout()
			{
				var builder = GridColumnLayoutBuilder.Create();
				builder.AddColumn<ZDropEditColumnStyleInfo>("CodeProperty", 100);
				return builder.Build();
			}
		}

		sealed class MessageSendingFormWithValidationDetailsForTest : MessageSendingFormWithValidationDetails
		{
			public MessageSendingFormWithValidationDetailsForTest(BaseMessageSendingObjectParent messageSendingObjectParent, bool showPreviewMessageCheckbox = false, bool sendSingleMessageForMultipleObjects = false) : base(messageSendingObjectParent)
			{
				this.showPreviewMessageCheckbox = showPreviewMessageCheckbox;
				this.sendSingleMessageForMultipleObjects = sendSingleMessageForMultipleObjects;
			}

			readonly bool showPreviewMessageCheckbox;
			readonly bool sendSingleMessageForMultipleObjects;

			protected override bool PreviewMessageCheckboxVisible => showPreviewMessageCheckbox;
			protected override bool ShouldSendSingleMessageForMultipleObjects => sendSingleMessageForMultipleObjects;
			protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => new JobDeclarationMessageSendingGridColumnLayoutProviderForTest();
		}
	}
}
