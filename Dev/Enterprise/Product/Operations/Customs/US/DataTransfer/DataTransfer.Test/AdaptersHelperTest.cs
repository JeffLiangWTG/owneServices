using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.US.DataTransfer.Test
{
	sealed class AdaptersHelperTest : TestCaseWithFactory
	{
		public void TestFindAddressByCusCode()
		{
			AssertNull(AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
			var org = Factory.New<OrgHeader>();
			var mainAddress = org.MainAddress;
			var code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code.OK_CustomsRegNo = MID;
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code.OK_OA_PremisesAddress = mainAddress.PK;
			mainAddress.CustomsCodes.Add(code);
			AssertEquals(mainAddress, AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
			var org2 = Factory.New<OrgHeader>();
			var mainAddress2 = org2.MainAddress;
			var code2 = Factory.New<OrgCusCode>();
			code2.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code2.OK_CustomsRegNo = MID;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code2.OK_OA_PremisesAddress = mainAddress2.PK;
			mainAddress2.CustomsCodes.Add(code2);
			AssertNotNull(AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
		}

		public void TestFindOrganisationByCusCode()
		{
			AssertNull(AdaptersHelper.FindOrganisationByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
			OrgHeader testOrg1 = Factory.New<OrgHeader>();
			OrgCusCode code = Factory.New<OrgCusCode>();
			code.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code.OK_CustomsRegNo = MID;
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code.OK_OA_PremisesAddress = testOrg1.MainAddress.PK;
			testOrg1.CustomsCodes.Add(code);
			AssertEquals(testOrg1, AdaptersHelper.FindOrganisationByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
			OrgHeader testOrg2 = Factory.New<OrgHeader>();
			OrgCusCode code2 = Factory.New<OrgCusCode>();
			code2.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			code2.OK_CustomsRegNo = MID;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code2.OK_OA_PremisesAddress = testOrg2.MainAddress.PK;
			testOrg2.CustomsCodes.Add(code2);
			AssertNotNull(AdaptersHelper.FindOrganisationByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, MID, Factory));
		}

		public void TestFindOrCreateOrgFromRegistrationTypeIfNeeded()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "BOB THE BUILDER";
			org.MainAddress.OA_Address1 = "MAIN ADDRESS 1";
			org.MainAddress.OA_Address2 = "MAIN ADDRESS 2";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "SECOND ADDDRES 1";
			address2.OA_Address1 = "SECOND ADDDRES 2";
			org.OH_Code = "BOB15DUMMY";
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLien = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLien.FDAs.AddNew();
			fda.US_OA_FDAFEI = address2.PK;
			var exportContext = new ValueObjectExportContext(Notify);
			exportContext.SimplifiedXML = false;
			var xmlObject = new Xsd.USFDA();
			new OGADataTransferTool().ExportOrganisation(xmlObject.EstablismentID, address2, exportContext);
			var importContext = new ValueObjectImportContext(Factory, Notify);
			fda.US_OA_FDAFEI = ZGuid.Empty;
			AdaptersHelper.FindOrCreateOrgFromRegistrationTypeIfNeeded(xmlObject.EstablismentID.Item, importContext, fda.US_OA_FDAFEIInfo, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, null);
			AssertEquals(address2.PK, fda.US_OA_FDAFEI);
			var fEI = Factory.New<OrgCusCode>();
			fEI.OK_CodeType = OrgCusCode.CodeTypes.FDAEstablishmentIdentifier;
			fEI.OK_CustomsRegNo = "001234567890";
			fEI.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			org.CustomsCodes.Add(fEI);
			AssertEquals(ZGuid.Empty, fEI.OK_OA_PremisesAddress);
			fda.US_OA_FDAFEI = ZGuid.Empty;
			AdaptersHelper.FindOrCreateOrgFromRegistrationTypeIfNeeded(xmlObject.EstablismentID.Item, importContext, fda.US_OA_FDAFEIInfo, OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, null);
			AssertEquals(address2.PK, fda.US_OA_FDAFEI);
			var fEIFromFDA = fda.FDAFEIAddress.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals(address2.PK, fEIFromFDA.OK_OA_PremisesAddress);
		}

		public void TestSetOrgDetailsToBusinessObjectOrganization()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.ConsigneeAddressOrgPK = org.PK;
			Xsd.USDeclaration xmlDeclaration = ExportToDeclaration(declaration.ConsigneeOrgAddress);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			declaration.ConsigneeAddressOrgPK = ZGuid.Empty;
			AdaptersHelper.SetOrgDetailsToBusinessObjectOrganization(xmlDeclaration.Organisations.UltimateConsignee.Item, context, declaration.ConsigneeAddressOrgPKInfo);
			AssertNotEquals(ZGuid.Empty, declaration.ConsigneeAddressOrgPK);
		}

		public void TestSetOrgAddressDetailsToBusinessObjectOrganization()
		{
			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.JE_OA_ManufacturerAddress = org.Addresses[0].PK;
			Xsd.USDeclaration xmlDeclaration = ExportToDeclaration(declaration.ManufacturerAddress.Header);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Notify);
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			AdaptersHelper.FindOrCreateOrgFromMID(xmlDeclaration.Organisations.Manufacturer.Item, context, declaration.JE_OA_ManufacturerAddressInfo);
			AssertNotEquals(ZGuid.Empty, declaration.JE_OA_ManufacturerAddress);
			OrgCusCode mID = Factory.New<OrgCusCode>();
			mID.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			mID.OK_CustomsRegNo = "ZA456GTFR";
			mID.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			org.CustomsCodes.Add(mID);
			AssertEquals(ZGuid.Empty, mID.OK_OA_PremisesAddress);
			AdaptersHelper.FindOrCreateOrgFromMID(xmlDeclaration.Organisations.Manufacturer.Item, context, declaration.JE_OA_ManufacturerAddressInfo);
			AssertNotEquals(ZGuid.Empty, declaration.JE_OA_ManufacturerAddress);
			OrgCusCode mIDFromImportedDecl = declaration.ManufacturerAddress.Header.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);
			AssertNotEquals(ZGuid.Empty, mIDFromImportedDecl.OK_OA_PremisesAddress);
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			xmlDeclaration.Organisations.Manufacturer.Item = "34WSQDRGF344WSQDRGF34";
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			AdaptersHelper.FindOrCreateOrgFromMID(xmlDeclaration.Organisations.Manufacturer.Item, context, declaration.JE_OA_ManufacturerAddressInfo);
			AssertNotEquals(ZGuid.Empty, declaration.JE_OA_ManufacturerAddress);
			AssertEquals("MID exceeds 15 characters in length and therefore will be truncated.", context.LastNotificationMessage);
			xmlDeclaration.Organisations.Manufacturer.Item = "34WSQDRGF34";
			declaration.JE_OA_ManufacturerAddress = ZGuid.Empty;
			AdaptersHelper.FindOrCreateOrgFromMID(xmlDeclaration.Organisations.Manufacturer.Item, context, declaration.JE_OA_ManufacturerAddressInfo);
			AssertNotEquals(ZGuid.Empty, declaration.JE_OA_ManufacturerAddress);
		}

		public void TestGetAddressWithSequence1()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OA_InvoicerAddress = Organization.Addresses[2].PK;
			Xsd.USDeclaration xmlDeclaration = ExportToDeclaration(declaration.InvoicerAddress.Header);
			xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Addresses[0].Sequence = 2;
			xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Addresses[1].Sequence = 3;
			xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Addresses[2].Sequence = 1;
			xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Addresses[3].Sequence = 4;
			Xsd.OrgAddress xmlAddress = AdaptersHelper.GetAddressWithSequence1(xmlDeclaration.Organisations.Invoicer.OrganisationDetails.Addresses);
			AssertEquals("Address 2", xmlAddress.AddressLine1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateOrganizations()
		{
			AssertEquals("Precondition: No Org exists", 0, Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AGRFRUGUA")).Length);
			AssertEquals("Precondition: No Org exists", 0, Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WALSTOBNT")).Length);
			AssertEquals("Precondition: No Org exists", 0, Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BANNACGUA")).Length);
			var pathToXmlDocSample = Path.Combine(baseTestFilePath + @"StandAloneInvoiceForUpdate.XML", "");
			var exampleDoc = new XmlDocument();
			try
			{
				using (StreamReader reader = new StreamReader(pathToXmlDocSample))
				{
					exampleDoc.Load(reader);
				}
			}
			catch (XmlException)
			{
				throw new AssertionFailedError("Could not load xml document. it may be malformed.");
			}

			var rdr = new XmlNodeReader(exampleDoc);
			Xsd.XmlInterchange interchange;
			var serialiser = new XmlValueObjectSerializer(typeof(Xsd.InvoiceHeader));
			var collection = Xsd.XmlInterchange.DeserializeInterchangeAndPayload(rdr, serialiser, out interchange);
			var context = new ValueObjectImportContext(Factory, interchange, Notify);
			var adapter = new USStandAloneInvoiceDataAdapter();
			var dummyCollection = new DummyBusinessObjectCollection(Factory);
			adapter.FromXmlInterchange(dummyCollection, context);
			AssertNotNull(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AGRFRUGUA")));
			AssertNotNull(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "WALSTOBNT")));
			AssertNotNull(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BANNACGUA")));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportXMLWithMIDNotFoundInEnterprise()
		{
			USCustomsDataRegistry.Instance.CreateMIDOrganizationOnUnmatchedImport.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			ZString mIDForTest = "CHCHATEC810MEY";
			DeclarationXmlDataImporter importer = new DeclarationXmlDataImporter(new USDeclarationValueObjectDataAdapter());
			AssertEquals("Precondition: Organisation with MID not found in database", null, AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, mIDForTest, Factory));
			ImportDeclarationXmlData(importer, baseTestFilePath + "USPopulatedDeclarationForMIDQueryTest.xml", Notify);
			OrgAddress addressFound = AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, mIDForTest, Factory);
			OrgHeader header = addressFound.Header;
			AssertEquals("OH_Code autogenerated", "AUTCRE", header.OH_Code);
			AssertEquals("Closest Port should be equal country code", "CH", header.OH_RL_NKClosestPort);
			AssertEquals("A MESSAGE HAS BEEN SENT TO US CUSTOMS TO REQUEST", header.MainAddress.OA_Address1);
			AssertEquals(1, header.MainAddress.CustomsCodes.Count);
			AssertEquals(mIDForTest, header.MainAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportXMLWithMIDHasInvalidCharacters()
		{
			DeclarationXmlDataImporter importer = new DeclarationXmlDataImporter(new USDeclarationValueObjectDataAdapter());
			AssertEquals("Precondition: Organisation with MID not found in database", null, AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, " ~CHCHATEC810ME", Factory));
			ImportDeclarationXmlData(importer, baseTestFilePath + "USPopulatedDeclarationForMIDWithInvalidCharacters.xml", Notify);
			AssertContains("There are invalid characters in MID  ~CHCHATEC810ME, only alphanumeric characters are allowed.", Notify.AsString.Trim());
			AssertEquals("Organisation with MID not found in database", null, AdaptersHelper.FindOrganisationAddressByCusCode(OrgCusCode.USACodeTypes.ManufacturerID, " ~CHCHATEC810ME", Factory));
		}

		void ImportDeclarationXmlData(DeclarationXmlDataImporter importer, string fileName, NotificationBuffer notify)
		{
			using (StreamReader reader = new StreamReader(fileName))
			{
				importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}

			Factory.Save();
		}

		Xsd.USDeclaration ExportToDeclaration(OrgHeader bizObj)
		{
			Xsd.USDeclaration xmlDeclaration = new Xsd.USDeclaration();
			xmlDeclaration.Organisations.Manufacturer.Item = OrganisationDataAdapter.ExportToValueObject(bizObj, new ValueObjectExportContext(Notify));
			xmlDeclaration.Organisations.Manufacturer.IsSpecified = true;
			xmlDeclaration.Organisations.Invoicer = OrganisationDataAdapter.ExportToValueObject(bizObj, new ValueObjectExportContext(Notify));
			xmlDeclaration.Organisations.Invoicer.IsSpecified = true;
			xmlDeclaration.Organisations.UltimateConsignee.Item = OrganisationDataAdapter.ExportToValueObject(bizObj, new ValueObjectExportContext(Notify));
			xmlDeclaration.Organisations.UltimateConsignee.IsSpecified = true;
			return xmlDeclaration;
		}

		NotificationBuffer notify;
		NotificationBuffer Notify => notify ?? (notify = new NotificationBuffer());

		OrgHeader organization;
		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.New<OrgHeader>();
					var address1 = organization.Addresses.AddNew();
					address1.OA_Address1 = "Address 1";
					var address2 = organization.Addresses.AddNew();
					address2.OA_Address1 = "Address 2";
					var address3 = organization.Addresses.AddNew();
					address3.OA_Address1 = "Address 3";
				}

				return organization;
			}
		}

		AdaptersHelper adaptersHelper;
		AdaptersHelper AdaptersHelper => adaptersHelper ?? (adaptersHelper = new AdaptersHelper());

		OrganisationValueObjectDataAdapter organisationDataAdapter;
		OrganisationValueObjectDataAdapter OrganisationDataAdapter => organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter());

		readonly string baseTestFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\DataTransfer\DataTransfer.Test\Testing\";

		const string MID = "34WSQDRGF34";
	}
}
