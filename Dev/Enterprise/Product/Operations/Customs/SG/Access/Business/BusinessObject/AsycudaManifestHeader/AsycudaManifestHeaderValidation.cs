namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_TransportMode()
		{
			base.CheckAMA_TransportMode();
			if (!Parent.IsAir && !Parent.IsRoad)
			{
				Parent.AMA_TransportModeInfo.AddMessageError(ValidationConstants.SGManifestValidOnlyForAirAndRoad);
			}
		}
	}
}
