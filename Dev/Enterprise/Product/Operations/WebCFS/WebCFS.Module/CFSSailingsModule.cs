using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.WebCFS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.WebCFS.Module
{
	public class CFSSailingsModule : ZFilterGridModule
	{
		public CFSSailingsModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ZArchitecture.Modules.ModuleIdentifier ID
		{
			get { return WebModuleIDs.CFSSailings; }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(SailingFilterBusinessObject); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(Web.Sailing), "SailingSearchUserControl.ascx", Page, "Enterprise.WebCFS.Web.Sailing");
		}

		public override Type FilterControlType
		{
			get { return typeof(Web.SailingSearchUserControl); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] result = new DataGridColumn[7];

			result[0] = new ZTextEditColumn(Res.GetString("841d0bb4-6012-47b3-b771-0d20348cdf87", "Vessel"), Freight.Business.BaseJobSailing.Schema.JX_JV_NKVessel);
			result[1] = new ZTextEditColumn(Res.GetString("9be70e15-62ae-4f6b-aa9a-adb4f7162c60", "Voyage"), Freight.Business.BaseJobSailing.Schema.JX_JV_VoyageFlight);
			result[2] = new ZTextEditColumn(Res.GetString("c8094d9b-5a92-41a7-b6da-d428d22145d4", "Load"), Freight.Business.BaseJobSailing.Schema.JX_JA_RL_NKPortOfLoading);

			ZDateTimeColumn eTDColumn = new ZDateTimeColumn(Res.GetString("a8a720a6-4777-434a-a849-97f859ef2e4a", "ETD"), Freight.Business.BaseJobSailing.Schema.JX_JA_E_DEP);
			eTDColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			result[3] = eTDColumn;

			result[4] = new ZTextEditColumn(Res.GetString("549e5456-844f-4b6d-9a63-6696a09b849f", "Discharge"), Freight.Business.BaseJobSailing.Schema.JX_JB_RL_NKPortOfDischarge);

			ZDateTimeColumn eTAColumn = new ZDateTimeColumn(Res.GetString("ef53a71d-0f59-4298-8c80-ddf350f14b16", "ETA"), Freight.Business.BaseJobSailing.Schema.JX_JB_E_ARV);
			eTAColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			result[5] = eTAColumn;

			ZDateTimeColumn fCLColumn = new ZDateTimeColumn(Res.GetString("e7b3c1c1-6a69-4f4a-a24f-2b78288f7245", "CTO Available"), Freight.Business.BaseJobSailing.Schema.JX_JB_CTOAvailabilityDate);
			fCLColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			result[6] = fCLColumn;

			return result;
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(JobSailingSchema.JX_DepotAvailabilityDate.Name, DefaultSortOrder) };
		}

		#endregion

		public override Type GridCollectionType
		{
			get { return typeof(SailingCollection); }
		}
	}
}
