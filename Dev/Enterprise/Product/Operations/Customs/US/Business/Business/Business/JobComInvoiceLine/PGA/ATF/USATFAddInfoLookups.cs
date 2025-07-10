//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSATFAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSATFAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USATFAddInfoLookups : AutoUSATFAddInfoLookups
	{
		public USATFAddInfoLookups(AutoUSATFAddInfo parent) : base(parent)
		{
		}

		new ATFAddInfo Parent
		{
			get { return (ATFAddInfo)base.Parent; }
		}

		public CodeDescriptionPairList CategoryCodeList
		{
			get
			{
				var isExport = Parent != null && Parent.Parent != null && Parent.Parent.IsExport;
				return Factory.GetCachedValue("ATFCategoryCodeList" + (isExport ? "Y" : "N"), delegate
				{
					if (isExport)
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ATFCategoryCodeList.Codes.AW, ATFCategoryCodeList.Descriptions.AW);
						result.AddPair(ATFCategoryCodeList.Codes.DD, ATFCategoryCodeList.Descriptions.DD);
						result.AddPair(ATFCategoryCodeList.Codes.MG, ATFCategoryCodeList.Descriptions.MG);
						result.AddPair(ATFCategoryCodeList.Codes.SI, ATFCategoryCodeList.Descriptions.SI);
						result.AddPair(ATFCategoryCodeList.Codes.SR, ATFCategoryCodeList.Descriptions.SR);
						result.AddPair(ATFCategoryCodeList.Codes.SS, ATFCategoryCodeList.Descriptions.SS);
						return result;
					}
					else
					{
						return new ATFCategoryCodeList();
					}
				});
			}
		}

		public ExemptionCodesCodeList ExemptionCodesList
		{
			get { return Factory.GetCachedValue<ExemptionCodesCodeList>(); }
		}

		public MunitionsCategoryList MunitionsCategoryList
		{
			get { return Factory.GetCachedValue<MunitionsCategoryList>(); }
		}
	}
}
