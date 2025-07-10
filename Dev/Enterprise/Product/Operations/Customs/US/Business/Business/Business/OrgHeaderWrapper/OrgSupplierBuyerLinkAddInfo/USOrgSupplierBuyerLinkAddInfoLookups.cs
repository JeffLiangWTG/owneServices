using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USOrgSupplierBuyerLinkAddInfoLookups : ZLookups
	{
		public USOrgSupplierBuyerLinkAddInfoLookups(USOrgSupplierBuyerLinkAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("YesNoList",
				delegate
				{
					var result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public ReconIssueCodeList OtherReconIssueList
		{
			get { return OtherReconIssueListCreator.CreateOtherReconIssueList(Factory); }
		}

		public UltimateConsigneeTypeList UltConsigneeTypeList
		{
			get { return Factory.GetCachedValue<UltimateConsigneeTypeList>(); }
		}

		public CodeDescriptionPairList EntryTypes
		{
			get
			{
				return Factory.GetCachedValue("USOrgSupplierBuyerLinkEntryTypes", () =>
				{
					return EntryTypeList.GetACEList();
				});
			}
		}
	}
}
