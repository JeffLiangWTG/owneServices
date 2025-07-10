using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CustomsDSBCreditorOverrideRegistryItemEditor))]
	sealed class CustomsDSBCreditorOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public override void TestRegistryItemAcceptsEditorValue()
		{
			var testHelper = new ZA.Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();
			base.TestRegistryItemAcceptsEditorValue();
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CustomsDSBCreditorOverrideRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CustomsDSBCreditorOverrideUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CustomsDSBCreditorOverrideUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CustomsDSBCreditorOverrideRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			CustomsDSBCreditorOverrideCollection collection = new CustomsDSBCreditorOverrideCollection();
			CustomsDSBCreditorOverride creditor = collection.AddNew();
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			creditor.DistrictOfficeCode = "BBR";
			creditor.CreditorPK = organisation.PK;
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}
	}
}
