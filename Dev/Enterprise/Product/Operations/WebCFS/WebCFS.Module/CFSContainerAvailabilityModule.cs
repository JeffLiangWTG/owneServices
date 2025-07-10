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
	public class CFSContainerAvailabilityModule : ZFilterGridModule
	{
		public CFSContainerAvailabilityModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override ZArchitecture.Modules.ModuleIdentifier ID
		{
			get { return WebModuleIDs.CFSContainerAvailability; }
		}

		public override Type FilterBusinessObjectType
		{
			get { return typeof(ContainerAvailabilityFilterBusinessObject); }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			return new FilterBusinessObjectDefaults();
		}

		protected override ZWebResource GetNewFilterControlResource()
		{
			return new ZWebResource(typeof(Web.Containers), "ContainersSearchUserControl.ascx", Page, "Enterprise.WebCFS.Web.Containers");
		}

		public override Type FilterControlType
		{
			get { return typeof(Web.ContainersSearchUserControl); }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			DataGridColumn[] result = new DataGridColumn[7];

			ZHyperLinkColumn containerNumberColumn = new ZHyperLinkColumn(Res.GetString("6c044917-f475-4bf5-b1a6-f8c4476d3dd6", "Container #"), vw_List_ContainerAvailabilitySchema.LCV_ContainerNum.Name);
			containerNumberColumn.DataNavigateUrlFormatString = UrlFormatWithAppRoot("Containers/ContainerDetails.aspx") + "?" + ZPage.RefParameterName + "={0}";
			containerNumberColumn.DataNavigateUrlFields = new string[1] { "PK" };
			result[0] = containerNumberColumn;

			result[1] = new ZTextEditColumn(Res.GetString("56e892f5-e540-46fe-a25b-7cd8a98ae89f", "Vessel"), vw_List_ContainerAvailabilitySchema.LCV_Vessel.Name);
			result[2] = new ZTextEditColumn(Res.GetString("5b30b20a-48b9-4519-889c-b8811e0f0af1", "Voyage"), vw_List_ContainerAvailabilitySchema.LCV_VoyageFlight.Name);

			ZDateTimeColumn estimateArrivalColumn = new ZDateTimeColumn(Res.GetString("f0011db8-cce7-468e-a0af-05b08d7a5364", "Estimate Arrival"), vw_List_ContainerAvailabilitySchema.LCV_EstimatedArrivalDate.Name);
			estimateArrivalColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			result[3] = estimateArrivalColumn;

			ZDateTimeColumn availableDateColumn = new ZDateTimeColumn(Res.GetString("813e288b-7470-4852-a1f5-bfe28ce79008", "Available Date"), vw_List_ContainerAvailabilitySchema.LCV_AvailabilityDate.Name);
			availableDateColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			result[4] = availableDateColumn;

			ZDateTimeColumn storageDateColumn = new ZDateTimeColumn(Res.GetString("757fe922-fbd5-4dd4-bf86-9a6c2ca0134b", "Storage Date"), vw_List_ContainerAvailabilitySchema.LCV_StorageDate.Name);
			storageDateColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			result[5] = storageDateColumn;

			ZDateTimeColumn unpackDateColumn = new ZDateTimeColumn(Res.GetString("a2fa8850-471d-499c-9c18-bf7913552e7a", "Date Unpacked"), vw_List_ContainerAvailabilitySchema.LCV_LCLUnpack.Name);
			unpackDateColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			result[6] = unpackDateColumn;

			return result;
		}

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return new[] { new ColumnAndSortOrder(vw_List_ContainerAvailabilitySchema.LCV_AvailabilityDate.Name, DefaultSortOrder) };
		}

		#endregion

		public override Type GridCollectionType
		{
			get { return typeof(ContainerAvailabilityCollection); }
		}
	}
}
