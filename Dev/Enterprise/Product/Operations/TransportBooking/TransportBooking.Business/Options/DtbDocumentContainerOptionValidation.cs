
namespace Enterprise.TransportBookings.Business.Options
{
	public class DtbDocumentContainerOptionValidation : AutoDtbDocumentContainerOptionValidation
	{
		public DtbDocumentContainerOptionValidation(AutoDtbDocumentContainerOption parent)
			: base(parent) { }

		public new DtbDocumentContainerOption Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DtbDocumentContainerOption)base.Parent; }
		}
	}
}
