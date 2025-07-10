using System.Collections.Generic;
using WTG.ROPE.Model;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.GUI
{
	[CodeAlive("CreditReportInformationWindow, deleted in WI00756942, see respective WI's e-doc for details.")]
	public class RegistrationNumberModel : ModelBase<RegistrationNumberModel>
	{
		public RegistrationNumberModel(string oldRegistryNumber, string newRegistrationNumber, IdentifierType identifierType)
		{
			CurrentRegistryNumber = oldRegistryNumber;
			NewRegistryNumber = newRegistrationNumber;
			RegistryNumberType = identifierType.ToString();
			if (string.IsNullOrEmpty(CurrentRegistryNumber))
			{
				SelectedMergeAction = MergeAction.Codes.Add;
				MergeActions = MergeAction.GetAddActions();
			}
			else
			{
				SelectedMergeAction = MergeAction.Codes.Update;
				MergeActions = MergeAction.GetUpdateActions();
			}
		}

		public string CurrentRegistryNumber { get; }

		public string NewRegistryNumber { get; }

		public string RegistryNumberType { get; }

		public string SelectedMergeAction { get; set; }

		public Dictionary<string, string> MergeActions { get; }
	}
}
