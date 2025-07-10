using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Edifact;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using D05B = Enterprise.Edifact.D05B;
using D95B = Enterprise.Edifact.D95B;

namespace Enterprise.Customs.US.AMS.Messaging
{
	public class StowPlanMessage : EDIMessage, Integration.Customs.US.USAMS.IStowPlanMessage
	{
		public StowPlanMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", EDIInterchange.ApplicationCodes.StowPlan, EDIInterchange.InterchangePartyIDs.AMSMailbox).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIInterchange.ApplicationCodes.StowPlan;
			EM_MessageType = EDIMessage.ApplicationCodes.StowPlan;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(VoyageDestination);
			yield return typeof(VoyageOrigin);
		}

		#region Transmit

		D95B.Messages.BAPLIE.BAPLIEMessage TransmitContent
		{
			get
			{
				if (fTransmitContent == null && this.EM_ReceiveTransmit == Direction.Transmit)
				{
					fTransmitContent = this.GetAutoEdifactMessageUsingNamedFactory(new D95B.EdifactD95BMessageFactory(), new UNOACharacterSet()) as D95B.Messages.BAPLIE.BAPLIEMessage;
				}
				return fTransmitContent;
			}
		}
		D95B.Messages.BAPLIE.BAPLIEMessage fTransmitContent;

		IEnumerable<ZString> TransmitEquipmentNumbers
		{
			get
			{
				if (fTransmitEquipmentNumbers == null)
				{
					fTransmitEquipmentNumbers = new List<ZString>();
					if (TransmitContent != null)
					{
						fTransmitEquipmentNumbers.AddRange(TransmitContent.Group2.Cast<D95B.Messages.BAPLIE.SegmentGroup2>().SelectMany(
							x => x.Group3.Cast<D95B.Messages.BAPLIE.SegmentGroup3>().SelectMany(
								y => y.EQD.Cast<D95B.Segments.EQDSegment>().Where(z => !string.IsNullOrEmpty(z.EquipmentIdentification.EquipmentIdentificationNumber))
								.Select(z => (ZString)z.EquipmentIdentification.EquipmentIdentificationNumber))));
					}
				}
				return fTransmitEquipmentNumbers;
			}
		}
		List<ZString> fTransmitEquipmentNumbers;

		#endregion

		#region Receive

		internal D05B.Messages.CUSRES.CUSRESMessage ReceiveContent
		{
			get
			{
				if (fReceiveContent == null && this.EM_ReceiveTransmit == Direction.Receive)
				{
					fReceiveContent = this.GetAutoEdifactMessageUsingNamedFactory(new D05B.EdifactD05BMessageFactory(), new UNOACharacterSet()) as D05B.Messages.CUSRES.CUSRESMessage;
				}
				return fReceiveContent;
			}
		}
		D05B.Messages.CUSRES.CUSRESMessage fReceiveContent;

		IEnumerable<ZString> AcceptedEquipmentNumbers
		{
			get
			{
				if (fAcceptedEquipmentNumbers == null)
				{
					fAcceptedEquipmentNumbers = new List<ZString>();
					if (ReceiveContent != null)
					{
						var isAccepted = ReceiveContent.Group4.Cast<D05B.Messages.CUSRES.SegmentGroup4>().SelectMany(
							x => x.ERC.Cast<D05B.Segments.ERCSegment>().Select(y => y.ApplicationErrorDetail.ApplicationErrorCode))
							.Any(x => x == CusresErrorCodeList.Codes.Acceptance || x == CusresErrorCodeList.Codes.AcceptanceWithWarnings);
						var originalMessage = OriginalMessage;

						if (isAccepted && originalMessage != null)
						{
							fAcceptedEquipmentNumbers.AddRange(originalMessage.TransmitEquipmentNumbers);
						}
					}
				}
				return fAcceptedEquipmentNumbers;
			}
		}
		List<ZString> fAcceptedEquipmentNumbers;

		internal StowPlanMessage OriginalMessage
		{
			get
			{
				if (fOriginalMessage == null && ReceiveContent != null)
				{
					var bgm = ReceiveContent.BGM.Cast<D05B.Segments.BGMSegment>().FirstOrDefault();
					if (bgm != null)
					{
						var docMsgId = bgm.DocumentMessageIdentification;
						if (docMsgId != null && !string.IsNullOrEmpty(docMsgId.DocumentIdentifier))
						{
							var query = new ZQuery(EDIMessageSchema.EM_MessageNum, docMsgId.DocumentIdentifier);
							query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.StowPlan);
							query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, AMSEDIMessage.Direction.Transmit);
							query.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Descending;
							fOriginalMessage = Factory.LoadTop1<StowPlanMessage>(query);
						}
					}
				}
				return fOriginalMessage;
			}
		}
		StowPlanMessage fOriginalMessage;

		public ZInt CountAcceptedContainersWhichPreviouslyNotAccepted()
		{
			var result = ZInt.Zero;
			var msgColProvider = this.EM_LinkedObject as IEDIMessageCollectionProvider;
			if (msgColProvider == null && this.OriginalMessage != null)
			{
				msgColProvider = this.OriginalMessage.EM_LinkedObject as IEDIMessageCollectionProvider;
			}
			if (msgColProvider != null)
			{
				var previousAcceptedContainers = new List<ZString>(msgColProvider.Messages.OfType<StowPlanMessage>().
					Where(x => x.EM_SystemCreateTimeUtc < this.EM_SystemCreateTimeUtc && x.EM_ReceiveTransmit == Direction.Receive)
					.SelectMany(x => x.AcceptedEquipmentNumbers).Distinct());
				result = this.AcceptedEquipmentNumbers.Count(x => !previousAcceptedContainers.Contains(x));
			}
			return result;
		}

		#endregion
	}
}
