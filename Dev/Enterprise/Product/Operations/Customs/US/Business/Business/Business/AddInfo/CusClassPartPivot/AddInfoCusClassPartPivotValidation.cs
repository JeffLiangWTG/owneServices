using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusClassPartPivotValidation : USAddInfoValidation
	{
		public AddInfoCusClassPartPivotValidation(AddInfoCusClassPartPivot parent)
			: base(parent)
		{
		}

		protected new AddInfoCusClassPartPivot Parent
		{
			get { return (AddInfoCusClassPartPivot)base.Parent; }
		}

		protected AddInfoCusClassPartPivotLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckUS_ProductExclusion()
		{
			base.CheckUS_ProductExclusion();

			var exclusionCode = Parent.US_ProductExclusion;

			if (exclusionCode == AdditionalDeclarationTypeCodeList.Codes._02 || exclusionCode == AdditionalDeclarationTypeCodeList.Codes._03)
			{
				var pivot = Parent.Parent;

				if (pivot.IsOnlySteelProductAvailable && exclusionCode != AdditionalDeclarationTypeCodeList.Codes._02)
				{
					Parent.US_ProductExclusionInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
				}
				else if (pivot.IsOnlyAluminumProductAvailable && exclusionCode != AdditionalDeclarationTypeCodeList.Codes._03)
				{
					Parent.US_ProductExclusionInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductExclusionInfo, Parent.Lookups.ProductExclusionList);
			}

			ValidateUS_ExclusionNumber();
		}

		protected override void CheckUS_ExclusionNumber()
		{
			base.CheckUS_ExclusionNumber();

			var productExclusion = Parent.US_ProductExclusion;
			if (productExclusion.IsEmpty)
			{
				if (!Parent.US_ExclusionNumber.IsEmpty)
				{
					Parent.US_ExclusionNumberInfo.AddMessageError(ACEImportAddInfoJobComInvoiceLineValidation.ProductExclustionNumberNotAllowed);
				}
			}
			else
			{
				var exclusionNumber = Parent.US_ExclusionNumber;
				UniversalReferenceDataHelper.ValidateNumberByRegexInZZ(
					Parent.Factory,
					Parent.US_ExclusionNumberInfo,
					exclusionNumber,
					productExclusion,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USProductExclusionTypes,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeMask,
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeErrorText,
					ACEImportAddInfoJobComInvoiceLineValidation.InvalidProductExclusionNumberMessagePrefix);
			}
		}
	}
}
