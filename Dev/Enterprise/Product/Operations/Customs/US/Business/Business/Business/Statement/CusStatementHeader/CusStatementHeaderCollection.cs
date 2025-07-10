using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	[ModuleID("USCustomsStatement")]
	public class CusStatementHeaderCollection : ActiveBusinessObjectCollection<CusStatementHeader>, ICusStatementHeaderCollection
	{
		public CusStatementHeaderCollection(CusStatementHeader monthlyStatement)
			: this(monthlyStatement, false)
		{
		}

		public CusStatementHeaderCollection(CusStatementHeader monthlyStatement, bool filteredByAccField)
			: base(monthlyStatement.Factory, monthlyStatement, new ZQuery(CusStatementHeaderSchema.B2_B2_PeriodicStatement, monthlyStatement.PK), CusStatementHeaderSchema.B2_B2_PeriodicStatement)
		{
			this.monthlyStatement = monthlyStatement;
			this.filteredByAccField = filteredByAccField;
		}

		readonly CusStatementHeader monthlyStatement;
		readonly bool filteredByAccField;

		/// <summary>
		/// Used by a module search screen
		/// </summary>
		public CusStatementHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusStatementHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CusStatementHeaderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override object[] GetCollectionState()
		{
			object[] result = base.GetCollectionState();

			if (filteredByAccField)
			{
				result = new object[] { monthlyStatement.FilterStatementLinesBy };
			}

			return result;
		}

		public ZString GetUniqueAccountNo()
		{
			ZString? result = null;

			foreach (CusStatementHeader dailyStatement in this)
			{
				if (!dailyStatement.B2_AccountNo.IsEmpty)
				{
					if (!result.HasValue)
					{
						result = dailyStatement.B2_AccountNo;
					}
					else if (result.Value != dailyStatement.B2_AccountNo)
					{
						result = ZString.Empty;
						break;
					}
				}
			}

			return result ?? ZString.Empty ;
		}

		public ZString GetUniquePaymentParty()
		{
			ZString? result = null;

			foreach (CusStatementHeader dailyStatement in this)
			{
				if (!dailyStatement.B2_PaymentParty.IsEmpty)
				{
					if (!result.HasValue)
					{
						result = dailyStatement.B2_PaymentParty;
					}
					else if (result.Value != dailyStatement.B2_PaymentParty)
					{
						result = ZString.Empty;
						break;
					}
				}
			}

			return result ?? ZString.Empty ;
		}

		protected override bool MatchesFilterCore(CusStatementHeader element, bool fetchOnlyFromLocalCache)
		{
			bool result = base.MatchesFilterCore(element, fetchOnlyFromLocalCache);

			if (result && filteredByAccField && monthlyStatement != null && !monthlyStatement.FilterStatementLinesBy.IsEmpty)
			{
				switch (monthlyStatement.FilterStatementLinesBy)
				{
					case StatementLineFilterByOptionList.Codes.LinesWithAPDiscrepany:
						result = element.DifferenceBetweenAPInvoiceAndCustomsAmount != 0m;
						break;

					case StatementLineFilterByOptionList.Codes.LinesWithARDiscrepany:
						result = element.DifferenceBetweenARInvoiceAndCustomsAmount != 0m;
						break;

					case StatementLineFilterByOptionList.Codes.LinesWithDiscrepany:
						result = element.HasDiscrepancyBetweenInvoicesAndCustomsAmount;
						break;

					case StatementLineFilterByOptionList.Codes.All:
						result = true;
						break;

					default:
						result = false;
						break;
				}
			}

			return result;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			return result;
		}
	}
}
