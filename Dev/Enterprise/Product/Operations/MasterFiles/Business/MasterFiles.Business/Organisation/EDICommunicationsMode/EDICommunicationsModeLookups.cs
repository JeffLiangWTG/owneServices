using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EDICommunicationsModeLookups : AutoEDICommunicationsModeLookups
	{
		public EDICommunicationsModeLookups(AutoEDICommunicationsMode parent)
			: base(parent)
		{
		}

		public IEDIMessagePurposeCollection MessagePurposeList
		{
			get => Factory.GetCachedValue("IEDIMessagePurposeCollection", () => ObjectFactory.Get<IEDIMessagePurposeCollection>("IEDIMessagePurposeCollection", Factory, new ZQuery()));
		}

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = Factory.GetCachedValue("EDICommunicationModeLookups.Organisations", () => new OrgHeaderCollection(Factory));
				}
				return fOrganisations;
			}
		}

		OrgHeaderCollection fOrganisations;

		public EDICommunicationPartyCollection Parties
		{
			get
			{
				if (fParties == null)
				{
					fParties = Factory.GetCachedValue("EDICommunicationModeLookups.Parties", () =>
					{
						var dbQuery = new ZDBOnlyQuery(typeof(EDICommunicationParty));
						var subQuery = GetCommunicationPartyConfig(EDICommunicationPartyConfigDirectionsList.Codes.Outbound);

						subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_IsActive, 1);
						dbQuery.AddSubQuery(EDICommunicationPartySchema.PK, subQuery, JoinCondition.And);

						var ediCommPartyCollection = new EDICommunicationPartyCollection(Factory, dbQuery);
						ediCommPartyCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Active Status: Outbound", "Property", (ZString)Res.GetString("8FAB5490-A8A8-4FDF-B257-AC957A78C153", "Active"), false));
						return ediCommPartyCollection;
					});
				}
				return fParties;
			}
		}

		EDICommunicationPartyCollection fParties;

		ZDBOnlySubQuery GetCommunicationPartyConfig(ZString direction)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(EDICommunicationPartyConfig),
				EDICommunicationPartyConfigSchema.ECC_ECP_Party);
			subQuery.AddToFilter(EDICommunicationPartyConfigSchema.ECC_Direction, direction);
			return subQuery;
		}

		public CodeDescriptionPairList ModuleList
		{
			get
			{
				if (fModuleList == null)
				{
					fModuleList = new CodeDescriptionPairList();
					fModuleList.AddRange(ObjectFactory.Get<IWorkflowDescriptorList>());
					fModuleList.AddPair(EDICommunicationsMode.Modules.US_BIRD, Res.GetString("38bb1c14-f0f8-406d-a73d-15f6d6e095a2", "US BIRD Data Transfer"));
					fModuleList.AddPair(EDICommunicationsMode.Modules.ContainerMovements, Res.GetString("a4f6c95f-1ca8-4c82-81b7-b5bc8db4b84c", "Container Movements"));
					fModuleList.AddPair(EDICommunicationsMode.Modules.ClientSpecific, Res.GetString("d686bbd4-815e-435a-9522-dc35963b5c31", "Client Specific"));
					if (AccountingMasterFilesRegistry.Instance.EnableNetting.Value)
					{
						fModuleList.AddPair(EDICommunicationsMode.Modules.Netting, Res.GetString("be0efcd5-793d-481f-986f-40d89f106280", "Netting System"));
					}
					if (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.Value == AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code)
					{
						fModuleList.AddPair(EDICommunicationsMode.Modules.CreditControlledDocumentApproval, Res.GetString("03cd5da4-a08b-423a-a5f1-060546521056", "Credit Controlled Document Approval"));
					}
					if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value)
					{
						fModuleList.AddPair(EDICommunicationsMode.Modules.GlobalElectronicInvoicing, Res.GetString("4ed16c74-45de-40ec-929d-c4cc4e6d7843", "Global Electronic Invoicing"));
					}
				}
				return fModuleList;
			}
		}

		public CodeDescriptionPairList FileFormatList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				switch (Parent.EK_Module)
				{
					case EDICommunicationsMode.Modules.Shipnet:
						result.AddPair("", "");
						break;

					case EDICommunicationsMode.Modules.ContainerMovements:
						result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalEvent);
						break;

					case EDICommunicationsMode.Modules.GlobalElectronicInvoicing:
						AddGenericXmlFileFormat(result);
						break;

					default:
						if (Parent.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector)
						{
							AddGenericXmlFileFormat(result);
							AddUniversalXmlFileFormatsIfSupported(result);
						}
						else
						{
							result.AddRange(FileFormatListCore);
							result.AddPair(EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeFileFormatList.Descriptions.All);
						}
						break;
				}

				return result;
			}
		}

		public CodeDescriptionPairList AllFileFormatList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var fileList = new EDICommunicationsModeFileFormatList();
				foreach (var item in fileList.GetAllCodes())
				{
					result.AddPair(item, fileList.GetDescriptionFromCode(item));
				}
				result.RemoveCode(EDICommunicationsModeFileFormatList.Codes.All);
				result.Sort();

				return result;
			}
		}

		protected virtual CodeDescriptionPairList FileFormatListCore
		{
			get
			{
				var result = new CodeDescriptionPairList();
				AddGenericXmlFileFormat(result);
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.SterlingCommerceFlatFile, EDICommunicationsModeFileFormatList.Descriptions.SterlingCommerceFlatFile);
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.NotificationEmail, EDICommunicationsModeFileFormatList.Descriptions.NotificationEmail);
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.NotificationBodyEmail, EDICommunicationsModeFileFormatList.Descriptions.NotificationBodyEmail);
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.EXL, EDICommunicationsModeFileFormatList.Descriptions.EXL);
				if (Parent.EK_Module == WorkflowDescriptors.OrderWorkflowDescriptorCode)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.OrderImportReport, EDICommunicationsModeFileFormatList.Descriptions.OrderImportReport);
				}
				if (Parent.EK_Module == JobInvoicingConsumerTypes.Shipment.Code || Parent.EK_Module == JobInvoicingConsumerTypes.Consol.Code)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.DXL, EDICommunicationsModeFileFormatList.Descriptions.DXL);
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.FXL, EDICommunicationsModeFileFormatList.Descriptions.FXL);
				}
				if (Parent.EK_Module == WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode || Parent.EK_Module == JobInvoicingConsumerTypes.Shipment.Code)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XMB, EDICommunicationsModeFileFormatList.Descriptions.XMB);
				}
				if (Parent.EK_Module == WorkflowDescriptors.WhsOrderWorkflowDescriptorCode)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.IFS, EDICommunicationsModeFileFormatList.Descriptions.IFS);
				}

				if (Parent.EK_Module.Equals(JobInvoicingConsumerTypes.Consol.Code)
					&& Parent.Organisation != null
					&& Parent.Organisation.OH_IsAirLine && Parent.Organisation.OH_IsShippingProvider)
				{
					if (Parent.EK_CommunicationsTransport.Equals(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)
					|| (Parent.EK_CommunicationsTransport.Equals(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
					&& CanSendCargoImpMessagesThroughEAdaptor()))
					{
						result.AddPair(EDICommunicationsModeFileFormatList.Codes.FHL, EDICommunicationsModeFileFormatList.Descriptions.FHL);
						result.AddPair(EDICommunicationsModeFileFormatList.Codes.FWB, EDICommunicationsModeFileFormatList.Descriptions.FWB);
					}
				}

				AddUniversalXmlFileFormatsIfSupported(result);

				return result;
			}
		}

		bool CanSendCargoImpMessagesThroughEAdaptor()
		{
			return eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value;
		}

		void AddGenericXmlFileFormat(CodeDescriptionPairList result)
		{
			result.AddPair(EDICommunicationsModeFileFormatList.Codes.XML, EDICommunicationsModeFileFormatList.Descriptions.XML);
		}

		void AddUniversalXmlFileFormatsIfSupported(CodeDescriptionPairList result)
		{
			WorkflowDescriptor workflowDescriptor;
			if (WorkflowDescriptors.Instance.TryGetValue(Parent.EK_Module, out workflowDescriptor))
			{
				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalEventXML ||
					workflowDescriptor.SupportsWorkflowTriggerActionUniversalEventCollectionXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalEvent);
				}

				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalShipmentXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalShipment);
				}

				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalTransactionXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalTransaction);
				}

				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalScheduleXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalSchedule);
				}

				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalTransactionBatchXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalTransactionBatch);
				}

				if (workflowDescriptor.SupportsWorkflowTriggerActionUniversalActivityXML)
				{
					result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalActivity);
				}
			}
			else if (Parent.EK_Module == EDICommunicationsMode.Modules.Netting)
			{
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalTransaction);
			}
			else if (Parent.EK_Module == EDICommunicationsMode.Modules.CreditControlledDocumentApproval)
			{
				result.AddPair(EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeFileFormatList.Descriptions.XmlUniversalShipment);
			}
		}

		public CodeDescriptionPairList CommunicationsTransportList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				switch (Parent.EK_Module)
				{
					case EDICommunicationsMode.Modules.Shipnet:
						result.AddPair(ShipnetExportCommunicationsTransportMappingList.Codes.Email, ShipnetExportCommunicationsTransportMappingList.Descriptions.Email);
						result.AddPair(ShipnetExportCommunicationsTransportMappingList.Codes.File, ShipnetExportCommunicationsTransportMappingList.Descriptions.File);
						result.AddPair(ShipnetExportCommunicationsTransportMappingList.Codes.FTP, ShipnetExportCommunicationsTransportMappingList.Descriptions.FTP);
						break;

					case EDICommunicationsMode.Modules.ContainerMovements:
					case EDICommunicationsMode.Modules.Netting:
						// eHub only
						break;

					case EDICommunicationsMode.Modules.CreditControlledDocumentApproval:
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, EDICommunicationsModeCommunicationsTransportList.Descriptions.EAdaptorInterface);
						break;

					case EDICommunicationsMode.Modules.GlobalElectronicInvoicing:
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, EDICommunicationsModeCommunicationsTransportList.Descriptions.EAdaptorInterface);
						break;

					default:
						if (AllowInterfaceConnector)
						{
							result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile, EDICommunicationsModeCommunicationsTransportList.Descriptions.SaveToFile);
						}

						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, EDICommunicationsModeCommunicationsTransportList.Descriptions.EmailAsAttachment);
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, EDICommunicationsModeCommunicationsTransportList.Descriptions.EmailAsText);
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, EDICommunicationsModeCommunicationsTransportList.Descriptions.NativeXMLConnector);
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, EDICommunicationsModeCommunicationsTransportList.Descriptions.EAdaptorInterface);
						result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, EDICommunicationsModeCommunicationsTransportList.Descriptions.XTInterface);
						if (AllowInterfaceConnector)
						{
							result.AddPair(ShipnetExportCommunicationsTransportMappingList.Codes.FTP, ShipnetExportCommunicationsTransportMappingList.Descriptions.FTP);
						}

						break;
				}

				result.AddPair(EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, EDICommunicationsModeCommunicationsTransportList.Descriptions.EHubService);

				return result;
			}
		}

