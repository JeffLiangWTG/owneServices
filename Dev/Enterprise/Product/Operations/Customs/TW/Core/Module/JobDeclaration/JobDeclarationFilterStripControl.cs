using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var messageStatusColumn = FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_MessageStatus);
				if (messageStatusColumn != null)
				{
					((ZTextBoxColumnStyleInfo)messageStatusColumn).GroupName = Enterprise.Customs.TW.Module.Res.GetData("JobDeclarationFilterStripControl|a223bcbb-c1ec-4a04-92b9-2d1c87380984", "Message Status");
				}

				var messageStatusDescriptionColumn = FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_MessageStatusDescription);
				if (messageStatusDescriptionColumn != null)
				{
					messageStatusDescriptionColumn.IsVisible = true;
					((ZTextBoxColumnStyleInfo)messageStatusDescriptionColumn).GroupName = Enterprise.Customs.TW.Module.Res.GetData("JobDeclarationFilterStripControl|a223bcbb-c1ec-4a04-92b9-2d1c87380984", "Message Status");
				}

				var importerNameColumn = FilteredGrid.GetColumnStyle(JobDeclaration.Schema.ImporterName);
				if (importerNameColumn != null && !importerNameColumn.IsVisible)
				{
					importerNameColumn.IsVisible = true;
				}

				var supplierNameColumn = FilteredGrid.GetColumnStyle(JobDeclaration.Schema.SupplierName);
				if (supplierNameColumn != null && !supplierNameColumn.IsVisible)
				{
					supplierNameColumn.IsVisible = true;
				}

				var entryReleaseDateColumn = FilteredGrid.GetColumnStyle(BaseJobDeclaration.Schema.EntryReleaseDate);
				if (entryReleaseDateColumn != null && !entryReleaseDateColumn.IsVisible)
				{
					entryReleaseDateColumn.IsVisible = true;
				}
			}
		}
	}
}
