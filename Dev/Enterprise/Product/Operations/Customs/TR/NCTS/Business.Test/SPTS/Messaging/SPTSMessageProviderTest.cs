using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq.Protected;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSMessageProviderTest : TestCaseWithFactory
	{
		public void TestSPTSMesaage()
		{
			var header = Factory.New<SPTSHeader>();
			header.BH_JobReference = "SPTS0001";

			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			EDIMessage messageHeader = mockMessage.Object;

			messageHeader.EM_LinkUniqueID = header.PK;
			messageHeader.EM_LinkTable = SPTSHeader.Schema.TableName;
			messageHeader.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			messageHeader.EM_MessageType = TRMessageTypes.Codes.TSP;
			messageHeader.EM_IsTestMessage = TRCustomsDataRegistry.Instance.IsTRTestingSystem;
			messageHeader.EM_MessageOwner = "CZH";
			messageHeader.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageHeader.EM_Status = EDIMessage.Status.Queued;
			messageHeader.EM_ApplicationReference = "ULU-SPTS0001";
			header.Messages.Add(messageHeader);

			Factory.Save();

			var provider = new SPTSMessageProvider(header);
			var iheader = provider as ISPTS;

			AssertNotNull(iheader.Messages);
			AssertNotNull(iheader.Parent);
			AssertEquals("SPTS0001", iheader.JobReference);

			var message = iheader.Messages[0] as EDIMessage;

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
				AssertEquals(TRMessageTypes.Codes.TSP, message.EM_MessageType);
				AssertEquals(TRCustomsDataRegistry.Instance.IsTRTestingSystem, message.EM_IsTestMessage);
				AssertEquals("CZH", message.EM_MessageOwner);
				AssertEquals(EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("ULU-SPTS0001", message.EM_ApplicationReference);
				AssertEquals("CusInBondHeader", message.EM_LinkTable);
				AssertEquals(header.PK, message.EM_LinkUniqueID);
			});
		}

		public void TestInterfaceMembers()
		{
			var currentUser = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			GlbStaff.CurrentUser.GS_Code = "CZH";
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "123456789";
			currentUser.GP_UserID = "12345678901";

			var header = Factory.New<SPTSHeader>();

			header.BH_JobReference = "ULU1987";
			header.RegistrationNumber = "0123456789";
			header.BH_VoyageNumber = "9988";
			header.BH_SailingDate = ZDate.Today;

			var movementHeader = SPTSDepartureMovementHeader.LoadOrCreate(header, "D");

			var orgWithVAT = Factory.New<OrgHeader>();
			orgWithVAT.OH_Code = "orgVATCode";
			orgWithVAT.OH_FullName = "orgVATName";

			var addressWithVAT = orgWithVAT.MainAddress;
			addressWithVAT.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456");

			movementHeader.BM_PortOfPresentationCode = "TR041500";
			movementHeader.BM_DestinationPortCode = "TR041500";
			movementHeader.BM_InlandTransportMode = "SEA";
			movementHeader.BM_OA_InBondCarrier = orgWithVAT.MainAddress.PK;

			var sptpsBill = header.Bills.AddNew();
			sptpsBill.B0_MasterBillNumber = "001";
			sptpsBill.B0_ReferenceQualifier = "TR";
			sptpsBill.B0_ReferenceID = "9988776655";
			sptpsBill.B0_ServiceType = "Y";
			var billContainer = sptpsBill.SPTSBillContainers.AddNew();
			billContainer.BC_ContainerNum = "ABCD 123456-7";

			var sPTSulds = header.HeaderContainers.AddNew();
			sPTSulds.BC_ContainerNum = "ABCD 123456-7";

			ISPTS provider = new SPTSMessageProvider(header);

			CombineAssertions(() =>
			{
				AssertEquals("BH_JobReference", "ULU1987", provider.JobReference);
				AssertEquals("RegistrationNoToBeUpdated", ZString.Empty, provider.RegistrationNoToBeUpdated);
				AssertEquals("BusinessRegNo", "123456789", provider.BusinessRegNo);
				AssertEquals("CarrierBusinessRegNo", "123456", provider.CarrierBusinessRegNo);
				AssertEquals("BH_VoyageNumber", "9988", provider.VoyageNumber);
				AssertEquals("BH_VoyageDate", ZDate.Today, provider.VoyageDate);
				AssertEquals("BM_PortOfPresentationCode", "041500", provider.PortOfPresentationDCode);
				AssertEquals("BM_DestinationPortCode", "041500", provider.DestinationPortDCode);
				AssertEquals("BM_InlandTransportMode", "10", provider.TransportType);
				AssertEquals("XmlRefId", header.PK.ToString(), provider.XmlRefId);
				AssertEquals("UserID", "12345678901", provider.UserID);

				foreach (var bill in provider.SPTSBills)
				{
					AssertEquals("BillOrderNo", 1, bill.BillOrderNo);
					AssertEquals("B0_MasterBillNumber", "001", bill.BillNumber);
					AssertEquals("B0_ReferenceQualifier", "TR", bill.DeclarationType);
					AssertEquals("B0_ReferenceID", "9988776655", bill.DeclarationNo);
					AssertEquals("B0_ServiceType", "EVET", bill.IsSubType);

					foreach (var billLine in bill.SPTSBillLines)
					{
						AssertEquals("LineOrderNo", 1, billLine.LineOrderNo);
						AssertEquals("BC_ContainerNum", "ABCD 123456-7", billLine.LineContainerNo);
					}
				}

				foreach (var ulds in provider.SPTSUlds)
				{
					AssertEquals("UldOrderNo", 1, ulds.UldOrderNo);
					AssertEquals("BC_ContainerNum", "ABCD 123456-7", ulds.UldNumber);
				}
			});
		}
	}
}
