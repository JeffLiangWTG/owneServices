using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This structure acts as enum with [Flags] attribute, but allows more than 64 flags.
	/// </summary>
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct MessageRecipientPartyType
	{
		public static readonly MessageRecipientPartyType None = DeclareFlag(new MessageRecipientPartyType(0, 0));
		public static readonly MessageRecipientPartyType Consignee = DeclareFlag(BitFlag(0));
		public static readonly MessageRecipientPartyType Consignor = DeclareFlag(BitFlag(1));
		public static readonly MessageRecipientPartyType BillToParty = DeclareFlag(BitFlag(2));
		public static readonly MessageRecipientPartyType Broker = DeclareFlag(BitFlag(3));
		public static readonly MessageRecipientPartyType PickupCartage = DeclareFlag(BitFlag(4));
		public static readonly MessageRecipientPartyType DeliveryCartage = DeclareFlag(BitFlag(5));
		public static readonly MessageRecipientPartyType ReceivingAgent = DeclareFlag(BitFlag(6));
		public static readonly MessageRecipientPartyType SendingAgent = DeclareFlag(BitFlag(7));
		public static readonly MessageRecipientPartyType ControllingCustomer = DeclareFlag(BitFlag(8));
		public static readonly MessageRecipientPartyType OrgProxy = DeclareFlag(BitFlag(9));
		public static readonly MessageRecipientPartyType Client = DeclareFlag(BitFlag(10));
		public static readonly MessageRecipientPartyType Email = DeclareFlag(BitFlag(11));
		public static readonly MessageRecipientPartyType Print = DeclareFlag(BitFlag(12));
		public static readonly MessageRecipientPartyType TransportCo = DeclareFlag(BitFlag(13));
		public static readonly MessageRecipientPartyType Carrier = DeclareFlag(BitFlag(14));
		public static readonly MessageRecipientPartyType NotifyParty = DeclareFlag(BitFlag(15));
		public static readonly MessageRecipientPartyType DeliveryToParty = DeclareFlag(BitFlag(16));
		public static readonly MessageRecipientPartyType PickupParty = DeclareFlag(BitFlag(17));
		public static readonly MessageRecipientPartyType EDICommunication = DeclareFlag(BitFlag(18));
		public static readonly MessageRecipientPartyType InvoiceDebtor = DeclareFlag(BitFlag(19));
		public static readonly MessageRecipientPartyType DepartureCFS = DeclareFlag(BitFlag(20));
		public static readonly MessageRecipientPartyType ArrivalCFS = DeclareFlag(BitFlag(21));
		public static readonly MessageRecipientPartyType Forwarder = DeclareFlag(BitFlag(22));
		public static readonly MessageRecipientPartyType ArrivalCarrier = DeclareFlag(BitFlag(23));
		public static readonly MessageRecipientPartyType DepartureCarrier = DeclareFlag(BitFlag(24));
		public static readonly MessageRecipientPartyType Principal = DeclareFlag(BitFlag(25));
		public static readonly MessageRecipientPartyType DepartureCTO = DeclareFlag(BitFlag(26));
		public static readonly MessageRecipientPartyType ArrivalCTO = DeclareFlag(BitFlag(27));
		public static readonly MessageRecipientPartyType DepartureContainerYard = DeclareFlag(BitFlag(28));
		public static readonly MessageRecipientPartyType ArrivalContainerYard = DeclareFlag(BitFlag(29));
		public static readonly MessageRecipientPartyType HVLVAirClearanceAgent = DeclareFlag(BitFlag(30));
		public static readonly MessageRecipientPartyType ShippingManager = DeclareFlag(BitFlag(31));
		public static readonly MessageRecipientPartyType BookingParty = DeclareFlag(BitFlag(32));
		public static readonly MessageRecipientPartyType WarehouseInwards = DeclareFlag(BitFlag(33));
		public static readonly MessageRecipientPartyType WarehouseOutwards = DeclareFlag(BitFlag(34));
		public static readonly MessageRecipientPartyType DeConsolidator = DeclareFlag(BitFlag(35));
		public static readonly MessageRecipientPartyType AirCargoResponsibleParty = DeclareFlag(BitFlag(36));
		public static readonly MessageRecipientPartyType PickupAgent = DeclareFlag(BitFlag(37));
		public static readonly MessageRecipientPartyType DeliveryAgent = DeclareFlag(BitFlag(38));
		public static readonly MessageRecipientPartyType ImportBroker = DeclareFlag(BitFlag(39));
		public static readonly MessageRecipientPartyType ExportBroker = DeclareFlag(BitFlag(40));
		public static readonly MessageRecipientPartyType CartageAgent = DeclareFlag(BitFlag(41));
		public static readonly MessageRecipientPartyType JapanCustomsAFR = DeclareFlag(BitFlag(42));
		public static readonly MessageRecipientPartyType HVLVSeaClearanceAgent = DeclareFlag(BitFlag(43));
		public static readonly MessageRecipientPartyType Warehouse = DeclareFlag(BitFlag(44));
		public static readonly MessageRecipientPartyType SeaCargoResponsibleParty = DeclareFlag(BitFlag(45));
		public static readonly MessageRecipientPartyType DepartureTransitWarehouse = DeclareFlag(BitFlag(46));
		public static readonly MessageRecipientPartyType ASYCUDA = DeclareFlag(BitFlag(47));
		public static readonly MessageRecipientPartyType IndianCustomsEDISystem = DeclareFlag(BitFlag(48));
		public static readonly MessageRecipientPartyType PortForExportManifest = DeclareFlag(BitFlag(49));
		public static readonly MessageRecipientPartyType PortForExportRelease = DeclareFlag(BitFlag(50));
		public static readonly MessageRecipientPartyType PortForImportManifest = DeclareFlag(BitFlag(51));
		public static readonly MessageRecipientPartyType PortForImportRelease = DeclareFlag(BitFlag(52));
		public static readonly MessageRecipientPartyType YardForExportRelease = DeclareFlag(BitFlag(53));
		public static readonly MessageRecipientPartyType YardForImportPreArrival = DeclareFlag(BitFlag(54));
		public static readonly MessageRecipientPartyType NettingSystem = DeclareFlag(BitFlag(55));
		public static readonly MessageRecipientPartyType ContainerYard = DeclareFlag(BitFlag(56));
		public static readonly MessageRecipientPartyType CTO = DeclareFlag(BitFlag(57));
		public static readonly MessageRecipientPartyType ArrivalTransitWarehouse = DeclareFlag(BitFlag(58));
		public static readonly MessageRecipientPartyType ControllingAgent = DeclareFlag(BitFlag(59));
		public static readonly MessageRecipientPartyType BondedWarehouseInwards = DeclareFlag(BitFlag(60));
		public static readonly MessageRecipientPartyType BondedWarehouseOutwards = DeclareFlag(BitFlag(61));
		public static readonly MessageRecipientPartyType BondedWhsChangeOfOwnership = DeclareFlag(BitFlag(62));
		public static readonly MessageRecipientPartyType AutoDocumentDelivery = DeclareFlag(BitFlag(63));
		public static readonly MessageRecipientPartyType SGAccess = DeclareFlag(BitFlag(64));
		public static readonly MessageRecipientPartyType USAirAMS = DeclareFlag(BitFlag(65));
		public static readonly MessageRecipientPartyType CustomsOutturnAgent = DeclareFlag(BitFlag(66));
		public static readonly MessageRecipientPartyType LastCompletedTaskResource = DeclareFlag(BitFlag(67));
		public static readonly MessageRecipientPartyType JobLevelWorkflowGroup = DeclareFlag(BitFlag(68));
		public static readonly MessageRecipientPartyType NotificationGroup = DeclareFlag(BitFlag(69));
		public static readonly MessageRecipientPartyType CreditControlledDocumentApproval = DeclareFlag(BitFlag(70));
		public static readonly MessageRecipientPartyType CarrierBookingAgent = DeclareFlag(BitFlag(71));
		public static readonly MessageRecipientPartyType AssignedStaff = DeclareFlag(BitFlag(72));
		public static readonly MessageRecipientPartyType RequiredCapabilityMembers = DeclareFlag(BitFlag(73));
		public static readonly MessageRecipientPartyType AssignedGroupMembers = DeclareFlag(BitFlag(74));
		public static readonly MessageRecipientPartyType PersonalEmail = DeclareFlag(BitFlag(75));
		public static readonly MessageRecipientPartyType PersonPrimaryWorkEmail = DeclareFlag(BitFlag(76));
		public static readonly MessageRecipientPartyType PersonalFallbackPrimaryWorkEmail = DeclareFlag(BitFlag(77));
		public static readonly MessageRecipientPartyType NVOCC = DeclareFlag(BitFlag(78));
		public static readonly MessageRecipientPartyType GroupOwners = DeclareFlag(BitFlag(79));
		public static readonly MessageRecipientPartyType CurrentUser = DeclareFlag(BitFlag(80));
		public static readonly MessageRecipientPartyType ExternalBroker = DeclareFlag(BitFlag(81));
		public static readonly MessageRecipientPartyType HVLVForwarder = DeclareFlag(BitFlag(82));
		public static readonly MessageRecipientPartyType CarrierMessagingDebtor = DeclareFlag(BitFlag(83));
		public static readonly MessageRecipientPartyType FirstApprovalTask = DeclareFlag(BitFlag(84));
		public static readonly MessageRecipientPartyType OnBoardingEmail = DeclareFlag(BitFlag(85));
		public static readonly MessageRecipientPartyType BondedWhsChangeOfRegime = DeclareFlag(BitFlag(86));
		public static readonly MessageRecipientPartyType WarehouseWorkOrder = DeclareFlag(BitFlag(87));
		public static readonly MessageRecipientPartyType WarehouseDynamicWorkOrder = DeclareFlag(BitFlag(88));
		public static readonly MessageRecipientPartyType Staff = DeclareFlag(BitFlag(89));
		public static readonly MessageRecipientPartyType TransportJobRegistry = DeclareFlag(BitFlag(90));
		public static readonly MessageRecipientPartyType GlobalTradeManagement = DeclareFlag(BitFlag(91));
		public static readonly MessageRecipientPartyType GateManagement = DeclareFlag(BitFlag(92));
		public static readonly MessageRecipientPartyType PortForTransitManifest = DeclareFlag(BitFlag(93));

		// valueSet is only used during static initialization
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static HashSet<MessageRecipientPartyType> valueSet;

		// values are constructed during static initialization and never change after that
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly ImmutableList<MessageRecipientPartyType> values;

		readonly ulong value0;
		readonly ulong value1;

		MessageRecipientPartyType(ulong value0, ulong value1)
		{
			this.value0 = value0;
			this.value1 = value1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
		static MessageRecipientPartyType()
		{
			values = ImmutableList.Create(valueSet.ToArray());
			valueSet = null;
		}

		static MessageRecipientPartyType BitFlag(int bitNumber)
		{
			if (bitNumber < 0 || bitNumber >= 128)
			{
				throw new ArgumentException("Bit number must be between 0 and 127.");
			}

			MessageRecipientPartyType flag;
			if (bitNumber < 64)
			{
				flag = new MessageRecipientPartyType(1ul << bitNumber, 0);
			}
			else
			{
				flag = new MessageRecipientPartyType(0, 1ul << (bitNumber - 64));
			}
			return flag;
		}

		static MessageRecipientPartyType DeclareFlag(MessageRecipientPartyType flag)
		{
			if (values != null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "New flags should not be added to {0} outside of static initializer.", nameof(MessageRecipientPartyType)));
			}

			if (valueSet == null)
			{
				// Field initializer is not used for usedBits set construction, because it would be called only after flags initialization causing NRE.
				valueSet = new HashSet<MessageRecipientPartyType>();
			}

			if (!valueSet.Add(flag))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The flag (0x{0:X16}, 0x{1:X16}) is already declared.", flag.value0, flag.value1));
			}

			return flag;
		}

		public static MessageRecipientPartyType[] GetValues()
		{
			return values.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static MessageRecipientPartyType operator |(MessageRecipientPartyType a, MessageRecipientPartyType b)
		{
			return new MessageRecipientPartyType(a.value0 | b.value0, a.value1 | b.value1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static MessageRecipientPartyType operator &(MessageRecipientPartyType a, MessageRecipientPartyType b)
		{
			return new MessageRecipientPartyType(a.value0 & b.value0, a.value1 & b.value1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static MessageRecipientPartyType operator ~(MessageRecipientPartyType a)
		{
			return new MessageRecipientPartyType(~a.value0, ~a.value1);
		}
		public static MessageRecipientPartyType operator ^(MessageRecipientPartyType left, MessageRecipientPartyType right)
		{
			return new MessageRecipientPartyType(left.value0 ^ right.value0, left.value1 ^ right.value1);
		}

		public bool HasFlag(MessageRecipientPartyType flag)
		{
			return (value0 & flag.value0) == flag.value0 && (value1 & flag.value1) == flag.value1;
		}

		public bool Equals(MessageRecipientPartyType other)
		{
			return value0 == other.value0 && value1 == other.value1;
		}

		public override bool Equals(object obj)
			=> !(obj is null) && obj is MessageRecipientPartyType type && Equals(type);

		public override int GetHashCode()
		{
			unchecked
			{
				return (value0.GetHashCode() * 397) ^ value1.GetHashCode();
			}
		}

		public static bool operator ==(MessageRecipientPartyType left, MessageRecipientPartyType right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MessageRecipientPartyType left, MessageRecipientPartyType right)
		{
			return !left.Equals(right);
		}

		public static IEnumerable<int> GetFlagDifferences(MessageRecipientPartyType left, MessageRecipientPartyType right)
		{
			var diff = left ^ right;

			if (diff == None)
			{
				yield break;
			}

			var partialSize = sizeof(ulong) * 8;

			for (var i = 0; i < partialSize; ++i)
			{
				if ((diff.value0 & ((ulong)1 << i)) != 0)
				{
					yield return i;
				}
			}

			for (var i = 0; i < partialSize; ++i)
			{
				if ((diff.value1 & ((ulong)1 << i)) != 0)
				{
					yield return partialSize + i;
				}
			}
		}

		public override string ToString()
		{
			var val1 = Convert.ToString((long)value0, 2).PadLeft(sizeof(ulong) * 8, '0');
			var val2 = Convert.ToString((long)value1, 2).PadLeft(sizeof(ulong) * 8, '0');
			return $"{val1}:{val2}";
		}

		#region Properties and methods that are exposed for testing purposes only.

#if DEBUG

		internal ulong Value0
		{
			get { return value0; }
		}

		internal ulong Value1
		{
			get { return value1; }
		}

		internal static MessageRecipientPartyType CreateForTest(ulong value0, ulong value1)
		{
			return new MessageRecipientPartyType(value0, value1);
		}

		internal static MessageRecipientPartyType BitFlagForTest(int bitNumber)
		{
			return BitFlag(bitNumber);
		}

		internal static MessageRecipientPartyType DeclareFlagForTest(MessageRecipientPartyType flag)
		{
			return DeclareFlag(flag);
		}

#endif

		#endregion
	}
}
