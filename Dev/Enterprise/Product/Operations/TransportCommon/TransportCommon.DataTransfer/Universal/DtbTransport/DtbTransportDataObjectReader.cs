using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using Customs = Enterprise.Integration.Customs;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportDataObjectReader<T> : ShipmentDataObjectReader<T>
		where T : DtbTransport
	{
		protected DtbTransportDataObjectReader(UniversalShipment shipment, UniversalShipment topLevelDO, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalShipment sourceDO = null)
			: base(shipment, logger, factory)
		{
			TopLevelDO = topLevelDO;
			SourceDO = sourceDO;
		}

		protected readonly UniversalShipment TopLevelDO;
		protected readonly UniversalShipment SourceDO;

		#region PopulateRelatedEntities

		protected virtual void PopulateRelatedEntities(T transport)
		{
			PopulateAdditionalReferences(transport);
			PopulateNotes(transport);
		}

		#endregion

		#region PopulateAdditionalReferences

		protected void PopulateAdditionalReferences(T transport)
		{
			var dataObjectWithReferences = SourceDO ?? dataObject;
			var additionalReferenceCollection = GetAdditionalReferences(dataObjectWithReferences);
			RemoveUnnecessaryAdditionalReferences(ref additionalReferenceCollection, transport);
			if (additionalReferenceCollection != null)
			{
				var additionalReferenceCollectionReader = new TransportAdditionalReferenceCollectionReader<T>(additionalReferenceCollection, logger, factory, transport);
				additionalReferenceCollectionReader.ReadIntoCollection();
			}

			PopulateWayBills(GetAdditionalReferenceCollectionForWayBills(transport));
		}

		protected virtual void RemoveUnnecessaryAdditionalReferences(ref DataObjectList<AdditionalReference> additionalReferenceCollection, T transport)
		{
		}

		protected abstract DataObjectList<AdditionalReference> GetAdditionalReferences(UniversalShipment dataObject);
		protected abstract ICusEntryNumAdditionalReferenceCollection GetAdditionalReferenceCollectionForWayBills(T transport);

		#endregion

		#region PopulateAdditionalReferenceFromWayBill

		void PopulateWayBills(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			if (additionalReferenceNumbers != null)
			{
				var waybills = new List<(string WayBillType, ZString BillNumber)>();
				UpdateWayBillsFromShipment(waybills, dataObject);

				if (SourceDO != null)
				{
					UpdateWayBillsFromShipment(waybills, SourceDO);
				}

				UpdateWayBillsFromShipment(waybills, TopLevelDO);

				if (waybills.Count > 0)
				{
					RemoveWaybillReferences(additionalReferenceNumbers);

					foreach (var waybill in waybills)
					{
						PopulateWayBill(waybill.WayBillType, waybill.BillNumber, additionalReferenceNumbers);
					}
				}
			}
		}

		void UpdateWayBillsFromShipment(List<(string, ZString)> currentWayBills, UniversalShipment shipment)
		{
			if (shipment.WayBillType != null && shipment.WayBillNumber.HasValue)
			{
				AddWayBillIfAppropriate(currentWayBills, shipment.WayBillType.Code, shipment.WayBillNumber);
			}

			if (shipment.AdditionalBillCollection != null)
			{
				foreach (var additionalBill in shipment.AdditionalBillCollection)
				{
					var billNumber = additionalBill.BillNumber.GetValueOrDefault();
					if (!billNumber.IsEmpty)
					{
						var additionalRefType = GetAdditionalReferenceTypeFromWayBillType(additionalBill.BillType);
						AddWayBillIfAppropriate(currentWayBills, additionalRefType, billNumber);
					}
				}
			}
		}

		void AddWayBillIfAppropriate(List<(string WayBillType, ZString BillNumber)> result, string wayBillType, string wayBill)
		{
			var houseBillFound = result.Any(w => w.WayBillType == AdditionalReferenceTypes.Codes.HouseBill);
			if (!houseBillFound && HouseBillTypes.Contains(wayBillType))
			{
				result.Add((AdditionalReferenceTypes.Codes.HouseBill, wayBill));
			}
			else if (MasterBillTypes.Contains(wayBillType))
			{
				result.Add((AdditionalReferenceTypes.Codes.MasterBill, wayBill));
			}
		}

		IEnumerable<string> HouseBillTypes => new[] { WayBillTypeList.Codes.House, AdditionalReferenceTypes.Codes.HouseBill };
		IEnumerable<string> MasterBillTypes => new[] { WayBillTypeList.Codes.Master, AdditionalReferenceTypes.Codes.MasterBill };

		void RemoveWaybillReferences(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			foreach (Customs.ICusEntryNumber reference in additionalReferenceNumbers.ToArray())
			{
				if (reference.CE_EntryType == AdditionalReferenceTypes.Codes.HouseBill || reference.CE_EntryType == AdditionalReferenceTypes.Codes.MasterBill)
				{
					additionalReferenceNumbers.RemoveAndDelete(reference);
				}
			}
		}

		void PopulateWayBill(string wayBillType, ZString? billNumber, ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			var value = billNumber.Value.Trim();
			if (wayBillType == AdditionalReferenceTypes.Codes.MasterBill)
			{
				ZString masterBillSeparator = "-";
				var masterBillNum = FormatMasterBillNum(value, masterBillSeparator);
				AddOrUpdate(additionalReferenceNumbers, wayBillType, masterBillNum, ignoreChar: masterBillSeparator);
			}
			else
			{
				additionalReferenceNumbers.AddNewIfNotExist(wayBillType, value);
			}
		}

		ZString FormatMasterBillNum(ZString value, ZString masterBillSeparator)
		{
			var masterBillNum = value;
			var isAir = IsAir;

			var hasSeparator = value.Contains(masterBillSeparator, StringComparison.OrdinalIgnoreCase);
			if (isAir && !hasSeparator)
			{
				masterBillNum = value.FormatAirMAWB();
			}
			if (!isAir && hasSeparator)
			{
				masterBillNum = value.Replace(masterBillSeparator, "");
			}

			return masterBillNum;
		}

		void AddOrUpdate(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers, ZString type, ZString number, ZString ignoreChar)
		{
			if (!type.IsEmpty && !number.IsEmpty)
			{
				var numberSubstringSafe = number.SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
				var existingItem = additionalReferenceNumbers.Cast<Customs.ICusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == type && n.CE_EntryNum.Replace(ignoreChar, "") == numberSubstringSafe.Replace(ignoreChar, ""));
				if (existingItem == null)
				{
					additionalReferenceNumbers.AddNewIfNotExist(type, numberSubstringSafe);
				}
				else if (existingItem.CE_EntryNum != numberSubstringSafe)
				{
					SetValue((IColumnIndexer)existingItem, CusEntryNumSchema.CE_EntryNum, numberSubstringSafe);
				}
			}
		}

		bool IsAir
		{
			get { return dataObject.TransportMode.GetCodeAsUpperCase() == Core.Constants.TransportModes.Air; }
		}

		string GetAdditionalReferenceTypeFromWayBillType(WayBillType type)
		{
			// Sub House, Master House or "Waybills" become HouseBills
			return (type != null && type.Code.GetValueOrDefault() == WayBillTypeList.Codes.Master)
					? TransportCommonAdditionalReferenceTypes.Codes.MasterBill : TransportCommonAdditionalReferenceTypes.Codes.HouseBill;
		}

		#endregion

		#region PopulateNotes

		protected virtual void PopulateNotes(T transport)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, transport).ReadIntoCollection();
			}
		}

		#endregion
	}
}
