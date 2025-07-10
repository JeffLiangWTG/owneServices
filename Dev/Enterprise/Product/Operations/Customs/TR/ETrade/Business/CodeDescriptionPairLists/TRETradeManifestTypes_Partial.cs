using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public partial class TRETradeManifestTypes
	{
		public IManifestType ETRADE => eTrade ?? (eTrade = new ManifestType(
			Codes.TRETrade,
			Descriptions.TRETrade,
			new[] { TransportModes.Air, TransportModes.Sea, TransportCodes.Road },
			new[] { ApplicationCodeTypeList.Codes.TRETrade },
			MessageLevel.Manifest
		));

		IManifestType eTrade;

		public IReadOnlyList<IManifestType> All => new[] { ETRADE };
	}
}
