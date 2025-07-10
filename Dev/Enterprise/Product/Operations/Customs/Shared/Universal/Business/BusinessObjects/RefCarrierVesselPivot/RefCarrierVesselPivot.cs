using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCarrierVesselPivot : AutoRefCarrierVesselPivot
	{
		public RefCarrierVesselPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{ }

		#region Related Business Objects

		public RefCarrierCode Carrier
		{
			get { return Factory.Load<RefCarrierCode>(ZZQ_ZZ4); }
		}

		public RefVesselZZ Vessel  // Not RefVessel, but RefVesselZZ.  Genius.
		{
			get { return Factory.Load<RefVesselZZ>(ZZQ_ZZO); }
		}

		#endregion

		#region Properties

		[RelatedBusinessObject("Carrier")]
		public override ZGuid ZZQ_ZZ4
		{
			get { return base.ZZQ_ZZ4; }
			set
			{
				if (value != base.ZZQ_ZZ4)
				{
					base.ZZQ_ZZ4 = value;
				}
			}
		}

		[RelatedBusinessObject("Vessel")]
		public override ZGuid ZZQ_ZZO
		{
			get { return base.ZZQ_ZZO; }
			set
			{
				if (value != base.ZZQ_ZZO)
				{
					base.ZZQ_ZZO = value;
				}
			}
		}

		#endregion

	}
}
