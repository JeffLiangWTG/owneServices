using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(TransportBookingDocumentOptions))]
	internal sealed class TransportBookingDocumentOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertiesFor8ParameterConstructor()
		{
			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false);
			var expectedDefaultTemplate = TransportRegistry.Instance.JobTemplateDefault.Value.GetBookingTemplate("SHP", "ORG", "SEA", "CNT", false);
			CombineAssertions("TransportBookingDocumentOptions should be set correctly for 8 parameter constructor", () =>
			{
				AssertEquals(DataContextType.ForwardingShipment, options.ParentDataContext);
				AssertEquals(DtbBookingDirection.PIC, options.Direction);
				AssertEquals(true, options.IsFCL);
				AssertEquals(true, options.ShowAutoDelivery);
				AssertEquals(true, options.ShowTemplateSelection);
				AssertEquals(false, options.ShowCommencedError);
				AssertEquals("If template constructor parameter not specified then set option template to default template as per matched registry row", expectedDefaultTemplate, options.Template);
			});
		}

		public void TestPropertiesFor9ParameterConstructorWithEmptyTemplate()
		{
			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false, string.Empty);
			var expectedDefaultTemplate = TransportRegistry.Instance.JobTemplateDefault.Value.GetBookingTemplate("SHP", "ORG", "SEA", "CNT", false);
			CombineAssertions("TransportBookingDocumentOptions should be set correctly for 9 parameter constructor with empty template parameter", () =>
			{
				AssertEquals(DataContextType.ForwardingShipment, options.ParentDataContext);
				AssertEquals(DtbBookingDirection.PIC, options.Direction);
				AssertEquals(true, options.IsFCL);
				AssertEquals(true, options.ShowAutoDelivery);
				AssertEquals(true, options.ShowTemplateSelection);
				AssertEquals(false, options.ShowCommencedError);
				AssertEquals("If template constructor parameter specified as empty string then set option template to default template as per matched registry row", expectedDefaultTemplate, options.Template);
			});
		}

		public void TestPropertiesFor9ParameterConstructorWithNonEmptyTemplate()
		{
			const string specifiedTemplate = "XXXX";
			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false, specifiedTemplate);

			CombineAssertions("TransportBookingDocumentOptions should be set correctly for 9 parameter constructor with specified non-empty template parameter", () =>
			{
				AssertEquals(DataContextType.ForwardingShipment, options.ParentDataContext);
				AssertEquals(DtbBookingDirection.PIC, options.Direction);
				AssertEquals(true, options.IsFCL);
				AssertEquals(true, options.ShowAutoDelivery);
				AssertEquals(true, options.ShowTemplateSelection);
				AssertEquals(false, options.ShowCommencedError);
				AssertEquals("If template constructor parameter specified then set option template to that value", specifiedTemplate, options.Template);
			});
		}

		public void TestPropertiesFor2ParameterConstructor()
		{
			var options = new TransportBookingDocumentOptions(Factory, DtbBookingDirection.PIC);
			CombineAssertions("TransportBookingDocumentOptions should be set correctly for 2 parameter constructor", () =>
			{
				AssertEquals(DtbBookingDirection.PIC, options.Direction);
				AssertEquals(true, options.ShowAutoDelivery);
				AssertEquals(false, options.ShowTemplateSelection);
				AssertEquals(false, options.ShowCommencedError);
			});
		}

		public void TestMessageLabelText()
		{
			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false);

			AssertEquals(@"Changes may have been made on the job that affect the Cartage Advice.
				These changes will not be reflected on the Cartage Advice as the booking has been overridden and is now managed independently of the job.", options.MessageLabelText.Caption);
			options.ShowCommencedError = true;
			AssertEquals(true, options.ShowCommencedError);
			AssertEquals(@"The Transport company has commenced work on transport related to this job.  If any changes have been made to the job it is necessary to advise the transport company manually.
Click ‘Deliver’ again to deliver the cartage advice with original details or select Open Transport Booking to action further.", options.MessageLabelText.Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransportBookingDocumentOptions(null, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, false, "SEA", "FCL", false);
		}
	}
}
