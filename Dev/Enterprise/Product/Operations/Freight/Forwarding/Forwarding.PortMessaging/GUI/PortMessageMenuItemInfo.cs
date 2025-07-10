using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public class PortMessageMenuItemInfo
	{
		PortMessageMenuItemInfo(MultilingualString menuItemName, MultilingualString category, PortMessagingManager.MessageType messageType, bool isCancellation, Func<bool> messageAvailablePredicate)
		{
			this.menuItemName = menuItemName;
			this.category = category;
			this.messageType = messageType;
			this.isCancellation = isCancellation;
			this.messageAvailablePredicate = messageAvailablePredicate;
		}

		readonly MultilingualString menuItemName;
		readonly MultilingualString category;
		readonly PortMessagingManager.MessageType messageType;
		readonly bool isCancellation;
		readonly Func<bool> messageAvailablePredicate;

		public MultilingualString MenuItemName
		{
			get { return menuItemName; }
		}

		public MultilingualString Category
		{
			get { return category; }
		}

		public PortMessagingManager.MessageType MessageType
		{
			get { return messageType; }
		}

		public bool IsCancellation
		{
			get { return isCancellation; }
		}

		public Func<bool> MessageAvailablePredicate
		{
			get { return messageAvailablePredicate; }
		}

		public static ReadOnlyCollection<PortMessageMenuItemInfo> GetAllMenuItemInfos(PortMessagingManager manager)
		{
			Argument.NotNull(manager, "manager");

			var result = new List<PortMessageMenuItemInfo>();

			Func<bool> isExport = () => manager.IsExport;
			Func<bool> isImport = () => manager.IsImport;
			Func<bool> isEither = () => manager.IsExport || manager.IsImport;

			result.Add(new PortMessageMenuItemInfo(SendPortOrderWithHDS, PortOrderWithHDS, PortMessagingManager.MessageType.PortOrderWithHDS, false, isExport));

			result.Add(new PortMessageMenuItemInfo(SendPortOrderWithHDSCancellationBecauseOfErrors, PortOrderWithHDS, PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, true, isExport));
			result.Add(new PortMessageMenuItemInfo(SendPortOrderWithHDSCancellationOnExit, PortOrderWithHDS, PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit, true, isExport));
			result.Add(new PortMessageMenuItemInfo(SendPortOrderWithHDSForwardingCancellation, PortOrderWithHDS, PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation, true, isExport));

			result.Add(new PortMessageMenuItemInfo(SendPortOrderInbound, PortOrder, PortMessagingManager.MessageType.PortOrderInbound, false, isExport));
			result.Add(new PortMessageMenuItemInfo(SendPortOrderInboundCancellation, PortOrder, PortMessagingManager.MessageType.PortOrderInbound, true, isExport));
			result.Add(new PortMessageMenuItemInfo(SendPortOrderOutbound, PortOrder, PortMessagingManager.MessageType.PortOrderOutbound, false, isImport));
			result.Add(new PortMessageMenuItemInfo(SendPortOrderOutboundCancellation, PortOrder, PortMessagingManager.MessageType.PortOrderOutbound, true, isImport));
			result.Add(new PortMessageMenuItemInfo(SendStopRequest, PortOrder, PortMessagingManager.MessageType.StopRequest, false, isEither));
			result.Add(new PortMessageMenuItemInfo(SendGatePass, PortOrder, PortMessagingManager.MessageType.GatePass, false, isEither));
			result.Add(new PortMessageMenuItemInfo(SendGatePassCancellation, PortOrder, PortMessagingManager.MessageType.GatePass, true, isEither));
			result.Add(new PortMessageMenuItemInfo(SendRequestForPortServices, PortOrder, PortMessagingManager.MessageType.RequestForPortServices, false, isEither));
			result.Add(new PortMessageMenuItemInfo(SendRequestForPortServicesCancellation, PortOrder, PortMessagingManager.MessageType.RequestForPortServices, true, isEither));
			result.Add(new PortMessageMenuItemInfo(SendCertificateOfObligation, PortOrder, PortMessagingManager.MessageType.CertificateOfObligation, false, isEither));
			result.Add(new PortMessageMenuItemInfo(SendCertificateOfObligationCancellation, PortOrder, PortMessagingManager.MessageType.CertificateOfObligation, true, isEither));
			result.Add(new PortMessageMenuItemInfo(SendRequestForRailDischarge, PortOrder, PortMessagingManager.MessageType.RequestForRailDischarge, false, isEither));
			result.Add(new PortMessageMenuItemInfo(SendRequestForRailDischargeCancellation, PortOrder, PortMessagingManager.MessageType.RequestForRailDischarge, true, isEither));

			return new ReadOnlyCollection<PortMessageMenuItemInfo>(result);
		}

		static MultilingualString PortOrderWithHDS
		{
			get { return ResString.GetMultilingualString("9ef5fdcb-14c4-41b7-8361-b76f6944a271", "Port Order with HDS (DEHAM)"); }
		}

		static MultilingualString PortOrder
		{
			get { return ResString.GetMultilingualString("7f14ae78-ee9a-4ea5-a9e5-714e8c27a2d2", "Port Order (DEHAM)"); }
		}

		#region Menu Item Names

		static MultilingualString SendPortOrderWithHDS
		{
			get { return ResString.GetMultilingualString("74cad062-4c26-4d75-b5c9-d161c8f47c75", "Send Port Order With HDS"); }
		}

		static MultilingualString SendPortOrderWithHDSCancellationBecauseOfErrors
		{
			get { return ResString.GetMultilingualString("4aece717-9dd5-46cd-b6af-aef633fbf8b6", "Cancellation Because of Errors"); }
		}

		static MultilingualString SendPortOrderWithHDSCancellationOnExit
		{
			get { return ResString.GetMultilingualString("0e9413f4-cb40-443c-8be7-b6fe147079ea", "Cancellation of Export on Exit"); }
		}

		static MultilingualString SendPortOrderWithHDSForwardingCancellation
		{
			get { return ResString.GetMultilingualString("24ce5708-71c6-45d2-85f5-04c20bacacf4", "Forwarding to Another Port"); }
		}

		static MultilingualString SendPortOrderInbound
		{
			get { return ResString.GetMultilingualString("4c27ccd7-2e58-45b8-80f5-db808605ca39", "Send Port Order for Inbound Delivery"); }
		}

		static MultilingualString SendPortOrderInboundCancellation
		{
			get { return ResString.GetMultilingualString("7accca77-cfa6-4bd2-98e9-4ba946d5f267", "Send Port Order for Inbound Delivery Cancellation"); }
		}

		static MultilingualString SendPortOrderOutbound
		{
			get { return ResString.GetMultilingualString("72c3ca63-3959-4c41-b758-195f66ee96a6", "Send Port Order for Outbound Delivery"); }
		}

		static MultilingualString SendPortOrderOutboundCancellation
		{
			get { return ResString.GetMultilingualString("baa35af6-7d8c-4c85-a0dc-94cd7e931443", "Send Port Order for Outbound Delivery Cancellation"); }
		}

		static MultilingualString SendStopRequest
		{
			get { return ResString.GetMultilingualString("6f9df253-70d4-4842-926c-5bbfbcaed0e2", "Send Stop Request"); }
		}

		static MultilingualString SendGatePass
		{
			get { return ResString.GetMultilingualString("53ca386e-cdee-468c-9020-56eff1be4404", "Send Gate Pass"); }
		}

		static MultilingualString SendGatePassCancellation
		{
			get { return ResString.GetMultilingualString("af7fc7de-4770-4d3f-9800-38213e01c528", "Send Gate Pass Cancellation"); }
		}

		static MultilingualString SendRequestForPortServices
		{
			get { return ResString.GetMultilingualString("08eda4a9-49aa-4ba8-ac25-cd71740f2325", "Send Request for Port Services"); }
		}

		static MultilingualString SendRequestForPortServicesCancellation
		{
			get { return ResString.GetMultilingualString("0cf1a94e-2492-4ae3-8eb2-000d841acebf", "Send Request for Port Services Cancellation"); }
		}

		static MultilingualString SendCertificateOfObligation
		{
			get { return ResString.GetMultilingualString("6bfcc037-1924-4d11-b19a-20289c43c5a3", "Send Certificate of Obligation"); }
		}

		static MultilingualString SendCertificateOfObligationCancellation
		{
			get { return ResString.GetMultilingualString("dc9657b3-f3ae-4f2a-9ba4-2ab0a84c94f2", "Send Certificate of Obligation Cancellation"); }
		}

		static MultilingualString SendRequestForRailDischarge
		{
			get { return ResString.GetMultilingualString("505ba708-c9fc-42d9-a123-0c8d186be7b0", "Send Request for Rail Discharge"); }
		}

		static MultilingualString SendRequestForRailDischargeCancellation
		{
			get { return ResString.GetMultilingualString("f264a6c8-ef1b-4b99-bee0-269170447458", "Send Request for Rail Discharge Cancellation"); }
		}

		#endregion
	}
}
