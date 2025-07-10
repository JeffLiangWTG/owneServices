using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CargoManifestStatusQueryMessageBuilder
	{
		public CargoManifestStatusQueryMessageBuilder(ICargoManifestStatusQueryHeader messageSendingHeader, ICargoManifestQuerySendingObject sendingObject)
			: this(sendingObject.Factory, sendingObject.ActionCode, new[] { sendingObject }, messageSendingHeader.ProcessingPortCode, messageSendingHeader.ProcessingOfficeCode)
		{
			if (messageSendingHeader is JobDeclaration declaration)
			{
				this.processingPortCode = declaration.ProcessingPortCodeForQuery;
			}

			this.entryFilerCode = messageSendingHeader.EntryFilerCode;
			this.isACEQuery = messageSendingHeader.IsACEQuery;
			this.updateEntryWithResults = sendingObject.UpdateEntryWithResults;
		}
		readonly ZString entryFilerCode;
		readonly ZBool updateEntryWithResults;

		public CargoManifestStatusQueryMessageBuilder(BusinessObjectFactory factory, ZString actionCode, IEnumerable<ICargoManifestQuerySendingObject> sendingObjects, ZString processingPortCode, ZString processingOfficeCode)
		{
			this.factory = factory;
			this.actionCode = actionCode;
			this.sendingObjects = sendingObjects;
			this.processingPortCode = processingPortCode;
			this.processingOfficeCode = processingOfficeCode;
			this.isACEQuery = true;
		}
		readonly BusinessObjectFactory factory;
		readonly string actionCode;
		readonly IEnumerable<ICargoManifestQuerySendingObject> sendingObjects;
		readonly ZString processingPortCode;
		readonly ZString processingOfficeCode;
		readonly ZBool isACEQuery;

		public MQEDIMessage GenerateMessages()
		{
			Messaging.Business.BlockControlGenerator block = null;

			if (ShouldSendCQ(isACEQuery))
			{
				block = new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch.GB_GC.ToGuid(), processingPortCode, processingOfficeCode);
				block.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			}
			else
			{
				block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
				block.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus;
			}

			GenerateBlocks(block);

			var message = block.CreateMessage<MQEDIMessage>(factory);
			foreach (var item in sendingObjects)
			{
				item.LinkToMessage(message);
			}

			if (updateEntryWithResults)
			{
				message.EM_ApplicationReference = "UpdateEntryWithResults";
			}

			message.EM_MessageSubType = actionCode switch
			{
				CargoManifestStatusQueryActionList.Codes.AIR => EM_MessageSubTypeList.Codes.CargoManifestAirQuery,
				CargoManifestStatusQueryActionList.Codes.Entry => EM_MessageSubTypeList.Codes.CargoManifestEntryQuery,
				CargoManifestStatusQueryActionList.Codes.HAWB => EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery,
				CargoManifestStatusQueryActionList.Codes.MAWB => EM_MessageSubTypeList.Codes.CargoManifestMAWBQuery,
				CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill => EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery,
				CargoManifestStatusQueryActionList.Codes.InBond => EM_MessageSubTypeList.Codes.CargoManifestInBondQuery,
				_ => string.Empty
			};

			return message;
		}

		bool ShouldSendCQ(bool isACECargoCertificationMode)
		{
			return isACECargoCertificationMode || actionCode != CargoManifestStatusQueryActionList.Codes.Entry;
		}

		void GenerateBlocks(Messaging.Business.BlockControlGenerator block)
		{
			var shouldSendCQ = ShouldSendCQ(isACEQuery);
			foreach (var sendingObject in sendingObjects)
			{
				ICargoManifestEntryReleaseStatusQuery r1 = shouldSendCQ ? new ACEQWR1() : new CMQR1();

				if (sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.Entry)
				{
					r1.EntryFilerCode = entryFilerCode;
					r1.EntryNumber = sendingObject.EntryOrInBondNumber.Left(9).PadLeft(9);//When serialised, it should be padded right with space
				}
				else if (sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.InBond)
				{
					r1.InbondNumber = sendingObject.EntryOrInBondNumber.KeepAlphanumericCharacters().Left(12);//When serialised, it should be padded right with space
				}
				else if (sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill)
				{
					r1.IssuerCodeOfBillOfLadingNumber = sendingObject.BillIssuerCode;
					r1.BillOfLadingNumber = sendingObject.HouseBillNumber.IsEmpty ? sendingObject.MasterBillNumber.KeepAlphanumericCharacters().Left(12) : sendingObject.HouseBillNumber.KeepAlphanumericCharacters().Left(12);
					r1.RequestForRelatedBOLIndicator = sendingObject.RequestForRelatedBOL ? "Y" : "";
				}
				else if (sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.MAWB || sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.HAWB || sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.AIR)
				{
					r1.AirWaybillNumber = sendingObject.MasterBillNumber.KeepAlphanumericCharacters().Left(11);
					r1.HouseAirWaybillNumber = sendingObject.HouseBillNumber.KeepAlphanumericCharacters().Left(12);
				}

				if (isACEQuery && sendingObject.ActionCode == CargoManifestStatusQueryActionList.Codes.Entry)
				{
					r1.RequestForBillOfLadingAndEntryDataIndicator = USCustomsDataRegistry.Instance.RequestForBillAndEntryData.Value ? "Y" : "";
					//Limit Output Option indicator is always empty. JY: If it is 1 or 2, histories of disposition details will be returned. 
					//Then the implication is that we have to filter out the historical data that is no longer valid when we display details from query response messages on the Status tab.
					//And flag 'hasEarlierReleaseDisposition' determined by the 'firstReleaseBlock' will always be true if historical data is sent in the chronological order.
				}

				if (r1 is ACEQWR1)
				{
					var outputOption = sendingObject.LimitOutputOption.EqualsIgnoringCase(LimitOutputCodeList.Codes._1Last5Results) ? (ZString)"1" :
						sendingObject.LimitOutputOption.EqualsIgnoringCase(LimitOutputCodeList.Codes._2AllAvailableResults) ? (ZString)"2" : ZString.Empty;

					r1.LimitOutputOption = outputOption;
				}

				block.AddMessageBlock((MessageBlock)r1);
			}
		}
	}
}
