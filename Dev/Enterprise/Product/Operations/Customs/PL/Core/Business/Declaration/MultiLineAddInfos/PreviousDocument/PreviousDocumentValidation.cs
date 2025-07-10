using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent)
	{
	}

	protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

	protected override void CheckCSI_DateOfIssue() { }

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		var csiQuantityInfo = Parent.CSI_QuantityInfo;
		var csiQuantityValue = Parent.CSI_Quantity;
		var csiUnitOfQuantityValue = Parent.CSI_UnitOfQuantity;
		if (!csiQuantityValue.IsEmpty && UnitOfQuantityRequiresIntegerQuantity(csiUnitOfQuantityValue) && !csiQuantityValue.IsInteger)
		{
			csiQuantityInfo.AddMessageError(Res.GetString("PLPreviousDocumentValidation|CSI_QuantityInteger",
				"Quantity should be an integer value."));
		}
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		if (Parent.CSI_Quantity > 0)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo);
		}
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo);
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();
		var parent = Parent;
		var type = parent.CSI_Code;
		if (!type.IsEmpty)
		{
			var dataGroupingCode = parent.Declaration?.GetDefaultDataGroupingCode() ?? ZString.Empty;
			var attributes = parent.Factory.GetCachedValue($"PreviousDocumentValidation_CSI_CodeAttributes_{type}_{dataGroupingCode}_{ZDateTime.Today}", () =>
			{
				var attributeName = Constants.RefCusCodeListAttributeName.ItemNumber;
				var previousDocumentOfExportDirectionAttributes = new RefCusCodeListAttribute.Loader(parent.Factory).Load(
					dataGroupingCode, ZDateTime.Today,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, type, attributeName);
				var exportPreviousDocumentSpecialProceduresAttributes = new RefCusCodeListAttribute.Loader(parent.Factory).Load(
					dataGroupingCode, ZDateTime.Today,
					UniversalReferenceConstants.RefCusCodeListType.Codes.ExportPreviousDocumentSpecialProcedures, type, attributeName);

				return previousDocumentOfExportDirectionAttributes.Concat(exportPreviousDocumentSpecialProceduresAttributes);
			});

			if (attributes.Any(x => x.ZZE_Value.EqualsIgnoringCase("R")))
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CSI_LineNoInfo);
			}

			if (parent.CSI_LineNo.IsEmpty
				&& (type == Constants.PreviousDocumentCodes._355 || type == Constants.PreviousDocumentCodes._337))
			{
				parent.CSI_LineNoInfo.AddMessageError(Res.GetString("PLPreviousDocumentValidation|CheckRuleR1044",
					"(R1044) For Previous Document type 355 and 337 it’s required to enter Line no."));
			}
		}
	}

	bool UnitOfQuantityRequiresIntegerQuantity(ZString csiUnitOfQuantityValue)
	{
		return csiUnitOfQuantityValue == Constants.SupportingDocumentUnitOfQuantityCodes.NumberOfCells ||
				csiUnitOfQuantityValue == Constants.SupportingDocumentUnitOfQuantityCodes.NumberOfItems ||
				csiUnitOfQuantityValue == Constants.SupportingDocumentUnitOfQuantityCodes.NumberOfPairs;
	}
}
