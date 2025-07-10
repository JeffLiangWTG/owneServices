using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	public class LegsPopupModuleDecisionProvider : PopupModuleDecisionProvider
	{
		readonly string scacCode;

		public LegsPopupModuleDecisionProvider(IFindBox findBox, string scacCode) : base(findBox)
		{
			this.scacCode = scacCode;
		}

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			var selectedOrgHeaders = selectedObjects.Cast<OrgHeader>().ToList();

			if (selectedOrgHeaders.Count == 1)
			{
				var carrier = selectedOrgHeaders[0];

				if (carrier.SCACCode != ZString.Empty)
				{
					var resultMessage = Res.GetString("9BA1B0F2-9DAA-4757-A3D4-DF2FD3CEB734", "The carrier {0} already has SCAC {1}. Please choose another carrier.", carrier.OH_Code, carrier.SCACCode);
					Globals.Message.ShowError(resultMessage);
					return false;
				}

				var message = Res.GetString("D73180F3-4D22-405B-BED6-3F85AEC9BA16", "During this operation the SCAC code {0} will be assigned to the carrier {1}. Are you sure you want to proceed?", scacCode, carrier.OH_Code);
				var dialogResult = Globals.Message.Show(message, Res.GetString("52C6E994-1876-41DC-9E57-D0B4E254C4AA", "Assign SCAC code to carrier"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				return (dialogResult == DialogResult.Yes);
			}

			return false;
		}
	}
}
