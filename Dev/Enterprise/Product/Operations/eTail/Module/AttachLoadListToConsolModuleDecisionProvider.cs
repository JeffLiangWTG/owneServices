using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	public class AttachLoadListToConsolModuleDecisionProvider : CreateConsolFromLoadListModuleDecisionProvider
	{
		public AttachLoadListToConsolModuleDecisionProvider(ForwardingConsol forwardingConsol, IFindBox findBox)
			: base(forwardingConsol.Factory, findBox)
		{
			Consol = forwardingConsol;
		}

		public override bool ShouldLoadFilterBizObj => false;

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			Contract.Assume(selectedObjects.Any());
			var loadLists = selectedObjects.Cast<HVLVOriginLoadList>().ToArray();

			return ValidateLoadListHasItems(loadLists)
				&& ValidateDetailsAreMatching(loadLists);
		}

		bool ValidateDetailsAreMatching(HVLVOriginLoadList[] loadLists)
		{
			var validationResult = true;
			var firstLoadList = loadLists[0];
			Func<HVLVOriginLoadList, bool> isMatchingConsolMasterBill = loadList =>
				loadList.HVL_MasterBillNumber != Consol.JK_MasterBillNum;

			if (loadLists.Any(isMatchingConsolMasterBill))
			{
				var dialogHeader = Res.GetString("80cc91a7-7a31-4f38-8658-68a629c6e0ae", "Attach Load List Confirmation");
				var dialogMessage = Res.GetString("efb780da-7f42-40d2-a9cd-d34f60177e49", "Selected HVLV Origin Load List(s) do not have matching Master Bill with this Consol, do you wish to proceed?");

				return Globals.Message.Show(dialogMessage, dialogHeader, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
			}

			return validationResult;
		}

		public override IBusinessObjectCollection List
		{
			get
			{
				if (list == null)
				{
					var loadLists = new AttachedHVLVOriginLoadListCollection(Consol.Factory, Consol.JK_MasterBillNum);
					var filterProvider = new HVLVOriginLoadListFilterProvider();

					filterProvider.MasterBill = Consol.JK_MasterBillNum;
					if (Consol.JK_TransportMode == Core.Constants.TransportModes.Air)
					{
						filterProvider.Voyage = Consol.JK_JX_JV_VoyageFlight;
					}
					else if (Consol.JK_TransportMode == Core.Constants.TransportModes.Sea)
					{
						filterProvider.Vessel = Consol.JK_JX_JV_NKVessel;
					}

					filterProvider.ETDFrom = Consol.JK_JX_JA_E_DEP;
					filterProvider.ETATo = Consol.JK_JX_JB_E_ARV;

					filterProvider.SetDefaultFilters(loadLists);
					list = loadLists;
				}

				return list;
			}
		}

		IBusinessObjectCollection list;

		ForwardingConsol Consol { get; }
	}
}
