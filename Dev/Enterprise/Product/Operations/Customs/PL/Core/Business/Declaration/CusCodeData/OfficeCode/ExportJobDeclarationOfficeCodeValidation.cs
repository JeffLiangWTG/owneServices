using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportJobDeclarationOfficeCodeValidation : OfficeCodeValidation
{
	public ExportJobDeclarationOfficeCodeValidation(OfficeCode parent) : base(parent)
	{
	}

	JobDeclaration Declaration => (JobDeclaration)Parent.Parent;

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		Utils.RuleR0091EForSCOPurpose(Declaration, Parent);
	}

	#region CY_Code

	protected override void CheckCY_Code()
	{
		base.CheckCY_Code();

		if (Parent.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation)
		{
			CheckRuleR0006E();
			CheckRuleG0066();
		}
	}

	void CheckRuleR0006E()
	{
		if (!AnyCustomsEntryInstructionIsCentralizedCustomsDeclaration())
		{
			Parent.CY_CodeInfo.AddMessageError(Res.GetString("ExportJobDeclarationOfficeCodeValidation|CheckRuleR0006E",
				"(R0006E) Presentation Customs Office cannot be specified if the authorization code C513 does not exist."));
		}
	}

	void CheckRuleG0066()
	{
		if (IsIE515BMessage)
		{
			Parent.CY_CodeInfo.AddMessageError(Res.GetString("ExportOfficeCodeValidation|CheckRuleG0066", "(G0066) Presentation Customs Office code is not allowed."));
		}
	}

	bool IsIE515BMessage => Declaration?.CustomsEntryInstructions.Any(entry => entry.IsIE515BMessage) ?? false;

	#endregion

	#region CY_Data

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();

		var code = Parent.CY_Code;

		if (code == EuOfficeCodesTypes.Codes.OfficeOfPresentation)
		{
			CheckRuleR0007E();
		}
		else if (code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice)
		{
			CheckRuleG0187();
		}
	}

	void CheckRuleR0007E()
	{
		var customsOffice = Declaration?.JE_CustomsOffice ?? ZString.Empty;

		if (!customsOffice.IsEmpty && customsOffice == Parent.CY_Data)
		{
			var authorisationCodeC513Exists = Declaration?.CustomsEntryInstructions.Any(x => x.HasAuthorisationUsageCode(Constants.CusAuthorizationUsageType.C513)) ?? false;
			if (authorisationCodeC513Exists)
			{
				Parent.CY_DataInfo.AddMessageError(Res.GetString("ExportJobDeclarationOfficeCodeValidation|CheckRuleR0007E", "(R0007E) A Decl. Customs Office code cannot be the same as a Customs Office of Presentation"));
			}
		}
	}

	void CheckRuleG0187()
	{
		var customsOffice = Declaration?.JE_CustomsOffice ?? ZString.Empty;

		if (!customsOffice.IsEmpty && customsOffice == Parent.CY_Data)
		{
			Parent.CY_DataInfo.AddMessageError(Res.GetString("ExportJobDeclarationOfficeCodeValidation|CheckRuleG0187", "(G0187) A Decl. Customs Office of Export and Supervising Customs Office codes must be different."));
		}
	}
	#endregion

	bool AnyCustomsEntryInstructionIsCentralizedCustomsDeclaration() => Declaration?.CustomsEntryInstructions.Any(entry => entry.IsCentralizedCustomsDeclaration) ?? true;
}
