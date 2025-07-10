using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportConsignment.Module.Shared
{
	public static class LandTransportInformationDisplayer
	{
		public static void ShowLandTransportDisabledInformation()
		{
			Globals.Message.ShowInformation(Res.GetString("B31E2E17-DE21-4D29-9382-B2857FC65DA3", "Land Transport is not enabled in your system, please request access by raising a CR9 incident."));
		}
	}
}
