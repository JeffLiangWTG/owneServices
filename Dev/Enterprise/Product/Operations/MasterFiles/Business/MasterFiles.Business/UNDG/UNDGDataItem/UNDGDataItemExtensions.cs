using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class UNDGDataItemExtensions
	{
		public static ZString GetUnnoPrefix(this UNDGDataItem dataItem) => UNDGPrefixHelper.GetUnnoPrefix(dataItem.Substance);
	}
}
