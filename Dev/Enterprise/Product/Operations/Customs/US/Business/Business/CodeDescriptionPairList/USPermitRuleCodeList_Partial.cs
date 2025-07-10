using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class USPermitRuleCodeList
	{
		public static class ShortDescriptions
		{
			public static MultilingualString COO { get { return ResString.GetMultilingualString("PermitRuleCodeList|COO|Short", "Country"); } }
			public static MultilingualString ZST { get { return ResString.GetMultilingualString("PermitRuleCodeList|ZST|Short", "Status"); } }
		}

		public override string GetShortDescriptionFromCode(ZString code)
		{
			var propertyInfo = typeof(ShortDescriptions).GetProperty(code);
			if (propertyInfo != null)
			{
				return propertyInfo.GetValue(null).ToString();
			}
			else
			{
				return base.GetShortDescriptionFromCode(code);
			}
		}
	}
}
