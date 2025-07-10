using System.Diagnostics;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public class ParsingResult
	{
		public string TariffCode { get; }
		public bool IsValid { get; }
		public RefCusTariff Content { get; }
		public string ErrorMessage { get; }
		public string StackTrace { get; }
		public string ErrorDescription { get; }

		public bool IsCriticalError => !string.IsNullOrWhiteSpace(ErrorMessage) && ErrorMessage != ErrorMessagesConstant.NoNationalSection;

		public ParsingResult(string tariffCode, RefCusTariff content)
		{
			TariffCode = tariffCode;
			IsValid = true;
			Content = content;
		}

		public ParsingResult(string tariffCode, string errorMessage)
		{
			TariffCode = tariffCode;
			IsValid = false;
			ErrorMessage = errorMessage;

			var frame = new StackTrace().GetFrame(1);
			var callingMethod = frame.GetMethod();
			StackTrace = $"{callingMethod?.DeclaringType?.FullName} : {callingMethod?.Name}";
		}

		public ParsingResult(string tariffCode, string errorMessage, string description) : this(tariffCode, errorMessage)
		{
			ErrorDescription = description;

			var frame = new StackTrace().GetFrame(1);
			var callingMethod = frame.GetMethod();
			StackTrace = $"{callingMethod?.DeclaringType?.FullName} : {callingMethod?.Name}";
		}
	}
}
