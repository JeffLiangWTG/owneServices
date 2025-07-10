using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
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
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleHLVLCreateMultipleMAWBs()
		{
			NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\HVLVShipment.xml"));
			manager.Process(message);

			var mawbs = new CusMAWB.Loader(Factory.BOFactory).FindMatchingForwarderMAWBs("08123232322", "QF200", new ZDateTime(2012, 10, 13));
			AssertEquals(4, mawbs.Length);
			var mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "1"));
			AssertEquals("HB1", mawb1.CM_MasterHouseBill);
			AssertNotNull(mawb1);
			AssertEquals(5, mawb1.ChildBills.Count);
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "2"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "3"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "4"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "5"));
			var mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "6"));
			AssertNotNull(mawb2);
			AssertEquals("HB1", mawb2.CM_MasterHouseBill);
			AssertEquals(4, mawb2.ChildBills.Count);
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "7"));
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "8"));
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "9"));

			var mawb3 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "11"));
			AssertNotNull(mawb3);
			AssertEquals("HB2", mawb3.CM_MasterHouseBill);
			AssertEquals(5, mawb3.ChildBills.Count);
			Assert(mawb3.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "12"));
			Assert(mawb3.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "13"));
			Assert(mawb3.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "14"));
			Assert(mawb3.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "15"));
			var mawb4 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "16"));
			AssertNotNull(mawb4);
			AssertEquals("HB2", mawb4.CM_MasterHouseBill);
			AssertEquals(4, mawb4.ChildBills.Count);
			Assert(mawb4.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "17"));
			Assert(mawb4.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "18"));
			Assert(mawb4.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "19"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTerminateProcessWhenOneMawbHasActiveMessaging()
		{
			NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "08123232322";
			mawb1.CM_FlightNo = "QF200";
			mawb1.CM_ArrivalDate = new ZDateTime(2012, 10, 13);
			mawb1.CM_RL_NKLoadPort = "AUSYD";
			mawb1.CM_RL_NKDischargePort = "NZHLZ";
			mawb1.CM_MasterHouseBill = "HB1";

			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			hawb1.CS_HAWB = "1";
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "2";
			var hawb3 = mawb1.ChildBills.AddNew();
			hawb3.CS_HAWB = "3";
			var hawb4 = mawb1.ChildBills.AddNew();
			hawb4.CS_HAWB = "4";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "08123232322";
			mawb2.CM_FlightNo = "QF200";
			mawb2.CM_ArrivalDate = new ZDateTime(2012, 10, 13);
			mawb2.CM_RL_NKLoadPort = "AUSYD";
			mawb2.CM_RL_NKDischargePort = "NZHLZ";
			mawb2.CM_MasterHouseBill = "HB1";

			var hawb6 = mawb2.ChildBills.AddNew();
			hawb6.CS_HAWB = "6";
			var hawb7 = mawb2.ChildBills.AddNew();
			hawb7.CS_HAWB = "7";
			var hawb8 = mawb2.ChildBills.AddNew();
			hawb8.CS_HAWB = "8";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertMultilineASCIIEquals("Logs", @"Successfully loaded matching CusMAWB.
Error - Cannot populate CusMAWB because:
There is an attempt to update Discharge Port from 'NZHLZ' to 'NZAKL' on this Master/Sub-Master while it has at least one House Bill with an active messaging.

Successfully saved, but nothing was reported as being updated.", message.GetLogNoteText());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMultipleCusMAWBs()
		{
			NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbs = new CusMAWB.Loader(Factory.BOFactory).FindMatchingForwarderMAWBs("08123232322", "QF200", new ZDateTime(2012, 10, 13));
			AssertEquals(2, mawbs.Length);
			var mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "1"));
			AssertNotNull(mawb1);
			AssertEquals(5, mawb1.ChildBills.Count);
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "2"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "3"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "4"));
			Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "5"));
			var mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "6"));
			AssertNotNull(mawb2);
			AssertEquals(4, mawb2.ChildBills.Count);
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "7"));
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "8"));
			Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "9"));

			AssertMultilineASCIIEquals("Logs", @"No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' with a score of 250.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Gagan gogia; Address 1: 22 Kellaway Street; Address 2: doonside; City: sydney]'.
Added Consignment: (HAWB: 1) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' by short code.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: dillen ozdemir; Address 1: 15 glenmore st; Address 2: boxhill; City: melbourne]'.
Added Consignment: (HAWB: 2) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Sean Supierz; Address 1: 15 Queensbury Road Joondalup; City: Perth]'.
Added Consignment: (HAWB: 3) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: amanda takasch; Address 1: 23  Mildred  St; City: Whyalla Norrie]'.
Added Consignment: (HAWB: 4) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Darryl Frost; Address 1: U2 / 5; Address 2: Bottlebrush Ave; City: Bli Bli]'.
Added Consignment: (HAWB: 5) from UniversalShipment.
Added AirCargo Report (MAWB: 08123232322) from UniversalShipment.
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Micaela Alcaino; Address 1: 4 Ravenna st; Address 2: strathfield; City: sydney]'.
Added Consignment: (HAWB: 8) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Rebecca Erhart; Address 1: 3o Florida Ave; City: Woy Woy]'.
Added Consignment: (HAWB: 9) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Blueprint Productions; Address 1: 10 Marlino Avenue; City: Warburton]'.
Added Consignment: (HAWB: 6) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: rajnish sharma; Address 1: unit 12 /; Address 2: 8 lower mount street; City: wentworthville]'.
Added Consignment: (HAWB: 7) from UniversalShipment.
Added AirCargo Report (MAWB: 08123232322) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 08123232322 Job#: X00001001) with 9 x CusHAWB, 1 x CusMAWB.", message.GetLogNoteText());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMultipleCusMAWBs_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation2.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var mawbs = new CusMAWB.Loader(Factory.BOFactory).FindMatchingMAWBs("08123232322");
				AssertEquals(2, mawbs.Length);
				var mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "1"));
				AssertNotNull(mawb1);
				AssertEquals(5, mawb1.ChildBills.Count);
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "2"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "3"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "4"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "5"));
				var mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "6"));
				AssertNotNull(mawb2);
				AssertEquals(4, mawb2.ChildBills.Count);
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "7"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "8"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "9"));

				AssertMultilineASCIIEquals("Logs", @"No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' with a score of 250.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Gagan gogia; Address 1: 22 Kellaway Street; Address 2: doonside; City: sydney]'.
Added Consignment: (HAWB: 1) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' by short code.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: dillen ozdemir; Address 1: 15 glenmore st; Address 2: boxhill; City: melbourne]'.
Added Consignment: (HAWB: 2) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Sean Supierz; Address 1: 15 Queensbury Road Joondalup; City: Perth]'.
Added Consignment: (HAWB: 3) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: amanda takasch; Address 1: 23  Mildred  St; City: Whyalla Norrie]'.
Added Consignment: (HAWB: 4) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Darryl Frost; Address 1: U2 / 5; Address 2: Bottlebrush Ave; City: Bli Bli]'.
Added Consignment: (HAWB: 5) from UniversalShipment.
Added AirCargo Report (MAWB: 08123232322) from UniversalShipment.
No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Micaela Alcaino; Address 1: 4 Ravenna st; Address 2: strathfield; City: sydney]'.
Added Consignment: (HAWB: 8) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Rebecca Erhart; Address 1: 3o Florida Ave; City: Woy Woy]'.
Added Consignment: (HAWB: 9) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Blueprint Productions; Address 1: 10 Marlino Avenue; City: Warburton]'.
Added Consignment: (HAWB: 6) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: rajnish sharma; Address 1: unit 12 /; Address 2: 8 lower mount street; City: wentworthville]'.
Added Consignment: (HAWB: 7) from UniversalShipment.
Added AirCargo Report (MAWB: 08123232322) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 08123232322 Job#: X00001001) with 9 x CusHAWB, 1 x CusMAWB.", message.GetLogNoteText());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateMultipleCusMAWBs()
		{
			NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "08123232322";
			mawb1.CM_FlightNo = "QF200";
			mawb1.CM_ArrivalDate = new ZDateTime(2012, 10, 13);
			mawb1.CM_RL_NKLoadPort = "AUSYD";
			mawb1.CM_RL_NKDischargePort = "NZAKL";
			mawb1.CM_MasterHouseBill = "HB1";

			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "1";
			var hawb2 = mawb1.ChildBills.AddNew();
			hawb2.CS_HAWB = "2";
			var hawb3 = mawb1.ChildBills.AddNew();
			hawb3.CS_HAWB = "3";
			var hawb9 = mawb1.ChildBills.AddNew();
			hawb9.CS_HAWB = "10";
			hawb9.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
			hawb9.CS_IsHVLV = true;
			var hawb10 = mawb1.ChildBills.AddNew();
			hawb10.CS_HAWB = "11";
			hawb10.CS_IsHVLV = true;

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "08123232322";
			mawb2.CM_FlightNo = "QF200";
			mawb2.CM_ArrivalDate = new ZDateTime(2012, 10, 13);
			mawb2.CM_RL_NKLoadPort = "AUSYD";
			mawb2.CM_RL_NKDischargePort = "NZAKL";
			mawb2.CM_MasterHouseBill = "HB1";

			var hawb6 = mawb2.ChildBills.AddNew();
			hawb6.CS_HAWB = "6";
			var hawb7 = mawb2.ChildBills.AddNew();
			hawb7.CS_HAWB = "7";
			var hawb8 = mawb2.ChildBills.AddNew();
			hawb8.CS_HAWB = "8";

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation.xml"));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			var mawbs = new CusMAWB.Loader(new BusinessObjectFactory()).FindMatchingForwarderMAWBs("08123232322", "QF200", new ZDateTime(2012, 10, 13)).Cast<CusMAWB>();
			CombineAssertions(() =>
			{
				AssertEquals(2, mawbs.Count());
				mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "1"));
				AssertNotNull(mawb1);
				mawb1.ChildBills.Load();
				AssertEquals(5, mawb1.ChildBills.Count);
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "2"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "3"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "4"));
				Assert("Consignement with active messaging will not be deleted", mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "10"));
				Assert("Consignement without active messaging will be deleted", !mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "11"));
				mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "5"));
				AssertNotNull(mawb2);
				mawb2.ChildBills.Load();
				AssertEquals(5, mawb2.ChildBills.Count);
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "6"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "7"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "8"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "9"));

				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching CusMAWB.
