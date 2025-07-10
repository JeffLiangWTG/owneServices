using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(EIDOMessagingRegistryItemEditor))]
	internal class EIDOMessagingRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new EIDOMessagingRegistryItem("", null, null, null, new EIDOMessagingRegistryDataType());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EIDOMessagingRegistryItemEditor(new EIDOMessagingRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EIDOMessagingRegistryItemControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			EIDOMessagingHeader enabled = new EIDOMessagingHeader();
			enabled.Email = "bob@freadnet.org";
			EIDOMessagingIdentity identity = enabled.Identities.AddNew();
			identity.PrincipalPK = CreatePrincipal("Principal");
			identity.Password = "password1";
			identity.SenderID = "sender1";
			identity.RecipientID = "recipient1";
			EIDOMessagingHeader disabled = new EIDOMessagingHeader();
			return new object[] { disabled, enabled, };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			RegistryZUserControl control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		ZGuid CreatePrincipal(ZString code)
		{
			BusinessObjectFactory dirtyFactory = new BusinessObjectFactory();
			OrgHeader principal = dirtyFactory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = code;
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			dirtyFactory.Save();
			return principal.PK;
		}
		#endregion
	}
}
