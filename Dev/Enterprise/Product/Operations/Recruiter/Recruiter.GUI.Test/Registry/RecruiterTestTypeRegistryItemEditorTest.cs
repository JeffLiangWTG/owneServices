using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(RecruiterTestTypeRegistryItemEditor))]
	class RecruiterTestTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new RecruiterTestTypeRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RecruiterTestTypeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RecruiterTestTypeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RecruiterTestTypeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			RecruiterTestTypeCollection collection = new RecruiterTestTypeCollection();
			RecruiterTestType testType = collection.AddNew();
			testType.Code = "TST";
			testType.Description = (NoResString)"Only for test.";
			testType.Category = RecruiterTestCategory.Codes.Accreditation;
			testType.NotificationType = RecruiterTestNotificationType.Codes.DoNotDeliver;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
		#endregion
	}
}
