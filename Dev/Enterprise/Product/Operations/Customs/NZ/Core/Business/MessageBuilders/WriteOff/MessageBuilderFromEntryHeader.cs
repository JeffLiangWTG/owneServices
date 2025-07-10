using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Edifact.D98A.Elements;
using Enterprise.Edifact.D98A.Messages.CUSCAR;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff
{
	public class MessageBuilderFromEntryHeader : EdifactMessageBuilderFromEntryHeader
	{
		public MessageBuilderFromEntryHeader(Declaration.CusEntryHeader entryHeader, ECIMessageGenerator.MessageTypes messageType)
			: base(entryHeader)
		{
			this.entryHeader = (CusEntryHeader)entryHeader;
			MessageType = messageType;
			MainDeclaration = entryHeader.Declaration;
		}
		readonly CusEntryHeader entryHeader;
		protected readonly ECIMessageGenerator.MessageTypes MessageType;
		protected readonly JobDeclaration MainDeclaration;

		protected override Edifact.Auto.SegmentGroup GetNewEDIFACTMessage()
		{
			return new CUSCARMessage();
		}

		protected override void SetMessageType()
		{
			message.EM_MessageType = Declaration.NZCMessage.MessageTypes.ECIWriteOff.MessageType;
		}

		protected override void SetMessageSubType()
		{
			switch (MessageType)
			{
				case ECIMessageGenerator.MessageTypes.CancelECI:
					message.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Cancellation;
					break;
				case ECIMessageGenerator.MessageTypes.Original:
					message.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.Original;
					break;
				case ECIMessageGenerator.MessageTypes.ReplaceHeader:
					message.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.ReplaceHeader;
					break;
				case ECIMessageGenerator.MessageTypes.ReplaceConsignment:
					message.EM_MessageSubType = NZCMessage.MessageTypes.ECIWriteOff.MessageSubTypes.ReplaceLines;
					break;
			}
		}

		protected override void GenerateGroup0()
		{
			CUSCARMessage group0 = (CUSCARMessage)EDIFACTMessage;
			ECIMessageGenerator generator = new ECIMessageGenerator(group0, MessageType);

			generator.GenerateGroup0UNH(entryHeader.EntryNumber);
			generator.GenerateGroup0BGM();

			if (MessageType == ECIMessageGenerator.MessageTypes.Original || MessageType == ECIMessageGenerator.MessageTypes.ReplaceHeader)
			{
				generator.GenerateGroup2(MainDeclaration.ShippingLine, MainDeclaration.Forwarder);
			}

			generator.GenerateGroup0FTX(entryHeader.CH_CustomsMessageRemarks);

			if (MessageType == ECIMessageGenerator.MessageTypes.Original || MessageType == ECIMessageGenerator.MessageTypes.ReplaceHeader)
			{
				ZString transportModeCoded = MainDeclaration.IsSea ? "1" : "4";
				ZString vesselName = MainDeclaration.IsSea && MainDeclaration.Vessel != null ? MainDeclaration.Vessel.RV_Code : MainDeclaration.JE_VesselName;
				ZString barrierPort = MainDeclaration.IsExport ? MainDeclaration.JE_RL_NKPortOfLoading : MainDeclaration.JE_RL_NKPortOfArrival;
				generator.GenerateGroup4(transportModeCoded, vesselName, MainDeclaration.JE_VoyageFlightNo, barrierPort, MainDeclaration.BarrierDate); // M1

				generator.GenerateGroup0GIS((MainDeclaration.IsImport ? "10" : "40"), CodeListQualifierList.CustomsDeclarationType);
				generator.GenerateGroup0GIS("N", CodeListQualifierList.CustomsIndicator); // Always send indicator to say it's a part manifest to allow additions to the manifest later.

				if (MainDeclaration.JE_TransportMode != JobTransportModeList.Codes.Air)
				{
					foreach (CusContainer container in MainDeclaration.CusContainers)
					{
						generator.GenerateGroup5(container.CO_ContainerNumber, container.CO_ContainerSize, container.CO_FCL_LCL_AIR, "N", "N", ""); // C9999
					}
				}
			}

			if (MessageType == ECIMessageGenerator.MessageTypes.Original || MessageType == ECIMessageGenerator.MessageTypes.ReplaceConsignment)
			{
				generator.GenerateGroup0CNT(GetConsignmentCount(), ControlQualifierList.TotalNumberOfConsignments);
			}

			if (MainDeclaration.JE_TransportMode != JobTransportModeList.Codes.Air && (MessageType == ECIMessageGenerator.MessageTypes.Original || MessageType == ECIMessageGenerator.MessageTypes.ReplaceHeader))
			{
				ZInt containerCount = MainDeclaration.CusContainers.Count;
				if (!containerCount.IsEmpty)
				{
					generator.GenerateGroup0CNT(containerCount, ControlQualifierList.TotalNumberOfEquipment);
				}
			}

			if (MessageType == ECIMessageGenerator.MessageTypes.Original || MessageType == ECIMessageGenerator.MessageTypes.ReplaceConsignment)
			{
				GenerateGroup7ForAllConsignments(generator);
			}

			generator.GenerateGroup0UNT();
		}

		#region Virtual Methods Overridden When Sending ECI Manifest
		protected override void SetParentMessagingStatusAfterMessagePosting()
		{
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			MainDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			MainDeclaration.JE_EDITransmitDate = MainDeclaration.CachedTodaysDate;
			MainDeclaration.JE_EntrySubmittedDate = MainDeclaration.CachedTodaysDate;
			MainDeclaration.LogCustomsCommencedIfNeeded();
		}

		protected virtual void GenerateGroup7ForAllConsignments(ECIMessageGenerator generator)
		{
			generator.GenerateGroup7(MainDeclaration, "1");
		}

		protected virtual ZInt GetConsignmentCount()
		{
			return 1;
		}
		#endregion
	}
}
