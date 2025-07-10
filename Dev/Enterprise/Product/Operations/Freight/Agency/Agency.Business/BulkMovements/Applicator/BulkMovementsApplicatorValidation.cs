using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkMovementsApplicatorValidation : AutoBulkMovementsApplicatorValidation
	{
		public BulkMovementsApplicatorValidation(AutoBulkMovementsApplicator parent)
			: base(parent) { }

		protected override void CheckDepotAddressPK()
		{
			base.CheckDepotAddressPK();
			MandatoryValidation.CheckEntered(Parent.DepotAddressPKInfo);
		}

		protected override void CheckMovementType()
		{
			base.CheckMovementType();
			MandatoryValidation.CheckEntered(Parent.MovementTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MovementTypeInfo, Parent.Lookups.MovementTypes);
		}

		protected override void CheckMovementDate()
		{
			base.CheckMovementDate();
			MandatoryValidation.CheckEntered(Parent.MovementDateInfo);

			if (!Parent.MovementDate.IsEmpty && Parent.MovementDate > ZDateTime.Now)
			{
				Parent.MovementDateInfo.AddWarning(Res.GetString("833f9af7-7573-445f-bfb2-a157a8fddc23", "This date is in the future, you should only add movements that have actually taken place."));
			}
		}

		#region Implementation

		public new BulkMovementsApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkMovementsApplicator)base.Parent; }
		}

		#endregion
	}
}
