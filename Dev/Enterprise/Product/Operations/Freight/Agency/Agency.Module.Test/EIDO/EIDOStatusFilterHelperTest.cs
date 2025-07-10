using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class EIDOStatusFilterHelperTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			SetupContainers();
			Factory.Save();
			Asserter.AddFieldOfInterest(AgencyShipmentContainer.Schema.JC_ImportReleaseOrderStatus);
			Asserter.AssertMatches("Empty", EIDOStatusFilterHelper.GetEIDOStatusFilter(""), container_non, container_O_Acc, container_O_Fai, container_O_Que, container_O_Rec, container_O_Rej, container_O_Snt, container_C_Acc, container_C_Fai, container_C_Que, container_C_Rec, container_C_Rej, container_C_Snt);
			Asserter.AssertMatches("Accepted", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.Accepted), container_O_Rec);
			Asserter.AssertMatches("Acknowledged", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.Acknowledged), container_O_Acc);
			Asserter.AssertMatches("Failed", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.Failed), container_O_Fai, container_C_Fai);
			Asserter.AssertMatches("NotSent", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.NotSent), container_non, container_C_Acc, container_C_Rec);
			Asserter.AssertMatches("PendingResponse", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.PendingResponse), container_O_Que, container_O_Snt, container_C_Que, container_C_Snt);
			Asserter.AssertMatches("Rejected", EIDOStatusFilterHelper.GetEIDOStatusFilter(EIDOFilterList.Codes.Rejected), container_O_Rej, container_C_Rej);
		}

		#region Implementation
		BillOfLadingContainer container_non;
		BillOfLadingContainer container_O_Acc;
		BillOfLadingContainer container_O_Fai;
		BillOfLadingContainer container_O_Que;
		BillOfLadingContainer container_O_Rec;
		BillOfLadingContainer container_O_Rej;
		BillOfLadingContainer container_O_Snt;
		BillOfLadingContainer container_C_Acc;
		BillOfLadingContainer container_C_Fai;
		BillOfLadingContainer container_C_Que;
		BillOfLadingContainer container_C_Rec;
		BillOfLadingContainer container_C_Rej;
		BillOfLadingContainer container_C_Snt;
		void SetupContainers()
		{
			container_non = AddContainer("non");
			container_O_Acc = AddContainer("O_Acc");
			container_O_Fai = AddContainer("O_Fai");
			container_O_Que = AddContainer("O_Que");
			container_O_Rec = AddContainer("O_Rec");
			container_O_Rej = AddContainer("O_Rej");
			container_O_Snt = AddContainer("O_Snt");
			container_C_Acc = AddContainer("C_Acc");
			container_C_Fai = AddContainer("C_Fai");
			container_C_Que = AddContainer("C_Que");
			container_C_Rec = AddContainer("C_Rec");
			container_C_Rej = AddContainer("C_Rej");
			container_C_Snt = AddContainer("C_Snt");
			ZDateTime now = ZDateTime.Now;
			int offset = 0;
			ResponseMessage(SendEIDOMessage(container_O_Acc, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			SendEIDOMessage(container_O_Fai, EIDOMessageFunction.Original, now, offset++).EM_Status = EIDOMessage.Status.Failed;
			SendEIDOMessage(container_O_Que, EIDOMessageFunction.Original, now, offset++).EM_Status = EIDOMessage.Status.Queued;
			ResponseMessage(SendEIDOMessage(container_O_Rec, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Received);
			ResponseMessage(SendEIDOMessage(container_O_Rej, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Rejected);
			SendEIDOMessage(container_O_Snt, EIDOMessageFunction.Original, now, offset++).EM_Status = EIDOMessage.Status.Sent;
			ResponseMessage(SendEIDOMessage(container_C_Acc, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Fai, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Que, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Rec, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Rej, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Snt, EIDOMessageFunction.Original, now, offset++), EIDOResponseType.Accepted);
			ResponseMessage(SendEIDOMessage(container_C_Acc, EIDOMessageFunction.Cancelation, now, offset++), EIDOResponseType.Accepted);
			SendEIDOMessage(container_C_Fai, EIDOMessageFunction.Cancelation, now, offset++).EM_Status = EIDOMessage.Status.Failed;
			SendEIDOMessage(container_C_Que, EIDOMessageFunction.Cancelation, now, offset++).EM_Status = EIDOMessage.Status.Queued;
			ResponseMessage(SendEIDOMessage(container_C_Rec, EIDOMessageFunction.Cancelation, now, offset++), EIDOResponseType.Received);
			ResponseMessage(SendEIDOMessage(container_C_Rej, EIDOMessageFunction.Cancelation, now, offset++), EIDOResponseType.Rejected);
			SendEIDOMessage(container_C_Snt, EIDOMessageFunction.Cancelation, now, offset++).EM_Status = EIDOMessage.Status.Sent;
		}

		BillOfLadingContainer AddContainer(string containerNumber)
		{
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_UniqueConsignRef = containerNumber;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Asserter.AddToScope(container);
			return container;
		}

		EIDOMessage SendEIDOMessage(AgencyShipmentContainer container, EIDOMessageFunction function, ZDateTime now, int offset)
		{
			EIDOMessage message = EIDOMessage.New(container, function, EDIMessage.MessageNumberPlaceHolder);
			message.EM_SystemCreateTimeUtc = now.AddMinutes(offset);
			return message;
		}

		void ResponseMessage(EIDOMessage sentMessage, EIDOResponseType responseType)
		{
			EIDOMessage message = Factory.New<EIDOMessage>();
			message.EM_ApplicationCode = EIDOMessage.ApplicationCodes.EIDO;
			message.EM_ReceiveTransmit = EIDOMessage.Direction.Receive;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_LinkTable = sentMessage.EM_LinkTable;
			message.EM_LinkUniqueID = sentMessage.EM_LinkUniqueID;
			message.EM_Status = EIDOMessage.Status.Recognised;
			switch (responseType)
			{
				case EIDOResponseType.Accepted:
					sentMessage.EM_Status = EIDOMessage.Status.Acknowledged;
					break;
				case EIDOResponseType.Received:
					sentMessage.EM_Status = EIDOMessage.Status.Received;
					break;
				case EIDOResponseType.Rejected:
					sentMessage.EM_Status = EIDOMessage.Status.Rejected;
					break;
			}
		}

		FilterStripAsserter<BillOfLadingContainer> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<BillOfLadingContainer>(Factory, (c) => c.JC_ContainerNum));
			}
		}

		FilterStripAsserter<BillOfLadingContainer> asserter;
		#endregion
	}
}
