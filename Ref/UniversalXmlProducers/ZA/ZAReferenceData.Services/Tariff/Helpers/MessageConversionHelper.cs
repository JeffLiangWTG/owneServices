using System.Text;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers
{
	public static class MessageConversionHelper
	{
		public static Header Convert(SourceDataMessage msg, ILogger logger)
		{
			Header header = null;

			var edifactMessage = EdifactLoader.LoadProdatMessage(msg.Content);

			var validationErrors = new StringBuilder();

			if (ProdatValidator.ValidateMessage(edifactMessage, validationErrors))
			{
				header = ProdatLoader.PopulateHeader(edifactMessage);
			}
			else
			{
				logger.LogError($"Validation errors for: {msg.ID} {msg.Filename}");
				logger.LogError(validationErrors.ToString());
			}

			return header;
		}
	}
}
