using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class OrgCustomLabelsLookups : MasterFiles.Business.OrgCustomLabelsLookups
	{
		public OrgCustomLabelsLookups(OrgCustomLabels parent) : base(parent)
		{
		}

		public CodeDescriptionPairList FieldNameList
		{
			get
			{
				var type = Parent.OT_Type;
				return Factory.GetCachedValue("Enterprise.Customs.TW.Business.OrgCustomLabelsLookups.FieldNameList" + type, () =>
				{
					CodeDescriptionPairList result = null;
					switch (type)
					{
						case OrgConstants.CustomLabelType.OverrideExportDoc:
							result = new ExportDeclarationDocumentFieldList();
							break;
						case OrgConstants.CustomLabelType.OverrideImportDoc:
							result = new ImportDeclarationDocumentFieldList();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}

		protected new OrgCustomLabels Parent => (OrgCustomLabels)base.Parent;
	}
}
