using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentGoodsDescriptionConfigLookups : ZLookups
	{
		public JobDeclarationDocumentGoodsDescriptionConfigLookups(JobDeclarationDocumentGoodsDescriptionConfig parent) : base(parent)
		{
		}

		protected new JobDeclarationDocumentGoodsDescriptionConfig Parent => (JobDeclarationDocumentGoodsDescriptionConfig)base.Parent;

		public CodeDescriptionPairList FiledsList
		{
			get
			{
				var isImport = Parent.IsImport;
				return Factory.GetCachedValue("Enterprise.Customs.TW.Business.JobDeclarationDocumentGoodsDescriptionConfigLookups.FiledsList" + isImport, () =>
				{
					return isImport ? new ImportDeclarationDocumentFieldList() as CodeDescriptionPairList : new ExportDeclarationDocumentFieldList();
				});
			}
		}
	}
}
