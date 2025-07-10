using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusMAWBDataObjectReader : DataTransfer.Universal.AirManifest.CusMAWBDataObjectReader<CusMAWB, CusHAWB, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper>
	{
		public CusMAWBDataObjectReader(Shipment mawbDataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid shipmentPK, bool singleHAWBCheck = false)
			: base(mawbDataObject, hVLVShipperConsolidation, logger, factory, Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff, singleHAWBCheck) // TODO : Check if this happens in NZ as well
		{
			this.shipmentPK = shipmentPK;
		}

		public CusMAWBDataObjectReader(CusMAWB existingMawb, IEnumerable<ZString> hawbsToBeInserted, Shipment mawbDataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory, ZGuid shipmentPK, bool singleHAWBCheck = false)
			: base(mawbDataObject, hVLVShipperConsolidation, logger, factory, Enterprise.Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff, singleHAWBCheck)
		{
			shouldLookForExistingMawb = false;
			this.existingMawb = existingMawb;
			this.hawbsToBeInserted = hawbsToBeInserted?.ToHashSet();
			this.shipmentPK = shipmentPK;
		}

		readonly bool shouldLookForExistingMawb = true;
		readonly CusMAWB existingMawb;
		readonly HashSet<ZString> hawbsToBeInserted;
		readonly ZGuid shipmentPK;

		protected override CusMAWB GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return shouldLookForExistingMawb ? new CusMAWB.Loader(factory.BOFactory).FindMatchingMAWBs(dataObject, HVLVShipperConsolidation).FirstOrDefault() : existingMawb;
		}

		protected override IEnumerable<DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper>> GetNewCusHAWBDataObjectReaders(Shipment shipmentDataObject, Shipment mawbDataObject, CusMAWB mawb)
		{
			if (shipmentDataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipmentDataObject.SubShipmentCollection)
				{
					foreach (var hawbReader in GetNewCusHAWBDataObjectReaders(subShipment, mawbDataObject, mawb))
					{
						yield return hawbReader;
					}
				}
			}
			else
			{
				var hawbReader =
					IsHVLV
					? new ETailCusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, Helper, mawb, null, shipmentPK, singleHAWBCheck)
					: new CusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, Helper, mawb, null, IsHVLV, singleHAWBCheck);
				if (hawbsToBeInserted == null || hawbsToBeInserted.Contains(shipmentDataObject.WayBillNumber.GetValueOrDefault().ToUpper()) || hawbReader.GetExistingBusinessObject() != null)
				{
					yield return hawbReader;
				}
			}
		}

		protected override void FillHAWBs(CusMAWB mawb)
		{
			var generator = mawb.ChildBillLineNumberGenerator;

			using (generator.GetLineNumberSuspender())
			{
				base.FillHAWBs(mawb);
			}

			generator.ReCalculateAll();
		}

		protected override DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return IsHVLV
				? new ETailAirManifestDataObjectReaderHelper(factory, Core.Constants.CountryCodes.NewZealand, shipmentPK)
				: new DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper(factory, Core.Constants.CountryCodes.NewZealand);
		}

		protected override string ResponsiblePartyAddressType
		{
			get { return IsHVLV ? nameof(DocAddressType.ShippingLineAddress) : AddressTypes.ShippingLine; }
		}

		protected override void FillCountrySpecificDetails(CusMAWB mawb, Dictionary<string, ValueSetter> valueSetters)
		{
			base.FillCountrySpecificDetails(mawb, valueSetters);
			var mawbRow = GetColumnIndexer(mawb);
			SetValue(mawbRow, CusMAWBSchema.CM_HasProhibitedPackaging, dataObject.HasProhibitedPackaging, valueSetters);
			SetValue(mawbRow, CusMAWBSchema.CM_IsFinalManifest, dataObject.IsFinalManifest, valueSetters);
			PopulateWorkflowCustomFields(mawb, dataObject);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusMAWB mawb)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(mawb));
			if (builder.IsEmpty && !singleHAWBCheck && mawb != null && mawb.IsInDatabase && mawb.ChildBills.Cast<CusHAWB>().Any(CusHAWBDataObjectReader.IsMessagingActive))
			{
				foreach (var setterInfo in GetValueSetters(mawb).OfType<IColumnValueSetterInfo>())
				{
					if (GetFieldsThatShouldNotChangeAfterCargoReporting(mawb).Contains(setterInfo.Column.Name))
					{
						var oldValue = ConvertValue(setterInfo.Row.GetValue(setterInfo.Column));
						var newValue = ConvertValue(setterInfo.Value);
						var propertyInfo = mawb.ZPropertyInfoHash.GetPropertySafe(setterInfo.Column.Name);
						if (newValue != null && oldValue.CompareTo(newValue) != 0)
						{
							builder.AppendLine(Res.GetString("8FE13A9F-996C-4570-9A66-1DC0E3267E6D", "There is an attempt to update {0} from '{1}' to '{2}' on this Master/Sub-Master while it has at least one House Bill with an active messaging.",
								propertyInfo != null ? (string)propertyInfo.HumanReadableName : setterInfo.Column.Name, oldValue, newValue));
						}
					}
				}
			}
			return builder.ToString();
		}

		protected override TransportLeg GetArrivalFlight(CusMAWB mawb)
		{
			return dataObject.GetArrivalTransportLeg();
		}

		protected override TransportLeg GetDepartureFlight(CusMAWB mawb)
		{
			return dataObject.GetDepartureTransportLeg();
		}

		IZType ConvertValue(object value)
		{
			var dateValue = value as ZDateTime?;
			return dateValue.HasValue && dateValue.Value.IsValid ? dateValue.Value.Date : (IZType)value;
		}

		static IEnumerable<string> GetFieldsThatShouldNotChangeAfterCargoReporting(CusMAWB mawb)
		{
			yield return CusMAWBSchema.Constants.CM_FlightNo;
			yield return CusMAWBSchema.Constants.CM_OH_ResponsibleParty;
			if (mawb.IsImport)
			{
				yield return CusMAWBSchema.Constants.CM_RL_NKDischargePort;
				yield return CusMAWBSchema.Constants.CM_ArrivalDate;
			}
			else if (mawb.IsExport)
			{
				yield return CusMAWBSchema.Constants.CM_RL_NKLoadPort;
				yield return CusMAWBSchema.Constants.CM_DepartureDate;
			}
		}

		public bool IsUpdatable(CusMAWB mawb)
		{
			return GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(mawb).IsEmpty;
		}
	}
}
