using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public static class ExtensionMethods
	{
		public static CodeDescriptionPairList GetChargeTypeList(this IncoTermAndCustomsChargeFactory factory, ChargeParentTypes parentTypes)
		{
			var result = new CodeDescriptionPairList();
			var incoTermAndChargeFactory = factory;
			if (incoTermAndChargeFactory != null)
			{
				foreach (var charge in incoTermAndChargeFactory.GetChargeList(parentTypes))
				{
					result.AddPairIfNotExist(charge.Code, charge.Description);
				}
			}

			return result;
		}
	}
}
