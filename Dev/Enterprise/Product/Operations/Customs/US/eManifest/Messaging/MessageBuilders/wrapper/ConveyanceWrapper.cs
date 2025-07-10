using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class ConveyanceWrapper : IConveyance
	{
		public ConveyanceWrapper(Equipment conveyance)
		{
			this.conveyance = conveyance;
			refEquipment = conveyance.RefEquipment;
		}

		#region Implementation of IEquipment

		public ZString EquipmentType
		{
			get
			{
				var result = ZString.Empty;
				var refContainer = conveyance.RoadContainerType;
				if (refContainer != null)
				{
					result = refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
				}
				return result.IsEmpty ? (ZString)ConveyanceTypes.Codes.SemiTractor : result;
			}
		}

		public ZString EquipmentId
		{
			get { return conveyance.BJ_VIN; }
		}

		public IEnumerable<ILicensePlate> LicensePlates
		{
			get
			{
				if (refEquipment != null)
				{
					yield return new LicensePlateWrapper(refEquipment);
				}
				else if (!conveyance.BJ_RegistrationNumber.IsEmpty)
				{
					yield return new LicensePlateWrapper(conveyance);
				}
			}
		}

		public IEnumerable<ZString> SealNumbers
		{
			get { return from SealNumber number in conveyance.SealNumbers select number.CY_Data; }
		}

		#endregion

		#region Implementation of IConveyance

		public ZString ConveyanceACEId
		{
			get { return conveyance.BJ_ACEID; }
		}

		public ZString ConveyanceId
		{
			get { return conveyance.GetReferenceNumber(ConveyanceReferences.Codes.CarrierId); }
		}

		public ZString TransponderId
		{
			get { return refEquipment == null ? ZString.Empty : refEquipment.RQ_GateTransponder1; }
		}

		public IInsurance Insurance
		{
			get { return new InsuranceWrapper(conveyance); }
		}

		public IEnumerable<ZString> IITEntityIndicators
		{
			get { return conveyance.BJ_IITEntityIndicators.Split(2); }
		}

		#endregion

		readonly Equipment conveyance;
		readonly RefEquipment refEquipment;
	}
}
