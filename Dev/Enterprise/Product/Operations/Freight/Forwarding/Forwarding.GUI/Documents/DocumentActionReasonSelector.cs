using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class DocumentActionReasonSelector : IDocumentActionReasonSelector
	{
		public DocumentActionReasonModel SelectDocumentActionReason(string message, string caption, ICodeDescriptionPairList optionsList)
		{
			using (var form = new DocumentActionReasonDialog(message, caption, optionsList))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					return form.model;
				}

				return null;
			}
		}
	}
}
