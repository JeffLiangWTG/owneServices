using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public partial class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_CountryOfOrigin()
		{
			base.CheckJI_CountryOfOrigin();
			var invoiceLine = Parent;
			var declaration = invoiceLine.Declaration;
			if (invoiceLine.JI_CountryOfOrigin.IsEmpty
				&& declaration.CusEntryInstruction.CEI_Style == Constants.DeclarationTypes.Export.F5 && invoiceLine.F5FTZDestinationCountryCodes.Contains(declaration.FinalDestinationCountryCode))
			{
				invoiceLine.JI_CountryOfOriginInfo.AddMessageError(Res.GetString("65905af9-6297-427a-84f6-f86a38435429", "When the Final Destination country for export from the Free Trade Zone is one of EU, US, and GB, the Goods Origin is required."));
			}

			ListValidation.MessageErrorIfInvalidCode(invoiceLine.JI_CountryOfOriginInfo);
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();

			var parent = Parent;
			if (parent.IsModeOfStatisticsRequirePermitNumber && parent.PermitCusSupportingCollection.Count == 0)
			{
				parent.AddRowMessageError(ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);
			}
		}

		protected override void CheckPermitCusSupportingNo1()
		{
			base.CheckPermitCusSupportingNo1();
			var parent = Parent;
			if (parent.IsModeOfStatisticsRequirePermitNumber && parent.PermitCusSupportingNo1.IsEmpty)
			{
				parent.PermitCusSupportingNo1Info.AddMessageError(ValidationConstants.InvoiceLine.IsModeOfStatisticsRequirePermitNumberMessage);
			}
		}

		protected override void CheckJI_CustomsSecondQuantity()
		{
			base.CheckJI_CustomsSecondQuantity();
			if (!Parent.JI_CustomsSecondUnitQty.IsEmpty)
			{
				CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JI_CustomsSecondQuantityInfo);
			}
		}

		protected override void CheckJI_BrandName()
		{
			base.CheckJI_BrandName();
			if (Parent.JI_BrandName.IsEmpty)
			{
				Parent.JI_BrandNameInfo.AddWarning(Res.GetString("8DC04C28-CAF0-4543-AFCD-D011C69EC632", "System will automatically declare 'No Brand' when Brand Name is empty."));
			}
		}
	}
}
