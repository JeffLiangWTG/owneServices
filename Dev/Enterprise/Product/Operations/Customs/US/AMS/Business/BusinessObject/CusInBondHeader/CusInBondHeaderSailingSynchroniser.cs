using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondHeaderSailingSynchroniser : BusinessObjectSynchroniser
	{
		public CusInBondHeaderSailingSynchroniser(CusInBondHeader destination)
			: base(destination, destination.Sailing)
		{
		}

		protected new CusInBondHeader Destination
		{
			get { return (CusInBondHeader)base.Destination; }
		}

		protected new JobSailing Source
		{
			get { return (JobSailing)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BH_RL_NKPortUnladingInfo, Source.JX_JB_RL_NKPortOfDischargeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.BH_ETAInfo, Source.JX_JB_E_ARVInfo));
			var voyage = Source.Voyage;
			if (voyage != null)
			{
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_ImportConveyanceNameInfo, voyage.JV_RV_NKVesselInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BH_VoyageNumberInfo, () => voyage.JV_VoyageFlight.Left(Destination.VoyageNumberMaxLength), () => new ZPropertyInfo[] { voyage.JV_VoyageFlightInfo }));
			}
		}
	}
}
