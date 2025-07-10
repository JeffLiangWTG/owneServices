using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MessageSendingFormWithValidationDetails))]
	public sealed class MessageSendingFormWithValidationDetailsBaseOnlyTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		public void TestInitializeGridColumnsUsingMessageSendingObject()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			var parent = new MessageSedingObjectParentForTest(declaration);
			using var form = new MessageSendingFormWithValidationDetails(parent);
			form.Show();

			var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
			AssertGridColumn(nameof(MessageSendObjectForTest.MessageType), typeof(ZDropEditColumnStyleInfo), isMandatory: true, isVisible: true, columnWidth: 80);
			AssertGridColumn(nameof(MessageSendObjectForTest.IsExport), typeof(ZCheckBoxColumnStyleInfo), isMandatory: true, isVisible: true, columnWidth: 100);
			AssertGridColumn(nameof(MessageSendObjectForTest.EntryStatus), typeof(ZTextBoxColumnStyleInfo), isMandatory: true, isVisible: true, columnWidth: 100, "Status");
			AssertGridColumn(nameof(MessageSendObjectForTest.DeclarationDate), typeof(ZDateEditColumnStyleInfo), isMandatory: true, isVisible: true, columnWidth: 100,
					assertions: x => AssertEquals(ZDateTimePickerFormat.Short, (x as ZDateEditColumnStyleInfo).DateTimeFormat));
			AssertGridColumn(nameof(MessageSendObjectForTest.CreateTime), typeof(ZDateEditColumnStyleInfo), isMandatory: true, isVisible: false, columnWidth: 100,
					assertions: x => AssertEquals(ZDateTimePickerFormat.Long, (x as ZDateEditColumnStyleInfo).DateTimeFormat));

			void AssertGridColumn(string propertyName, Type expectedColumnType, bool isMandatory, bool isVisible, int columnWidth, string caption = null, Action<ZGridColumnInfo> assertions = null)
			{
				var columnStyleInfo = grid.GetColumnStyle(propertyName);
				AssertEquals(propertyName + ".Type", expectedColumnType, columnStyleInfo.GetType());
				AssertEquals(propertyName + ".IsMandatory", isMandatory, columnStyleInfo.IsMandatory);
				AssertEquals(propertyName + ".IsVisible", isVisible, columnStyleInfo.IsVisible);
				AssertEquals(propertyName + ".Width", columnWidth, columnStyleInfo.Width);
				if (caption != null)
				{
					AssertEquals(propertyName + ".Caption", caption, columnStyleInfo.CaptionResourceString.Caption);
				}
				assertions?.Invoke(columnStyleInfo);
			}
		}

		protected override MessageSendingFormWithValidationDetails GetFormToTestPreviewCheckbox(BaseMessageSendingObjectParent messageSendingObjectParent) =>
			new MessageSendingFormWithValidationDetailsForTest(messageSendingObjectParent);

		class MessageSendingFormWithValidationDetailsForTest : MessageSendingFormWithValidationDetails
		{
			public MessageSendingFormWithValidationDetailsForTest(BaseMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
			{
			}

			protected override bool PreviewMessageCheckboxVisible => true;
		}

		class MessageSedingObjectParentForTest(BaseJobDeclaration declaration) : JobDeclarationMessageSendingObjectParent<MessageSendObjectForTest>(declaration)
		{
			public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties =>
			[
				new MessageSendingObjectProperty(nameof(MessageSendObjectForTest.MessageType), ismandatory: true, 80),
				new MessageSendingObjectProperty(nameof(MessageSendObjectForTest.IsExport), ismandatory: true, 100),
				new MessageSendingObjectProperty(nameof(MessageSendObjectForTest.EntryStatus), ismandatory: true, 100, new ResourceStringData("MessageSendObjectForTest.EntryStatus", "Status")),
				new MessageSendingObjectProperty(nameof(MessageSendObjectForTest.DeclarationDate), ismandatory: true, 100),
				new MessageSendingObjectProperty(nameof(MessageSendObjectForTest.CreateTime), ismandatory: true, 100, isVisible: false)
			];
		}

		class MessageSendObjectForTest(CusEntryHeader entryHeader) : JobDeclarationMessageSendingObject(entryHeader)
		{
			public readonly CusEntryHeader EntryHeader = entryHeader;

			[List(nameof(MesasgeTypeList))]
			public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }
			public CodeDescriptionPairList MesasgeTypeList => new CodeDescriptionPairList();

			public ZBool IsExport => EntryHeader.IsExport;
			public ZDate DeclarationDate => EntryHeader.DeclarationDate.Date;
			public ZDateTime CreateTime => EntryHeader.CH_SystemCreateTimeUtc;
		}
	}

	public abstract class MessageSendingFormWithValidationDetailsAbstractTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MessageSendingFormWithValidationDetails(GetMessageSendingObjectParent());
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return base.ShouldIgnoreMissingBindingMember(control)
				|| control.Name == "SendWithValidationErrorsCheckBox"
				|| control.Name == "SendWithAdditionalWarningCheckBox"
				|| control.Name == "PreviewMessageCheckBox";
		}

		[RequiresSTA]
		public void TestPreviewMessageCheckBox()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (var form = GetFormToTestPreviewCheckbox(GetMessageSendingObjectParent()))
			{
				if (form != null)
				{
					var messageSendingObjectParent = form.MessageSendingObjectParent;
					var previewMessageCheckBox = form.PreviewMessageCheckBox;
					form.Show();
					CombineAssertions(() =>
					{
						Assert("should hidden for non-developer user", !previewMessageCheckBox.Visible);

						if (messageSendingObjectParent.SendingObjectsCollection.Count > 1)
						{
							var sendingAction1 = messageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().First();
							var sendingAction2 = messageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().Last();
							sendingAction1.ShouldSend = false;
							sendingAction2.ShouldSend = false;
							Assert("should disabled for 0 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Enabled);
							Assert("should not checked for 0 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Checked);

							sendingAction1.ShouldSend = true;
							Assert("should disabled for 1 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Enabled);
							Assert("should not checked for 1 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Checked);

							sendingAction2.ShouldSend = true;
							Assert("should disabled for more than 1 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Enabled);
							Assert("should not checked for more than 1 ShouldSend sendingAction(non-developer user)", !previewMessageCheckBox.Checked);
						}
					});
				}
			}

			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (var form = GetFormToTestPreviewCheckbox(GetMessageSendingObjectParent()))
			{
				if (form != null)
				{
					var messageSendingObjectParent = form.MessageSendingObjectParent;
					var previewMessageCheckboxVisible = form.GetType().GetProperty("PreviewMessageCheckboxVisible", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
					var previewMessageCheckBox = form.PreviewMessageCheckBox;

					form.Show();
					CombineAssertions(() =>
					{
						AssertEquals("should visible show for developer user", previewMessageCheckboxVisible, previewMessageCheckBox.Visible);

						if (messageSendingObjectParent.SendingObjectsCollection.Count > 1)
						{
							var sendingAction1 = messageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().First();
							var sendingAction2 = messageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().Last();
							sendingAction1.ShouldSend = false;
							sendingAction2.ShouldSend = false;
							Assert("should disabled for just 1 ShouldSend sendingAction(developer user)", !previewMessageCheckBox.Enabled);
							Assert("should not checked for 1 ShouldSend sendingAction(developer user)", !previewMessageCheckBox.Checked);

							sendingAction1.ShouldSend = true;
							AssertEquals("should enabled for 1 ShouldSend sendingAction(developer user)", previewMessageCheckboxVisible, previewMessageCheckBox.Enabled);
							AssertEquals("should checked for 1 ShouldSend sendingAction(developer user)", previewMessageCheckboxVisible, previewMessageCheckBox.Checked);

							sendingAction2.ShouldSend = true;
							Assert("should disabled for more than 1 ShouldSend sendingAction(developer user)", !previewMessageCheckBox.Enabled);
							Assert("should not checked for more than 1 ShouldSend sendingAction(developer user)", !previewMessageCheckBox.Checked);
						}
					});
				}
			}
		}

		protected virtual MessageSendingFormWithValidationDetails GetFormToTestPreviewCheckbox(BaseMessageSendingObjectParent messageSendingObjectParent) => GetFormToBash() as MessageSendingFormWithValidationDetails;

		protected virtual BaseMessageSendingObjectParent GetMessageSendingObjectParent()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = "EXP";

			_ = declaration.CustomsEntryHeaders.AddNew();
			_ = declaration.CustomsEntryHeaders.AddNew();
			return new MessageSendingObjectParentForTest<BaseMessageSendObjectForTest>(declaration);
		}

		class MessageSendingObjectParentForTest<TSendingAction> : BaseMessageSendingObjectParent<TSendingAction>
			where TSendingAction : BaseMessageSendObjectForTest
		{
			public MessageSendingObjectParentForTest(BaseJobDeclaration declaration) : base(declaration.Factory)
			{
				Declaration = declaration;
			}

			public BaseJobDeclaration Declaration;

			public override BusinessObject TopLevelBusinessObject { get; }

			public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError
			{
				get
				{
					var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
					var checkpoint = new SecurityCheckpoint("TS1", (NoResString)"Test 1", null, security.ZSecurityInstance)
					{
						IsAllowed = true
					};
					return checkpoint;
				}
			}

			protected override NonPersistentBusinessObjectCollection<TSendingAction> GetSendingObjectsCollectionCore()
			{
				var result = (MessageSendingActionCollectionForTest<TSendingAction>)Activator.CreateInstance(typeof(MessageSendingActionCollectionForTest), this);
				result.PopulateElements();
				return result;
			}
		}

		class MessageSendingActionCollectionForTest : MessageSendingActionCollectionForTest<BaseMessageSendObjectForTest>
		{
			public MessageSendingActionCollectionForTest(MessageSendingObjectParentForTest<BaseMessageSendObjectForTest> parentObject) : base(parentObject)
			{
			}
		}

		class MessageSendingActionCollectionForTest<TSendingAction> : NonPersistentBusinessObjectCollection<TSendingAction>
			where TSendingAction : BaseMessageSendObjectForTest
		{
			public MessageSendingActionCollectionForTest(MessageSendingObjectParentForTest<TSendingAction> parentObject)
				: base(parentObject.Declaration.Factory)
			{
				Parent = parentObject;
			}

			MessageSendingObjectParentForTest<TSendingAction> Parent { get; }

			protected override bool AllowNewCore => false;

			protected override bool AllowRemoveCore => false;

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}

			internal void PopulateElements()
			{
				foreach (var entryHeader in Parent.Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>())
				{
					Add((TSendingAction)Activator.CreateInstance(typeof(TSendingAction), entryHeader));
				}
			}
		}

		class BaseMessageSendObjectForTest : BaseMessageSendingObject
		{
			public BaseMessageSendObjectForTest(CusEntryHeader entryHeader) : base(entryHeader.Factory)
			{
				EntryHeader = entryHeader;
			}

			public readonly CusEntryHeader EntryHeader;
		}
	}
}
