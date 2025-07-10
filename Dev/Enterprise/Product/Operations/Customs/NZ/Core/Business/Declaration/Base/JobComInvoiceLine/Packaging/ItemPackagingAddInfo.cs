using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.NZItemPackaging)]
	public class ItemPackagingAddInfo : AutoItemPackagingAddInfo
	{
		public new class Schema : AutoItemPackagingAddInfo.Schema
		{
		}

		public ItemPackagingAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new JobComInvoiceLine Parent { get; internal set; }

		public AutoItemPackaging ItemPackaging
		{
			get;
			set;
		}
	}
}
