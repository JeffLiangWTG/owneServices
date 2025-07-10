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
	[TestedType(typeof(FinancialAccountNumberPortMapRegistryItemEditor))]
	sealed class FinancialAccountNumberPortMapRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new FinancialAccountNumberPortMapRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((FinancialAccountNumberPortMapUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(FinancialAccountNumberPortMapUserControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new FinancialAccountNumberPortMapRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			creditor.OrganizationPK = organisation.PK;
			creditor.CustomsOfficeCode = "BBR";
			creditor.CreditorPK = organisation.PK;
			creditor.FinancialAccountNumber = "3924089023";
			creditor.AccountStartDay = 1;
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
