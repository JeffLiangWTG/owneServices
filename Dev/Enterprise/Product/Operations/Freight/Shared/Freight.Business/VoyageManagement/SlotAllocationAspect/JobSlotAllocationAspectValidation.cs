namespace Enterprise.Freight.Business
{
	public sealed class JobSlotAllocationAspectValidation : AutoJobSlotAllocationAspectValidation
	{
		public JobSlotAllocationAspectValidation(AutoJobSlotAllocationAspect parent)
			: base(parent) { }

		protected override void CheckD5_ValueIsValidZDecimal()
		{
			// adding range validation here would duplicate the notifications shown by the wrapper object.
		}
	}
}
