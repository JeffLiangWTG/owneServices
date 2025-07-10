using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitLineTransactionLookups : CusPermitLineTransactionLookups
	{
		public SharedCusPermitLineTransactionLookups(SharedCusPermitLineTransaction parent) : base(parent)
		{
		}

		public new SharedCusPermitLineTransaction Parent => (SharedCusPermitLineTransaction)base.Parent;

		public CodeDescriptionPairList PermitTransactionCategories => Factory.GetCachedValue<PermitTransactionCategoryList>();

		public CodeDescriptionPairList PermitTransactionTypes
		{
			get
			{
				CodeDescriptionPairList result = null;
				var permitHeader = Parent.PermitHeader;
				if (permitHeader != null)
				{
					result = permitHeader.GetCountrySpecificInstruction().GetTransactionTypeList(permitHeader.CPH_Type, permitHeader.CPH_SubType);
				}
				return result;
			}
		}

		public CodeDescriptionPairList PermitTransactionStatuses
		{
			get
			{
				return Factory.GetCachedValue("PermitTransactionStatuses", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(PermitTransactionStatusList.Codes.Pending, PermitTransactionStatusList.Descriptions.Pending);
					result.AddPair("", ResString.GetMultilingualString("22F89B3B-C452-4038-B932-D9845AE64099", "Empty"));
					return result;
				});
			}
		}
	}
}
