using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	public class TrackingConsol : ForwardingConsol
	{
		public TrackingConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new TrackingConsolShipmentCollection(this);
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			return new TrackingContainerCollection(this, Factory);
		}

		#endregion

		#region Properties

		public ZString PortOfLoadingPortName
		{
			get { return (LoadPort != null) ? LoadPort.RL_PortName : ZString.Empty; }
		}

		public ZString PortOfDischargePortName
		{
			get { return (DischargePort != null) ? DischargePort.RL_PortName : ZString.Empty; }
		}

		#endregion

		#region Suppress Flight Details

		public IFlightDetailsSuppression FlightDetailsSuppressionBizO { get; set; }

		public ZString VoyageFlightWithSuppression
		{
			get { return Suppression.GetWebValue(JK_JX_JV_VoyageFlight, FlightDetailsSuppressionBizO, SuppressFields.FlightNumber); }
		}

		public ZString MasterBillNumWithSuppression
		{
			get { return Suppression.GetWebValue(JK_MasterBillNum, FlightDetailsSuppressionBizO, SuppressFields.MasterBill); }
		}

		#region Property overrides

		[RequiresSuppression]
		public override ZString JK_JX_JV_VoyageFlight
		{
			get { return base.JK_JX_JV_VoyageFlight; }
		}

		[RequiresSuppression]
		public override ZString JK_MasterBillNum
		{
			get { return base.JK_MasterBillNum; }
			set { base.JK_MasterBillNum = value; }
		}

		#endregion

		#endregion
	}
}
