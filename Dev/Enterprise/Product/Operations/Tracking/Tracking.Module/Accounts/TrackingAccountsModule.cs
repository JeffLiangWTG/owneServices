using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	/// <summary>
	/// Summary description for TrackingAccountsModule.
	/// </summary>
	public class TrackingAccountsModule : ZFilterGridModule
	{
		public TrackingAccountsModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override string GetBusinessObjectTableName(FilterBusinessObject filter) => AccTransactionHeaderSchema.Constants.TableName;

		public override ZArchitecture.Modules.ModuleIdentifier ID => WebModuleIDs.TrackingAccounts;

		public override Type FilterBusinessObjectType => typeof(TrackingTransactionFilterBusinessObject);

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults() => new FilterBusinessObjectDefaults();

		protected override ZWebResource GetNewFilterControlResource() => new ZWebResource(typeof(Web.Accounts.Transactions), "AccountsSearchUserControl.ascx", Page, "Enterprise.Tracking.Web.Accounts");

		public override Type FilterControlType => typeof(Web.Accounts.AccountsSearchUserControl);

		public InvoicePresenter Presenter => presenter ?? (presenter = new InvoicePresenter(Page.SiteUser as TrackingSiteUser));

		InvoicePresenter presenter;

		protected override DataGridColumn[] GetNewGridColumnFields() => Presenter.GetSearchResultColumns();

		protected override DataGridColumn[] GetDefaultGridColumnFields()
		{
			var defaultColumns = new DataGridColumn[GridColumnFields.Length - 3];
			for (var i = 0; i < defaultColumns.Length; i++)
			{
				defaultColumns[i] = GridColumnFields[i];
			}

			return defaultColumns;
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(AccTransactionHeaderSchema.AH_InvoiceDate.Name, DefaultSortOrder) };

		#endregion

		public override Type GridCollectionType => typeof(TrackingTransactionHeaderCollection);
	}
}
