using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class CommunicationFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRemoveOrganizationColumns()
		{
			using (var filterControl = new CommunicationFilterControl(new OrgSalesCallCollection(Factory), new CommunicationFilterBusinessObject(), null))
			{
				var columnStyles = filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>();

				Assert("Show organization columns if there is no header", columnStyles.Any(c => c.ColumnName == OrgSalesCallSchema.OQ_OH.Name));
				Assert("Show organization columns if there is no header", columnStyles.Any(c => c.ColumnName == OrgSalesCall.Schema.OrgName));
			}

			using (var filterControl = new CommunicationFilterControl(new OrgSalesCallCollection(Factory), new CommunicationFilterBusinessObject(), Factory.New<OrgHeader>()))
			{
				var columnStyles = filterControl.Grid.ColumnStyles.Cast<ZGridColumnInfo>();

				Assert("Do not show organization columns if there is header", !columnStyles.Any(c => c.ColumnName == OrgSalesCallSchema.OQ_OH.Name));
				Assert("Do not show organization columns if there is header", !columnStyles.Any(c => c.ColumnName == OrgSalesCall.Schema.OrgName));
			}
		}

		[RequiresSTA]
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = OrgSalesCallWorkflowDescriptor.WorkflowTypeCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new OrgSalesCollection(Factory);
			var filter = new CommunicationFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new CommunicationFilterControl(collection, filter, null))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		[RequiresSTA]
		public void TestPurposeColumnCaption()
		{
			OrganisationsDataRegistry.Instance.CategoryListLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Custom Purpose");
			var collection = new OrgSalesCallCollection(Factory);
			using (var control = new CommunicationFilterControl(collection, new CommunicationFilterBusinessObject(), null))
			{
				ZTextBoxColumnStyleInfo purposeColumnStyle = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if ((textBoxColumnStyle != null) && (textBoxColumnStyle.ColumnName == OrgSalesCallSchema.OQ_Category.Name))
					{
						purposeColumnStyle = textBoxColumnStyle;
						break;
					}
				}

				AssertNotNull("Column should exist initially", purposeColumnStyle);
				AssertEquals("purposeColumnStyle.Caption", "Custom Purpose", purposeColumnStyle.Caption);
			}
		}
	}
}
