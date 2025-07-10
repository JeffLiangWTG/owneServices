using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ExportJobDeclarationLookups : JobDeclarationLookups
	{
		public ExportJobDeclarationLookups(JobDeclaration parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList EntryStyleListCore
		{
			get
			{
				return Factory.GetCachedValue("TR_EntryStyleList_Export", () =>
				{
					var pairList = new CodeDescriptionPairList();
					pairList.AddPair(Business.EntryStyleList.Codes.EXPEU, Business.EntryStyleList.Descriptions.EXPEU);
					pairList.AddPair(Business.EntryStyleList.Codes.EXPEX, Business.EntryStyleList.Descriptions.EXPEX);
					pairList.AddPair(Business.EntryStyleList.Codes.EXPTR, Business.EntryStyleList.Descriptions.EXPTR);
					return pairList;
				});
			}
		}
	}
}
