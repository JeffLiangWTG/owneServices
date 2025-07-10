using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestRatingAdapter : LinehaulAndRunSheetRatingAdapter<DtbLinehaulManifest>
	{
		public DtbLinehaulManifestRatingAdapter(DtbLinehaulManifest manifest)
			: base(manifest)
		{
		}

		public override Creditors Creditors
		{
			get { return Parent.OriginDepot != null ? Creditors.New(OrgWithSource.NewFrom<OrgHeader>(Parent.TransportCompanyPKInfo)) : Creditors.New(); }
		}

		public override OrgHeader Carrier
		{
			get { return Parent.TransportCompany; }
		}

		protected override ZString CarrierServiceLevel
		{
			get { return Parent.LHM_PL_NKCarrierServiceLevel; }
		}

		public override ILocation Origin
		{
			get { return Parent.OriginDepot != null ? Parent.OriginDepot.EffectiveRelatedPortCode : null; }
		}

		public override ILocation Destination
		{
			get { return Parent.DestinationDepot != null ? Parent.DestinationDepot.EffectiveRelatedPortCode : null; }
		}

		public override IDocAddress PickupAddress
		{
			get { return Parent.OriginDepot; }
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.DestinationDepot; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new DtbLinehaulManifestJobDatesProvider(Parent); }
		}

		protected override RefEquipment Vehicle
		{
			get { return null; }
		}

		protected override IEnumerable<PkgPackage> Packages
		{
			get { return Parent.Packages; }
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var measures = (RateableMeasureSet)base.RateableMeasures;

				var timeInfo = Parent.EndDateTimeUTC.IsValid && Parent.LHM_StartDateTimeUtc.IsValid ? new TimeInfo(Parent.EndDateTimeUTC - Parent.LHM_StartDateTimeUtc) : new TimeInfo(TimeSpan.Zero);
				measures.Time = timeInfo;

				return measures;
			}
		}

		internal class DtbLinehaulManifestJobDatesProvider : JobDatesProvider<DtbLinehaulManifest>
		{
			public DtbLinehaulManifestJobDatesProvider(DtbLinehaulManifest manifest)
				: base(manifest)
			{
			}

			protected override ZDateTime GetDepartureDateCore()
			{
				return Parent.LHM_StartDateTimeUtc;
			}

			protected override ZDateTime GetArrivalDateCore()
			{
				return Parent.EndDateTimeUTC;
			}
		}
	}
}
