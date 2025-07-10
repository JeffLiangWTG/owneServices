using System.Collections.Generic;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	record ImportGSSResponse : IImportGSSResponse
	{
		public ImportGSSResponseCode Code { get; set; }
		public string Message { get; set; }
		public string ErrorMessage { get; set; }
		public IEnumerable<string> ErrorNotifications { get; set; }
		public List<ISailingModel> Sailings { get; set; }
		IEnumerable<ISailingModel> IImportGSSResponse.Sailings => Sailings;
	}
}
