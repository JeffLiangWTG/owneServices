using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.CountryStatesGlbHoliday)]
	public class CountryStatesGlbHolidayBizoCollection : NonPersistentBusinessObjectCollection<CountryStatesGlbHolidayBizo>
	{
		public CountryStatesGlbHolidayBizoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			glbHolidays = new ActiveBusinessObjectCollection<GlbHoliday>(factory);
			((IBusinessObjectState)glbHolidays).NotificationsChanged += CountryStatesGlbHolidayBizoCollection_NotificationsChanged;
		}

		readonly ActiveBusinessObjectCollection<GlbHoliday> glbHolidays;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryStatesGlbHolidayBizo(Factory);
		}

		public override void Load(ZQuery query)
		{
			glbHolidays.AdditionalFilter = query;
			Reload();
		}

		void CountryStatesGlbHolidayBizoCollection_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			Reload();
		}

		void Reload()
		{
			RemoveAll();
			foreach (var bizo in ToCountryStateBizos(Factory, glbHolidays))
			{
				Add(bizo);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "<Pending>")]
		static IEnumerable<CountryStatesGlbHolidayBizo> ToCountryStateBizos(BusinessObjectFactory factory, IEnumerable<GlbHoliday> glbHolidays)
		{
			foreach (var groupedHoliday in glbHolidays.Where(x => x.GH_ParentTableCode == RefCountryStatesSchema.Constants.Prefix).GroupBy(x => new { x.GH_HolidayName, x.GH_ParentTableCode, x.GH_RecurrType, x.GH_Recurring, x.GH_RecurrMonth, x.GH_RecurrDay, x.GH_IsWorkingDay, x.GH_Date, x.GH_IsActive, x.Country.RN_Code }))
			{
				yield return new CountryStatesGlbHolidayBizo(factory, groupedHoliday.First());
			}

			foreach (var holiday in glbHolidays.Where(x => x.GH_ParentTableCode == RefCountrySchema.Constants.Prefix))
			{
				yield return new CountryStatesGlbHolidayBizo(factory, holiday);
			}
		}

		public static CountryStatesGlbHolidayBizo[] LoadCountryStates(BusinessObjectFactory factory, ZQuery query)
		{
			return ToCountryStateBizos(factory, factory.Load<GlbHoliday>(query)).ToArray();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
