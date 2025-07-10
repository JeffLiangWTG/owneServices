using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class JobContainerMoveValidation : AutoJobContainerMoveValidation
	{
		public JobContainerMoveValidation(AutoJobContainerMove parent)
			: base(parent) { }

		protected override void CheckE9_MovementType()
		{
			base.CheckE9_MovementType();
			MandatoryValidation.CheckEntered(Parent.E9_MovementTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.E9_MovementTypeInfo, Parent.Lookups.MovementCodeList);
		}

		protected override void CheckE9_MovementDate()
		{
			base.CheckE9_MovementDate();
			MandatoryValidation.CheckEntered(Parent.E9_MovementDateInfo);

			if (Parent.E9_MovementDate > ZDateTime.Now)
			{
				Parent.E9_MovementDateInfo.AddWarning(Res.GetString("a7b23a88-7eb2-4b7c-8752-0a8d604689a9", "This date is in the future, you should only record movements that have actually taken place here."));
			}
		}

		protected override void CheckE9_ContainerQuality()
		{
			base.CheckE9_ContainerQuality();
			ListValidation.ErrorIfInvalidCode(Parent.E9_ContainerQualityInfo, Parent.Lookups.CleanCodeList);
		}

		protected override void CheckE9_ContainerCondition()
		{
			base.CheckE9_ContainerCondition();
			ListValidation.ErrorIfInvalidCode(Parent.E9_ContainerConditionInfo, Parent.Lookups.DamageCodeList);
		}

		protected override void CheckE9_OA_Depot()
		{
			base.CheckE9_OA_Depot();
			MandatoryValidation.CheckEntered(Parent.E9_OA_DepotInfo);
		}

		protected override void CheckE9_OH_Principal()
		{
			base.CheckE9_OH_Principal();

			if (ContainerMovementTypes.RequiresClientAndPrincipal(Parent.E9_MovementType) && string.IsNullOrEmpty(Parent.RelatedInfo.Principal))
			{
				Parent.E9_OH_PrincipalInfo.AddWarning(Res.GetString("ecf11e87-5ad4-49a8-bd65-f8c1b7018166", "No principal could be detected for this movement, it is strongly recommended that one be entered here."));
			}
		}

		protected override void CheckE9_OH_ResponsibleParty()
		{
			base.CheckE9_OH_ResponsibleParty();

			if (ContainerMovementTypes.RequiresClientAndPrincipal(Parent.E9_MovementType) && string.IsNullOrEmpty(Parent.RelatedInfo.LocalClient))
			{
				Parent.E9_OH_ResponsiblePartyInfo.AddWarning(Res.GetString("adcdd549-0a17-414b-8746-fb5c1e7afddc", "No responsible party could be detected for this movement, it is strongly recommended that one be entered here."));
			}
		}

		protected override void CheckE9_DetentionDays()
		{
			base.CheckE9_DetentionDays();

			short? calculatedDays = Parent.DetentionStrategy.GetDefaultDetentionDays(Parent);

			if (calculatedDays.HasValue)
			{
				if (Parent.E9_DetentionDays != calculatedDays.Value)
				{
					Parent.E9_DetentionDaysInfo.AddWarning(Res.GetString("a8bd6a65-37c1-4ad5-b3ac-c7e4d8742367", "Detention days are calculated to be {0}.", calculatedDays.Value));
				}
			}
			else
			{
				if (Parent.E9_DetentionDays != 0)
				{
					Parent.E9_DetentionDaysInfo.AddError(Res.GetString("4fa3381a-f807-42f6-b12e-d29aa700f9a2", "Detention days are not supported on movements of this type."));
				}
			}
		}

		protected override void CheckE9_LeaseNumber()
		{
			base.CheckE9_LeaseNumber();

			var stock = Parent.Stock;
			if (stock != null && Parent.E9_MovementType != ContainerMovementTypes.Codes.OnHire)
			{
				var relevantOnHire = stock.Movements
					.Where(m => m.E9_MovementType == ContainerMovementTypes.Codes.OnHire
						&& m.E9_MovementDate <= Parent.E9_MovementDate)
					.OrderByDescending(m => m.E9_MovementDate)
					.FirstOrDefault();

				if (!string.IsNullOrEmpty(relevantOnHire?.E9_LeaseNumber))
				{
					var relevantOffHire = stock.Movements
						.Where(m => m.E9_MovementType == ContainerMovementTypes.Codes.OffHire
							&& m.E9_MovementDate > relevantOnHire.E9_MovementDate
							&& m.E9_MovementDate < Parent.E9_MovementDate)
						.OrderByDescending(m => m.E9_MovementDate)
						.FirstOrDefault();

					if (relevantOffHire == null)
					{
						if (Parent.E9_LeaseNumber.IsEmpty)
						{
							Parent.E9_LeaseNumberInfo.AddWarning(Res.GetString("42c1c88a-1170-47b4-8636-bc0878439b81", @"The Lease Contract is valid up to the next Off-Hire movement.
Add Lease Contract No to the movements of leased container for better tracking and reporting."));
						}
						else if (Parent.E9_LeaseNumber != relevantOnHire.E9_LeaseNumber)
						{
							Parent.E9_LeaseNumberInfo.AddWarning(Res.GetString("0b211846-1ebf-464b-bbb1-9c9dfec30bac", "The contract number entered does not match the contract number of the On-Hire movement."));
						}
					}
				}
			}
		}

		#region Implementation

		new ContainerMovement Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ContainerMovement)base.Parent; }
		}

		#endregion
	}
}



