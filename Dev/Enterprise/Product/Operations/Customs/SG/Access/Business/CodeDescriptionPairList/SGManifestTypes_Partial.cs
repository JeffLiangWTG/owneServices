using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.SG.Access.Business
{
	public partial class SGManifestTypes
	{
		public IManifestType MGI => mgi ?? (mgi = new ManifestType(
			Codes.MGI,
			Descriptions.MGI,
			new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road },
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Pack
		));
		IManifestType mgi;

		public IManifestType MGE => mge ?? (mge = new ManifestType(
			Codes.MGE,
			Descriptions.MGE,
			new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road },
			ApplicationCodeTypeList.Codes.Consolidator,
			MessageLevel.Pack
		));
		IManifestType mge;

		public IReadOnlyList<IManifestType> All => new[] { MGI, MGE };
	}
}
