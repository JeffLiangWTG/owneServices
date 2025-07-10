//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobCartageRunSheetValidation
//
//    This class should be used for overriding validation in AutoJobCartageRunSheetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;

namespace Enterprise.Freight.Common.Business
{
	public class LocalCartageJobLegTypeValidation : AutoLocalCartageJobLegTypeValidation
	{
		public LocalCartageJobLegTypeValidation(AutoLocalCartageJobLegType parent)
			: base(parent)
		{
		}

		#region Overrides

		protected override void CheckE4_DisplayOrder()
		{
			base.CheckE4_DisplayOrder();
			if (CommonCartageType != null)
			{
				CheckOrder(CommonCartageType.AllCartageLegTypes);
			}
		}

		void CheckOrder(CommonCartageLegTypeCollection collection)
		{
			ArrayList legs = new ArrayList(collection);
			legs.Sort(new LegTypesComparer());

			if (legs.Count > 0 && (Parent == legs[0]) && (Parent.E4_DisplayOrder != 1))
			{
				Parent.E4_DisplayOrderInfo.AddError(Res.GetString("a45fc758-b6e7-4a7d-be92-444162740951", "Display Order of the first Leg should be 1."));
			}
			else
			{
				for (int index = 1; index <= legs.Count; index++)
				{
					CommonCartageLegType currentLeg = (CommonCartageLegType)legs[index - 1];
					if ((Parent == currentLeg && Parent.E4_DisplayOrder != index)
						|| (Parent != currentLeg && currentLeg.E4_DisplayOrder == Parent.E4_DisplayOrder))
					{
						Parent.E4_DisplayOrderInfo.AddError(Res.GetString("77d6e457-a319-43fe-a2e8-4a285bbdd066", "Display Orders must be sequential integers, continuously increasing by one."));
						return;
					}
				}
			}
		}

		protected override void CheckE4_ContainerMode()
		{
			base.CheckE4_ContainerMode();
			if (Parent.E4_ContainerMode.Length == 0)
			{
				Parent.E4_ContainerModeInfo.AddError(Res.GetString("40dd01bd-a4b6-47ed-9724-a0eabc215f97", "Container mode must have a value."));
			}
		}

		protected override void CheckE4_E5_FromOrg()
		{
			base.CheckE4_E5_FromOrg();

			if (Parent.GetType().IsAssignableFrom(typeof(CommonCartageLegType)))
			{
				if (Parent.E4_IsBooking)
				{
					if (Parent.E4_E5_FromOrg.IsEmpty)
					{
						Parent.E4_E5_FromOrgInfo.AddError(Res.GetString("a11b816d-2c92-47ed-aef7-f0db74a27afe", "First Party must have a value."));
					}
				}
				else
				{
					if (Parent.E4_E5_FromOrg.IsEmpty)
					{
						Parent.E4_E5_FromOrgInfo.AddError(Res.GetString("3917bb8e-7e4c-4b5d-8336-0edd37ecded8", "From organization must have a value."));
					}
				}
			}
		}

		protected override void CheckE4_E5_WaitPointOrg()
		{
			base.CheckE4_E5_WaitPointOrg();

			if (Parent.GetType().IsAssignableFrom(typeof(CommonCartageLegType)))
			{
				if (Parent.E4_IsBooking)
				{
					if (Parent.E4_E5_WaitPointOrg.IsEmpty)
					{
						Parent.E4_E5_WaitPointOrgInfo.AddError(Res.GetString("0dcf96bb-39de-4f7a-8da7-3dbedd896461", "Second Party must have a value."));
					}
				}
			}
		}

		protected override void CheckE4_E5_ToOrg()
		{
			base.CheckE4_E5_ToOrg();

			if (Parent.GetType().IsAssignableFrom(typeof(CommonCartageLegType)))
			{
				if (!Parent.E4_IsBooking && Parent.E4_E5_ToOrg.IsEmpty)
				{
					Parent.E4_E5_ToOrgInfo.AddError(Res.GetString("9b700668-f023-41a5-b4ff-cab56dfa5217", "To organization must have a value."));
				}
			}
		}

		#endregion

		#region Implementation

		CommonCartageType CommonCartageType
		{
			get
			{
				if (Parent is CommonCartageLegType)
				{
					return ((CommonCartageLegType)Parent).CommonCartageType;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion
	}
}
