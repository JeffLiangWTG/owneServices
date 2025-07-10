namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(ASYCUDA.Business.AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEstDateAtFirstArrival();
		}

		public void ValidateEstDateAtFirstArrival()
		{
			ValidateCalculatedProperty(Parent.EstDateAtFirstArrivalInfo);
		}

		protected void CheckEstDateAtFirstArrival()
		{
			if (Parent.PortOfFirstArrival != null && Parent.EstDateAtFirstArrival.IsEmpty)
			{
				Parent.EstDateAtFirstArrivalInfo.AddMessageError(ValidationConstants.EstDateAtFirstArrivalRequired);
			}
		}

		protected override void CheckAMA_VoyageCore()
		{
			if (Parent.IsAir)
			{
				var errorString = AIMFlightHelper.CheckCombinedCarrierFlight(Parent.AMA_Voyage, Parent.AMA_CarrierCode);
				if (!errorString.IsEmpty)
				{
					Parent.AMA_VoyageInfo.AddMessageError(errorString);
				}
			}
			else
			{
				base.CheckAMA_VoyageCore();
			}
		}

		protected override void CheckAMA_OA_DeconsolidateAddress()
		{
			base.CheckAMA_OA_DeconsolidateAddress();
			if (Parent.AMA_OA_DeconsolidateAddress.IsValid && Parent is Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader header && header.AirAMSOriginatorCode.IsEmpty)
			{
				Parent.AMA_OA_DeconsolidateAddressInfo.AddMessageError(ValidationConstants.AirAMSOriginatorCodeRequired);
			}
		}
	}
}
