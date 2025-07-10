using System;
using CargoWise.Common;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PickedPackTypeInfo : DataObjectInfo
	{
		public PickedPackTypeInfo()
		{
		}

		public PickedPackTypeInfo(string packType, decimal unitsPicked, string palletID, WhsReleaseCapturedInfo[] releaseCapturedInfo)
		{
			PackType = Argument.NotNull(packType, nameof(packType));
			UnitsPicked = unitsPicked;
			PalletID = palletID;
			ReleaseCapturedInfos = releaseCapturedInfo;
		}

		public string PackType { get; set; }
		public decimal UnitsPicked { get; set; }
		public string PalletID { get; set; }
		public WhsReleaseCapturedInfo[] ReleaseCapturedInfos { get; set; }
	}
}
