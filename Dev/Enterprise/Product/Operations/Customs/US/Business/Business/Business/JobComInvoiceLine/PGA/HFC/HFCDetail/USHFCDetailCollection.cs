using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class USHFCDetailCollection : DependentCusAddInfoCollection<USHFCDetail, BusinessObject>
	{
		public USHFCDetailCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USHFCDetail)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (Master is USHFCHeader header && !header.US_ASHRAENumber.IsEmpty)
			{
				header.US_ASHRAENumber = ZString.Empty;
			}
		}
	}
}
