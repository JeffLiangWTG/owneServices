using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentLookups_OLD : DtbTransportLookups
	{
		public DtbBookingConsignmentLookups_OLD(DtbBookingConsignment parent)
			: base(parent)
		{
		}

		#region BookingTemplates

		protected override DtbTransportTmplCollection GetTransportTemplates()
		{
			return Factory.GetCachedValue("DtbConsignmentLookups|BookingTemplates", () => GetNewConsignmentTemplateCollection());
		}

		DtbConsignmentTmplCollection GetNewConsignmentTemplateCollection()
		{
			var bookingTemplates = new DtbConsignmentTmplCollection(Factory, new ZQuery(DtbBookingTmplSchema.KT_IsActive, true));
			bookingTemplates.ApplySort(DtbBookingTmplSchema.Constants.KT_Description, ListSortDirection.Ascending);
			return bookingTemplates;
		}

		#endregion

		#region DistanceUnits

		public CodeDescriptionPairList DistanceUnits
		{
			get { return Factory.GetCachedValue("DtbConsignmentLookups|DistanceUnits", () => new DistanceUnitList()); }
		}

		#endregion
	}
}
