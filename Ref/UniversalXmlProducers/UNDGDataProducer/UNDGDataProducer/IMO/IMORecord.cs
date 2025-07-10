namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class IMORecord
	{
		public string Variant { get; set; }
		public string CVL { get; set; }
		public string Variation { get; set; }
		public string SubLabel1 { get; set; }
		public string SubLabel2 { get; set; }
		public string EMS { get; set; }
		public string MP { get; set; }
		public string FP { get; set; }
		public string LQ { get; set; }
		public string EQ { get; set; }
		public string TechName { get; set; }
		public string TreatAs { get; set; }
		public string DGLPhrase { get; set; }
		public string Stow { get; set; }
		public string Seg { get; set; }
		public string SpecProv { get; set; }
		public string PackIns { get; set; }
		public string PackProv { get; set; }
		public string IbcIns { get; set; }
		public string IbcProv { get; set; }
		public string UNTankIns { get; set; }
		public string TankProv { get; set; }
		public string StowCat { get; set; }
		public string ExpLim { get; set; }
		public string UlineEMS { get; set; }
		public string UNNO { get; set; }
		public string Class { get; set; }
		public string PSN { get; set; }
		public string PG { get; set; }
		public string Code => UNNO + Variant;
	}
}
