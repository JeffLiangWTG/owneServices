
namespace Enterprise.Customs.US.Business
{
	static class ReconTeamPortMapper
	{
		public static bool IsPortValidForRecon(string port)
		{
			return !string.IsNullOrEmpty(DefaultReconTeam(port));
		}

		public static string DefaultReconTeam(string port)
		{
			switch (port)
			{
				case ReconPortsList.Codes._0712:
					return ReconTeamsList.Codes._1R1;
				case ReconPortsList.Codes._0401:
					return ReconTeamsList.Codes._1R2;
				case ReconPortsList.Codes._1001:
					return ReconTeamsList.Codes._2R1;
				case ReconPortsList.Codes._2305:
					return ReconTeamsList.Codes._6R5;
				case ReconPortsList.Codes._2304:
					return ReconTeamsList.Codes._6R2;
				case ReconPortsList.Codes._2402:
					return ReconTeamsList.Codes._6R3;
				case ReconPortsList.Codes._2604:
					return ReconTeamsList.Codes._6R4;
				case ReconPortsList.Codes._2506:
					return ReconTeamsList.Codes._7R1;
				case ReconPortsList.Codes._2904:
					return ReconTeamsList.Codes._7R2;
				case ReconPortsList.Codes._3501:
					return ReconTeamsList.Codes._3R1;
				case ReconPortsList.Codes._3801:
					return ReconTeamsList.Codes._3RC;
				case ReconPortsList.Codes._5201:
					return ReconTeamsList.Codes._4R1;
				case ReconPortsList.Codes._5301:
					return ReconTeamsList.Codes._6RT;
#if DEBUG
				case "8888":
					return "8R8";
#endif
				default:
					return "";
			}
		}
	}
}
