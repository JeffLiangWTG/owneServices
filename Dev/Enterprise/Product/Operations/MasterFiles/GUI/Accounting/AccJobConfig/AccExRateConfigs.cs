using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AccExRateConfigs : ZUserControl
	{
		public AccExRateConfigs()
		{
			InitializeComponent();
		}

		BusinessObjectFactory dataSourceFactory;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is IBusiness bizObj)
			{
				dataSourceFactory = bizObj.Factory;
				dataSourceFactory.SetContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig);
			}
			UpdateJobExRateGridColumns();
		}

		void UpdateJobExRateGridColumns()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool showInvoiceCurrencyType = !IsSystemLevel && InvoiceCurrencyTypeEnabled;

				MasterGrid.SetAvailability(showInvoiceCurrencyType, AccExchangeRateConfiguration.Schema.JCE_InvoiceCurrencyType);
			}
		}

		bool IsSystemLevel =>
			DataSource is AccExchangeRateConfigurationCollection collection
			&& collection.Level == AccExRateConfigurationLevelEnum.System;

		bool InvoiceCurrencyTypeEnabled =>
			AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.GetFallBackValueAtAllLevels(CompanyPK, Guid.Empty, Guid.Empty);

		Guid CompanyPK
		{
			get
			{
				switch (DataSource)
				{
					case AccExchangeRateConfigurationCollection collection:
						return collection.CompanyPK.IsEmpty ? Guid.Empty : collection.CompanyPK.ToGuid();

					case GlbCompany company:
						return company.PK.ToGuid();

					default:
						return Env.CurrentCompanyPK;
				}
			}
		}

		//public bool ReadOnly { get => zGrid1.ReadOnly; set => zGrid1.ReadOnly = value; }
		public bool ReadOnly { get; set; }

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (dataSourceFactory != null)
			{
				dataSourceFactory.RemoveContext(BusinessContext.PermittedToDeleteJobExchangeRateConfig);
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
