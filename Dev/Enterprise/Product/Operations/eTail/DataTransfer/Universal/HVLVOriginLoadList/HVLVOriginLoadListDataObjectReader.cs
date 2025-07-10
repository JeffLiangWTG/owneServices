using System;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOriginLoadListDataObjectReader : ShipmentDataObjectReader<HVLVOriginLoadList>
	{
		public HVLVOriginLoadListDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.HVLVOriginLoadList;

		protected override HVLVOriginLoadList GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override IMatchingBusinessEntityFinder<HVLVOriginLoadList> GetCombinedReferenceMatcher() => null;

		protected override void PopulateBusinessObject(HVLVOriginLoadList originLoadList)
		{
			var branchPK = GetMatchingGlbBranchPKFromDataContext(originLoadList);
			using (DisposableEnvironment.ForBranch(branchPK))
			{
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_TransportMode, dataObject.TransportMode?.Code);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_VesselName, dataObject.VesselName);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_VoyageFlight, dataObject.VoyageFlightNo);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_IsMasterHouse, dataObject.IsMasterHouse);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_Status, dataObject.OperationalStatus?.Code);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_RS_NKServiceLevel, dataObject.ServiceLevel?.Code);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_INCO, dataObject.ShipmentIncoTerm?.Code);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_RL_NKOrigin, dataObject.PortOfOrigin?.Code);
				SetValue(originLoadList, HVLVOriginLoadListSchema.HVL_RL_NKDestination, dataObject.PortOfDestination?.Code);

				PopulateAdditionalBill(originLoadList);
				PopulateContainer(originLoadList);
				PopulateDates(originLoadList);
				PopulateOrganisations(originLoadList);
				PopulateAddresses(originLoadList);
				PopulateItems(originLoadList);
			}
		}

		void PopulateAdditionalBill(HVLVOriginLoadList originLoadListBO)
		{
			if (dataObject.AdditionalBillCollection != null)
			{
				foreach (var additionalBill in dataObject.AdditionalBillCollection)
				{
					switch (additionalBill.BillType?.Code)
					{
						case BillTypeList.Codes.MasterBill:
							SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_MasterBillNumber, additionalBill.BillNumber);
							break;
						case BillTypeList.Codes.HouseBill:
							SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_HouseBillNumber, additionalBill.BillNumber);
							break;
						default:
							break;
					}
				}
			}
		}

		void PopulateContainer(HVLVOriginLoadList originLoadListBO)
		{
			if (dataObject.ContainerCollection?.Count == 1)
			{
				var containerDataObject = dataObject.ContainerCollection.FirstOrDefault();
				SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_ContainerNumber, containerDataObject.ContainerNumber);

				var containerType = containerDataObject.ContainerType?.Code.GetValueOrDefault();
				if (!string.IsNullOrEmpty(containerType))
				{
					var refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, containerType);
					SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_RC_ContainerType, refContainer?.PK);
				}
			}
		}

		void PopulateDates(HVLVOriginLoadList originLoadListBO)
		{
			if (dataObject.DateCollection != null)
			{
				foreach (var dateDataObject in dataObject.DateCollection)
				{
					switch (dateDataObject.Type)
					{
						case DateType.Departure:
							SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_E_Dep, dateDataObject.Value);
							break;
						case DateType.Arrival:
							SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_E_Arv, dateDataObject.Value);
							break;
						default:
							break;
					}
				}
			}
		}

		void PopulateOrganisations(HVLVOriginLoadList originLoadListBO)
		{
			var carrierOrgAddressDataObject = dataObject.OrganizationAddressCollection?.Where(a => (ZString)a.AddressType == nameof(DocAddressType.Carrier)).FirstOrDefault();

			if (carrierOrgAddressDataObject != null)
			{
				var carrierOrgHeader = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, carrierOrgAddressDataObject.OrganizationCode.ToString()));

				if (carrierOrgHeader != null)
				{
					SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_OH_Carrier, carrierOrgHeader.PK);
				}
			}
		}

		void PopulateAddresses(HVLVOriginLoadList originLoadListBO)
		{
			if (TryGetMatchingOrgAddress(DocAddressType.ArrivalCFSAddress, out var destinationDepotAddress, out var errorMessage))
			{
				SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_OA_DestinationDepot, destinationDepotAddress.PK);
			}
			else
			{
				throw new DataObjectReadFailureException(errorMessage);
			}

			if (TryGetMatchingOrgAddress(DocAddressType.DepartureCFSAddress, out var originDepotOrgAddress, out _))
			{
				SetValue(originLoadListBO, HVLVOriginLoadListSchema.HVL_OA_OriginDepot, originDepotOrgAddress.PK);
			}
		}

		bool ShouldDetachAllItemsFromLoadList(HVLVOriginLoadList originLoadListBO) => originLoadListBO.IsInDatabase && dataObject.PackingLineCollection.Content == CollectionContent.Complete;

		bool IsPackingLineValid => dataObject.PackingLineCollection != null && dataObject.PackingLineCollection.Content != null;

		void PopulateItems(HVLVOriginLoadList originLoadListBO)
		{
			if (IsPackingLineValid)
			{
				if (ShouldDetachAllItemsFromLoadList(originLoadListBO))
				{
					var attachedItemsList = factory.Load<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_HVL_LoadList, originLoadListBO.PK));
					foreach (var attachedItem in attachedItemsList)
					{
						attachedItem.HVI_HVL_LoadList = ZGuid.Empty;
					}
				}

				foreach (var packingLine in dataObject.PackingLineCollection)
				{
					if (packingLine.PackingLineCollection != null)
					{
						var outerPackageReader = new HVLVOuterPackageReader(packingLine, logger, factory);
						var outerPackage = outerPackageReader.ReadIntoBusinessObject();

						if (packingLine.PackingLineCollection.Any())
						{
							originLoadListBO.AttachMatchingItems(packingLine.PackingLineCollection, logger, factory);
						}

						outerPackage.HVO_HVL_LoadList = originLoadListBO.PK;
						originLoadListBO.OuterPackages.Add(outerPackage);
					}
					else
					{
						originLoadListBO.AttachMatchingItems(packingLine, logger, factory);
					}
				}
			}
		}

		#region GetMatchingGlbBranchFromDataContext

		Guid GetMatchingGlbBranchPKFromDataContext(HVLVOriginLoadList originLoadList)
		{
			var branchPK = Env.CurrentBranchPK;
			var dataContext = dataObject.DataContext;

			if (dataContext != null && !dataContext.EventBranchCode.IsEmpty)
			{
				var branch = originLoadList.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, dataContext.EventBranchCode);
				if (branch != null)
				{
					branchPK = branch.PK.ToGuid();
				}
			}

			return branchPK;
		}

		#endregion

		#region GetMatchingOrgAddress

		bool TryGetMatchingOrgAddress(DocAddressType addressType, out OrgAddress address, out string errorMessage)
		{
			errorMessage = string.Empty;
			address = null;

			var addressDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType.ToString());
			if (addressDataObject == null)
			{
				errorMessage = GetErrorMessageWhenMissingAddressDataObject(addressType);
			}
			else
			{
				address = new OrganisationDataObjectReader(addressDataObject, logger, factory).GetMatched();
				if (address == null)
				{
					errorMessage = GetErrorMessageWhenAddressMatchingFailed(addressType);
				}
			}

			return address != null;
		}

		string GetErrorMessageWhenMissingAddressDataObject(DocAddressType addressType)
		{
			var message = Res.GetString("b4d69e7b-8b41-4102-b1fa-0f93d222809c", "Can not find {0}", addressType.ToString());
			switch (addressType)
			{
				case DocAddressType.ArrivalCFSAddress:
					message = Res.GetString("6432ab4e-29c5-4fb9-8a7e-2abd683bff1d", "Can not find destination depot.");
					break;
			}
			return message;
		}

		string GetErrorMessageWhenAddressMatchingFailed(DocAddressType addressType)
		{
			var message = Res.GetString("72fd4676-98e8-4c4f-8674-76b3d3d1a4aa", "{0} is invalid.", addressType.ToString());
			switch (addressType)
			{
				case DocAddressType.ArrivalCFSAddress:
					message = Res.GetString("f5cae479-eb70-4f70-9780-f720ac796739", "Destination depot is invalid.");
					break;
			}
			return message;
		}

		#endregion

		#region GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(HVLVOriginLoadList targetBO)
		{
			if (targetBO != null)
			{
				if (targetBO.HVL_Status == HVLVOriginLoadListStatus.Codes.Consolidated)
				{
					return Res.GetString("a56b54eb-9692-4098-a720-228e4095b557", "Load List {0} cannot be updated via XUS as it has already been consolidated.", targetBO.HVL_UniqueReference);
				}
			}
			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		#endregion
	}
}
