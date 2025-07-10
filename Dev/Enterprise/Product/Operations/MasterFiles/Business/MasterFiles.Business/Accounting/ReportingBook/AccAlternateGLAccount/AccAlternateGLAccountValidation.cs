using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountValidation : AutoAccAlternateGLAccountValidation
	{
		public AccAlternateGLAccountValidation(AutoAccAlternateGLAccount parent) : base(parent)
		{
		}

		protected override void CheckAGA_AccountNum()
		{
			base.CheckAGA_AccountNum();
			if (!Parent.IsDeleted && !Parent.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.AGA_AccountNumInfo);

				if (Parent.AlternateChart != null)
				{
					CheckAccountNumLength();
					CheckAccountNumFormat();
					CheckDuplicateAccountNum();
					CheckRUPGRPAccountNum();
					CheckParentRelatedValue();
				}
			}
		}

		void CheckAccountNumLength()
		{
			var formats = Parent.AlternateChart.AlternateChartFormats.Cast<AccAlternateChartFormat>().OrderBy(x => x.ANF_Tier);

			if (Parent.AlternateChart.AAC_IsFixedLength)
			{
				var length = formats.Sum(x => x.ANF_Format.Length);
				if (Parent.AGA_AccountNum.Length != length)
				{
					Parent.AGA_AccountNumInfo.AddError(Res.GetString("11C3A8E6-743E-427D-92CC-C9BA9AAA24B9", "The length of Account Number is invalid. Please enter the number with {0} characters.", length));
				}
			}
			else
			{
				var totalLengthForEachTier = new List<int>();
				foreach (var format in formats)
				{
					totalLengthForEachTier.Add(totalLengthForEachTier.LastOrDefault() + format.ANF_Format.Length);
				}

				if (!totalLengthForEachTier.Contains(Parent.AGA_AccountNum.Length))
				{
					var eachTierLengthMessage = string.Join((NoResString)", or ", totalLengthForEachTier);
					Parent.AGA_AccountNumInfo.AddError(Res.GetString("F0360933-C0C9-4E3C-B010-D6EDC723BBC5", "The length of Account Number is invalid. Please enter the number with {0} characters.", eachTierLengthMessage));
				}
			}
		}

		void CheckAccountNumFormat()
		{
			if (!Parent.AGA_AccountNumInfo.HasErrors())
			{
				var index = 0;
				var formats = Parent.AlternateChart.AlternateChartFormats.Cast<AccAlternateChartFormat>().OrderBy(x => x.ANF_Tier);
				foreach (var format in formats.Select(x => x.ANF_Format))
				{
					if (Parent.AGA_AccountNum.Length >= index + format.Length && !Regex.IsMatch(Parent.AGA_AccountNum.Substring(index, format.Length), "^[0]*$"))
					{
						for (var i = 0; i < format.Length; i++)
						{
							if ((format[i] == AlternateGLAccountFormatType.AlphabatFormat && !Regex.IsMatch(Parent.AGA_AccountNum[index + i].ToString(), "^[a-zA-Z]*$"))
							|| (format[i] == AlternateGLAccountFormatType.NumberFormat && !Regex.IsMatch(Parent.AGA_AccountNum[index + i].ToString(), "^[0-9]*$")))
							{
								var joinFormat = string.Join(string.Empty, formats.Select(x => x.ANF_Format)).Substring(0, Parent.AGA_AccountNum.Length);
								Parent.AGA_AccountNumInfo.AddError(Res.GetString("630F2D0D-3232-4457-AAB1-BE0BAABF29F4", "The Account Number does not matched up with the Account Format '{0}' which is set in Chart {1}.", joinFormat, Parent.AlternateChart.AAC_Code));
								break;
							}
						}
					}

					if (Parent.AGA_AccountNumInfo.HasErrors())
					{
						break;
					}

					index += format.Length;
				}
			}
		}

		void CheckDuplicateAccountNum()
		{
			if (!Parent.AGA_AccountNumInfo.HasErrors())
			{
				var query = new ZQuery();
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, Parent.AGA_AAC_AlternateChart);
				query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, Parent.AGA_AccountNum);
				query.AddToFilter(AccAlternateGLAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Load<AccAlternateGLAccount>(query).Any(x => !x.IsDeleted))
				{
					Parent.AGA_AccountNumInfo.AddError(Res.GetString("69446C95-2103-4BD5-A17C-C694649E8AA1", "Please select another Account Number as this one is already used."));
				}
			}
		}

		void CheckRUPGRPAccountNum()
		{
			var groupAndRollupList = new List<string>()
			{
				Core.Constants.AccountType.Rollup,
				Core.Constants.AccountType.Group
			};

			if (!Parent.AGA_AccountNumInfo.HasErrors()
				&& groupAndRollupList.Contains(Parent.AGA_AccountType)
				&& !string.IsNullOrEmpty(Parent.AGA_AccountNum)
				&& Parent.AlternateChart != null)
			{
				if (!Parent.HasChildAccounts())
				{
					Parent.AGA_AccountNumInfo.AddError(Res.GetString("E41BC751-52EC-453E-9B1A-F1239BC8337E", "The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));
				}
			}
		}

		void CheckParentRelatedValue()
		{
			if (!Parent.AGA_AccountNumInfo.HasErrors())
			{
				var attributes = Parent.AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>();
				var currentGLAccount = attributes.FirstOrDefault(x => !x.IsInDatabase || x.HasChanges)?.GLHeader;
				if (currentGLAccount != null)
				{
					if (!Parent.AGA_AccountNumInfo.HasErrors())
					{
						var attributeRelatedToOtherGLHeader = attributes.FirstOrDefault(x => x.AAA_AGA_AlternateGLAccount == Parent.PK && x.AAA_AG_GLHeader != currentGLAccount.PK);
						if (attributeRelatedToOtherGLHeader != null && (attributeRelatedToOtherGLHeader.GLHeader.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(p => p.ADC_AAC_AlternateChart == Parent.AGA_AAC_AlternateChart) || currentGLAccount.AlternateGLAccountDissections.Cast<AccAlternateGLAccountDissection>().Any(p => p.ADC_AAC_AlternateChart == Parent.AGA_AAC_AlternateChart)))
						{
							Parent.AGA_AccountNumInfo.AddError(Res.GetString("854DFFBA-FB31-476B-BFF6-4DA71EB831E1", @"The specified Alternate Account's Number '{0}' already existed in the Alternate Chart '{1}' and mapped to Parent Account '{2}'.
It cannot be mapped to a different Parent Account.", Parent.AGA_AccountNum, Parent.AlternateChart.AAC_Code, attributeRelatedToOtherGLHeader.GLHeader.AccountNum));
						}
					}

					if (!Parent.AGA_AccountNumInfo.HasErrors())
					{
						if (Parent.AGA_AccountType == Core.Constants.AccountType.Note && attributes.Any(x => x.AAA_AGA_AlternateGLAccount == Parent.PK && x.AAA_AG_GLHeader != currentGLAccount.PK))
						{
							Parent.AGA_AccountNumInfo.AddError(Res.GetString("FF6EAE58-1F94-40AF-9F47-3363DA55A9D3", @"The specified Alternate Account's Number '{0}' already existed and cannot be linked to a different Parent Account as NTE Alternate GL Account cannot be mapped from multiple NTE Parent Accounts.
Please enter a different value.", Parent.AGA_AccountNum));
						}
					}

					if (!Parent.AGA_AccountNumInfo.HasErrors())
					{
						if (attributes.Any(x => x.GLHeader.AG_CashFlowType != Parent.CashFlowType))
						{
							Parent.AGA_AccountNumInfo.AddError(Res.GetString("9CD20681-4311-4759-99FE-BCEB5223F41E", @"The specified Alternate Account's Number '{0}' already existed in the Alternate Chart '{1}' and cannot be linked to a different Parent Account as there is a mis-match in the Cash Flow Cat.
Please enter a different value.", Parent.AGA_AccountNum, Parent.AlternateChart.AAC_Code));
						}
					}

					if (!Parent.AGA_AccountNumInfo.HasErrors())
					{
						var glHeaders = attributes.Select(x => x.AAA_AG_GLHeader).ToHashSet();
						var accounting = ObjectFactory.Get<IAccounting>();
						if (glHeaders.Count > 1 && (glHeaders.Any(glheader => accounting.IsNotAllowedForDissectionAttributes(glheader) || accounting.IsNotAllowedForSeparateNumbering(glheader) || AccountingMasterFilesUtils.IsNotAllowedForDissectionControlAccount(glheader.ToGuid())) || currentGLAccount.Factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, glHeaders)).Any(x => x.IsBankAccount())))
						{
							Parent.AGA_AccountNumInfo.AddError(Res.GetString("BF1586C9-477A-439B-B59F-351DCBD040D5", @"The specified Alternate Account's Number '{0}' already existed in the Alternate Chart '{1}' and is linked to Parent Account used in the system registries or bank accounts that do not allow dissection. It cannot be mapped to multiple Parent Accounts.
Please enter a different value.", Parent.AGA_AccountNum, Parent.AlternateChart.AAC_Code));
						}
					}
				}
			}
		}

		protected override void CheckAGA_Description()
		{
			if (!Parent.ReadOnly)
			{
				base.CheckAGA_Description();
				MandatoryValidation.CheckEntered(Parent.AGA_DescriptionInfo);
			}
		}

		string MustBeSameChartErrorMessage(string selectedAlternateGLAccountNum, string selectedAlternateGLAccountDesc, string selectedAlternateGLAccountChartCode, string selectedAlternateGLAccountChartDesc, string currentChartCode, string currentChartDesc, ZPropertyInfo fieldInfo)
		{
			return Res.GetString("FEAFC3F3-83B9-4459-8549-D097834632F8", "The alternate account '{0} - {1}' cannot be chosen here as it belongs to a different chart '{2} - {3}', please choose another '{4}' alternate account from the same chart '{5} - {6}'.", selectedAlternateGLAccountNum, selectedAlternateGLAccountDesc, selectedAlternateGLAccountChartCode, selectedAlternateGLAccountChartDesc, GetAccountTypeMessage(fieldInfo), currentChartCode, currentChartDesc);
		}

		string GetAccountTypeMessage(ZPropertyInfo fieldInfo)
		{
			var result = "";
			if (fieldInfo.Name == AccAlternateGLAccountSchema.Constants.AGA_AGA_PercentNum)
			{
				result = Core.Constants.AccountType.Consolidation + (NoResString)" or " + Core.Constants.AccountType.Total;
			}
			else if (fieldInfo.Name == AccAlternateGLAccountSchema.Constants.AGA_AGA_ConsolidationNum)
			{
				result = Core.Constants.AccountType.Consolidation;
			}
			else if (fieldInfo.Name == AccAlternateGLAccountSchema.Constants.AGA_AGA_AlternateNum)
			{
				result = Core.Constants.AccountType.Alternate;
			}
			else if (fieldInfo.Name == AccAlternateGLAccountSchema.Constants.AGA_AGA_HeaderDependsOnTotal)
			{
				result = Core.Constants.AccountType.Total;
			}
			return result;
		}

		void CheckMustBeSameChart(AccAlternateGLAccount selectedAlternateGLAccount, ZPropertyInfo fieldInfo, AccAlternateChart currentChart)
		{
			if (selectedAlternateGLAccount.AGA_AAC_AlternateChart != currentChart.PK)
			{
				var selectedAlternateGLAccountChart = selectedAlternateGLAccount.AlternateChart;
				fieldInfo.AddError(MustBeSameChartErrorMessage(selectedAlternateGLAccount.AGA_AccountNum, selectedAlternateGLAccount.AGA_Description, selectedAlternateGLAccountChart.AAC_Code, selectedAlternateGLAccountChart.AAC_Description, currentChart.AAC_Code, currentChart.AAC_Description, fieldInfo));
			}
		}

		protected override void CheckAGA_AGA_PercentNum()
		{
			base.CheckAGA_AGA_PercentNum();
			if (!Parent.ReadOnly && !Parent.AGA_AGA_PercentNumInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AGA_AGA_PercentNumInfo, ResString.GetMultilingualString("0B1814CC-9A10-4B23-9D4B-355208515A79", "Invalid Percent Account. A valid Percent Account must be 'CLN' or 'TTL' account type."));

				if (!Parent.AGA_AGA_PercentNumInfo.HasErrors() && !Parent.AGA_AGA_PercentNum.IsEmpty && !Parent.AGA_AAC_AlternateChart.IsEmpty)
				{
					CheckMustBeSameChart(Parent.PercentNum, Parent.AGA_AGA_PercentNumInfo, Parent.AlternateChart);
				}
			}
		}

		protected override void CheckAGA_AGA_AlternateNum()
		{
			base.CheckAGA_AGA_AlternateNum();

			if (!Parent.ReadOnly && !Parent.AGA_AGA_AlternateNumInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AGA_AGA_AlternateNumInfo, ResString.GetMultilingualString("78D2AD7C-99DF-4211-8F3B-AE7522E01505", "Invalid Alternate Account. A valid Alternate Account must be 'ALT' account type."));

				if (!Parent.AGA_AGA_AlternateNum.IsEmpty)
				{
					if (!Parent.AGA_AGA_AlternateNumInfo.HasErrors())
					{
						var factory = new BusinessObjectFactory();
						var query = new ZQuery();
						query.AddToFilter(AccAlternateGLAccountSchema.AGA_AGA_AlternateNum, Parent.AGA_AGA_AlternateNum);
						query.AddToFilter(AccAlternateGLAccountSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
						var alternateGLAccountWIthSameAlternateAccount = factory.LoadTop1<AccAlternateGLAccount>(query);

						if (alternateGLAccountWIthSameAlternateAccount != null)
						{
							Parent.AGA_AGA_AlternateNumInfo.AddError(Res.GetString("B93BED0F-8F43-4AF6-B035-3460D742AE31", "The Alternate Number is already used by another Alternate GL account {0}.", alternateGLAccountWIthSameAlternateAccount.AGA_AccountNum));
						}
					}
					if (!Parent.AGA_AGA_AlternateNumInfo.HasErrors() && !Parent.AGA_AAC_AlternateChart.IsEmpty)
					{
						CheckMustBeSameChart(Parent.AlternateNum, Parent.AGA_AGA_AlternateNumInfo, Parent.AlternateChart);
					}
				}
			}
		}

		protected override void CheckAGA_AGA_ConsolidationNum()
		{
			base.CheckAGA_AGA_ConsolidationNum();

			if (!Parent.ReadOnly && !Parent.AGA_AGA_ConsolidationNumInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AGA_AGA_ConsolidationNumInfo, ResString.GetMultilingualString("01954C20-60D6-4731-B4A3-0DA48F959E4C", "Invalid Consolidate Account. A valid Consolidate Account must be 'CLN' account type."));
				if (!Parent.AGA_AGA_ConsolidationNumInfo.HasErrors() && !Parent.AGA_AGA_ConsolidationNum.IsEmpty && !Parent.AGA_AAC_AlternateChart.IsEmpty)
				{
					CheckMustBeSameChart(Parent.ConsolidationNum, Parent.AGA_AGA_ConsolidationNumInfo, Parent.AlternateChart);
				}
			}
		}

		protected override void CheckAGA_AGA_HeaderDependsOnTotal()
		{
			base.CheckAGA_AGA_HeaderDependsOnTotal();

			if (!Parent.ReadOnly && !Parent.AGA_AGA_HeaderDependsOnTotalInfo.ReadOnly)
			{
				ListValidation.ErrorIfInvalidPK(Parent.AGA_AGA_HeaderDependsOnTotalInfo, ResString.GetMultilingualString("4D1307BD-2226-4F72-A142-53C2339BBA13", "Invalid Total Reference. A valid Total Reference must be 'TTL' account type."));
				if (!Parent.AGA_AGA_HeaderDependsOnTotalInfo.HasErrors() && !Parent.AGA_AGA_HeaderDependsOnTotal.IsEmpty && !Parent.AGA_AAC_AlternateChart.IsEmpty)
				{
					CheckMustBeSameChart(Parent.HeaderDependsOnTotal, Parent.AGA_AGA_HeaderDependsOnTotalInfo, Parent.AlternateChart);
				}
			}
		}

		protected override void CheckAGA_TotalLevel()
		{
			if (!Parent.ReadOnly && !Parent.AGA_TotalLevelInfo.ReadOnly)
			{
				if (Parent.AGA_TotalLevel < 1 || Parent.AGA_TotalLevel > 999)
				{
					Parent.AGA_TotalLevelInfo.AddError(Res.GetString("23AD0B85-6822-4ADB-9464-D3BCB74199E7", "Total Level should be between 1 and 999."));
				}
			}
		}

		protected override void CheckAGA_PrintSequence()
		{
			if (!Parent.ReadOnly)
			{
				if (Parent.AGA_PrintSequence < 0 || Parent.AGA_PrintSequence > 999)
				{
					Parent.AGA_PrintSequenceInfo.AddError(Res.GetString("9610354E-C8A3-4DE6-AE0C-CFC45B0B9F69", "Print Sequence should be between 0 and 999."));
				}
			}
		}

		protected override void CheckAGA_AccountType()
		{
			if (!Parent.ReadOnly)
			{
				base.CheckAGA_AccountType();
				MandatoryValidation.CheckEntered(Parent.AGA_AccountTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AGA_AccountTypeInfo);
			}
		}

		protected override void CheckAGA_DebitCredit()
		{
			if (!Parent.ReadOnly)
			{
				base.CheckAGA_DebitCredit();
				MandatoryValidation.CheckEntered(Parent.AGA_DebitCreditInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AGA_DebitCreditInfo);
			}
		}

		protected override void CheckAGA_ReportSection()
		{
			if (!Parent.ReadOnly)
			{
				base.CheckAGA_ReportSection();
				MandatoryValidation.CheckEntered(Parent.AGA_ReportSectionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.AGA_ReportSectionInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
		}

		protected new AccAlternateGLAccount Parent => (AccAlternateGLAccount)base.Parent;
	}
}
