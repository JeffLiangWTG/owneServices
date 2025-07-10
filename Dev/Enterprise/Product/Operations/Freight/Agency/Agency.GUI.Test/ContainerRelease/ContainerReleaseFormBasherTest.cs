using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(ContainerReleaseForm))]
	internal class ContainerReleaseFormBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.BookedContainers.AddNew();
			ReleaseHeader header = new ReleaseHeader(shipment, false);
			header.Init();
			return new ContainerReleaseForm(header);
		}
		#endregion
	}
}
