using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class ServiceTypes : CodeDescriptionPairList
	{
		public ServiceTypes() : base()
		{
			AddPair(CommodityInspection, ResString.GetMultilingualString("5A92B69F-1DF9-4F30-8D99-9D8FC8C77133", "Commodity Inspection"));
		}

		#region Constants

		public const string CommodityInspection = "ICI";

		#endregion
	}
}
