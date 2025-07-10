using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class CreateConsolFromLoadListModuleDecisionProvider : ModuleDecisionProvider
	{
		public CreateConsolFromLoadListModuleDecisionProvider(BusinessObjectFactory factory, IFindBox findBox)
			: base(findBox, null)
		{
			Factory = factory;
		}

		public override bool AllowExcelExport => false;

		public override bool EnablePreviousNextSupport => false;

		public override IBusinessObjectCollection List => new CreatedConsolHVLVOriginLoadListCollection(Factory);

		public override bool ShouldDisplayNotifications => true;

		public override bool ShouldIgnoreAdditionalFilter => true;

		public override bool ShouldLoadFilterBizObj => true;

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			Contract.Assume(selectedObjects.Any());
			var loadLists = selectedObjects.Cast<HVLVOriginLoadList>().ToArray();
			return ValidateLoadListHasItems(loadLists)
				&& ValidateDetailsAreMatching(loadLists);
		}

		protected bool ValidateLoadListHasItems(HVLVOriginLoadList[] loadLists)
		{
			var validationResult = true;

			var loadListsWithoutItems = loadLists.Where(LoadListHasNoItems).ToArray();

			if (loadListsWithoutItems.Length > 0)
			{
				var dialogHeader = Res.GetString("01c6d841-1a5f-4dcd-8f7b-e4fd9fcdf5c8", "Cannot create Consol");
				var dialogMessage = Res.GetString("5e809975-cce4-4694-a5c1-ee049ec94273", @"The following eLoadLists do not have any Items attached:
{0}", string.Join(", ", loadListsWithoutItems.Select(x => x.HVL_UniqueReference)));

				Globals.Message.Show(dialogMessage, dialogHeader, MessageBoxButtons.OK, MessageBoxIcon.Error);

				validationResult = false;
			}

			return validationResult;
		}

		bool ValidateDetailsAreMatching(HVLVOriginLoadList[] loadLists)
		{
			var firstLoadList = loadLists[0];
			Func<HVLVOriginLoadList, bool> isMismatchingLoadList = loadList =>
				loadList.HVL_TransportMode != firstLoadList.HVL_TransportMode
				|| loadList.HVL_VesselName != firstLoadList.HVL_VesselName
				|| loadList.HVL_VoyageFlight != firstLoadList.HVL_VoyageFlight
				|| loadList.HVL_MasterBillNumber != firstLoadList.HVL_MasterBillNumber
				|| loadList.HVL_OH_Carrier != firstLoadList.HVL_OH_Carrier
				|| loadList.HVL_OA_OriginDepot != firstLoadList.HVL_OA_OriginDepot
				|| loadList.HVL_OA_DestinationDepot != firstLoadList.HVL_OA_DestinationDepot;

			if (loadLists.Skip(1).Any(isMismatchingLoadList) || !LoadListDatesAreWithin24HourPeriod(loadLists))
			{
				var dialogHeader = Res.GetString("65dd0cc9-d8fc-4926-b7bc-6115b8e3fb2f", "eLoadLists Confirmation");
				var dialogMessage = Res.GetString("9db5fcf2-edba-4540-b726-fdc1d1bbc878", "Selected eLoadLists do not have compatible Voyage/Flight, Master Bill, Carrier, Origin & Destination, and ETD & ETA Details. Are you sure you want to continue (the Consol will be created using one of the selected eLoadLists details)?");

				return Globals.Message.Show(dialogMessage, dialogHeader, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
			}

			return true;
		}

		BusinessObjectFactory Factory { get; }

		bool LoadListDatesAreWithin24HourPeriod(IEnumerable<HVLVOriginLoadList> loadLists)
		{
			Func<Func<HVLVOriginLoadList, ZDateTime>, bool> datesAreWithin24Hours = selector =>
			{
				var dates = loadLists.Select(selector).Where(x => !x.IsEmpty).ToArray();
				return dates.Length <= 1 || (dates.Max() - dates.Min()).Days < 1;
			};

			return datesAreWithin24Hours(x => x.HVL_E_Dep) && datesAreWithin24Hours(x => x.HVL_E_Arv);
		}

		bool LoadListHasNoItems(HVLVOriginLoadList loadList)
		{
			var query = new ZQuery(HVLVItemSchema.HVI_HVL_LoadList, loadList.PK);
			return !Factory.Exists(typeof(HVLVItem), query);
		}
	}
}
