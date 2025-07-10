using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentAddressDataObjectReader : DataObjectReader<Instruction, DtbConsignmentAddress>
	{
		public DtbConsignmentAddressDataObjectReader(Instruction addressDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, DtbConsignment consignment, Dictionary<ZInt, PkgPackage> packageLinksDictionary = null, Dictionary<ZInt, PkgPackage> containerLinksDictionary = null)
			: base(addressDataObject, logger, factory)
		{
			Consignment = Argument.NotNull(consignment, nameof(consignment));
			this.packageLinksDictionary = packageLinksDictionary;
			this.containerLinksDictionary = containerLinksDictionary;
		}

		readonly DtbConsignment Consignment;
		readonly Dictionary<ZInt, PkgPackage> packageLinksDictionary;
		readonly Dictionary<ZInt, PkgPackage> containerLinksDictionary;

		#region GetExistingBusinessObject

		protected override DtbConsignmentAddress GetExistingBusinessObject()
		{
			DtbConsignmentAddress result = null;

			// we want to update existing ConsignmentAddresses since Consignments will have only 1 Pick Up and 1 Delivery
			var instructionType = dataObject.Type.GetCodeAsUpperCase();
			if (instructionType != ConsignmentAddressTypes.Codes.Multi)
			{
				var consignmentRow = GetColumnIndexerFromRow(Consignment);
				var query = new ZQuery();
				query.AddToFilter(DtbConsignmentAddressSchema.LTS_InstructionType, instructionType);
				query.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, consignmentRow.GetValue(DtbConsignmentSchema.PK));

				var existingConsignmentAddress = factory.Load<DtbConsignmentAddress>(query);
				result = existingConsignmentAddress.Length == 1 ? existingConsignmentAddress[0] : null;
			}

			return result;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(DtbConsignmentAddress consignmentAddress)
		{
			PopulateData(consignmentAddress);
			PopulateRelatedEntities(consignmentAddress);
		}

		#region PopulateRelatedEntities

		void PopulateRelatedEntities(DtbConsignmentAddress consignmentAddress)
		{
			PopulateEquipment(consignmentAddress);
			PopulateAddress(consignmentAddress);
			PopulateActions(consignmentAddress);
		}

		#region PopulateEquipment

		void PopulateEquipment(DtbConsignmentAddress consignmentAddress)
		{
			var equipmentCode = dataObject.Equipment.GetValueOrDefault();
			var equipment = GetColumnIndexerFromRow(factory.RowFactory.LoadFromNaturalKey(RefEquipmentSchema.Constants.TableName, RefEquipmentSchema.RQ_ShortCode, equipmentCode, false));
			if (equipment != null)
			{
				var row = GetColumnIndexerFromRow(consignmentAddress);
				SetValue(row, DtbConsignmentAddressSchema.LTS_RQ_RequiredEquipment, equipment.GetValue(RefEquipmentSchema.PK));
			}
		}

		#endregion

		#region PopulateAddress

		void PopulateAddress(DtbConsignmentAddress consignmentAddress)
		{
			if (dataObject.Address != null)
			{
				var reader = new OrganisationDataObjectReader(dataObject.Address, logger, factory);
				var docAddressType = OrganisationDataObjectReader.GetDocAddressType(dataObject.Address.AddressType.GetValueOrDefault());
				var orgAddress = dataObject.Address.HasTypeOnly()
					? null // If Address is empty it means no address was specified, but we want to Create an empty JobDocAddress to have an Address Type.
					: reader.GetMatched(Consignment, docAddressType.GetOrganisationType(), docAddressType.GetOrganisationSubType());

				var consignmentAddressRow = GetColumnIndexerFromRow(consignmentAddress);
				DeleteExistingJobDocAddresses(consignmentAddressRow);

				var jobDocAddress = factory.New<JobDocAddress>();
				PopulateJobDocAddressData(docAddressType, orgAddress, consignmentAddressRow, jobDocAddress);
				ReloadDocAddresses(consignmentAddress, jobDocAddress);

				if (orgAddress != null || !dataObject.Address.HasTypeOnly())
				{
					reader.PopulateJobDocAddress(orgAddress, jobDocAddress);
				}
				else
				{
					jobDocAddress.MakePersistentEvenIfEmpty();
				}

				PopulateDomesticZone(consignmentAddressRow, jobDocAddress);
			}
		}

		void DeleteExistingJobDocAddresses(IColumnIndexer consignmentAddressRow)
		{
			var existingDocAddresses = factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignmentAddressRow.GetValue(DtbConsignmentAddressSchema.PK)));

			foreach (var existingJobDocAddress in existingDocAddresses)
			{
				existingJobDocAddress.HasChanges = true; // for proper delete, see WIx, once core fixes we should be able to remove this hack
			}

			consignmentAddressRow.DeleteAllJobDocAddresses(factory.RowFactory, DtbConsignmentAddressSchema.PK);
		}

		void PopulateJobDocAddressData(DocAddressType docAddressType, OrgAddress orgAddress, IColumnIndexer consignmentAddressRow, JobDocAddress jobDocAddress)
		{
			var row = GetColumnIndexerFromRow(jobDocAddress);
			SetValue(row, JobDocAddressSchema.E2_ParentID, consignmentAddressRow.GetValue(DtbConsignmentAddressSchema.PK));
			SetValue(row, JobDocAddressSchema.E2_ParentTableCode, DtbConsignmentAddressSchema.Constants.Prefix);

			var organisation = orgAddress != null ? orgAddress.Header : null;
			var consignmentAddressDocAddressType = ConsignmentHelper.GetInstructionDocAddressTypeToChangeTo(consignmentAddressRow.GetValue(DtbConsignmentAddressSchema.LTS_InstructionType), docAddressType);
			var addressType = DocAddressTypes.GetCode(factory.BOFactory, consignmentAddressDocAddressType);

			SetValue(row, JobDocAddressSchema.E2_AddressType, addressType);
		}

		static void ReloadDocAddresses(DtbConsignmentAddress consignmentAddress, JobDocAddress jobDocAddress)
		{
			var docAddresses = consignmentAddress.DocAddresses;
			if (!docAddresses.Contains(jobDocAddress))
			{
				docAddresses.Load();

				if (!docAddresses.Contains(jobDocAddress))
				{
					throw new InvalidOperationException("Could not add JobDocAddress to ConsignmentAddress.");
				}
			}
		}

		#endregion

		#region PopulateDomesticZone

		void PopulateDomesticZone(IColumnIndexer consignmentAddress, JobDocAddress jobDocAddress)
		{
			var row = GetColumnIndexerFromRow(jobDocAddress);

			ZString countryCode = ZString.Empty;
			ZString postCode = ZString.Empty;
			ZString city = ZString.Empty;
			ZString stateCode = ZString.Empty;

			if (row.GetValue(JobDocAddressSchema.E2_AddressOverride))
			{
				countryCode = row.GetValue(JobDocAddressSchema.E2_RN_NKCountryCode);
				postCode = row.GetValue(JobDocAddressSchema.E2_Postcode);
				city = row.GetValue(JobDocAddressSchema.E2_City);
				stateCode = row.GetValue(JobDocAddressSchema.E2_State);
			}

			else
			{
				var addressRow = GetAddressRowFromJobDocAddress(row);
				if (addressRow != null)
				{
					countryCode = addressRow.GetValue(OrgAddressSchema.OA_RN_NKCountryCode);
					postCode = addressRow.GetValue(OrgAddressSchema.OA_PostCode);
					city = addressRow.GetValue(OrgAddressSchema.OA_City);
					stateCode = addressRow.GetValue(OrgAddressSchema.OA_State);
				}
			}

			var zone = GetDomesticZone(countryCode, postCode, city, stateCode, "OPS");
			if (zone != null)
			{
				SetValue(consignmentAddress, DtbConsignmentAddressSchema.LTS_TZ_DomesticZone, zone.PK);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Sql command")]
		RateTransportZone GetDomesticZone(ZString countryCode, ZString postCode, ZString city, ZString stateCode, ZString zoneType)
		{
			var queryText = @"EXEC [dbo].[GetTransportZoneSP] @CountryCode, @PostCode, @CityName, @StateCode, @ZoneType";

			ZSqlParameterCollection queryParameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@CountryCode", countryCode, RateTransportZoneItemSchema.TQ_RN_NKCountry),
				ZSqlParameter.New("@PostCode", postCode, RefPostCodeSchema.RK_CityTownPostCode),
				ZSqlParameter.New("@CityName", city, RefCityTownSchema.R9_InternationalName),
				ZSqlParameter.New("@StateCode", stateCode, RefCityTownSchema.R9_RW_NKState),
				ZSqlParameter.New("@ZoneType", zoneType, RateTransportProviderSchema.TP_ZoneType)
			};

			DynamicBusinessObjectCollection dynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(factory.BOFactory);
			dynamicBusinessObjectCollection.Load(queryText, queryParameters);

			if (dynamicBusinessObjectCollection.Count == 1)
			{
				return factory.Load<RateTransportZone>((ZGuid)dynamicBusinessObjectCollection[0][RateTransportZoneItemSchema.TQ_TZ_DomesticZone]);
			}

			return null;
		}

		IColumnIndexer GetAddressRowFromJobDocAddress(IColumnIndexer jobDocAddress)
		{
			var orgAddressPK = jobDocAddress.GetValue(JobDocAddressSchema.E2_OA_Address);
			var orgAddress = factory.RowFactory.LoadFromPK(OrgAddressSchema.Constants.TableName, orgAddressPK);
			return GetColumnIndexerFromRow(orgAddress);
		}

		#endregion

		#region PopulateActions

		void PopulateActions(DtbConsignmentAddress consignmentAddress)
		{
			// we do not want override existing actions if the user has already entered data for them on the Consignment side.
			if (IsNewBO)
			{
				ReadActionCollection(consignmentAddress);
			}

			AddActionIfConsignmentAddressHasNone(consignmentAddress);
		}

		void ReadActionCollection(DtbConsignmentAddress consignmentAddress)
		{
			var packingLineActions = dataObject.InstructionPackingLineLinkCollection?.FirstOrDefault()?.ConfirmationCollection?
									.Where(action => action.DateDescription.HasValue && !action.DateDescription.Value.EqualsIgnoringCase(ActionTypes.Codes.ConNoteNo));
			var containerLineActions = dataObject.InstructionContainerLinkCollection?.FirstOrDefault()?.ConfirmationCollection?
									.Where(action => action.DateDescription.HasValue && !action.DateDescription.Value.EqualsIgnoringCase(ActionTypes.Codes.ConNoteNo));

			ProcessActions(consignmentAddress, packingLineActions);
			ProcessActions(consignmentAddress, containerLineActions);
		}

		void ProcessActions(DtbConsignmentAddress consignmentAddress, IEnumerable<Confirmation> actions)
		{
			if (actions != null && actions.Any())
			{
				new ActionDataObjectCollectionReader(this, consignmentAddress, actions.ToArray(), packageLinksDictionary, containerLinksDictionary).ReadIntoCollection();
			}
		}

		void AddActionIfConsignmentAddressHasNone(DtbConsignmentAddress consignmentAddress)
		{
			var consignmentAddressPK = GetColumnIndexerFromRow(consignmentAddress).GetValue(DtbConsignmentAddressSchema.PK);
			var query = new ZQuery(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, consignmentAddressPK);
			var allowedActionTypes = new[] { ActionTypes.Codes.PickUp, ActionTypes.Codes.Delivery };

			if (factory.RowFactory.Load(DtbConsignmentActionSchema.Constants.TableName, query).Length == 0 && allowedActionTypes.Contains(dataObject.Type?.Code?.ToString()))
			{
				var action = factory.New<DtbConsignmentAction>();
				SetValue(action, DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, consignmentAddressPK);
				SetValue(action, DtbConsignmentActionSchema.LTA_ActionType, dataObject.Type);
			}
		}

		#endregion

		#endregion

		#region PopulateData

		void PopulateData(DtbConsignmentAddress consignmentAddress)
		{
			var row = GetColumnIndexerFromRow(consignmentAddress);
			SetValue(row, DtbConsignmentAddressSchema.LTS_DropMode, dataObject.DropMode);
			SetValue(row, DtbConsignmentAddressSchema.LTS_InstructionType, dataObject.Type);
			SetValue(row, DtbConsignmentAddressSchema.LTS_Notes, dataObject.ServiceInstruction);
			SetValue(row, DtbConsignmentAddressSchema.LTS_Status, TransportStatuses.Codes.Allocated);
			SetValue(row, DtbConsignmentAddressSchema.LTS_Sequence, dataObject.Sequence);
		}

		#endregion

		#endregion

		#region ActionDataObjectCollectionReader

		class ActionDataObjectCollectionReader : DataObjectCollectionReader<Confirmation, DtbConsignmentAction>
		{
			public ActionDataObjectCollectionReader(DtbConsignmentAddressDataObjectReader reader,
				IColumnIndexer action, Confirmation[] actionDataObjects,
				Dictionary<ZInt, PkgPackage> packageLinksDictionary,
				Dictionary<ZInt, PkgPackage> containerLinksDictionary)
				: base(actionDataObjects)
			{
				Reader = reader;
				Action = action;
				this.packageLinksDictionary = packageLinksDictionary;
				this.containerLinksDictionary = containerLinksDictionary;
			}

			readonly DtbConsignmentAddressDataObjectReader Reader;
			readonly IColumnIndexer Action;
			readonly Dictionary<ZInt, PkgPackage> packageLinksDictionary;
			readonly Dictionary<ZInt, PkgPackage> containerLinksDictionary;

			protected override void AddToCollection(DtbConsignmentAction action)
			{
				var row = GetColumnIndexerFromRow(action);
				Reader.SetValue(row, DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, Action.GetValue(DtbConsignmentAddressSchema.PK));
			}

			protected override DtbConsignmentAction[] BusinessObjects
			{
				get
				{
					var actionAddressQuery = new ZQuery(DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress, Action[DtbConsignmentAddressSchema.Constants.PK]);

					return Reader.factory.Load<DtbConsignmentAction>(actionAddressQuery);
				}
			}

			protected override DtbConsignmentAction FindMatchingBusinessObject(Confirmation actionDataObject)
			{
				return null;
			}

			protected override DtbConsignmentAction ReadIntoBusinessObject(Confirmation actionDataObject, DtbConsignmentAction action)
			{
				return new DtbConsignmentActionDataObjectReader(actionDataObject, Reader.logger, Reader.factory, packageLinksDictionary, containerLinksDictionary).ReadIntoBusinessObject();
			}

			protected override void RemoveFromCollection(DtbConsignmentAction action)
			{
				GetColumnIndexerFromRow(action).DeleteRowAndSetHasChanges(action);
			}

			protected override bool SkipEntity(Confirmation dataObject)
			{
				return dataObject.DateDescription.GetValueOrDefault().EqualsIgnoringCase(ActionTypes.Codes.ConNoteNo);
			}
		}

		#endregion
	}
}


