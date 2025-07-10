using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyCountryCollection : NonPersistentBusinessObjectCollection<AgencyCountry>
	{
		public AgencyCountryCollection(JobVoyage voyage)
		{
			this.voyage = voyage;
			this.useGlobalAllocations = FreightConfigurationRegistry.Instance.UseGlobalAllocations.Value;
			this.dirty = true;
			((IBindingList)voyage.Origins).ListChanged += new ListChangedEventHandler(AgencyCountryCollection_ListChanged);
		}

		public void Refresh()
		{
			if (dirty)
			{
				dirty = false;

				using (SuspendListChanged())
				{
					RefreshCore();
				}
			}
		}

		public void PerformPreSave()
		{
			Mutex.Lock();

			if (Mutex.HasLock)
			{
				RefreshAllUsageData();
			}
		}

		public void PerformPostSave()
		{
			if (Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		public void RefreshAllUsageData()
		{
			foreach (AgencyCountry country in this)
			{
				country.RefreshAllUsageData();
			}
		}

		public JobVoyage Voyage
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return voyage; }
		}

		public AgencyAllocationMutex Mutex
		{
			get { return mutex ?? (mutex = new AgencyAllocationMutex(voyage)); }
		}
		AgencyAllocationMutex mutex;

		public bool UseGlobalAllocations
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return useGlobalAllocations; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AgencyCountry(voyage, GlbCompany.CurrentCompany.Country);
		}

		static IDictionary<string, RefCountry> GetGlobalCountries(JobVoyage voyage)
		{
			List<string> countryCodes = new List<string>();

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				countryCodes.Add(origin.JA_RL_NKPortOfLoading.Left(2));
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(RefCountrySchema.RN_Code, countryCodes);

			IDictionary<string, RefCountry> result = new SortedDictionary<string, RefCountry>();

			foreach (RefCountry country in voyage.Factory.Load<RefCountry>(filter))
			{
				result.Add(country.RN_Code, country);
			}

			return result;
		}

		static IDictionary<string, RefCountry> GetLocalCountry()
		{
			IDictionary<string, RefCountry> countriesToAdd;
			RefCountry country = GlbCompany.CurrentCompany.Country;
			countriesToAdd = new Dictionary<string, RefCountry>();

			countriesToAdd.Add(country.RN_Code, country);
			return countriesToAdd;
		}

		void RefreshCore()
		{
			IDictionary<string, RefCountry> countriesToAdd = useGlobalAllocations ? GetGlobalCountries(voyage) : GetLocalCountry();

			List<AgencyCountry> countriesToRemove = new List<AgencyCountry>();

			foreach (AgencyCountry country in this)
			{
				if (!countriesToAdd.Remove(country.CountryCode))
				{
					countriesToRemove.Add(country);
				}
			}

			foreach (AgencyCountry country in countriesToRemove)
			{
				Remove(country);
			}

			foreach (KeyValuePair<string, RefCountry> pair in countriesToAdd)
			{
				Add(new AgencyCountry(voyage, pair.Value));
			}
		}

		void AgencyCountryCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			dirty = true;
		}

		bool dirty;
		readonly JobVoyage voyage;
		readonly bool useGlobalAllocations;
	}
}
