using System;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IEHubClientSystemAccessor
	{
		void InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage(string systemID, string url, Guid trackingID, string senderID);
	}
}