#if DEBUG
		protected virtual
#endif
		bool AllowInterfaceConnector
		{
			get { return Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}

		public CodeDescriptionPairList CommsDirectionList
		{
			get
			{
				if (fCommsDirectionList == null)
				{
					fCommsDirectionList = new CodeDescriptionPairList();
					switch (Parent.EK_Module)
					{
						case EDICommunicationsMode.Modules.GlobalElectronicInvoicing:
							fCommsDirectionList.AddPair(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommsDirectionList.Descriptions.Transmit);
							break;

						default:
							fCommsDirectionList.AddPair(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommsDirectionList.Descriptions.Transmit);
							fCommsDirectionList.AddPair(EDICommunicationsModeCommsDirectionList.Codes.Receive, EDICommunicationsModeCommsDirectionList.Descriptions.Receive);
							break;
					}
				}
				return fCommsDirectionList;
			}
		}

		protected new EDICommunicationsMode Parent
		{
			get { return (EDICommunicationsMode)base.Parent; }
		}

		CodeDescriptionPairList fModuleList;
		CodeDescriptionPairList fCommsDirectionList;

		public CodeDescriptionPairList TransportModes
		{
			get => Factory.GetCachedValue("EDICommunicationModeTransportModes", () =>
			{
				var list = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);
				list.RemoveCode(Enterprise.Core.Constants.TransportModes.All);
				return list;
			});
		}

		public CodeDescriptionPairList RecipientRoles
		{
			get => MessageRecipientPartyTypeList.AllPossiblePartyTypes;
		}

		public CodeDescriptionPairList Events
		{
			get => Factory.GetCachedValue("EDICommunicationModeEventTypes", () => EventTypeListProvider.CreateMilestoneEventTypeList(Core.Constants.Workflow.MilestoneType, string.Empty, false, Factory));
		}

		public CodeDescriptionPairList EventReferenceTypes
		{
			get => Factory.GetCachedValue("EDICommunicationModeEventReferenceTypes", () =>
				{
					var list = new EventReferenceConditionList();
					list.RemoveCode(EventReferenceConditionList.Codes.ConditionWithMacros);
					list.RemoveCode(EventReferenceConditionList.Codes.UserDefined);
					return list;
				}
			);
		}
	}
}
