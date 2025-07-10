using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public static class JobDeclarationZControllerDecider
	{
		public static ZController GetZController(JobDeclaration declaration)
		{
			ZController result = null;

			if (declaration != null)
			{
				if (declaration.JE_MessageType == JobMessageTypeList.Codes.Import ||
					declaration.JE_MessageType == JobMessageTypeList.Codes.Export ||
					declaration.JE_MessageType == JobMessageTypeList.Codes.ImportByExternalBroker ||
					declaration.JE_MessageType == JobMessageTypeList.Codes.Miscellaneous ||
					declaration.JE_MessageType == JobMessageTypeList.Codes.FTZ)
				{
					if (declaration.Shipment == null)
					{
						result = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
					}
					else
					{
						result = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
					}
				}
				else if (declaration.JE_MessageType == JobMessageTypeList.MoreCodes.Protest)
				{
					result = ZControllerFactory.Create(ControllerIDs.Customs.US.Protest);
				}
				else if (declaration.JE_MessageType == JobMessageTypeList.Codes.Recon)
				{
					result = ZControllerFactory.Create(ControllerIDs.Customs.US.Recon);
				}
				else if (declaration.JE_MessageType == JobMessageTypeList.Codes.Drawback)
				{
					result = ZControllerFactory.Create(ControllerIDs.Customs.US.Drawback);
				}
			}
			return result;
		}
	}
}