Populating CusMAWB...
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' with a score of 250.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Gagan gogia; Address 1: 22 Kellaway Street; Address 2: doonside; City: sydney]'.
Updated Consignment: (HAWB: 1) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' by short code.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: dillen ozdemir; Address 1: 15 glenmore st; Address 2: boxhill; City: melbourne]'.
Updated Consignment: (HAWB: 2) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Sean Supierz; Address 1: 15 Queensbury Road Joondalup; City: Perth]'.
Updated Consignment: (HAWB: 3) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: amanda takasch; Address 1: 23  Mildred  St; City: Whyalla Norrie]'.
Added Consignment: (HAWB: 4) from UniversalShipment.
Warning - Consignment: (HAWB: 10) does not appear in UniveralShipment, but this Consignment: (HAWB: 10) cannot be deleted because there are messages associated with it.
Deleted Consignment: (HAWB: 11) from UniversalShipment.
Updated AirCargo Report (MAWB: 08123232322 Job#: X00001000) from UniversalShipment.
Successfully loaded matching CusMAWB.
Populating CusMAWB...
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Micaela Alcaino; Address 1: 4 Ravenna st; Address 2: strathfield; City: sydney]'.
Updated Consignment: (HAWB: 8) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Rebecca Erhart; Address 1: 3o Florida Ave; City: Woy Woy]'.
Added Consignment: (HAWB: 9) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Darryl Frost; Address 1: U2 / 5; Address 2: Bottlebrush Ave; City: Bli Bli]'.
Added Consignment: (HAWB: 5) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Blueprint Productions; Address 1: 10 Marlino Avenue; City: Warburton]'.
Updated Consignment: (HAWB: 6) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: rajnish sharma; Address 1: unit 12 /; Address 2: 8 lower mount street; City: wentworthville]'.
Updated Consignment: (HAWB: 7) from UniversalShipment.
Updated AirCargo Report (MAWB: 08123232322 Job#: X00001001) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 08123232322 Job#: X00001001) with 9 x CusHAWB, 1 x CusMAWB.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUpdateMultipleCusMAWBs_HVLV_LimitNotExceededWhenNonHVLBillsExist()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var existingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var (universalConsol, universalShipment, forwardingConsol, forwardingShipment) = SetupHVLVShipmentAndUniversalShipment(null, "S001", TransportModeCodeList.Codes.Air, isImport: true, consignmentsOnShipment: 9, consignmentWaybillPrefix: "CON-");

				var dataObjectProvider = new UniversalCustomsDataObjectProvider();

				var mawbReaders = dataObjectProvider.GetNewAirManifestDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).Cast<CusMAWBDataObjectReader>();
				var mawbs = mawbReaders.Select(x => x.ReadIntoBusinessObject()).OrderBy(x => x.ChildBills.Count).ToList();

				var newHAWB = mawbs[0].ChildBills.AddNew();
				newHAWB.CS_IsHVLV = false;
				newHAWB.CS_HAWB = "KEEP";
				newHAWB.CS_JS = existingShipment.PK;

				Factory.SaveForTesting();

				AddHVLVConsignmentWithItemToUniversalShipment(universalShipment, "NewBill");

				mawbReaders = dataObjectProvider.GetNewAirManifestDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).Cast<CusMAWBDataObjectReader>();
				mawbs = mawbReaders.Select(x => x.ReadIntoBusinessObject()).ToList();

				CombineAssertions(() =>
				{
					AssertEquals("Total MAWBs", 3, mawbs.Count);
					var mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "CON-0"));
					AssertNotNull("MAWB containing HAWB 1 should exist", mawb1);
					AssertEquals("MAWB1 child bills count", 5, mawb1.ChildBills.Count);
					AssertContainsExactElementsInAnyOrder("MAWB1 child bills", ["CON-0", "CON-1", "CON-2", "CON-3", "CON-4"], mawb1.ChildBills.Select(x => x.CS_HAWB));
					var mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "CON-6"));
					AssertNotNull("MAWB2 exists", mawb2);
					AssertEquals("MAWB2 child bills count", 5, mawb2.ChildBills.Count);
					AssertContainsExactElementsInAnyOrder("MAWB2 child bills", ["CON-5", "CON-6", "CON-7", "CON-8", "KEEP"], mawb2.ChildBills.Select(x => x.CS_HAWB));
					var mawb3 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "NEWBILL"));
					AssertNotNull("MAWB3 exists", mawb3);
					AssertEquals("MAWB3 child bills count", 1, mawb3.ChildBills.Count);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateMultipleCusMAWBs_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var mawb1 = Factory.New<CusMAWB>();
				mawb1.CM_MAWB = "08123232322";
				mawb1.CM_FlightNo = "QF200";
				mawb1.CM_DepartureDate = new ZDateTime(2012, 10, 12);
				mawb1.CM_RL_NKDischargePort = "AUSYD";
				mawb1.CM_RL_NKLoadPort = "NZAKL";
				mawb1.CM_MasterHouseBill = "HB1";

				var hawb1 = mawb1.ChildBills.AddNew();
				hawb1.CS_HAWB = "1";
				var hawb2 = mawb1.ChildBills.AddNew();
				hawb2.CS_HAWB = "2";
				var hawb3 = mawb1.ChildBills.AddNew();
				hawb3.CS_HAWB = "3";
				var hawb9 = mawb1.ChildBills.AddNew();
				hawb9.CS_HAWB = "10";
				hawb9.CS_IsHVLV = true;
				hawb9.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				var hawb10 = mawb1.ChildBills.AddNew();
				hawb10.CS_HAWB = "11";
				hawb10.CS_IsHVLV = true;

				var mawb2 = Factory.New<CusMAWB>();
				mawb2.CM_MAWB = "08123232322";
				mawb2.CM_FlightNo = "QF200";
				mawb2.CM_DepartureDate = new ZDateTime(2012, 10, 12);
				mawb2.CM_RL_NKDischargePort = "AUSYD";
				mawb2.CM_RL_NKLoadPort = "NZAKL";
				mawb2.CM_MasterHouseBill = "HB1";

				var hawb6 = mawb2.ChildBills.AddNew();
				hawb6.CS_HAWB = "6";
				var hawb7 = mawb2.ChildBills.AddNew();
				hawb7.CS_HAWB = "7";
				var hawb8 = mawb2.ChildBills.AddNew();
				hawb8.CS_HAWB = "8";

				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\AirCargo ECI\TestFiles\UniversalShipment For HVLV Shipper Consolidation2.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusMAWBSchema.CM_MAWB, "08123232322");
				var mawbs = (new BusinessObjectFactory()).Load<CusMAWB>(findFilter);

				AssertEquals(2, mawbs.Length);
				mawb1 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "1"));
				AssertNotNull(mawb1);
				mawb1.ChildBills.Load();
				AssertEquals(5, mawb1.ChildBills.Count);
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "2"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "3"));
				Assert(mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "4"));
				Assert("Consignement with active messaging will not be deleted", mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "10"));
				Assert("Consignement without active messaging will be deleted", !mawb1.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "11"));
				mawb2 = mawbs.FirstOrDefault(x => x.ChildBills.Cast<CusHAWB>().Any(y => y.CS_HAWB == "5"));
				AssertNotNull(mawb2);
				mawb2.ChildBills.Load();
				AssertEquals(5, mawb2.ChildBills.Count);
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "6"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "7"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "8"));
				Assert(mawb2.ChildBills.Cast<CusHAWB>().Any(x => x.CS_HAWB == "9"));

				AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Successfully loaded matching CusMAWB.
