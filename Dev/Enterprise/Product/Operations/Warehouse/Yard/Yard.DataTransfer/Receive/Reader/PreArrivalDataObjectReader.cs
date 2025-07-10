using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class PreArrivalDataObjectReader : ContainerYardDataObjectReader<CYDReceiveAdvice>
	{
		public PreArrivalDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			MatchedClientOrgAddress = GetClientOrgAddress();
		}
		readonly IOrgAddress MatchedClientOrgAddress;

		IOrgAddress GetClientOrgAddress()
		{
			var orgAddress = Client?.MainAddress;
			return orgAddress ?? throw new DataObjectReadFailureException("Client organisation details are not found in UXML.");
		}

		public override DataContextType DataContextType => DataContextType.CYDReceiveAdvice;

		protected override IMatchingBusinessEntityFinder<CYDReceiveAdvice> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CYDReceiveAdvice GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var sourceDO = GetSourceDataObject();

			if (string.IsNullOrEmpty(sourceDO.BookingConfirmationReference))
			{
				return null;
			}

			var targetJobNumber = string.Empty;

			if (!sourceDO.DataContext.DataTargetCollection.IsNullOrEmpty() && sourceDO.DataContext.DataTargetCollection.First().Key.HasValue)
			{
				targetJobNumber = sourceDO.DataContext.DataTargetCollection.Single().Key;
			}

			var receiveAdviceQuery = @"
