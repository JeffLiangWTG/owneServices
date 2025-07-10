using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsChildValidation : AutoBulkMovementsChildValidation
	{
		public BulkMovementsChildValidation(AutoBulkMovementsChild parent)
			: base(parent) { }

		protected override void CheckMovementType()
		{
			base.CheckMovementType();
			MandatoryValidation.CheckEntered(Parent.MovementTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MovementTypeInfo, Parent.Lookups.MovementCodeList);
			ValidateDuplicate(Parent.MovementTypeInfo);
		}

		protected override void CheckOwnerType()
		{
			base.CheckOwnerType();
			MandatoryValidation.CheckEntered(Parent.OwnerTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OwnerTypeInfo, Parent.Lookups.OwnerTypeList);
		}

		protected override void CheckContainerNum()
		{
			base.CheckContainerNum();
			MandatoryValidation.CheckEntered(Parent.ContainerNumInfo);
			ValidateDuplicate(Parent.ContainerNumInfo);

			if (AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.Value)
			{
				ContainerNumberValidation.WarnIfInvalid(Parent.ContainerNumInfo);
			}
			else
			{
				ContainerNumberValidation.ErrorIfInvalid(Parent.ContainerNumInfo);
			}
		}

		protected override void CheckCondition()
		{
			base.CheckCondition();
			ListValidation.ErrorIfInvalidCode(Parent.ConditionInfo, Parent.Lookups.CleanCodeList);
		}

		protected override void CheckDamage()
		{
			base.CheckDamage();
			ListValidation.ErrorIfInvalidCode(Parent.DamageInfo, Parent.Lookups.DamageCodeList);
		}

		protected override void CheckContainerType()
		{
			base.CheckContainerType();
			MandatoryValidation.CheckEntered(Parent.ContainerTypeInfo);
			ListValidation.ErrorIfInvalidPK(Parent.ContainerTypeInfo, Parent.Lookups.ContainerTypeList);
		}

		protected override void CheckDepotAddressPK()
		{
			base.CheckDepotAddressPK();
			MandatoryValidation.CheckEntered(Parent.DepotAddressPKInfo);
		}

		protected override void CheckMovementDate()
		{
			base.CheckMovementDate();
			MandatoryValidation.CheckEntered(Parent.MovementDateInfo);
			ValidateDuplicate(Parent.MovementDateInfo);

			if (Parent.MovementDate > ZDateTime.Now)
			{
				Parent.MovementDateInfo.AddWarning(Res.GetString("33386407-49a7-43b7-80c4-faed94f782e0", "This date is in the future, you should only record movements that have actually taken place."));
			}
		}

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();
			ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo, Parent.Lookups.Principals);
		}

		protected override void CheckResponsiblePartyPK()
		{
			base.CheckResponsiblePartyPK();
			ListValidation.ErrorIfInvalidPK(Parent.ResponsiblePartyPKInfo, Parent.Lookups.ResponsibleParties);
		}

		#region Implementation

		void ValidateDuplicate(ZPropertyInfo info)
		{
			if (!Parent.IsGenerated && (Parent.IsDuplicate || Parent.MovementExists))
			{
				info.AddWarning(Res.GetString(
					"7aa4d300-dcd1-4fde-95b1-3ecb3518531d",
					"A movement with this type, date and time already exists for this container."));
			}
		}

		public new BulkMovementsChild Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsChild)base.Parent; }
		}

		#endregion
	}
}
