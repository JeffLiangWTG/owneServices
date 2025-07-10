using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

public sealed class CusEntryInstructionValidation(CusEntryInstruction parent) : AutoNOCusEntryInstructionValidation(parent)
{
	readonly CusEntryInstruction parent = Argument.NotNull(parent, nameof(parent));

	public override void ValidateAll()
	{
		base.ValidateAll();
		VerifyIfInvoiceHeadersHaveDistinctMergeValues();
	}

	protected override void CheckCEI_DateForDuty()
	{
		base.CheckCEI_DateForDuty();
		var today = ZDateTime.Today.EndOfDay();
		if (parent.CEI_DateForDuty > today && !parent.HasDigitollGoodsNumber)
		{
			parent.CEI_DateForDutyInfo.AddMessageError(Res.GetString(
				"237f2443-39e2-479e-b039-6741c7a35438",
				"Date ahead in time is only allowed on a Digitoll or Direct declaration (Goods number char 7-8 = DT or D)."));
			return;
		}
		var fiveDaysInFuture = today.AddDays(5);
		if (parent.CEI_DateForDuty > fiveDaysInFuture && parent.HasDigitollGoodsNumber)
		{
			parent.CEI_DateForDutyInfo.AddMessageError(Res.GetString(
				"c1457304-4d80-45b6-9c74-7bf9643c672e",
				"Date can be maximum 5 days ahead in time."));
		}
	}

	protected override void CheckCEI_Description()
	{
		base.CheckCEI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(parent.CEI_DescriptionInfo);
	}

	protected override void CheckCEI_PackageCount()
	{
		base.CheckCEI_PackageCount();
		var packageCountInfo = parent.CEI_PackageCountInfo;
		MandatoryValidation.MessageErrorIfNotEntered(packageCountInfo);
		MandatoryValidation.MessageErrorIfIsNegative(packageCountInfo);
		CheckSumOfUnitsOnAllInstructionsShouldBeEqualToTheDeclarationHeader();
	}

	protected override void CheckCEI_Procedure()
	{
		base.CheckCEI_Description();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CEI_ProcedureInfo);
	}

	protected override void CheckCEI_Style()
	{
		base.CheckCEI_Style();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CEI_StyleInfo);
	}

	protected override void CheckCEI_SubStyle()
	{
		base.CheckCEI_SubStyle();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CEI_SubStyleInfo);
	}

	internal void VerifyIfInvoiceHeadersHaveDistinctMergeValues()
	{
		var invoices = parent.Invoices;
		if (invoices.Count > 1)
		{
			if (invoices.Select(x => x.JZ_IncoTerm).Distinct().Count() > 1)
			{
				parent.AddRowError(GetErrorMessage(Res.GetString("FA6964A8-0589-9B9D-420D-61CA9752B37E", "Incoterm Codes")));
			}
			if (invoices.Select(x => x.JZ_ValuationCode).Distinct().Count() > 1)
			{
				parent.AddRowError(GetErrorMessage(Res.GetString("E5D45181-4092-E88A-4ECB-52675EB4AB7E", "Natures of Transaction")));
			}
		}
		string GetErrorMessage(string name)
		{
			return Res.GetString("BAADAB82-AE1F-FC89-45BB-7A4F75E7856A", "Invoice Headers with different {0} are linked to this Entry Instruction.", name);
		}
	}

	void CheckSumOfUnitsOnAllInstructionsShouldBeEqualToTheDeclarationHeader()
	{
		if (parent.CEI_PackageCount > 0 && parent.JobDeclaration is { } declaration)
		{
			var sumOfPackages = declaration.CustomsEntryInstructions.Sum(x => x.CEI_PackageCount);
			if (sumOfPackages != declaration.JE_TotalNoOfPieces)
			{
				parent.CEI_PackageCountInfo.AddWarning(Res.GetString("918AB0C1-D743-46DB-A1BB-9512B4D0841E",
					@"The sum of number of Units on all Entry Instructions should be equal to {0}, the Number of Units on Declaration Header.",
					declaration.JE_TotalNoOfPieces));
			}
		}
	}
}
