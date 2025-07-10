using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Enterprise.Edifact;
using Enterprise.Edifact.D99A;
using Enterprise.Edifact.D99A.Elements;
using Enterprise.Edifact.D99A.Messages.APERAK;
using Enterprise.Edifact.D99A.Segments;

namespace Enterprise.Freight.Agency.Business
{
	public static class EIDOResponseDecoder
	{
		/// <summary>
		/// Decode D99A APERAK message text into a IEIDOResponseMessage object.
		/// </summary>
		/// <param name="messageText">The text to decode.</param>
		/// <returns>The decoded content of the message.</returns>
		public static IEIDOResponseMessage Parse(string messageText)
		{
			object segment = NewMessageFactory().GetMessage(CharacterSet, messageText);
			APERAKMessage message = segment as APERAKMessage;

			if (message == null)
			{
				throw new InvalidFormatException("Corrupted or Malformed D99A APERAK Response Message. Cannot Process.");
			}
			else
			{
				return new EIDOResponseMessage(message);
			}
		}

		static MessageFactory NewMessageFactory()
		{
			return new D99AMessageFactory();
		}

		#region CharacterSet

		static UNCharacterSet CharacterSet
		{
			get { return new UNOACharacterSet(); }
		}

		#endregion

		#region EIDOResponseMessage

		sealed class EIDOResponseMessage : IEIDOResponseMessage
		{
			public EIDOResponseMessage(APERAKMessage message)
			{
				if (message.UNH.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} UNH segments in group0", message.UNH.Count));
				}

				if (message.BGM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} BGM segments in group0", message.BGM.Count));
				}

				if (message.DTM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} DTM segments in group0", message.DTM.Count));
				}

				if (message.Group1.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} group1's in group0", message.Group1.Count));
				}

				SegmentGroup1 group1 = message.Group1[0];
				if (group1.DOC.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} DOC segments in group1", group1.DOC.Count));
				}

				if (group1.DTM.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} DTM segments in group1", group1.DTM.Count));
				}

				responseType = TypeFromQualifier(message.BGM[0].ResponseTypeCoded);
				responseDateTime = EdifactDateParser.GetDate(message.DTM[0]);

				documentReference = group1.DOC[0].DocumentMessageDetails.DocumentMessageNumber;
				documentIssuedDate = EdifactDateParser.GetDate(group1.DTM[0]);

				foreach (SegmentGroup3 group3 in message.Group3)
				{
					foreach (NADSegment nAD in group3.NAD)
					{
						if (nAD.PartyQualifier == PartyQualifierList.DocumentMessageIssuerSender)
						{
							messageSender = nAD.PartyIdentificationDetails.PartyIdentification;
						}
						else if (nAD.PartyQualifier == PartyQualifierList.MessageRecipient)
						{
							messageReceipient = nAD.PartyIdentificationDetails.PartyIdentification;
						}
					}
				}

				List<IEIDOResponseError> errors = new List<IEIDOResponseError>();
				foreach (SegmentGroup4 group4 in message.Group4)
				{
					errors.Add(new EIDOResponseError(group4));
				}

				this.errors = errors.ToArray();
			}

			#region IEIDOResponseMessage Members

			EIDOResponseType IEIDOResponseMessage.ResponseType
			{
				get { return responseType; }
			}
			DateTime IEIDOResponseMessage.ResponseDateTime
			{
				get { return responseDateTime; }
			}
			string IEIDOResponseMessage.DocumentReference
			{
				get { return documentReference; }
			}
			DateTime IEIDOResponseMessage.DocumentIssuedDate
			{
				get { return documentIssuedDate; }
			}
			string IEIDOResponseMessage.MessageSender
			{
				get { return messageSender; }
			}
			string IEIDOResponseMessage.MessageRecipient
			{
				get { return messageReceipient; }
			}
			IEnumerable<IEIDOResponseError> IEIDOResponseMessage.Errors
			{
				get { return errors; }
			}

			#endregion

			static EIDOResponseType TypeFromQualifier(ResponseTypeCodedList qualifier)
			{
				if (qualifier == ResponseTypeCodedList.Accepted)
				{
					return EIDOResponseType.Accepted;
				}

				if (qualifier == ResponseTypeCodedList.ConditionallyAccepted)
				{
					return EIDOResponseType.Received;
				}

				if (qualifier == ResponseTypeCodedList.Rejected)
				{
					return EIDOResponseType.Rejected;
				}

				throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unknown response type ({0})", qualifier.ToString()));
			}

			readonly EIDOResponseType responseType;
			readonly DateTime responseDateTime;
			readonly string documentReference;
			readonly DateTime documentIssuedDate;
			readonly string messageSender;
			readonly string messageReceipient;
			readonly IEIDOResponseError[] errors;
		}

		#endregion

		#region EIDOResponseError

		sealed class EIDOResponseError : IEIDOResponseError
		{
			public EIDOResponseError(SegmentGroup4 segmentGroup)
			{
				if (segmentGroup.ERC.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} ERC segments in group4", segmentGroup.ERC.Count));
				}

				if (segmentGroup.FTX.Count != 1)
				{
					throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Found {0} FTX segments in group4", segmentGroup.FTX.Count));
				}

				this.code = segmentGroup.ERC[0].ApplicationErrorDetail.ApplicationErrorIdentification;
				this.description = FTXString(segmentGroup.FTX[0]);

				foreach (SegmentGroup5 group5 in segmentGroup.Group5)
				{
					foreach (RFFSegment segment in group5.RFF)
					{
						reference = segment.Reference.ReferenceNumber;
						errorRefType = ErrorRefTypeFromCode(segment.Reference.ReferenceQualifier);
					}
				}
			}

			static string FTXString(FTXSegment segment)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendLine(segment.TextLiteral.FreeText1);
				builder.AppendLine(segment.TextLiteral.FreeText2);
				builder.AppendLine(segment.TextLiteral.FreeText3);
				builder.AppendLine(segment.TextLiteral.FreeText4);
				builder.AppendLine(segment.TextLiteral.FreeText5);

				while (char.IsWhiteSpace(builder[builder.Length - 1]))
				{
					builder.Length--;
				}

				return builder.ToString();
			}
			static EIDOResponseErrorRefType ErrorRefTypeFromCode(ReferenceQualifierList qualifier)
			{
				string code = qualifier.ToString();

				switch (code)
				{
					case "EQD":
						return EIDOResponseErrorRefType.Equipment;
					case "CNI":
						return EIDOResponseErrorRefType.Consgnment;
					case "GID":
						return EIDOResponseErrorRefType.GoodsItem;
					default:
						throw new InvalidFormatException(string.Format(CultureInfo.InvariantCulture, "Unrecognised error reference type ({0})", code));
				}
			}

			#region IEIDOResponseError Members

			string IEIDOResponseError.Code
			{
				get { return code; }
			}
			string IEIDOResponseError.Description
			{
				get { return description; }
			}
			string IEIDOResponseError.Reference
			{
				get { return reference; }
			}
			EIDOResponseErrorRefType IEIDOResponseError.ErrorRefType
			{
				get { return errorRefType; }
			}

			#endregion

			readonly string code;
			readonly string description;
			readonly string reference;
			readonly EIDOResponseErrorRefType errorRefType;
		}

		#endregion
	}
}


