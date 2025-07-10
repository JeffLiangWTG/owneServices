using System;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	static class ValidationModesMessageTypesTranslator
	{
		public static ValidationModes GetValidationModesRelatedTo(ImportMessageStatusList.MessageType messageType)
		{
			switch (messageType)
			{
				case ImportMessageStatusList.MessageType.BorderCargoRelease:
				case ImportMessageStatusList.MessageType.CargoRelease:
				case ImportMessageStatusList.MessageType.ACECargoRelease:
					return ValidationModes.CargoRelease;
				case ImportMessageStatusList.MessageType.EntrySummary:
					return ValidationModes.EntrySummary;
				default:
					throw new NotSupportedException("Translation on this type of message is not supported");
			}
		}
	}
}
