using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class PackProvider : ILadingLines
	{
		public PackProvider(AsycudaPack asycudaPack, ZBool isManifestContainer)
		{
			this.pack = asycudaPack;
			this.isManifestContainer = isManifestContainer;
		}

		readonly AsycudaPack pack;
		readonly ZBool isManifestContainer;

		public ZDecimal GrossWeight => AsycudaHelper.ConvertWeightQty(pack.APA_Weight, pack.APA_WeightUQ, Core.Constants.Weight.Kilograms);
		public ZInt PackQuantity => pack.APA_PackQty;
		public ZString PackType => pack.APA_PackUQ;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String comparison")]
		public ZString ContainerType
		{
			get
			{
				var returnValue = ZString.Empty;
				var containerStatus = pack.Container?.Relation ?? ZString.Empty;
				if (containerStatus != ZString.Empty && isManifestContainer)
				{
					returnValue = containerStatus == "Local" ? TurkishConstants.ContainerLocal : TurkishConstants.ContainerForeign;
				}
				return returnValue;
			}
		}
		public ZString ContainerNumber => pack.Container?.ACN_ContainerNumber ?? pack.APA_MarksAndNumbers;
		public ZString SealNumber => pack.Container?.ACN_Seal1 ?? ZString.Empty;
		public ZDecimal NetWeight => pack.GetAllPackedItems().Cast<AsycudaPackedItem>().Sum(x => Business.AsycudaHelper.ConvertWeightQty(x.API_NetWeight, x.API_NetWeightUQ, Core.Constants.Weight.Kilograms));
		public ZString WeightUQ => TurkishConstants.WeightUnitType;
		public ZInt LineNo => pack.APA_LineNo;
		public ZString ContainerLoadStatus
		{
			get
			{
				var returnValue = ZString.Empty;
				var containerStatus = pack.Container?.ACN_EmptyFullIndicator ?? ZString.Empty;
				if (containerStatus != ZString.Empty && isManifestContainer)
				{
					returnValue = containerStatus == "MT" ? TurkishConstants.ContainerEmpty : TurkishConstants.ContainerFull;
				}
				return returnValue;
			}
		}
		public IEnumerable<IGoodsInformation> GoodsInformation
		{
			get
			{
				var items = pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => (AsycudaPackedItem)x.PackedItem);
				ZInt itemSeq = 0;
				foreach (var item in items)
				{
					itemSeq++;
					yield return new ItemProvider(item, itemSeq);
				}
			}
		}
	}
}
