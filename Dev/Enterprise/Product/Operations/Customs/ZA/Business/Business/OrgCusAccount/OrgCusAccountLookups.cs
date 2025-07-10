using System.Linq;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgCusAccountLookups : MasterFiles.Business.OrgCusAccountLookups
	{
		public OrgCusAccountLookups(MasterFiles.Business.OrgCusAccount cusAccount) : base(cusAccount)
		{
		}

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue("ZA OrgCusAccount.ZA_CodeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(OrgCusAccountProvider.FANCode, OrgCusAccountProvider.FANDesc);
			return result;
		});

		public override CodeDescriptionPairList AccountList
		{
			get
			{
				return Factory.GetCachedValue("ZA.OrgCusAccount AccountList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(((IZACustomsRegistry)ZACustomsRegistry.Instance).FANList.Cast<CodeDescriptionPair>().Distinct().ToList());
					return result;
				});
			}
		}
	}
}
