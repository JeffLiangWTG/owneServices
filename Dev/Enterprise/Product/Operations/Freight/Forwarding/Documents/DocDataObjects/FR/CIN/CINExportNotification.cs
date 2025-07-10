using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class CINExportNotification : DocDataObject, IDataSourceProvider
	{
		public CINExportNotification(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region Master Bill

		public ZString MasterBill
		{
			get => masterBill;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillInfo, ref masterBill, value))
				{
					Validate(MasterBillInfo);
				}
			}
		}
		ZString masterBill;

		public ZPropertyInfo MasterBillInfo => GetZPropertyInfo(nameof(MasterBill));

		public ZString MasterBillWithPrefix
		{
			get => masterBillWithPrefix;
			set
			{
				if (SetNonPersistentPropertyValue(MasterBillWithPrefixInfo, ref masterBillWithPrefix, value))
				{
					Validate(MasterBillWithPrefixInfo);
				}
			}
		}
		ZString masterBillWithPrefix;

		public ZPropertyInfo MasterBillWithPrefixInfo => GetZPropertyInfo(nameof(MasterBillWithPrefix));

		#endregion

		#region House Bill

		public ZString HouseBill
		{
			get => houseBill;
			set
			{
				if (SetNonPersistentPropertyValue(HouseBillInfo, ref houseBill, value))
				{
					Validate(HouseBillInfo);
				}
			}
		}
		ZString houseBill;

		public ZPropertyInfo HouseBillInfo => GetZPropertyInfo(nameof(HouseBill));

		#endregion

		#region Shipment ID

		public ZString ShipmentID
		{
			get => shipmentID;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentIDInfo, ref shipmentID, value))
				{
					Validate(ShipmentIDInfo);
				}
			}
		}
		ZString shipmentID;

		public ZPropertyInfo ShipmentIDInfo => GetZPropertyInfo(nameof(ShipmentID));

		#endregion

		#region MRNNumbers

		public IReadOnlyCollection<ReferenceNumber> MRNNumbers
		{
			get => mrnNumbers;
			set => mrnNumbers = SetChild(mrnNumbers, value);
		}

		IReadOnlyCollection<ReferenceNumber> mrnNumbers;

		#endregion

		#region Str MRNNumbers

		public ZString StrMRNNumbers
		{
			get => strMRNNumbers;
			set
			{
				if (SetNonPersistentPropertyValue(StrMRNNumbersInfo, ref strMRNNumbers, value))
				{
					Validate(StrMRNNumbersInfo);
				}
			}
		}
		ZString strMRNNumbers;

		public ZPropertyInfo StrMRNNumbersInfo => GetZPropertyInfo(nameof(StrMRNNumbers));

		#endregion

		#region Customs Office Code

		public ZString CustomsOfficeCode
		{
			get => customsOfficeCode;
			set
			{
				if (SetNonPersistentPropertyValue(CustomsOfficeCodeInfo, ref customsOfficeCode, value))
				{
					Validate(CustomsOfficeCodeInfo);
				}
			}
		}
		ZString customsOfficeCode;

		public ZPropertyInfo CustomsOfficeCodeInfo => GetZPropertyInfo(nameof(CustomsOfficeCode));

		#endregion

		#region PackCount

		public ZInt PackCount
		{
			get => packCount;
			set
			{
				if (SetNonPersistentPropertyValue(PackCountInfo, ref packCount, value))
				{
				}
			}
		}

		ZInt packCount;

		public ZPropertyInfo PackCountInfo => GetZPropertyInfo(nameof(PackCount));

		#endregion

		#region PackType

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}
		ICodeDescription packType;

		#endregion

		#region GrossWeight

		public IMeasurement GrossWeight
		{
			get => grossWeight;
			set => grossWeight = SetChild(grossWeight, value);
		}
		IMeasurement grossWeight;

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}
		IUnloco operationalPort;

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartyCIN

		public RegistrationNumber SendingPartyCINNumber
		{
			get => sendingPartyCINNumber;
			set => sendingPartyCINNumber = SetChild(sendingPartyCINNumber, value);
		}

		RegistrationNumber sendingPartyCINNumber;

		public ZString SendingPartyCIN
		{
			get => sendingPartyCIN;
			set
			{
				if (SetNonPersistentPropertyValue(SendingPartyCINInfo, ref sendingPartyCIN, value))
				{
					Validate(SendingPartyCINInfo);
				}
			}
		}
		ZString sendingPartyCIN;

		public ZPropertyInfo SendingPartyCINInfo => GetZPropertyInfo(nameof(SendingPartyCIN));

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierCIN

		public ZString CarrierCIN
		{
			get => carrierCIN;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierCINInfo, ref carrierCIN, value))
				{
					Validate(CarrierCINInfo);
				}
			}
		}
		ZString carrierCIN;

		public ZPropertyInfo CarrierCINInfo => GetZPropertyInfo(nameof(CarrierCIN));

		public RegistrationNumber CarrierCINNumber
		{
			get => carrierCINNumber;
			set => carrierCINNumber = SetChild(carrierCINNumber, value);
		}

		RegistrationNumber carrierCINNumber;

		#endregion

		#region Warehouse

		public Address Warehouse
		{
			get => warehouse;
			set => warehouse = SetChild(warehouse, value);
		}

		Address warehouse;

		#endregion

		#region WarehouseCIN

		public ZString WarehouseCIN
		{
			get => warehouseCIN;
			set
			{
				if (SetNonPersistentPropertyValue(WarehouseCINInfo, ref warehouseCIN, value))
				{
					Validate(WarehouseCINInfo);
				}
			}
		}
		ZString warehouseCIN;

		public ZPropertyInfo WarehouseCINInfo => GetZPropertyInfo(nameof(WarehouseCIN));

		public RegistrationNumber WarehouseCINNumber
		{
			get => warehouseCINNumber;
			set => warehouseCINNumber = SetChild(warehouseCINNumber, value);
		}

		RegistrationNumber warehouseCINNumber;

		#endregion

		#region AgentTypes

		public IReadOnlyCollection<CodeDescription> AgentTypes
		{
			get => agentTypes;
			set => agentTypes = SetChildCollection(agentTypes, value);
		}

		IReadOnlyCollection<CodeDescription> agentTypes;

		#endregion
	}
}
