using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Constants = Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business;

sealed class ImportJobDeclarationValidation : JobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_OH_Importer()
	{
		base.CheckJE_OH_Importer();

		var parent = Parent;
		var importer = parent.Importer;
		var orgType = importer?.OH_Category ?? ZString.Empty;
		var targetInfo = parent?.JE_OH_ImporterInfo;

		if (orgType == string.Empty)
		{
			return;
		}
		if (orgType.EqualsIgnoringCase(Constants.OrgHeaderType.NaturalPerson))
		{
			if (!parent.HasImporterWithSocialSecurityNumber)
			{
				targetInfo.AddMessageError(Res.GetString("3F959066-E964-4977-98D9-B5DCEC384CA2", "The importer is a natural person but has no social security number."));
			}
			targetInfo.AddWarning(Res.GetString("D6FC026F-9CF9-F881-4CC1-96E05D574C39", "The importer is a natural person. That is: No deferred customs account and not MVA registered. Potential outlays."));
		}
		else
		{
			var hasMVA = parent.HasImporterWithMVARegistration;
			var hasDAN = parent.HasImporterWithDeferredCustomsPaymentAccount;
			var hasORG = parent.HasImporterWithOrganizationNumber;

			if (!hasORG && !hasMVA)
			{
				targetInfo.AddMessageError(Res.GetString("5F66E288-0CF0-47A6-B016-42BCC7DA5631", "The importer is an organization but has no ORG number or MVA registered number."));
			}
			if (!hasMVA && !hasDAN)
			{
				targetInfo.AddWarning(Res.GetString("836BCCD8-AE9F-CCA3-456A-72E43701830B", "The importer is not MVA registered and has no deferred customs account. Potential outlays."));
			}
			if (!hasMVA && hasDAN)
			{
				targetInfo.AddWarning(Res.GetString("020CCF53-F05D-8DA7-48E9-76113A87FC90", "The importer is not MVA registered. Potential outlays."));
			}
			if (hasMVA && !hasDAN)
			{
				targetInfo.AddWarning(Res.GetString("666A8F52-EE1A-03BC-475A-45F23AD1D99A", "The importer has no deferred customs account. Potential outlays."));
			}
		}

		CheckValidPowerOfAttorney(targetInfo, importer);
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_GoodsDestinationInfo);
	}

	protected override void CheckJE_GoodsNumber()
	{
		base.CheckJE_GoodsNumber();
		if (IsProcedureEndingWith(ProcedureCodeSuffix.TemporaryTransferCodes))
		{
			if (!Parent.JE_GoodsNumber.IsEmpty)
			{
				Parent.JE_GoodsNumberInfo.AddIsNotRequiredMessageError(because: Res.GetString("2E88ADFE-20BA-491A-A663-D389866FE3DB", "The Procedure code you have selected"));
			}
		}
		else
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsNumberInfo);
		}
	}

	protected override void CheckJE_GoodsOrigin()
	{
		base.CheckJE_GoodsOrigin();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_GoodsOriginInfo);
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();
		if (IsProcedureEndingWith(ProcedureCodeSuffix.TemporaryTransferCodes))
		{
			if (!Parent.JE_LocationOfGoods.IsEmpty)
			{
				Parent.JE_LocationOfGoodsInfo.AddIsNotRequiredMessageError(because: Res.GetString("E6CDF663-C509-4868-85F8-C3F779022C57", "The Procedure code you have selected"));
			}
		}
		else
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_LocationOfGoodsInfo);
		}
	}

	protected override ZString CusAuthorizationType => CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration;

	#region Helpers

	static class ProcedureCodeSuffix
	{
		public const string TemporaryTransferOther = "50";
		public const string TemporaryTransferProcessing = "51";
		public const string TemporaryTransferReparation = "52";
		public static string[] TemporaryTransferCodes => new[] {
			TemporaryTransferOther,
			TemporaryTransferProcessing,
			TemporaryTransferReparation
		};
	}

	bool IsProcedureEndingWith(params string[] codes) => IsCEI_ProcedureEndingWith(codes) || IsJI_ProcedureEndingWith(codes);
	bool IsCEI_ProcedureEndingWith(params string[] codes) => Parent.CustomsEntryInstructions.ContainsAnyEndingWith(x => x.CEI_Procedure, codes);
	bool IsJI_ProcedureEndingWith(params string[] codes) => Parent.InvoiceLines.Select(x => x.JI_Procedure).ContainsAnyEndingWith(codes);

	#endregion
}
