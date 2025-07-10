using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GLAccountCommonValidationHelper
	{
		protected GLAccountCommonValidationHelper(IGLAccount gLAccount, BusinessObjectFactory factory)
		{
			this.GLAccount = gLAccount;
			this.Factory = factory;
		}

		public ZPropertyInfo ControlAccountInfo
		{
			get { return GLAccount.ControlAccountInfo; }
		}

		public ZString GLAccountFormat
		{
			get { return GLAccount.GLAccountFormat; }
		}

		public ZPropertyInfo PercentNumInfo
		{
			get { return GLAccount.PercentNumInfo; }
		}

		public ZGuid ConsolidationAccount
		{
			get { return GLAccount.ConsolidationAccount; }
		}

		public ZPropertyInfo ConsolidationNumInfo
		{
			get { return GLAccount.ConsolidationNumInfo; }
		}

		public ZPropertyInfo AlternateNumInfo
		{
			get { return GLAccount.AlternateNumInfo; }
		}

		public ZPropertyInfo TotalLevelInfo
		{
			get { return GLAccount.TotalLevelInfo; }
		}

		public ZPropertyInfo HeaderDependsOnTotalInfo
		{
			get { return GLAccount.HeaderDependsOnTotalInfo; }
		}

		public ZPropertyInfo CarriedForwardInfo
		{
			get { return GLAccount.CarriedForwardInfo; }
		}

		public ZString AccountNumber
		{
			get { return GLAccount.AccountNum; }
		}

		public ZString AccountNumberWithPrefix
		{
			get { return GLAccount.AccountNumWithPrefix; }
		}

		public ZPropertyInfo AccountNumInfo
		{
			get { return GLAccount.AccountNumInfo; }
		}

		public ZGuid PK
		{
			get { return GLAccount.PK; }
		}

		public ZGuid TotalReferenceAccount
		{
			get { return GLAccount.TotalReferenceAccount; }
		}

		public virtual void ValidateAccountNumber()
		{
			ValidateUniqueAccountNumber();
			if (ShouldValidateInvalidFormat)
			{
				ValidateInvalidFormat();
			}
			if (ShouldEnsureConsolidationAccountNumGreaterThanAccountNum)
			{
				ValidateConsolidatedAccountNum(AccountNumInfo);
			}
			ValidateTotalReferenceAccountNum(AccountNumInfo);
		}

		protected virtual bool ShouldEnsureConsolidationAccountNumGreaterThanAccountNum
		{
			get { return true; }
		}

		protected virtual bool ShouldValidateInvalidFormat
		{
			get { return true; }
		}

		public void ValidateConsolidatedAccountNum(ZPropertyInfo propertyToPutError)
		{
			ValidateAccountNumComparedToLinkAccount(ConsolidationNumInfo, ConsolidationAccount, propertyToPutError, Res.GetString("7569a225-1399-4caf-8462-53d149d717d8", "Consolidation"));
		}

		public void ValidateTotalReferenceAccountNum(ZPropertyInfo propertyToPutError)
		{
			ValidateAccountNumComparedToLinkAccount(HeaderDependsOnTotalInfo, TotalReferenceAccount, propertyToPutError, Res.GetString("dbd63d94-2f9a-4573-af39-fea75429801d", "Total Reference"));
		}

		public void ValidateAccountNumComparedToLinkAccount(ZPropertyInfo accountInfo, ZGuid accountPK, ZPropertyInfo propertyToPutError, string accountDescription)
		{
			if (!AccountNumInfo.HasErrors() && !accountInfo.HasErrors())
			{
				//IGLAccount GLAccount = (IGLAccount) Factory.Load(GetBusinessObjectType(), AccountPK);
				IGLAccount gLAccountToCompare = (IGLAccount)Factory.Load(this.GLAccount.GetType(), accountPK);
				if (gLAccountToCompare != null)
				{
					ZDecimal accountNum = ConvertToDecimal(AccountNumberWithPrefix);
					ZDecimal numberToCompare = ConvertToDecimal(gLAccountToCompare.AccountNumWithPrefix);

					if (accountNum != -1 && numberToCompare != -1 && accountNum >= numberToCompare)
					{
						propertyToPutError.AddError(Res.GetString("760ac36b-c0c7-4a43-a706-7bdcac391ef7", "Account number must be less than {0} number", accountDescription));
					}
				}
			}
		}

		protected void ValidateInvalidFormat()
		{
			if (!AccountNumInfo.HasErrors())
			{
				string newExpression = "^" + GetMask().Replace("X", "[0-9]").Replace(".", "\\.") + "$";
				Regex r = new Regex(newExpression);

				if (!r.IsMatch(AccountNumber))
				{
					AccountNumInfo.AddError(Res.GetString("8b3388f6-32b6-4d8e-ac9a-ebf1d3c6d5d0", "GL Account Number invalid.  Please enter in the following format: [{0}]", GetMask()));
				}
			}
		}

		protected abstract void ValidateUniqueAccountNumber();

		protected readonly IGLAccount GLAccount;
		protected readonly BusinessObjectFactory Factory;

		protected virtual ZString GetMask()
		{
			return GLAccountFormat;
		}

		public static ZInt ConvertToZInt(ZString str)
		{
			ZInt result;
			if (!ZInt.TryParse(str.Replace(".", ""), out result))
			{
				result = -1;
			}

			return result;
		}

		public static bool CompareTwoAccount(string largerAccount, string smallerAccount)
		{
			return true;
		}

		public static ZDecimal ConvertToDecimal(ZString largerAccount)
		{
			string intPart = "";
			string decimalPart = "";
			ZDecimal result;

			if (!ZDecimal.TryParse(largerAccount, out result))
			{
				int firstDot = largerAccount.IndexOf('.');

				if (firstDot >= 0)
				{
					intPart = largerAccount.Left(firstDot);
					decimalPart = largerAccount.Right(largerAccount.Length - firstDot).Replace(".", "");

					if (!ZDecimal.TryParse(intPart + "." + decimalPart, out result))
					{
						result = -1;
					}
				}
				else
				{
					result = -1;
				}
			}

			return result;
		}
	}
}
