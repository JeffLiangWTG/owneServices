using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;
using HBLCodes = Enterprise.Core.Constants.HBLDeliveryModes.Codes;
using HBLDescriptions = Enterprise.Core.Constants.HBLDeliveryModes.Descriptions;

namespace Enterprise.Freight.Business
{
	[Immutable]
	[CodeAlive("WI00888867 baseline")]
	public class HBLDeliveryModeObject
	{
		HBLDeliveryModeObject(bool isEmpty)
		{
			_name = String.Empty;
			_description = String.Empty;
			_isEmpty = isEmpty;
			_isValid = false;
			_generalFlags = General.None;
			_requiredFields = Field.None;
			_pickupEndpointType = EndpointType.None;
			_deliveryEndpointType = EndpointType.None;
		}

		HBLDeliveryModeObject(string name, string description, EndpointType pickupEndpointType, EndpointType deliveryEndpointType, General generalFlags, Field requiredFields)
		{
			this._name = name;
			this._description = description;
			_isEmpty = false;
			_isValid = true;
			_generalFlags = generalFlags;
			_requiredFields = requiredFields;
			_pickupEndpointType = pickupEndpointType;
			_deliveryEndpointType = deliveryEndpointType;
		}

		readonly string _name;
		readonly string _description;
		readonly bool _isEmpty;
		readonly bool _isValid;
		readonly General _generalFlags;
		readonly Field _requiredFields;
		readonly EndpointType _pickupEndpointType;
		readonly EndpointType _deliveryEndpointType;

		public bool IsEmpty => _isEmpty;
		public bool IsValid => _isValid;

		public string Name => _name;
		public string Description => _description;

		public string LogString
		{
			get
			{
				if (_isEmpty)
				{
					return Res.GetString("27f46b20-af6a-4390-ad8e-7ca5c13bf688", "-");
				}

				if (!_isValid)
				{
					return Res.GetString("4165754c-731f-45eb-9e1e-61b361d30641", "-Invalid-");
				}

				return _name;
			}
		}

		public bool SupportsDDD => _generalFlags.HasFlag(General.SupportsDDD);
		public bool SupportsDTC => _generalFlags.HasFlag(General.SupportsDTC);
		public bool RequiresServiceLevel => _requiredFields.HasFlag(Field.ServiceLevel);
		public bool RequiresPickupCFS => _requiredFields.HasFlag(Field.PickupCFS);
		public bool RequiresDeliveryCFS => _requiredFields.HasFlag(Field.DeliveryCFS);

		public EndpointType PickupEndpointType => _pickupEndpointType;

		public bool IsAirportPickup => PickupEndpointType == EndpointType.Airport;
		public bool IsCFSPickup => PickupEndpointType == EndpointType.CFS;
		public bool IsCYPickup => PickupEndpointType == EndpointType.CY;
		public bool IsDoorPickup => PickupEndpointType == EndpointType.Door;
		public bool IsPortPickup => PickupEndpointType == EndpointType.Port;

		public EndpointType DeliveryEndpointType => _deliveryEndpointType;

		public bool IsAirportDelivery => DeliveryEndpointType == EndpointType.Airport;
		public bool IsCFSDelivery => DeliveryEndpointType == EndpointType.CFS;
		public bool IsCYDelivery => DeliveryEndpointType == EndpointType.CY;
		public bool IsDoorDelivery => DeliveryEndpointType == EndpointType.Door;
		public bool IsPortDelivery => DeliveryEndpointType == EndpointType.Port;

		public static HBLDeliveryModeObject Get(string name)
		{
			if (name.IsNullOrEmpty())
			{
				return Empty;
			}

			if (LookupTable.TryGetValue(name, out var result))
			{
				return result;
			}

			return Invalid;
		}

		public static IEnumerable<string> ListAllModeNames()
		{
			return LookupTable.Keys;
		}

		public static IEnumerable<HBLDeliveryModeObject> ListAllModes()
		{
			return LookupTable.Values;
		}

