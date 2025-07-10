using System.Collections.Generic;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public record ImportManyGSSError : IImportManyGSSError
	{
		public IConnectionQuery ConnectionQuery { get; set; }

		public ImportGSSResponseCode Code { get; set; }

		public IEnumerable<string> ErrorNotifications { get; set; }

		public string ErrorMessage { get; set; }
	}
}
