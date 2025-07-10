using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AllowedOrgsAndWarehousesControl : ZUserControl, IFindBox
	{
		public AllowedOrgsAndWarehousesControl()
		{
			InitializeComponent();
		}

		void AddButton_Click(object sender, EventArgs e)
		{
			var orgModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID);
			var moduleDecisionpRovider = new OrgModuleDecisionProvider(this);
			orgModule.OverrideModuleDecisionProvider(moduleDecisionpRovider);
			if (Provider.SecurityAllowedOrgsAndWarehousesView.Count > 0)
			{
				moduleDecisionpRovider.AlreadySelectedFilter = GetAdditionalFilter();
			}
			orgPopup = new EmbeddedModulePopup(orgModule);
			orgPopup.ShowModal(this, (ZForm)FindForm());
		}

		ZQuery GetAdditionalFilter()
		{
			ZQuery additionalFilter = null;
			var pks = Provider.SecurityAllowedOrgsAndWarehousesView.Cast<GlbSecurity>().Select(el => el.GU_ItemGUID);
			switch (ModuleID.ID)
			{
				case ModuleId.Organisation:
					additionalFilter = new ZQuery(OrgHeaderSchema.PK, pks);
					break;
				case ModuleId.WhsConfigWarehouse:
					additionalFilter = new ZQuery(WhsWarehouseSchema.PK, pks);
					break;
			}
			return additionalFilter;
		}

		class OrgModuleDecisionProvider : PopupModuleDecisionProvider, IFilterModuleExtraNotificationProvider
		{
			public OrgModuleDecisionProvider(IFindBox findBox) : base(findBox)
			{
			}

			public INotification GetExtraNotification(BusinessObject businessObject)
			{
				if (AlreadySelectedFilter != null && businessObject.MatchesFilter(AlreadySelectedFilter))
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("9E2E257E-01B6-4097-A8DC-41B96FFF860C", "This record is already selected"));
				}
				return null;
			}

			public ZQuery AlreadySelectedFilter { get; set; }
		}

		#region Properties

		public void SetAllowedOrgsAndWarehousesControlData(ISecurityCheckpoint security)
		{
			fSecurityCheckpoin = security;
			SetDescriptionLabelText(security);
		}

		Type typeOfFindBoxCollection;
		public Type TypeOfFindBoxCollection
		{
			get { return typeOfFindBoxCollection ?? typeof(ShipsAgencyPrincipalCollection); }
			set { typeOfFindBoxCollection = value; }
		}

		public ModuleIdentifier ModuleID
		{
			get
			{
				return (fSecurityCheckpoin != null && fSecurityCheckpoin.LookupKey == Environment.Env.Security.WhsAllowedWarehouses.LookupKey)
					? ModuleIDs.WhsConfigWarehouse
					: ModuleIDs.Organisation;
			}
		}

		#endregion

		#region IFindBox Members

		string IFindBox.Code
		{
			get { return ""; }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					Provider.AddSecurityToAccessOrgOrWarehouse(value);
				}
			}
		}

		string IFindBox.Description
		{
			get { return ""; }
			set { }
		}

		IFindBoxListProvider IFindBox.ListProvider
		{
			get { return (IFindBoxListProvider)Activator.CreateInstance(TypeOfFindBoxCollection, Provider.Factory); }
		}

		IFindBoxPopup IFindBox.PopupForm
		{
			get { return orgPopup; }
		}

		#endregion

		#region Implementation

		void SetDescriptionLabelText(ISecurityCheckpoint security)
		{
			if (security.Code == Env.Security.AgencyPrincipalAccess.Code)
			{
				DescriptionLabel.Text = Res.GetString("AllowedOrgsAndWarehousesControl|DescriptionLabelForAgencyPrincipalAccess", "If the above security permission to access shipment and allocation information for all principals is denied, then access is still granted for shipment and allocation information relating to the following principals.");
			}
			else if (security.Code == Env.Security.WhsAllowedClients.Code)
			{
				DescriptionLabel.Text = Res.GetString("AllowedOrgsAndWarehousesControl|DescriptionLabelForWhsAllowedClients", "If the above security permission to access warehouse and allocation information for all clients is denied, then access is still granted for warehouse and allocation information relating to the following clients.");
			}
			else if (security.Code == Env.Security.WhsAllowedWarehouses.Code)
			{
				DescriptionLabel.Text = Res.GetString("AllowedOrgsAndWarehousesControl|DescriptionLabelForWhsAllowedWarehouses", "If the above security permission to access warehouse and allocation information for all warehouses is denied, then access is still granted for warehouse and allocation information relating to the following warehouses.");
			}
		}

		IOrgsAndWarehousesAccessProvider Provider
		{
			get { return (IOrgsAndWarehousesAccessProvider)base.DataSource; }
		}

		EmbeddedModulePopup orgPopup;
		ISecurityCheckpoint fSecurityCheckpoin;

		#endregion
	}
}
