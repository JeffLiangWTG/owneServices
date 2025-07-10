using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using Constants = Enterprise.Customs.US.ISF.Business.ISFConstants;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using USPackingingUnitList = Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader.Testing
{
	sealed class ISFDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestSkipDeleteProductLinesWhenFillISFLines()
		{
			var header = Factory.New<CusISFHeader>();
			header.SuspendDeleteProductLines = true;
			for (int i = 0; i < 30; i++)
			{
				var line1 = header.Lines.AddNew();
				var line2 = header.Lines.AddNew();
				var line3 = header.Lines.AddNew();
				var line4 = header.Lines.AddNew();
				var line5 = header.Lines.AddNew();

				line2.BL_BL_Parent = line1.PK;
				line3.BL_BL_Parent = line1.PK;
				line4.BL_BL_Parent = line1.PK;
				line5.BL_BL_Parent = line1.PK;
				line2.BL_LineType = "REL";
				line3.BL_LineType = "REL";
				line4.BL_LineType = "REL";
				line5.BL_LineType = "REL";
			}
			Factory.SaveForTesting();

			var lines = new List<CusISFLine>(Factory.Load<CusISFLine>(new ZQuery(CusISFLineSchema.BL_BF, header.GetValue(CusISFHeaderSchema.PK))));
			lines.DeleteAll();
			Factory.SaveForTesting();
			AssertEquals(0, header.Lines.Count);
		}

		[ExpectNoExceptions]
		public void TestDontExplodeWhenTotalWeightisNull()
		{
			var uShipment = SetupISFHeader("OWNER REF", "XJ5-23534542100", "HB11223344", BillTypeList.Codes.HouseBillOfLading);
			uShipment.TotalWeight = null;
			var reader = new ISFHeaderDataObjectReader(uShipment, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
		}

		public void TestImportingISFHeaderData()
		{
			var headerDataObject = SetupISFHeader(SubmissionTypeList.Codes.ISF10, ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, TransportModeCodes.Codes.OceanVesselContainerized, "TEST OWNER REF", ActionReasonCodeList.Codes.CompliantTransaction,
									"TEST CUSTOMS REF", "ABC-12345678901", "BD323423", "MWB012345", BillTypeList.Codes.OceanBillOfLading);
			var headerBO = Factory.New<CusISFHeader>();
			Factory.SaveForTesting();

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);

			#region Check Contents of header Business Object

			CombineAssertions(delegate
			{
				AssertEquals("headerBO.BF_EntryType", SubmissionTypeList.Codes.ISF10, headerBO.BF_EntryType);
				AssertEquals("headerBO.BF_ShipmentType", ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, headerBO.BF_ShipmentType);
				AssertEquals("headerBO.BF_TransportMode", TransportModeCodes.Codes.OceanVesselContainerized, headerBO.BF_TransportMode);
				AssertEquals("headerBO.BF_OwnerReference", "TEST OWNER REF", headerBO.BF_OwnerReference);
				AssertEquals("headerBO.BF_ActionReasonCode", ActionReasonCodeList.Codes.CompliantTransaction, headerBO.BF_ActionReasonCode);
				AssertEquals("headerBO.BF_RL_NKPortOfUnload", "CNSHA", headerBO.BF_RL_NKPortOfUnload);
				AssertEquals("headerBO.BF_RL_NKPlaceOfDelivery", "USCHI", headerBO.BF_RL_NKPlaceOfDelivery);
				AssertEquals("headerBO.BF_EntryNumber", "ABC-12345678901", headerBO.BF_EntryNumber);
				AssertEquals("headerBO.BF_BondReferenceNumber", "BD323423", headerBO.BF_BondReferenceNumber);
				AssertEquals("headerBO.BF_OceanBill", "MWB012345", headerBO.BF_OceanBill);
				AssertEquals("headerBO.CustomAttribute1", "ATTRIB ONE", headerBO.CustomAttribute1);
				AssertEquals("headerBO.CustomAttribute2", "ATTRIB TWO", headerBO.CustomAttribute2);
				AssertEquals("headerBO.BF_ShipmentSubType", ShipmentSubTypeList.Codes.LowValueEntriesShipments, headerBO.BF_ShipmentSubType);
				AssertEquals("headerBO.BF_EstimatedValue", 1500m, headerBO.BF_EstimatedValue);
				AssertEquals("headerBO.BF_EstimatedQuantity", 1000, headerBO.BF_EstimatedQuantity);
				AssertEquals("headerBO.BF_EstimatedQuantityUQ", USPackingingUnitList.ShippingOrPackingingUnitList.Codes.Bag, headerBO.BF_EstimatedQuantityUQ);
				AssertEquals("headerBO.BF_EstimatedWeight", 1100, headerBO.BF_EstimatedWeight);
				AssertEquals("headerBO.BF_EstimatedWeightUQ", Core.Constants.Weight.Kilograms, headerBO.BF_EstimatedWeightUQ);
			});

			#endregion
		}

		public void TestISFHeaderImportCustomizedFields()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534333666", "HB1233366", BillTypeList.Codes.HouseBillOfLading);
			AssertISFHeaderImportCustomizedFields(headerDataObject, "XJ5-23534333666", @"CustomAttribute1 - ZString - ATTRIB ONE
CustomAttribute2 - ZString - ATTRIB TWO
TESTA - ZString - Aha");
		}

		public void TestISFHeaderImportCustomizedFields_OldFormatTakePrecedent()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534333666", "HB1233366", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute1, Value = "ATTRIB ONE B", DataType = DataType.String });
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute2, Value = "ATTRIB TWO B", DataType = DataType.String });
			AssertISFHeaderImportCustomizedFields(headerDataObject, "XJ5-23534333666", @"CustomAttribute1 - ZString - ATTRIB ONE
CustomAttribute2 - ZString - ATTRIB TWO
TESTA - ZString - Aha");
		}

		public void TestISFHeaderImportCustomizedFields_MatchFieldName()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534333666", "HB1233366", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>()
				{
					new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute1, Value = "ATTRIB ONE" , DataType = DataType.String },
					new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute2, Value = "ATTRIB TWO" , DataType = DataType.String },
					new CustomizedField() { Key = "TESTA", Value = "Aha", DataType = DataType.String },
				});
			AssertISFHeaderImportCustomizedFields(headerDataObject, "XJ5-23534333666", @"CustomAttribute1 - ZString - ATTRIB ONE
CustomAttribute2 - ZString - ATTRIB TWO
TESTA - ZString - Aha");
		}

		public void TestISFHeaderImportCustomizedFields_DifferentDataType()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534333666", "HB1233366", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = Constants.CustomizedFieldConstants.CustomAttribOne, Value = "1234", DataType = DataType.Integer });
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = Constants.CustomizedFieldConstants.CustomAttribTwo, Value = "5678", DataType = DataType.Integer });
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute1, Value = "123.456", DataType = DataType.Decimal });
			headerDataObject.CustomizedFieldCollection.Add(new CustomizedField() { Key = CusISFHeader.Schema.CustomAttribute2, Value = "456.789", DataType = DataType.Decimal });
			AssertISFHeaderImportCustomizedFields(headerDataObject, "XJ5-23534333666", @"
CustomAttrib1 - ZInt - 1234
CustomAttrib2 - ZInt - 5678
CustomAttribute1 - ZDecimal - 123.456
CustomAttribute2 - ZDecimal - 456.789
TESTA - ZString - Aha");
		}

		void AssertISFHeaderImportCustomizedFields(Shipment headerDataObject, ZString customsReference, string expectResult)
		{
			var header0 = Factory.New<CusISFHeader>();
			header0.BF_CustomsReference = customsReference;

			Factory.SaveForTesting();
			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(header0, headerBO);
			AssertEquals(header0.CustomAttribute1, headerBO.CustomAttribute1);
			AssertEquals(header0.CustomAttribute2, headerBO.CustomAttribute2);

			CombineAssertions(delegate
			{
				var customFields = headerBO.GetUserDefinedValues();
				var result = "";
				foreach (var customField in customFields)
				{
					result += customField.PropertyName + " - " + customField.Value + "\r\n";
				}

				AssertMultilineASCIIEquals("All Custom Fields should have been imported with none extra", expectResult, string.Join("\r\n", customFields.Select(x => $"{x.PropertyName} - {x.Value.GetType().Name} - {x.Value}")));

				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		public void TestFindMatchingISFHeader()
		{
			var header0 = Factory.New<CusISFHeader>();
			header0.BF_CustomsReference = "XJ5-23534542100";
			Factory.SaveForTesting();
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", "HB1233333", BillTypeList.Codes.HouseBillOfLading);
			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(header0, headerBO);

			var header1 = Factory.New<CusISFHeader>();
			header1.BF_CustomsReference = "XJ5-23534542100";
			Factory.SaveForTesting();
			reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple ISF Header matched.", headerBO);
			Assert("Multiple ISF Header matched.", logger.Logs.Contains(ISFHeaderDataObjectReader.MultipleISFMatchingTransactionNumber));

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";

			header0.BF_CustomsReference = ZString.Empty;
			header0.BF_OwnerReference = "OWNER REF";
			header0.BF_HouseBill = "HB1233333";
			header0.BF_MasterBill = "MB582123";
			header0.BF_OH_Importer = orgHeader.PK;

			header1.BF_CustomsReference = ZString.Empty;
			header1.BF_OwnerReference = "OWNER REF 1";
			header1.BF_HouseBill = "HB1233333";
			header1.BF_MasterBill = "MB5811103";
			header1.BF_OH_Importer = orgHeader.PK;

			Factory.SaveForTesting();

			headerDataObject = SetupISFHeader("OWNER REF", ZString.Empty, "HB1233333", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.SetEntryNumberCollection(() => null);
			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header0));
			headerDataObject.AddOrgAddress(writingManager, orgHeader, DocAddressType.ImporterDocumentaryAddress);
			reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple ISF Header matched.", headerBO);
			Assert("Multiple ISF Header matched.", logger.Logs.Contains(ISFHeaderDataObjectReader.MultipleISFMatchingEitherHouseBillOrOreanBill));

			header1.BF_HouseBill = "HB12338945";
			Factory.SaveForTesting();
			reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(header0, headerBO);

			var headerDataObject1 = SetupISFHeader("OWNER REF", ZString.Empty, ZString.Empty, ZString.Empty);
			headerDataObject1.AddOrgAddress(writingManager, orgHeader, DocAddressType.ImporterDocumentaryAddress);
			reader = new ISFHeaderDataObjectReader(headerDataObject1, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(header0, headerBO);

			header0.BF_OwnerReference = "OWNER REF";
			header0.BF_SystemCreateTimeUtc = ZDateTime.Today;
			header1.BF_OwnerReference = "OWNER REF";
			Factory.SaveForTesting();
			reader = new ISFHeaderDataObjectReader(headerDataObject1, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertNull("Multiple ISF Header matched.", headerBO);
			Assert("Multiple ISF Header matched.", logger.Logs.Contains(ISFHeaderDataObjectReader.MultipleISFMatchingOwnerReference));
		}

		public void TestImportReferenceDatas()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", ZString.Empty, BillTypeList.Codes.HouseBillOfLading);
			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJ5-23534542100";
			header.BF_HouseBill = "HB11223344";
			header.BF_MasterBill = "MB223445566";
			header.BF_OceanBill = "OB9821273";
			Factory.SaveForTesting();

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals("House bill number should be empty.", ZString.Empty, headerBO.BF_HouseBill);
		}

		public void TestImportOrganizations()
		{
			#region Sepup Organizations

			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJ5-23534542100";

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "TSTIMPORT";
			header.BF_OH_Importer = importerOrg.PK;

			var buyingPartyOrg = Factory.New<OrgHeader>();
			buyingPartyOrg.OH_Code = "BUYPTYORG";
			header.BuyingParty.OrganisationPK = buyingPartyOrg.PK;
			header.BuyingParty.E2_Contact = "BUYING PARTY ADDRESS";

			var stuffingLocationOrg = Factory.New<OrgHeader>();
			stuffingLocationOrg.OH_Code = "STUFFORG";
			header.StuffingLocation.OrganisationPK = stuffingLocationOrg.PK;

			var shipToPartyOrg = Factory.New<OrgHeader>();
			shipToPartyOrg.OH_Code = "TESTSHPORG";
			header.MainShipToParty.E2_OA_Address = shipToPartyOrg.MainAddress.PK;
			header.MainShipToParty.E2_AddressOverride = true;
			header.MainShipToParty.E2_CompanyName = "TEST SHP COM.";

			var manufacturerOrg = Factory.New<OrgHeader>();
			manufacturerOrg.OH_Code = "TESTMANORG";
			manufacturerOrg.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS";
			var manufacturerAddress = header.ManufacturerAddresses.AddNew();
			manufacturerAddress.E2_OA_Address = manufacturerOrg.MainAddress.PK;

			Factory.SaveForTesting();

			#endregion

			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", ZString.Empty, BillTypeList.Codes.HouseBillOfLading);
			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header));
			var orgAddressWriter = new JobDocAddressDataObjectWriter(writingManager);
			headerDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			foreach (ISFDocAddress docAddress in header.DocAddresses)
			{
				var addressDataObject = orgAddressWriter.GetDataObject(docAddress);
				if (addressDataObject != null)
				{
					headerDataObject.OrganizationAddressCollection.Add(addressDataObject);
				}
			}
			headerDataObject.AddOrgAddress(writingManager, header.Importer, DocAddressType.ImporterDocumentaryAddress);

			var newImporterOrg = Factory.New<OrgHeader>();
			newImporterOrg.OH_Code = "NEWIMPORTER";
			header.BF_OH_Importer = newImporterOrg.PK;

			var newShipToPartyOrg = Factory.New<OrgHeader>();
			newShipToPartyOrg.OH_Code = "NEWSHPPTY";
			header.MainShipToParty.E2_AddressOverride = false;
			header.ExtraShipToPartyAddresses.DeleteAll();

			Factory.SaveForTesting();

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals("TSTIMPORT", headerBO.Importer.OH_Code);
			AssertEquals("BUYING PARTY ADDRESS", headerBO.BuyingParty.E2_Contact);
			Assert(headerBO.MainShipToParty.E2_AddressOverride);
			AssertEquals("TEST SHP COM.", headerBO.MainShipToParty.E2_CompanyName);
			AssertEquals(1, headerBO.ManufacturerAddresses.Count);
		}

		public void TestImportWaybillTypeIfNotExistsInAdditionalBillCollection()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", "HB11223344", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.SetAdditionalBillCollection(() => null);

			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJ5-23534542100";
			Factory.SaveForTesting();

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals("House bill number shouldn't be empty.", "HB11223344", headerBO.BF_HouseBill);
		}

		public void TestRemoveUnRelatedBills()
		{
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", "HB11223344", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.SetAdditionalBillCollection(() => null);

			var header = Factory.New<CusISFHeader>();
			header.BF_CustomsReference = "XJ5-23534542100";
			var referenceBill1 = header.ReferenceDatas.AddNew();
			referenceBill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			referenceBill1.BB_BillNum = "MB51132514";
			referenceBill1.BB_CustomsStatus = DispositionCodeList.Codes.S1;
			var referenceBill2 = header.ReferenceDatas.AddNew();
			referenceBill2.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			referenceBill2.BB_BillNum = "HB51348789";
			referenceBill2.BB_FirstMatchedDate = ZDateTime.Today;
			var referenceBill3 = header.ReferenceDatas.AddNew();
			referenceBill3.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			referenceBill3.BB_BillNum = "MB512351384";
			referenceBill3.BB_MatchDate = ZDateTime.Today;
			var referenceBill4 = header.ReferenceDatas.AddNew();
			referenceBill4.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			referenceBill4.BB_BillNum = "HB5115845987";
			var referenceBill5 = header.ReferenceDatas.AddNew();
			referenceBill5.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			referenceBill5.BB_BillNum = "MB815658123";
			Factory.SaveForTesting();

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertNotNull(headerBO.ReferenceDatas.FirstOrDefault(x => x.BB_BillType == BillTypeList.Codes.OceanBillOfLading && x.BB_BillNum == "MB51132514"));
			AssertNotNull(headerBO.ReferenceDatas.FirstOrDefault(x => x.BB_BillType == BillTypeList.Codes.HouseBillOfLading && x.BB_BillNum == "HB51348789"));
			AssertNotNull(headerBO.ReferenceDatas.FirstOrDefault(x => x.BB_BillType == BillTypeList.Codes.OceanBillOfLading && x.BB_BillNum == "MB512351384"));
			AssertNotNull(headerBO.ReferenceDatas.FirstOrDefault(x => x.BB_BillType == BillTypeList.Codes.OceanBillOfLading && x.BB_BillNum == "MB815658123"));
			AssertNull(headerBO.ReferenceDatas.FirstOrDefault(x => x.BB_BillType == BillTypeList.Codes.HouseBillOfLading && x.BB_BillNum == "HB5115845987"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateExistingISF_CS00451065()
		{
			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "AMYUSTNYC";
			importerOrg.OH_FullName = "AMY US TEST";
			importerOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var patternMatch = Factory.New<OrgPatternMatch>();
			patternMatch.OS_OH = importerOrg.PK;
			patternMatch.OS_OA = importerOrg.MainAddress.PK;
			patternMatch.OS_FullCompanyName = "AMY US TEST";
			patternMatch.OS_UNLOCO = Core.Constants.CountryCodes.UnitedStates;
			patternMatch.OS_CompanyName1 = "A500";
			patternMatch.OS_IsCorporation = false;

			Factory.SaveForTesting();

			var matchedISF = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("No matched ISF", 0, matchedISF.Length);

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseTestFilePath + "CS00451065Sample.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			matchedISF = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("1 ISF created", 1, matchedISF.Length);
			AssertEquals("HNLTQD16A03539", matchedISF[0].BF_HouseBill);
			AssertEquals("EGLV140600905468", matchedISF[0].BF_MasterBill);

			message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseTestFilePath + "CS00451065Sample.xml"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			matchedISF = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("No more ISF created", 1, matchedISF.Length);
			AssertEquals("HNLTQD16A03539", matchedISF[0].BF_HouseBill);
			AssertEquals("EGLV140600905468", matchedISF[0].BF_MasterBill);
		}

		public void TestTransportModeNotChangedIfNotFound()
		{
			var header0 = Factory.New<CusISFHeader>();
			header0.BF_CustomsReference = "XJ5-23534542100";
			header0.BF_TransportMode = TransportModeCodes.Codes.OceanVesselNonContainerized;
			Factory.SaveForTesting();
			var headerDataObject = SetupISFHeader("OWNER REF", "XJ5-23534542100", "HB1233333", BillTypeList.Codes.HouseBillOfLading);
			headerDataObject.CustomsContainerMode = null;
			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(header0, headerBO);
			AssertEquals(TransportModeCodes.Codes.OceanVesselNonContainerized, headerBO.BF_TransportMode);

			headerDataObject.CustomsContainerMode = new ContainerMode() { Code = ContainerModeList.Codes.Containerized };
			reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			headerBO = reader.ReadIntoBusinessObject();
			AssertEquals(header0, headerBO);
			AssertEquals(TransportModeCodes.Codes.OceanVesselContainerized, headerBO.BF_TransportMode);
		}

		public void TestImportingISFLineData()
		{
			var headerBO = Factory.New<CusISFHeader>();
			var lineDataObject = SetupISFLine("TEST PRODUCT", Core.Constants.CountryCodes.China, "9201.10.00");
			var helper = new ISFDataObjectHelper(headerBO);
			var reader = new ISFLineDataObjectReader(lineDataObject, logger, Factory, headerBO);
			var lineBO = reader.ReadIntoBusinessObject();
			AssertNotNull(lineBO);
			CombineAssertions(delegate
			{
				AssertEquals("lineBO.BL_TextProductCode", "TEST PRODUCT", lineBO.BL_TextProductCode);
				AssertEquals("lineBO.BL_RN_NKGoodsOrigin", Core.Constants.CountryCodes.China, lineBO.BL_RN_NKGoodsOrigin);
				AssertEquals("lineBO.BL_FormattedHarmonisedNum", "9201.10.00", lineBO.BL_FormattedHarmonisedNum);
			});
		}

		public void TestImportManufacturer()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var header = Factory.New<CusISFHeader>();
			var manufacturerOrg = Factory.New<OrgHeader>();
			manufacturerOrg.OH_Code = "TESTMANU";
			manufacturerOrg.OH_FullName = "TEST MANUFACTURER";
			manufacturerOrg.MainAddress.OA_Address1 = "TEST ADDRESS 1";
			var manufacturer = header.ManufacturerAddresses.AddNew();
			manufacturer.E2_OA_Address = manufacturerOrg.MainAddress.PK;
			manufacturer.E2_Contact = "TEST CONTACT";
			manufacturer.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.SaveForTesting();
			var helper = new ISFDataObjectHelper(header);
			var lineDataObject = SetupISFLine("TEST PRODUCT", Core.Constants.CountryCodes.China, "9201.10.00");
			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header));
			var orgAddressWriter = new JobDocAddressDataObjectWriter(writingManager);
			lineDataObject.OrganizationAddressCollection = new List<OrganizationAddress>();
			var organizationDataObject = orgAddressWriter.GetDataObject(manufacturer);
			lineDataObject.OrganizationAddressCollection.Add(organizationDataObject);
			var headerReader = new ISFHeaderDataObjectReader(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), logger, Factory);
			var reader = new ISFLineDataObjectReader(lineDataObject, logger, Factory, header, headerReader);
			var lineBO = reader.ReadIntoBusinessObject();
			AssertNotNull(lineBO);
			AssertEquals(manufacturer, lineBO.ManufacturerDocAddress);
			AssertEquals(manufacturerOrg.MainAddress.PK, lineBO.ManufacturerDocAddress.E2_OA_Address);
			manufacturer.E2_AddressOverride = true;
			manufacturer.E2_CompanyName = "TEST COMPANY";
			manufacturer.E2_Address1 = "TEST ADDRESS LINE 1";
			manufacturer.E2_Address2 = "TEST ADDRESS LINE 2";
			manufacturer.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			manufacturer.E2_State = "31";
			manufacturer.E2_City = "SHANGHAI";
			manufacturer.E2_Postcode = "200000";
			manufacturer.E2_GovRegNumType = string.Empty;
			manufacturer.E2_GovRegNum = string.Empty;
			manufacturer.E2_Mobile = "13812345678";
			manufacturer.E2_Phone = "(021)87512354";
			manufacturer.E2_Fax = "(021)87512354";
			manufacturer.E2_Email = "INC@TEST.COM";
			Factory.SaveForTesting();
			lineDataObject.OrganizationAddressCollection = new List<OrganizationAddress>();
			organizationDataObject = orgAddressWriter.GetDataObject(manufacturer);
			lineDataObject.OrganizationAddressCollection.Add(organizationDataObject);
			reader = new ISFLineDataObjectReader(lineDataObject, logger, Factory, header, headerReader);
			lineBO = reader.ReadIntoBusinessObject();
			AssertNotNull(lineBO);
			AssertEquals(manufacturer, lineBO.ManufacturerDocAddress);
			AssertNotEquals(manufacturerOrg.MainAddress.PK, lineBO.ManufacturerDocAddress.E2_OA_Address);
			AssertEquals(Core.Constants.CountryCodes.China, lineBO.BL_RN_NKGoodsOrigin);
			manufacturer.E2_GovRegNumType = "DN4";
			manufacturer.E2_GovRegNum = "542135869";
			Factory.SaveForTesting();
			reader = new ISFLineDataObjectReader(lineDataObject, logger, Factory, header, headerReader);
			lineBO = reader.ReadIntoBusinessObject();
			AssertNotNull(lineBO);
		}

		public void TestImportManufacturerExceedsMaximum()
		{
			var headerDataObject = SetupISFHeader(SubmissionTypeList.Codes.ISF10, ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, TransportModeCodes.Codes.OceanVesselContainerized, "TEST OWNER REF", ActionReasonCodeList.Codes.CompliantTransaction,
				"TEST CUSTOMS REF", "ABC-12345678901", "BD323423", "MWB012345", BillTypeList.Codes.OceanBillOfLading);
			Factory.New<CusISFHeader>();
			Factory.SaveForTesting();

			var invoiceLineCollection = new DataObjectList<CommercialInvoiceLine>();
			headerDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => invoiceLineCollection))
				})
			};

			for (int i = 0; i < 1000; i++)
			{
				var lineDataObject = SetupISFLine("TEST PRODUCT", Core.Constants.CountryCodes.China, "9201.10.00");
				PopulateManufacturer(lineDataObject, $"Test Company {i}", "US", (211000 + i).ToString(), "CHI", $"GODOWN {i} TRANSFLEET BUILDING");
				invoiceLineCollection.Add(lineDataObject);
			}

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			AssertExceptionThrown<MessageProcessingBusinessFailureException>("Manufacturer Number Exceeds Maximum.", ISFHeaderDataObjectReader.ManufacturerNumberExceedsMaximum, () => reader.ReadIntoBusinessObject());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateExistingISF_ManufacturerByXml()
		{
			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "AMYUSTNYC";
			importerOrg.OH_FullName = "AMY US TEST";
			importerOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var patternMatch = Factory.New<OrgPatternMatch>();
			patternMatch.OS_OH = importerOrg.PK;
			patternMatch.OS_OA = importerOrg.MainAddress.PK;
			patternMatch.OS_FullCompanyName = "AMY US TEST";
			patternMatch.OS_UNLOCO = Core.Constants.CountryCodes.UnitedStates;
			patternMatch.OS_CompanyName1 = "A500";
			patternMatch.OS_IsCorporation = false;

			Factory.SaveForTesting();

			var matchedISF = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("No matched ISF", 0, matchedISF.Length);

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseTestFilePath + "Job2Manufacturers.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			matchedISF = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("1 ISF created", 1, matchedISF.Length);

			AssertEquals("APLU2153022277", matchedISF[0].BF_OceanBill);
			AssertEquals("3 lines", 3, matchedISF[0].Lines.Count);
			AssertEquals("3 Manufacturers", 3, matchedISF[0].ManufacturerAddresses.Count);

			message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseTestFilePath + "Job3Manufacturers.xml"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			var newFactory = new UniversalObjectFactory();
			matchedISF = newFactory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerOrg.PK));
			AssertEquals("No more ISF created", 1, matchedISF.Length);
			AssertEquals("APLU2153022277", matchedISF[0].BF_OceanBill);
			AssertEquals("Added new Manufacturer", 4, matchedISF[0].ManufacturerAddresses.Count);
		}

		public void TestImportDuplicateManufacturers()
		{
			var headerDataObject = SetupISFHeader(SubmissionTypeList.Codes.ISF10, ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, TransportModeCodes.Codes.OceanVesselContainerized, "TEST OWNER REF", ActionReasonCodeList.Codes.CompliantTransaction,
				"TEST CUSTOMS REF", "ABC-12345678901", "BD323423", "MWB012345", BillTypeList.Codes.OceanBillOfLading);
			Factory.New<CusISFHeader>();
			Factory.SaveForTesting();

			var invoiceLineCollection = new DataObjectList<CommercialInvoiceLine>();
			headerDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => invoiceLineCollection))
				})
			};

			for (int i = 0; i < 500; i++)
			{
				var lineDataObject = SetupISFLine("TEST PRODUCT", Core.Constants.CountryCodes.China, "9201.10.00");
				PopulateManufacturer(lineDataObject, "Test Company", "US", 211000.ToString(), "CHI", $"GODOWN 292 TRANSFLEET BUILDING");
				invoiceLineCollection.Add(lineDataObject);
			}

			var reader = new ISFHeaderDataObjectReader(headerDataObject, logger, Factory);
			var headerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(headerBO);
			AssertEquals(1, headerBO.ManufacturerAddresses.Count);
		}

		void PopulateManufacturer(CommercialInvoiceLine line, ZString companyName, ZString countryCode, ZString postcode, ZString city, ZString address1)
		{
			var orgAddress = new OrganizationAddress
			{
				AddressType = nameof(DocAddressType.Manufacturer),
				CompanyName = companyName,
				Country = new Country
				{
					Code = countryCode
				},
				Postcode = postcode,
				Address1 = address1,
				City = city,
			};
			line.OrganizationAddressCollection = new List<OrganizationAddress> { orgAddress };
		}

		CommercialInvoiceLine SetupISFLine(ZString productCode, ZString goodsOfOrigin, ZString tariffCode)
		{
			var result = new CommercialInvoiceLine()
			{
				PartNo = productCode,
				CountryOfOrigin = new Country()
				{ Code = goodsOfOrigin },
				HarmonisedCode = tariffCode
			};
			return result;
		}

		Shipment SetupISFHeader(ZString ownerRef, ZString customsRef, ZString billNumber, ZString billType)
		{
			return SetupISFHeader(SubmissionTypeList.Codes.ISF10, ShipmentTypeList.Codes.HouseholdGoodsAndPersonalEffects, TransportModeCodes.Codes.OceanVesselContainerized, ownerRef, ActionReasonCodeList.Codes.CompliantTransaction, customsRef,
				"ABC-12345678901", "BD323423", billNumber, billType);
		}

		Shipment SetupISFHeader(ZString entryType, ZString shipmentType, ZString transportMode, ZString ownerRef, ZString actionCode, ZString customsRef, ZString entryNumber, ZString bondRefNumber, ZString billNumber, ZString billType)
		{
			return SetupISFHeader(entryType, shipmentType, transportMode, INCBranch, ownerRef, actionCode, "CNSHA", "USCHI", ShipmentSubTypeList.Codes.LowValueEntriesShipments, 1500m, 1000, USPackingingUnitList.ShippingOrPackingingUnitList.Codes.Bag, 1100,
				Core.Constants.Weight.Kilograms, customsRef, entryNumber, bondRefNumber, billNumber, billType, ZDateTime.BrettsBirthday, "ATTRIB ONE", "ATTRIB TWO");
		}

		Shipment SetupISFHeader(ZString entryType, ZString shipmentType, ZString transportMode, GlbBranch branch, ZString ownerRef, ZString actionCode, ZString portOfUnloads, ZString placeOfDelivery, ZString shipmentSubType, ZDecimal goodsValue, ZInt noOfPacks, ZString packageType,
			ZInt totalWeight, ZString weightUnit, ZString customsRef, ZString entryNumber, ZString bondRefNumber, ZString billNumber, ZString billType, ZDateTime lastAcceptedDate, ZString customAttribute1, ZString customAttribute2)
		{
			var containerCode = transportMode == TransportModeCodes.Codes.OceanVesselContainerized
								? ContainerModeList.Codes.Containerized
								: ContainerModeList.Codes.NonContainerized;

			var result = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransportMode = new CodeDescriptionPair() { Code = TransportTypeList.Codes.Sea, Description = TransportTypeList.Codes.Sea },
				CustomsContainerMode = new ContainerMode() { Code = containerCode },
				Branch = Branch.New(branch),
				OwnerRef = ownerRef,
				PortOfDischarge = new UNLOCO() { Code = portOfUnloads },
				PortOfDestination = new UNLOCO() { Code = placeOfDelivery },
				GoodsValue = goodsValue,
				TotalNoOfPacks = noOfPacks,
				TotalNoOfPacksPackageType = new PackageType() { Code = packageType },
				TotalWeight = (ZDecimal)totalWeight,
				TotalWeightUnit = new UnitOfWeight() { Code = weightUnit },
				WayBillNumber = billNumber,
				WayBillType = new WayBillType() { Code = billType == BillTypeList.Codes.HouseBillOfLading ? WayBillTypeList.Codes.House : billType == BillTypeList.Codes.OceanBillOfLading ? WayBillTypeList.Codes.Master : WayBillTypeList.Codes.MasterHouse },
			};
			result.SetDateCollection(() => new List<Date> { new List<Date>().Add(DateType.ISFLastAccepted, true, lastAcceptedDate) });
			result.SetAddInfoCollection(() => new List<AddInfo>()
				{
					new AddInfo() { Key = Constants.AddInfoConstants.EntryType, Value = entryType },
					new AddInfo() { Key = Constants.AddInfoConstants.ISFShipmentType, Value = shipmentType },
					new AddInfo() { Key = Constants.AddInfoConstants.ISFShipmentSubType, Value = shipmentSubType },
					new AddInfo() { Key = Constants.AddInfoConstants.ActionReason, Value = actionCode }
				});
			result.SetEntryNumberCollection(() => new List<EntryNumber>()
				{
					new EntryNumber() { Type = new EntryType() { Code = Constants.EntryNumberConstants.ISF }, Number = customsRef, CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates } },
					new EntryNumber() { Type = new EntryType() { Code = Constants.EntryNumberConstants.ENS }, Number = entryNumber, CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates } }
				});
			result.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
				{
					new AdditionalReference() { Type = new EntryType() { Code = BillTypeList.Codes.BondReferenceNumber }, ReferenceNumber = bondRefNumber }
				});
			result.SetAdditionalBillCollection(() => new List<AdditionalBill>()
				{
					new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance) { BillType = new WayBillType() { Code = billType }, BillNumber = billNumber }
				});
			result.SetCustomizedFieldCollection(() => new List<CustomizedField>()
				{
					new CustomizedField() { Key = Constants.CustomizedFieldConstants.CustomAttribOne, Value = customAttribute1 , DataType = DataType.String },
					new CustomizedField() { Key = Constants.CustomizedFieldConstants.CustomAttribTwo, Value = customAttribute2 , DataType = DataType.String },
					new CustomizedField() { Key = "TESTA", Value = "Aha", DataType = DataType.String },
				});

			return result;
		}

		GlbCompany INCCompany
		{
			get
			{
				if (incCompany == null)
				{
					incCompany = Factory.New<GlbCompany>();
					incCompany.GC_Code = "INC";
					incCompany.GC_Name = "IAN TEST COMPANY";
					incCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
					incCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				}

				return incCompany;
			}
		}
		GlbCompany incCompany;

		GlbBranch INCBranch
		{
			get
			{
				if (incBranch == null)
				{
					incBranch = INCCompany.Branches.AddNew();
					incBranch.GB_Code = "INB";
					incBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				}

				return incBranch;
			}
		}
		GlbBranch incBranch;

		string BaseTestFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Customs\US\ISF\DataTransfer.Test\TestFiles\";
	}
}
