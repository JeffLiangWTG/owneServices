using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Freight.QuotedBookings.DataTransfer.Res;
using ResString = Enterprise.Freight.QuotedBookings.DataTransfer.ResString;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingWorkflowDescriptor : WorkflowDescriptor, ISpecificDocumentBusinessContextProvider
	{
		#region Id / Description

		public override string Code
		{
			get { return WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("bac149b7-7b50-40d6-a425-81c969f7e07f", "Quoted Booking"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(QuotedBooking); }
		}

		protected override Type WorkflowProviderTypeForNonPersistentBusinessObjectsCore => typeof(ViewQuotedBooking);

		public override ControllerID ControllerID
		{
			get
			{
				return ControllerIDs.QuotedBookings;
			}
		}

		protected override bool SupportsTaskLineTriggersCore => false;

		public override bool SupportsConvertToShipment => true;

		#endregion

		#region Workflow Triggers

		protected override void CheckWorkflowTriggerActionTypeCore(IBaseTrigger trigger, IBusiness parent, ZPropertyInfo actionTypeInfo)
		{
			base.CheckWorkflowTriggerActionTypeCore(trigger, parent, actionTypeInfo);
			var processTemplateTask = (trigger as TemplateProcessTask)?.Parent;
			var quotedBookingTask = (trigger as QuotedBookingProcessTask)?.Parent;
			var actionType = (ZString)actionTypeInfo.Value;
			var isSpotQuoteSubtype = processTemplateTask != null && processTemplateTask.P0_SubType2 == QuotedBooking.SpotQuoteCode;
			var isQuoteOnlyTask = quotedBookingTask != null && quotedBookingTask.ObjectState == QuotedBookingState.QuoteOnly;

			if ((isSpotQuoteSubtype || isQuoteOnlyTask) && !IsValidOneOffQuoteAction(actionType))
			{
				actionTypeInfo.AddError(Res.GetString("17020dec-7469-48d5-8696-7399da7855a8", "Not available for Spot Quotes."));
			}
		}

		bool IsValidOneOffQuoteAction(ZString triggerType)
		{
			if (WorkflowTriggerActionTypeConstants.IsXmlUniversalShipment(triggerType)
			 || WorkflowTriggerActionTypeConstants.IsXmlUniversalEventWithEdoc(triggerType))
			{
				return true;
			}

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(triggerType)
			 || WorkflowTriggerActionTypeConstants.IsSimplifiedXml(triggerType)
			 || WorkflowTriggerActionTypeConstants.IsDebtorBalanceXml(triggerType))
			{
				return false;
			}

			return true;
		}

		#endregion

		#region Templates Differentiation

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("ae256afb-26ca-46bb-8e82-436097e9f41e", "Transport Mode"), GetTransportModes()));
				list.Add(new ProcessTemplateSubType(Res.GetString("49b51577-0245-42b6-93ea-4a6802d9f235", "Type"), GetQuotedBookingTypes(), true));
				list.Add(new ProcessTemplateSubType(Res.GetString("fe12c9d1-6387-4ee8-8faf-816d684068d9", "Direction"), GetDirectionList()));
				return list.ToArray();
			}
		}

		CodeDescriptionPairList GetTransportModes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(string.Empty, Res.GetString("44e5d757-3b36-4fa9-a534-a58e0fc7d266", "All"));
			result.AddPair(Core.Constants.TransportModes.Air, ResString.GetMultilingualString("6FF96CC5-0023-4716-B080-1D38A4C227F7", "Air Freight"));
			result.AddPair(Core.Constants.TransportModes.Sea, ResString.GetMultilingualString("0D34E736-F682-41F9-9808-748FE86EB645", "Sea Freight"));
			result.AddPair(Core.Constants.TransportModes.Road, ResString.GetMultilingualString("C41B5857-564E-4B1A-B50D-D19809042AE9", "Road Freight"));
			result.AddPair(Core.Constants.TransportModes.Rail, ResString.GetMultilingualString("D226E317-7F72-4EAB-9685-186ADAD4BC29", "Rail Freight"));
			result.AddPair(Core.Constants.TransportModes.Courier, ResString.GetMultilingualString("C63BE49E-2CD2-4D1C-BCDA-01EB145C5732", "Courier"));
			result.AddPair(Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModeDescriptions.FCL);
			result.AddPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other);
			return result;
		}

		CodeDescriptionPairList GetQuotedBookingTypes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(QuotedBooking.BookingWithQuoteCode, Res.GetString("e1a58553-0eb9-4256-b5b0-ca7a0881aa36", "Booking with Quote"));
			result.AddPair(QuotedBooking.QuickBookingCode, Res.GetString("105e6c45-3ab6-4701-9c3e-f8bbc496dd1f", "Quick Booking"));
			result.AddPair(QuotedBooking.SpotQuoteCode, Res.GetString("022653da-f019-49cb-8a98-580f540c0519", "One Off Quote"));
			return result;
		}

		CodeDescriptionPairList GetDirectionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair("", Res.GetString("cc8ceea2-a964-4041-a4f7-d3608263bca8", "All"));
			result.AddPair(DirectionsContext.Import, Res.GetString("398f31c0-8427-4960-a254-b5b0668216b8", "Import"));
			result.AddPair(DirectionsContext.Export, Res.GetString("f0b7caa2-9ffe-4510-9a55-9a2892c9ec1a", "Export"));
			result.AddPair(DirectionsContext.Domestic, Res.GetString("199ff773-3e3c-4a97-8a3d-41c8315c5c8b", "Domestic"));
			return result;
		}

		public override ZString Port1Name
		{
			get { return Res.GetString("26ddf9a0-c7be-45f0-b4c2-7079d9a1c799", "Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("b2bbdaed-d8cb-4bcd-8d15-04c624636dba", "Destination"); }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		#endregion

		#region Form Customisation

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new QuotedBookingFormCustomisationSettingsProvider(this);
		}

		#endregion

		#region Supported Recipients

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Client |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.ControllingAgent |
				MessageRecipientPartyType.ControllingCustomer;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			var adapterType = new QuotedBookingValueObjectDataAdapter().GetType();
			var communicationModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
			return WorkflowDescriptorHelper.GetWorkflowTriggerActionForStandardXmlActionType(adapterType, (IWorkflowProvider)source.Job, communicationModes, action);
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get
			{
				return new[]
				{
					BusinessContext.QuotedBooking,
					BusinessContext.Quotation
				};
			}
		}

		BusinessContext[] ISpecificDocumentBusinessContextProvider.GetDocumentBusinessContext(IWorkflowProvider workflowProvider)
		{
			var quotedBooking = workflowProvider as QuotedBooking;

			if (quotedBooking != null)
			{
				return quotedBooking.Booking != null
					? new[] { BusinessContext.QuotedBooking }
					: new[] { BusinessContext.Quotation };
			}

			var processTaskTemplate = workflowProvider as ProcessTaskTemplate;

			if (processTaskTemplate != null)
			{
				switch (processTaskTemplate.P0_SubType2)
				{
					case QuotedBooking.SpotQuoteCode:
						return new[] { BusinessContext.Quotation };

					case QuotedBooking.QuickBookingCode:
					case QuotedBooking.BookingWithQuoteCode:
						return new[] { BusinessContext.QuotedBooking };

					default:
						return DocumentBusinessContext;
				}
			}

			return DocumentBusinessContext;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject workflowProvider, ZString partyType)
		{
			var booking = (QuotedBooking)workflowProvider;

			if (partyType == MessageRecipientPartyTypeList.Codes.Client)
			{
				if (booking.ClientDocAddress != null && booking.ClientDocAddress.HasRealOrganisation)
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(booking.ClientDocAddress));
				}
				else
				{
					messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(booking.ClientAddrForWorkFlow));
				}
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ControllingAgent)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(booking.ControllingAgentDocumentaryAddress));
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.ControllingCustomer)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(booking.ControllingCustomerDocumentaryAddress));
			}
		}

		#endregion

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return QuotedBooking.New(Integration.QuoteBookingType.BookingWithQuote, factory);
		}

#endif
	}
}
