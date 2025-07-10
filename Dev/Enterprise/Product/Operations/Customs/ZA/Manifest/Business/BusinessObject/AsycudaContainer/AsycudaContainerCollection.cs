using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaContainerCollection : ManifestBase.AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>
	{
		public AsycudaContainerCollection(AsycudaManifestHeader master) : base(master)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var bizO = (AsycudaContainer)bizOAdded;
			if (bizO != null && bizO.LandedPurpose.IsEmpty)
			{
				switch (Master.AMA_Nature)
				{
					case ShipmentTypeList.Codes.Export22:
						bizO.LandedPurpose = LandedPurposeList.Codes.Export;
						break;
					case ShipmentTypeList.Codes.Import23:
						bizO.LandedPurpose = LandedPurposeList.Codes.Import;
						break;
					case ShipmentTypeList.Codes.Transhipment28:
						bizO.LandedPurpose = LandedPurposeList.Codes.Transhipment;
						break;
					case ShipmentTypeList.Codes.Transit24:
						bizO.LandedPurpose = LandedPurposeList.Codes.ContinentalTransit;
						break;
				}
			}
			base.OnAdded(bizO);
		}
	}
}
