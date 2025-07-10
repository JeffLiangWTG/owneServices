
using System.Diagnostics;

namespace Enterprise.eManifest.Business
{
	public class FlattenedSupplierBookingLineValidation : AutoFlattenedSupplierBookingLineValidation
	{
		public FlattenedSupplierBookingLineValidation(AutoFlattenedSupplierBookingLine parent)
			: base(parent) { }

		#region Implementation

		public new FlattenedSupplierBookingLine Parent
		{
			[DebuggerStepThrough]
			get { return (FlattenedSupplierBookingLine)base.Parent; }
		}

		#endregion
	}
}
