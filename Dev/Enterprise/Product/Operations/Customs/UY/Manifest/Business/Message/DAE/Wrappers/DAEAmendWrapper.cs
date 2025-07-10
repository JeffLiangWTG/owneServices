using System.Collections.Generic;
using CargoWise.Customs.UY.MessageContracts;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class DAEAmendWrapper : DAEWrapper, IDaeDeclaration
	{
		public DAEAmendWrapper(AsycudaManifestHeader header, AsycudaBill[] bills) : base(header, bills)
		{
		}

		IReadOnlyCollection<IManifest> IDaeDeclaration.Manifests => new List<IManifest> { new DAEAmendManifestWrapper(Header, Bills) };
	}
}
