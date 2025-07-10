using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class LegValidation : ZValidation
	{
		readonly Leg leg;

		public LegValidation(Leg leg)
			: base(leg)
		{
			Argument.NotNull(leg, "leg");
			this.leg = leg;
		}

		public override Type AutoValidationType => typeof(LegValidation);

		#region Validate Methods

		public override void ValidateAll()
		{
			ValidateOriginPortUnloco();
			ValidateDestinationPortUnloco();
			ValidateDeparture();
			ValidateArrival();
			ValidateVoyageCode();
			ValidateVesselName();
			ValidateLloydsNumber();
			ValidateCarrierSCAC();
		}

		public void ValidateOriginPortUnloco()
		{
			ValidateCalculatedProperty(leg.OriginPortUnlocoInfo);
		}

		public void ValidateDestinationPortUnloco()
		{
			ValidateCalculatedProperty(leg.DestinationPortUnlocoInfo);
		}

		public void ValidateDeparture()
		{
			ValidateCalculatedProperty(leg.DepartureInfo);
		}

		public void ValidateArrival()
		{
			ValidateCalculatedProperty(leg.ArrivalInfo);
		}

		public void ValidateVoyageCode()
		{
			ValidateCalculatedProperty(leg.VoyageCodeInfo);
		}

		public void ValidateVesselName()
		{
			ValidateCalculatedProperty(leg.VesselNameInfo);
		}

		public void ValidateLloydsNumber()
		{
			ValidateCalculatedProperty(leg.LloydsNumberInfo);
		}

		public void ValidateCarrierSCAC()
		{
			ValidateCalculatedProperty(leg.CarrierSCACInfo);
		}

		#endregion

		#region Check Methods

		protected virtual void CheckOriginPortUnloco()
		{
			MandatoryValidation.CheckEntered(leg.OriginPortUnlocoInfo);
			ListValidation.ErrorIfInvalidCode(leg.OriginPortUnlocoInfo);
		}

		protected virtual void CheckDestinationPortUnloco()
		{
			MandatoryValidation.CheckEntered(leg.DestinationPortUnlocoInfo);
			ListValidation.ErrorIfInvalidCode(leg.DestinationPortUnlocoInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestValidateDeparture (LegValidationTests)")]
		void CheckDeparture()
		{
			if (leg.IsSea)
			{
				MandatoryCheckNotEmptyForSeaLeg(leg.DepartureInfo);
			}

			TypeValidation.CheckValidSmallDateTime(leg.DepartureInfo);
			TypeValidation.CheckValidZDateTimeRange(leg.DepartureInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests TestValidateVoyageCode_ValidationError_When_LegTypeIsNonSeaAndTooLong, TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndEmpty, TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndTooLong (LegValidationTests)")]
		void CheckVoyageCode()
		{
			if (leg.IsSea)
			{
				MandatoryCheckNotEmptyForSeaLeg(leg.VoyageCodeInfo);
			}

			if (!string.IsNullOrEmpty(leg.VoyageCode) && leg.VoyageCode.Length > JobVoyageSchema.JV_VoyageFlight.MaxLength)
			{
				leg.VoyageCodeInfo.AddError(ResString.GetMultilingualString("1b1fec1b-07e8-422d-a5a3-694544207bfb", "Maximum length of Voyage Code has been exceeded."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests TestValidateVesselName_ (LegValidationTests)")]
		void CheckVesselName()
		{
			if (!leg.IsSea)
			{
				return;
			}

			MandatoryCheckNotEmptyForSeaLeg(leg.VesselNameInfo);

			if (!string.IsNullOrEmpty(leg.VesselName))
			{
				if (leg.VesselName.Length > RefVesselSchema.RV_Code.MaxLength)
				{
					leg.VesselNameInfo.AddError(ResString.GetMultilingualString("b125cf39-1ecc-40d7-be3c-5fdee97605b1", "Maximum length of Vessel Name has been exceeded."));
					return;
				}

				var vessel = RefVessel.LookupVesselByName(leg.VesselName, leg.Factory, true).FirstOrDefault();

				if (vessel != null && !vessel.RV_IsActive && leg.Vessel == null)
				{
					leg.VesselNameInfo.AddError(ResString.GetMultilingualString("4D060BA9-9BFE-47F3-A550-0D2313758526", "Vessel with Vessel Name {0} exists in database and is inactive. Please go to Reference Files->Vessels and activate the vessel to be able to import schedule for it.", leg.VesselName));
					return;
				}

				if (!leg.LloydsNumber.IsEmpty)
				{
					var vessels = leg.Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, leg.LloydsNumber));

					if (vessels.Length == 1 && !vessels[0].RV_Name.EqualsIgnoringCase(leg.VesselName))
					{
						leg.VesselNameInfo.AddWarning(ResString.GetMultilingualString("47DE1A8B-C055-4906-848D-A702593BDFE9", "Vessel name of the vessel found by IMO number differs from the one provided by schedule service. Use right click popup menu to create vessel with Vessel Name from schedule service."));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests TestValidateLloydsNumber_ (LegValidationTests)")]
		void CheckLloydsNumber()
		{
			if (!leg.IsSea)
			{
				return;
			}

			if (leg.LloydsNumber.IsEmpty)
			{
				leg.LloydsNumberInfo.AddError(ResString.GetMultilingualString("5c98185f-e173-460e-822a-27560e08d5a1", "IMO Number cannot be empty for sea legs."));
			}

			if (!leg.LloydsNumber.IsEmpty)
			{
				var lloydsNumberValidation = new LloydsNumberValidation();
				lloydsNumberValidation.Validate(leg.LloydsNumber, (NoResString)"IMO number");

				if (!lloydsNumberValidation.IsValid)
				{
					leg.LloydsNumberInfo.AddWarning(lloydsNumberValidation.ErrorText);
					return;
				}

				var vessels = leg.Factory.Load<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, leg.LloydsNumber));

				if (vessels.Length > 1)
				{
					leg.LloydsNumberInfo.AddError(ResString.GetMultilingualString("2B78BBC4-6AAD-44B6-BA3E-DD48D3A34D3C", "More than one vessel found with IMO Number {0}.", leg.LloydsNumber));

					return;
				}

				if (vessels.Length == 0 && !leg.VesselName.IsEmpty)
				{
					var vessel = RefVessel.LookupVesselByName(leg.VesselName, leg.Factory).FirstOrDefault();

					if (vessel != null && !vessel.RV_LloydsNumber.IsEmpty && vessel.RV_LloydsNumber != "0")
					{
						leg.LloydsNumberInfo.AddError(ResString.GetMultilingualString("8EE479FC-1E44-45DB-BE52-0D18BD61C41F", "IMO Number of vessel found by vessel name differs from the one provided by schedule service. Use right click popup menu to update existing vessel with IMO from schedule service."));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for test TestValidateCarrierSCAC (LegValidationTests)")]
		void CheckCarrierSCAC()
		{
			CarrierSCACHelper.CheckCarrierSCAC(leg.CarrierSCACInfo, leg.Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for tests TestValidateVoyageCode_ValidationError_When_LegTypeIsNonSeaAndTooLong, TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndEmpty, TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndTooLong (LegValidationTests)")]
		void MandatoryCheckNotEmptyForSeaLeg(ZPropertyInfo propertyInfo)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddError(ResString.GetMultilingualString("e92a952d-d76c-460a-be4c-70f4a19dd11b", "{0} cannot be empty for sea legs.", propertyInfo.HumanReadableName));
			}
		}

		#endregion
	}
}
