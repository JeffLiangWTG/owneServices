using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(nameof(CodeProperty)), DescriptionProperty(nameof(CodeProperty))]
	public class ContainerPenaltyDayExclusion : AutoContainerPenaltyDayExclusion, IContainerPenaltyDayExclusion
	{
		public ContainerPenaltyDayExclusion(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString CodeProperty
		{
			get
			{
				var stringBuilder = new ZStringBuilder();

				if (CEX_Monday)
				{
					stringBuilder.Append(Res.GetString("9ccc778b-3baf-4e91-4c68-5eafb83d0e71", "MON"));
				}

				if (CEX_Tuesday)
				{
					stringBuilder.Append(Res.GetString("4594d9b6-7ad2-108f-4b97-5ecb2f2c5758", "TUE"));
				}

				if (CEX_Wednesday)
				{
					stringBuilder.Append(Res.GetString("c4dc7957-9212-0586-4fe3-268796651baa", "WED"));
				}

				if (CEX_Thursday)
				{
					stringBuilder.Append(Res.GetString("ded8412c-aced-f1a4-406a-4929f39bd949", "THU"));
				}

				if (CEX_Friday)
				{
					stringBuilder.Append(Res.GetString("74e03c84-4444-7c84-42d7-64a5f3119844", "FRI"));
				}

				if (CEX_Saturday)
				{
					stringBuilder.Append(Res.GetString("c4ebefa2-8cad-1e9f-4164-5d9137299d04", "SAT"));
				}

				if (CEX_Sunday)
				{
					stringBuilder.Append(Res.GetString("2c865e3b-d2ab-7db1-4cdd-036c2fce9203", "SUN"));
				}

				if (CEX_Weekend)
				{
					stringBuilder.Append(Res.GetString("3dea396c-4cb2-18b3-4ce0-6640a9e1e2f4", "WKD"));
				}

				if (CEX_Holiday)
				{
					stringBuilder.Append(Res.GetString("fd89bacf-abd2-22b2-43f9-5fb6bc9cb153", "HOL"));
				}

				return stringBuilder.ToStringWithDelimiterBetweenAppends(" / ");
			}
		}

		public IReadOnlyCollection<DayOfWeek> GetExcludedDaysOfWeek()
		{
			var exclusions = new Dictionary<DayOfWeek, bool>
			{
				{ DayOfWeek.Monday, CEX_Monday },
				{ DayOfWeek.Tuesday, CEX_Tuesday },
				{ DayOfWeek.Wednesday, CEX_Wednesday },
				{ DayOfWeek.Thursday, CEX_Thursday },
				{ DayOfWeek.Friday, CEX_Friday },
				{ DayOfWeek.Saturday, CEX_Saturday },
				{ DayOfWeek.Sunday, CEX_Sunday }
			};

			return exclusions.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
		}

		public override void OnSaving()
		{
			if (IsEmpty())
			{
				Delete();
			}

			base.OnSaving();
		}

		public bool IsEmpty() => !CEX_Monday && !CEX_Tuesday && !CEX_Wednesday && !CEX_Thursday && !CEX_Friday && !CEX_Saturday && !CEX_Sunday && !CEX_Weekend && !CEX_Holiday;

		protected override bool SupportsCloneCore() => true;
	}
}
