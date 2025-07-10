using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketDataObjectReader<TDocket, TDocketLine> : ShipmentDataObjectReader<TDocket>, IDocketType
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
	{
		protected WhsDocketDataObjectReader(UniversalShipment docketDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(docketDataObject, logger, factory)
		{
		}

		#region Matching Job

		protected override TDocket GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			TDocket result = null;

			var matchingReferenceNumber = ImportStrategy.MatchingReference.GetValueOrDefault();
			if (!matchingReferenceNumber.IsEmpty)
			{
				if (!ImportStrategy.RequireClientForMatch || !ClientOrganisationPK.IsMissing)
				{
					var query = new ZQuery();
					ImportStrategy.AddMatchingReferenceFilter(matchingReferenceNumber, query);
					query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketTypeCode);

					if (ImportStrategy.RequireClientForMatch)
					{
						query.AddToFilter(WhsDocketSchema.WD_OH_Client, ClientOrganisationPK);
					}

					AddAdditionalFilter(dataObject, query);

					result = factory.LoadTop1<TDocket>(query);
				}
			}

			return result;
		}

		protected virtual void AddAdditionalFilter(UniversalShipment dataObject, ZQuery query)
		{
			ImportStrategy.AddAdditionalFilter(dataObject, query);
		}

		protected abstract string DocketTypeCode { get; }

		#region ClientOrganisationPK

		protected ZGuid ClientOrganisationPK
		{
			get { return (clientOrganizationPK ?? (clientOrganizationPK = GetClientOrganisationPK())).Value; }
		}

		ZGuid GetClientOrganisationPK()
		{
			var clientOrganization = ZGuid.Missing;

			if (dataObject.OrganizationAddressCollection != null)
			{
				var clientAddressDO = ImportStrategy.ClientOrganizationAddress;
				if (clientAddressDO != null)
				{
					var warehouseClientAddress = new OrganisationDataObjectReader(clientAddressDO, logger, factory).GetMatched(true);
					if (warehouseClientAddress != null)
					{
						clientOrganization = warehouseClientAddress.Header.PK;
					}
				}
			}

			return clientOrganization;
		}

		ZGuid? clientOrganizationPK;

		#endregion

		#endregion

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(TDocket matchedDocket)
		{
			return matchedDocket != null
				? ImportStrategy.GetReasonForNotAbleToUpdateMatchedDocket(matchedDocket)
				: base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(matchedDocket);
		}

		// Create / Update

		#region Create / Update Job

		protected sealed override void PopulateBusinessObject(TDocket docket)
		{
			using (new DisposableAction(
				() => PopulateBizOImportSuspender(docket, isImportingData: true),
				() => PopulateBizOImportSuspender(docket, isImportingData: false)))
			using (ImportStrategy.PopulateBizOSuspender(docket))
			{
				try
				{
					ImportStrategy.BeforePopulate(docket);

					if (ImportStrategy.ShouldPopulateBizO(dataObject, docket))
					{
						PopulateBizO(docket);
					}

					ImportStrategy.AfterPopulate(docket);
				}
				catch (DataObjectReadFailureException)
				{
					DataObjectReadFailureExceptionCaught(docket);
					throw;
				}
			}
		}

		protected virtual void PopulateBizO(TDocket docket)
		{
			ValidateCriticalStringFieldsUseWesternEuropeanCharactersOnly();

			PopulateWarehouseAndClientAddressIfValidDocket(docket);
			PopulateAddresses(docket);
			PopulateNotes(docket);
			PopulateAdditionalReferences(docket);
			PopulateCustomFields(docket);
			SetValueIfNotReadOnly(docket, WhsDocketSchema.WD_DocketSubType, docket.Lookups.SubTypes, ImportStrategy.DocketSubType);
			PopulateBusinessObjectCore(docket);
			PopulateLines(docket);
			CalculateTotalsIfNeededCore(docket);
			LogRowErrors(docket);
		}

		protected virtual void PopulateBizOImportSuspender(TDocket docket, bool isImportingData)
		{
			((ISupportDataImporting)docket).IsImportingData = isImportingData;
		}

		protected virtual void CalculateTotalsIfNeededCore(TDocket docket)
		{
		}

		void ValidateCriticalStringFieldsUseWesternEuropeanCharactersOnly()
		{
			if (dataObject.Order != null)
			{
				if (!dataObject.Order.OrderNumber.GetValueOrDefault().IsWesternEuropeanOrEmpty)
				{
					ThrowErrorForInvalidCharactersInField(nameof(dataObject.Order.OrderNumber));
				}
			}

			ValidateDataObjectFieldsUseWesternEuropeanOnlyCore();
		}

		protected virtual void ValidateDataObjectFieldsUseWesternEuropeanOnlyCore()
		{
		}

		protected void ThrowErrorForInvalidCharactersInField(string fieldName)
		{
			var errorMessage = Res.GetString("79351d6a-91de-41d0-91ed-e97612300af8", "Cannot perform import due to invalid characters in field: {0}.", fieldName);
			throw new DataObjectReadFailureException(errorMessage);
		}

		protected virtual void PopulateBusinessObjectCore(TDocket docket)
		{
		}

		protected virtual void DataObjectReadFailureExceptionCaught(TDocket docket)
		{
		}

		void LogRowErrors(TDocket docket)
		{
			if (docket.HasRowErrors)
			{
				foreach (var rowError in docket.RowErrors)
				{
					logger.Log(LogType.Error, rowError.Message);
				}
			}
		}

		#endregion

		#region PopulateWarehouseAndClientAddressIfValidDocket

		void PopulateWarehouseAndClientAddressIfValidDocket(TDocket docket)
		{
			var clientAddressImportResult = GetValidClientAddress(docket);

			if (clientAddressImportResult.HasValidClientAddress)
			{
				SetValueIfNotReadOnly(docket, WhsDocketSchema.WD_OH_Client, typeof(OrgHeader), clientAddressImportResult.ClientPK);
			}

			var hasValidWarehouse = PopulateWarehouse(docket);
			var message = new ZStringBuilder();

			if (!clientAddressImportResult.HasValidClientAddress)
			{
				if (clientAddressImportResult.ClientAddressDataObject == null)
				{
					message.Append(Res.GetString("a2da7f77-aa33-449e-8bad-ec7438af064a", "No {0} Address was provided.", ClientDescriptionForErrorMessage));
				}
				else
				{
					clientAddressImportResult.ClientAddressDataObject.AddAddressErrorMessage(ClientDescriptionForErrorMessage, message);
				}
			}
			else if (clientAddressImportResult.ClientAddressDataObject != null)
			{
				var client = docket.Client;
				if (!client.OH_IsWarehouseClient)
				{
					message.Append(Res.GetString("347ed8b2-5fb7-4491-a836-143a32dd53f1",
						"The matched Organization {0} must be marked as a Warehouse Client.", client.OH_FullName));
				}
			}

			if (!hasValidWarehouse)
			{
				message.Append(
					dataObject.Order != null && dataObject.Order.Warehouse != null
					? Res.GetString("40126449-c829-4658-abfa-85e254b91a91", "Unable to match Warehouse: {0}.", dataObject.Order.Warehouse.ToStringContents())
					: GetErrorMessageWhenNoOrderOrWarehouseFound());
			}

			this.ThrowImportFailureExceptionIfNotEmpty(message);
		}

		protected virtual ZString ClientDescriptionForErrorMessage => Res.GetString("10c9ac5e-5f4b-42cf-93d4-323763097d25", "Client");

		string GetErrorMessageWhenNoOrderOrWarehouseFound()
		{
			string result;

			var warehouseAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.CustomsWarehouseAddress));
			if (warehouseAddress != null)
			{
				result = Res.GetString("DDBCAEA8-21BD-4FD3-96AD-09F406EA573B", "Unable to match Warehouse for Organization: {0} Address: {1}.", warehouseAddress.CompanyName, warehouseAddress.Address1);
			}
			else
			{
				result = Res.GetString("625d9bac-f1ed-4acb-84e8-6ed26dcaec22", "No Warehouse was provided.");
			}

			return result;
		}

		protected abstract string DocketType { get; }

		#region GetValidClientAddress

		ClientAddressImportResult GetValidClientAddress(TDocket docketBO)
		{
			var clientAddressDO = ImportStrategy.ClientOrganizationAddress;
			if (clientAddressDO != null)
			{
				var hasValidClientAddress = false;
				var clientPK = ZGuid.Empty;

				var clientAddress = new OrganisationDataObjectReader(clientAddressDO, logger, factory).GetMatched(docketBO, OrganisationTypes.WarehouseClient);
				if (clientAddress != null)
				{
					hasValidClientAddress = true;
					clientPK = clientAddress.OA_OH;
				}

				return new ClientAddressImportResult(hasValidClientAddress, clientAddressDO, clientPK);
			}

			return new ClientAddressImportResult(!IsNewBO, null, ZGuid.Empty);
		}

		class ClientAddressImportResult
		{
			public ClientAddressImportResult(bool hasValidClientAddress, OrganizationAddress clientAddressDataObject, ZGuid clientPK)
			{
				HasValidClientAddress = hasValidClientAddress;
				ClientAddressDataObject = clientAddressDataObject;
				ClientPK = clientPK;
			}

			public readonly bool HasValidClientAddress;
			public readonly OrganizationAddress ClientAddressDataObject;
			public readonly ZGuid ClientPK;
		}

		#endregion

		#region PopulateWarehouse

		protected virtual bool CheckCanChangeWarehouse(TDocket docket) => true;

		bool PopulateWarehouse(TDocket docket)
		{
			var warehouseImportResult = GetValidWarehouse();
			var hasValidWarehouse = warehouseImportResult.HasValidWarehouse;
			var warehouse = warehouseImportResult.Warehouse;

			if (!hasValidWarehouse)
			{
				warehouse = ImportStrategy.GetRelatedWarehouse(docket);
			}

			if (warehouse != null)
			{
				ImportStrategy.OnWarehouseMatched(warehouse);

				if (CheckCanChangeWarehouse(docket))
				{
					SetValueIfNotReadOnly(docket, WhsDocketSchema.WD_WW_Whs, typeof(WhsWarehouse), warehouse.PK);
					hasValidWarehouse = true;
				}
				else
				{
					logger.Log(LogType.Warning, Res.GetString("1034FA76-E014-4E18-B235-A3BFE049546B", "Cannot update warehouse from {0} to {1} as it is not allowed.", docket.Warehouse.WW_WarehouseCode, warehouse.WW_WarehouseCode));
				}
			}

			return hasValidWarehouse;
		}

		protected virtual WarehouseImportResult GetValidWarehouse()
		{
			var result = new WarehouseImportResult(!IsNewBO, null);

			var orderDataObject = dataObject.Order;
			if (orderDataObject != null)
			{
				var warehouseCode = orderDataObject.Warehouse.GetCodeAsUpperCase();
				if (!warehouseCode.IsEmpty)
				{
					var codeQuery = new ZQuery();
					codeQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseCode, warehouseCode);
					codeQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
					var warehouse = factory.LoadTop1<WhsWarehouse>(codeQuery);
					result = warehouse != null ? new WarehouseImportResult(true, warehouse) : new WarehouseImportResult(false, null);
				}
			}

			return result;
		}

		protected class WarehouseImportResult
		{
			public WarehouseImportResult(bool hasValidWarehouse, WhsWarehouse warehouse)
			{
				HasValidWarehouse = hasValidWarehouse;
				Warehouse = warehouse;
			}

			public readonly bool HasValidWarehouse;
			public readonly WhsWarehouse Warehouse;
		}

		protected ZQuery GetWarehouseAddressQuery(ZGuid warehouseAddressPK)
		{
			var addressQuery = new ZQuery();
			addressQuery.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddressPK);
			addressQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
			addressQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, SQLComparisonOperator.Equal, true);

			return addressQuery;
		}

		#endregion

		#endregion

		#region PopulatePickPriority

		protected void PopulatePickPriority(TDocket docket, Order orderDataObject)
		{
			if (orderDataObject.PickPriority > 20)
			{
				var errorMessage = Res.GetString("2f0b7b20-7c5a-415a-aacc-1a25d25a2c1a",
					"Cannot Import {0}, Pick Priority should be in range 0 to 20.", DocketType);
				throw new DataObjectReadFailureException(errorMessage);
			}
			else
			{
				SetValue(docket, WhsDocketSchema.WD_PickPriority, orderDataObject.PickPriority);
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(TDocket docket)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var addressesToIgnore = AddressTypesToIgnoreWhenAddingToDocketDocAddressCollection.ToArray();
				foreach (var organisationDataObject in dataObject.OrganizationAddressCollection)
				{
					bool ignoreAddress = addressesToIgnore.Any(a => organisationDataObject.AddressType.GetValueOrDefault().EqualsIgnoringCase(a.ToString()));
					if (!ignoreAddress && !PopulateAdditionalAddresses(organisationDataObject, docket))
					{
						var orgReader = new OrganisationDataObjectReader(organisationDataObject, logger, factory)
						{
							PopulateIsResidential = PopulateJobDocAddressIsResidential
						};
						var jobDocAddress = orgReader.GetMatchedOrNew(docket);
						if (jobDocAddress != null)
						{
							docket.DocAddresses.Add(jobDocAddress);
						}
					}
				}

				AfterPopulateAddresses(docket);
			}
		}

		void AfterPopulateAddresses(TDocket docket)
		{
			AfterPopulateAddressesCore(docket);
		}

		protected virtual void AfterPopulateAddressesCore(TDocket docket)
		{
		}

		protected virtual bool PopulateJobDocAddressIsResidential
		{
			get { return false; }
		}

		IEnumerable<ZString> AddressTypesToIgnoreWhenAddingToDocketDocAddressCollection
		{
			get { return new ZString[] { ImportStrategy.ClientDocAddressType.ToString(), nameof(DocAddressType.Warehouse), AddressTypes.WarehouseClient }.Concat(AddressTypesToIgnoreWhenAddingToDocketDocAddressCollectionCore); }
		}

		protected virtual IEnumerable<ZString> AddressTypesToIgnoreWhenAddingToDocketDocAddressCollectionCore
		{
			get { return Enumerable.Empty<ZString>(); }
		}

		protected virtual bool PopulateAdditionalAddresses(OrganizationAddress organizationDataObject, TDocket docket)
		{
			return false;
		}

		protected bool IsAddressTypeNotPresentInAddressCollection(DocAddressType addressType)
		{
			return !dataObject.OrganizationAddressCollection.Any(o => o.AddressType.GetValueOrDefault().EqualsIgnoringCase(addressType.ToString()));
		}

		#endregion

		#region PopulateNotes

		void PopulateNotes(TDocket docket)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, docket).ReadIntoCollection();
			}
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(TDocket docket)
		{
			var additionalReferenceCollection = GetAdditionalReferencesFromImport();
			if (additionalReferenceCollection != null)
			{
				var reader = new WhsDocketReferenceCollectionReader(additionalReferenceCollection, logger, factory, docket);
				reader.ReadIntoCollection();
			}
		}

		protected virtual DataObjectList<AdditionalReference> GetAdditionalReferencesFromImport()
		{
			return dataObject.AdditionalReferenceCollection;
		}

		#endregion

		#region PopulateLines

		void PopulateLines(TDocket docket)
		{
			using (new SemaphoreManager(docket.UpdatingWeightAndVolumeSemaphore))
			{
				var lines = GetDocketLinesCollection(docket.Client);
				if (lines.Length > 0)
				{
					if (IsNewBO || ImportStrategy.CanUpdateDocketLines(docket))
					{
						SortDocketLinesToImport(lines);
						var docketLineCollectionReader = new DocketLinesDataObjectCollectionReader(docket, this, lines);
						if (ShouldDeleteUnmatchedDocketLines(docket, lines))
						{
							ImportStrategy.BeforeReadIntoCollection(docket);
							docketLineCollectionReader.ReadIntoCollection();
						}
						else
						{
							docketLineCollectionReader.ReadIntoCollectionRetainingUnmatchedElements();
						}
						ImportStrategy.ValidateAfterPopulateLines(docket);
					}
					else
					{
						logger.Log(LogType.Warning, CannotUpdateLinesWarningMessage);
					}
				}
			}
		}

		protected OrderLine[] GetDocketLinesCollection(OrgHeader client)
		{
			// Empty collection on UXML means "delete all lines", but we're just saying "no lines to update".
			// Do not change to return IEnumerable<T> because the collection will be evaluated multiple times.
			var result = GetDocketLinesCollectionCore(client);
			return (result != null) ? result.ToArray() : Array.Empty<OrderLine>();
		}

		protected virtual IEnumerable<OrderLine> GetDocketLinesCollectionCore(OrgHeader client)
		{
			return dataObject.Order?.OrderLineCollection;
		}

		void SortDocketLinesToImport(OrderLine[] lines)
		{
			Array.Sort(lines, (x, y) =>
			{
				var comparison = CompareNullableNumbers(x.LineNumber, y.LineNumber);
				return comparison != 0 ? comparison : CompareNullableNumbers(x.SubLineNumber, y.SubLineNumber);
			});
		}

		static int CompareNullableNumbers(int? a, int? b)
		{
			return (a == null, b == null) switch
			{
				(true, true) => 0,
				(true, false) => 1,
				(false, true) => -1,
				_ => a.Value.CompareTo(b.Value)
			};
		}

		protected virtual string CannotUpdateLinesWarningMessage => Res.GetString("4936397a-e58d-4ac7-90d9-988c5ad8bc16", "Cannot update {0} Lines on a Finalized {0}.", DocketType);

		#region class DocketLinesDataObjectReader

		class DocketLinesDataObjectCollectionReader : DataObjectCollectionReader<OrderLine, TDocketLine>
		{
			internal DocketLinesDataObjectCollectionReader(TDocket docket, WhsDocketDataObjectReader<TDocket, TDocketLine> reader, OrderLine[] orderLineDataObjects)
				: base(orderLineDataObjects)
			{
				Docket = docket;
				Reader = reader;
			}

			readonly TDocket Docket;
			readonly WhsDocketDataObjectReader<TDocket, TDocketLine> Reader;
			int Index = 1;

			protected override void AddToCollection(TDocketLine businessObject)
			{
				Docket.Lines.Add(businessObject);
			}

			protected override TDocketLine[] BusinessObjects
			{
				get { return businessObjects ?? (businessObjects = Docket.Lines.ToArray<TDocketLine>()); }
			}
			TDocketLine[] businessObjects;

			protected override TDocketLine ReadIntoBusinessObject(OrderLine orderLineDataObject, TDocketLine businessObject)
			{
				orderLineDataObject.Link = Index++;
				var bizoRead = Reader.GetNewLineReader(Docket, orderLineDataObject, MatchedLines).ReadIntoBusinessObject();
				if (bizoRead != null)
				{
					MatchedLines.Add(bizoRead);
				}
				return bizoRead;
			}

			HashSet<TDocketLine> MatchedLines
			{
				get { return matchedLines ?? (matchedLines = new HashSet<TDocketLine>()); }
			}
			HashSet<TDocketLine> matchedLines;

			protected override void RemoveFromCollection(TDocketLine docketLine) => Reader.RemoveFromCollection(Docket, docketLine);

			protected override TDocketLine FindMatchingBusinessObject(OrderLine orderLineDataObject)
			{
				return null;
			}
		}

		protected abstract bool ShouldDeleteUnmatchedDocketLines(TDocket docket, IEnumerable<OrderLine> lines);
		protected abstract DataObjectReader<OrderLine, TDocketLine> GetNewLineReader(TDocket docket, OrderLine orderLineDataObject, IEnumerable<TDocketLine> matchedLines);

		protected virtual void RemoveFromCollection(TDocket docket, TDocketLine docketLine)
		{
			docket.Lines.Delete(docketLine);
		}

		#endregion

		#endregion

		#region PopulateCustomFields

		void PopulateCustomFields(TDocket docket)
		{
			var usedCustomField = PopulateOrganisationLevelCustomFields(docket);

			if (SupportsWorkflowCustomFieldImport)
			{
				PopulateWorkflowCustomFields(docket, dataObject, usedCustomField);
			}
		}

		protected virtual bool SupportsWorkflowCustomFieldImport => true;

		(string, DataType?)[] PopulateOrganisationLevelCustomFields(TDocket docket)
		{
			var reader = new CustomLabelsCustomizedFieldDataObjectReader(logger);
			return reader.PopulateCustomFields(WhsDocketSchema.Instance, docket, dataObject, new WhsDocket.CustomLabelsProvider(docket));
		}

		#endregion

		#region Import Strategy

		protected WhsImportStrategy ImportStrategy
		{
			get { return importStrategy ?? (importStrategy = GetNewImportStrategy()); }
		}

		protected virtual WhsImportStrategy GetNewImportStrategy()
		{
			return new WhsImportStrategy(this);
		}

		WhsImportStrategy importStrategy;

		#region WhsImportStrategy

		public class WhsImportStrategy
		{
			public WhsImportStrategy(WhsDocketDataObjectReader<TDocket, TDocketLine> reader)
			{
				Reader = reader;
			}

			protected readonly WhsDocketDataObjectReader<TDocket, TDocketLine> Reader;

			#region ShouldImportLine

			public bool ShouldImportLine(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
			{
				return ShouldImportLineCore(warehouseCustomsLineDetails);
			}

			protected virtual bool ShouldImportLineCore(IWarehouseCustomsLineDetails warehouseCustomsLineDetails)
			{
				return true; // all lines are imported by default
			}

			#endregion

			#region SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelled

			public bool SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelled(TDocket docket)
			{
				return SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelledCore(docket);
			}

			protected virtual bool SetMostRecentDocketForCustomsIfValid_VirtualFinalisedOrRealCancelledCore(TDocket docket)
			{
				return false; // never needed
			}

			#endregion

			#region LoadDocketFromCustomsLinesDocketNumbers

			public TDocket LoadDocketFromCustomsLinesDocketNumbers => LoadDocketFromCustomsLinesDocketNumbersCore();

			protected virtual TDocket LoadDocketFromCustomsLinesDocketNumbersCore() => null; // never needed

			#endregion

			#region DocketSubType

			public UniversalCodeDescriptionPair DocketSubType => DocketSubTypeCore;

			protected virtual UniversalCodeDescriptionPair DocketSubTypeCore => Reader.dataObject.Order?.Type;

			#endregion

			#region ExternalReference

			public void SetExternalReference(TDocket docket)
			{
				SetExternalReferenceCore(docket);
			}

			protected virtual void SetExternalReferenceCore(TDocket docket)
			{
				Reader.SetValue(docket, WhsDocketSchema.WD_ExternalReference, ExternalReference);
			}

			protected virtual ZString? ExternalReference => Reader.dataObject.Order?.OrderNumber;

			#endregion

			#region MatchingReference

			public ZString? MatchingReference => MatchingReferenceCore;

			protected virtual ZString? MatchingReferenceCore => ExternalReference;

			#endregion

			#region ExternalReferenceSplit

			public ZByte? ExternalReferenceSplit => ExternalReferenceSplitCore;

			protected virtual ZByte? ExternalReferenceSplitCore => Reader.dataObject.Order?.OrderNumberSplit;

			#endregion

			#region TransportMode

			public UniversalCodeDescriptionPair TransportMode => TransportModeCore;

			protected virtual UniversalCodeDescriptionPair TransportModeCore => Reader.dataObject.TransportMode;

			#endregion

			#region ClientDocAddressType

			public DocAddressType ClientDocAddressType => ClientDocAddressTypeCore;

			protected virtual DocAddressType ClientDocAddressTypeCore => DocAddressType.ConsignorDocumentaryAddress;

			#endregion

			#region ClientOrganizationAddress

			public OrganizationAddress ClientOrganizationAddress
			{
				get { return ClientOrganizationAddressCore ?? Reader.dataObject.OrganizationAddressCollection.FirstOrDefault(new ZString[] { AddressTypes.WarehouseClient, ClientDocAddressType.ToString() }); }
			}

			protected virtual OrganizationAddress ClientOrganizationAddressCore => null;

			#endregion

			#region GetRelatedWarehouse

			public WhsWarehouse GetRelatedWarehouse(TDocket docket)
			{
				WhsWarehouse result = null;

				if (Reader.dataObject.OrganizationAddressCollection != null)
				{
					var warehouseAddressData = Reader.dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Warehouse));
					if (warehouseAddressData != null)
					{
						var warehouseAddress = new OrganisationDataObjectReader(warehouseAddressData, Reader.logger, Reader.factory).GetMatched();
						if (warehouseAddress != null)
						{
							result = LoadWarehouseByAddress(warehouseAddress);
						}
					}
				}

				return result ?? GetRelatedWarehouseCore(docket);
			}

			protected WhsWarehouse LoadWarehouseByAddress(OrgAddress warehouseAddress)
			{
				return Reader.factory.LoadTop1<WhsWarehouse>(Reader.GetWarehouseAddressQuery(warehouseAddress.PK));
			}

			protected virtual WhsWarehouse GetRelatedWarehouseCore(TDocket docket)
			{
				return null;
			}

			#endregion

			#region CanUpdateDocketLines

			public bool CanUpdateDocketLines(TDocket docket) => CanUpdateDocketLinesCore(docket);

			protected virtual bool CanUpdateDocketLinesCore(TDocket docket) => !docket.WD_FinalisedDate.IsValid;

			#endregion

			#region IsInwardProcessingJob

			public bool IsInwardProcessingJob => IsInwardProcessingJobCore;

			protected virtual bool IsInwardProcessingJobCore => false;

			#endregion

			#region IsWarehouseBondedChangeOfWarehouse

			public bool IsWarehouseBondedChangeOfWarehouse => IsWarehouseBondedChangeOfWarehouseCore;

			protected virtual bool IsWarehouseBondedChangeOfWarehouseCore => false;

			#endregion

			#region BeforeReadIntoCollection

			public void BeforeReadIntoCollection(TDocket docket)
			{
				BeforeReadIntoCollectionCore(docket);
			}

			protected virtual void BeforeReadIntoCollectionCore(TDocket docket)
			{
			}

			#endregion

			#region BeforePopulate

			public void BeforePopulate(TDocket docket)
			{
				BeforePopulateCore(docket);
			}

			protected virtual void BeforePopulateCore(TDocket docket)
			{
			}

			#endregion

			#region PopulateBizOSuspender

			public IDisposable PopulateBizOSuspender(TDocket docket)
			{
				return PopulateBizOSuspenderCore(docket);
			}

			protected virtual IDisposable PopulateBizOSuspenderCore(TDocket docket)
			{
				return null;
			}

			#endregion

			#region AfterPopulate

			public void AfterPopulate(TDocket docket)
			{
				AfterPopulateCore(docket);
			}

			protected virtual void AfterPopulateCore(TDocket docket)
			{
			}

			#endregion

			#region SetRequiredDateToTodayIfEmpty

			protected static void SetRequiredDateToTodayIfEmpty(WhsDocket docket)
			{
				if (docket.WD_RequiredDate.IsEmpty)
				{
					docket.WD_RequiredDate = docket.Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
				}
			}

			#endregion

			#region RejectIfInShortfall

			protected void RejectIfInShortfall(WhsPickableDocket order)
			{
				if (order.ShortfallExists)
				{
					var shortfallErrors = new ZStringBuilder();
					var productsToOrderToFixShortfall = WhsOrderCustomsErrorMessageAdvisorUS.GetQuantitiesRequired(order);
					var hasStock = productsToOrderToFixShortfall.HasEnoughStock;
					if (hasStock)
					{
						if (productsToOrderToFixShortfall.ProductsToOrder.Count > 0)
						{
							shortfallErrors.Append(Res.GetString("6987d759-cdf8-4853-b330-3d3eed528ea0",
								"In order to fix shortfalls, you will need to modify lines as follows:"));
						}
						else
						{
							shortfallErrors.Append(Res.GetString("873c7632-5eb7-400c-9d7d-69eba5b94134",
								"The Pick could not match Inventory to your Order. /r/n Please ensure your order details are correct: - Warehouse Entry No., Entry/Line No. and WHS Line configuration."));
						}
					}
					else
					{
						shortfallErrors.Append(Res.GetString("4ce53ddb-5c8f-4b54-9085-6046be098ede", "You do not have enough stock to fulfill shortfalls on this order"));
					}

					var productWithAttributesCollectionToUse = hasStock ? productsToOrderToFixShortfall.ProductsToOrder : productsToOrderToFixShortfall.QtyShortInWhs;
					foreach (var productWithAttributes in productWithAttributesCollectionToUse)
					{
						if (productWithAttributes.Value < 0)
						{
							var product = order.Factory.Load<OrgSupplierPart>(productWithAttributes.Key.ProductPK);

							var lines = order.AllLines.Cast<WhsDocketLine>().Where(l => WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(ProductWithAttributes.GetProductWithAttributes(l), productWithAttributes.Key));
							var numberOrdered = (ZDecimal)lines.Sum(l => l.WE_TransactionQuantity);

							var startOfAppend = !productWithAttributes.Key.BondedEntryKey.IsEmpty
								? Res.GetString("7cd60f84-62ef-4e6c-8c42-5c1bbd54900e", "{0} Product {1}/{2}", productWithAttributes.Key.BondedEntryKey, product.OP_PartNum, product.OP_Desc)
								: Res.GetString("847d405c-c199-4a47-9a4d-962416cfa704", "Product {0}/{1}", product.OP_PartNum, product.OP_Desc);

							var endOfAppend = hasStock
								? Res.GetString("784ba84e-6da9-4a54-afae-0b81f60a98e9", "will need {0:0} to be added to the order", Math.Abs(productWithAttributes.Value))
								: Res.GetString("dceb8d63-5653-46c4-b450-c00240c13098", "can not be ordered due to lack of stock. {0:0} was ordered, but {1:0} is available",
									numberOrdered, numberOrdered + productWithAttributes.Value);

							shortfallErrors.Append(Res.GetString("05d4faa0-b29a-4e2a-b779-565b1169f6f9", "{0} {1}", startOfAppend, endOfAppend));
						}
					}

					var message = new ZStringBuilder(Res.GetString("50384d8a-86b5-4659-8977-8e31576b9af2",
						"{0} could not be created for {1} because there are errors:\r\n{2}",
						Reader.DocketType, GetNameForFinaliseDocket(order as TDocket), shortfallErrors.ToStringWithNewLineBetweenAppends()));

					Reader.ThrowImportFailureExceptionIfNotEmpty(message);
				}
			}

			#endregion

			#region FinaliseDocket

			protected void FinaliseDocket(TDocket docket)
			{
				using (((IBusinessObjectInternals)docket).ResumeValidationForAllDescendantsTemporarily())
				{
					FinaliseDocketWithoutUserConfirmation(docket);
				}

				if (!docket.IsFinalised)
				{
					// unfortunately orders adds errors to the notify but receive adds to both, so to be safe just grab both.
					var errorsFromNotify = ((NotificationBuffer)docket.NotificationSubscriber).AsString.TrimEnd();
					var errorsFromDocket = string.Join("\r\n", docket.NotificationsIncludingChildren.GetErrors().Select(e => e.Message));

					var finaliseErrors = new ZStringBuilder();
					finaliseErrors.AppendIfNotEmpty(errorsFromNotify);
					finaliseErrors.AppendIfNotEmpty(errorsFromDocket);

					var message = new ZStringBuilder(Res.GetString("96e9401f-3cea-48de-b72a-b490fd49deab",
						"{0} could not be finalized into the Warehouse for {1} because of the following error(s):\r\n{2}", Reader.DocketType, GetNameForFinaliseDocket(docket), finaliseErrors.ToStringWithNewLineBetweenAppends()));

					Reader.ThrowImportFailureExceptionIfNotEmpty(message);
				}

				AfterFinaliseDocket(docket);
			}

			protected virtual void FinaliseDocketWithoutUserConfirmation(TDocket docket) => docket.FinaliseDocketWithoutUserConfirmation();

			protected virtual string GetNameForFinaliseDocket(TDocket docket) => docket.HumanReadableName;

			protected virtual void AfterFinaliseDocket(TDocket docket)
			{
			}

			#endregion

			#region SendErrorReporterIfFailedToFinalizePick

			protected void SendErrorReporterIfFailedToFinalizePick(WhsPickableDocket order)
			{
				var pick = order.Pick;
				if (!pick.IsFinalised)
				{
					var pickErrorReportingHelper = new WhsPickFinalisationErrorReportingHelper(pick);
					throw new DataObjectReadFailureException($"Cannot finalize pick\r\nOrder External Reference: {order.WD_ExternalReference} {pickErrorReportingHelper.ReportPickFinalisationErrorMessage()}");
				}
			}

			#endregion

			#region RequireClientForMatch

			public bool RequireClientForMatch => RequireClientForMatchCore;

			protected virtual bool RequireClientForMatchCore => true;

			public bool ShouldPopulateBizO(UniversalShipment dataObject, TDocket docket)
			{
				return ShouldPopulateBizOCore(dataObject, docket);
			}

			protected virtual bool ShouldPopulateBizOCore(UniversalShipment dataObject, TDocket docket)
			{
				return true;
			}

			#endregion

			#region AddMatchingReferenceFilter

			public void AddMatchingReferenceFilter(ZString matchingReferenceNumber, ZQuery query)
			{
				AddMatchingReferenceFilterCore(matchingReferenceNumber, query);
			}

			protected virtual void AddMatchingReferenceFilterCore(ZString matchingReferenceNumber, ZQuery query)
			{
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, matchingReferenceNumber);
			}

			#endregion

			#region AddAdditionalFilter

			public void AddAdditionalFilter(UniversalShipment dataObject, ZQuery query)
			{
				AddAdditionalFilterCore(dataObject, query);
			}

			protected virtual void AddAdditionalFilterCore(UniversalShipment dataObject, ZQuery query)
			{
				if (dataObject.Order != null)
				{
					query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, dataObject.Order.OrderNumberSplit.GetValueOrDefault());
				}
			}

			#endregion

			#region GetReasonForNotAbleToUpdateMatchedDocket

			public ZString GetReasonForNotAbleToUpdateMatchedDocket(TDocket matchedDocket)
			{
				return GetReasonForNotAbleToUpdateMatchedDocketCore(matchedDocket);
			}

			protected virtual ZString GetReasonForNotAbleToUpdateMatchedDocketCore(TDocket matchedDocket)
			{
				return matchedDocket.IsCancelled
					? Res.GetString("ab72ee2a-a2a2-43a5-961c-9920023f447a", "Warehouse {0} {1} could not be updated because it is Canceled.", Reader.DocketType, matchedDocket.WD_DocketID)
					: "";
			}

			#endregion

			#region OnWarehouseMatched

			public void OnWarehouseMatched(WhsWarehouse warehouse)
			{
				OnWarehouseMatchedCore(warehouse);
			}

			protected virtual void OnWarehouseMatchedCore(WhsWarehouse warehouse)
			{
			}

			#endregion

			#region AfterPopulateLines

			public void ValidateAfterPopulateLines(TDocket docket)
			{
				ValidateAfterPopulateLinesCore(docket);
			}

			protected virtual void ValidateAfterPopulateLinesCore(TDocket docket)
			{
			}

			#endregion
		}

		#endregion

		#endregion

		#region IDocketType Members

		string IDocketType.DocketType => DocketType;

		#endregion
	}
}
