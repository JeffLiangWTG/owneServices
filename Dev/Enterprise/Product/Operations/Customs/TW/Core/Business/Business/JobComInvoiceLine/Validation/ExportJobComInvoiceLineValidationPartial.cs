using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public partial class ExportJobComInvoiceLineValidation
	{
		protected override void CheckJI_BondedGoodsCode()
		{
			base.CheckJI_BondedGoodsCode();
			var targetInfo = Parent.JI_BondedGoodsCodeInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.BondedGoodsCodeList);

			var entryInstruction = Parent?.EntryInstruction;
			if (entryInstruction != null && entryInstruction.CEI_Style == Constants.DeclarationTypes.Export.B9 && Parent.JI_BondedGoodsCode.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("E62E09F9-AC8E-4EB0-AC6B-8F93945EB353", "Bonded Goods Code cannot be empty when the Declaration Type is 'B9'."));
			}
		}
	}
}
