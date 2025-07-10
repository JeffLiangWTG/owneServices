using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
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
	sealed class OrgOpportunityFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestProductTypeColumnCaption()
		{
			OrganisationsDataRegistry.Instance.ProductTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Product Type");
			var collection = new OrgOpportunityCollection(Factory);
			using (var control = new OrgOpportunityFilterControl(collection, new OrgOpportunityFilterBusinessObject()))
			{
				ZTextBoxColumnStyleInfo productTypeColumnStyle = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if ((textBoxColumnStyle != null) && (textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_PackageType.Name))
					{
						productTypeColumnStyle = textBoxColumnStyle;
						break;
					}
				}

				AssertNotNull("Precondition: column exists", productTypeColumnStyle);
				AssertEquals("productTypeColumnStyle.Caption", "My Product Type", productTypeColumnStyle.Caption);
			}
		}

		[RequiresSTA]
		public void TestTotalDiscountColumnCaption()
		{
			OrganisationsDataRegistry.Instance.CurrentLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Discount Value");
			var collection = new OrgOpportunityCollection(Factory);
			using (var control = new OrgOpportunityFilterControl(collection, new OrgOpportunityFilterBusinessObject()))
			{
				ZTextBoxColumnStyleInfo styleInfo = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if (textBoxColumnStyle != null && textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
					{
						styleInfo = textBoxColumnStyle;
						break;
					}
				}

				AssertNotNull("Precondition: column exists", styleInfo);
				AssertEquals("Caption", "Discount Value", styleInfo.Caption);
			}
		}

		[RequiresSTA]
		public void TestRentalMultiplierColumnCaption()
		{
			OrganisationsDataRegistry.Instance.PotentialLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Rental Category");
			var collection = new OrgOpportunityCollection(Factory);
			using (var control = new OrgOpportunityFilterControl(collection, new OrgOpportunityFilterBusinessObject()))
			{
				ZTextBoxColumnStyleInfo styleInfo = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
					if (textBoxColumnStyle != null && textBoxColumnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
					{
						styleInfo = textBoxColumnStyle;
						break;
					}
				}

				AssertNotNull("Precondition: column exists", styleInfo);
				AssertEquals("Caption", "Rental Category", styleInfo.Caption);
			}
		}

		[RequiresSTA]
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = OpportunityWorkflowDescriptor.WorkflowTypeCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new OrgOpportunityCollection(Factory);
			var filter = new OrgOpportunityFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new OrgOpportunityFilterControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		[RequiresSTA]
		public void TestOrganisationRelatedPortColumn()
		{
			var testOrgs = Factory.Load<OrgHeader>(new ZQuery { MaximumRows = 1 });
			AssertEquals(1, testOrgs.Length);
			var org = testOrgs[0];
			org.OH_RL_NKClosestPort = "AUSYD";
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			opp.P8_OH = org.PK;

			var collection = new OrgOpportunityCollection(Factory);
			collection.Add(opp);
			var filter = new OrgOpportunityFilterBusinessObject();

			Factory.Save();

			using (var form = new ZForm())
			using (var filterControl = new OrgOpportunityFilterControl(collection, filter))
			{
				var columnName = $"Header+{OrgHeaderSchema.OH_RL_NKClosestPort.Name}";
				var columnStyle = filterControl.Grid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().Single(c => c.ColumnName == columnName);
				columnStyle.IsVisible = true;

				form.Controls.Add(filterControl);
				form.Show();

				var gridColumnStyles = filterControl.Grid.TableStyles[0].GridColumnStyles;
				var columnIndex = gridColumnStyles.IndexOf(gridColumnStyles[columnName]);
				var cell = new DataGridCell(0, columnIndex);
				AssertEquals(org.OH_RL_NKClosestPort, filterControl.Grid[cell].ToString());
			}
		}

		[RequiresSTA]
		public void TestOpportunityRecallDateColumn()
		{
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			opp.P8_RecallDateLocal = new ZDateTime(2022, 01, 05, 12, 00, 12);

			var collection = new OrgOpportunityCollection(Factory);
			collection.Add(opp);
			var filter = new OrgOpportunityFilterBusinessObject();

			Factory.Save();

			using (var form = new ZForm())
			using (var filterControl = new OrgOpportunityFilterControl(collection, filter))
			{
				var columnName = "P8_RecallDateLocal";
				var columnStyle = filterControl.Grid.ColumnStyles.OfType<ZDateEditColumnStyleInfo>().Single(c => c.ColumnName == columnName);
				columnStyle.IsVisible = true;

				form.Controls.Add(filterControl);
				form.Show();

				var gridColumnStyles = filterControl.Grid.TableStyles[0].GridColumnStyles;
				var columnIndex = gridColumnStyles.IndexOf(gridColumnStyles[columnName]);
				var cell = new DataGridCell(0, columnIndex);
				AssertEquals("05-Jan-22 12:00:12", filterControl.Grid[cell].ToString());
			}
		}
	}
}
