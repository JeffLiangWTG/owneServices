using System.Windows.Forms;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Options.Testing
{
	[TestedType(typeof(DtbDocumentContainerOptionsForm))]
	public class DtbDocumentContainerOptionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var options = new TransportBookingDocumentOptions(null, Factory, DataContextType.DummyBusinessObject, DtbBookingDirection.DLV, false, "", "", false);
			return new DtbDocumentContainerOptionsForm(options);
		}
	}
}
