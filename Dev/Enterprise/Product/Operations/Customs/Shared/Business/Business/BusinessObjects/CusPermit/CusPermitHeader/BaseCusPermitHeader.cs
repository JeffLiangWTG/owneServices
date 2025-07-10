using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	[CodeProperty(CusPermitHeaderSchema.Constants.CPH_Number)]
	[DescriptionProperty(CusPermitHeaderSchema.Constants.CPH_Number)]
	public class BaseCusPermitHeader : SharedCusPermitHeader, IBaseCusPermitHeader
	{
		public BaseCusPermitHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly CusPermitHeaderTypeDecider TypeDecider = new CusPermitHeaderTypeDecider();

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BaseCusPermitHeader Load(ZString country, ZString permitNumber, ZGuid permitHolder, ZDateTime transactionDate, string qtyValIndicator = "", string type = "", string subType = "", Dictionary<ZString, ZString> filterRules = null, ZGuid? appliesTo = null)
			{
				BaseCusPermitHeader result = null;
				var cusStatementHeaderFilter = CreateNewCusPermitHeaderFilter(country, permitNumber, permitHolder, transactionDate, qtyValIndicator, type, subType, appliesTo);
				var permitsMatchingQuery = Factory.Load(GetTypeOfBusinessObjectToLoad(), cusStatementHeaderFilter);

				if (filterRules != null)
				{
					foreach (BaseCusPermitHeader permit in permitsMatchingQuery)
					{
						var groups = permit.CusPermitRules?.GroupBy(x => x.CPR_RuleCode);
						if (
							groups?.All(group =>
							{
								var applicableFilterRules = filterRules.Where(x => x.Key == group.Key);
								return group.Any(rule =>
								{
									return applicableFilterRules.Any(x => rule.MatchesValue(x.Value));
								});
							}) ?? false
						)
						{
							result = permit;
						}
					}
				}
				else
				{
					result = permitsMatchingQuery.OfType<BaseCusPermitHeader>().FirstOrDefault();
				}

				return result;
			}

			public BaseCusPermitHeader[] LoadByNumber(ZString countryCode, ZString permitNumber)
			{
				var query = new ZQuery();
				query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCode);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Number, permitNumber);
				return Factory.Load<BaseCusPermitHeader>(query);
			}

			ZQuery CreateNewCusPermitHeaderFilter(ZString country, ZString permitNumber, ZGuid permitHolder, ZDateTime transactionDate, string qtyValIndicator = "", string type = "", string subType = "", ZGuid? appliesTo = null)
			{
				var cusPermitHeaderFilter = new ZQuery();
				cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
				cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
				if (!permitHolder.IsEmpty)
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, permitHolder);
				}
				if (!permitNumber.IsEmpty)
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_Number, permitNumber);
				}
				if (!string.IsNullOrEmpty(qtyValIndicator))
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_QtyValIndicator, qtyValIndicator);
				}
				if (!string.IsNullOrEmpty(type))
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_Type, type);
				}
				if (!string.IsNullOrEmpty(subType))
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_SubType, subType);
				}
				if (appliesTo.HasValue && !appliesTo.Value.IsEmpty)
				{
					cusPermitHeaderFilter.AddToFilter(PermitFindBoxCollection.GetAppliesToQuery(appliesTo.Value));
				}

				if (!transactionDate.IsEmpty)
				{
					cusPermitHeaderFilter.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, transactionDate.Date);

					var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, transactionDate.Date);
					endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
					cusPermitHeaderFilter.AddToFilter(endDateQuery);
				}

				return cusPermitHeaderFilter;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(BaseCusPermitHeader);
			}
		}

		#endregion

		#region Override Properties

		public override ZString CPH_RN_NKCountryCode
		{
			get { return base.CPH_RN_NKCountryCode; }
			set
			{
				var hasChanged = value != base.CPH_RN_NKCountryCode;
				base.CPH_RN_NKCountryCode = value;
				if (hasChanged)
				{
					countrySpecificInstruction = null;
				}
			}
		}

		public override ZString ShortName => Res.GetString("88C6E7FD-E81F-4B67-8260-6839251B760F", "Permit");

		#endregion

		#region New Properties

		public virtual PermitCountrySpecificInstruction CountrySpecificInstruction
		{
			get
			{
				if (countrySpecificInstruction == null)
				{
					countrySpecificInstruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, CPH_RN_NKCountryCode);
				}
				return countrySpecificInstruction;
			}
		}
		PermitCountrySpecificInstruction countrySpecificInstruction;

		public override ISharedCountrySpecificInstruction GetCountrySpecificInstruction() => CountrySpecificInstruction;

		#endregion

		public ZString QtyValIndicatorDescription => Lookups.PermitQtyValIndicators.GetDescriptionFromCode(CPH_QtyValIndicator);

		public void Close()
		{
			if (CPH_IsClosed)
			{
				return;
			}
			CPH_IsClosed = true;
			Logs.AddNew(Enterprise.ZArchitecture.Business.Events.PeriodClosed, "Permit closed");
		}

		/// <summary>
		/// Returns true if there are transactions besides the original balance transactions and those transactions do not nullify each other; otherwise, false.
		/// </summary>
		public bool HasNonZeroOrderBalance()
		{
			var orderTransactions = this.CusPermitLineTransactions.Where(t => t.CPL_TransactionType != PermitTransactionTypeList.Codes.OBL).ToList();
			ZDecimal ordersQty = orderTransactions.Sum(t => t.CPL_TranQty);
			ZDecimal ordersValue = orderTransactions.Sum(t => t.CPL_TranValue);
			return ordersQty != ZDecimal.Zero || ordersValue != ZDecimal.Zero;
		}

		public bool HasPendingTransaction()
		{
			return this.CusPermitLineTransactions.Any(t => t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending);
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("D91699F1-03EF-493E-BC33-2E7611B9381A", "Permit {0} - {1}", PermitHolder?.OH_Code ?? ZString.Empty, CPH_Number).Trim(); }
		}

		#endregion

		#region Implementation

		#region Rules

		[ChildEditable(true)]
		public CusPermitRuleCollection CusPermitRules
		{
			get
			{
				if (cusPermitRules == null)
				{
					cusPermitRules = CreateNewCusPermitRuleCollection();
					RegisterEditableChildObject(cusPermitRules);
				}
				return cusPermitRules;
			}
		}
		CusPermitRuleCollection cusPermitRules;

		protected CusPermitRuleCollection CreateNewCusPermitRuleCollection()
		{
			return new CusPermitRuleCollection(this);
		}

		protected override IEnumerable<SharedCusPermitRule> GetRules() => CusPermitRules;

		public override IActiveBusinessObjectCollection GetRulesCollection() => CusPermitRules;

		public override Type GetRuleType() => typeof(BaseCusPermitRule);

		#endregion Rules

		#region Transactions

		[ChildEditable(true)]
		public CusPermitLineTransactionCollection CusPermitLineTransactions
		{
			get
			{
				if (cusPermitLineTransactions == null)
				{
					cusPermitLineTransactions = CreateNewCusPermitLineTransactionCollection();
					RegisterEditableChildObject(cusPermitLineTransactions);
				}
				return cusPermitLineTransactions;
			}
		}
		CusPermitLineTransactionCollection cusPermitLineTransactions;

		protected virtual CusPermitLineTransactionCollection CreateNewCusPermitLineTransactionCollection()
		{
			return new CusPermitLineTransactionCollection(this);
		}

		public override IEnumerable<SharedCusPermitLineTransaction> GetTransactions()
			=> CusPermitLineTransactions;

		protected override IActiveBusinessObjectCollection GetTransactionsCollection() => CusPermitLineTransactions;

		public override void ReloadLatestTransactions()
		{
			if (((IBusinessObjectCollectionInternals)CusPermitLineTransactions).HasChangesFromDatabase())
			{
				CusPermitLineTransactions.RefreshFromDb();
			}
		}

		protected override bool IsNotBusting(ZDecimal value, ZDecimal quantity, ZString transactionType, Action<ZString, ZDecimal> notifier)
		{
			var result = true;
			if (IsVAL && ValueBalance is ZDecimal valueBalance && IsBursting(valueBalance, value))
			{
				notifier?.Invoke(Res.GetString("064f3d35-dadc-4bc5-93d5-b772c6365746", "The permit {0} available value will be exceeded by {1:0.##}. The available is {2:0.##}.", CPH_Number, (value + valueBalance) * -1, valueBalance), valueBalance);
				result = false;
			}
			if (IsQTY && QuantityBalance is ZDecimal quantityBalance && IsBursting(quantityBalance, quantity))
			{
				notifier?.Invoke(Res.GetString("aa0f51e4-804e-4038-998f-69a9afa69c99", "The permit {0} available quantity will be exceeded by {1:0.##}. The available is {2:0.##}.", CPH_Number, (quantity + quantityBalance) * -1, quantityBalance), quantityBalance);
				result = false;
			}
			return result;
		}

		public override Type GetTransactionType() => typeof(BaseCusPermitLineTransaction);

		#endregion Transactions

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CPH_StartDate = ZDate.BrettsBirthday;
			CPH_QtyValIndicator = "BTH";
		}
#endif

		#endregion

		protected override string DocManagerCode => Core.Constants.DocManagerCodes.CustomsPermit;

		protected override ControllerID ControllerID => ControllerIDs.Customs.Permits;

		#region Validation and Lookups

		public new BaseCusPermitHeaderValidation Validation
		{
			get { return (BaseCusPermitHeaderValidation)base.Validation; }
		}

		protected override CusPermitHeaderValidation GetNewValidation()
		{
			return new BaseCusPermitHeaderValidation(this);
		}

		public new BaseCusPermitHeaderLookups Lookups
		{
			get { return (BaseCusPermitHeaderLookups)base.Lookups; }
		}

		protected override CusPermitHeaderLookups GetNewLookups()
		{
			return new BaseCusPermitHeaderLookups(this);
		}

		#endregion
	}
}
