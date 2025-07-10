using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DummyLinehaulAndRunSheetRatingAdapter : LinehaulAndRunSheetRatingAdapter<DummyBusinessObject>
	{
		public DummyLinehaulAndRunSheetRatingAdapter(DummyBusinessObject parent) : base(parent)
		{
		}

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var measures = (RateableMeasureSet)base.RateableMeasures;
				measures.Time = new TimeInfo(TimeSpan.FromMinutes(13));
				return measures;
			}
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { throw new NotImplementedException(); }
		}

		public override ILocation Origin
		{
			get { return null; }
		}

		public override Creditors Creditors
		{
			get { return CreditorsToReturn; }
		}

		public override OrgHeader Carrier
		{
			get { return CarrierToReturn; }
		}

		protected override ZString CarrierServiceLevel
		{
			get { throw new NotImplementedException(); }
		}
		public OrgHeader CarrierToReturn { get; set; }

		public Creditors CreditorsToReturn
		{
			get { return Carrier != null ? Creditors.New(new[] { OrgWithSource.New(Carrier, new List<string>() { "Provider" }) }) : Creditors.New(); }
		}

		protected override RefEquipment Vehicle
		{
			get { return EquipmentToReturn; }
		}

		public RefEquipment EquipmentToReturn { get; set; }

		protected override IEnumerable<PkgPackage> Packages
		{
			get { return PackagesToReturn; }
		}

		public IEnumerable<PkgPackage> PackagesToReturn { get; set; }
	}
}
