using System.Collections.Generic;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public record ImportManyGSSResponse : IImportManyGSSResponse
	{
		public bool Success { get; set; }

		public List<List<List<ISailingModel>>> Sailings { get; set; }

		public IImportManyGSSError Error { get; set; }
	}
}
