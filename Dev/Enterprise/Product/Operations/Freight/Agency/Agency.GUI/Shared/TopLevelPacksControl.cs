using System;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class TopLevelPacksControl : ZUserControl
	{
		public TopLevelPacksControl()
		{
			InitializeComponent();
			importReleaseNumberColumnStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.IsDesignMode())
			{
				new UNDGDataItemFormManager(TopLevelPacksGrid).Initialize();

				TariffFindHelper.AddDefaultPropertyToTariffControl(harmonisedCodeColumnStyleInfo, null);

				this.workflowFormHelper = new AgencyContainerWorkflowFormHelper(TopLevelPacksGrid);
				this.workflowFormHelper.Hook();
			}
		}

		public void RemoveColumnsWhichAreUnavailableOnBookings()
		{
			var columnsToRemove = new[]
			{
				JobContainerSchema.Constants.JC_ArrivalCartageComplete,
				JobContainerSchema.Constants.JC_ArrivalSlotDateTime,
				JobContainerSchema.Constants.JC_ArrivalSlotReference,
				JobContainerSchema.Constants.JC_DepartureCartageComplete,
				JobContainerSchema.Constants.JC_DepartureSlotDateTime,
				JobContainerSchema.Constants.JC_DepartureSlotReference,
				JobContainerSchema.Constants.JC_FCLOnBoardVessel,
				JobContainerSchema.Constants.JC_FCLUnloadFromVessel,
				JobContainerSchema.Constants.JC_FCLWharfGateIn,
				JobContainerSchema.Constants.JC_FCLWharfGateOut,
				JobContainerSchema.Constants.JC_ContainerImportDORelease,
				JobContainerSchema.Constants.JC_StowagePosition,
			};

			foreach (ZGridColumnInfo columnInfo in TopLevelPacksGrid.ColumnStyles.ToArray())
			{
				if (columnsToRemove.Contains(columnInfo.ColumnName))
				{
					TopLevelPacksGrid.ColumnStyles.Remove(columnInfo);
				}
			}
		}

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing && workflowFormHelper != null)
			{
				workflowFormHelper.Dispose();
				workflowFormHelper = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		AgencyContainerWorkflowFormHelper workflowFormHelper;

		#endregion
	}
}


