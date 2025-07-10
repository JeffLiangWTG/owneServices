namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class DeltaIECustomsProcedureUniversalReferenceDataFileGenerator : CustomsProcedureUniversalReferenceDataFileGenerator
	{
		public override string OutputFile => ApplicationConfig.Instance.FRDeltaIECustomsProcedureOutputFile;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Just switch Case with lot of case.")]
		protected override string GetProcedureGroupCore(string procedureCode, string previousProcedureCode)
		{
			var group = string.Empty;
			switch (procedureCode)
			{
				case "01":
				case "07":
					group += "H1,H6,I1";
					break;
				case "10":
					group += "B1,B4,C1";
					break;
				case "11":
				case "23":
				case "31":
					group += "B1,C1";
					break;
				case "21":
				case "22":
					group += "B2";
					break;
				case "40":
					group += "H1,H5,H6";
					if (previousProcedureCode == "00")
					{
						group += ",H7";
					}
					group += ",I1";
					break;
				case "42":
				case "61":
				case "63":
					group += "H1,H5,I1";
					break;
				case "43":
				case "44":
				case "45":
				case "46":
				case "48":
				case "68":
					group += "H1,I1";
					break;
				case "51":
					group += "H4,I1";
					break;
				case "53":
					group += "H3,I1";
					break;
				case "71":
					group += "H2";
					break;
				case "76":
				case "77":
					group += "B3";
					break;
				case "95":
				case "96":
					group += "H5";
					break;
			}
			return group;
		}

		protected override string GetDefaultDataGrouping() => UniversalDataHelper.Constants.DeltaIE;

		public override string DataSource => "FR - Delta IE Customs Procedures";
	}
}
