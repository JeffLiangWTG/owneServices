using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCityTown)]
	public class RefCityTownCollection : ActiveBusinessObjectCollection<RefCityTown>, ICityTownPostcodeUserInteraction
	{
		public RefCityTownCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefCityTownCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public RefCityTownCollection(BusinessObjectFactory factory, ZQuery filter, string countryCode, string postcode)
			: base(factory, filter)
		{
			CountryCode = countryCode;
			Postcode = postcode;
		}

		readonly string CountryCode;

		readonly string Postcode;

		protected override IFindBoxListProvider FindBoxListProvider => string.IsNullOrEmpty(CountryCode) ? base.FindBoxListProvider : new RefCityTownListProvider(this, CountryCode, Postcode);

		public RefCityTownCollection(RefPostCode postCode)
			: base(postCode, typeof(RefCityPCodePivot))
		{
			this.parentPostCode = postCode;
		}

		readonly RefPostCode parentPostCode;

		protected override void SetDefaultsForNewElementCore(RefCityTown newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (parentPostCode != null)
			{
				newElement.R9_RN_NKCountry = parentPostCode.RK_RN_NKCountry;
			}
		}

		#region ICityTownPostcodeUserInteraction Implementation

		public ZGuid OnSelectionNeeded(ZGuid defaultPK)
		{
			var args = new SelectionNeededEventArgs(defaultPK);
			SelectionNeeded?.Invoke(this, args);
			return args.SelectedPK;
		}

		public void OnBusy(string message) => Busy?.Invoke(this, new BusyEventArgs(message));

		public void OnCompleted() => Completed?.Invoke(this, new EventArgs());

		public event EventHandler<BusyEventArgs> Busy;

		public event EventHandler Completed;

		public event EventHandler<SelectionNeededEventArgs> SelectionNeeded;

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
