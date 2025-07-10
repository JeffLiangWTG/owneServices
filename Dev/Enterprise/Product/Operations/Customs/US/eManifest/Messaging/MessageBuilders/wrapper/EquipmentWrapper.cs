using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class EquipmentWrapper : IEquipment
	{
		public EquipmentWrapper(Equipment equipment)
		{
			this.equipment = equipment;
			refEquipment = equipment.RefEquipment;
		}

		#region Implementation of IEquipment

		public ZString EquipmentACEId
		{
			get { return equipment.GetReferenceNumber(ConveyanceReferences.Codes.ACEId); }
		}

		public ZString EquipmentId
		{
			get { return ZString.Empty; }
		}

		public ZString EquipmentType
		{
			get
			{
				var result = ZString.Empty;
				if (equipment != null)
				{
					var refContainer = equipment.RoadContainerType;
					if (refContainer != null)
					{
						result = refContainer.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return result;
			}
		}

		public IEnumerable<ILicensePlate> LicensePlates
		{
			get
			{
				if (refEquipment != null)
				{
					yield return new LicensePlateWrapper(refEquipment);
				}
				else if (!equipment.BJ_RegistrationNumber.IsEmpty)
				{
					yield return new LicensePlateWrapper(equipment);
				}
			}
		}

		public IEnumerable<ZString> SealNumbers
		{
			get { return from SealNumber number in equipment.SealNumbers select number.CY_Data; }
		}

		public IEnumerable<ZString> IITEntityIndicators
		{
			get { return equipment.BJ_IITEntityIndicators.Split(2); }
		}

		#endregion

		readonly Equipment equipment;
		readonly RefEquipment refEquipment;
	}
}
