using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChequeBookValidation : AutoAccChequeBookValidation
	{
		public AccChequeBookValidation(AutoAccChequeBook parent) : base(parent)
		{
		}

		protected new AccChequeBook Parent
		{
			get { return (AccChequeBook)base.Parent; }
		}

		protected internal AccValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new AccValidationHelper();
				}
				return fValidationHelper;
			}
		}
		AccValidationHelper fValidationHelper;

		protected override void CheckAK_Code()
		{
			base.CheckAK_Code();

			CheckSecurity(Parent.AK_CodeInfo, Env.Security.ChequeBooksModifyCode);

			if (!Parent.AK_CodeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.AK_CodeInfo);
			}

			if (!Parent.AK_CodeInfo.HasErrors())
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(AccChequeBook));
				dBOnlyQuery.AddToFilter(AccChequeBookSchema.AK_Code, Parent.AK_Code);
				dBOnlyQuery.AddToFilter(AccChequeBookSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccBankAccount), AccChequeBookSchema.AK_AB);

				GlbCompany company = Parent.GetReferencedCompany();
				if (company != null)
				{
					subQuery.AddToFilter(AccBankAccountSchema.AB_GC, company.PK);
				}
				else
				{
					subQuery.AddToFilter(AccBankAccountSchema.AB_GC, Env.CurrentCompany.PK);
				}

				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
				AccChequeBook chequeBook = new BusinessObjectFactory().LoadTop1<AccChequeBook>(dBOnlyQuery);
				if (chequeBook != null)
				{
					Parent.AK_CodeInfo.AddError(Res.GetString("dc9980b8-5aab-4677-8e84-2a7ec0cb80d6", "Check book code must be unique"));
				}
			}
		}

		protected override void CheckAK_Desc()
		{
			base.CheckAK_Desc();

			CheckSecurity(Parent.AK_DescInfo, Env.Security.ChequeBooksModifyDescription);

			if (!Parent.AK_DescInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.AK_DescInfo);
			}
		}

		protected override void CheckAK_GB()
		{
			base.CheckAK_GB();

			CheckSecurity(Parent.AK_GBInfo, Env.Security.ChequeBooksModifyBranchCode);
		}

		protected override void CheckAK_AB()
		{
			base.CheckAK_AB();
			if (!Parent.AK_ABInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.AK_ABInfo);
			}
		}

		protected override void CheckAK_StartNo()
		{
			base.CheckAK_StartNo();

			CheckSecurity(Parent.AK_StartNoInfo, Env.Security.ChequeBooksModifyStartNo);

			MandatoryValidation.CheckEntered(Parent.AK_StartNoInfo);
			if (!Parent.AK_StartNo.IsInteger)
			{
				Parent.AK_StartNoInfo.AddError(Res.GetString("9EC57E7C-3418-462C-A28A-EB2C484BB1A8", "Start number must be an integer"));
			}
			if (Parent.AK_StartNo > Parent.AK_LastNo && Parent.AK_LastNo != 0)
			{
				Parent.AK_StartNoInfo.AddError(Res.GetString("397c992e-0f6d-4319-a93a-b711e6bfc265", "Start number cannot be greater than last number"));
			}
			if (Parent.StartNoOverlapsExistingRanges())
			{
				Parent.AK_StartNoInfo.AddError(Res.GetString("2e3af315-df8e-4cc7-b263-755a9fd0713d", "Check Book Number ranges cannot overlap for different check books for the same bank account"));
			}
			if (Parent.BankAccount != null)
			{
				ValidationHelper.ValidateChequeDigits(Parent.AK_StartNoInfo, Parent.BankAccount.AB_ChequeNumDigits);
			}
		}

		protected override void CheckAK_LastNo()
		{
			base.CheckAK_LastNo();

			CheckSecurity(Parent.AK_LastNoInfo, Env.Security.ChequeBooksModifyLastNo);

			MandatoryValidation.CheckEntered(Parent.AK_LastNoInfo);
			if (!Parent.AK_LastNo.IsInteger)
			{
				Parent.AK_LastNoInfo.AddError(Res.GetString("E10AA047-C6FD-48A3-89B3-8F9B83089F35", "Last number must be an integer"));
			}
			if (Parent.AK_LastNo <= Parent.AK_StartNo)
			{
				Parent.AK_LastNoInfo.AddError(Res.GetString("77a22b55-8087-4aa7-8b1d-d117551e0cff", "Last number must be greater than Start number"));
			}
			if (Parent.LastNoOverlapsExistingRanges())
			{
				Parent.AK_LastNoInfo.AddError(Res.GetString("2e3af315-df8e-4cc7-b263-755a9fd0713d", "Check Book Number ranges cannot overlap for different check books for the same bank account"));
			}
			if (Parent.BankAccount != null)
			{
				ValidationHelper.ValidateChequeDigits(Parent.AK_LastNoInfo, Parent.BankAccount.AB_ChequeNumDigits);
			}
		}

		protected override void CheckAK_CurrentNo()
		{
			base.CheckAK_CurrentNo();

			CheckSecurity(Parent.AK_CurrentNoInfo, Env.Security.ChequeBooksModifyCurrentNo);

			MandatoryValidation.CheckEntered(Parent.AK_CurrentNoInfo);
			if (!Parent.AK_CurrentNo.IsInteger)
			{
				Parent.AK_CurrentNoInfo.AddError(Res.GetString("C2C6A86E-5C5E-4537-BAA0-2EA959685F03", "Current number must be an integer"));
			}
			if (Parent.AK_CurrentNo < Parent.AK_StartNo || Parent.AK_CurrentNo > Parent.AK_LastNo)
			{
				Parent.AK_CurrentNoInfo.AddError(Res.GetString("540f992a-a719-4f0d-8936-1702e6c6d4c1", "Current number must be between start number and last number"));
			}

			if (!Parent.AK_CurrentNoInfo.HasErrors()
				&& Parent.AK_AutoPrintCheque
				&& Parent.AK_CurrentNoInfo.HasChanges
				&& (ZDecimal)Parent.AK_CurrentNoInfo.Value < (ZDecimal)Parent.AK_CurrentNoInfo.OriginalValue)
			{
				Parent.AK_CurrentNoInfo.AddError(Res.GetString("6e439f9c-5300-49bd-8f4d-3428fdbdc1d5"
					, "This cheque book is configured as auto cheque and you can only change the current number to a greater number. Current No: {0}", Parent.AK_CurrentNoInfo.OriginalValue));
			}

			if (Parent.BankAccount != null)
			{
				ValidationHelper.ValidateChequeDigits(Parent.AK_CurrentNoInfo, Parent.BankAccount.AB_ChequeNumDigits);
			}
		}

		protected override void CheckAK_SQ()
		{
			base.CheckAK_SQ();

			CheckSecurity(Parent.AK_SQInfo, Env.Security.ChequeBooksModifyAutoPrintCheckPrinter);

			if (Parent.AK_AutoPrintCheque)
			{
				if (Parent.AK_SQ.IsEmpty)
				{
					Parent.AK_SQInfo.AddError(Res.GetString("02194d14-9c7a-486d-9ded-ffebaa53c7fa", "Please select a Printer"));
				}
				else
				{
					string chequeBookWithTheSamePrinter = GetChequeBookWithTheSamePrinter(Parent.AK_SQ);
					if (!string.IsNullOrEmpty(chequeBookWithTheSamePrinter))
					{
						Parent.AK_SQInfo.AddWarning(AccChequeBook.WarningSamePrinterMessageStart + chequeBookWithTheSamePrinter + AccChequeBook.WarningSamePrinterMessageEnd);
					}
				}
			}
		}

		string GetChequeBookWithTheSamePrinter(ZGuid printer)
		{
			string result = "";
			AccChequeBook[] booksWithSamePrinter = Parent.Factory.Load<AccChequeBook>(new ZQuery(new ZQuery(AccChequeBookSchema.AK_SQ, printer), new ZQuery(AccChequeBookSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK)));
			foreach (AccChequeBook book in booksWithSamePrinter)
			{
				result = result + "'" + book.AK_Code + "'" + ", ";
			}
			result = result.TrimEnd(", ".ToCharArray());
			return result;
		}

		protected override void CheckAK_AutoPrintCheque()
		{
			base.CheckAK_AutoPrintCheque();

			CheckSecurity(Parent.AK_AutoPrintChequeInfo, Env.Security.ChequeBooksModifyAutoPrintCheck);

			if (Parent.BankAccount != null && Parent.BankAccount.ChequeTemplate == null && Parent.AK_AutoPrintCheque)
			{
				Parent.AK_AutoPrintChequeInfo.AddWarning(Res.GetString("f9187fbb-58ed-4e26-a992-04d9a7f11b35", "Check Auto Printing will be disabled for current Bank Account.\r\nTo enable the Auto Printing, select a Check Template for Bank Account."));
			}
		}

		void CheckSecurity(ZPropertyInfo propertyInfo, SecurityCheckpoint securityCheckpoint)
		{
			if (!propertyInfo.HasErrors())
			{
				if (!securityCheckpoint.IsAllowed && !propertyInfo.Value.Equals(propertyInfo.OriginalValue))
				{
					propertyInfo.AddError(securityCheckpoint.ErrorMessageForNotAllowed);
				}
			}
		}
	}
}