Populating CusMAWB...
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' with a score of 250.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Gagan gogia; Address 1: 22 Kellaway Street; Address 2: doonside; City: sydney]'.
Updated Consignment: (HAWB: 1) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Matching 'ConsignorDocumentaryAddress':- Matched to 'BARSOU' by code, address 'Pickup and Delivery Addre' by short code.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: dillen ozdemir; Address 1: 15 glenmore st; Address 2: boxhill; City: melbourne]'.
Updated Consignment: (HAWB: 2) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Sean Supierz; Address 1: 15 Queensbury Road Joondalup; City: Perth]'.
Updated Consignment: (HAWB: 3) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: amanda takasch; Address 1: 23  Mildred  St; City: Whyalla Norrie]'.
Added Consignment: (HAWB: 4) from UniversalShipment.
Warning - Consignment: (HAWB: 10) does not appear in UniveralShipment, but this Consignment: (HAWB: 10) cannot be deleted because there are messages associated with it.
Deleted Consignment: (HAWB: 11) from UniversalShipment.
Updated AirCargo Report (MAWB: 08123232322 Job#: X00001000) from UniversalShipment.
Successfully loaded matching CusMAWB.
Populating CusMAWB...
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Micaela Alcaino; Address 1: 4 Ravenna st; Address 2: strathfield; City: sydney]'.
Updated Consignment: (HAWB: 8) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Rebecca Erhart; Address 1: 3o Florida Ave; City: Woy Woy]'.
Added Consignment: (HAWB: 9) from UniversalShipment.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Darryl Frost; Address 1: U2 / 5; Address 2: Bottlebrush Ave; City: Bli Bli]'.
Added Consignment: (HAWB: 5) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: Blueprint Productions; Address 1: 10 Marlino Avenue; City: Warburton]'.
Updated Consignment: (HAWB: 6) from UniversalShipment.
Successfully loaded matching CusHAWB.
Populating CusHAWB...
Warning - Matching 'ConsignorDocumentaryAddress':- No match found for '[Company Name: eforcity1; Address 1: 12340 denholm drive; City: el monte]'.
Warning - Matching 'ConsigneeAddress':- No match found for '[Company Name: rajnish sharma; Address 1: unit 12 /; Address 2: 8 lower mount street; City: wentworthville]'.
Updated Consignment: (HAWB: 7) from UniversalShipment.
Updated AirCargo Report (MAWB: 08123232322 Job#: X00001001) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 08123232322 Job#: X00001001) with 9 x CusHAWB, 1 x CusMAWB.
".Trim(), message.GetLogNoteText());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMultipleOceanBills_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment2.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");

				var oceanBills = Factory.Load<CusSCAOceanBill>(findFilter);
				AssertEquals(2, oceanBills.Length);
				var houseBills1 = oceanBills[0].HouseBills;
				AssertNotNull(houseBills1);
				AssertEquals(5, houseBills1.Count);
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "1111"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "2222"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "3333"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "4444"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "5555"));

				var houseBills2 = oceanBills[1].HouseBills;
				AssertNotNull(houseBills2);
				AssertEquals(2, houseBills2.Count);
				Assert(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "6666"));
				Assert(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "7777"));

				var logs = File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment2_create.txt");

				AssertMultilineASCIIEquals("Logs", logs, message.GetLogNoteText());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateMultipleOceanBills_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment3.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");

				var oceanBills = Factory.Load<CusSCAOceanBill>(findFilter);
				AssertEquals(2, oceanBills.Length);
				var houseBills1 = oceanBills[0].HouseBills;
				AssertNotNull(houseBills1);
				AssertEquals(5, houseBills1.Count);
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "1111"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "2222"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "3333"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "4444"));
				Assert(houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "5555"));

				var houseBills2 = oceanBills[1].HouseBills;
				AssertNotNull(houseBills2);
				AssertEquals(2, houseBills2.Count);
				Assert(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "6666"));
				Assert(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "7777"));

				var logs = File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment3_create.txt");

				AssertMultilineASCIIEquals("Logs", logs, message.GetLogNoteText());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateMultipleOceanBills_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var oceanBill1 = Factory.New<CusSCAOceanBill>();
				oceanBill1.CB_OceanBill = "TEST OCEAN BILL";
				oceanBill1.CB_LloydsIMO = "9143245";
				oceanBill1.CB_Voyage = "VGE N1";
				oceanBill1.CB_MasterHouseBill = "PARENT BILL";
				oceanBill1.CB_IsActive = true;

				var hb1 = oceanBill1.HouseBills.AddNew();
				hb1.CA_HouseBill = "1111";
				hb1.CA_GoodsValue = 1;

				var oceanBill2 = Factory.New<CusSCAOceanBill>();
				oceanBill2.CB_OceanBill = "TEST OCEAN BILL";
				oceanBill2.CB_LloydsIMO = "9143245";
				oceanBill2.CB_Voyage = "VGE N1";
				oceanBill2.CB_MasterHouseBill = "PARENT BILL";
				oceanBill2.CB_IsActive = true;

				var hb8 = oceanBill2.HouseBills.AddNew();
				hb8.CA_HouseBill = "8888";

				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment2.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");

				var oceanBills = (new BusinessObjectFactory()).Load<CusSCAOceanBill>(findFilter);
				AssertEquals(2, oceanBills.Length);
				var houseBills1 = oceanBills[0].HouseBills;
				AssertNotNull(houseBills1);
				AssertEquals(5, houseBills1.Count);
				Assert("update goods value", houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "1111" && x.CA_GoodsValue == 22.0));

				var houseBills2 = oceanBills[1].HouseBills;
				AssertNotNull(houseBills2);
				AssertEquals(2, houseBills2.Count);
				Assert("deleted house bill", !(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "8888")));

				var logs = File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment2_update.txt");

				AssertMultilineASCIIEquals("Logs", logs, message.GetLogNoteText());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateMultipleOceanBills_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var oceanBill1 = Factory.New<CusSCAOceanBill>();
				oceanBill1.CB_OceanBill = "TEST OCEAN BILL";
				oceanBill1.CB_LloydsIMO = "9143245";
				oceanBill1.CB_Voyage = "VGE N1";
				oceanBill1.CB_MasterHouseBill = "PARENT BILL";
				oceanBill1.CB_IsActive = true;

				var hb1 = oceanBill1.HouseBills.AddNew();
				hb1.CA_HouseBill = "1111";
				hb1.CA_GoodsValue = 1;

				var oceanBill2 = Factory.New<CusSCAOceanBill>();
				oceanBill2.CB_OceanBill = "TEST OCEAN BILL";
				oceanBill2.CB_LloydsIMO = "9143245";
				oceanBill2.CB_Voyage = "VGE N1";
				oceanBill2.CB_MasterHouseBill = "PARENT BILL";
				oceanBill2.CB_IsActive = true;

				var hb8 = oceanBill2.HouseBills.AddNew();
				hb8.CA_HouseBill = "8888";

				Factory.SaveForTesting();

				var message = GetQueuedUniversalShipmentMessage(File.ReadAllText(BaseSourcePath +
	@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment3.xml"));

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				manager.Process(message);

				var findFilter = new ZQuery();
				findFilter.AddToFilter(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");

				var oceanBills = (new BusinessObjectFactory()).Load<CusSCAOceanBill>(findFilter);
				AssertEquals(2, oceanBills.Length);
				var houseBills1 = oceanBills[0].HouseBills;
				AssertNotNull(houseBills1);
				AssertEquals(5, houseBills1.Count);
				Assert("update goods value", houseBills1.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "1111" && x.CA_GoodsValue == 22.0));

				var houseBills2 = oceanBills[1].HouseBills;
				AssertNotNull(houseBills2);
				AssertEquals(2, houseBills2.Count);
				Assert("deleted house bill", !(houseBills2.Cast<CusSCAHouse>().Any(x => x.CA_HouseBill == "8888")));

				var logs = File.ReadAllText(BaseSourcePath +
@"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\DataImportExport\Universal\SeaCargo\TestFiles\OceanBillShipment3_update.txt");

				AssertMultilineASCIIEquals("Logs", logs, message.GetLogNoteText());
			}
		}

		public void TestUpdateMultipleOceanBills_ICR_HVLV_LimitNotExceededWhenNonHVLBillsExist()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var existingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var (universalConsol, universalShipment, forwardingConsol, forwardingShipment) = SetupHVLVShipmentAndUniversalShipment(null, "S001", TransportModeCodeList.Codes.Sea, isImport: true, consignmentsOnShipment: 9, consignmentWaybillPrefix: "CON-");

				var dataObjectProvider = new UniversalCustomsDataObjectProvider();

				var oceanBillReaders = dataObjectProvider.GetNewCusSCAOceanBillDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory).Cast<CusSCAOceanBillDataObjectReader>();
				var oceanBills = oceanBillReaders.Select(x => x.ReadIntoBusinessObject()).OrderBy(x => x.HouseBills.Count).ToList();

				var newHouseBill = oceanBills[0].HouseBills.AddNew();
				newHouseBill.CA_IsHVLV = false;
				newHouseBill.CA_HouseBill = "KEEP";
				newHouseBill.CA_JS = existingShipment.PK;

				Factory.SaveForTesting();

				AddHVLVConsignmentWithItemToUniversalShipment(universalShipment, "NewBill");

				oceanBillReaders = dataObjectProvider.GetNewCusSCAOceanBillDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory).Cast<CusSCAOceanBillDataObjectReader>();
				oceanBills = oceanBillReaders.Select(x => x.ReadIntoBusinessObject()).ToList();

				CombineAssertions(() =>
				{
					AssertEquals("Total MAWBs", 3, oceanBills.Count);
					var hb1 = oceanBills.FirstOrDefault(x => x.HouseBills.Cast<CusSCAHouse>().Any(y => y.CA_HouseBill == "CON-0"));
					AssertNotNull("OceanBill containing Housebill 1 should exist", hb1);
					AssertEquals("OceanBill 1 house bills count", 5, hb1.HouseBills.Count);
					AssertContainsExactElementsInAnyOrder("OceanBill1 house bills", ["CON-0", "CON-1", "CON-2", "CON-3", "CON-4"], hb1.HouseBills.Select(x => x.CA_HouseBill));
					var hb2 = oceanBills.FirstOrDefault(x => x.HouseBills.Cast<CusSCAHouse>().Any(y => y.CA_HouseBill == "CON-6"));
					AssertNotNull("OceanBill 2 exists", hb2);
					AssertEquals("OceanBill2 house bills count", 5, hb2.HouseBills.Count);
					AssertContainsExactElementsInAnyOrder("OceanBill2 house bills", ["CON-5", "CON-6", "CON-7", "CON-8", "KEEP"], hb2.HouseBills.Select(x => x.CA_HouseBill));
					var hb3 = oceanBills.FirstOrDefault(x => x.HouseBills.Cast<CusSCAHouse>().Any(y => y.CA_HouseBill == "NEWBILL"));
					AssertNotNull("OceanBill3 exists", hb3);
					AssertEquals("OceanBill3 house bills count", 1, hb3.HouseBills.Count);
				});
			}
		}

		public void TestCreateOceanBills_Consolidate_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 1, isImport: true, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 readers to consolidate bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNotNull("Reader should be reading into existing bill", existingBill);
			}
		}

		public void TestCreateOceanBills_Consolidate_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 1, isImport: false, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 readers to consolidate bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNotNull("Reader should be reading into existing bill", existingBill);
			}
		}

		public void TestCreateMAWBs_Consolidate_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 1, isImport: true, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 readers to consolidate bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNotNull("Reader should be reading into existing bill", existingBill);
			}
		}

		public void TestCreateMAWBs_Consolidate_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 1, isImport: false, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 readers to consolidate bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNotNull("Reader should be reading into existing bill", existingBill);
			}
		}

		public void TestCreateOceanBills_NotConsolidateToTwinsBills_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: true, houseBillsOnLatestBill: 1, isImport: true, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 reader, incoming bills will not consolidate to twins bills", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateOceanBills_NotConsolidateToTwinsBills_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: true, houseBillsOnLatestBill: 1, isImport: false, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 reader, incoming bills will not consolidate to twins bills", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateMAWBs_NotConsolidateToTwinsBills_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: true, houseBillsOnLatestBill: 1, isImport: true, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateMAWBs_NotConsolidateToTwinsBills_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: true, houseBillsOnLatestBill: 1, isImport: false, consignmentsOnShipment: 3);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateOceanBills_NotConsolidateWhenLimitExceed_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: true, consignmentsOnShipment: 4);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateOceanBills_NotConsolidateWhenLimitExceed_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: false, consignmentsOnShipment: 4);

				var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateMAWBs_NotConsolidateWhenLimitExceed_ICR()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: true, consignmentsOnShipment: 4);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCreateMAWBs_NotConsolidateWhenLimitExceed_CRE()
		{
			using (NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: false, consignmentsOnShipment: 4);

				var readers = new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false);

				AssertEquals("Should load 1 reader to read incoming bills into new master bill", 1, readers.Count());

				var reader = readers.First();
				var getExistingBillMethod = reader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var existingBill = getExistingBillMethod.Invoke(reader, Array.Empty<object>());

				AssertNull("Reader should be reading into new bill", existingBill);
			}
		}

		public void TestCheckNoDuplicateWaybills_Air_WhenDuplicateWaybillFromDifferentShipment_ThrowException()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 4;
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: true, consignmentsOnShipment: numberOfConsignments);

				for (var i = 0; i < numberOfConsignments; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).ToList());
				var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
