using System;
using System.Diagnostics;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public delegate ZQuery GetJobHeaderQuery(ZQuery jobHeaderFilter, bool notIn);

	public sealed class ChargeModuleFilter : AutoChargeModuleFilter
	{
		public ChargeModuleFilter(ZString description, GetJobHeaderQuery queryDelegate)
			: base(description, queryDelegate) { }

		[List("Lookups.ChargeGroups")]
		public override ZString ChargeGroup
		{
			[DebuggerStepThrough]
			get { return base.ChargeGroup; }
			[DebuggerStepThrough]
			set { base.ChargeGroup = value; }
		}

		public override ZBool UseLowerBound
		{
			[DebuggerStepThrough]
			get { return base.UseLowerBound; }
			set
			{
				bool valueChanged = UseLowerBound != value;
				base.UseLowerBound = value;

				if (valueChanged)
				{
					if (value)
					{
						LowerBound = UpperBound >= 0 ? ZDecimal.Zero : UpperBound;
					}
					else
					{
						LowerBound = 0;
					}
				}
			}
		}

		public override ZDecimal LowerBound
		{
			[DebuggerStepThrough]
			get { return base.LowerBound; }
			set
			{
				base.LowerBound = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateUpperBound();
				}
			}
		}
		protected override bool LowerBound_ReadOnly
		{
			get { return !UseLowerBound; }
		}

		public override ZBool UseUpperBound
		{
			[DebuggerStepThrough]
			get { return base.UseUpperBound; }
			set
			{
				bool valueChanged = UseUpperBound != value;
				base.UseUpperBound = value;

				if (valueChanged)
				{
					if (value)
					{
						UpperBound = LowerBound <= 0 ? ZDecimal.Zero : LowerBound;
					}
					else
					{
						UpperBound = 0;
					}
				}
			}
		}

		public override ZDecimal UpperBound
		{
			[DebuggerStepThrough]
			get { return base.UpperBound; }
			set
			{
				base.UpperBound = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateLowerBound();
				}
			}
		}
		protected override bool UpperBound_ReadOnly
		{
			get { return !UseUpperBound; }
		}

		public ChargeModuleFilterLookups Lookups
		{
			get { return lookups ?? (lookups = new ChargeModuleFilterLookups(this)); }
		}
		ChargeModuleFilterLookups lookups;

		#region Implementation

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotImplementedException();
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			throw new NotSupportedException();
		}

		protected override bool IsEmptyCore => !UseUpperBound && !UseLowerBound;

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get
			{
				if ((UseLowerBound && LowerBound > 0) || (UseUpperBound && UpperBound < 0))
				{
					return new object[] { GetHeaderQuery_TotalWithinBounds(), false };
				}
				else
				{
					return new object[] { GetHeaderQuery_TotalOutsideBounds(), true };
				}
			}
		}

		ZQuery GetHeaderQuery_TotalWithinBounds()
		{
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			StringBuilder builder = new StringBuilder();

			builder.Append("JH_PK in (select JR_JH from dbo.JobCharge join dbo.AccChargeCode on JR_AC = AC_PK");

			if (!ChargeGroup.IsEmpty)
			{
				builder.Append(" where AC_ChargeGroup = @ChargeGroup");
				parameters.Add("@ChargeGroup", ChargeGroup, AccChargeCodeSchema.AC_ChargeGroup);
			}

			builder.Append(" group by JR_JH having");

			if (UseLowerBound)
			{
				builder.Append(" sum(JR_LocalSellAmt) >= @LowerBound");
				parameters.Add("@LowerBound", LowerBound, JobChargeSchema.JR_LocalSellAmt);

				if (UseUpperBound)
				{
					builder.Append(" and sum(JR_LocalSellAmt) <= @UpperBound");
					parameters.Add("@UpperBound", UpperBound, JobChargeSchema.JR_LocalSellAmt);
				}
			}
			else
			{
				builder.Append(" sum(JR_LocalSellAmt) <= @UpperBound");
				parameters.Add("@UpperBound", UpperBound, JobChargeSchema.JR_LocalSellAmt);
			}

			builder.Append(")");

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));
			result.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			result.AddFilterAndZSQLParameterCollection(builder.ToString(), parameters);

			return result;
		}

		ZQuery GetHeaderQuery_TotalOutsideBounds()
		{
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			StringBuilder builder = new StringBuilder();

			builder.Append("JH_PK in (select JR_JH from dbo.JobCharge join dbo.AccChargeCode on JR_AC = AC_PK");

			if (!ChargeGroup.IsEmpty)
			{
				builder.Append(" where AC_ChargeGroup = @ChargeGroup");
				parameters.Add("@ChargeGroup", ChargeGroup, AccChargeCodeSchema.AC_ChargeGroup);
			}

			builder.Append(" group by JR_JH having");

			if (UseLowerBound)
			{
				builder.AppendFormat(" sum(JR_LocalSellAmt) < @LowerBound", LowerBound);
				parameters.Add("@LowerBound", LowerBound, JobChargeSchema.JR_LocalSellAmt);

				if (UseUpperBound)
				{
					builder.AppendFormat(" or sum(JR_LocalSellAmt) > @UpperBound", UpperBound);
					parameters.Add("@UpperBound", UpperBound, JobChargeSchema.JR_LocalSellAmt);
				}
			}
			else
			{
				builder.AppendFormat(" sum(JR_LocalSellAmt) > @UpperBound", UpperBound);
				parameters.Add("@UpperBound", UpperBound, JobChargeSchema.JR_LocalSellAmt);
			}

			builder.Append(")");

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));
			result.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			result.AddFilterAndZSQLParameterCollection(builder.ToString(), parameters);

			return result;
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			ChargeGroup = RandomString(3);
			UseLowerBound = ZBool.True;
			UseUpperBound = ZBool.True;
		}

#endif
		#endregion

		#endregion
	}
}
