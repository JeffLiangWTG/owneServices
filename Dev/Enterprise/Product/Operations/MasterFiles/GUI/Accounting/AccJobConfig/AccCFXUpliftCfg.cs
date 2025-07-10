using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccCFXUpliftCfg : ZUserControl
	{
		public AccCFXUpliftCfg()
		{
			InitializeComponent();
		}

		BusinessObjectFactory dataSourceFactory;

		public ZGrid Grid => zGrid1;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is IBusiness bizObj)
			{
				dataSourceFactory = bizObj.Factory;
				dataSourceFactory.SetContext(BusinessContext.PermittedToDeleteCFXUpliftConfig);
			}

			UpdateDateColumns();
		}

		void UpdateDateColumns()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var shouldEnableDateColumns = AccountingMasterFilesRegistry.Instance.EnableCFXUpliftStartAndExpiryDateColumns.Value;

				Grid.SetAvailability(shouldEnableDateColumns, AccCFXUpliftConfiguration.Schema.JCF_StartDate);
				Grid.SetAvailability(shouldEnableDateColumns, AccCFXUpliftConfiguration.Schema.JCF_ExpiryDate);
			}
		}
	}
}
