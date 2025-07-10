using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class NPBOCusGuaranteeLineTransactionLookups
	{
		public NPBOCusGuaranteeLineTransactionLookups()
		{
		}

		public CodeDescriptionPairList TransactionTypes
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(PermitTransactionTypeList.Codes.ADJ, PermitTransactionTypeList.Descriptions.ADJ);
				result.AddPair(PermitTransactionTypeList.Codes.OBL, PermitTransactionTypeList.Descriptions.OBL);
				result.AddPair(GuaranteeTransactionTypeList.Codes.OBA, GuaranteeTransactionTypeList.Descriptions.OBA);
				return result;
			}
		}
	}
}
