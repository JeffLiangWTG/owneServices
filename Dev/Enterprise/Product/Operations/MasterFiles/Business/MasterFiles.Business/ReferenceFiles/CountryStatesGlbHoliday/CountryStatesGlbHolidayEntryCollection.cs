using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class CountryStatesGlbHolidayEntryCollection : NonPersistentBusinessObjectCollection<CountryStatesGlbHolidayEntry>
	{
		readonly CountryStatesGlbHolidayBizo fParentEntry;
		readonly BusinessObjectFactory fFactory;
		public CountryStatesGlbHolidayEntryCollection(CountryStatesGlbHolidayBizo parentEntry, BusinessObjectFactory factory)
		{
			fParentEntry = parentEntry;
			fFactory = factory;
		}

		public ZString CountryCode { get; set; }

		[SuppressMessage("Enterprise.Globalization", "EDI007:CustomizableDataTranslationRule")]
		public override void Load()
		{
			if (fParentEntry != null && fFactory != null)
			{
				var refCountry = fFactory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode);
				if (refCountry != null)
				{
					var query = new ZQuery(GlbHolidaySchema.GH_ParentID, refCountry.States.GetPKs());
					query.AddToFilter(GlbHolidaySchema.GH_ParentTableCode, RefCountryStatesSchema.Constants.Prefix);
					query.AddToFilter(GlbHolidaySchema.GH_Recurring, fParentEntry.GHC_Recurring);
					query.AddToFilter(GlbHolidaySchema.GH_HolidayName, fParentEntry.GHC_HolidayName);
					query.AddToFilter(GlbHolidaySchema.GH_IsWorkingDay, fParentEntry.GHC_IsWorkingDay);
					query.AddToFilter(GlbHolidaySchema.GH_Date, fParentEntry.GHC_Date.IsValid ? fParentEntry.GHC_Date : ZDate.Empty);
					query.AddToFilter(GlbHolidaySchema.GH_RecurrType, GlbHolidayRecurTypeCodeList.Codes.Date);
					var countryStatesHolidays = fFactory.Load<GlbHoliday>(query);
					foreach (RefCountryStates countryState in refCountry.States)
					{
						var holiday = countryStatesHolidays.FirstOrDefault(x => x.GH_ParentID == countryState.PK);
						var entry = new CountryStatesGlbHolidayEntry(fParentEntry, countryState.PK, countryState.RW_Description, holiday != null);
						if (holiday != null)
						{
							entry.HolidayPK = holiday.PK;
						}

						Add(entry);
					}
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
