using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.CONTRL;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Messaging.MessageProcessors;
using D96BMessageFactory = Enterprise.Edifact.D96B.EdifactD96BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CONTRLMessageHelper : MessageHelper
	{
		public CONTRLMessageHelper(ZAMessage ediMessage, CONTRLMessage message)
			: base(ediMessage)
		{
			contrlMessage = Argument.NotNull(message, "message");
			interchangeTime = ZDateTime.Invalid;
		}

		readonly CONTRLMessage contrlMessage;

		public static CONTRLMessageHelper New(ZAMessage message)
		{
			CONTRLMessageHelper result = null;
			if (message != null)
			{
				var d96bMessageFactory = new D96BMessageFactory();
				var zaCharSet = new ZACharacterSet();
				CONTRLMessage contrlMessage = (CONTRLMessage)message.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet);
				if (contrlMessage != null)
				{
					result = new CONTRLMessageHelper(message, contrlMessage);
					result.interchangeTime = message.EM_DateTimeInterchangeSent;
				}
			}
			return result;
		}

		#region Implementation

		public ZString MessageReferenceNumber
		{
			get
			{
				if (messageReferenceNumber == null)
				{
					messageReferenceNumber = UCMSegment?.MessageReferenceNumber ?? ZString.Empty;
				}
				return messageReferenceNumber;
			}
		}
		string messageReferenceNumber;

		internal ZString InterpretContrlFor(ZAMessage outGoingMessage)
		{
			if (ActionCodedForMessage == ActionCodedList.AcknowledgedErrorsDetectedAndReported ||
				ActionCodedForMessage == ActionCodedList.OneOrMoreRejectedNextLowerLevel ||
				ActionCodedForMessage == ActionCodedList.ThisLevelAndAllLowerLevelsRejected ||
				ActionCodedForMessage == ActionCodedList.UnbUnzRejected)
			{
				return InterpretError(outGoingMessage);
			}
			else if (ActionCodedForMessage == ActionCodedList.AcknowledgedThisLevelAndAllLowerLevels ||
				ActionCodedForMessage == ActionCodedList.InterchangeReceived ||
				ActionCodedForMessage == ActionCodedList.ThisLevelAcknowledgedNextLowerLevelAcknowledgedIfNotExplicitlyRejected ||
				ActionCodedForMessage == ActionCodedList.UnbUnzAccepted)
			{
				return "Message " + MessageReferenceNumber + " was acknowledged";
			}
			else
			{
				return "CONTRL message received";
			}
		}

		ZString InterpretError(ZAMessage outGoingMessage)
		{
			var errors = new List<ErrorInformation>();
			foreach (SegmentGroup1 grp1 in contrlMessage.Group1)
			{
				foreach (SegmentGroup2 grp2 in grp1.Group2)
				{
					UCSSegment ucs = grp2.UCS[0];
					UCDSegment ucd = grp2.UCD[0];
					var originalSegmentNumber = ucs.SegmentPositionInMessage;
					var segmentErrorNumber = ucs.SyntaxErrorCoded;
					var elementNumber = ucd.DataElementIdentification.ErroneousDataElementPositionInSegment;
					var subElement = ucd.DataElementIdentification.ErroneousComponentDataElementPosition;
					var ucdErrorNumber = ucd.SyntaxErrorCoded;
					errors.Add(new ErrorInformation(originalSegmentNumber, segmentErrorNumber, elementNumber, subElement, ucdErrorNumber));
				}
			}

			if (outGoingMessage.CharacterSet != null)
			{
				var originalMessageText = outGoingMessage.EM_MessageText;
				var originalSegments = Regex.Split(originalMessageText, @"\" + outGoingMessage.CharacterSet.SegmentDelimiter);

				foreach (var error in errors)
				{
					error.ExtractValuesFromOriginalOutgoingMessage(outGoingMessage, originalSegments);
				}

				var table = new HtmlTableCreator(new[] { "Error Code", "Segment", "Element", "Subelement" });
				foreach (var e in errors)
				{
					table.WriteRow(e.ErrorTextFromNumbers(), e.OutputSegment, e.OutputElement, e.OutputSubElement);
				}
				return table.ToHtml();
			}
			else
			{
				return "";
			}
		}

		public ActionCodedList ActionCodedForMessage
		{
			get
			{
				if (actionCodedForMessage == null)
				{
					actionCodedForMessage = UCMSegment?.ActionCoded;
				}
				return actionCodedForMessage;
			}
		}
		ActionCodedList actionCodedForMessage;

		public ZString MessageType
		{
			get
			{
				if (messageType == null)
				{
					messageType = UCMSegment?.MessageIdentifier?.MessageType.ToString() ?? ZString.Empty;
				}
				return messageType;
			}
		}
		string messageType;

		UCMSegment UCMSegment
		{
			get
			{
				if (ucmSegment == null)
				{
					ucmSegment = contrlMessage?.Group1?[0]?.UCM[0];
				}
				return ucmSegment;
			}
		}
		UCMSegment ucmSegment;

		#endregion
	}
}
