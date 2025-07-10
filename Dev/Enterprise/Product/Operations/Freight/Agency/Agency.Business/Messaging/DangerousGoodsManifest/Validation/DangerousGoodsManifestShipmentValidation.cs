using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DangerousGoodsManifestShipmentValidation : JobShipmentValidation
	{
		public DangerousGoodsManifestShipmentValidation(AutoJobShipment parent) : base(parent)
		{
		}

		new BillOfLading Parent => (BillOfLading)base.Parent;

		#region ValidateVoyageVesselForBinding

		public void ValidateVoyageVesselForBinding()
		{
			ValidateCalculatedProperty(Parent.VoyageVesselForBindingInfo);
		}

		protected virtual void CheckVoyageVesselForBinding()
		{
			var vessel = RefVessel.LookupVesselByFK(Parent.VoyageVesselForBinding, Parent.Factory);

			if (vessel == null || vessel.RV_LloydsNumber.IsEmpty)
			{
				Parent.VoyageVesselForBindingInfo.AddMessageError(Res.GetString("A69B0867-D9DC-46B4-AB17-1CAE1E3D0E62", "Vessel Lloyds/IMO Number of linked Sailing Schedule is missing."));
			}
		}

		#endregion
	}
}
