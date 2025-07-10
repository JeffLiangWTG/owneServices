using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ImportJobDeclarationLookups : JobDeclarationLookups
	{
		public ImportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList EntryStyleListCore
		{
			get
			{
				return Factory.GetCachedValue("TR_EntryStyleList_Import", () =>
				{
					var pairList = new CodeDescriptionPairList();
					pairList.AddPair(Business.EntryStyleList.Codes.IMPEU, Business.EntryStyleList.Descriptions.IMPEU);
					pairList.AddPair(Business.EntryStyleList.Codes.IMPIM, Business.EntryStyleList.Descriptions.IMPIM);
					pairList.AddPair(Business.EntryStyleList.Codes.IMPAN, Business.EntryStyleList.Descriptions.IMPAN);
					return pairList;
				});
			}
		}
	}
}
