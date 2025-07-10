using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class OrgCustomLabelsValidation : MasterFiles.Business.OrgCustomLabelsValidation
	{
		public OrgCustomLabelsValidation(OrgCustomLabels parent) : base(parent)
		{
		}

		protected new OrgCustomLabels Parent => (OrgCustomLabels)base.Parent;

		protected override void CheckOT_Caption()
		{
		}

		protected override void CheckOT_FieldName()
		{
			var targetInfo = Parent.OT_FieldNameInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo, Parent.Lookups.FieldNameList);
			var customDocumentsCollection = Parent.customDocumentsCollection;
			if (customDocumentsCollection != null && !customDocumentsCollection.Cast<OrgCustomLabels>().Any(c => c.OT_FieldName == ExportDeclarationDocumentFieldList.Codes.GoodsDescription))
			{
				targetInfo.AddError(Res.GetString("409D851F-2E1F-4F64-B992-A74D1A2F0B76", "Goods Description is mandatory for printing Customs Declaration."));
			}
		}
	}
}
