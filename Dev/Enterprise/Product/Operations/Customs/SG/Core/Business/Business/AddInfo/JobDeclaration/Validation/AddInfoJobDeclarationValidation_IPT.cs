namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation_IPT : AddInfoCUSDECValidation
	{
		public AddInfoJobDeclarationValidation_IPT(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_IsSeaStore()
		{
			base.CheckSG_IsSeaStore();

			if (Parent.Declaration.IsSeaStore && !Parent.Declaration.IsTradeNet4Point1)
			{
				Parent.SG_IsSeaStoreInfo.AddMessageError("Sea Store details are only valid on INP, TNP and OUT Declarations.");
			}
		}

		protected override void CheckSG_RemovalStartDate()
		{
			base.CheckSG_RemovalStartDate();
			if (Parent.Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT && Parent.SG_RemovalStartDate.IsEmpty)
			{
				Parent.SG_RemovalStartDateInfo.AddWarning("Start Date.\r\nFor blanket imports, specify the Start Date");
			}
		}
	}
}
