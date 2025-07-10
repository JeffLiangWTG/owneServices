using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class ADDCVDNonReimbursementList
	{
		public static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("US_ADDCVDNonReimbursementList", // 'US_ADDCVDNonReimbursementList' is not a database field
			delegate
			{
				return new ADDCVDNonReimbursementList();
			});
		}
	}
}
