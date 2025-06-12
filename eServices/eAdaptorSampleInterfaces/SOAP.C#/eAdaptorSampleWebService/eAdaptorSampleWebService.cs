using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using System.IO;
using System.ServiceModel;

namespace CargoWise.eAdaptorSampleWebService
{
	public class eAdaptorSampleWebService : IeAdapterStreamedService
	{
		public bool Ping()
		{
			return true;
		}

		public void SendStream(SendStreamRequest request)
		{
			string senderId = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;

			string destinationFolder = @"C:\eAdaptorSampleService";
#warning Please specify a destination folder for received messages. Make sure the folder exists on the machine that will run your eAdaptor Sample Web Service. You can then remove this warning.

			Validate(destinationFolder);

			foreach (var message in request.Messages)
			{
				string fileName = string.Format("Message_{1}_{0}.xml", message.MessageTrackingID, senderId);

				using (var fileStream = new FileStream(Path.Combine(destinationFolder, fileName), FileMode.Create))
				{
					message.MessageStream.DecodeAndDecompress().WriteTo(fileStream);
				}
			}
		}

		public void Validate(string destinationFolder)
		{
			if ((destinationFolder == "")) throw new FaultException("No destination folder for received messages has been specified. Please modify the eAdaptorSampleWebService.cs file located within the eAdaptor solution and specify a destination folder for received messages.");
		}
	}
}
