using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLAccountDescriptorValidationHelper : GLAccountCommonValidationHelper
	{
		protected readonly ZString Language;
		protected readonly ZString CountryCode;

		public AccGLAccountDescriptorValidationHelper(IGLAccount gLAccount, ZString language, ZString countryCode, BusinessObjectFactory factory)
			: base(gLAccount, factory)
		{
			this.Language = language;
			CountryCode = countryCode;
		}

		public bool IsCFWAlreadyDefinedForThisLanguage(ZString language)
		{
			return IsAccountTypeAlreadyUsedForThisLanguage(AccountTypeComboBoxConstants.CarriedForwardAccount, language);
		}

		public bool ColumnReferredByMoreThanOneAccount(SchemaGuidColumn columnToCheck, ZGuid referredPK)
		{
			ZQuery filter = new ZQuery(columnToCheck, referredPK);
			filter.AddToFilter(AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, GLAccount.PK);
			//BusinessObject Result = Factory.LoadTop1(GetBusinessObjectType(), Filter);
			BusinessObject result = Factory.LoadTop1(GLAccount.GetType(), filter);

			return (result != null);
		}

		protected bool IsAccountTypeAlreadyUsedForThisLanguage(ZString reportCategoryInfo, ZString language)
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_ReportCategory, reportCategoryInfo);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, language);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, CountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.PK, SQLComparisonOperator.NotEqual, GLAccount.PK);

			AccGLAccountDescriptor account = Factory.LoadTop1(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor;

			return (account != null && account.PK.IsValid);
		}

		protected override void ValidateUniqueAccountNumber()
		{
			CheckAccountNumberAlreadyUsed(Language);
		}

		protected override bool ShouldValidateInvalidFormat
		{
			get { return false; }
		}

		protected ZString[] GetGLLocalNumberFormatMask()
		{
			ZString[] format = Array.Empty<ZString>();
			GLLocalNumberFormat glLocalNumberFormat = GLLocalNumberFormatRegistryValue();
			if (glLocalNumberFormat == null)
			{
				return format;
			}

			ZString[] registryFormat = glLocalNumberFormat.NumberFormat.Split(new[] { '-' });
			Array.Resize(ref format, registryFormat.Length);

			ZString newExpression = "{0}";
			ZString numberExpression = ZString.Empty;
			int cnt = 0;

			foreach (var variable in registryFormat)
			{
				ZInt i = ConvertToZInt(variable);
				for (int j = 0; j < i; j++)
				{
					numberExpression = numberExpression + "X";
				}

				format[cnt] = ZString.Format(newExpression, numberExpression);
				cnt++;
			}

			return format;
		}

		public override void ValidateAccountNumber()
		{
			base.ValidateAccountNumber();
			ValidateFormat();
		}

		void ValidateFormat()
		{
			if (!AccountNumInfo.HasErrors())
			{
				GLLocalNumberFormat glLocalNumberFormat = GLLocalNumberFormatRegistryValue();

				if (glLocalNumberFormat != null)
				{
					ZString msg;
					ZString[] masks = GetGLLocalNumberFormatMask();
					if (glLocalNumberFormat.IsFixedLength)
					{
						msg = Res.GetString("8F764497-A709-48F1-B917-29E21160927A", "in the following Length: [{0}]", masks[masks.Length - 1].Length);
					}
					else
					{
						ZStringBuilder builder = new ZStringBuilder();
						foreach (ZString t in masks)
						{
							builder.Append(t.Length.ToString());
						}
						msg = Res.GetString("CB2BB011-323F-4309-9D20-E14CFE4C6EA5", "in the following Length: [{0}]", builder.ToStringWithDelimiterBetweenAppends((NoResString)" or "));
					}

					if (!AccountNumber.IsNumbersOnlyOrEmpty)
					{
						AccountNumInfo.AddError(Res.GetString("9077025A-F0A0-49F4-9F01-C42BAA80D279", "GL Account Number invalid.  Please only enter numeric value 0 to 9. and {0}", msg));
					}
					else
					{
						msg = Res.GetString("6277C389-F493-4683-A033-A37A0D46AF2B", "GL Account Number Length invalid. Please enter {0}", msg);
						if (glLocalNumberFormat.IsFixedLength)
						{
							if (masks[masks.Length - 1].Length != AccountNumber.Length)
							{
								AccountNumInfo.AddError(msg);
							}
						}
						else
						{
							if (!masks.Any(mask => mask.Length == AccountNumber.Length))
							{
								AccountNumInfo.AddError(msg);
							}
						}
					}
				}
			}
		}

		GLLocalNumberFormat GLLocalNumberFormatRegistryValue()
		{
			return AccountingMasterFilesRegistry.Instance.LocalNumberFormats.Value.GetGLLocalNumberFormat(Language, CountryCode);
		}

		protected void CheckAccountNumberAlreadyUsed(ZString aJ_Language)
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, aJ_Language);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, CountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, AccountNumber);

			var matchingDescriptors = Factory.Load<AccGLAccountDescriptor>(filter);
			if (matchingDescriptors.Any() && !AccountNumInfo.HasErrors())
			{
				var gLHeaderPKs = (from AccGLAccountDescriptor gLDesc in matchingDescriptors where gLDesc.AJ_ReportType == AccGLAccountDescriptor.ReportTypeCOA select gLDesc.PK).Distinct();
				if (gLHeaderPKs.Count() > 1)
				{
					AccountNumInfo.AddError(Res.GetString("97c07425-efb7-4f40-b894-9d32db513a68", "Please enter another account number as this one is already used by another Local account"));
				}
			}
		}
	}
}
