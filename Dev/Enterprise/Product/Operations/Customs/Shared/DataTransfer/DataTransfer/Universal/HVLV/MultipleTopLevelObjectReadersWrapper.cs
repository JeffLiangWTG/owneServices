using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class MultipleTopLevelObjectReadersWrapper : ITopLevelDataObjectReader
	{
		public static ITopLevelDataObjectReader CreateWrapper(UniversalShipment shipment, bool isHVLVShipment, Func<UniversalShipment, IEnumerable<ITopLevelDataObjectReader>> createReadersForEachHLSShipment)
		{
			var readers = new List<ITopLevelDataObjectReader>();
			if (isHVLVShipment)
			{
				foreach (var hlsShipment in GetHVLVShipperConsolidations(shipment))
				{
					readers.AddRange(createReadersForEachHLSShipment(hlsShipment));
				}
			}
			else
			{
				readers.AddRange(createReadersForEachHLSShipment(null));
			}

			if (readers.Count == 0)
			{
				return null;
			}
			else if (readers.Count == 1)
			{
				return readers[0];
			}
			else
			{
				return new MultipleTopLevelObjectReadersWrapper(readers);
			}
		}

		static IEnumerable<UniversalShipment> GetHVLVShipperConsolidations(UniversalShipment dataObject)
		{
			if (dataObject != null && dataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					var shipmentType = subShipment.ShipmentType.GetCodeAsUpperCase();

					if (shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy
						|| shipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
					{
						yield return subShipment;
					}
					else
					{
						foreach (var hvlvShipment in GetHVLVShipperConsolidations(subShipment))
						{
							yield return hvlvShipment;
						}
					}
				}
			}
		}

		MultipleTopLevelObjectReadersWrapper(IEnumerable<ITopLevelDataObjectReader> readers)
		{
			this.readers = readers;
		}
		internal readonly IEnumerable<ITopLevelDataObjectReader> readers;

		public IEnumerable<BusinessObject> PopulatedBusinessObjects { get; private set; }

		public BusinessObject GetExistingBusinessObject()
		{
			return null;
		}

		public IEnumerable<BusinessObject> GetExistingBusinessObjects()
		{
			var results = new List<BusinessObject>();
			foreach (var reader in readers)
			{
				results.Add(reader.GetExistingBusinessObject());
			}

			return results.Distinct(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer);
		}

		public void ReadIntoBusinessObject(ref BusinessObject targetBO)
		{
			var results = new List<BusinessObject>();
			foreach (var reader in readers)
			{
				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				results.Add(bizObj);
			}

			PopulatedBusinessObjects = results.Distinct(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer);
		}

		public BusinessObject ReadIntoTopLevelBusinessObject()
		{
			var businessObject = (BusinessObject)null;
			ReadIntoBusinessObject(ref businessObject);
			return businessObject;
		}

		public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
		{
			return readers.SelectMany(r => r.ReadKeysForParallelism());
		}
	}
}
