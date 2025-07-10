using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentGoodsDescriptionConfigValidation : AutoJobDeclarationDocumentGoodsDescriptionConfigValidation
	{
		public JobDeclarationDocumentGoodsDescriptionConfigValidation(AutoJobDeclarationDocumentGoodsDescriptionConfig parent) : base(parent)
		{
		}

		public new JobDeclarationDocumentGoodsDescriptionConfig Parent => (JobDeclarationDocumentGoodsDescriptionConfig)base.Parent;

		protected override void CheckField()
		{
			base.CheckField();
			var targetInfo = Parent.FieldInfo;
			ListValidation.ErrorIfInvalidCode(targetInfo);
			MandatoryValidation.CheckEntered(targetInfo);
		}
	}
}
