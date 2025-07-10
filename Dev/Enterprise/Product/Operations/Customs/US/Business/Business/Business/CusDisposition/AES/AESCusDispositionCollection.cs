using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AESCusDispositionCollection : CusDispositionCollection
	{
		public AESCusDispositionCollection(ICusDispositionParent master) : base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			var cusDisposition = child as CusDisposition;
			if (cusDisposition != null)
			{
				cusDisposition.CDI_Type = CusDispositionTypeCodeList.Codes.USAESEntryStatus;
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var additionalFilter = new ZQuery();
			additionalFilter.AddToFilter(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USAESEntryStatus);
			result.AddToFilter(additionalFilter);
			return result;
		}
	}
}
