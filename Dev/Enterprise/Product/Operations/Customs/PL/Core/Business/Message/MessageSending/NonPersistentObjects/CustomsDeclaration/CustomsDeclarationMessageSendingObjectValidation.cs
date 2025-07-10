using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using ExportSecurityTypeList = Enterprise.Customs.EU.Business.ExportSecurityTypeList;

namespace Enterprise.Customs.PL.Business;

public class CustomsDeclarationMessageSendingObjectValidation : BaseMessageSendingObjectValidation
{
	public CustomsDeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	protected override void CheckEntryNumber()
	{
		var parent = Parent;
		var expectedEntryNumberLength = parent.ExpectedEntryNumberLength;
		switch (parent.Action)
		{
			case MessageSendingObjectActionCodes.CC513:
				if (parent.EntryNumber.Length != expectedEntryNumberLength)
				{
					parent.EntryNumberInfo.AddError(Res.GetString("2005D483-4B8D-4BAD-A06A-DB436D52499B", "Enter the {0} char long MRN number of declaration being amended.", expectedEntryNumberLength));
				}
				break;
			case MessageSendingObjectActionCodes.CC583:
				if (parent.EntryNumber.IsEmpty)
				{
					parent.EntryNumberInfo.AddError(Res.GetString("E38F9F1D-ACD0-4199-969C-EC2845BBE411", "Enter the MRN number of declaration being amended."));
				}
				break;
		}
	}

	protected override void CheckSecurity()
	{
		base.CheckSecurity();

		if (Parent.Action.ToString() is MessageSendingObjectActionCodes.CC513 or MessageSendingObjectActionCodes.CC515)
		{
			CheckRuleR0094E();
			CheckRuleR0095E();
		}
	}

	void CheckRuleR0094E()
	{
		if (Parent.Header.Declaration is not { } declaration
			|| declaration.JE_EntryStyle != EntryStyleListExport.Codes.ExportNormal
			|| Parent.Security != ExportSecurityTypeList.Codes.EXS
			|| !ContainsSubStyleAorD(declaration.EntryInstructionSubStyle)
			|| !declaration.Invoices.Cast<JobComInvoiceHeader>().All(x => x.ZG_TransportChargesMethodOfPayment.IsEmpty))
		{
			return;
		}

		Parent.SecurityInfo.AddMessageError(Res.GetString("PLJobDeclarationMessageSendingObjectValidation|CheckRuleR0094E",
			"[R0094E] For Declaration Type = 'EX' and Additional Declaration Type = 'A' or 'D' where Security = '2' Method of Payment is required."));
	}

	void CheckRuleR0095E()
	{
		if (Parent.Header.Declaration is not { } declaration
			|| !BaseMessageSendingObjectSecurityValidationHelper.IsRuleR0095Valid(declaration)
			|| Parent.Security == ExportSecurityTypeList.Codes.EXS)
		{
			return;
		}

		Parent.SecurityInfo.AddMessageError(Res.GetString("PLJobDeclarationMessageSendingObjectValidation|CheckRuleR0095E",
			"[R0095E] For Declaration Type = 'EX' and Additional Declaration Type = 'A' or 'D' Security must be value '2'"));
	}

	bool ContainsSubStyleAorD(string subStyles) => subStyles.Contains(SubStyleCodes.A) || subStyles.Contains(SubStyleCodes.D);
}
