using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class CloneProcessObject
	{
		public Guid DataProcessingClonePK { get; set; }
		public Guid ExpiredRecordPk { get; set; }
		public Guid NewRecordPk { get; set; }
		public IDictionary<string, List<Guid>> ExceptionListForCloning { get; set; }
	}
}
