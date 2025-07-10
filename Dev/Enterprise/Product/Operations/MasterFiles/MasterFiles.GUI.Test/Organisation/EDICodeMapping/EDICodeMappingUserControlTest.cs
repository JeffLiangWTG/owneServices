using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class EDICodeMappingUserControlTest : TestCaseWithFactory
	{
		public void TestLocalCodeControlVisibilityOnFormLoad()
		{
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, false, true, false);

			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Organisation, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Port, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Currency, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Country, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Commodities, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Equipment, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.ContainerType, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.Warehouse, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.IntZone, true, false, false);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.DocumentType, true, false, false);

			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.DropMode, false, false, true);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.IncoTerm, false, false, true);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.PackageType, false, false, true);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.EventCode, false, false, true);
			AssertVisibilityOnFormLoad(Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel, false, false, true);
		}

		public void TestLocalCodeControlVisibilityWhenRelationshipChanges()
		{
			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();

			using (var form = new ZForm())
			using (var control = new EDICodeMappingUserControlForTest())
			{
				form.SetDataBinding(orgPatternMatchOverride, ".");
				form.Controls.Add(control);
				form.Show();

				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.ChargeCodes, false, true, false);

				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Organisation, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Port, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Currency, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Country, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Commodities, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Equipment, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.ContainerType, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.Warehouse, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.ServiceLevel, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.IntZone, true, false, false);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.DocumentType, true, false, false);

				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.DropMode, false, false, true);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.IncoTerm, false, false, true);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.PackageType, false, false, true);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.EventCode, false, false, true);
				AssertVisibilityWhenRelationshipChanges(control, Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel, false, false, true);
			}
		}

		public void TestBinding()
		{
			AssertBinding(Constants.OrgPatternMatchOverrideRelationships.Organisation);
			AssertBinding(Constants.OrgPatternMatchOverrideRelationships.DropMode);
			AssertBinding(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes);
		}

		#region Implementation

		void AssertVisibilityOnFormLoad(string relationship, bool isLocalGuidFindBoxVisible, bool isLocalCodeFindBoxVisible, bool isLocalCodeDropEditVisible)
		{
			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride.OO_Relationship = relationship;

			using (var form = new ZForm())
			using (var control = new EDICodeMappingUserControlForTest())
			{
				form.SetDataBinding(orgPatternMatchOverride, ".");
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(isLocalCodeFindBoxVisible, control.LocalCodeFindBox_Exposed.Visible);
					AssertEquals(isLocalCodeDropEditVisible, control.LocalCodeDropEdit_Exposed.Visible);
					AssertEquals(isLocalGuidFindBoxVisible, control.LocalGuidFindBox_Exposed.Visible);
				});
			}
		}

		void AssertVisibilityWhenRelationshipChanges(EDICodeMappingUserControlForTest control, string relationship, bool isLocalGuidFindBoxVisible, bool isLocalCodeFindBoxVisible, bool isLocalCodeDropEditVisible)
		{
			control.RelationshipDropEdit_Exposed.Text = relationship;

			CombineAssertions(() =>
			{
				AssertEquals(isLocalCodeFindBoxVisible, control.LocalCodeFindBox_Exposed.Visible);
				AssertEquals(isLocalCodeDropEditVisible, control.LocalCodeDropEdit_Exposed.Visible);
				AssertEquals(isLocalGuidFindBoxVisible, control.LocalGuidFindBox_Exposed.Visible);
			});
		}

		void AssertBinding(string relationship)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();

			orgPatternMatchOverride.OO_OH = org1.PK;
			orgPatternMatchOverride.OO_ForeignCode = "ForeignCode";
			orgPatternMatchOverride.OO_Relationship = relationship;

			if (relationship == Constants.OrgPatternMatchOverrideRelationships.Organisation)
			{
				orgPatternMatchOverride.OO_LocalGuid = org2.PK;
			}
			else
			{
				orgPatternMatchOverride.OO_LocalCode = "LOCALCODE";
			}

			orgPatternMatchOverride.OO_Context = "CTX";

			using (var form = new ZForm())
			using (var control = new EDICodeMappingUserControlForTest())
			{
				form.SetDataBinding(orgPatternMatchOverride, ".");
				form.Controls.Add(control);
				form.Show();

				AssertEquals(typeof(OrgPatternMatchOverride), control.DataSourceType);
				CombineAssertions(() =>
				{
					AssertEquals(org1.OH_Code, control.OrganisationCodeGuidFindBox_Exposed.Text);
					AssertEquals("FOREIGNCODE", control.ForeignCodeTextBox_Exposed.Text);
					AssertEquals(relationship, control.RelationshipDropEdit_Exposed.Text);
					AssertEquals(relationship == Constants.OrgPatternMatchOverrideRelationships.ChargeCodes ? "LOCALCODE" : string.Empty, control.LocalCodeFindBox_Exposed.Text);
					AssertEquals(relationship == Constants.OrgPatternMatchOverrideRelationships.DropMode ? "LOCALCODE" : string.Empty, control.LocalCodeDropEdit_Exposed.Text);
					AssertEquals(relationship == Constants.OrgPatternMatchOverrideRelationships.Organisation ? org2.OH_Code.ToString() : string.Empty, control.LocalGuidFindBox_Exposed.Text);
					AssertEquals("CTX", control.ContextTextBox_Exposed.Text);
				});
			}
		}

		#endregion
	}

	public class EDICodeMappingUserControlForTest : EDICodeMappingUserControl
	{
		public ZGroupBox DetailsGroupBox_Exposed => DetailsGroupBox;
		public ZGuidFindBox OrganisationCodeGuidFindBox_Exposed => OrganisationCodeGuidFindBox;
		public ZTextBox ForeignCodeTextBox_Exposed => ForeignCodeTextBox;
		public ZCodeFindBox LocalCodeFindBox_Exposed => LocalCodeFindBox;
		public ZDropEdit RelationshipDropEdit_Exposed => RelationshipDropEdit;
		public ZDropEdit LocalCodeDropEdit_Exposed => LocalCodeDropEdit;
		public ZGuidFindBox LocalGuidFindBox_Exposed => LocalGuidFindBox;
		public ZDropEdit ContextTextBox_Exposed => ContextTextBox;
	}
}
