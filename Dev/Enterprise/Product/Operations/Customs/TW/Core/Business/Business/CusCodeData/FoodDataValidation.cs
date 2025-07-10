using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class FoodDataValidation : Customs.Business.CusCodeDataValidation
	{
		public FoodDataValidation(FoodData parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateContent();
		}

		protected new FoodData Parent => (FoodData)base.Parent;

		protected override void CheckCY_CodeList()
		{
		}

		protected override void CheckCY_Data()
		{
			var targetInfo = Parent.CY_DataInfo;
			if (!Parent.Content.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		public void ValidateContent()
		{
			ValidateCalculatedProperty(Parent.ContentInfo);
		}

		protected void CheckContent()
		{
			var targetInfo = Parent.ContentInfo;
			var content = Parent.Content;
			if (content < 0m)
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLine.InvalidValue(targetInfo.HumanReadableName));
			}
			TypeValidation.CheckValidDecimal(targetInfo, 14, 4);
		}
	}
}
