using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class ActionTypeProvider : IActionType
	{
		public ActionTypeProvider(string action)
		{
			ActionCode = action;
		}

		public string ActionCode { get; }

		public Collection<string> ActionParameter => new Collection<string>();

		public string NarrativeText => "";
	}
}
