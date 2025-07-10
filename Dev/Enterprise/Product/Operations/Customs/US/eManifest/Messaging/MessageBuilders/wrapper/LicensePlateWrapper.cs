using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders.eManifest;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	internal class LicensePlateWrapper : ILicensePlate
	{
		public LicensePlateWrapper(Equipment equipment)
		{
			this.equipment = equipment;
		}

		public LicensePlateWrapper(RefEquipment refEquipment)
		{
			this.refEquipment = refEquipment;
		}

		#region Implementation of ILicensePlate

		public ZString LicensePlateNumber
		{
			get { return equipment != null ? equipment.BJ_RegistrationNumber : refEquipment.RQ_Registration; }
		}

		public ZString StateOrProvinceOfRegistration
		{
			get { return equipment != null ? equipment.BJ_RW_NKRegistrationState : refEquipment.RQ_RegState; }
		}

		public ZString CountryOfRegistration
		{
			get
			{
				return equipment != null ? equipment.BJ_RN_NKRegistrationCountry : refEquipment.RQ_RN_NKRegistrationCountry;
			}
		}

		#endregion

		readonly Equipment equipment;
		readonly RefEquipment refEquipment;
	}
}
