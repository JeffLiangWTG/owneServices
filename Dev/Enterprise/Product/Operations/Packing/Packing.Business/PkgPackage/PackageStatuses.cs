namespace Enterprise.Packing.Business
{
	public static class PackageStatuses
	{
		public static string Closed
		{
			get { return Res.GetString("807b95c1-6cc5-4967-8711-c98b56b5e748", "Closed"); }
		}

		public static string Released
		{
			get { return Res.GetString("35489607-8fbe-489d-9c76-088dce235c34", "Released"); }
		}

		public static string ReleasedViaJob
		{
			get { return Res.GetString("36c1cc92-e1cd-4640-b570-b41031f13cb0", "Released via Job"); }
		}
	}
}
