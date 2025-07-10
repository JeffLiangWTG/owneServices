using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using IDirectionIsDomesticFreight = Enterprise.Integration.Forwarding.IDirectionIsDomesticFreight;

namespace Enterprise.Freight.Forwarding.Business.HelperClasses
{
	public static class DirectionsContext
	{
		public const string Import = "IMP";
		public const string Export = "EXP";
		public const string Domestic = "DOM";
		public const string CrossTrade = "CST";
	}

	public static class DirectionsHelper<T> where T : IDirectionIsDomesticFreight, IImportExport
	{
		public static ZString FromBusinessObject(T obj)
		{
			ZString direction = "";
			if (obj.IsDomesticFreight)
			{
				direction = DirectionsContext.Domestic;
			}
			else if (obj.IsImport())
			{
				direction = DirectionsContext.Import;
			}
			else if (obj.IsExport())
			{
				direction = DirectionsContext.Export;
			}
			else if (obj.IsCrossTrade())
			{
				direction = DirectionsContext.CrossTrade;
			}
			return direction;
		}
	}
}
