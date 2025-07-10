using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class ConsolPortMessagingManager : PortMessagingManager
	{
		public ConsolPortMessagingManager(ForwardingConsol consol)
			: base(consol)
		{
			Consol.Shipments.CountChanged += (s, e) => NotifyDataChanged();
		}

		ForwardingConsol Consol
		{
			get { return (ForwardingConsol)MessageOriginator; }
		}

		public override PortMessagingData Data
		{
			get { return data ?? (data = new ConsolPortMessagingData(Consol)); }
		}
		PortMessagingData data;

		#region SZB

		public override ZString SZBNumber
		{
			get { return Consol.JK_SZB; }
		}

		public override ZString SZBInformation
		{
			get { return Consol.JK_SZBInformation; }
		}

		public override ZDateTime SZBIssueDate
		{
			get { return Consol.JK_SZBIssueDate; }
		}

		#endregion

		public override bool ShouldShowPortMessagingForDakosy
		{
			get { return Consol != null && Consol.IsSea && IsValidConsolForDakosyPortMessaging(Consol); }
		}

		public override ZString CheckPortMessagingAvailability()
		{
			if (Consol == null)
			{
				return Res.GetString("7c069ec9-ebdf-4650-b610-c0b5ed3fd2ca", "Could not load this Consol for Port Messaging. Please try closing and reopening this Consol to access Port Messaging.");
			}
			else if (!Env.Security.PortMessaging.IsAllowed)
			{
				return Env.Security.PortMessaging.ErrorMessageForNotAllowed;
			}
			else if (!IsValidConsolForDakosyPortMessaging(Consol))
			{
				return Res.GetString("06ff4ed8-5e27-4a78-b19a-895587ce506c", "Port Order messages to DAKOSY can only be sent for Consols linked to a Sea transport leg to or from Hamburg (DEHAM).");
			}

			return ZString.Empty;
		}

		public override ForwardingConsol GetCurrentConsol()
		{
			return Consol;
		}

		protected override IEnumerable<ForwardingShipment> GetTopLevelShipments()
		{
			return Consol.TopLevelShipments.Cast<ForwardingShipment>();
		}

		protected override ZString RunPreSendDataValidationCore(MessageType messageType)
		{
			if (Data.SenderCode.IsEmpty)
			{
				return Res.GetString("a4a1e536-ac0b-4aed-9c88-5611f636a09c", "Cannot send message while the Sending Forwarder's Dakosy Participant Code is missing. Enter the Dakosy Participant Code on Organization -> Config -> Registration Numbers / Codes.");
			}

			if (!Consol.Shipments.Any(s => ((ForwardingShipment)s).IsSea) || !DoAllSeaShipmentsHaveSZBNumber(Consol))
			{
				return Res.GetString("7576c5f5-fae8-4708-8fb6-49acbf316def", "Not all sea shipments on this Consol have received their release SZB number. Send the Port Order on the Shipments to receive release number(s) from Dakosy.");
			}

			foreach (ForwardingShipment shipment in Consol.Shipments)
			{
				if (PortMessagingHelper.CheckPortMessagingForShipment(shipment, x => x.EntryType == EntryTypeList.Codes.EmergencyConcept)
					&& shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(x => x.JL_HarmonisedCode.IsEmpty))
				{
					return Res.GetString("8441eb3e-4459-4bbf-9ffd-8000d5368a13", "Shipment {0}: {1}",
						shipment.JS_UniqueConsignRef, HarmonizedCodeMandatoryForAUSError);
				}
			}

			var shipmentNames = Consol.Shipments.Cast<ForwardingShipment>().Where(HasInvalidUNDGDataItems).Select(c => c.JS_UniqueConsignRef);
			if (shipmentNames.Any())
			{
				return Res.GetString("98351708-0302-4772-823a-6c8701517967",
					"Multiple Dangerous Goods are recorded against a pack line on these shipments {0}, please enter Package Count and Package Type for each DG recorded via Shipment > Packing > Pack line > Right click menu > Dangerous Goods",
					string.Join(", ", shipmentNames));
			}

			var validateContainersMessage = ValidateContainers();
			if (!string.IsNullOrWhiteSpace(validateContainersMessage))
			{
				return validateContainersMessage;
			}

			return CheckEntryTypeBeEntered(messageType);
		}

		ZString CheckEntryTypeBeEntered(MessageType messageType)
		{
			if (IsHDS(messageType))
			{
				var entryTypeBeEntered = false;

				foreach (ForwardingShipment shipment in Consol.Shipments)
				{
					var portMessaging = PortMessagingHelper.GetShipmentPortMessaging(shipment);
					if (portMessaging != null
						&& (!portMessaging.JSM_EntryType.IsEmpty
							|| shipment.OuterPackLines.Cast<ForwardingPackLine>().All(packLine => PortMessagingHelper.CheckPortMessagingForPackLine(packLine, x => !x.EntryType.IsEmpty))))
					{
						entryTypeBeEntered = true;
						break;
					}
				}

				if (!entryTypeBeEntered)
				{
					return Res.GetString("8a0fbecb-b874-432e-be23-415c4abf93a6", "At least one shipment must have port messaging entry type be entered before you can send Port Order.");
				}
			}

			return ZString.Empty;
		}

		#region Validation checks

		public override bool IsExport
		{
			get { return IsExportConsolForDakosy(Consol); }
		}

		public static bool IsExportConsolForDakosy(ForwardingConsol consol)
		{
			return consol != null
				&& (consol.JK_RL_NKLoadPort == "DEHAM" && consol.IsSea
				|| consol.Transports.Cast<Transport>().Any(t => t.JW_RL_NKLoadPort == "DEHAM" && t.IsSea));
		}

		public override bool IsImport
		{
			get { return IsImportConsolForDakosy(Consol); }
		}

		public static bool IsImportConsolForDakosy(ForwardingConsol consol)
		{
			return consol != null
				&& (consol.JK_RL_NKDischargePort == "DEHAM" && consol.IsSea
				|| consol.Transports.Cast<Transport>().Any(t => t.JW_RL_NKDiscPort == "DEHAM" && t.IsSea));
		}

		public static bool IsValidConsolForDakosyPortMessaging(ForwardingConsol consol)
		{
			return IsExportConsolForDakosy(consol) || IsImportConsolForDakosy(consol);
		}

		static bool DoAllSeaShipmentsHaveSZBNumber(ForwardingConsol consol)
		{
			foreach (ForwardingShipment shipment in consol.TopLevelShipments)
			{
				if (shipment.IsSea && !Core.Constants.ShipmentTypes.AssemblyMaster.Equals(shipment.JS_ShipmentType) && !shipment.Numbers.OfType<CusEntryNumber>().Any(n => n.CE_EntryType == GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber && !n.CE_EntryNum.IsEmpty))
				{
					return false;
				}
			}

			return true;
		}

		public override bool HasDG
		{
			get { return Consol.Shipments.Cast<ForwardingShipment>().Any(x => x.OuterPackLines.HasDangerousGoods); }
		}

		string ValidateContainers()
		{
			var messages = new ZStringBuilder();

			foreach (var container in Consol.Containers.Cast<ForwardingContainer>())
			{
				var packLines = container.PackLines.Cast<PackLine>().ToArray();

				if (packLines.Any())
				{
					var fclShipmentsCount = packLines.Where(c => c.Shipment != null && c.Shipment.JS_PackingMode == Core.Constants.ContainerModes.FCL)
						.Select(c => c.Shipment.PK)
						.Distinct()
						.Count();

					if (fclShipmentsCount > 1)
					{
						var message = Res.GetString("acb25cb3-1610-4965-b744-b2ff4df469db",
							"{0}: A container is not permitted to have multiple {1} shipments attached.",
							container.ContainerCode,
							Core.Constants.ContainerModes.FCL);

						messages.Append(message);
					}

					if (fclShipmentsCount == 1
						&& (container.JC_ContainerMode == Core.Constants.ContainerModes.LCL || container.JC_ContainerMode == Core.Constants.ContainerModes.Groupage))
					{
						var message = Res.GetString("60adbb16-68e1-4899-89f4-aa09649f6829",
							"{0}: {1} or {2} container modes are not permitted to have {3} shipments attached.",
							container.ContainerCode,
							Core.Constants.ContainerModes.LCL,
							Core.Constants.ContainerModes.Groupage,
							Core.Constants.ContainerModes.FCL);

						messages.Append(message);
					}
				}
				else
				{
					var message = Res.GetString("51decb80-b6bd-4d58-8839-7c95650dbf45",
						"{0}: No packlines were found. Please allocate packlines to this container.",
						container.ContainerCode);

					messages.Append(message);
				}
			}

			return messages.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
