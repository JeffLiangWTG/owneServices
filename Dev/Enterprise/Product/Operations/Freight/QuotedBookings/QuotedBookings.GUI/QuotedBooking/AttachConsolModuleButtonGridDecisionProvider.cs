using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public class AttachConsolModuleButtonGridDecisionProvider : ModuleDecisionProvider
	{
		public AttachConsolModuleButtonGridDecisionProvider(BusinessObjectFactory factory, IFindBox findBox, ForwardingShipment booking)
			: base(findBox, null)
		{
			Argument.NotNull(booking, "booking");
			this.factory = factory;
			this.booking = booking;
		}

		public override bool AllowExcelExport
		{
			get { return false; }
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}

		public override bool ShouldLoadFilterBizObj
		{
			get { return false; }
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override bool AllowMultiSelect
		{
			get { return false; }
		}

		public override IBusinessObjectCollection List
		{
			get { return collection ?? (collection = new MainFormForwardingConsolCollection(factory, booking)); }
		}

		MainFormForwardingConsolCollection collection;

		protected override bool ValidateSelection(IEnumerable<BusinessObject> selectedObjects)
		{
			return AttachConsolToShipmentHelper.CheckAttaching(selectedObjects.ToList(), booking);
		}

		readonly BusinessObjectFactory factory;
		readonly ForwardingShipment booking;
	}
}
