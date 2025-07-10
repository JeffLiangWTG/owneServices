using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefPostCode)]
	public class RefPostCodeCollection : ActiveBusinessObjectCollection<RefPostCode>, ICityTownPostcodeUserInteraction
	{
		public RefPostCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefPostCodeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public RefPostCodeCollection(BusinessObjectFactory factory, ZQuery filter, string countryCode, string cityTown, string state)
			: base(factory, filter)
		{
			CountryCode = countryCode;
			CityTown = cityTown;
			State = state;
		}

		readonly string CityTown;

		readonly string State;

		readonly string CountryCode;

		protected override IFindBoxListProvider FindBoxListProvider => string.IsNullOrEmpty(CountryCode) ? base.FindBoxListProvider : new RefPostcodeListProvider(this, CountryCode, CityTown, State);

		public RefPostCodeCollection(RefCityTown cityTown)
			: base(cityTown, typeof(RefCityPCodePivot))
		{
			this.parentCityTown = cityTown;
		}

		readonly RefCityTown parentCityTown;

		protected override void SetDefaultsForNewElementCore(RefPostCode newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (parentCityTown != null)
			{
				newElement.RK_RN_NKCountry = parentCityTown.R9_RN_NKCountry;
			}
		}

		#region ICityTownPostcodeUserInteraction Implementation

		public ZGuid OnSelectionNeeded(ZGuid defaultPK) => defaultPK;

		public void OnBusy(string message) => Busy?.Invoke(this, new BusyEventArgs(message));

		public void OnCompleted() => Completed?.Invoke(this, new EventArgs());

		public event EventHandler<BusyEventArgs> Busy;

		public event EventHandler Completed;

		public event EventHandler<SelectionNeededEventArgs> SelectionNeeded { add { } remove { } }

		public bool AreEventsSubscribed
		{
			get => areEventsSubscribed;
			set => areEventsSubscribed = value;
		}

#if DEBUG
		[SuppressCollectionStateTest]
#endif
		bool areEventsSubscribed;

		#endregion
	}
}
