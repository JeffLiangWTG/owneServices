using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration;

public abstract class JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : AutoNZJobComInvoiceHeaderValidation(invoiceHeader)
{
	protected new JobComInvoiceHeader Parent
	{
		get { return (JobComInvoiceHeader)base.Parent; }
	}

	protected override void CheckJZ_IsGSTPrePaid()
	{
		base.CheckJZ_IsGSTPrePaid();

		var declaration = Parent.JobDeclaration;
		if (declaration?.IsImport ?? false)
		{
			if (Parent.JZ_IsGSTPrePaid.IsEmpty)
			{
				if (!Parent.JZ_SupplierGSTNumber.IsEmpty)
				{
					Parent.JZ_IsGSTPrePaidInfo.AddMessageError(Res.GetString("A116CBB7-3279-4A36-914A-84B688CFB6A2", "GST Prepaid must be entered when Overseas Registered Supplier GST Number is entered."));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JZ_IsGSTPrePaidInfo, declaration.Lookups.YesNoList);
			}

			CheckSupplierNumberPrepaidCombination(declaration, Parent.JZ_IsGSTPrePaidInfo);
		}
	}

	protected override void CheckJZ_SupplierGSTNumber()
	{
		base.CheckJZ_SupplierGSTNumber();

		var declaration = Parent?.JobDeclaration;
		if (declaration?.IsImport ?? false)
		{
			var parent = Parent;
			if (parent.JZ_SupplierGSTNumber.IsEmpty && !parent.JZ_IsGSTPrePaid.IsEmpty)
			{
				parent.JZ_SupplierGSTNumberInfo.AddMessageError(Res.GetString("70F547EA-EA92-463F-82C8-659EF90D8DEF", "Overseas Registered Supplier GST Number must be entered when GST Prepaid is entered."));
			}

			if (!parent.JZ_SupplierGSTNumber.IsEmpty && !GSTNumberValidation.ValidateGSTNumber(parent.JZ_SupplierGSTNumber))
			{
				parent.JZ_SupplierGSTNumberInfo.AddMessageError(Res.GetString("e09fbea7-14f6-478b-852a-8e7338de5d86", "The entered GST Number is not valid.\r\nA GST Number must be 8 or 9 digits and can be formatted as 99-999-999 or 999-999-999 with the final digit a check-digit."));
			}

			CheckSupplierNumberPrepaidCombination(declaration, parent.JZ_SupplierGSTNumberInfo);
		}
	}

	void CheckSupplierNumberPrepaidCombination(JobDeclaration declaration, ZPropertyInfo info)
	{
		if ((declaration?.IsECIWriteoffAndGSTIsApplicable ?? false) && !declaration.HasTheSameGSTDetailsOnAllInvoices)
		{
			info.AddMessageError(Res.GetString("F96B901D-5652-4E17-9A19-E4D45C26C78F", "The Supplier GST Number / Prepaid combination must be the same on all Invoices."));
		}
	}

