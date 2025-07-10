namespace Enterprise.Warehouse.Transactions.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Lucene Index strings are not translatable.")]
	static class WhsLuceneProcessTaskDefinitions
	{
		public static string ProcessTaskEntityType => "IProcessTask";
		public static string PK => "PK";
		public static string FormFlowType => "FORMFLOWTYPE";
		public static string Capability => "CAPABILITY";
		public static string IsInABuffer => "ISINABUFFER";
		public static string Staff => "Staff";
		public static string Status => "Status";
		public static string Reference => "Reference";
		public static string Client => "CLIENT";
		public static string Warehouse => "Warehouse";
		public static string AreaName => "AREANAME";
		public static string PickMethod => "PICKMETHOD";
		public static string UOMType => "UOMTYPE";
		public static string PickGroup => "PICKGROUP";
	}
}
