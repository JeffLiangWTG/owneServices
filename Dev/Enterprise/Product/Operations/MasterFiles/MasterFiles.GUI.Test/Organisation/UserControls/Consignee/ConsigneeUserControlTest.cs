using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ConsigneeUserControl))]
	sealed class ConsigneeUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		public void TestCPDecPlugInIsAddedOnConsigneePage()
		{
			OrgFormForTest testForm;
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsConsignee = true;
			using (testForm = new OrgFormForTest(organisation))
			{
				testForm.Show();
				testForm.OrgTabControl.SelectedTab = testForm.ConsigneeTabPage;
				bool containsPlugIn = false;
				for (int i = 0; i < testForm.ConsigneeControl.ConsigneeTabControl.PlugIns.Instances.Length; i++)
				{
					string name = testForm.ConsigneeControl.ConsigneeTabControl.PlugIns.Instances[i].Name;
					if (name == "Default CP Questions and Answers")
					{
						containsPlugIn = true;
						break;
					}
				}
				Assert("PlugIns should include Default CP Questions and Answers", containsPlugIn);
			}
		}

		public void TestCustomsAdditionalDetailsPlugInVisibility()
		{
			OrgFormForTest testForm;
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsConsignee = true;
			var link = organisation.SupplierLinks.AddNew();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;

			using (testForm = new OrgFormForTest(organisation))
			{
				testForm.Show();
				testForm.OrgTabControl.SelectedTab = testForm.ConsigneeTabPage;
				testForm.ConsigneeControl.RelationshipsTabPage.Show();

				foreach (ZPlugIn plugin in testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.PlugIns.Instances)
				{
					if (plugin.Name == "Additional Customs Defaults")
					{
						AssertEquals(true, plugin.Enabled);

						link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Afghanistan;
						AssertEquals(true, plugin.Enabled);
						break;
					}
				}
			}
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new ConsigneeUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyConsignee", "IsModifyConsigneeDetails", "IsModifyConsigneeLandedCosting", "IsModifyConsigneeRelationships" }; }
		}
	}
}
