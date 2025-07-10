using System;
using System.Windows.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(DeclarationLockConfigRegistryItemEditor))]
	sealed class DeclarationLockConfigRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new DeclarationLockConfigRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DeclarationLockConfigControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DeclarationLockConfigControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var registryOptions = new RegistryOptions();
			return new DeclarationLockConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, registryOptions);
		}

		protected override object[] GetValidRegistryValues()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var collection = new DeclarationLockConfigCollection(fallbackLevel, Factory);

			var config = collection.AddNew();
			config.DeclarationType = "IMP";

			var eventLockInfo = config.EventInfos.AddNew();
			eventLockInfo.EventType = "ARV";
			eventLockInfo.EventReference = "REF=XXX";
			eventLockInfo.EventSource = "DEC";

			var tabLockInfo = config.TabInfos.AddNew();
			tabLockInfo.TabPage = "DEC";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
