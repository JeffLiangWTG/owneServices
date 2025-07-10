using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class CusStatementLineChargeCollection : ActiveBusinessObjectCollection<CusStatementLineCharge>
	{
		public CusStatementLineChargeCollection(CusStatementLine statementLine)
			: base(statementLine)
		{
		}

		/// <summary>
		/// Used by a module search screen
		/// </summary>
		public CusStatementLineChargeCollection(BusinessObjectFactory factory)
			: base(factory, GetCompanyFilter())
		{
		}

		static ZQuery GetCompanyFilter()
		{
			var headerFilter = new ZDBOnlySubQuery(typeof(CusStatementHeader), CusStatementLineSchema.B3_B2);
			headerFilter.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbBranch.CurrentBranch.Company.PK);

			var lineFilter = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineChargeSchema.B4_B3);
			lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(CusStatementLineCharge));
			query.AddSubQuery(lineFilter, JoinCondition.And);

			return query;
		}

		/// <summary>
		/// If there is an existing charge and amount == 0, then the charge is to be deleted
		/// If there is no charge, then create a new charge and update amount.
		/// </summary>
		public void UpdateLineChargeFor(ZString chargeType, ZDecimal amount)
		{
			var result = GetFirstCharge(chargeType);

			if (result != null && amount == 0)
			{
				result.Delete();
			}
			else if (amount != 0)
			{
				if (result == null)
				{
					result = AddNew();
					result.B4_ChargeType = chargeType;
				}
				result.B4_ChargeAmount = amount;
			}
		}

		public CusStatementLineCharge GetFirstCharge(ZString chargeType)
		{
			return GetChargesFor(chargeType)?.FirstOrDefault();
		}

		public CusStatementLineCharge GetChargeLineFor(ZString chargeType, ZDecimal amount)
		{
			return this.FirstOrDefault(x => x.B4_ChargeType == chargeType && x.B4_ChargeAmount == amount);
		}

		public ZDecimal GetAmountFor(ZString chargeType)
		{
			return GetFirstCharge(chargeType)?.B4_ChargeAmount ?? ZDecimal.Zero;
		}

		CusStatementLineCharge[] GetChargesFor(ZString chargeType)
		{
			return this.Where(x => x.B4_ChargeType == chargeType)?.ToArray();
		}
	}
}
