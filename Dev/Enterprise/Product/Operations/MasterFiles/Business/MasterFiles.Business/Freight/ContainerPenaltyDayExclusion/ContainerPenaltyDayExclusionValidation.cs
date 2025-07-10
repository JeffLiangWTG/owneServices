//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoContainerPenaltyDayExclusionValidation
//
//    This class should be used for overriding validation in AutoContainerPenaltyDayExclusionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ContainerPenaltyDayExclusionValidation : AutoContainerPenaltyDayExclusionValidation
	{
		public ContainerPenaltyDayExclusionValidation(AutoContainerPenaltyDayExclusion parent) : base(parent)
		{
		}

		protected override void CheckCEX_Monday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_MondayInfo);
			}
		}

		protected override void CheckCEX_Tuesday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_TuesdayInfo);
			}
		}

		protected override void CheckCEX_Wednesday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_WednesdayInfo);
			}
		}

		protected override void CheckCEX_Thursday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_ThursdayInfo);
			}
		}

		protected override void CheckCEX_Friday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_FridayInfo);
			}
		}

		protected override void CheckCEX_Saturday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_SaturdayInfo);
			}
		}

		protected override void CheckCEX_Sunday()
		{
			if (Parent is ContainerPenaltyDayExclusion exclusion)
			{
				CheckNotAllDaysSelected(exclusion, exclusion.CEX_SundayInfo);
			}
		}

		void CheckNotAllDaysSelected(ContainerPenaltyDayExclusion exclusion, ZPropertyInfo propertyInfo)
		{
			if (!IsExclusionValid(exclusion))
			{
				propertyInfo.AddError(Res.GetString("53116042-647a-a28f-497f-b428b74e5461", "Penalty Exclusion should not have all days excluded."));
			}
		}

		bool IsExclusionValid(ContainerPenaltyDayExclusion exclusion)
		{
			return !(exclusion.CEX_Monday
				&& exclusion.CEX_Tuesday
				&& exclusion.CEX_Wednesday
				&& exclusion.CEX_Thursday
				&& exclusion.CEX_Friday
				&& exclusion.CEX_Saturday
				&& exclusion.CEX_Sunday);
		}
	}
}
