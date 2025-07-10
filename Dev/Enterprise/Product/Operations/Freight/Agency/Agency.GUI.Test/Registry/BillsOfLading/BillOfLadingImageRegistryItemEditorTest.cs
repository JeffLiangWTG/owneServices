using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Test.Registry.BillsOfLading
{
	[TestedType(typeof(BillOfLadingImageRegistryItemEditor))]
	public class BillOfLadingImageRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestEditorPaneLayout()
		{
			var editor = GetEditor();
			using (Control control = editor.NewWinFormsEditorPane())
			{
				editor.SetEditorPaneLayout(control, 432, 395);
				AssertEquals("should have the correct width", 432, control.Width);
				AssertEquals("should have the correct height", 395, control.Height);
				AssertEquals("should be anchored", AnchorStyles.Top | AnchorStyles.Left, control.Anchor);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new BillOfLadingImageRegistryItemEditor(RegistryItem.DataType, NewFallbackLevel(), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((BillOfLadingImageControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(BillOfLadingImageControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new BillOfLadingImageCollectionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new BillOfLadingImageCollection(NewFallbackLevel(), Factory);
			var bi = collection.AddNew();
			bi.PrincipalPK = Principal.PK;
			bi.Description = "Test";
			bi.Enabled = true;
			bi.Image = new Bitmap(1, 1);

			return new object[] { collection };
		}

		FallbackLevel NewFallbackLevel()
		{
			return new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = CreatePrincipal("Principal1");
				}

				return principal;
			}
		}

		OrgHeader principal;

		OrgHeader CreatePrincipal(ZString code)
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = code;
			principal.OH_IsShippingProvider = true;

			OrgCompanyData companyData = principal.CompanyData;
			companyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			return Factory.Load<OrgHeader>(principal.PK);
		}

		#endregion
	}
}
