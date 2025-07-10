using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class VoyageOriginByCountryView : BusinessObjectCollectionView<VoyageOrigin>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in VoyageOriginByCountryView")]
		readonly VoyageCountry fCountry;
		readonly string CountryCode;

		public VoyageOriginByCountryView(VoyageCountry country) : base(country.Voyage.Origins)
		{
			fCountry = country;
			CountryCode = country.Country.RN_Code;

			base.RebuildOnConstruction();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(VoyageOrigin);
		}

		protected sealed override void RebuildOnConstruction()
		{
			// do nothing, instead call base.RebuildOnConstruction from our constructor so that we can setup our enviroment first.
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			VoyageOrigin origin = (VoyageOrigin)child;
			origin.JA_RL_NKPortOfLoading = CountryCode;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			VoyageOrigin origin = (VoyageOrigin)element;
			return origin.JA_RL_NKPortOfLoading.StartsWith(CountryCode);
		}
	}
}
