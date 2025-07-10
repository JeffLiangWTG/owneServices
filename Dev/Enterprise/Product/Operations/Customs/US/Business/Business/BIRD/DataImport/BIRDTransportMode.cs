using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	static class BIRDTransportMode
	{
		public static void SetTransportMode(JobDeclaration declaration, ZString modeOfTransportationMOTCode, INotifications notifications)
		{
			declaration.JE_TransportMode = TransportTypeList.ConvertFromTransportCode(modeOfTransportationMOTCode);

			if (TransportModeCodes.IsContainerised(modeOfTransportationMOTCode))
			{
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			}
			else
			{
				if (declaration.JE_ContainerMode.IsEmpty || !Core.Constants.ContainerModes.IsContainerised(declaration.JE_ContainerMode))
				{
					declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
				}
			}
		}
	}
}