000
001";
				AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
			}
		}

		public void TestCheckNoDuplicateWaybills_MultipleMAWB()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 8;
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 5, isImport: true, consignmentsOnShipment: numberOfConsignments);

				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MAWB = "C00001194";
				mawb.CM_FlightNo = "TT0001";
				mawb.CM_IsActive = true;
				mawb.CM_MasterHouseBill = "S00001538";

				mawb.Logs.AddNew(AutoEvents.Transferred,
				[
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.ShipmentTypes.HighVolumeLowValue),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, "S00001538")
				]);

				var secondshipmentPK = new ZGuid();
				for (var i = 5; i < numberOfConsignments; i++)
				{
					var hb = mawb.ChildBills.AddNew();
					hb.CS_MasterHouseBill = i.ToString(WaybillStringFormat);
					hb.CS_HAWB = i.ToString(WaybillStringFormat);
					hb.CS_GoodsValue = 1;
					hb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
					hb.CS_JS = secondshipmentPK;
				}

				for (var i = 0; i < numberOfConsignments; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).ToList());
				var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
000
001
002
003
004
005
006
007";
				AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
			}
		}

		public void TestCheckNoDuplicateWaybills_Ocean_WhenDuplicateWaybillFromDifferentShipment_ThrowException()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 4;
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: true, consignmentsOnShipment: numberOfConsignments);

				for (var i = 0; i < numberOfConsignments; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory).ToList());
				var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