YRA_PK in
(
	SELECT
		PRA.YRA_PK
	FROM
		CYDReceiveAdvice AS PRA INNER JOIN JobDocAddress AS JDA
			ON PRA.YRA_PK = JDA.E2_ParentID
		WHERE
			JDA.E2_AddressType = @AddressType AND
			JDA.E2_ParentTableCode = @ParentTableCode AND
			PRA.YRA_AcceptanceNumber = @AcceptanceNumber AND
			JDA.E2_OA_Address = @AddressPK
)";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@AddressType", DocAddressTypes.Codes.BookingPartyDocumentaryAddress, JobDocAddressSchema.E2_AddressType },
				{ "@ParentTableCode", CYDReceiveAdviceSchema.Constants.Prefix, JobDocAddressSchema.E2_ParentTableCode },
				{ "@AcceptanceNumber", sourceDO.BookingConfirmationReference, CYDReceiveAdviceSchema.YRA_AcceptanceNumber },
				{ "@AddressPK", MatchedClientOrgAddress.PK, JobDocAddressSchema.PK },
			};

			var query = new ZDBOnlyQuery(typeof(CYDReceiveAdvice));
			query.AddFilterAndZSQLParameterCollection(receiveAdviceQuery, sqlParams);

			var matchingReceiveAdvices = factory.Load<CYDReceiveAdvice>(query).ToArray();

			var incomingFromDate = sourceDO.DateCollection.First(d => d.Type == DateType.Start).Value;
			var incomingToDate = sourceDO.DateCollection.First(d => d.Type == DateType.End).Value;

			CYDReceiveAdvice result = null;

			foreach (var matchedReceiveAdvice in matchingReceiveAdvices)
			{
				if (!targetJobNumber.IsNullOrEmpty() && targetJobNumber == matchedReceiveAdvice.YRA_JobNumber)
				{
					if (result == null)
					{
						result = matchedReceiveAdvice;
					}

					continue;
				}

				if (matchedReceiveAdvice.YRA_FromDate <= incomingFromDate && matchedReceiveAdvice.YRA_ToDate >= incomingToDate)
				{
					if (result == null)
					{
						result = matchedReceiveAdvice;
						continue;
					}

					var warnMsgBuilder = new StringBuilder();
					warnMsgBuilder.Append((NoResString)"Found multiple PRA that match with the incoming UXML");
					if (!targetJobNumber.IsNullOrEmpty())
					{
						warnMsgBuilder.Append($"with target job number [{targetJobNumber}] to ");
					}
					warnMsgBuilder.Append($"update. Overlapping PRA job numbers [{result.YRA_JobNumber}, {matchedReceiveAdvice.YRA_JobNumber}]. Out of these two PRAs, one that's has the later creation time will be updated.");
					logger.Log(LogType.Warning, warnMsgBuilder.ToString());

					if (result.YRA_SystemCreateTimeUtc < matchedReceiveAdvice.YRA_SystemCreateTimeUtc)
					{
						result = matchedReceiveAdvice;
					}

					continue;
				}

				if ((matchedReceiveAdvice.YRA_ToDate >= incomingFromDate &&
						matchedReceiveAdvice.YRA_ToDate <= incomingToDate) ||
						(matchedReceiveAdvice.YRA_FromDate <= incomingToDate &&
						matchedReceiveAdvice.YRA_FromDate >= incomingFromDate))
				{
					logger.Log(LogType.Error, $"Incoming UXML PRA is conflicting with existing saved PRA [{matchedReceiveAdvice.YRA_AcceptanceNumber}]. Detected when getting ExistingBusinessObject using Module Specific Business Rules.");
					throw new DataObjectReadFailureException("The from and to dates of incoming PRA is overlapping with existing saved PRA data.");
				}
			}

			return result;
		}

		#region GetSourceDataObject

		protected Shipment GetSourceDataObject()
		{
			return SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE ? dataObject : Shipment.GetSourceDataObject(dataObject);
		}

		#endregion

		protected override void PopulateBusinessObject(CYDReceiveAdvice receiveAdvice)
		{
			var receiveAdviceRow = GetColumnIndexer(receiveAdvice);
			var warehouse = YardMatchingHelper.GetYard(dataObject, factory, logger);
			if (warehouse != null)
			{
				ValidateReceiveAdvice(receiveAdvice);
				if (IsNewBO)
				{
					var jobNumber = PopulateJobNumber(receiveAdviceRow) ?? receiveAdvice.JobNumber;
					PopulateAcceptNumber(receiveAdviceRow, jobNumber);
					PopulateWarehouse(receiveAdviceRow, warehouse);
				}
				PopulateDates(receiveAdviceRow);
				PopulateContainers(receiveAdvice, warehouse);
				PopulateAdditionalAddresses(receiveAdvice);
			}
		}

		void PopulateContainers(CYDReceiveAdvice receiveAdvice, IColumnIndexer warehouse)
		{
			if (!IsNewBO)
			{
				var query = new ZQuery(CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, receiveAdvice.PK);

				var cydReceiveAdviceLineEntries = factory.Load<CYDReceiveAdviceLine>(query);
				foreach (var cydReceiveAdviceLine in cydReceiveAdviceLineEntries)
				{
					if (cydReceiveAdviceLine == null)
					{
						continue;
					}

					var unitLineItemQuery = new ZQuery(CYDUnitLineItemSchema.PK, cydReceiveAdviceLine.YRL_YLI_UnitLineItem);
					var unitLineItemEntry = factory.Load<CYDUnitLineItem>(unitLineItemQuery).SingleOrDefault();

					if (unitLineItemEntry != null)
					{
						unitLineItemEntry.Delete();
					}

					var yardUnitStateQuery = new ZQuery(CYDYardUnitStateSchema.YUS_YRL_ReceiveLine, cydReceiveAdviceLine.PK);
					var yardUnitStateEntry = factory.Load<CYDYardUnitState>(yardUnitStateQuery).SingleOrDefault();

					if (yardUnitStateEntry != null)
					{
						yardUnitStateEntry.Delete();
					}

					var genCustomAddOnValueQuery = new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, cydReceiveAdviceLine.PK);
					genCustomAddOnValueQuery.AddToFilter(GenCustomAddOnValueSchema.XV_ParentTableCode, CYDReceiveAdviceLineSchema.Constants.Prefix);
					var genCustomAddOnValueEntry = factory.Load<GenCustomAddOnValue>(genCustomAddOnValueQuery).SingleOrDefault();

					if (genCustomAddOnValueEntry != null)
					{
						genCustomAddOnValueEntry.Delete();
					}

					cydReceiveAdviceLine.Delete();
				}
			}

			var containerCollection = dataObject.SubShipmentCollection
				.Where(s => s.RelatedShipmentCollection != null)
				.SelectMany(s => s.RelatedShipmentCollection)
				.Where(r => r.ContainerCollection != null)
				.SelectMany(r => r.ContainerCollection);
			var containers = containerCollection?.Where(c => c.ContainerNumber.HasValue && c.ContainerType != null).GroupBy(c => c.ContainerNumber).Select(g => g.First()).ToList();

			var validatedContainers = ValidateContainerNumber(containers);

			validatedContainers.ForEach(c =>
			{
				var containerTypePK = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, c.ContainerType.Code)).PK;
				ValidateContainer(receiveAdvice, c, containerTypePK);

				var receiveLineRow = factory.New<CYDReceiveAdviceLine>();
				SetValue(receiveLineRow, CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, receiveAdvice.PK);

				var unitLineItem = factory.New<CYDUnitLineItem>();
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_Quantity, c.ContainerCount);
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_Type, "CNT");
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_RC_ContainerType, containerTypePK);
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_SealNumber, c.AdditionalSealNumberCollection?.FirstOrDefault().Number);
				SetValue(unitLineItem, CYDUnitLineItemSchema.YLI_IsEmpty, c.IsEmptyContainer);
				SetValue(receiveLineRow, CYDReceiveAdviceLineSchema.YRL_YLI_UnitLineItem, unitLineItem.PK);

				var yardUnitStateRow = factory.New<CYDYardUnitState>();
				SetValue(yardUnitStateRow, CYDYardUnitStateSchema.YUS_UnitID, c.ContainerNumber);
				SetValue(yardUnitStateRow, CYDYardUnitStateSchema.YUS_YRL_ReceiveLine, receiveLineRow.PK);
				SetValue(yardUnitStateRow, CYDYardUnitStateSchema.YUS_WW_CurrentYard, warehouse.GetValue(WhsWarehouseSchema.PK));
			});
		}

		void ValidateContainer(CYDReceiveAdvice receiveAdvice, Container container, ZGuid containerTypePK)
		{
			var queryStr = @"YUS_PK in (
SELECT YUS_PK FROM CYDYardUnitState YUS 
JOIN CYDReceiveAdviceLine YRL ON YRL.YRL_PK = YUS.YUS_YRL_ReceiveLine
JOIN CYDReceiveAdvice YRA ON YRA.YRA_PK = YRL.YRL_YRA_ReceiveAdvice 
JOIN CYDUnitLineItem YLI ON YLI.YLI_PK = YRL.YRL_YLI_UnitLineItem
JOIN JobDocAddress JDA ON JDA.E2_ParentID = YRA.YRA_PK AND JDA.E2_AddressType = 'BKD' 
JOIN OrgAddress OAD ON OAD.OA_PK = JDA.E2_OA_Address
LEFT JOIN CYDTransportationUnit YTU ON YTU_PK = YUS.YUS_YTU_ReceiveTransportationUnit
WHERE OAD.OA_PK = @AddressPK
AND YUS_UnitID >= @UnitID
AND YLI.YLI_RC_ContainerType = @ContainerTypePK
AND YTU.YTU_GateInTime IS NULL
AND YRA_FromDate <= @ToDate
AND YRA_ToDate >= @FromDate)";

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@UnitID", container.ContainerNumber, CYDYardUnitStateSchema.YUS_UnitID },
				{ "@ContainerTypePK", containerTypePK, CYDUnitLineItemSchema.YLI_RC_ContainerType },
				{ "@FromDate", receiveAdvice.YRA_FromDate, CYDReceiveAdviceSchema.YRA_FromDate },
				{ "@ToDate", receiveAdvice.YRA_ToDate, CYDReceiveAdviceSchema.YRA_ToDate },
				{ "@AddressPK", MatchedClientOrgAddress.PK, JobDocAddressSchema.PK },
			};

			var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			query.AddFilterAndZSQLParameterCollection(queryStr, sqlParams);

			var matchingYardUnits = factory.Load<CYDYardUnitState>(query).ToList();

			if (matchingYardUnits.Count > 0)
			{
				throw new DataObjectReadFailureException("Same unit number already exists in Pre-Arrival Instruction ID.");
			}
		}

		void PopulateAdditionalAddresses(CYDReceiveAdvice receiveAdvice)
		{
			if (!IsNewBO)
			{
				PopulateLessee(receiveAdvice);
			}
			else
			{
				PopulateClient(receiveAdvice);
				PopulateLessee(receiveAdvice);
			}
		}

		void PopulateClient(CYDReceiveAdvice receiveAdvice)
		{
			var address = CreateJobDocAddress(receiveAdvice, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			var reader = new OrganisationDataObjectReader(new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance), logger, factory);
			reader.PopulateJobDocAddress(MatchedClientOrgAddress, address);
		}

		void PopulateLessee(CYDReceiveAdvice receiveAdvice)
		{
			var addressFromShipment = GetSourceDataObject().OrganizationAddressCollection
							.FirstOrDefault(a => a.AddressType.GetValueOrDefault().EqualsIgnoringCase(nameof(DocAddressType.ControllingCustomer)));
			if (addressFromShipment != null)
			{
				var reader = new OrganisationDataObjectReader(addressFromShipment, logger, factory);
				reader.GetMatchedOrNew(receiveAdvice, DocAddressType.ControllingCustomer);
			}
		}

		JobDocAddress CreateJobDocAddress(CYDReceiveAdvice receiveAdvice, string docAddressType)
		{
			var address = factory.New<JobDocAddress>();
			SetValue(address, JobDocAddressSchema.E2_AddressType, docAddressType);
			SetValue(address, JobDocAddressSchema.E2_ParentID, receiveAdvice.PK);
			SetValue(address, JobDocAddressSchema.E2_ParentTableCode, CYDReceiveAdviceSchema.Constants.Prefix);
			return address;
		}

		void ValidateReceiveAdvice(CYDReceiveAdvice sourceBO)
		{
			if (!IsNewBO)
			{
				var sourceDO = GetSourceDataObject();
				if (!sourceDO.DataContext.DataTargetCollection.IsNullOrEmpty() && sourceDO.DataContext.DataTargetCollection.First().Key.HasValue)
				{
					var matchedCYDReceiveAdvice = GetExistingBusinessObjectUsingModuleSpecificBusinessRules();
					if (matchedCYDReceiveAdvice != null && matchedCYDReceiveAdvice.PK != sourceBO.PK)
					{
						logger.Log(LogType.Error, $"Incoming UXML PRA is conflicting with existing saved PRA [{matchedCYDReceiveAdvice.YRA_AcceptanceNumber}]. Detected when validating incoming receive advice.");
						throw new DataObjectReadFailureException("The from and to dates of incoming PRA is overlapping with existing saved PRA data.");
					}
				}

				var deliveryQuery = new ZDBOnlyQuery(typeof(CYDDelivery));
				var receiveAdviceLineQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDReceiveAdviceLineSchema.PK);
				receiveAdviceLineQuery.AddToFilter(CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, SQLComparisonOperator.Equal, sourceBO.PK);
				deliveryQuery.AddSubQuery(CYDDeliverySchema.YDL_YRL_ReceiveAdviceLine, CYDReceiveAdviceLineSchema.PK, receiveAdviceLineQuery, JoinCondition.And);

				var deliveries = sourceBO.Factory.Load<CYDDelivery>(deliveryQuery);
				if (deliveries.Any())
				{
					throw new DataObjectReadFailureException("The PRA is associated with one or more deliveries.");
				}
			}
		}

		void PopulateWarehouse(IColumnIndexer receiveAdviceRow, IColumnIndexer warehouse)
		{
			if (IsNewBO)
			{
				SetValue(receiveAdviceRow, CYDReceiveAdviceSchema.YRA_WW_Yard, warehouse.GetValue(WhsWarehouseSchema.PK));
				SetValue(receiveAdviceRow, CYDReceiveAdviceSchema.YRA_Mode, "EMT");
			}
		}

		string PopulateJobNumber(IColumnIndexer receiveAdviceRow)
		{
			if (IsNewBO)
			{
				var jobNumberStrategy = new PreArrivalInstructionJobNumberStrategy(factory.BOFactory);
				var jobNumber = jobNumberStrategy.GetJobNumber();
				SetValue(receiveAdviceRow, CYDReceiveAdviceSchema.YRA_JobNumber, jobNumber);
				return jobNumber;
			}
			return null;
		}

		void PopulateAcceptNumber(IColumnIndexer receiveAdviceRow, string jobNumber)
		{
			var acceptNumber = dataObject.BookingConfirmationReference;

			if (string.IsNullOrEmpty(acceptNumber))
			{
				acceptNumber = jobNumber;
			}
			SetValue(receiveAdviceRow, CYDReceiveAdviceSchema.YRA_AcceptanceNumber, acceptNumber);
		}

		void PopulateDates(IColumnIndexer headerRow)
		{
			if (dataObject.DateCollection != null && dataObject.DateCollection.Count > 0)
			{
				FillDates(headerRow, dataObject.DateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(CYDReceiveAdviceSchema.YRA_FromDate, new[] { DateType.Start }),
					new DateTypeSchemaColumnMap(CYDReceiveAdviceSchema.YRA_ToDate, new[] { DateType.End }));
			}
			else
			{
				var fromDate = ZDateTime.Now;
				SetValue(headerRow, CYDReceiveAdviceSchema.YRA_FromDate, fromDate);
				var toDate = fromDate.AddMonths(1);
				SetValue(headerRow, CYDReceiveAdviceSchema.YRA_ToDate, toDate);
			}
		}

		bool IsAlphaNumeric(string val)
		{
			return Regex.IsMatch(val, @"^[A-Z0-9]*$", RegexOptions.IgnoreCase);
		}

		List<Container> ValidateContainerNumber(List<Container> containers)
		{
			List<Container> validContainers = new List<Container>();

			foreach (var container in containers)
			{
				if (IsAlphaNumeric(container.ContainerNumber))
				{
					validContainers.Add(container);
				}
				else
				{
					logger.Log(LogType.Warning, $"Invalid container number found in the incoming UXML: {container.ContainerNumber}. Container number should not contain special characters.");
				}
			}

			if (!validContainers.Any())
			{
				throw new DataObjectReadFailureException("No containers with valid container numbers found in the incoming UXML.");
			}

			return validContainers;
		}
	}
}
