using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.DataTransfer.Universal.SeaManifest
{
	public abstract class CusSCAOceanBillDataObjectReader<TCusSCAOceanBill, TCusSCAHouse, TCusSCAContainer, TCusSCAPivot> : ShipmentDataObjectReader<TCusSCAOceanBill>
	where TCusSCAOceanBill : BaseCusSCAOceanBill
	where TCusSCAHouse : BaseCusSCAHouse
	where TCusSCAContainer : BaseCusSCAContainer
	where TCusSCAPivot : BaseCusSCAPivot
	{
		protected CusSCAOceanBillDataObjectReader(Shipment dataObject, Shipment hVLVShipperConsolidation, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			this.HVLVShipperConsolidation = hVLVShipperConsolidation;
		}

		#region Overrides

		public sealed override DataContextType DataContextType
		{
			get { return DataContextType.SeaOceanBill; }
		}

		protected sealed override IMatchingBusinessEntityFinder<TCusSCAOceanBill> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override TCusSCAOceanBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			TCusSCAOceanBill result = null;
			if (dataObject.WayBillNumber.HasValue &&
				dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master)
			{
				var coLoadBill = ZString.Empty;

				if (IsInHVLVProcess)
				{
					coLoadBill = HVLVShipperConsolidation.WayBillNumber.GetValueOrDefault();
				}
				else
				{
					coLoadBill = GetMasterHouse().GetValueOrDefault();
				}

				var query = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, dataObject.WayBillNumber.Value);
				query.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, GetApplicationCode());
				query.AddToFilter(CusSCAOceanBillSchema.CB_LloydsIMO, dataObject.LloydsIMO.GetValueOrDefault());
				query.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, dataObject.VoyageFlightNo.GetValueOrDefault());
				query.AddToFilter(CusSCAOceanBillSchema.CB_MasterHouseBill, coLoadBill);

				var bills = factory.Load<TCusSCAOceanBill>(query);

				if (IsInHVLVProcess && coLoadBill.IsEmpty && !string.IsNullOrEmpty(ShipmentJobNumber))
				{
					result = GetMostInterestingExistingBusinessObjectForHVLV(bills);
				}
				else if (bills.Length > 1)
				{
					var addedTimeQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode);
					addedTimeQuery.AddToFilter(StmALogSchema.SL_Table, CusSCAOceanBillSchema.Constants.TableName);
					addedTimeQuery.AddToFilter(StmALogSchema.SL_Parent, bills.Select(bill => bill.PK));
					addedTimeQuery.OrderBy = StmALogSchema.Constants.SL_EventTime + " desc";
					var mostRecentLog = factory.LoadTop1<StmALog>(addedTimeQuery);
					result = bills.First(bill => bill.PK == mostRecentLog.SL_Parent);
				}
				else if (bills.Length == 1)
				{
					result = bills[0];
				}
			}

			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TCusSCAOceanBill targetBO)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO));
			if (dataObject.WayBillType.GetCodeAsUpperCase() != WayBillTypeList.Codes.Master)
			{
				builder.AppendLine(Res.GetString("B25B9F6C-874A-46E6-8196-C5634360BA91", "{1} must be '{0}'.", WayBillTypeList.Codes.Master, "WayBillType.Code"));
			}
			if (dataObject.WayBillNumber.GetValueOrDefault().IsEmpty)
			{
				builder.AppendLine(Res.GetString("91EFAB76-6BD3-4442-A749-765CD5CA81C4", "{0} must not be empty.", "WayBillNumber"));
			}
			return builder.ToString();
		}

		protected override void PopulateBusinessObject(TCusSCAOceanBill targetBO)
		{
			using (targetBO.SuspendMarkingAsNeedingValidation())
			{
				var oceanBill = GetColumnIndexer(targetBO);
				var valueSetters = GetValueSetters(oceanBill);
				if (CheckUpdateCusSCAOceanBillDataIsAllowed(targetBO))
				{
					foreach (var delaySetter in valueSetters)
					{
						delaySetter.SetValue();
					}
					PopulateNotes(oceanBill);
				}
				PopulateContainers(oceanBill, dataObject);
				PopulateSubshipments(oceanBill, HVLVShipperConsolidation ?? dataObject);
			}
		}

		#endregion // Overrides

		#region HVLV

		protected readonly Shipment HVLVShipperConsolidation;

		protected bool IsInHVLVProcess => HVLVShipperConsolidation != null;

		protected string ShipmentJobNumber => shipmentJobNumber ?? (shipmentJobNumber = HVLVShipperConsolidation.DataContext?.DataSourceCollection?.FirstOrDefault()?.Key);
		string shipmentJobNumber;

		TCusSCAOceanBill GetMostInterestingExistingBusinessObjectForHVLV(TCusSCAOceanBill[] bills)
		{
			return bills.FirstOrDefault(b => b.Logs.GetAllLogs().Cast<StmALog>().Any(l =>
				l.Parameters.TryGetValue(EventReferenceParameters.Codes.Type, out var type) && type == Core.Constants.ShipmentTypes.HighVolumeLowValue
				&& (l.Parameters.TryGetValue(EventReferenceParameters.Codes.JobNumber, out var jobNumber) && jobNumber == ShipmentJobNumber
				|| l.Parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var refNumber) && refNumber == ShipmentJobNumber)));
		}

		#endregion

		protected virtual bool CheckUpdateCusSCAOceanBillDataIsAllowed(TCusSCAOceanBill oceanBill)
		{
			return true;
		}

		#region Implementation

		protected abstract ZString GetApplicationCode();

		protected abstract CusSCAHouseDataObjectReader<TCusSCAHouse, TCusSCAPivot> GetNewCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, Shipment subShipmentDataObject);
		protected abstract CusSCAContainerDataObjectReader<TCusSCAContainer> GetNewCusSCAContainerDataObjectReader(IColumnIndexer oceanBill, Container containerDataObject);

		void PopulateOrganizationAddresses(IColumnIndexer oceanBill, Dictionary<string, ValueSetter> valueSetter)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var shippingLineDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ShippingLineAddress));
				if (shippingLineDataObject != null)
				{
					var shippingLineAddress = new OrganisationDataObjectReader(shippingLineDataObject, logger, factory).GetMatched();
					if (shippingLineAddress != null && shippingLineAddress.OA_OH != OrgHeader.UnmatchedOrganisationPK)
					{
						SetValue(oceanBill, CusSCAOceanBillSchema.CB_OH_ShippingLine, shippingLineAddress.OA_OH, valueSetter);
						SetValue(oceanBill, CusSCAOceanBillSchema.CB_PrincipalID, shippingLineAddress.Header.PrimaryRegistrationNumber.Number, valueSetter);
					}
				}

				var goodsLocationDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.GoodsLocation));
				if (goodsLocationDataObject != null)
				{
					var goodsLocationAddress = new OrganisationDataObjectReader(goodsLocationDataObject, logger, factory).GetMatched();
					if (goodsLocationAddress != null)
					{
						SetValue(oceanBill, CusSCAOceanBillSchema.CB_OA_GoodsLocation, goodsLocationAddress.PK, valueSetter);
					}
				}
			}
			if (oceanBill.GetValue(CusSCAOceanBillSchema.CB_PrincipalID).IsEmpty && dataObject.AdditionalReferenceCollection != null)
			{
				var principalIDReference = dataObject.AdditionalReferenceCollection.FirstOrDefault(reference =>
					reference.Type.GetCodeAsUpperCase() == Constants.AdditionalReference.EntryType.Codes.PrincipalID);
				SetValue(oceanBill, CusSCAOceanBillSchema.CB_PrincipalID,
					principalIDReference != null ? principalIDReference.ReferenceNumber : null, valueSetter);
			}
		}

		void PopulateDates(IColumnIndexer oceanBill, Dictionary<string, ValueSetter> valueSetter)
		{
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_DateOfDeparture, dataObject.GetLoadingDateForSea(GetDepartureVoyage()), valueSetter);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_DateOfFirstArrival, GetDate(DateType.FirstArrivalInCountry), valueSetter);

			var dischargeDate = dataObject.GetDischargeDateForSea(GetArrivalVoyage());
			if (dischargeDate != null && dischargeDate.Value != ZDateTime.Empty)
			{
				SetValue(oceanBill, CusSCAOceanBillSchema.CB_DateOfArrival, dischargeDate, valueSetter);
			}
		}

		protected virtual TransportLeg GetDepartureVoyage() { return null; }
		protected virtual TransportLeg GetArrivalVoyage() { return null; }

		void PopulateMasterHouse(IColumnIndexer oceanBill, Dictionary<string, ValueSetter> valueSetter)
		{
			var masterHouseBill = HVLVShipperConsolidation != null
				? HVLVShipperConsolidation.WayBillNumber
				: GetMasterHouse();
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_MasterHouseBill, masterHouseBill, valueSetter);
		}

		void PopulateNotes(IColumnIndexer oceanBill)
		{
			var notesParent = oceanBill as IUniversalXMLNoteParent;
			if (notesParent != null &&
				dataObject.NoteCollection != null &&
				oceanBill.GetValue(CusSCAOceanBillSchema.CB_ParentId).IsEmpty)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, notesParent).ReadIntoCollection();
			}
		}

		void PopulateSubshipments(IColumnIndexer oceanBill, Shipment shipmentDataObject)
		{
			if (shipmentDataObject.SubShipmentCollection != null)
			{
				var houseBills = new List<TCusSCAHouse>();
				var existingHouseBills = CusSCADataObjectHelper.LoadHouseBills<TCusSCAHouse>(oceanBill, factory.BOFactory).ToList();
				foreach (var subShipmentDataObject in shipmentDataObject.SubShipmentCollection)
				{
					var houseBill = GetNewCusSCAHouseDataObjectReader(oceanBill, subShipmentDataObject)?.ReadIntoBusinessObject();
					if (houseBill != null)
					{
						existingHouseBills.Remove(houseBill);
						houseBills.Add(houseBill);
					}
				}

				var houseBillsToDelete = new List<TCusSCAHouse>();
				foreach (var house in existingHouseBills)
				{
					if (house.CanDelete && ShouldDeleteUnprocessedHouseBill(house))
					{
						logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("09019862-ABB5-4DDD-B28E-ADC7B814A6F7", "Deleted {0} from {1}.", house.HumanReadableName, "Universal Shipment"));
						houseBillsToDelete.Add(house);
					}
					else
					{
						var reason = (ZString)house.ReasonForNotAbleToDelete;
						var reasonToShow = reason.Left(1).ToLower() + reason.SubstringSafe(1);

						logger.Log(Enterprise.Integration.LogType.Warning, Res.GetString("A7993B94-B9CF-4EB3-B168-836761D6904B", "{0} does not appear in Universal Shipment, but {1}", house.HumanReadableName, reasonToShow));
						houseBills.Add(house);
					}
				}
				houseBillsToDelete.DeleteAll();

				ProcessPopulatedHouseBills(oceanBill, houseBills);
			}
		}

		protected virtual bool ShouldDeleteUnprocessedHouseBill(BaseCusSCAHouse house)
		{
			return true;
		}

		protected virtual void ProcessPopulatedHouseBills(IColumnIndexer oceanBill, List<TCusSCAHouse> houseBills)
		{
		}

		void PopulateContainers(IColumnIndexer oceanBill, Shipment shipmentDataObject)
		{
			if (shipmentDataObject.ContainerCollection != null)
			{
				var containers = CusSCADataObjectHelper.LoadContainers<TCusSCAContainer>(oceanBill, factory.BOFactory);
				var containersToDelete = containers.ToList();
				if (cusSCAContainerReaderCollection != null)
				{
					foreach (var containerReader in cusSCAContainerReaderCollection)
					{
						var container = containerReader.ReadIntoBusinessObject();
						containersToDelete.Remove(container);
					}
				}
				else
				{
					foreach (var containerData in dataObject.ContainerCollection)
					{
						var container = GetNewCusSCAContainerDataObjectReader(oceanBill, containerData).ReadIntoBusinessObject();
						containersToDelete.Remove(container);
					}
				}
				containersToDelete.DeleteAll();
			}
		}

		protected virtual void PopulateCountrySpecificDetails(IColumnIndexer oceanBill, Dictionary<string, ValueSetter> valueSetters)
		{
		}

		protected IEnumerable<ValueSetter> GetValueSetters(IColumnIndexer oceanBill)
		{
			var result = new Dictionary<string, ValueSetter>();
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_GB, GetBranch(), result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_ApplicationCode, GetApplicationCode(), result);

			SetValue(oceanBill, CusSCAOceanBillSchema.CB_OceanBill, dataObject.WayBillNumber, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_VesselName, dataObject.VesselName, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_Voyage, dataObject.VoyageFlightNo, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_LloydsIMO, dataObject.LloydsIMO, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_RL_NKPortOfLoading, dataObject.PortOfLoading, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_RL_NKPortOfDischarge, dataObject.PortOfDischarge, result);
			SetValue(oceanBill, CusSCAOceanBillSchema.CB_RL_NKPortOfFirstArrival, dataObject.PortOfFirstArrival, result);

			SetValue(oceanBill, CusSCAOceanBillSchema.CB_ResponsiblePartyID, dataObject.GetResponsiblePartyID(logger, factory), result);

			PopulateOrganizationAddresses(oceanBill, result);
			PopulateDates(oceanBill, result);
			PopulateMasterHouse(oceanBill, result);
			PopulateCountrySpecificDetails(oceanBill, result);

			return result.Values;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IEnumerable<CusSCAContainerDataObjectReader<TCusSCAContainer>> cusSCAContainerReaderCollection;

		ZGuid? GetBranch()
		{
			var branchPK = dataObject.GetBranchPK(factory.BOFactory);
			return branchPK.IsValid ? new ZGuid?(branchPK) : null;
		}

		ZDateTime? GetDate(DateType dateType)
		{
			var dates = dataObject.DateCollection;
			var date = dates != null ? dates.FirstOrDefault(dateType, false) : null;
			return date != null ? date.Value : null;
		}

		ZString? GetMasterHouse()
		{
			if (dataObject.AdditionalBillCollection != null)
			{
				var masterHouseBill = dataObject.AdditionalBillCollection.FirstOrDefault(bill =>
					bill.BillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse &&
					bill.ParentBillNumber == dataObject.WayBillNumber);
				if (masterHouseBill != null)
				{
					return masterHouseBill.BillNumber.GetValueOrDefault();
				}
			}
			if (dataObject.ShipmentType.GetCodeAsUpperCase() == Core.Constants.AgentType.CoLoad)
			{
				return dataObject.BookingConfirmationReference.GetValueOrDefault();
			}
			return null;
		}

		#endregion // Implementation
	}
}
