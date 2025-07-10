using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class TripValidation : CusInBondHeaderValidation
	{
		public TripValidation(Trip parent)
			: base(parent)
		{
		}

		new Trip Parent
		{
			get { return (Trip)base.Parent; }
		}

		#region CheckBH_CarrierSCAC

		protected override void CheckBH_CarrierSCAC()
		{
			base.CheckBH_CarrierSCAC();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_CarrierSCACInfo);
			if (!Parent.BH_CarrierSCAC.IsEmpty && !Parent.BH_CarrierSCACInfo.HasNotifications() && Parent.CarrierSCAC == null)
			{
				Parent.BH_CarrierSCACInfo.AddWarning("There should be an organization with the carrier SCAC specified in order to print CBP Form 7533.");
			}
		}

		#endregion

		#region CheckBH_ETA

		protected override void CheckBH_ETA()
		{
			base.CheckBH_ETA();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ETAInfo);

			var info = Parent.BH_ETAInfo;
			if (!info.BizObj.IsInDatabase || !info.OriginalValue.Equals(info.Value))
			{
				var eta = Parent.BH_ETA;
				if (eta.IsValid && !eta.IsEmpty)
				{
					var now = ZDateTime.Now;
					if (eta < now)
					{
						info.AddError("Estimated Date of Arrival cannot be a past date.");
					}
					else if ((eta - now) < new TimeSpan(0, 30, 0))
					{
						info.AddWarning("e-Manifest should be lodged at least 30 mins before arrival.");
					}
				}
			}
			ValidateBH_VoyageNumber();
		}

		#endregion

		#region CheckBH_ImportTransportMode

		protected override void CheckBH_ImportTransportMode()
		{
			base.CheckBH_ImportTransportMode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_ImportTransportModeInfo);
		}

		#endregion

		#region CheckBH_JobReference

		protected override void CheckBH_JobReference()
		{
			base.CheckBH_JobReference();

			if (Parent.ValidateAllHasBeenRun)
			{
				ValidateAtLeastResponsiblePartyEntered(Parent.BH_JobReferenceInfo);
			}
		}

		void ValidateAtLeastResponsiblePartyEntered(ZPropertyInfo notificationInfo)
		{
			if (Parent.CrewMembers.All(crew => crew.CP_Type != CrewTypes.Codes.ResponsibleParty))
			{
				notificationInfo.AddMessageError(@"Responsible party is required on the Crew tab.
The responsible party may also be the driver, passenger, or crew member.
If this is the case, use RP in this data element to report that person.");
			}
		}

		#endregion

		#region CheckBH_PortUnladingDCode

		protected override void CheckBH_PortUnladingDCode()
		{
			base.CheckBH_PortUnladingDCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_PortUnladingDCodeInfo);
		}

		#endregion

		#region CheckBH_RL_NKPortUnlading

		protected override void CheckBH_RL_NKPortUnlading()
		{
			base.CheckBH_RL_NKPortUnlading();
			ListValidation.MessageErrorIfInvalidCode(Parent.BH_RL_NKPortUnladingInfo);
		}

		#endregion

		#region CheckBH_TransitDirection

		protected override void CheckBH_TransitDirection()
		{
			base.CheckBH_TransitDirection();
			ListValidation.MessageErrorIfInvalidCode(Parent.BH_TransitDirectionInfo);
		}

		#endregion

		protected override void CheckBH_VoyageNumber()
		{
			base.CheckBH_VoyageNumber();
			if (Parent.BH_VoyageNumber.Length <= 1 || !Parent.BH_VoyageNumber.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.BH_VoyageNumberInfo.AddMessageError("Trip reference should be 2-10 alphanumeric characters");
			}
			else if (Parent.BH_ETA.IsValid)
			{
				var q = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, "MAN");
				q.AddToFilter(CusInBondHeaderSchema.BH_VoyageNumber, Parent.BH_VoyageNumber);
				q.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				q.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.BH_ETA.AddYears(-1));
				q.AddToFilter(CusInBondHeaderSchema.BH_ETA, SQLComparisonOperator.LessThanOrEqualTo, Parent.BH_ETA.AddYears(1));
				var otherTrip = Parent.Factory.LoadTop1<Trip>(q);
				if (otherTrip != null)
				{
					Parent.BH_VoyageNumberInfo.AddMessageError("Trip reference is already in use on job " + otherTrip.BH_JobReference + ", the trip reference must be unique for a period of one year of Estimated Date of Arrival.");
				}
			}
		}

		protected override void CheckBH_OA_Importer()
		{
			base.CheckBH_OA_Importer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_OA_ImporterInfo);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			Parent.ValidateAllHasBeenRun = true;
			base.ValidateAll();
		}

		#endregion
	}
}
