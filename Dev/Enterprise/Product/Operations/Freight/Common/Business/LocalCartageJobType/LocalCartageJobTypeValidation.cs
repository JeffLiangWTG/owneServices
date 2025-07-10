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

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Common.Business
{
	public class LocalCartageJobTypeValidation : AutoLocalCartageJobTypeValidation
	{
		public LocalCartageJobTypeValidation(AutoLocalCartageJobType parent)
			: base(parent)
		{
		}

		protected override void CheckE3_JobType()
		{
			base.CheckE3_JobType();
			if (Parent.GetType().IsAssignableFrom(typeof(CommonCartageType)))
			{
				if (Parent.E3_JobType.Length != 4)
				{
					Parent.E3_JobTypeInfo.AddError(Res.GetString("b9c5abbe-461d-4340-b7c2-4c0a2df87d21", "Needs to be 4 characters"));
				}

				if (!Parent.E3_JobTypeInfo.HasErrors())
				{
					string directionChar = BindToLists.OneCharDirections.GetCodeFromDescription(CartageType.Direction);
					string actualDirectionChar = Parent.E3_JobType.SubstringSafe(0, 1);
					string postFix = Parent.E3_JobType.SubstringSafe(1);
					if (actualDirectionChar != directionChar)
					{
						Parent.E3_JobTypeInfo.AddError(Res.GetString("850aef42-e8ec-4a38-98d6-08ef4e7d677c", "'{0}'{1} does not match the Direction '{2}' ({3})", actualDirectionChar, postFix, directionChar, CartageType.Direction));
					}

					string freightChar = BindToLists.OneCharConnectingFreightModes.GetCodeFromDescription(CartageType.E3_ShippingTransportMode);
					string actualFreightChar = Parent.E3_JobType.SubstringSafe(1, 1);
					string preFix = Parent.E3_JobType.SubstringSafe(0, 1);
					postFix = Parent.E3_JobType.SubstringSafe(2);
					if (freightChar != actualFreightChar)
					{
						Parent.E3_JobTypeInfo.AddError(Res.GetString("0ef6bfc2-be02-4bfc-88c5-c90670b60fd6", "{0}'{1}' {2} does not match the Connection Freight Mode '{3}' ({4})", preFix, actualFreightChar, postFix, freightChar, CartageType.E3_ShippingTransportMode));
					}

					string containerChar = BindToLists.OneCharContainerModes.GetCodeFromDescription(CartageType.ContainerMode);
					string actualContainerChar = Parent.E3_JobType.SubstringSafe(2, 1);
					preFix = Parent.E3_JobType.SubstringSafe(0, 2);
					postFix = Parent.E3_JobType.SubstringSafe(3);
					if (containerChar != actualContainerChar)
					{
						Parent.E3_JobTypeInfo.AddError(Res.GetString("0ef6bfc2-be02-4bfc-88c5-c90670b60fd6", "{0}'{1}' {2} does not match the Connection Freight Mode '{3}' ({4})", preFix, actualContainerChar, postFix, containerChar, CartageType.ContainerMode));
					}
				}

				if (!Parent.E3_JobTypeInfo.HasErrors())
				{
					if (Parent.Factory.Load<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, Parent.E3_JobType)).Length > 1)
					{
						Parent.E3_JobTypeInfo.AddError(Res.GetString("fa058ce5-adc5-492b-8685-1d2e1c7db6cc", "Another Port Transport Job Type is already using this Code"));
					}
				}
			}
		}

		protected override void CheckE3_Description()
		{
			base.CheckE3_Description();
			MandatoryValidation.CheckEntered(Parent.E3_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.E3_DescriptionInfo);
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();
			if (Parent.GetType().IsAssignableFrom(typeof(CommonCartageType)))
			{
				ValidateOrganisationsNumber();
			}
		}

		public void ValidateOrganisationsNumber()
		{
			if (CartageType.CommonCartageOrganisations.Count > 4)//CommonCartage.MaxNumberOfJobDocAddresses)
			{
				Parent.AddRowError(Res.GetString("c1903059-faee-4523-8494-2ee34292c756", "Only 4 Organizations are supported on the Port Transport Screen."));
			}
		}

		CommonCartageType CartageType
		{
			get { return (CommonCartageType)Parent; }
		}

		#region BindToLists

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Parent.Factory); }
		}

		#endregion
	}
}
