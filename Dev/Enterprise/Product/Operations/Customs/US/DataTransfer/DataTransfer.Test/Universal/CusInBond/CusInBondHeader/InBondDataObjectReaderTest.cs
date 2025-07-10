using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public abstract partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> : InBondHelperTest
		where THeader : Customs.Business.CusInBondHeader
		where TBill : Customs.Business.CusInBondBill
		where TMoveHeader : CusInBondMoveHeader
		where TMoveDetail : CusInBondMoveDetail
		where TContainer : Customs.Business.CusInBondContainer
		where TCommodity : Customs.Business.CusInBondCargoDesc
	{
		public void TestMatchingToExistingInBondHeaderSetFreightOverride()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.New<ForwardingConsol>();
			var existingHeader = newFactory.New<THeader>();
			existingHeader.BH_ParentID = consol.PK;
			existingHeader.BH_ParentTableCode = consol.TablePrefix;
			existingHeader.BH_GB = InBondBranch.PK;
			Factory.SaveForTesting();
			var existingHeaderMoveHeader = (TMoveHeader)existingHeader.MovementHeaders.AddNew();
			SetupInBondMoveHeader(existingHeaderMoveHeader);
			existingHeaderMoveHeader.InBondNumber = "IB2343";
			newFactory.Save();
			AssertEquals(false, existingHeader.BH_OverrideFreightDefaults);
			var headerDataObject = SetupInBondHeader();
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>(new[] { SetupInBondMoveHeader("IB2343", true) }));
			var reader = InBondHeaderReader(headerDataObject, logger, Factory, null);
			var headerBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertNotNull(headerBO);
			CombineAssertions(delegate
			{
				AssertEquals("Should have been matched to existingHeader", existingHeader.PK, headerBO.PK);
				AssertContents(headerBO);
				AssertEquals("headerBO.BH_OverrideFreightDefaults", true, headerBO.BH_OverrideFreightDefaults);
				var inbondMovementHeaders = GetInBondMoveHeaderCollection(headerBO);
				AssertEquals("inbondMovementHeaders.Count", 1, inbondMovementHeaders.Count);
				var inBondMoveHeaderBO = (CusInBondMoveHeader)inbondMovementHeaders[0];
				AssertEquals("inBondMoveHeaderBO should have been matched to existingHeaderMoveHeader", existingHeaderMoveHeader.PK, inBondMoveHeaderBO.PK);
				AssertEquals("inBondMoveHeaderBO.InBondNumber", "IB2343", inBondMoveHeaderBO.InBondNumber);
				AssertCusInBondMoveHeaderContents(inBondMoveHeaderBO);
				AssertMatchingToExistingInBondHeaderLogs(logger.Logs);
			});
		}

		protected abstract CusInBondHeaderDataObjectReader<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity> InBondHeaderReader(Shipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO);

		protected abstract void AssertContents(Customs.Business.CusInBondHeader headerBO);

		protected abstract Shipment SetupInBondHeader();

		protected abstract DataContextType ContextType { get; }

		protected virtual void SetupInBondMoveHeader(TMoveHeader moveHeader)
		{
		}

		protected virtual Customs.Business.CusInBondMoveHeaderCollection GetInBondMoveHeaderCollection(THeader header) => header.MovementHeaders;

		protected virtual void AssertMatchingToExistingInBondHeaderLogs(ZString actualLogs)
		{
			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusInBondHeader.
Information - Populating CusInBondHeader...
Information - Matching 'ImporterDocumentaryAddress':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Successfully loaded matching CusInBondMoveHeader.
Information - Populating CusInBondMoveHeader...
Information - Matching 'InBondCarrier':- Matched to 'CRAHOLSYD' by code, address '1804 Fudrucker Way' (only address).
Information - Matching 'TransferOfLiabilityCarrier':- Matched to 'WUFSHIJNB' by code, address 'Level 2, Building G' (only address).
Information - Updated INB0000001 from UniversalShipment.", actualLogs);
		}

		protected void SetupAddressData(IOrganizationAddressCollectionParent parentData, OrgAddress address, DocAddressType docAddressType)
		{
			if (address != null)
			{
				parentData.AddOrgAddress(WritingManager, address, docAddressType);
			}
		}

		protected void SetupAddressData(IOrganizationAddressCollectionParent parentData, OrgAddress address, ZString addressType)
		{
			if (address != null)
			{
				parentData.AddOrgAddress(WritingManager, address, addressType);
			}
		}

		protected void SetupAddressData(IOrganizationAddressCollectionParent parentData, OrgHeader org, DocAddressType docAddressType)
		{
			if (org != null)
			{
				parentData.AddOrgAddress(WritingManager, org, docAddressType);
			}
		}

		protected void SetupAddressData(IOrganizationAddressCollectionParent parentData, OrgHeader org, ZString addressType)
		{
			if (org != null)
			{
				parentData.AddOrgAddress(WritingManager, org, addressType);
			}
		}

		DataWritingManager writingManager;
		protected DataWritingManager WritingManager => writingManager ?? (writingManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>())));

		protected Shipment SetupInBondHeader(GlbBranch branch, ZString lloyds, ZString voyage, ZString transportMode, ZString containerMode, ZString vessel, ZString vesselCountryOfRegistration, ZString portOfLoading, ZString portOfDischarge)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(ContextType, null);
			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				Branch = Branch.New(branch),
				LloydsIMO = lloyds,
				VoyageFlightNo = voyage,
				TransportMode = new CodeDescriptionPair()
				{ Code = transportMode },
				CustomsContainerMode = new ContainerMode()
				{ Code = containerMode },
				VesselName = vessel,
				VesselCountryOfRegistration = new Country()
				{ Code = vesselCountryOfRegistration },
				PortOfLoading = new UNLOCO()
				{ Code = portOfLoading },
				PortOfDischarge = new UNLOCO()
				{ Code = portOfDischarge }
			};
			return result;
		}

		InBondDataObjectReaderHelper helper;
		protected InBondDataObjectReaderHelper Helper => helper ?? (helper = CreateHelper(Factory));

		protected virtual InBondDataObjectReaderHelper CreateHelper(UniversalObjectFactory factory) => new InBondDataObjectReaderHelper(factory);
	}
}
