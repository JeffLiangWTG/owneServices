
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingTmplFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", DtbBookingTmplSchema.KT_Code).MultilingualDescription = ResString.GetMultilingualString("TransportBooking|TransportBookingTemplateFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", DtbBookingTmplSchema.KT_Description, typeof(DtbBookingTmpl), ResString.GetMultilingualString("TransportBooking|TransportBookingTemplateFilter|Description", "Description"));
			filters.AddTextFilter("Direction", DtbBookingTmplSchema.KT_Direction, BindToLists.Directions).MultilingualDescription = ResString.GetMultilingualString("Freight|TransportBookingTemplateFilter|Direction", "Direction");
			filters.AddTextFilter("Organization Type", GetOrganisationFilter, BindToLists.OrganisationTypes).MultilingualDescription = ResString.GetMultilingualString("Freight|TransportBookingTemplateFilter|OrganizationType", "Organization Type");
		}

		ZQuery GetOrganisationFilter(ZString value)
		{
			if (!value.IsEmpty)
			{
				var result = new ZDBOnlyQuery(typeof(DtbBookingTmpl));
				var subQuery = new ZDBOnlySubQuery(typeof(DtbBookingInstructionTmpl), DtbBookingInstructionTmplSchema.K2_KT_BookingTmpl);
				subQuery.AddToFilter(DtbBookingInstructionTmplSchema.K2_OrgType, value);
				result.AddSubQuery(subQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		BindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}
		BindToLists bindToLists;
	}
}
