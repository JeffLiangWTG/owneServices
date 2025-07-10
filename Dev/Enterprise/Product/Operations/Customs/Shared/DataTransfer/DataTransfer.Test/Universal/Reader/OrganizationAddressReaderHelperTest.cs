using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	class OrganizationAddressReaderHelperTest : OrganizationAddressTestHelper
	{
		public void TestLoadAQISProcessingOrgAddress()
		{
			#region GetShipmentForTest

			var xmlAddress1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.AQISProcessingEstablishment), AddressShortCode = "TOOLATE", AddressOverride = true, OrganizationCode = "TOOLATE", Address1 = "Unit 24, Level 10", Country = new Country() { Code = "ER" } };
			var organizationAddresses = new List<OrganizationAddress>() { xmlAddress1 };

			var addInfoGroupObject1 = new List<UniversalCustoms.AddInfoGroup>() { new UniversalCustoms.AddInfoGroup() { Type = new CodeDescriptionPair() { Code = "QH" }, OrganizationAddressCollection = organizationAddresses } };
			var commercialInvoiceCollection1 = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>() { new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) { AddInfoGroupCollection = addInfoGroupObject1 } };

			#endregion

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MYMASTER1";
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declaration.JE_DeclarationReference = "B00001099";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.SaveForTesting();

			var declarationUxml = CreateUniversalShipment(null, "MYMASTER1", new WayBillType() { Code = "MWB", Description = "Master Waybill" });
			declarationUxml.DataContext.DataTargetCollection.First().Key = "B00001099";
			declarationUxml.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialInvoiceCollection = commercialInvoiceCollection1,
			};

			var reader = new JobDeclarationDataObjectReader(declarationUxml, logger, Factory);
			var loadedDeclaration = reader.ReadIntoBusinessObject();

			AssertSame(declaration, loadedDeclaration);

			var helper = new OrganizationAddressReaderHelper<BaseJobDeclaration>(logger, Factory, declaration, DocAddressType.AQISProcessingEstablishment);

			// no addresses exist, creating new address
			var loadedAddressPK1 = helper.GetOrCreateDocAddress(organizationAddresses, declaration.DocAddresses.OfType<JobDocAddress>(), FillAddAddress, false);
			var loadedAddress1 = Factory.Load<JobDocAddress>(loadedAddressPK1);
			AssertEquals("Unit 24, Level 10", loadedAddress1.Address1);
			declaration.DocAddresses.Add(loadedAddress1);

			// address from XML matches existing address
			var loadedAddressPK2 = helper.GetOrCreateDocAddress(organizationAddresses, declaration.DocAddresses.OfType<JobDocAddress>(), FillAddAddress, false);
			AssertEquals("Declaration has AQISProcessing address, return Existed AQISProcessing Address", loadedAddressPK1, loadedAddressPK2);

			// address from XML does not match any existing address
			loadedAddress1.Address1 = "addresss that does not match XML";
			var loadedAddressPK3 = helper.GetOrCreateDocAddress(organizationAddresses, declaration.DocAddresses.OfType<JobDocAddress>(), FillAddAddress, false);
			var loadedAddress3 = Factory.Load<JobDocAddress>(loadedAddressPK3);
			AssertNotEquals("new address was created", loadedAddressPK1, loadedAddressPK3);
			AssertEquals("Unit 24, Level 10", loadedAddress3.Address1);
			AssertEquals("addresss that does not match XML", loadedAddress1.Address1);

			// address from XML match existing address in XML before
			declaration.DocAddresses.Add(loadedAddress3);
			xmlAddress1.AddressOverride = null;
			var loadedAddressPK4 = helper.GetOrCreateDocAddress(organizationAddresses, declaration.DocAddresses.OfType<JobDocAddress>(), FillAddAddress, false);
			AssertEquals("Address should match the same address from xml", loadedAddress3.PK, loadedAddressPK4);
		}

		JobDocAddress FillAddAddress(BaseJobDeclaration declaration, DocAddressType docAddressType, OrganizationAddress orgAddressDataObject)
		{
			var nextSeqNum = (byte)declaration.DocAddresses.Cast<JobDocAddress>().Count(a => a.DocAddressType == docAddressType);

			var newAddress = Factory.New<JobDocAddress>();
			var typeCode = DocAddressTypes.GetCode(Factory.BOFactory, docAddressType);

			newAddress.E2_ParentID = declaration.PK;
			newAddress.E2_ParentTableCode = "JE";
			newAddress.E2_CompanyName = orgAddressDataObject.CompanyName ?? string.Empty;
			newAddress.E2_AddressType = typeCode;
			newAddress.E2_AddressSequence = nextSeqNum;
			newAddress.E2_AddressOverride = true;
			newAddress.Address1 = orgAddressDataObject.Address1 ?? string.Empty;

			return newAddress;
		}

		protected UniversalShipment CreateUniversalShipment(ZString? ownerRef, ZString? wayBillNumber, WayBillType wayBillType, List<AddInfo> addInfos = null, DataContextType dataContextType = DataContextType.CustomsDeclaration)
		{
			return SetupDeclaration(new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import, Description = JobMessageTypeList.Descriptions.Import },
				new CodeDescriptionPair() { Code = "ST1", Description = "STANDARD" },
				new ContainerMode() { Code = Core.Constants.ContainerModes.Containerised, Description = "Containerized" },
				"RAT HATS",
				new UNLOCO() { Code = "NZDUD", Name = "Dunedin" },
				new UNLOCO() { Code = "NZCHC", Name = "Christchurch" },
				new UNLOCO() { Code = "AUNTL", Name = "Newcastle" },
				new UNLOCO() { Code = "AUSYD", Name = "Sydney" },
				new UNLOCO() { Code = "AUBDG", Name = "Bendigo" },
				"BUNGA DELIMA",
				"343L",
				0,
				45,
				new PackageType() { Code = "KEG", Description = "Keg" },
				23.45m,
				new UnitOfVolume() { Code = "CF", Description = "Cubic Feet" },
				34.56m,
				new UnitOfWeight() { Code = "KT", Description = "Kilotons" },
				new CodeDescriptionPair() { Code = "SEA", Description = "Sea Freight" },
				ownerRef,
				wayBillNumber,
				wayBillType,
				addInfos,
				new ServiceLevel() { Code = ServiceLevel1.RS_Code, Description = ServiceLevel1.RS_Description },
				ZBool.True, new CodeDescriptionPair() { Code = "EFT", Description = "EFT MODE" }, new CodeDescriptionPair() { Code = OrgConstants.MergeInvoiceLines.Tariff, Description = "Tariff" }, 112,
				"7819369", new CodeDescriptionPair() { Code = "SP", Description = "STANDARD P" }, "AGREF123", "F234",
				new CodeDescriptionPair() { Code = PaymentPartyCodeDescriptionList.Codes.Broker, Description = PaymentPartyCodeDescriptionList.Descriptions.Broker },
				789.012m, dataContextType, "INCO PLACE", new CodeDescriptionPair() { Code = "IFD" });
		}

		public RefServiceLevel ServiceLevel1
		{
			get
			{
				if (serviceLevel1 == null)
				{
					serviceLevel1 = Factory.New<RefServiceLevel>();
					serviceLevel1.RS_Code = "Z!2";
					serviceLevel1.RS_Description = "DUMMY SERVICE";
				}
				return serviceLevel1;
			}
		}
		RefServiceLevel serviceLevel1;

		protected UniversalShipment SetupDeclaration(CodeDescriptionPair messageType, CodeDescriptionPair messageSubType, ContainerMode customsContainerMode, ZString? goodsDescription, UNLOCO portOfOrigin, UNLOCO portOfLoading, UNLOCO portOfFirstArrival, UNLOCO portOfDischarge, UNLOCO portOfDestination, ZString? vesselName, ZString? voyageFlightNo, ZInt? containerCount, ZInt? outerPack, PackageType outerPackType, ZDecimal? totalVolume, UnitOfVolume totalVolumeUnit, ZDecimal? totalWeight, UnitOfWeight totalWeightUnit, CodeDescriptionPair transportMode, ZString? ownerRef, ZString? wayBillNumber, WayBillType wayBillType, List<AddInfo> addInfoCollection,
			ServiceLevel serviceLevel, ZBool? isPersonalEffects, CodeDescriptionPair eftMode, CodeDescriptionPair mergeBy, ZInt? totalNoOfPieces,
			ZString? lloydsIMO, CodeDescriptionPair exportGoodsType, ZString? agentsReference, ZString? folio, CodeDescriptionPair paymentMethod, ZDecimal? totalNoOfPacksDecimal,
			DataContextType dataContextType = DataContextType.CustomsDeclaration, ZString? shipmentIncoTermPlace = null, CodeDescriptionPair declarantType = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(CurrentCompany);
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(dataContextType, null);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = messageType,
				MessageSubType = messageSubType,
				CustomsContainerMode = customsContainerMode,
				GoodsDescription = goodsDescription,
				PortOfOrigin = portOfOrigin,
				PortOfLoading = portOfLoading,
				PortOfFirstArrival = portOfFirstArrival,
				PortOfDischarge = portOfDischarge,
				PortOfDestination = portOfDestination,
				VesselName = vesselName,
				VoyageFlightNo = voyageFlightNo,
				ContainerCount = containerCount,
				OuterPacks = outerPack,
				OuterPacksPackageType = outerPackType,
				TotalVolume = totalVolume,
				TotalVolumeUnit = totalVolumeUnit,
				TotalWeight = totalWeight,
				TotalWeightUnit = totalWeightUnit,
				TransportMode = transportMode,
				OwnerRef = ownerRef,
				WayBillNumber = wayBillNumber,
				WayBillType = wayBillType,
				ServiceLevel = serviceLevel,
				IsPersonalEffects = isPersonalEffects,
				EFTMode = eftMode,
				MergeBy = mergeBy,
				TotalNoOfPieces = totalNoOfPieces,
				LloydsIMO = lloydsIMO,
				ExportGoodsType = exportGoodsType,
				AgentsReference = agentsReference,
				Folio = folio,
				PaymentMethod = paymentMethod,
				AdditionalTerms = shipmentIncoTermPlace,
				TotalNoOfPacksDecimal = totalNoOfPacksDecimal,
				DeclarantType = declarantType
			};
			shipment.SetAddInfoCollection(() => addInfoCollection);
			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		protected TestErrorLogger logger;

		public GlbCompany CurrentCompany
		{
			get { return currentCompany ?? (currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK)); }
		}
		GlbCompany currentCompany;
	}
}
