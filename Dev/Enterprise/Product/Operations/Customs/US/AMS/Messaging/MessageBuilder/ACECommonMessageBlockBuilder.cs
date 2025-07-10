using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class ACECommonMessageBlockBuilder
	{
		public ACECommonMessageBlockBuilder(IACEBillManifestMessageAttachee attachee)
		{
			this.attachee = attachee;
		}

		public IEnumerable<MessageBlock> Build()
		{
			return BuildCore();
		}

		protected virtual IEnumerable<MessageBlock> BuildCore()
		{
			var messageBlocks = new List<MessageBlock>();
			AddManifestRecord(messageBlocks);
			AddCarrierAssignedBatchNumber(messageBlocks);
			AddUniqueVoyageIdentifier(messageBlocks);
			AddPortRecord(messageBlocks, attachee.PortDetails);
			return messageBlocks;
		}

		protected void AddManifestRecord(List<MessageBlock> messageBlocks)
		{
			var inpm01 = new INPM01()
			{
				CarrierCode = attachee.CarrierCode,
				ModeOfTransportationCode = attachee.ModeOfTransportationCode,
				VesselCountryCode = attachee.ConveyanceCountryCode,
				VoyageNumber = attachee.VoyageNumber,
				ManifestSequenceNumber = attachee.ManifestSequenceNumber, // this is not needed as it will cause multiple match.
				VesselCode = attachee.ConveyanceCode
			};
			messageBlocks.Add(inpm01);

			if (attachee.ConveyanceCode.IsEmpty)
			{
				inpm01.VesselName = attachee.ConveyanceName.Left(23);
			}
		}

		protected void AddUniqueVoyageIdentifier(List<MessageBlock> messageBlocks)
		{
			if (IsUniqueVoyageIdentifierAllowed)
			{
				var uniqueVoyageIdentifier = attachee.UniqueVoyageIdentifier;
				if (!uniqueVoyageIdentifier.IsEmpty)
				{
					messageBlocks.Add(new INPB04()
					{
						ReferenceIdentifierQualifier = UniqueVoyageIdentifierQualifier,
						ReferenceIdentifier = uniqueVoyageIdentifier
					});
				}
			}
		}
		const string UniqueVoyageIdentifierQualifier = "V3";

		protected virtual bool IsUniqueVoyageIdentifierAllowed
		{
			get { return false; }
		}

		protected void AddCarrierAssignedBatchNumber(List<MessageBlock> messageBlocks)
		{
			messageBlocks.Add(new INPM02() { CarrierAssignedBatchNumber = CarrierAssignedBatchNumberCreator.CreateNumber(attachee.BillOfLadingDetails == null ? new ZString('0', 12) : attachee.BillOfLadingDetails.BillOfLadingSequenceNumber.Right(12)) });
		}

		protected void AddPortRecord(List<MessageBlock> messageBlocks, IPort portDetails)
		{
			if (portDetails != null)
			{
				messageBlocks.Add(new INPP01()
				{
					PortOfUnladingCode = portDetails.DistrictPortOfUnladingCode,
					OriginalEstimatedDate = portDetails.OriginalEstimatedDate
				});
			}
		}

		protected readonly IACEBillManifestMessageAttachee attachee;
	}
}
