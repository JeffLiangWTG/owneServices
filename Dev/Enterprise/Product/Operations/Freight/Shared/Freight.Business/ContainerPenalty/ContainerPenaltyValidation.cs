using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ContainerPenaltyValidation : JobContainerPenaltyValidation
	{
		public ContainerPenaltyValidation(ContainerPenalty parent) : base(parent)
		{
		}

		#region Parent

		public new ContainerPenalty Parent
		{
			get { return (ContainerPenalty)base.Parent; }
		}

		#endregion

		#region CPY_PenaltyType

		protected override void CheckCPY_PenaltyType()
		{
			base.CheckCPY_PenaltyType();

			if (Parent.CPY_PenaltyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CPY_PenaltyTypeInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.CPY_PenaltyTypeInfo, Parent.Lookups.PenaltyTypeList);
			if (!Parent.CPY_PenaltyTypeInfo.HasErrors())
			{
				CheckCreditorTypeRule(Parent.CPY_PenaltyTypeInfo);
			}
		}

		#endregion

		#region CPY_CreditorType

		protected override void CheckCPY_CreditorType()
		{
			base.CheckCPY_CreditorType();

			CheckUniqueness(Parent.CPY_CreditorTypeInfo);

			if (Parent.CPY_CreditorType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CPY_CreditorTypeInfo);
			}

			if ((Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention
				|| Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
				&& Parent.CPY_CreditorType != ContainerPenaltyCreditorType.Codes.Carrier)
			{
				Parent.CPY_CreditorTypeInfo.AddError(Res.GetString("897070d9-c35b-4d33-ae5e-d6edbc8af8f6", "Creditor Type is expected to be Carrier."));
			}

			if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.TruckWaitTime
				&& Parent.CPY_CreditorType != ContainerPenaltyCreditorType.Codes.Transport)
			{
				Parent.CPY_CreditorTypeInfo.AddError(Res.GetString("961fca7e-df64-44b4-8943-5062e18bb7eb", "Creditor Type is expected to be Transport."));
			}

			if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage
				&& Parent.CPY_CreditorType != ContainerPenaltyCreditorType.Codes.CTO
				&& Parent.CPY_CreditorType != ContainerPenaltyCreditorType.Codes.Carrier)
			{
				Parent.CPY_CreditorTypeInfo.AddError(Res.GetString("b6be20cb-0e2c-4daf-a6f3-8b7ac9b6d05b", "Creditor Type is expected to be CTO or Carrier."));
			}

			ListValidation.ErrorIfInvalidCode(Parent.CPY_CreditorTypeInfo, Parent.Lookups.CreditorTypeList);

			if (!Parent.CPY_CreditorTypeInfo.HasErrors())
			{
				CheckCreditorTypeRule(Parent.CPY_CreditorTypeInfo);
			}
		}

		#endregion

		protected override void CheckCPY_OH_Creditor()
		{
			base.CheckCPY_OH_Creditor();

			CheckUniqueness(Parent.CPY_OH_CreditorInfo);
			if (!Parent.CPY_OH_CreditorInfo.HasErrors())
			{
				CheckCreditorTypeRule(Parent.CPY_OH_CreditorInfo);
			}
		}

		#region CPY_RL_NKLocation

		protected override void CheckCPY_RL_NKLocation()
		{
			base.CheckCPY_RL_NKLocation();

			CheckUniqueness(Parent.CPY_RL_NKLocationInfo);

			if (Parent.Container?.SupportsContainerPenalties ?? false)
			{
				var location = Parent.CPY_RL_NKLocation;
				if (location.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.CPY_RL_NKLocationInfo);
				}
				else if (location.Length != 2 && location.Length != 5)
				{
					Parent.CPY_RL_NKLocationInfo.AddError(Res.GetString("3d88d841-b00a-dabe-4ed7-51a6ff1cc429", "Container Location should be 2 or 5 characters long."));
				}
			}
		}

		#endregion

		#region CPY_TimeUnit

		protected override void CheckCPY_TimeUnit()
		{
			base.CheckCPY_TimeUnit();

			if (Parent.CPY_TimeUnit.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CPY_TimeUnitInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.CPY_TimeUnitInfo, Parent.Lookups.TimeUnitList);
		}

		#endregion

		#region CPY_RX_NKCurrency

		protected override void CheckCPY_RX_NKCurrency()
		{
			base.CheckCPY_RX_NKCurrency();

			if (Parent.CPY_RX_NKCurrency.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CPY_RX_NKCurrencyInfo);
			}

			if (Parent.CPY_RX_NKCurrency.Length != 3)
			{
				Parent.CPY_RX_NKCurrencyInfo.AddError(Res.GetString("b255e259-7f38-4855-b448-faf65fb96a57", "Currency length should be 3."));
			}
		}

		#endregion

		#region CPY_Duration

		protected override void CheckCPY_Duration()
		{
			base.CheckCPY_Duration();

			CheckIfTotalCostEqualCostPerUnitTimeDuration(Parent.CPY_DurationInfo);

			if (Parent.Container != null)
			{
				var days = Parent.GetDefaultDurationAsDays();

				if (!days.HasValue || Parent.DurationAsDays == days)
				{
					return;
				}

				var (_, lastPenaltyDayName) = Parent.GetCalculatedLastPenaltyDay();
				var (_, lastFreeDayName) = Parent.GetLastFreeDayWithOverride();
				var (_, firstFreeDayName) = Parent.GetCalculatedFirstFreeDay();

				if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && Parent.CPY_ProcessType == ContainerPenaltyProcessType.Import)
				{
					var warning = Res.GetString("61ad92a2-9b7c-480c-8f18-3e5f8048b2f0", "Number of days in detention from {0} minus {1} is calculated to {2}",
						lastPenaltyDayName,
						lastFreeDayName,
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
				else if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
				{
					var warning = Res.GetString("c1890ff2-75b8-4f8a-9d09-72bd0a17c097", "Number of days in detention from {0} minus free days minus {1} plus 1 is calculated to {2}",
						lastPenaltyDayName,
						firstFreeDayName,
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
				else if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && Parent.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO && Parent.CPY_ProcessType == ContainerPenaltyProcessType.Import)
				{
					var warning = Res.GetString("35373e06-c9dd-4de3-9618-8afe50292598", "Number of days in storage from {0} minus {1} plus 1 is calculated to {2}",
						lastPenaltyDayName,
						"CTO Storage Start",
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
				else if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage)
				{
					var warning = Res.GetString("e858bbb0-5caa-4c4b-9b58-9ede5f6ab071", "Number of days in storage from {0} minus free days minus {1} plus 1 is calculated to {2}",
						lastPenaltyDayName,
						firstFreeDayName,
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
				else if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && ContainerPenalty.GetDirection(Parent.CPY_ProcessType) == ContainerDetentionDirection.Import)
				{
					var warning = Res.GetString("3cde51dc-3b53-470a-b829-01098ae98bd0", "Number of days in merged demurrage then detention from {0} minus free days minus {1} plus 1 is calculated to {2}",
						lastPenaltyDayName,
						firstFreeDayName,
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
				else if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && ContainerPenalty.GetDirection(Parent.CPY_ProcessType) == ContainerDetentionDirection.Export)
				{
					var warning = Res.GetString("8686f455-8599-4895-8fbd-df679e293c76", "Number of days in merged detention then demurrage from {0} minus free days minus {1} plus 1 is calculated to {2}",
						lastPenaltyDayName,
						firstFreeDayName,
						days);
					Parent.CPY_DurationInfo.AddWarning(warning);
				}
			}
		}

		#endregion

		#region CPY_FreeTime

		protected override void CheckCPY_FreeTime()
		{
			base.CheckCPY_FreeTime();

			if (Parent.Container == null)
			{
				return;
			}

			var container = Parent.Container;
			var freeDays = Parent.GetDefaultFreeTimeAsDays();

			if (!freeDays.HasValue || Parent.FreeTimeAsDays == freeDays)
			{
				return;
			}

			if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && Parent.CPY_ProcessType == ContainerPenaltyProcessType.Import)
			{
				var (_, availableDate, availableDateName) = container.NewContainerDefaultingStrategy()?.CalculateRequiredBy() ?? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);

				if (container.JC_EmptyReturnedBy.IsValid && availableDate.IsValid && freeDays.HasValue && freeDays.Value >= 0 && Parent.FreeTimeAsDays != freeDays.Value)
				{
					var warning = Res.GetString("eebd18fc-91c2-4d6a-bdbd-aaafafa15ee6", "The difference in days between {0} and {1} calculates to {2}",
						availableDateName,
						container.JC_EmptyReturnedByInfo.HumanReadableName,
						freeDays.Value);
					Parent.CPY_FreeTimeInfo.AddWarning(warning);
				}
			}
			else
			{
				var warning = Res.GetString("7fd17dd3-82d1-49e0-951f-79ec4c999237", "The number of free days calculates to {0}",
						freeDays.Value);
				Parent.CPY_FreeTimeInfo.AddWarning(warning);
			}
		}

		#endregion

		#region CPY_TotalCost

		protected override void CheckCPY_TotalCost()
		{
			base.CheckCPY_TotalCost();

			CheckIfTotalCostEqualCostPerUnitTimeDuration(Parent.CPY_TotalCostInfo);
		}

		protected override void CheckCPY_PerUnitCost()
		{
			base.CheckCPY_PerUnitCost();

			CheckIfTotalCostEqualCostPerUnitTimeDuration(Parent.CPY_PerUnitCostInfo);
		}

		void CheckIfTotalCostEqualCostPerUnitTimeDuration(ZPropertyInfo propertyInfo)
		{
			if (Parent.Container == null || Parent.CalculateTotalCost == Parent.CPY_TotalCost)
			{
				return;
			}

			propertyInfo.AddWarning(Parent is ShipmentContainerPenalty
				? Res.GetString("D6FFF075-9CB7-4013-AA4B-8F15FB64B8B7",
					"Sell Per Unit times duration does not equal Total Sell")
				: Res.GetString("C8F41721-3092-4F1B-BA68-BD9D89D40011",
					"Cost Per Unit times duration does not equal Total Cost"));
		}

		#endregion

		void CheckUniqueness(ZPropertyInfo property)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobContainerPenaltySchema.CPY_JC_Container, Parent.CPY_JC_Container);
			query.AddToFilter(JobContainerPenaltySchema.CPY_ProcessType, Parent.CPY_ProcessType);
			query.AddToFilter(JobContainerPenaltySchema.CPY_PenaltyType, Parent.CPY_PenaltyType);

			if (Parent.CPY_OH_Creditor.IsValid)
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_OH_Creditor, Parent.CPY_OH_Creditor);
			}
			else
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_OH_Creditor, null);
			}

			if (Parent.CPY_CreditorType.IsValid)
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_CreditorType, Parent.CPY_CreditorType);
			}
			else
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_CreditorType, null);
			}

			if (Parent.CPY_RL_NKLocation.IsValid)
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_RL_NKLocation, Parent.CPY_RL_NKLocation);
			}
			else
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_RL_NKLocation, null);
			}

			if (Parent.CPY_JS_Shipment.IsValid)
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_JS_Shipment, Parent.CPY_JS_Shipment);
			}
			else
			{
				query.AddToFilter(JobContainerPenaltySchema.CPY_JS_Shipment, null);
			}

			query.AddToFilter(JobContainerPenaltySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			if (Parent.Factory.Exists(Parent.GetType(), query))
			{
				property.AddWarning(Res.GetString("7f8d8117-e55d-4253-91d4-e5c44fe6a050", "You can't enter more than one penalty for same creditor and location."));
			}
		}

		void CheckCreditorTypeRule(ZPropertyInfo property)
		{
			var carrierWarningMessage = Res.GetString("ee469451-53b8-42ad-8467-acd61d0e134e",
@"Under the same Container Penalty grid, with Creditor Type of 'CAR' and the Creditor has the same value, either:
An entry with Penalty Type of 'MDD' is allowed, or.
Entries of Penalty Type of 'DET' and/or 'STO' allowed.");

			if (Parent.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
			{
				var query = new ZQuery();
				query.AddToFilter(JobContainerPenaltySchema.CPY_JC_Container, Parent.CPY_JC_Container);
				query.AddToFilter(JobContainerPenaltySchema.CPY_ProcessType, Parent.CPY_ProcessType);

				if (Parent.CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
				{
					query.AddToFilter(JobContainerPenaltySchema.CPY_PenaltyType, new ZString[]
					{
						ContainerPenaltyPenaltyType.Codes.Detention,
						ContainerPenaltyPenaltyType.Codes.Storage
					});
				}
				else
				{
					query.AddToFilter(JobContainerPenaltySchema.CPY_PenaltyType, ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
				}

				query.AddToFilter(JobContainerPenaltySchema.CPY_CreditorType, ContainerPenaltyCreditorType.Codes.Carrier);

				if (Parent.CPY_OH_Creditor.IsValid)
				{
					query.AddToFilter(JobContainerPenaltySchema.CPY_OH_Creditor, Parent.CPY_OH_Creditor);
				}
				else
				{
					query.AddToFilter(JobContainerPenaltySchema.CPY_OH_Creditor, null);
				}

				query.AddToFilter(JobContainerPenaltySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.Exists(Parent.GetType(), query))
				{
					property.AddWarning(carrierWarningMessage);
				}
			}
		}
	}
}
