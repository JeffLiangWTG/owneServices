using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentAddressConfigValidation : AutoJobDeclarationDocumentAddressConfigValidation
	{
		public JobDeclarationDocumentAddressConfigValidation(AutoJobDeclarationDocumentAddressConfig parent) : base(parent)
		{
		}

		public new JobDeclarationDocumentAddressConfig Parent => (JobDeclarationDocumentAddressConfig)base.Parent;

		protected override void CheckDocumentName()
		{
			base.CheckDocumentName();
			if (!Parent.GoodsDescriptionConfigs.Any())
			{
				Parent.DocumentNameInfo.AddError(Res.GetString("5FACE912-1F4D-4631-8C03-499C189EE704", "Must have at least a row where Field is Goods Description."));
			}
		}

		protected override void CheckCustomizeSectionBodyRow()
		{
			base.CheckCustomizeSectionBodyRow();
			ListValidation.ErrorIfInvalidCode(Parent.CustomizeSectionBodyRowInfo);
		}
	}
}