	protected override void CheckJZ_IsZeroRatedDuty()
	{
		base.CheckJZ_IsZeroRatedDuty();
		if (Parent != null && Parent.IsImport)
		{
			if (!Parent.JZ_IsZeroRatedDuty.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JZ_IsZeroRatedDuty))
			{
				Parent.JZ_IsZeroRatedDutyInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedDutyFlag);
			}
		}
	}

	protected override void CheckJZ_IsZeroRatedExcise()
	{
		base.CheckJZ_IsZeroRatedExcise();
		if (Parent != null && Parent.IsImport)
		{
			if (!Parent.JZ_IsZeroRatedExcise.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JZ_IsZeroRatedExcise))
			{
				Parent.JZ_IsZeroRatedExciseInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedExciseFlag);
			}
		}
	}

	protected override void CheckJZ_IsZeroRatedGST()
	{
		base.CheckJZ_IsZeroRatedGST();
		if (Parent != null && Parent.IsImport)
		{
			if (!Parent.JZ_IsZeroRatedGST.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JZ_IsZeroRatedGST))
			{
				Parent.JZ_IsZeroRatedGSTInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedGSTFlag);
			}
		}
	}

	protected override void CheckJZ_IsZeroRatedLevies()
	{
		base.CheckJZ_IsZeroRatedLevies();
		if (Parent != null && Parent.IsImport)
		{
			if (!Parent.JZ_IsZeroRatedLevies.IsEmpty && !Parent.Lookups.YesNoList.ContainsCode(Parent.JZ_IsZeroRatedLevies))
			{
				Parent.JZ_IsZeroRatedLeviesInfo.AddMessageError(MessageErrorMustHaveValidZeroRatedLeviesFlag);
			}
		}
	}

	protected override void CheckJZ_RN_NKCountryOfExport()
	{
		base.CheckJZ_RN_NKCountryOfExport();
		if (!Parent.JZ_RN_NKCountryOfExport.IsEmpty && Parent.IsImport && Parent.CountryOfExport == null)
		{
			Parent.JZ_RN_NKCountryOfExportInfo.AddMessageError(MessageErrorRemoveOrFixDefaultCountryOfExport);
		}
	}

	protected override void CheckJZ_QualifiesForPreferentialDuty()
	{
		base.CheckJZ_QualifiesForPreferentialDuty();
		if (!Parent.JZ_QualifiesForPreferentialDuty.IsEmpty && Parent.IsImport)
		{
			QualifiesForPreferentialDutyList list = new QualifiesForPreferentialDutyList();
			if (!list.ContainsCode(Parent.JZ_QualifiesForPreferentialDuty))
			{
				Parent.JZ_QualifiesForPreferentialDutyInfo.AddMessageError(MessageErrorRemoveOrFixDefaultQualForPrefDutyFlag);
			}
		}

		ValidateJZ_PreferentialCountryGroup();
	}

	protected override void CheckJZ_PreferentialCountryGroup()
	{
		base.CheckJZ_PreferentialCountryGroup();
		JobComInvoiceHeader invoiceHeader = Parent;
		if (invoiceHeader.IsImport)
		{
			if (!invoiceHeader.JZ_DefaultPreferentialCountryGroup.IsEmpty && !invoiceHeader.Lookups.PreferentialCountryGroupCodeList.ContainsCode(invoiceHeader.JZ_DefaultPreferentialCountryGroup))
			{
				Parent.JZ_PreferentialCountryGroupInfo.AddMessageError(MessageErrorPreferentialCountryGroupNotInList);
			}
		}
	}

	protected override void CheckJZ_RelationshipIndicator()
	{
		if (Parent.JobDeclaration != null && Parent.JobDeclaration.IsFormalEntry && Parent.JobDeclaration.IsImport)
		{
			ZString relationshipIndicator = Parent.JZ_RelationshipIndicator;
			if (relationshipIndicator.IsEmpty || !new RelationshipIndicatorList(Parent.JobDeclaration.IsTSWDeclaration).ContainsCode(relationshipIndicator))
			{
				Parent.JZ_RelationshipIndicatorInfo.AddMessageError(MessageErrorMissingRelationshipIndicator);
			}
		}
	}

	protected internal static string MessageErrorMissingRelationshipIndicator => Res.GetString("A7440438-9851-4F05-8EF2-2971887BEF97",
		@"Please enter a valid Relationship Indicator. 
	(NB: You can set a default for this Supplier/Importer combination by going to the 
	master file for the Supplier on this Invoice (F3) selecting the Consignor Tab 
	then the Consignee/Buyer/Importer Relationships tab and adding an 
	Importer Relationship, setting the 'Transactions Related' flag as desired.)");
	protected internal static string MessageErrorRemoveOrFixDefaultQualForPrefDutyFlag => Res.GetString("31DC2A7D-7195-4E52-ADF0-1C98CD0E399E", "Please either remove or enter a valid Default Qualifies for Preferential Duty flag.");
	protected internal static string MessageErrorPreferentialCountryGroupNotInList => Res.GetString("80FBFB44-47E9-4B85-99F8-5282A9ADA5EA", "The Preference Code you have entered is not valid for this Country/Region of Origin.");
	protected internal static string MessageErrorRemoveOrFixDefaultCountryOfExport => Res.GetString("FA5F5EDB-0359-49BA-8F3A-EC37179C0AD0", "Please either remove or enter a valid Default Country/Region of Export.");
	protected internal static string MessageErrorMustHaveValidZeroRatedDutyFlag => Res.GetString("35B1BBE0-FCA9-4E9B-9A05-B238AC4C427E", "'Zero Rated Duty' must be either blank, 'Yes' or 'No'.");
	protected internal static string MessageErrorMustHaveValidZeroRatedExciseFlag => Res.GetString("6879606C-B866-4180-A6E0-E9BD04C9C42C", "'Zero Rated Excise' must be either blank, 'Yes' or 'No'.");
	protected internal static string MessageErrorMustHaveValidZeroRatedGSTFlag => Res.GetString("391475FC-2186-473D-B3DD-8D38B6D1ADB4", "'Zero Rated GST' must be either blank, 'Yes' or 'No'.");
	protected internal static string MessageErrorMustHaveValidZeroRatedLeviesFlag => Res.GetString("2374C51A-BC9A-4D61-8F05-0C474DAFD23C", "'Zero Rated Levies' must be either blank, 'Yes' or 'No'.");
}