000
001";
				AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
			}
		}

		public void TestCheckNoDuplicateWaybills_MultipleOceanBills()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 8;
				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 5, isImport: true, consignmentsOnShipment: numberOfConsignments);

				var oceanBill = Factory.New<CusSCAOceanBill>();
				oceanBill.CB_OceanBill = "C00001194";
				oceanBill.CB_LloydsIMO = "9143245";
				oceanBill.CB_Voyage = "VGE N1";
				oceanBill.CB_IsActive = true;
				oceanBill.CB_MasterHouseBill = "S00001538";

				var secondshipmentPK = new ZGuid();
				for (var i = 5; i < numberOfConsignments; i++)
				{
					var hb = oceanBill.HouseBills.AddNew();
					hb.CA_HouseBill = i.ToString(WaybillStringFormat);
					hb.CA_GoodsValue = 1;
					hb.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
					hb.CA_JS = secondshipmentPK;
				}

				for (var i = 0; i < numberOfConsignments; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory).ToList());
				var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
000
001
002
003
004
005
006
007";
				AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
			}
		}

		public void TestCheckNoDuplicateWaybills_WhenDuplicatesInExistingHouseBills_MessageContainsUniqueWaybills()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 2;
				var (consol, shipment) = SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: 2, isImport: true, consignmentsOnShipment: numberOfConsignments);

				for (var i = 0; i < numberOfConsignments; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var hawb = Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_HAWB, 0.ToString(WaybillStringFormat)));
				hawb.CS_HAWB = 1.ToString(WaybillStringFormat);

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).ToList());
				var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
