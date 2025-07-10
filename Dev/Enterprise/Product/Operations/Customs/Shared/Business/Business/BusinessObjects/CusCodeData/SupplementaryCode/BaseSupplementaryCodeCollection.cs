using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCodeCollection<TSupplementaryCode> : CusCodeDataWithOrderCollection<TSupplementaryCode>
		where TSupplementaryCode : BaseSupplementaryCode
	{
		protected BaseSupplementaryCodeCollection(ZPropertyInfo info, short size, short startOrder = 1)
			: base(info, BaseCusCodeDataTypeList.Codes.SupplementaryCode, size, startOrder)
		{
		}

		public static BaseSupplementaryCodeCollection<TSupplementaryCode> New(ZPropertyInfo info)
		{
			var codeSupporter = info.BizObj as ISupplementaryCodeSupporter;
			var provider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(codeSupporter);
			var supplementaryCodeCollection = new BaseSupplementaryCodeCollection<TSupplementaryCode>(info, provider.NumberOfCodes, provider.CodesStartingOrder);
			supplementaryCodeCollection.Load();
			return supplementaryCodeCollection;
		}
	}
}
