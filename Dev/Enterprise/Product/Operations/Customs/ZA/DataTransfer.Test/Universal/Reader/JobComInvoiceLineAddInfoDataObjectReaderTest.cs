using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalRef = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class JobComInvoiceLineAddInfoDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestVehicleIsNotEmpty()
		{
			#region Setup
			Registry.Business.eServices.eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, true);
			var addInfos = new Dictionary<ZString, ZString>();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(MasterFiles.Business.GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var supplier = Factory.New<MasterFiles.Business.OrgHeader>();
			supplier.OH_Code = "#@923@#4";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var part = Factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "CAR1234";
			var relOrg = part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, MasterFiles.Business.OrgPartRelation.RelationshipTypes.Supplier);
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg.OU_OH;
			pivot.CI_CC = classification.PK;
			pivot.CI_Colour = "RED";
			var declarationDataObject = new UniversalRef.Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "@#1&*2343#@",
				MessageType = new UniversalRef.CodeDescriptionPair()
				{ Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[]
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23", Supplier = new UniversalRef.OrganizationAddress()
							{
								AddressType = nameof(MasterFiles.Integration.DocAddressType.Manufacturer), OrganizationCode = supplier.OH_Code, AddressShortCode = "AD1", Address1 = "AD1", City = "AA", Postcode = "1111"
							},
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[]
							{
								new UniversalCustoms.CommercialInvoiceLine()
								{
									LineNo = 1, PartNo = "CAR1234", AddInfoCollection = new List<UniversalRef.AddInfo>()
									{
										new UniversalRef.AddInfo()
											{ Key = "Colour", Value = "GREEN" }, new UniversalRef.AddInfo()
											{ Key = "EngineCapacity", Value = "1800" }, new UniversalRef.AddInfo()
											{ Key = "VehicleType", Value = "Passenger" }, new UniversalRef.AddInfo()
											{ Key = "VehicleFormat", Value = "FBU" }, new UniversalRef.AddInfo()
											{ Key = "VIN", Value = "WDX2030422R145601" }, new UniversalRef.AddInfo()
											{ Key = "ROOCert", Value = "ROO1703010755" }, new UniversalRef.AddInfo()
											{ Key = "NewUsed", Value = "N" }
									}
								}
							})))
					})
				}
			};
			Factory.SaveForTesting();
			#endregion
			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "@#1&*2343#@"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertNotNull("Excepted: invoiceline is not null", invoiceLine);
			AssertEquals(JobComInvoiceLine.Schema.JI_Colour, "GREEN", invoiceLine.JI_Colour);
			AssertEquals(JobComInvoiceLine.Schema.JI_EngineCapacity, 1800, invoiceLine.JI_EngineCapacity);
			AssertEquals(JobComInvoiceLine.Schema.JI_VehicleType, "Passenger", invoiceLine.JI_VehicleType);
			AssertEquals(JobComInvoiceLine.Schema.JI_VehicleFormat, "FBU", invoiceLine.JI_VehicleFormat);
			AssertEquals(JobComInvoiceLine.Schema.JI_VIN, "WDX2030422R145601", invoiceLine.JI_VIN);
			AssertEquals(JobComInvoiceLine.Schema.JI_ROOCert, "ROO1703010755", invoiceLine.JI_ROOCert);
			AssertEquals(JobComInvoiceLine.Schema.JI_NewUsed, "N", invoiceLine.JI_NewUsed);
		}
	}
}
