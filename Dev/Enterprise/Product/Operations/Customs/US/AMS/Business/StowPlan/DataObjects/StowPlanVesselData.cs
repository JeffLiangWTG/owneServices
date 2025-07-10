using System;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanVesselData : AutoStowPlanVesselData, IStowPlanVesselData, IStowPlanNotificationProvider
	{
		public StowPlanVesselData(RefVessel vessel)
			: base(vessel.Factory)
		{
			this.vessel = vessel;
		}
		readonly RefVessel vessel;

		#region IStowPlanVesselData members

		public override ZString IMONumber
		{
			get { return vessel.RV_LloydsNumber; }
		}

		public override ZString VesselName
		{
			get { return vessel.RV_Code; }
		}

		public override ZString VesselOperator
		{
			get { return vessel.RV_CarrierCode; }
		}

		#endregion

		#region IStowPlanNotificationProvider members

		Guid IStowPlanNotificationProvider.TargetPK
		{
			get { return vessel.PK.ToGuid(); }
		}

		string IStowPlanNotificationProvider.TargetCode
		{
			get { return vessel.TablePrefix; }
		}

		string IStowPlanNotificationProvider.TargetSubject
		{
			get { return vessel.HumanReadableName; }
		}

		#endregion
	}
}
