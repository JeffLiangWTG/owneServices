using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsHeaderValidation : AutoBulkMovementsHeaderValidation
	{
		public BulkMovementsHeaderValidation(AutoBulkMovementsHeader parent)
			: base(parent) { }

		protected override void CheckMovementType()
		{
			base.CheckMovementType();
			ListValidation.ErrorIfInvalidCode(Parent.MovementTypeInfo, Parent.Lookups.MovementCodeList);
		}

		protected override void CheckMovementDate()
		{
			base.CheckMovementDate();

			if (Parent.MovementDate > ZDateTime.Now)
			{
				Parent.MovementDateInfo.AddWarning(Res.GetString("2bb745c3-e44c-4e24-ada4-70bc1e49b42e", "This date is in the future, you should only record movements that have actually taken place."));
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

		public new BulkMovementsHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsHeader)base.Parent; }
		}

		#endregion
	}
}
