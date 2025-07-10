using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.DataTransfer.Universal
{
	class CFSLoadListConsolDataObjectReader : ShipmentDataObjectReader<CFSLoadListConsol>
	{
		public CFSLoadListConsolDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalFreightHelper helper = null)
			: base(dataObject, logger, factory)
		{
			logger.IsUpdatingConsol = true;
			this.helper = helper ?? new UniversalCFSHelper();
		}

		readonly IUniversalFreightHelper helper;

		public override DataContextType DataContextType
		{
			get { return DataContextType.CFSLoadListConsol; }
		}

		protected override IMatchingBusinessEntityFinder<CFSLoadListConsol> GetCombinedReferenceMatcher()
		{
			var consolReferences = new CommonConsolReferences();
			consolReferences.CarriersBookingReference = dataObject.BookingConfirmationReference.GetValueOrDefault();
			consolReferences.AgentsReference = dataObject.AgentsReference.GetValueOrDefault();
			consolReferences.LoadPort = dataObject.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
			consolReferences.DischargePort = dataObject.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory);

			return new ConsolMatcher<CFSLoadListConsol>(factory.BOFactory, consolReferences, logger, helper);
		}

		protected override CFSLoadListConsol GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var query = new ZQuery(JobConsolSchema.JK_AgentsReference, dataObject.AgentsReference);
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, dataObject.WayBillNumber);
			helper.AddConsolParameters(query);

			return factory.LoadTop1<CFSLoadListConsol>(query);
		}

		protected override void PopulateBusinessObject(CFSLoadListConsol consolBO)
		{
			ISupportDataImporting supportDataImporting = consolBO;
			supportDataImporting.IsImportingData = true;

			try
			{
				PopulateBusinessObjectCore(consolBO);
			}
			finally
			{
				supportDataImporting.IsImportingData = false;
			}
		}

		void PopulateBusinessObjectCore(CFSLoadListConsol consolBO)
		{
			var linkManager = new ContainerLinkManager<CFSLoadListConsol>(consolBO);

			SetValue(consolBO, JobConsolSchema.JK_TransportMode, dataObject.TransportMode);
			SetValue(consolBO, JobConsolSchema.JK_ConsolMode, dataObject.ContainerMode);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKLoadPort, dataObject.PortOfLoading);
			SetValue(consolBO, JobConsolSchema.JK_RL_NKDischargePort, dataObject.PortOfDischarge);
			SetValue(consolBO, JobConsolSchema.JK_AgentsReference, dataObject.AgentsReference);
			SetValue(consolBO, JobConsolSchema.JK_BookingReference, dataObject.BookingConfirmationReference);
			SetValue(consolBO, JobConsolSchema.JK_MasterBillNum, dataObject.WayBillNumber);
			SetValue(consolBO, JobConsolSchema.JK_CustomsReference, dataObject.CFSReference);

			SetOrgAddresses(consolBO);
			SetTransportLegs(consolBO);
			SetContainers(consolBO, linkManager);
			SetSubShipments(consolBO, linkManager);
		}

		void SetOrgAddresses(CFSLoadListConsol consolBO)
		{
			if (dataObject.OrganizationAddressCollection != null && dataObject.OrganizationAddressCollection.Count > 0)
			{
				var orgLookup = new Dictionary<string, OrganizationAddress>();

				foreach (var orgAddress in dataObject.OrganizationAddressCollection)
				{
					if (orgLookup.ContainsKey(orgAddress.AddressType.GetValueOrDefault()))
					{
						string errorMessage = Res.GetString("19E26416-170C-4E0A-B570-5FCFE8A05C56", "Attempted to import a CFS Load List Consol with duplicate organization address: '{0}'.", orgAddress.AddressType.GetValueOrDefault());
						throw new DataObjectReadFailureException(errorMessage);
					}

					orgLookup.Add(orgAddress.AddressType.GetValueOrDefault(), orgAddress);
				}

				Action<string, string, bool> setOrg = (addressType, propertyName, isOrgHeaderColumn) =>
				{
					if (orgLookup.ContainsKey(addressType))
					{
						var orgAddress = orgLookup[addressType];
						var addressBO = new OrganisationDataObjectReader(orgAddress, logger, factory).GetMatched();

						if (addressBO != null)
						{
							consolBO[propertyName] = isOrgHeaderColumn ? addressBO.OA_OH : addressBO.PK;
						}
					}
				};

				setOrg(AddressTypes.Forwarder, CFSLoadListConsol.Schema.JK_OH_Forwarder, true);

				if (!orgLookup.ContainsKey(AddressTypes.Forwarder))
				{
					setOrg(AddressTypes.SendingForwarderAddress, CFSLoadListConsol.Schema.JK_OA_SendingForwarderAddress, false);
					setOrg(AddressTypes.ReceivingForwarderAddress, CFSLoadListConsol.Schema.JK_OA_ReceivingForwarderAddress, false);
				}

				setOrg(nameof(DocAddressType.ShippingLineAddress), CFSLoadListConsol.Schema.JK_OA_ShippingLineAddress, false);
				setOrg(AddressTypes.ContainerYardAddress, CFSLoadListConsol.Schema.JK_OA_EmptyContainerYard, false);
				setOrg(AddressTypes.CTOAddress, CFSLoadListConsol.Schema.JK_OA_CTOAddress, false);
				setOrg(nameof(DocAddressType.LocalCartageAddress1), CFSLoadListConsol.Schema.JK_OA_CartageCoAddress, false);
				setOrg(AddressTypes.DepotAddress, CFSLoadListConsol.Schema.JK_OA_DepotAddress, false);
			}
		}

		void SetTransportLegs(CFSLoadListConsol consolBO)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var reader = new TransportLegCollectionReader<Transport>(dataObject.TransportLegCollection, logger, factory, consolBO);
				reader.ReadIntoCollection();
			}
		}

		void SetContainers(CFSLoadListConsol consolBO, IContainerLinkManager<CFSLoadListConsol> linkManager)
		{
			var consolContainerCollectionReader = new ConsolContainerCollectionReader<CFSContainer, CFSLoadListConsol>(dataObject.ContainerCollection, logger, factory, consolBO, linkManager);
			consolContainerCollectionReader.ReadIntoCollection();

			EnforceCFSRegisteredOnContainers(consolBO.Containers);
		}

		void EnforceCFSRegisteredOnContainers(CFSContainerCollection containers)
		{
			var cfsContainers = containers.Cast<CFSContainer>().ToArray();
			foreach (var container in cfsContainers)
			{
				SetValue(container, JobContainerSchema.JC_IsCFSRegistered, true);
			}
		}

		void SetSubShipments(CFSLoadListConsol consolBO, IContainerLinkManager<CFSLoadListConsol> linkManager)
		{
			if (dataObject.SubShipmentCollection != null)
			{
				foreach (var shipment in dataObject.SubShipmentCollection)
				{
					new CFSShipmentDataObjectReader(shipment, logger, factory, ChildShipmentsParent.FromLoadList(consolBO), linkManager, helper).ReadIntoBusinessObject();
				}
			}
		}
	}
}
