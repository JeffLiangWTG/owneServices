using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingContainer : AgencyShipmentContainer, Integration.Agency.IBillOfLadingContainer
	{
		public BillOfLadingContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Related BusinessObjects

		public new BillOfLading Booking
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLading)base.Booking; }
		}

		protected override Type ShipmentType
		{
			get { return typeof(BillOfLading); }
		}

		[ChildEditable(true)]
		public new BillOfLadingPackLineManyToManyCollection PackLines
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BillOfLadingPackLineManyToManyCollection)base.PackLines; }
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new BillOfLadingPackLineManyToManyCollection(this);
		}

		#endregion

		#region BusinessObject Overrides

		protected override AgencyShipmentContainerValidation GetNewContainerValidation()
		{
			AgencyShipmentContainerValidation result = new BillOfLadingBookingContainerValidation(this);
			JobContainerValidation extraValidation = StockManager.CreateExtraValidation();
			if (extraValidation != null)
			{
				result.Add(extraValidation);
			}

			return result;
		}

		#endregion

		#region Properties

		protected bool JC_DepartureDockReceipt_ReadOnly
		{
			get { return !Env.Security.AgencyBillOfLadingEditDockReceipt.IsAllowed; }
		}

		#endregion
	}
}


