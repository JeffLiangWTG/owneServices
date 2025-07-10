using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class OCRManifestHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestOCRManifestHeaderWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.MasterBill;
			bill.ABL_BillNumber = "bill1";
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_OA_Carrier = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			header.AMA_JobReference = "MA0000001";
			header.AMA_Voyage = "NZ1234";
			header.AMA_E_DEP = new ZDateTime(2019, 05, 10);
			header.AMA_RL_NKPortOfDischarge = "AU123";
			header.AMA_RL_NKPortOfLoading = "NZ123";
			header.AMA_VesselName = "BRIGIT";
			header.DeliveryNotificationParty.DeliveryNotificationPartyName = "TEST PARTY";
			header.DeliveryNotificationParty.DeliveryNotificationPartyEmail = "test@email.com";
			header.DeliveryNotificationParty.DeliveryNotificationPartyPort = "NZAKL";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = header.PK;
			entryNumber.CE_ParentTable = header.TableName;
			entryNumber.CE_EntryType = Common.CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNumber.CE_EntryNum = "Entry Number";
			Factory.Save();
			var wrapper = new OCRManifestHeaderWrapper(header, new AdditionalMessageInformation(TSWTransactionTypes.Original, header.Factory));
			AssertEquals(ZString.Empty, wrapper.Consolidator.CustomsClientCode);
			AssertEquals(true, wrapper.IsSea);
			AssertEquals(false, wrapper.IsConsolidation);
			AssertEquals("Entry Number", wrapper.TSWReferenceNumber);
			AssertEquals("MA0000001", wrapper.SenderReferenceNumber);
			AssertEquals("bill1", wrapper.MasterBillNumber);
			AssertEquals("BRIGIT", wrapper.CraftName);
			AssertEquals("7326659", wrapper.LloydsNo);
			AssertEquals("NZ1234", wrapper.VoyageNo);
			AssertEquals("NZ1234", wrapper.FlightNo);
			AssertEquals(new ZDateTime(2019, 05, 10), wrapper.DepartureDate);
			AssertEquals("AU", wrapper.RoutingCountryCodes.ElementAt(0));
			AssertEquals(ZString.Empty, wrapper.Carrier.CustomsClientCode);
			AssertEquals("NZ123", wrapper.PortOfDeparture);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			wrapper = new OCRManifestHeaderWrapper(header, new AdditionalMessageInformation(TSWTransactionTypes.Original, header.Factory));
			AssertEquals(ZString.Empty, wrapper.Consolidator.CustomsClientCode);
			AssertEquals(false, wrapper.IsSea);
			AssertEquals(false, wrapper.IsConsolidation);
			AssertEquals("Entry Number", wrapper.TSWReferenceNumber);
			AssertEquals("MA0000001", wrapper.SenderReferenceNumber);
			AssertEquals("bill1", wrapper.MasterBillNumber);
			AssertEquals(ZString.Empty, wrapper.CraftName);
			AssertEquals(ZString.Empty, wrapper.LloydsNo);
			AssertEquals(ZString.Empty, wrapper.VoyageNo);
			AssertEquals(ZString.Empty, wrapper.FlightNo);
			AssertEquals(new ZDateTime(2019, 05, 10), wrapper.DepartureDate);
			AssertEquals("AU", wrapper.RoutingCountryCodes.ElementAt(0));
			AssertEquals(ZString.Empty, wrapper.Carrier.CustomsClientCode);
			AssertEquals("NZ123", wrapper.PortOfDeparture);
			var ocrHeader = (IOutwardCargoReportHeader)wrapper;
			AssertEquals("NotifyPartyName", "TEST PARTY", ocrHeader.NotifyPartyName);
			AssertEquals("NotifyPartyEmail", "test@email.com", ocrHeader.NotifyPartyEmail);
			AssertEquals("Notify Party Code 0 should be Port code", "NZAKL", ocrHeader.NotifyPartyCodes.ToArray()[0]);
		}

		public void TestNotifyPartyCode()
		{
			var notifyOrg = OrgHeader.New(Factory);
			notifyOrg.OH_Code = "NZAKLBOND";
			notifyOrg.OH_FullName = "AUCKLAND BOND STORE";
			notifyOrg.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			notifyOrg.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			notifyOrg.OH_RL_NKClosestPort = "NZAKL";
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.MasterBill;
			bill.ABL_BillNumber = "bill1";
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_OA_Carrier = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			header.AMA_JobReference = "MA0000001";
			header.AMA_Voyage = "NZ1234";
			header.AMA_E_DEP = new ZDateTime(2019, 05, 10);
			header.AMA_RL_NKPortOfDischarge = "AU123";
			header.AMA_RL_NKPortOfLoading = "NZ123";
			header.AMA_VesselName = "BRIGIT";
			var address = Factory.New<IOrgAddress>();
			address.OA_OH = notifyOrg.PK;
			address.OA_Address1 = "Address 1";
			address.OA_City = "Sydney";
			header.DeliveryNotificationParty.E2_OA_DeliveryNotificationParty = address.PK;
			var wrapped = new OCRManifestHeaderWrapper(header, null);
			var ocrHeader = (IOutwardCargoReportHeader)wrapped;
			AssertEquals("Notify Party Code should be empty", false, ocrHeader.NotifyPartyCodes.Any());
			header.DeliveryNotificationParty.DeliveryNotificationPartyPort = "NZAKL";
			wrapped = new OCRManifestHeaderWrapper(header, null);
			ocrHeader = wrapped;
			AssertEquals("Notify Party Code 0 should be Port code", "NZAKL", ocrHeader.NotifyPartyCodes.ToArray()[0]);
			var ccpCusCode = notifyOrg.CustomsCodes.AddNew();
			ccpCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			ccpCusCode.OK_OA_PremisesAddress = address.PK;
			ccpCusCode.OK_CustomsRegNo = "79458278";
			ccpCusCode.OK_RN_NKCodeCountry = "NZ";
			wrapped = new OCRManifestHeaderWrapper(header, null);
			ocrHeader = wrapped;
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", ocrHeader.NotifyPartyCodes.ToArray()[0]);
			var ccdCusCode = notifyOrg.CustomsCodes.AddNew();
			ccdCusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			ccdCusCode.OK_OA_PremisesAddress = address.PK;
			ccdCusCode.OK_CustomsRegNo = "CC001";
			ccdCusCode.OK_RN_NKCodeCountry = "NZ";
			wrapped = new OCRManifestHeaderWrapper(header, null);
			ocrHeader = wrapped;
			var partyCodes = ocrHeader.NotifyPartyCodes.ToArray();
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", partyCodes[0]);
			AssertEquals("Notify Party Code 1 should be CCD code", "CC001", partyCodes[1]);
			AssertEquals("Notify Party Code 2 should be Port code", "NZAKL", partyCodes[2]);
			var atfCusCode = notifyOrg.CustomsCodes.AddNew();
			atfCusCode.OK_CodeType = OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility;
			atfCusCode.OK_OA_PremisesAddress = address.PK;
			atfCusCode.OK_CustomsRegNo = "1234Z";
			atfCusCode.OK_RN_NKCodeCountry = "NZ";
			wrapped = new OCRManifestHeaderWrapper(header, null);
			ocrHeader = wrapped;
			partyCodes = ocrHeader.NotifyPartyCodes.ToArray();
			AssertEquals("Notify Party Code 0 should be CCP code", "79458278", partyCodes[0]);
			AssertEquals("Notify Party Code 1 should now be ATF code", "1234Z", partyCodes[1]);
			AssertEquals("Notify Party Code 2 should be CCD code", "CC001", partyCodes[2]);
			AssertEquals("Notify Party Code 3 should be Port code", "NZAKL", partyCodes[3]);
		}
	}
}
