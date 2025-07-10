using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class GlbCompanyCampaignFilterControlTest : TestCaseWithFactory
	{
		public void TestCategoryColumnCaptions()
		{
			OrganisationsDataRegistry.Instance.CampaignCategory1Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My CRM Category 1");
			OrganisationsDataRegistry.Instance.CampaignCategory2Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My CRM Category 2");
			OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My HR Category 1");
			OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My HR Category 2");

			var collection = new OrgOpportunityCollection(Factory);

			using (var control = new GlbCompanyCampaignFilterControl(collection,
				new GlbCompanyCampaignFilterBusinessObject()))
			{
				ZTextBoxColumnStyleInfo category1ColumnStyle = null;
				ZTextBoxColumnStyleInfo category2ColumnStyle = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if (textBoxColumnStyle != null)
					{
						if (textBoxColumnStyle.ColumnName == GlbCompanyCampaignSchema.G0_Category.Name)
						{
							category1ColumnStyle = textBoxColumnStyle;
						}
						else if (textBoxColumnStyle.ColumnName == GlbCompanyCampaignSchema.G0_Type.Name)
						{
							category2ColumnStyle = textBoxColumnStyle;
						}
					}
				}

				AssertNotNull("Precondition: column exists", category1ColumnStyle);
				AssertNotNull("Precondition: column exists", category2ColumnStyle);
				AssertEquals("category1ColumnStyle.Caption", "My CRM Category 1", category1ColumnStyle.Caption);
				AssertEquals("category2ColumnStyle.Caption", "My CRM Category 2", category2ColumnStyle.Caption);
			}
		}

		public void TestCRMCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = CRMCampaignWorkflowDescriptor.WorkflowTypeCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new GlbCompanyCampaignCollection(Factory);
			var filter = new GlbCompanyCampaignFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new GlbCompanyCampaignFilterControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}
	}
}
