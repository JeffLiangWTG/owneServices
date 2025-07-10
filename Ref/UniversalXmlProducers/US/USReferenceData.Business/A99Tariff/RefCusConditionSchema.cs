using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class RefCusConditionSchema
	{
		public string ConditionType { get; set; }
		public string ConditionTypeDataGrouping { get; set; }
		public string Severity { get; set; }
		public string IsImport { get; set; }
		public string IsExport { get; set; }
		public DateTime StartDate { get; set; } = DateTime.MinValue;
		public DateTime EndDate { get; set; } = DateTime.MinValue;
		public IEnumerable<RefCusApplicabilitySchema> Applicabilities { get; set; }
		public IEnumerable<RefCusConditionValueSchema> ConditionValues { get; set; }
	}
}
