namespace Enterprise.eManifest.GUI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.eManifest.Business;
	using Enterprise.Freight.Business;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules.Internal;

	public class AttachELoadListModuleButtonGridDecisionProvider : ModuleDecisionProvider
	{
		public AttachELoadListModuleButtonGridDecisionProvider(BusinessObjectFactory factory, IFindBox findBox, CommonConsol consol)
			: base(findBox, null)
		{
			Argument.NotNull(consol, "consol");

			this.factory = factory;
			this.consol = consol;
		}

		public override bool AllowExcelExport
		{
			get { return false; }
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override IBusinessObjectCollection List
		{
			get
			{
				if (list == null)
				{
					list = new LodgedELoadListCollection(factory, consol);

					var defaultFilterProvider = ObjectFactory.Get<DefaultFilterProvider>("ELoadListForConsolFilterProvider", consol);
					defaultFilterProvider.SetDefaultFilters((IFilterBusinessObjectDefaultsProvider)list);
				}

				return list;
			}
		}

		IBusinessObjectCollection list;

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			var eLoadLists = selectedObjects.Cast<ELoadList>().ToList();
			var isValidSelection = true;

			Func<ELoadList, bool> commonDetailsMismatchWithConsol = eLoadList =>
			{
				return (!consol.JK_MasterBillNum.IsEmpty && eLoadList.DO_MasterBillNumber != consol.JK_MasterBillNum)
					|| (!consol.MostInterestingTransportForBinding[0].JW_Vessel.IsEmpty && eLoadList.DO_RV_NKVessel != consol.MostInterestingTransportForBinding[0].JW_Vessel)
					|| (!consol.MostInterestingTransportForBinding[0].JW_VoyageFlight.IsEmpty && eLoadList.DO_VoyageFlight != consol.MostInterestingTransportForBinding[0].JW_VoyageFlight);
			};

			if (eLoadLists.Any(commonDetailsMismatchWithConsol))
			{
				var header = Res.GetString("600e3a1d-c8f6-4a6d-85b7-8630afefcb0f", "eLoadLists Confirmation");
				var message = Res.GetString("0a628432-fd27-4b81-bee1-161c15c2492b", "Selected eLoadLists do not have same Voyage/Flight or Master Bill details as the Consol. Are you sure you want to continue?");

				var result = Globals.Message.Show(message, header, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
				isValidSelection = result == DialogResult.Yes;
			}

			return isValidSelection;
		}

		readonly BusinessObjectFactory factory;
		readonly CommonConsol consol;
	}
}
