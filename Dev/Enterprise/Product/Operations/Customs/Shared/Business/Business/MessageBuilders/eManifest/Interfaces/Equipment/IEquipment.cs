namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;
	using CargoWise.Types;

	public interface IEquipment
	{
		/// <summary>
		/// Coded identification of the type uniquely identifying each article of transport equipment. 
		/// US,CA: (C/2), Condition: if equipment is filed.
		/// </summary>
		ZString EquipmentType { get; }

		/// <summary>
		/// Number shown on the equipment - Mark/Initial (if applicable) + Number. 
		/// US,CA: (C/18), Condition: specify for Equipment without license plates. 
		/// (For Equipment with License Plate, License plate information should be provided)
		/// </summary>
		ZString EquipmentId { get; }

		/// <summary>
		/// License plates of equipment. 
		/// US,CA: (C), Condition: specify for Equipment with license plates.
		/// (For Equipment without License Plates, Equipment Number should be provided)
		/// </summary>
		IEnumerable<ILicensePlate> LicensePlates { get; }

		/// <summary>
		/// Seal numbers as applicable, attached to prevent tampering.
		/// This is to be provided if the Conveyance has the seals. If the Equipment has the Seals, then the Equipment-Seals portions should be submitted.
		/// US,CA: (C/15), Condition: May become Mandatory for certain Equipment types
		/// </summary>
		IEnumerable<ZString> SealNumbers { get; }
	}
}
