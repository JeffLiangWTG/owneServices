using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class CPSCRuleCollection : DependentCusAddInfoCollection<CPSCRule, BusinessObject>
	{
		public CPSCRuleCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USCPSCRule)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var header = Master as CPSCHeader;
				return header == null || !header.IsREF;
			}
		}
	}
}
