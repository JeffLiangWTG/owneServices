using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public interface IMessageHandler : IDisposable
	{
		IEnumerable<SourceDataMessage> GetMessages();
		void UpdateStatus(SourceDataMessage msg, bool success);
	}
}
