using CargoWise.Types;

namespace Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier
{
	partial class CutFlowersAndGreeneryTypeList
	{
		public static bool RequiresBouquetGroupingNumber(ZString code)
		{
			return code == Codes.AlstroemeriaBouquet || code == Codes.CarnationsBouquet ||
				code == Codes.LilyBouquet || code == Codes.MiniCarnationsBouquet ||
				code == Codes.MixedBouquet || code == Codes.PomponBouquet ||
				code == Codes.RoseBouquet || code == Codes.TropicalFlowerBouquet;
		}
	}
}
