using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public partial class ACEManifestTypes
	{
		public IManifestType IAM => iam ?? (iam = new ManifestType(
			Codes.IAM,
			Descriptions.IAM,
			new[] { Core.Constants.TransportModes.Air },
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Bill,
			ShipmentTypeList.Import23Only()
		));
		IManifestType iam;

		public IReadOnlyList<IManifestType> All => new[] { IAM };
	}
}
