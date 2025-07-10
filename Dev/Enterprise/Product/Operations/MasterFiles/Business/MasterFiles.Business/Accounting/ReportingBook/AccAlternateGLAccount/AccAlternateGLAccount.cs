using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AccAlternateGLAccount.Schema.AGA_AccountNum), DescriptionProperty(AccAlternateGLAccount.Schema.AGA_Description)]
	public class AccAlternateGLAccount : AutoAccAlternateGLAccount, IAuditParent
	{
		public AccAlternateGLAccount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region AccountNum With Separator

		public ZString AccountNumWithSeparator
		{
			get
			{
				var result = string.Empty;
				if (!AGA_AccountNumInfo.HasErrors() && AlternateChart != null)
				{
					var index = 0;
					foreach (var format in AlternateChart.AlternateChartFormats.Cast<AccAlternateChartFormat>().OrderBy(x => x.ANF_Tier))
					{
						if (AGA_AccountNum.Length >= format.ANF_Format.Length + index)
						{
							result += AGA_AccountNum.Substring(index, format.ANF_Format.Length);
							if (index + format.ANF_Format.Length < AGA_AccountNum.Length)
							{
								result += format.ANF_Separator;
							}
							index += format.ANF_Format.Length;
						}
						else
						{
							break;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region AccountType

		[List("Lookups.AccountTypeList")]
		public override ZString AGA_AccountType
		{
			get => base.AGA_AccountType;
			set
			{
				base.AGA_AccountType = value;
			}
		}

		public List<string> AccountTypeWithoutReferenceAlteranteGLAccountForPercentNum => new List<string>()
		{
			Core.Constants.AccountType.Header,
			Core.Constants.AccountType.Note,
			Core.Constants.AccountType.Rollup,
			Core.Constants.AccountType.Group,
			Core.Constants.AccountType.ChartOnly,
			Core.Constants.AccountType.Total,
			Core.Constants.AccountType.Consolidation
		};

		public List<string> AccountTypeWithoutReferenceAlteranteGLAccountForConsolidationNum => new List<string>()
		{
			Core.Constants.AccountType.Header,
			Core.Constants.AccountType.Note,
			Core.Constants.AccountType.Rollup,
			Core.Constants.AccountType.Group,
			Core.Constants.AccountType.ChartOnly,
			Core.Constants.AccountType.Consolidation
		};

		public List<string> BSHPAndLAccountType => new List<string>()
		{
			Core.Constants.AccountType.BalanceSheetAccount,
			Core.Constants.AccountType.ProfitAndLossAccount,
		};

		#endregion

		#region DebitCredit

		[List("Lookups.DebitCreditList")]
		public override ZString AGA_DebitCredit { get => base.AGA_DebitCredit; set => base.AGA_DebitCredit = value; }

		#endregion

		#region Report Section

		[List("Lookups.ReportSectionList")]
		public override ZString AGA_ReportSection { get => base.AGA_ReportSection; set => base.AGA_ReportSection = value; }

		#endregion

		#region Units

		[List("Lookups.StatisticalUnitsList")]
		public ZString StatisticalUnits
		{
			get => AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>()?.FirstOrDefault()?.GLHeader?.AG_StatisticalUnits ?? ZString.Empty;
		}

		#endregion

		#region CashFlow Type

		[List("Lookups.CashFlowTypeList")]
		public ZString CashFlowType { get => AlternateGLAccountAttributes.Cast<AccAlternateGLAccountAttribute>()?.FirstOrDefault()?.GLHeader?.AG_CashFlowType ?? ZString.Empty; }

		#endregion

		#region AlternateGLAccountAttributes

		[ChildEditable(true)]
		public AccAlternateGLAccountAttributeDependentCollection AlternateGLAccountAttributes
		{
			get
			{
				if (alternateGLAccountAttributes == null)
				{
					alternateGLAccountAttributes = new AccAlternateGLAccountAttributeDependentCollection(this);
					alternateGLAccountAttributes.Load();
					RegisterEditableChildObject(alternateGLAccountAttributes);
				}
				return alternateGLAccountAttributes;
			}
		}
		AccAlternateGLAccountAttributeDependentCollection alternateGLAccountAttributes;

		#endregion

		#region Prefixed A/C Num

		public ZString PrefixedAccountNum => SectionPrefix + "." + AccountNumWithSeparator;

		public ZInt SectionPrefix
		{
			get { return SectionPrefixes.ContainsKey(AGA_ReportSection) ? SectionPrefixes[AGA_ReportSection] : ZInt.Zero; }
		}

		Dictionary<ZString, ZInt> SectionPrefixes
		{
			get
			{
				var fSectionPrefixes = new Dictionary<ZString, ZInt>();
				fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 1);
				fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.Overheads, 2);
				fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation, 3);
				fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity, 4);

				if (AlternateChart != null && AlternateChart.AAC_BalanceSheetStyle == AccAlternateChartLookups.BalanceSheetStyleCode.ELA)
				{
					fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, 5);
					fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.Assets, 6);
				}
				else
				{
					fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.Assets, 5);
					fSectionPrefixes.Add(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, 6);
				}

				return fSectionPrefixes;
			}
		}

		#endregion

		#region AGA_AGA_PercentNum

		protected bool AGA_AGA_PercentNum_ReadOnly
		{
			get { return AccountTypeWithoutReferenceAlteranteGLAccountForPercentNum.Contains(AGA_AccountType); }
		}

		#endregion

		#region AGA_AGA_ConsolidationNum

		protected bool AGA_AGA_ConsolidationNum_ReadOnly
		{
			get { return AccountTypeWithoutReferenceAlteranteGLAccountForConsolidationNum.Contains(AGA_AccountType); }
		}

		#endregion

		#region AGA_AGA_AlternateNum

		protected bool AGA_AGA_AlternateNum_ReadOnly
		{
			get { return AGA_AccountType != Core.Constants.AccountType.BalanceSheetAccount; }
		}

		#endregion

		#region AGA_AGA_HeaderDependsOnTotal

		protected bool AGA_AGA_HeaderDependsOnTotal_ReadOnly
		{
			get { return AGA_AccountType != Core.Constants.AccountType.Header; }
		}

		#endregion

		#region AGA_TotalLevel

		protected bool AGA_TotalLevel_ReadOnly
		{
			get { return AGA_AccountType != Core.Constants.AccountType.Total; }
		}

		#endregion

		#region Company

		public ZGuid CompanyPK => AlternateChart?.AAC_GC_Company ?? ZGuid.Empty;

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore => $"{AGA_AccountNum} - {AGA_Description}";

		#endregion

		public override void Delete()
		{
			AlternateGLAccountAttributes.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete => ReasonForNotAbleToDelete.IsEmpty;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				var areAlternateAccountUsed = new List<MultilingualString>();
				var useAsAlternateNumMessage = CheckAlternateAccountIsUsed(AccAlternateGLAccountSchema.AGA_AGA_AlternateNum, ResString.GetMultilingualString("5B30F817-3434-427E-8796-21CFEED2C4BC", "Alternate Number"));
				if (!useAsAlternateNumMessage.IsEmpty)
				{
					areAlternateAccountUsed.Add(useAsAlternateNumMessage);
				}

				var useAsConsolidateMessage = CheckAlternateAccountIsUsed(AccAlternateGLAccountSchema.AGA_AGA_ConsolidationNum, ResString.GetMultilingualString("F73F3E6B-C9E4-48ED-AA37-6F7503968998", "Consolidate"));
				if (!useAsConsolidateMessage.IsEmpty)
				{
					areAlternateAccountUsed.Add(useAsConsolidateMessage);
				}

				var useAsPercentNumMessage = CheckAlternateAccountIsUsed(AccAlternateGLAccountSchema.AGA_AGA_PercentNum, ResString.GetMultilingualString("3D327B06-05D2-4EE9-8953-AB84E5BDCD18", "Percent Number"));
				if (!useAsPercentNumMessage.IsEmpty)
				{
					areAlternateAccountUsed.Add(useAsPercentNumMessage);
				}

				var useAsTotalReferenceMessage = CheckAlternateAccountIsUsed(AccAlternateGLAccountSchema.AGA_AGA_HeaderDependsOnTotal, ResString.GetMultilingualString("6F61ED2B-56D7-4399-B52B-90CBB8F9E268", "Total Reference"));
				if (!useAsTotalReferenceMessage.IsEmpty)
				{
					areAlternateAccountUsed.Add(useAsTotalReferenceMessage);
				}

				if (areAlternateAccountUsed.Any())
				{
					result = MultilingualString.Join("\r\n", areAlternateAccountUsed.ToArray());
				}

				return result;
			}
		}

		MultilingualString CheckAlternateAccountIsUsed(SchemaColumn usedColumnSchemaName, string usedColumnReadableName)
		{
			MultilingualString result = (NoResString)string.Empty;
			var readOnlyFactory = new ReadOnlyBusinessObjectFactory();
			var alternateAccountUsedIn = readOnlyFactory.Load<AccAlternateGLAccount>(new ZQuery(usedColumnSchemaName, PK));

			if (alternateAccountUsedIn.Any())
			{
				var alternateAccountUseInList = string.Join(", ", alternateAccountUsedIn.Select(x => x.AGA_AccountNum));
				result = ResString.GetMultilingualString("C6FB4CB4-9D63-44E1-83C3-229235C354AE", "The Alternate Account {0} cannot be deleted because it is used as {1} in Alternate Account: {2}.", AGA_AccountNum, usedColumnReadableName, alternateAccountUseInList);
			}

			return result;
		}

		public override void OnSaving()
		{
			base.OnSaving();
		}

		public bool HasChildAccounts()
		{
			if (AlternateChart == null || string.IsNullOrEmpty(AGA_AccountNum))
			{
				return false;
			}

			var accountNumWithoutZero = GetAccountNumWithoutZeroCore();

			var factory = new ReadOnlyBusinessObjectFactory();
			var query = new ZQuery();
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AAC_AlternateChart, AlternateChart.PK);
			query.AddToFilter(AccAlternateGLAccountSchema.AGA_AccountNum, SQLComparisonOperator.StartsWith, accountNumWithoutZero);
			query.AddToFilter(AccAlternateGLAccountSchema.PK, SQLComparisonOperator.NotEqual, PK);
			return factory.Exists(typeof(AccAlternateGLAccount), query);
		}

		IOrderedEnumerable<AccAlternateChartFormat> GetOrderedChartFormatsCore(bool asc = true)
		{
			var formats = AlternateChart.AlternateChartFormats.Cast<AccAlternateChartFormat>();
			return asc ? formats.OrderBy(x => x.ANF_Tier) : formats.OrderByDescending(x => x.ANF_Tier);
		}

		ZString GetAccountNumWithoutZeroCore()
		{
			var descOrderFormats = GetOrderedChartFormatsCore(false);
			var accountNumWithoutZero = AGA_AccountNum;
			var length = descOrderFormats.Sum(x => x.ANF_Format.Length);

			if (Regex.IsMatch(accountNumWithoutZero, "^[0]*$"))
			{
				return accountNumWithoutZero.Substring(0, GetOrderedChartFormatsCore().First().ANF_Format.Length);
			}

			foreach (var format in descOrderFormats)
			{
				if (accountNumWithoutZero.Length == length)
				{
					if (Regex.IsMatch(accountNumWithoutZero.Substring(length - format.ANF_Format.Length, format.ANF_Format.Length), "^[0]*$"))
					{
						accountNumWithoutZero = accountNumWithoutZero.Substring(0, length - format.ANF_Format.Length);
					}
					else
					{
						break;
					}
				}
				length -= format.ANF_Format.Length;
			}

			return accountNumWithoutZero;
		}

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount, null);
			}
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			var chart = new BusinessObjectFactory().NewWithValidTestData<AccAlternateChart>();
			chart.Factory.Save();

			this.AGA_AAC_AlternateChart = chart.PK;
			this.AGA_AccountNum = ZGuid.NewZGuid().ToString().Substring(0, 4);
			this.AGA_Description = "desc";
			this.AGA_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			this.AGA_DebitCredit = "DR";
			this.AGA_TotalLevel = 0;
			this.AGA_ReportSection = "OV";
			this.AGA_PrintSequence = 1;
		}
#endif
		#endregion
	}
}
