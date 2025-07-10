using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class Dossier : DocDataObject, IDataSourceProvider
	{
		public Dossier(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get => shipmentNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
				{
					Validate(ShipmentNumberInfo);
				}
			}
		}
		ZString shipmentNumber;

		public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

		#endregion

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
					Validate(ConsolNumberInfo);
				}
			}
		}
		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion

		#region CarrierBookingReference

		public ZString CarrierBookingReference
		{
			get => carrierBookingReference;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierBookingReferenceInfo, ref carrierBookingReference, value))
				{
					Validate(CarrierBookingReferenceInfo);
				}
			}
		}
		ZString carrierBookingReference;

		public ZPropertyInfo CarrierBookingReferenceInfo => GetZPropertyInfo(nameof(CarrierBookingReference));

		#endregion

		#region BillOfLading

		public ZString BillOfLading
		{
			get => billOfLading;
			set
			{
				if (SetNonPersistentPropertyValue(BillOfLadingInfo, ref billOfLading, value))
				{
					Validate(BillOfLadingInfo);
				}
			}
		}
		ZString billOfLading;

		public ZPropertyInfo BillOfLadingInfo => GetZPropertyInfo(nameof(BillOfLading));

		#endregion

		#region ATP

		public ZString ATP
		{
			get => atp;
			set
			{
				if (SetNonPersistentPropertyValue(ATPInfo, ref atp, value))
				{
					Validate(ATPInfo);
				}
			}
		}

		ZString atp;

		public ZPropertyInfo ATPInfo => GetZPropertyInfo(nameof(ATP));

		#endregion

		#region OTC

		public ZString OTC
		{
			get => otc;
			set
			{
				if (SetNonPersistentPropertyValue(OTCInfo, ref otc, value))
				{
					Validate(OTCInfo);
				}
			}
		}

		ZString otc;

		public ZPropertyInfo OTCInfo => GetZPropertyInfo(nameof(OTC));

		#endregion

		#region ContainerMode

		public ICodeDescription ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ICodeDescription containerMode;

		#endregion

		#region ShipmentType

		public ICodeDescription ShipmentType
		{
			get => shipmentType;
			set => shipmentType = SetChild(shipmentType, value);
		}

		ICodeDescription shipmentType;

		#endregion

		#region Notes

		public ZString Notes
		{
			get => notes;
			set
			{
				if (SetNonPersistentPropertyValue(NotesInfo, ref notes, value))
				{
					Validate(NotesInfo);
				}
			}
		}

		ZString notes;

		public ZPropertyInfo NotesInfo => GetZPropertyInfo(nameof(Notes));

		#endregion

		#region SendingParty

		public Address SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}

		Address sendingParty;

		#endregion SendingParty

		#region SendingPartySON

		public RegistrationNumber SendingPartySON
		{
			get => sendingPartySON;
			set => sendingPartySON = SetChild(sendingPartySON, value);
		}

		RegistrationNumber sendingPartySON;

		#endregion

		#region SendingPartyCI5

		public RegistrationNumber SendingPartyCI5
		{
			get => sendingPartyCI5;
			set => sendingPartyCI5 = SetChild(sendingPartyCI5, value);
		}

		RegistrationNumber sendingPartyCI5;

		#endregion

		#region FormattedSendingPartyProviderID

		public ZString FormattedSendingPartyProviderID
		{
			get => formattedSendingPartyProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedSendingPartyProviderIDInfo, ref formattedSendingPartyProviderID, value))
				{
					Validate(FormattedSendingPartyProviderIDInfo);
				}
			}
		}
		ZString formattedSendingPartyProviderID;

		public ZPropertyInfo FormattedSendingPartyProviderIDInfo => GetZPropertyInfo(nameof(FormattedSendingPartyProviderID));

		#endregion

		#region Agent

		public Address Agent
		{
			get => agent;
			set => agent = SetChild(agent, value);
		}

		Address agent;

		#endregion Agent

		#region AgentSOA

		public RegistrationNumber AgentSOA
		{
			get => agentSOA;
			set => agentSOA = SetChild(agentSOA, value);
		}

		RegistrationNumber agentSOA;

		#endregion

		#region AgentCI5

		public RegistrationNumber AgentCI5
		{
			get => agentCI5;
			set => agentCI5 = SetChild(agentCI5, value);
		}

		RegistrationNumber agentCI5;

		#endregion

		#region FormattedAgentProviderID

		public ZString FormattedAgentProviderID
		{
			get => formattedAgentProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedAgentProviderIDInfo, ref formattedAgentProviderID, value))
				{
					Validate(FormattedAgentProviderIDInfo);
				}
			}
		}
		ZString formattedAgentProviderID;

		public ZPropertyInfo FormattedAgentProviderIDInfo => GetZPropertyInfo(nameof(FormattedAgentProviderID));

		#endregion

		#region FileComplete

		public ZBool FileComplete
		{
			get => fileComplete;
			set
			{
				if (SetNonPersistentPropertyValue(FileCompleteInfo, ref fileComplete, value))
				{
					Validate(FileCompleteInfo);
				}
			}
		}

		ZBool fileComplete;

		public ZPropertyInfo FileCompleteInfo => GetZPropertyInfo(nameof(FileComplete));

		#endregion

		#region UniqueDeclaration

		public ZBool UniqueDeclaration
		{
			get => uniqueDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(UniqueDeclarationInfo, ref uniqueDeclaration, value))
				{
					Validate(UniqueDeclarationInfo);
				}
			}
		}

		ZBool uniqueDeclaration;

		public ZPropertyInfo UniqueDeclarationInfo => GetZPropertyInfo(nameof(UniqueDeclaration));

		#endregion

		#region MultipleDeclaration

		public ZBool MultipleDeclaration
		{
			get => multipleDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(MultipleDeclarationInfo, ref multipleDeclaration, value))
				{
					Validate(MultipleDeclarationInfo);
				}
			}
		}

		ZBool multipleDeclaration;

		public ZPropertyInfo MultipleDeclarationInfo => GetZPropertyInfo(nameof(MultipleDeclaration));

		#endregion

		#region LastDeclaration

		public ZBool LastDeclaration
		{
			get => lastDeclaration;
			set
			{
				if (SetNonPersistentPropertyValue(LastDeclarationInfo, ref lastDeclaration, value))
				{
					Validate(LastDeclarationInfo);
				}
			}
		}

		ZBool lastDeclaration;

		public ZPropertyInfo LastDeclarationInfo => GetZPropertyInfo(nameof(LastDeclaration));

		#endregion

		#region NumberOfDeclarations

		public ZInt NumberOfDeclarations
		{
			get => numberOfDeclarations;
			set
			{
				if (SetNonPersistentPropertyValue(NumberOfDeclarationsInfo, ref numberOfDeclarations, value))
				{
					Validate(NumberOfDeclarationsInfo);
				}
			}
		}

		ZInt numberOfDeclarations;

		public ZPropertyInfo NumberOfDeclarationsInfo => GetZPropertyInfo(nameof(NumberOfDeclarations));

		#endregion

		#region ImplicitAcknowledgement

		public ZBool ImplicitAcknowledgement
		{
			get => implicitAcknowledgement;
			set
			{
				if (SetNonPersistentPropertyValue(ImplicitAcknowledgementInfo, ref implicitAcknowledgement, value))
				{
					Validate(ImplicitAcknowledgementInfo);
				}
			}
		}

		ZBool implicitAcknowledgement;

		public ZPropertyInfo ImplicitAcknowledgementInfo => GetZPropertyInfo(nameof(ImplicitAcknowledgement));

		#endregion

		#region ExplicitAcknowledgement

		public ZBool ExplicitAcknowledgement
		{
			get => explicitAcknowledgement;
			set
			{
				if (SetNonPersistentPropertyValue(ExplicitAcknowledgementInfo, ref explicitAcknowledgement, value))
				{
					Validate(ExplicitAcknowledgementInfo);
				}
			}
		}

		ZBool explicitAcknowledgement;

		public ZPropertyInfo ExplicitAcknowledgementInfo => GetZPropertyInfo(nameof(ExplicitAcknowledgement));

		#endregion

		#region ImplicitBAET

		public ZBool ImplicitBAET
		{
			get => implicitBAET;
			set
			{
				if (SetNonPersistentPropertyValue(ImplicitBAETInfo, ref implicitBAET, value))
				{
					Validate(ImplicitBAETInfo);
				}
			}
		}

		ZBool implicitBAET;

		public ZPropertyInfo ImplicitBAETInfo => GetZPropertyInfo(nameof(ImplicitBAET));

		#endregion

		#region ExplicitBAET

		public ZBool ExplicitBAET
		{
			get => explicitBAET;
			set
			{
				if (SetNonPersistentPropertyValue(ExplicitBAETInfo, ref explicitBAET, value))
				{
					Validate(ExplicitBAETInfo);
				}
			}
		}

		ZBool explicitBAET;

		public ZPropertyInfo ExplicitBAETInfo => GetZPropertyInfo(nameof(ExplicitBAET));

		#endregion

		#region OperationalPort

		public IUnloco OperationalPort
		{
			get => operationalPort;
			set => operationalPort = SetChild(operationalPort, value);
		}

		IUnloco operationalPort;

		#endregion

		#region PCS

		public ZString PCS
		{
			get => pcs;
			set
			{
				if (SetNonPersistentPropertyValue(PCSInfo, ref pcs, value))
				{
					Validate(PCSInfo);
				}
			}
		}
		ZString pcs;

		public ZPropertyInfo PCSInfo => GetZPropertyInfo(nameof(PCS));

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		#endregion

		#region CarrierSON

		public RegistrationNumber CarrierSON
		{
			get => carrierSON;
			set => carrierSON = SetChild(carrierSON, value);
		}

		RegistrationNumber carrierSON;

		#endregion

		#region CarrierCI5

		public RegistrationNumber CarrierCI5
		{
			get => carrierCI5;
			set => carrierCI5 = SetChild(carrierCI5, value);
		}

		RegistrationNumber carrierCI5;

		#endregion

		#region FormattedCarrierProviderID

		public ZString FormattedCarrierProviderID
		{
			get => formattedCarrierProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedCarrierProviderIDInfo, ref formattedCarrierProviderID, value))
				{
					Validate(FormattedCarrierProviderIDInfo);
				}
			}
		}
		ZString formattedCarrierProviderID;

		public ZPropertyInfo FormattedCarrierProviderIDInfo => GetZPropertyInfo(nameof(FormattedCarrierProviderID));

		#endregion

		#region ReceivingForwarder

		public Address ReceivingForwarder
		{
			get => receivingForwarder;
			set => receivingForwarder = SetChild(receivingForwarder, value);
		}

		Address receivingForwarder;

		#endregion

		#region ReceivingForwarderSON

		public RegistrationNumber ReceivingForwarderSON
		{
			get => receivingForwarderSON;
			set => receivingForwarderSON = SetChild(receivingForwarderSON, value);
		}

		RegistrationNumber receivingForwarderSON;

		#endregion

		#region ReceivingForwarderCI5

		public RegistrationNumber ReceivingForwarderCI5
		{
			get => receivingForwarderCI5;
			set => receivingForwarderCI5 = SetChild(receivingForwarderCI5, value);
		}

		RegistrationNumber receivingForwarderCI5;

		#endregion

		#region SendingForwarder

		public Address SendingForwarder
		{
			get => sendingForwarder;
			set => sendingForwarder = SetChild(sendingForwarder, value);
		}

		Address sendingForwarder;

		#endregion

		#region SendingForwarderSON

		public RegistrationNumber SendingForwarderSON
		{
			get => sendingForwarderSON;
			set => sendingForwarderSON = SetChild(sendingForwarderSON, value);
		}

		RegistrationNumber sendingForwarderSON;

		#endregion

		#region SendingForwarderCI5

		public RegistrationNumber SendingForwarderCI5
		{
			get => sendingForwarderCI5;
			set => sendingForwarderCI5 = SetChild(sendingForwarderCI5, value);
		}

		RegistrationNumber sendingForwarderCI5;

		#endregion

		#region ThirdParty

		public Address ThirdParty
		{
			get => thirdParty;
			set => thirdParty = SetChild(thirdParty, value);
		}

		Address thirdParty;

		#endregion

		#region ThirdPartyReference

		public ZString ThirdPartyReference
		{
			get => thirdPartyReference;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdPartyReferenceInfo, ref thirdPartyReference, value))
				{
					Validate(ThirdPartyReferenceInfo);
				}
			}
		}

		ZString thirdPartyReference;

		public ZPropertyInfo ThirdPartyReferenceInfo => GetZPropertyInfo(nameof(ThirdPartyReference));

		#endregion

		#region ThirdPartyNotes

		public ZString ThirdPartyNotes
		{
			get => thirdPartyNotes;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdPartyNotesInfo, ref thirdPartyNotes, value))
				{
					Validate(ThirdPartyNotesInfo);
				}
			}
		}

		ZString thirdPartyNotes;

		public ZPropertyInfo ThirdPartyNotesInfo => GetZPropertyInfo(nameof(ThirdPartyNotes));

		#endregion

		#region DossierAPPlusID

		public ZString DossierAPPlusID
		{
			get => dossierAPPlusID;
			set
			{
				if (SetNonPersistentPropertyValue(DossierAPPlusIDInfo, ref dossierAPPlusID, value))
				{
					Validate(DossierAPPlusIDInfo);
				}
			}
		}

		ZString dossierAPPlusID;

		public ZPropertyInfo DossierAPPlusIDInfo => GetZPropertyInfo(nameof(DossierAPPlusID));

		#endregion

		#region ThirdPartySON

		public RegistrationNumber ThirdPartySON
		{
			get => thirdPartySON;
			set => thirdPartySON = SetChild(thirdPartySON, value);
		}

		RegistrationNumber thirdPartySON;

		#endregion

		#region ThirdPartyCI5

		public RegistrationNumber ThirdPartyCI5
		{
			get => thirdPartyCI5;
			set => thirdPartyCI5 = SetChild(thirdPartyCI5, value);
		}

		RegistrationNumber thirdPartyCI5;

		#endregion

		#region FormattedThirdPartyProviderID

		public ZString FormattedThirdPartyProviderID
		{
			get => formattedThirdPartyProviderID;
			set
			{
				if (SetNonPersistentPropertyValue(FormattedThirdPartyProviderIDInfo, ref formattedThirdPartyProviderID, value))
				{
					Validate(FormattedThirdPartyProviderIDInfo);
				}
			}
		}
		ZString formattedThirdPartyProviderID;

		public ZPropertyInfo FormattedThirdPartyProviderIDInfo => GetZPropertyInfo(nameof(FormattedThirdPartyProviderID));

		#endregion

		#region ConfirmationReference

		public ZString ConfirmationReference
		{
			get => confirmationReference;
			set
			{
				if (SetNonPersistentPropertyValue(ConfirmationReferenceInfo, ref confirmationReference, value))
				{
					Validate(ConfirmationReferenceInfo);
				}
			}
		}

		ZString confirmationReference;

		public ZPropertyInfo ConfirmationReferenceInfo => GetZPropertyInfo(nameof(ConfirmationReference));

		#endregion

		#region Import

		public ZBool IsImport
		{
			get => isImport;
			set
			{
				if (SetNonPersistentPropertyValue(IsImportInfo, ref isImport, value))
				{
					Validate(IsImportInfo);
				}
			}
		}
		ZBool isImport;

		public ZPropertyInfo IsImportInfo => GetZPropertyInfo(nameof(IsImport));

		#endregion

		#region ECVReference

		public ZString ECVReference
		{
			get => ecvReference;
			set
			{
				if (SetNonPersistentPropertyValue(ECVReferenceInfo, ref ecvReference, value))
				{
					Validate(ECVReferenceInfo);
				}
			}
		}
		ZString ecvReference;

		public ZPropertyInfo ECVReferenceInfo => GetZPropertyInfo(nameof(ECVReference));

		#endregion

		#region AgentReference

		public ZString AgentReference
		{
			get => agentReference;
			set
			{
				if (SetNonPersistentPropertyValue(AgentReferenceInfo, ref agentReference, value))
				{
					Validate(AgentReferenceInfo);
				}
			}
		}
		ZString agentReference;

		public ZPropertyInfo AgentReferenceInfo => GetZPropertyInfo(nameof(AgentReference));

		#endregion
	}
}
