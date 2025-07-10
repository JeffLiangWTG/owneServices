using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public partial class CommunicationFilterControl : ZFilterStripControl
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public CommunicationFilterControl()
		{
			InitializeComponent();
		}

		public CommunicationFilterControl(IBusinessObjectCollection collection, CommunicationFilterBusinessObject strip, OrgHeader header)
			: base(collection, strip)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetPurposeColumnCaption();
				if (header != null)
				{
					RemoveOrganizationColumns();
				}

				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, OrgSalesCallWorkflowDescriptor.WorkflowTypeCode);
			}
		}

		void SetPurposeColumnCaption()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					if (textBoxColumnStyle.ColumnName == OrgSalesCallSchema.OQ_Category.Name)
					{
						textBoxColumnStyle.Caption = OrganisationsDataRegistry.Instance.CategoryListLabel.Value;
					}
				}
			}
		}

		void RemoveOrganizationColumns()
		{
			var organizationColumns = new HashSet<string>
			{
				OrgSalesCallSchema.OQ_OH.Name,
				OrgSalesCall.Schema.OrgName
			};

			var columnStyles = Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			foreach (var columnStyle in columnStyles)
			{
				if (organizationColumns.Contains(columnStyle.ColumnName))
				{
					Grid.ColumnStyles.Remove(columnStyle);
				}
			}
		}
	}
}
