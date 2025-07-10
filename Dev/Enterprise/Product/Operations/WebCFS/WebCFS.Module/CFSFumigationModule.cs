using System;
using System.Collections;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.WebCFS.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.WebCFS.Module
{
	public class CFSFumigationModule : ZFilterGridModule
	{
		public CFSFumigationModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ZArchitecture.Modules.ModuleIdentifier ID
		{
			get { return WebModuleIDs.CFSFumigation; }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(FumigationFilterBusinessObject); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(Web.Fumigation), "FumigationSearchUserControl.ascx", Page, "Enterprise.WebCFS.Web.Fumigation");
		}

		public override Type FilterControlType
		{
			get { return typeof(Web.FumigationSearchUserControl); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			ArrayList columns = new ArrayList();

			columns.Add(new ZTextEditColumn(Res.GetString("be8d109a-50ef-4ad5-9456-3ed62d5b8b6c", "Container"), vw_List_FumigationSchema.LFV_ContainerNum.Name));
			columns.Add(new ZTextEditColumn(Res.GetString("0a45f523-e543-431b-8128-bf3dd9af5dee", "Vessel"), vw_List_FumigationSchema.LFV_Vessel.Name));
			columns.Add(new ZTextEditColumn(Res.GetString("46b27dd8-032d-4fde-9406-a8f9a3b18d67", "Voyage"), vw_List_FumigationSchema.LFV_VoyageFlight.Name));
			columns.Add(new ZDateTimeColumn(Res.GetString("6c36feab-d294-4f2e-9d6e-369f271e265f", "Estimated Arrival"), vw_List_FumigationSchema.LFV_EstimatedArrivalDate.Name));
			columns.Add(new ZDateTimeColumn(Res.GetString("44968704-352b-43e9-b092-6301cdd2b889", "Fumigation Booked"), vw_List_FumigationSchema.LFV_FumigationBooked.Name));
			columns.Add(new ZDateTimeColumn(Res.GetString("108333d4-2f2a-4f21-bd9a-6e220712ec24", "Fumigation Completed"), vw_List_FumigationSchema.LFV_FumigationCompleted.Name));

			return (DataGridColumn[])columns.ToArray(typeof(DataGridColumn));
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(ZArchitecture.Business.FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(vw_List_FumigationSchema.LFV_FumigationBooked.Name, DefaultSortOrder) };
		}

		#endregion

		public override Type GridCollectionType
		{
			get { return typeof(FumigationCollection); }
		}
	}
}
