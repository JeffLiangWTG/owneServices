using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Elements;
using Enterprise.Edifact.D08A.Messages.CUSCAR;
using Enterprise.Edifact.D08A.Messages.CUSREP;
using Enterprise.Edifact.D08A.Segments;
using SegmentGroup3 = Enterprise.Edifact.D08A.Messages.CUSREP.SegmentGroup3;
using SegmentGroup7 = Enterprise.Edifact.D08A.Messages.CUSCAR.SegmentGroup7;
using SegmentGroup8 = Enterprise.Edifact.D08A.Messages.CUSCAR.SegmentGroup8;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	public class eManifestOriginalMessageWrapper
	{
		internal eManifestOriginalMessageWrapper(EDIMessage message)
		{
			if (message != null)
			{
				messageType = message.EM_MessageType;
				messageSubType = message.EM_MessageSubType;
				var edifact = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet());
				cuscar = edifact as CUSCARMessage;
				cusrep = edifact as CUSREPMessage;
			}
		}

		internal bool IsCancelation
		{
			get { return messageSubType == MessageSubTypeCodes.Codes.Cancellation; }
		}

		internal bool IsPreliminary
		{
			get
			{
				return new ZString[]
				{
					MessageTypes.Codes.PreliminaryTrip,
					MessageTypes.Codes.UnassociatedShipments,
					MessageTypes.Codes.CrewAndPassenger
				}.Contains(messageType);
			}
		}

		internal bool IsConfirmation
		{
			get
			{
				var bgmSection = cuscar != null ? cuscar.BGM : cusrep != null ? cusrep.BGM : new BGMSegmentMessageSection(1);
				return (from BGMSegment bgm in bgmSection where bgm.MessageFunctionCode == MessageFunctionCodeList.Confirmation select bgm).Any();
			}
		}

		internal bool IsUnassociatedShipments
		{
			get { return messageType == MessageTypes.Codes.UnassociatedShipments; }
		}

		internal bool ContainsEquipmentInfo
		{
			get
			{
				return (cuscar != null && cuscar.Group5.Count > 0 && cuscar.Group5[0].EQD.Count > 0)
					   || (cusrep != null && cusrep.Group10.Count > 0 && cusrep.Group10[0].EQD.Count > 0);
			}
		}

		internal IEnumerable<ShipmentWrapper> Shipments
		{
			get
			{
				var result = Enumerable.Empty<ShipmentWrapper>();
				if (cuscar != null)
				{
					var tripReference = D08AMessageUtilities.GetMessageReference(cuscar.BGM);
					var linkToTrip = !tripReference.IsEmpty && tripReference != "SYSTEM";
					result = from SegmentGroup7 group7 in cuscar.Group7 select new ShipmentWrapper(group7, IsUnassociatedShipments, linkToTrip);
				}
				else if (cusrep != null)
				{
					result = from SegmentGroup3 group3 in cusrep.Group3 select new ShipmentWrapper(group3);
				}
				return result;
			}
		}

		#region Shipment

		internal class ShipmentWrapper
		{
			internal ShipmentWrapper(SegmentGroup7 group7, bool isUnassociatedShipment, bool linkToTrip)
			{
				this.group7 = group7;
				this.isUnassociatedShipment = isUnassociatedShipment;
				this.linkToTrip = linkToTrip;
			}

			internal ShipmentWrapper(SegmentGroup3 group3)
			{
				this.group3 = group3;
			}

			internal ZString RequestedShipmentStatus
			{
				get
				{
					var result = ZString.Empty;
					if (group7 != null)
					{
						var action = (from CNISegment cni in group7.CNI select cni.DocumentMessageDetails.DocumentStatusCode).FirstOrDefault();
						result = action == DocumentStatusCodeList.Status0 ? ShipmentEntryStatusList.Codes.Cancelled
									: action == DocumentStatusCodeList.Status2 && IsSplitShipment ? ShipmentEntryStatusList.Codes.Linked
										: action == DocumentStatusCodeList.Status2 ? string.Empty
											: isUnassociatedShipment && !linkToTrip ? ShipmentEntryStatusList.Codes.Accepted : ShipmentEntryStatusList.Codes.Linked;
					}
					else if (group3 != null)
					{
						var action = (from DOCSegment doc in group3.DOC select doc.DocumentMessageDetails.DocumentStatusCode).FirstOrDefault();
						result = action == DocumentStatusCodeList.Status0 ? ShipmentEntryStatusList.Codes.Accepted : ShipmentEntryStatusList.Codes.Linked;
					}
					return result;
				}
			}

			internal ZString ShipmentControlNumber
			{
				get
				{
					var rffSecion = group7 != null ? (from SegmentGroup8 group8 in group7.Group8 select group8.RFF).FirstOrDefault()
										: group3 != null ? group3.RFF : new RFFSegmentMessageSection(1);
					return D08AMessageUtilities.GetReference(rffSecion, ReferenceCodeQualifierList.WaybillNumber);
				}
			}

			bool IsSplitShipment
			{
				get
				{
					return !isUnassociatedShipment
						   && (from SegmentGroup8 group8 in group7.Group8
							   from SegmentGroup14 group14 in group8.Group14
							   from FTXSegment ftx in group14.FTX
							   where group8.CNT.Count > 0 && ftx.TextLiteral.FreeText1 == "DUMMY"
							   select ftx).Any();
				}
			}

			readonly SegmentGroup7 group7;
			readonly bool isUnassociatedShipment;
			readonly bool linkToTrip;
			readonly SegmentGroup3 group3;
		}

		#endregion

		readonly CUSCARMessage cuscar;
		readonly CUSREPMessage cusrep;
		readonly ZString messageType;
		readonly ZString messageSubType;
	}
}
