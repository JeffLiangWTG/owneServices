using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	[DependentBusinessObject(typeof(CusStatementLine), "Charges")]
	public class CusStatementLineCharge : BaseCusStatementLineCharge
	{
		public CusStatementLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseCusStatementLineCharge.Schema
		{
			public const string AccountNo = "AccountNo";
			public const string ProcessDate = "ProcessDate";
			public const string EntryNum = "EntryNum";
			public const string ChargeTypeDescription = "ChargeTypeDescription";
		}

		#region New Properties

		public CusStatementLine Line
		{
			get
			{
				if (IsNull || B4_B3.IsEmpty)
				{
					return (CusStatementLine)Factory.GetNull(GetCusStatementLineType());
				}
				else
				{
					ZGuid foreignKey = B4_B3;
					if (fLine == null || fLine.PK != foreignKey)
					{
						fLine = (CusStatementLine)Factory.Load(GetCusStatementLineType(), foreignKey);
					}
					return fLine != null && !fLine.IsDeleted ? fLine : (CusStatementLine)Factory.GetNull(GetCusStatementLineType());
				}
			}
		}
		CusStatementLine fLine;

		public ZString AccountNo => Line?.Header?.B2_AccountNo ?? ZString.Empty;
		public ZDateTime ProcessDate => Line?.Header?.B2_ProcessDate ?? ZDateTime.Empty;
		public ZString EntryNum => Line?.B3_EntryNum ?? ZString.Empty;

		public ZString ChargeTypeDescription => ChargeTypes.GetDescriptionFromCode(B4_ChargeType);

		#endregion

		#region Lists

		public CodeDescriptionPairList ChargeTypes => Factory.GetCachedValue<CusStatementChargeTypeList>();

		#endregion

		#region Implementation

		protected virtual Type GetCusStatementLineType()
		{
			return typeof(CusStatementLine);
		}

		#endregion

		#region Fetch Hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CusStatementLineCharge bizO)
				: base(bizO)
			{
			}

			public new CusStatementLineCharge BusinessObject => (CusStatementLineCharge)base.BusinessObject;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(CusStatementLineSchema.PK, BusinessObject.B4_B3);

				var headerFilter = new ZDBOnlyQuery(typeof(CusStatementHeader));
				var lineFilter = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
				lineFilter.AddToFilter(CusStatementLineSchema.PK, BusinessObject.B4_B3);
				headerFilter.AddSubQuery(lineFilter, JoinCondition.And);

				Factory.AddFetchHint(CusStatementHeaderSchema.Instance, headerFilter);
			}
		}

		#endregion
	}
}
