using System.Collections.Generic;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	class ImportManyImportGSSPayloadForTest : IImportManyGSSPayload
	{
		public List<IConnectionQuery> ConnectionQueries { get; set; }

		IEnumerable<IConnectionQuery> IImportManyGSSPayload.ConnectionQueries => ConnectionQueries;
	}
}