001";
				AssertEquals("Exception contents", expectedMessage, exception.Message.Trim());
			}
		}

		public void TestCheckNoDuplicateWaybills_When10OrMoreDuplicates_MessageEnding()
		{
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var numberOfConsignments = 20;

				var (consol, shipment) = SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(setupForTwinsBill: false, houseBillsOnLatestBill: numberOfConsignments, isImport: true, consignmentsOnShipment: numberOfConsignments);

				CreateDuplicatesAndAssertMessage(9, consol, shipment, hasReachedLimitForDetectingDuplicates: false);

				CreateDuplicatesAndAssertMessage(10, consol, shipment, hasReachedLimitForDetectingDuplicates: true);

				CreateDuplicatesAndAssertMessage(15, consol, shipment, hasReachedLimitForDetectingDuplicates: true);
			}

			void CreateDuplicatesAndAssertMessage(int numberOfDuplicates, Shipment consol, Shipment shipment, bool hasReachedLimitForDetectingDuplicates)
			{
				for (var i = 0; i < numberOfDuplicates; i++)
				{
					consol.SubShipmentCollection[0].SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
					shipment.SubShipmentCollection[i].WayBillNumber = i.ToString(WaybillStringFormat);
				}

				var exception = AssertExceptionThrown<DataObjectValidationException>(() => new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(consol, shipment, new TestErrorLogger(), Factory).ToList());
				var exceptionMessageLines = exception.Message.Trim().Replace("\r", "").Split('\n');
				if (hasReachedLimitForDetectingDuplicates)
				{
					AssertEquals("number of lines in exception message", 12, exceptionMessageLines.Length);
					AssertEquals("Last Line", "More duplicates may exist.", exceptionMessageLines.Last());
				}
				else
				{
					AssertEquals("number of lines in exception message", 1 + numberOfDuplicates, exceptionMessageLines.Length);
					AssertNotEquals("Last Line", "More duplicates may exist.", exceptionMessageLines.Last());
				}
			}
		}

		const string ExpectedDuplicateWaybillExceptionMessageBeginning = "Unable to merge this shipment into existing ICR/CRE because some House Bills already exist. Please fix these duplications and try again:";
		const string WaybillStringFormat = "D3";

		(Shipment, Shipment) SetupExistingOceanBillsAndUniversalShipmentsForConsolidateTesting(bool setupForTwinsBill, int houseBillsOnLatestBill, bool isImport, int consignmentsOnShipment)
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_UniqueConsignRef = "C00001194";
			forwardingConsol.JK_TransportMode = TransportModeCodeList.Codes.Sea;
			forwardingConsol.JK_MasterBillNum = "C00001194";

			var existingBillsOriginShipment = forwardingConsol.Shipments.AddNew();
			existingBillsOriginShipment.JS_UniqueConsignRef = "S00001538";
			existingBillsOriginShipment.JS_TransportMode = TransportModeCodeList.Codes.Sea;

			var shipmentToRunConvertion = forwardingConsol.Shipments.AddNew();
			shipmentToRunConvertion.JS_UniqueConsignRef = "S00001539";
			shipmentToRunConvertion.JS_TransportMode = TransportModeCodeList.Codes.Sea;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(existingBillsOriginShipment);
			HVLVConsignmentHeader.GetOrCreate(shipmentToRunConvertion);

			if (setupForTwinsBill)
			{
				var fullLoadTwinsBill = Factory.New<CusSCAOceanBill>();
				fullLoadTwinsBill.CB_OceanBill = "C00001194";
				fullLoadTwinsBill.CB_LloydsIMO = "9143245";
				fullLoadTwinsBill.CB_Voyage = "VGE N1";
				fullLoadTwinsBill.CB_IsActive = true;
				fullLoadTwinsBill.CB_MasterHouseBill = "S00001538";

				var hb1 = fullLoadTwinsBill.HouseBills.AddNew();
				hb1.CA_HouseBill = "aaaaa";
				hb1.CA_GoodsValue = 1;
				hb1.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb1.CA_JS = existingBillsOriginShipment.PK;
				var hb2 = fullLoadTwinsBill.HouseBills.AddNew();
				hb2.CA_HouseBill = "bbbbb";
				hb2.CA_GoodsValue = 1;
				hb2.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb2.CA_JS = existingBillsOriginShipment.PK;
				var hb3 = fullLoadTwinsBill.HouseBills.AddNew();
				hb3.CA_HouseBill = "ccccc";
				hb3.CA_GoodsValue = 1;
				hb3.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb3.CA_JS = existingBillsOriginShipment.PK;
				var hb4 = fullLoadTwinsBill.HouseBills.AddNew();
				hb4.CA_HouseBill = "ddddd";
				hb4.CA_GoodsValue = 1;
				hb4.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb4.CA_JS = existingBillsOriginShipment.PK;
				var hb5 = fullLoadTwinsBill.HouseBills.AddNew();
				hb5.CA_HouseBill = "eeeee";
				hb5.CA_GoodsValue = 1;
				hb5.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb5.CA_JS = existingBillsOriginShipment.PK;

				consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(fullLoadTwinsBill);
			}

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "C00001194";
			oceanBill.CB_LloydsIMO = "9143245";
			oceanBill.CB_Voyage = "VGE N1";
			oceanBill.CB_IsActive = true;
			oceanBill.CB_MasterHouseBill = "S00001538";

			for (var i = 0; i < houseBillsOnLatestBill; i++)
			{
				var hb = oceanBill.HouseBills.AddNew();
				hb.CA_HouseBill = i.ToString(WaybillStringFormat);
				hb.CA_GoodsValue = 1;
				hb.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
				hb.CA_JS = existingBillsOriginShipment.PK;
			}

			consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(oceanBill);

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.Code = "NZTST";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			consol.WayBillNumber = "C00001194";
			consol.LloydsIMO = "9143245";
			consol.VoyageFlightNo = "VGE N1";
			consol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C00001194");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001539");

			if (isImport)
			{
				consol.PortOfDischarge = UNLOCO.New(port);
			}
			else
			{
				consol.PortOfLoading = UNLOCO.New(port);
			}

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			shipment.WayBillNumber = "S00001539";
			shipment.ShipmentType = new CodeDescriptionPair { Code = ShipmentTypes.HighVolumeLowValue };
			shipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.MasterHouse };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001539");
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });

			var consignments = new DataObjectList<Shipment>();

			for (var i = 0; i < consignmentsOnShipment; i++)
			{
				var consignment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
				consignment.WayBillNumber = $"CON-{i}";
				consignment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
				consignments.Add(consignment);
			}
			shipment.SetSubShipmentCollection(() => consignments);

			return (consol, shipment);
		}

		(Shipment, Shipment) SetupExistingMAWBsAndUniversalShipmentsForConsolidateTesting(bool setupForTwinsBill, int houseBillsOnLatestBill, bool isImport, int consignmentsOnShipment)
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_UniqueConsignRef = "C00001194";
			forwardingConsol.JK_TransportMode = TransportModeCodeList.Codes.Air;
			forwardingConsol.JK_MasterBillNum = "PARENT BILL";

			var existingBillsOriginShipment = forwardingConsol.Shipments.AddNew();
			existingBillsOriginShipment.JS_UniqueConsignRef = "S00001538";
			existingBillsOriginShipment.JS_TransportMode = TransportModeCodeList.Codes.Air;

			var shipmentToRunConvertion = forwardingConsol.Shipments.AddNew();
			shipmentToRunConvertion.JS_UniqueConsignRef = "S00001539";
			shipmentToRunConvertion.JS_TransportMode = TransportModeCodeList.Codes.Air;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(existingBillsOriginShipment);
			HVLVConsignmentHeader.GetOrCreate(shipmentToRunConvertion);

			if (setupForTwinsBill)
			{
				var fullLoadTwinsBill = Factory.New<CusMAWB>();
				fullLoadTwinsBill.CM_MAWB = "C00001194";
				fullLoadTwinsBill.CM_FlightNo = "TT0001";
				fullLoadTwinsBill.CM_IsActive = true;
				fullLoadTwinsBill.CM_MasterHouseBill = "S00001538";

				var hb1 = fullLoadTwinsBill.ChildBills.AddNew();
				hb1.CS_MasterHouseBill = "aaaaa";
				hb1.CS_GoodsValue = 1;
				hb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb1.CS_JS = existingBillsOriginShipment.PK;
				var hb2 = fullLoadTwinsBill.ChildBills.AddNew();
				hb2.CS_MasterHouseBill = "aaaaa";
				hb2.CS_GoodsValue = 1;
				hb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb2.CS_JS = existingBillsOriginShipment.PK;
				var hb3 = fullLoadTwinsBill.ChildBills.AddNew();
				hb3.CS_MasterHouseBill = "aaaaa";
				hb3.CS_GoodsValue = 1;
				hb3.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb3.CS_JS = existingBillsOriginShipment.PK;
				var hb4 = fullLoadTwinsBill.ChildBills.AddNew();
				hb4.CS_MasterHouseBill = "aaaaa";
				hb4.CS_GoodsValue = 1;
				hb4.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb4.CS_JS = existingBillsOriginShipment.PK;
				var hb5 = fullLoadTwinsBill.ChildBills.AddNew();
				hb5.CS_MasterHouseBill = "aaaaa";
				hb5.CS_GoodsValue = 1;
				hb5.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb5.CS_JS = existingBillsOriginShipment.PK;

				consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(fullLoadTwinsBill);

				fullLoadTwinsBill.Logs.AddNew(AutoEvents.Transferred,
				[
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.ShipmentTypes.HighVolumeLowValue),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, "S00001538")
				]);
			}

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "C00001194";
			mawb.CM_FlightNo = "TT0001";
			mawb.CM_IsActive = true;
			mawb.CM_MasterHouseBill = "S00001538";

			mawb.Logs.AddNew(AutoEvents.Transferred,
			[
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.ShipmentTypes.HighVolumeLowValue),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, "S00001538")
			]);

			for (var i = 0; i < houseBillsOnLatestBill; i++)
			{
				var hb = mawb.ChildBills.AddNew();
				hb.CS_MasterHouseBill = i.ToString(WaybillStringFormat);
				hb.CS_HAWB = i.ToString(WaybillStringFormat);
				hb.CS_GoodsValue = 1;
				hb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;
				hb.CS_JS = existingBillsOriginShipment.PK;
			}

			consignmentHeader.GenPivotCollection.AddRelatedIfNotExist(mawb);

			var consol = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			consol.WayBillNumber = "C00001194";
			consol.VoyageFlightNo = "TT0001";
			consol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C00001194");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001539");

			var port1 = Factory.NewWithValidTestData<RefUNLOCO>();
			port1.Code = "NZTST";

			var port2 = Factory.NewWithValidTestData<RefUNLOCO>();
			port2.Code = "AABBB";

			if (isImport)
			{
				consol.PortOfLoading = UNLOCO.New(port2);
				consol.PortOfDischarge = UNLOCO.New(port1);
			}
			else
			{
				consol.PortOfLoading = UNLOCO.New(port1);
				consol.PortOfDischarge = UNLOCO.New(port2);
			}

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			shipment.WayBillNumber = "S00001539";
			shipment.ShipmentType = new CodeDescriptionPair { Code = ShipmentTypes.HighVolumeLowValue };
			shipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.MasterHouse };
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00001539");
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });

			var consignments = new DataObjectList<Shipment>();

			for (var i = 0; i < consignmentsOnShipment; i++)
			{
				var consignment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
				consignment.WayBillNumber = $"CON-{i}";
				consignment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
				consignments.Add(consignment);
			}
			shipment.SetSubShipmentCollection(() => consignments);

			return (consol, shipment);
		}

		public void TestCheckNoDuplicateWaybills_Sea_HVLShipment_WhenMultiShipmentRegistryIsFalseAndDuplicateFromNonHVLBill_ThrowException()
		{
			var numberOfConsignments = 2;
			var (universalConsol, universalShipment, forwardingConsol, forwardingShipment) = SetupHVLVShipmentAndUniversalShipment(null, "S00001539", TransportModeCodeList.Codes.Sea, isImport: true, numberOfConsignments);

			var dataObjectProvider = new UniversalCustomsDataObjectProvider();

			var oceanBillReaders = dataObjectProvider.GetNewCusSCAOceanBillDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory).Cast<CusSCAOceanBillDataObjectReader>();
			var oceanBills = oceanBillReaders.Select(x => x.ReadIntoBusinessObject()).ToList();

			var newHouseBill = oceanBills[0].HouseBills.AddNew();
			newHouseBill.CA_IsHVLV = false;
			newHouseBill.CA_HouseBill = "duplicate";

			AddHVLVConsignmentWithItemToUniversalShipment(universalShipment, "duplicate");

			var exception = AssertExceptionThrown<DataObjectValidationException>(() => dataObjectProvider.GetNewCusSCAOceanBillDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory).ToList());
			var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
