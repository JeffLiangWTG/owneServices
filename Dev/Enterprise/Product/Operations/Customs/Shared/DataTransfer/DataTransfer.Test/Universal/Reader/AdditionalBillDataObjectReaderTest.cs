using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestBillCusAddInfoAndCusCodeData()
		{
			var houseBillRefNoString = "HBR";
			var housebillRefNoOceanBill = "OB";
			var declaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var billCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)bill;
			Type itDocType = null;
			billCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITDoc, out itDocType);
			AssertNotNull(itDocType);
			var itdocAddInfo = USITDocAddInfoSchema.Constants.US_7512OpenArea.Substring(3) + "=AREA1234";
			Type type = null;
			Assert(billCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(CusAddInfoTypeAttribute.Codes.USITNumber, out type));
			var itNumberAddInfo = USITNumberAddInfoSchema.Constants.US_ITNumber.Substring(3) + "=IT123";
			var billCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)bill;
			Assert(billCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(houseBillRefNoString, out type));
			bill.Delete();
			var additionalBillDataObject = SetupAdditionalBill("HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 3), "MB1", null, 11, Core.Constants.PkgUnit.Box, "Box");
			var itdocDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USITDoc },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(itdocAddInfo)
			};
			var itNumberDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USITNumber },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(itNumberAddInfo)
			};
			additionalBillDataObject.SetAddInfoGroupCollection(() => new List<UniversalCustoms.AddInfoGroup>(new[] { itdocDataObject, itNumberDataObject }));
			var houseBillRefNoDataObject = new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = houseBillRefNoString },
				SubType = new CodeDescriptionPair35Char() { Code = housebillRefNoOceanBill },
				Reference = "OB1234"
			};
			additionalBillDataObject.SetCustomsReferenceCollection(() => new List<UniversalCustoms.CustomsReference>(new[] { houseBillRefNoDataObject }));

			var reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), new AdditionalBillDataProvider<Bill>(declaration), null, null);
			var billBO = reader.ReadIntoBusinessObject();

			AssertNotNull(billBO);

			CombineAssertions(delegate
			{
				var cusAddInfoBOs = LoadCusAddInfo(billBO.TablePrefix, billBO.PK);
				AssertEquals("cusAddInfoBOs.Length", 2, cusAddInfoBOs.Length);
				var itDocBO = cusAddInfoBOs[0];
				var itNumberBO = cusAddInfoBOs[1];
				if (CusAddInfoTypeAttribute.Codes.USITDoc.Equals(itNumberBO.GetValue(CusAddInfoSchema.B7_Type)))
				{
					itDocBO = cusAddInfoBOs[1];
					itNumberBO = cusAddInfoBOs[0];
				}
				AssertCusAddInfoContents(itDocBO, billBO.TablePrefix, billBO.PK, CusAddInfoTypeAttribute.Codes.USITDoc, partialAddInfoData: itdocAddInfo);
				AssertCusAddInfoContents(itNumberBO, billBO.TablePrefix, billBO.PK, CusAddInfoTypeAttribute.Codes.USITNumber, partialAddInfoData: itNumberAddInfo);
				var cusCodeDataBOs = LoadCusCodeData(billBO.TablePrefix, billBO.PK);
				AssertEquals("cusCodeDataBOs.Length", 1, cusCodeDataBOs.Length);
				var houseBillRefNoBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents(houseBillRefNoBO, billBO.TablePrefix, billBO.PK, houseBillRefNoString, housebillRefNoOceanBill, "OB1234", ZBool.False, ZShort.Zero);
			});
		}

		public void TestAdditionalBillMatchingToEmptyBillNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillNum = ZString.Empty;
			Factory.SaveForTesting();

			var additionalBillDataObject = SetupAdditionalBill("", WayBillTypeList.Codes.Master, WayBillTypeList.Descriptions.Master, new ZDateTime(2011, 4, 3), null, null, null, Core.Constants.PkgUnit.Box, null);
			var reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "" }, null);
			var billBO = reader.ReadIntoBusinessObject();
			AssertSame(masterBill1, billBO);
		}

		public void TestAdditionalBillDoNotImportIfBillTypeIsNotSpecified()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.SaveForTesting();

			var additionalBillDataObject = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BillNumber = "MB1"
			};
			var reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB2" }, null);
			AssertNull(reader.ReadIntoBusinessObject());
			AssertMultilineASCIIEquals("logger.Logs", @"
Error - Cannot populate Bill because:
BillType.Code must be specified and not empty.
			".Trim(), logger.Logs);
		}

		public void TestBasicAdditionalBillLevelFieldMappings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "SHB1";
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OverrideFreightDefaults = true;
				var additionalBillDataObject = SetupAdditionalBill("HB1", WayBillTypeList.Codes.House, WayBillTypeList.Descriptions.House, new ZDateTime(2011, 4, 3), "MB1", SetupAddInfos(USBillAddInfoString), 11, Core.Constants.PkgUnit.Box, "Box");
				var reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB1" }, new BillDetail() { BillNumber = "HB1" });
				var billBO = reader.ReadIntoBusinessObject();

				AssertNotNull(billBO);

				CombineAssertions(delegate
				{
					AssertEquals("billBO.CU_JE", declaration.PK, billBO.CU_JE);
					AssertContents(billBO, "HB1", BillTypeList.Codes.HouseBill, ZGuid.Empty, new ZDateTime(2011, 4, 3), 11, Core.Constants.PkgUnit.Box, USBillAddInfoString);
					AssertEquals("billBO.CU_GUIPresentationRecord", ZBool.True, billBO.CU_GUIPresentationRecord);
					AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Bill found, creating new Bill.
Information - Populating Bill...
Warning - Cannot find Parent Bill (Type:'MB', Number:'MB1') for Bill (Type:'HB', Number:'HB1').
".Trim(), logger.Logs);

					reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB2" }, new BillDetail() { BillNumber = "HB1" });
					billBO = reader.ReadIntoBusinessObject();
					AssertEquals("billBO.CU_JE", declaration.PK, billBO.CU_JE);
					AssertContents(billBO, "HB1", BillTypeList.Codes.HouseBill, ZGuid.Empty, new ZDateTime(2011, 4, 3), 11, Core.Constants.PkgUnit.Box, USBillAddInfoString);
					AssertEquals("billBO.CU_GUIPresentationRecord", ZBool.False, billBO.CU_GUIPresentationRecord);

					reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB1" }, new BillDetail() { BillNumber = "HB2" });
					billBO = reader.ReadIntoBusinessObject();
					AssertEquals("billBO.CU_JE", declaration.PK, billBO.CU_JE);
					AssertContents(billBO, "HB1", BillTypeList.Codes.HouseBill, ZGuid.Empty, new ZDateTime(2011, 4, 3), 11, Core.Constants.PkgUnit.Box, USBillAddInfoString);
					AssertEquals("billBO.CU_GUIPresentationRecord", ZBool.False, billBO.CU_GUIPresentationRecord);

					additionalBillDataObject.BillNumber = "MB2";
					additionalBillDataObject.BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
					reader = new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, CurrentCompanyHelper, new AdditionalBillDataProvider<Bill>(declaration), new BillDetail() { BillNumber = "MB2" }, new BillDetail() { BillNumber = "HB1" });
					billBO = reader.ReadIntoBusinessObject();
					AssertEquals("billBO.CU_JE", declaration.PK, billBO.CU_JE);
					AssertContents(billBO, "MB2", BillTypeList.Codes.MasterBill, ZGuid.Empty, new ZDateTime(2011, 4, 3), 11, Core.Constants.PkgUnit.Box, USBillAddInfoString);
					AssertEquals("billBO.CU_GUIPresentationRecord", ZBool.True, billBO.CU_GUIPresentationRecord);
				});
			}
		}

		const string USBillAddInfoString = "Weight=10*UI_NKBillIssuerSCAC=AAPJ";
	}
}
