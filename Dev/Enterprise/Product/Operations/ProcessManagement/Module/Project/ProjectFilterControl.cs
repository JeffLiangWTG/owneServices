using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.Module
{
	public partial class ProjectFilterControl : ZFilterStripControl
	{
#if DEBUG
		[Obsolete("This constructor is just for the designer", true)]
		public ProjectFilterControl()
		{
		}
#endif

		public ProjectFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetCustomLabels();
				if (GridCollection != null)
				{
					WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, JobInvoicingConsumerTypes.Project.Code);
				}
			}
		}

		void SetCustomLabels()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				var textBoxColumnStyle = columnStyle as ZTextBoxColumnStyleInfo;
				if (textBoxColumnStyle != null)
				{
					switch (textBoxColumnStyle.ColumnName)
					{
						case AutoWorkProject.Schema.WKP_Type:
						case Project.Schema.TypeDescription:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.ProjectTypeLabel.Value;
							break;
						case AutoWorkProject.Schema.WKP_SubType:
						case Project.Schema.SubtypeDescription:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.ProjectSubtypeLabel.Value;
							break;
						case AutoWorkProject.Schema.WKP_Module:
						case Project.Schema.ModuleDescription:
							textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.ProjectModuleLabel.Value;
							break;
					}
				}
			}
		}
	}
}