DUPLICATE";
			AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
		}

		public void TestCheckNoDuplicateWaybills_Air_HVLShipment_WhenMultiShipmentRegistryIsFalseAndDuplicateFromNonHVLBill_ThrowException()
		{
			var numberOfConsignments = 2;
			var (universalConsol, universalShipment, forwardingConsol, forwardingShipment) = SetupHVLVShipmentAndUniversalShipment(null, "S00001539", TransportModeCodeList.Codes.Air, isImport: true, numberOfConsignments);

			var dataObjectProvider = new UniversalCustomsDataObjectProvider();

			var mawbReaders = dataObjectProvider.GetNewAirManifestDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).Cast<CusMAWBDataObjectReader>();
			var mawbs = mawbReaders.Select(x => x.ReadIntoBusinessObject()).ToList();

			mawbs[0].Logs.AddNew(AutoEvents.Transferred,
			[
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Core.Constants.ShipmentTypes.HighVolumeLowValue),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, "S00001539")
			]);

			var newHAWB = mawbs[0].ChildBills.AddNew();
			newHAWB.CS_IsHVLV = false;
			newHAWB.CS_HAWB = "duplicate";

			AddHVLVConsignmentWithItemToUniversalShipment(universalShipment, "duplicate");

			var exception = AssertExceptionThrown<DataObjectValidationException>(() => dataObjectProvider.GetNewAirManifestDataObjectReaders(universalConsol, universalShipment, new TestErrorLogger(), Factory, singleHAWBCheck: false).ToList());
			var expectedMessage = $@"{ExpectedDuplicateWaybillExceptionMessageBeginning}