		static readonly HBLDeliveryModeObject _Empty = new (true);

		static readonly HBLDeliveryModeObject _Invalid = new (false);

		static readonly HBLDeliveryModeObject _ARPT_ARPT =
			new (HBLCodes.ARPT_ARPT,
				HBLDescriptions.ARPT_ARPT,
				EndpointType.Airport,
				EndpointType.Airport,
				General.SupportsDDD,
				Field.None);

		static readonly HBLDeliveryModeObject _ARPT_CFS =
			new(HBLCodes.ARPT_CFS,
				HBLDescriptions.ARPT_CFS,
				EndpointType.Airport,
				EndpointType.CFS,
				General.SupportsDDD,
				Field.ServiceLevel | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _ARPT_DOOR =
			new(HBLCodes.ARPT_DOOR,
				HBLDescriptions.ARPT_DOOR,
				EndpointType.Airport,
				EndpointType.Door,
				General.SupportsDDD,
				Field.ServiceLevel | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _CFS_ARPT =
			new(HBLCodes.CFS_ARPT,
				HBLDescriptions.CFS_ARPT,
				EndpointType.CFS,
				EndpointType.Airport,
				General.SupportsDDD,
				Field.ServiceLevel | Field.PickupCFS);

		static readonly HBLDeliveryModeObject _CFS_CFS =
			new(HBLCodes.CFS_CFS,
				HBLDescriptions.CFS_CFS,
				EndpointType.CFS,
				EndpointType.CFS,
				General.SupportsDDD,
				Field.ServiceLevel | Field.PickupCFS | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _CFS_CY =
			new(HBLCodes.CFS_CY,
				HBLDescriptions.CFS_CY,
				EndpointType.CFS,
				EndpointType.CY,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _CFS_DOOR =
			new(HBLCodes.CFS_DOOR,
				HBLDescriptions.CFS_DOOR,
				EndpointType.CFS,
				EndpointType.Door,
				General.SupportsDDD | General.SupportsDTC,
				Field.ServiceLevel | Field.PickupCFS | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _CY_CFS =
			new (HBLCodes.CY_CFS,
				HBLDescriptions.CY_CFS,
				EndpointType.CY,
				EndpointType.CFS,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _CY_CY =
			new(HBLCodes.CY_CY,
				HBLDescriptions.CY_CY,
				EndpointType.CY,
				EndpointType.CY,
				General.SupportsDDD,
				Field.None);

		static readonly HBLDeliveryModeObject _CY_DOOR =
			new(HBLCodes.CY_DOOR,
				HBLDescriptions.CY_DOOR,
				EndpointType.CY,
				EndpointType.Door,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _DOOR_ARPT =
			new(HBLCodes.DOOR_ARPT,
				HBLDescriptions.DOOR_ARPT,
				EndpointType.Door,
				EndpointType.Airport,
				General.SupportsDDD,
				Field.ServiceLevel | Field.PickupCFS | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _DOOR_CFS =
			new(HBLCodes.DOOR_CFS,
				HBLDescriptions.DOOR_CFS,
				EndpointType.Door,
				EndpointType.CFS,
				General.SupportsDDD,
				Field.ServiceLevel | Field.PickupCFS | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _DOOR_CY =
			new(HBLCodes.DOOR_CY,
				HBLDescriptions.DOOR_CY,
				EndpointType.Door,
				EndpointType.CY,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _DOOR_DOOR =
			new (HBLCodes.DOOR_DOOR,
				HBLDescriptions.DOOR_DOOR,
				EndpointType.Door,
				EndpointType.Door,
				General.SupportsDDD | General.SupportsDTC,
				Field.ServiceLevel | Field.PickupCFS | Field.DeliveryCFS);

		static readonly HBLDeliveryModeObject _DOOR_PORT =
			new(HBLCodes.DOOR_PORT,
				HBLDescriptions.DOOR_PORT,
				EndpointType.Door,
				EndpointType.Port,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _PORT_DOOR =
			new(HBLCodes.PORT_DOOR,
				HBLDescriptions.PORT_DOOR,
				EndpointType.Port,
				EndpointType.Door,
				General.None,
				Field.None);

		static readonly HBLDeliveryModeObject _PORT_PORT =
			new(HBLCodes.PORT_PORT,
				HBLDescriptions.PORT_PORT,
				EndpointType.Port,
				EndpointType.Port,
				General.SupportsDDD,
				Field.None);

		static readonly ImmutableSortedDictionary<string, HBLDeliveryModeObject> LookupTable = new Dictionary<string, HBLDeliveryModeObject>
		{
			{ HBLCodes.ARPT_ARPT, _ARPT_ARPT },
			{ HBLCodes.ARPT_CFS, _ARPT_CFS },
			{ HBLCodes.ARPT_DOOR, _ARPT_DOOR },
			{ HBLCodes.CFS_ARPT, _CFS_ARPT },
			{ HBLCodes.CFS_CFS, _CFS_CFS },
			{ HBLCodes.CFS_CY, _CFS_CY },
			{ HBLCodes.CFS_DOOR, _CFS_DOOR },
			{ HBLCodes.CY_CFS, _CY_CFS },
			{ HBLCodes.CY_CY, _CY_CY },
			{ HBLCodes.CY_DOOR, _CY_DOOR },
			{ HBLCodes.DOOR_ARPT, _DOOR_ARPT },
			{ HBLCodes.DOOR_CFS, _DOOR_CFS },
			{ HBLCodes.DOOR_CY, _DOOR_CY },
			{ HBLCodes.DOOR_DOOR, _DOOR_DOOR },
			{ HBLCodes.DOOR_PORT, _DOOR_PORT },
			{ HBLCodes.PORT_DOOR, _PORT_DOOR },
			{ HBLCodes.PORT_PORT, _PORT_PORT }
		}.ToImmutableSortedDictionary();

		public static HBLDeliveryModeObject Empty => _Empty;

		public static HBLDeliveryModeObject Invalid => _Invalid;

		public static HBLDeliveryModeObject ARPT_ARPT => _ARPT_ARPT;
		public static HBLDeliveryModeObject ARPT_CFS => _ARPT_CFS;
		public static HBLDeliveryModeObject ARPT_DOOR => _ARPT_DOOR;
		public static HBLDeliveryModeObject CFS_ARPT => _CFS_ARPT;
		public static HBLDeliveryModeObject CFS_CFS => _CFS_CFS;
		public static HBLDeliveryModeObject CFS_CY => _CFS_CY;
		public static HBLDeliveryModeObject CFS_DOOR => _CFS_DOOR;
		public static HBLDeliveryModeObject CY_CFS => _CY_CFS;
		public static HBLDeliveryModeObject CY_CY => _CY_CY;
		public static HBLDeliveryModeObject CY_DOOR => _CY_DOOR;
		public static HBLDeliveryModeObject DOOR_ARPT => _DOOR_ARPT;
		public static HBLDeliveryModeObject DOOR_CFS => _DOOR_CFS;
		public static HBLDeliveryModeObject DOOR_CY => _DOOR_CY;
		public static HBLDeliveryModeObject DOOR_DOOR => _DOOR_DOOR;
		public static HBLDeliveryModeObject DOOR_PORT => _DOOR_PORT;
		public static HBLDeliveryModeObject PORT_DOOR => _PORT_DOOR;
		public static HBLDeliveryModeObject PORT_PORT => _PORT_PORT;
	}

	[Flags]
	enum General
	{
		None = 0x0,
		SupportsDDD = 0x1,
		SupportsDTC = 0x2
	}

	[Flags]
	enum Field
	{
		None = 0x0,
		ServiceLevel = 0x1,
		PickupCFS = 0x2,
		DeliveryCFS = 0x4
	}

	public enum EndpointType
	{
		None,
		Airport,
		CFS,
		CY,
		Door,
		Port
	}
}
