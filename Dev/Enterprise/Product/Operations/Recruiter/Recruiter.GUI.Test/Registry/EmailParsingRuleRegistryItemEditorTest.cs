using System;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI.Testing
{
	[TestedType(typeof(EmailParsingRuleRegistryItemEditor))]
	public class EmailParsingRuleRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EmailParsingRuleRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((EmailParsingRuleControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EmailParsingRuleControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EmailParsingRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EmailParsingRuleCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EmailParsingRuleCollection() };
		}
	}
}
