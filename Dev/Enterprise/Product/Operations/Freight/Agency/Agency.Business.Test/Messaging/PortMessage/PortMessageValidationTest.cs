using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortMessageValidationTest : BaseAgencyTest
	{
		public void TestValidateDirection()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			var message = new PortMessage(voyage);

			message.Port = "AUBNE";
			message.Direction = Constants.PortDirection.Load;
			AssertNoNotifications(message.DirectionInfo);

			message.Direction = "Invalid";
			AssertHasError(message.DirectionInfo, "Enter a valid Direction.");

			message.Direction = Constants.PortDirection.Discharge;
			AssertNoNotifications(message.DirectionInfo);

			message.Direction = "";
			AssertHasError(message.DirectionInfo, "Please enter a Direction.");
		}

		public void TestValidatePort()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			var message = new PortMessage(voyage);
			message.Port = "";
			message.Validation.ValidatePort();
			AssertHasError(message.PortInfo, "Please enter a Port.");

			message.Port = "AUBNE";
			AssertNoNotifications(message.PortInfo);

			message.Port = "SGSIN";
			AssertHasError(message.PortInfo, "Enter a valid Port.");
		}
	}
}
