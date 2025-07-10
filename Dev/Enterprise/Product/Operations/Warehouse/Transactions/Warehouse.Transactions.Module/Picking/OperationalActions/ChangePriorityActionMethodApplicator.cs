using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ChangePriorityActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public ChangePriorityActionMethodApplicator()
		: base(Res.GetString("ChangePriorityActionMethodApplicator|LoggingText", "Change Priority")) // text used for logging
		{
		}

		protected override void ApplyCore(Services.OperationalActions.Support.IOperationalActionSectionLog log, BusinessObject[] picks)
		{
		}

		#region Validation

		public ChangePriorityActionMethodApplicatorValdidation Validation => new ChangePriorityActionMethodApplicatorValdidation(this);

		#endregion
	}

	#region ChangePriorityActionMethodApplicatorValdidation

	/// <summary>
	/// This class is unnecessary and only exists to satisfy the Z-test for IObsoleteValidation.
	/// </summary>
	public class ChangePriorityActionMethodApplicatorValdidation : ZValidation
	{
		public ChangePriorityActionMethodApplicatorValdidation(ChangePriorityActionMethodApplicator parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(ChangePriorityActionMethodApplicator);

		public override void ValidateAll()
		{
		}
	}

	#endregion
}