DUPLICATE";
			AssertContainsExactLinesInAnyOrder("Exception contents", expectedMessage, exception.Message.Trim());
		}

		(Shipment, Shipment, ForwardingConsol, ForwardingShipment) SetupHVLVShipmentAndUniversalShipment(ForwardingConsol consolToAttachTo, string shipmentUniqueConsignRef, string transportModeCode = TransportModeCodeList.Codes.Air, bool isImport = true, int consignmentsOnShipment = 5, string consignmentWaybillPrefix = "CON-")
		{
			ForwardingConsol forwardingConsol;
			if (consolToAttachTo != null)
			{
				forwardingConsol = consolToAttachTo;
			}
			else
			{
				forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
				forwardingConsol.JK_UniqueConsignRef = "C00001194";
				forwardingConsol.JK_TransportMode = transportModeCode;
				forwardingConsol.JK_MasterBillNum = "PARENT BILL";
			}

			var forwardingShipment = forwardingConsol.Shipments.AddNew();
			forwardingShipment.JS_UniqueConsignRef = shipmentUniqueConsignRef;
			forwardingShipment.JS_TransportMode = transportModeCode;

			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(forwardingShipment);

			var universalConsol = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			universalConsol.WayBillNumber = forwardingConsol.JK_UniqueConsignRef;
			universalConsol.VoyageFlightNo = "TT0001";
			universalConsol.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master };
			universalConsol.DataContext = DataContextFactory.New();
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingConsol, forwardingConsol.JK_UniqueConsignRef);
			universalConsol.DataContext.AddDataSource(DataContextType.ForwardingShipment, forwardingShipment.JS_UniqueConsignRef);

			var port1 = Factory.NewWithValidTestData<RefUNLOCO>();
			port1.Code = "NZTST";

			var port2 = Factory.NewWithValidTestData<RefUNLOCO>();
			port2.Code = "AABBB";

			if (isImport)
			{
				universalConsol.PortOfLoading = UNLOCO.New(port2);
				universalConsol.PortOfDischarge = UNLOCO.New(port1);
			}
			else
			{
				universalConsol.PortOfLoading = UNLOCO.New(port1);
				universalConsol.PortOfDischarge = UNLOCO.New(port2);
			}

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			universalShipment.WayBillNumber = forwardingShipment.JS_UniqueConsignRef;
			universalShipment.ShipmentType = new CodeDescriptionPair { Code = ShipmentTypes.HighVolumeLowValue };
			universalShipment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, forwardingShipment.JS_UniqueConsignRef);
			universalConsol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { universalShipment });

			for (var i = 0; i < consignmentsOnShipment; i++)
			{
				AddHVLVConsignmentWithItemToUniversalShipment(universalShipment, $"{consignmentWaybillPrefix}{i}");
			}

			return (universalConsol, universalShipment, forwardingConsol, forwardingShipment);
		}

		void AddHVLVConsignmentWithItemToUniversalShipment(Shipment universalForwardingShipment, string waybillNumber)
		{
			if (universalForwardingShipment.SubShipmentCollection == null)
			{
				universalForwardingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			}

			var consignments = universalForwardingShipment.SubShipmentCollection;
			var consignment = new Shipment(DefaultDataObjectWriterStrategy.Instance);
			consignments.Add(consignment);

			consignment.WayBillNumber = waybillNumber;
			consignment.WayBillType = new WayBillType { Code = WayBillTypeList.Codes.House };
			consignment.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.HighVolumeLowValue };
			consignment.GoodsValueCurrency = new Currency { Code = "AUD" };

			consignment.SetPackingLineCollection(() =>
			{
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackType = new PackageType() { Code = "BBG" },
					Weight = 1.23M,
					GoodsDescription = "Bag of things",
					WeightUnit = new UnitOfWeight() { Code = "g" }
				};
				packingLine.SetPackedItemCollection(() => new List<PackedItem>()
					{
						new PackedItem()
						{
							CommercialInvoiceLineLink = 888
						}
					});
				return new DataObjectList<PackingLine>() { packingLine };
			});

			consignment.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>()
						{
							new CommercialInvoiceLine()
							{
								Link = 888,
								CustomsValue = 5.281M,
								Description = "Invoice Desc",
								HarmonisedCode = "1111.11.11.11",
								CustomsQuantity = 2M,
								CountryOfOrigin = new Country() { Code = "CA" },
								CustomsSupportingInformationCollection = new List<CustomsSupportingInformation>()
								{
									new CustomsSupportingInformation()
									{
										Country = new Country() { Code = "CN" },
										Tariff = "2222.22.22.22"
									}
								}
							}
						}))
				}
			};
		}

		public void TestGetNewAirManifestDataObjectWriter()
		{
			AssertType(typeof(CusMAWBDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<CusMAWB>()))));
		}

		public void TestGetNewAirManifestLineDataObjectWriter()
		{
			AssertType(typeof(CusHAWBDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewAirManifestLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<CusHAWB>())), null));
		}

		public void TestGetNewDeclarationDataObjectWriter()
		{
			AssertType(typeof(DeclarationDataObjectWriter), new UniversalCustomsDataObjectProvider().GetNewDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))));
		}

		public void TestGetNewCusSCAOceanBillDataObjectReaders()
		{
			var readers = new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectReaders(new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new Shipment(DefaultDataObjectWriterStrategy.TestInstance), new TestErrorLogger(), Factory);
			AssertType<CusSCAOceanBillDataObjectReader>(readers.Single());
		}

		public void TestGetNewCusSCAOceanBillDataObjectWriter()
		{
			AssertType<CusSCAOceanBillDataObjectWriter>(new UniversalCustomsDataObjectProvider().GetNewCusSCAOceanBillDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<JobDeclaration>()))));
		}

		public void TestTableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix, ""));
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			AssertNull(new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty));
		}
	}
}
