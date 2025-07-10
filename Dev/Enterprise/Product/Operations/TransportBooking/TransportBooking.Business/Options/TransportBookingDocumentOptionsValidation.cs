using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business.Options
{
	public class TransportBookingDocumentOptionsValidation : AutoTransportBookingDocumentOptionsValidation
	{
		public TransportBookingDocumentOptionsValidation(AutoTransportBookingDocumentOptions parent)
			: base(parent) { }

		protected override void CheckTemplate()
		{
			base.CheckTemplate();

			if (Parent.ShowTemplateSelection)
			{
				MandatoryValidation.CheckEntered(Parent.TemplateInfo);
				ListValidation.ErrorIfInvalidCode(Parent.TemplateInfo);
			}
		}

		public new TransportBookingDocumentOptions Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TransportBookingDocumentOptions)base.Parent; }
		}
	}
}
