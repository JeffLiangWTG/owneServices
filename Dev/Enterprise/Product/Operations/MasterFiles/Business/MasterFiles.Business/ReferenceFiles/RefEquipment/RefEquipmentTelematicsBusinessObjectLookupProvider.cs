using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentTelematicsBusinessObjectLookupProvider : ITelematicsBusinessObjectLookupProvider
	{
		#region ITelematicsBusinessObjectLookupProvider

		public ITelematicsBusinessObject GetBusinessObject(BusinessObjectFactory factory, ZGuid primaryKey)
		{
			var equipment = factory.Load<RefEquipment>(primaryKey);
			return equipment == null ? null : new EquipmentTelematicsBusinessObject(equipment);
		}

		#endregion

		#region Implementation

		class EquipmentTelematicsBusinessObject : ITelematicsBusinessObject
		{
			public EquipmentTelematicsBusinessObject(RefEquipment equipment)
			{
				if (equipment == null)
				{
					throw new ArgumentNullException(nameof(equipment));
				}

				this.equipment = equipment;
			}

			readonly RefEquipment equipment;

			#region ITelematicsBusinessObject

			public string Code
			{
				get { return equipment.RQ_ShortCode; }
			}

			public string DescriptionForInterface
			{
				get { return equipment.RQ_DescriptionMultilingual.ToString(Res.CurrentLanguage); }
			}

			public string DescriptionInEnglish
			{
				get { return equipment.RQ_DescriptionMultilingual.ToString(Res.DefaultLanguage); }
			}

			public string TypeIdentifier
			{
				get { return Res.GetString("RefEquipment", "Equipment"); }
			}

			#endregion
		}

		#endregion
	}
}
