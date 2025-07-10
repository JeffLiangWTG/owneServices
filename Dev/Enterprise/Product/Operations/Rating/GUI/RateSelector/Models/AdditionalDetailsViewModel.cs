using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public sealed class AdditionalDetailsViewModel : ViewModelWithNotificationBase
	{
		public AdditionalDetailsViewModel(ICodeDescription codeDescription)
		{
			Argument.NotNull(codeDescription, nameof(codeDescription));

			Code = codeDescription.Code;
			Description = codeDescription.Description;
		}

		public AdditionalDetailsViewModel(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; private set; }

		public string Description { get; private set; }

		public override string ToString()
		{
			return $"{Code}: {Description}";
		}
	}
}
