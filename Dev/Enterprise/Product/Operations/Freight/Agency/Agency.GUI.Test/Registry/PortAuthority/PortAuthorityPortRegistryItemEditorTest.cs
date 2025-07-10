using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(PortAuthorityPortRegistryItemEditor))]
	internal sealed class PortAuthorityPortRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			RegistryItemEditor editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 666, 333);
				AssertEquals("should have the correct width", 666, control.Width);
				AssertEquals("should have the correct height", 333, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom, control.Anchor);
			}
		}

		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PortAuthorityPortCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, new PortAuthorityPortCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PortAuthorityPortRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PortAuthorityPortControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			PortAuthorityPortCollection collection = new PortAuthorityPortCollection();
			AddSetting(collection, "AUBNE", 1);
			AddSetting(collection, "AUSYD", 2);
			return new object[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PortAuthorityPortControl)editorPane).ReadOnly;
		}

		void AddSetting(PortAuthorityPortCollection collection, ZString port, int num)
		{
			PortAuthorityPort setting = collection.AddNew();
			setting.Port = port;
			setting.Version = PortAuthorityVersionList.Codes.V11;
			setting.ProductionEmail = string.Format("manifest@server{0}.com.au", num);
			setting.ProductionID = string.Format("Prod Recipient {0}", num);
			setting.TestingEmail = string.Format("manitest@server{0}.com.au", num);
			setting.TestingID = string.Format("Test Recipient {0}", num);
		}

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		#endregion
	}
}
