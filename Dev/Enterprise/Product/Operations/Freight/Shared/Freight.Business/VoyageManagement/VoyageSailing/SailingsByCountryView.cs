using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class SailingsByCountryView : BusinessObjectCollectionView<JobSailing>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SailingsByCountryView")]
		readonly VoyageCountry fCountry;
		readonly string CountryCode;

		public SailingsByCountryView(VoyageCountry country) : base(country.Voyage.Sailings)
		{
			fCountry = country;
			CountryCode = country.Country.RN_Code;

			base.RebuildOnConstruction();
		}

		protected sealed override void RebuildOnConstruction()
		{
			// do nothing, instead call base.RebuildOnConstruction from our constructor so that we can setup our enviroment first.
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(JobSailing);
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			JobSailing sailing = (JobSailing)element;
			return sailing.Origin != null && sailing.Origin.JA_RL_NKPortOfLoading.StartsWith(CountryCode);
		}
	}
}
