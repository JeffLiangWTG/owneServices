using System.Linq;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class APHISRoutingCollection : DependentCusAddInfoCollection<APHISRouting, APHISHeader>
	{
		public APHISRoutingCollection(APHISHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.USAPHISRouting)
		{
		}

		public void AddOriginalLocationElementIfRequired()
		{
			if (Master.IsLiveAnimalsCategory)
			{
				if (this.Count == 0 || this.Cast<APHISRouting>().All(x => x.US_Type != RoutingTypeList.Codes.OriginalLocation))
				{
					var newLine = this.AddNew();
					newLine.US_Type = RoutingTypeList.Codes.OriginalLocation;
					var invoiceLine = Master.Parent;
					if (invoiceLine != null)
					{
						newLine.US_Country = invoiceLine.US_UC_NKCountryOfOrigin;
					}
				}
			}
		}
	}
}
