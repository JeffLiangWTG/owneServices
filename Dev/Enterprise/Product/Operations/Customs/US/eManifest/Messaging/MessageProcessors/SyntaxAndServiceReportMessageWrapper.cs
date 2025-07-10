using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.CONTRL;
using Enterprise.Edifact.D08A.Segments;
using static Enterprise.Integration.Customs.US.eManifest;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	class SyntaxAndServiceReportMessageWrapper
	{
		internal SyntaxAndServiceReportMessageWrapper(CONTRLMessage contrl)
		{
			this.contrl = Argument.NotNull(contrl, "cusres", "Supported EDIFACT message type is D03B CONTRL");
		}

		internal IEnumerable<MessageResponse> MessageResponses
		{
			get
			{
				var result = from SegmentGroup1 group1 in contrl.Group1
							 select new MessageResponse(group1);

				result = result.Concat(from SegmentGroup3 group3 in contrl.Group3
									   from SegmentGroup4 group4 in group3.Group4
									   select new MessageResponse(group4));
				return result;
			}
		}

		#region MessageResponse

		internal class MessageResponse
		{
			#region Constructors

			internal MessageResponse(SegmentGroup1 group1)
			{
				ucm = group1.UCM.Count > 0 ? group1.UCM[0] : new UCMSegment();
				syntaxErrors = from SegmentGroup2 group2 in group1.Group2 select new SyntaxErrorGroup { UCS = group2.UCS, UCD = group2.UCD };
			}

			internal MessageResponse(SegmentGroup4 group4)
			{
				ucm = group4.UCM.Count > 0 ? group4.UCM[0] : new UCMSegment();
				syntaxErrors = from SegmentGroup5 group5 in group4.Group5 select new SyntaxErrorGroup { UCS = group5.UCS, UCD = group5.UCD };
			}

			class SyntaxErrorGroup
			{
				internal UCSSegmentMessageSection UCS { get; set; }
				internal UCDSegmentMessageSection UCD { get; set; }
			}

			#endregion

			internal ZString MessageNumber
			{
				get { return ucm.MessageReferenceNumber; }
			}

			internal bool IsSyntaxError
			{
				get { return MessageStatus == "4"; }
			}

			internal bool IsAcknowledgement
			{
				get { return MessageStatus == "7" || MessageStatus == "8"; }
			}

			ZString MessageStatus
			{
				get { return ucm.ActionCoded; }
			}

			internal IEnumerable<SyntaxError> GetSyntaxErrors(ZString sourceMessageText)
			{
				if (!string.IsNullOrEmpty(ucm.SyntaxErrorCoded))
				{
					yield return new SyntaxError(ucm.SyntaxErrorCoded);
				}

				foreach (var error in syntaxErrors)
				{
					var ucs = error.UCS.Cast<UCSSegment>().FirstOrDefault();
					if (error.UCD.Count == 0)
					{
						yield return new SyntaxError(sourceMessageText, ucs, null);
					}

					foreach (UCDSegment ucd in error.UCD)
					{
						yield return new SyntaxError(sourceMessageText, ucs, ucd);
					}
				}
			}

			internal string GetSourceMessageTextWithErrorMarks(ZString sourceMessageText, IEnumerable<SyntaxError> errors)
			{
				var builder = new ZStringBuilder();
				if (!sourceMessageText.IsEmpty)
				{
					errors = errors as List<SyntaxError> ?? errors.ToList();
					var charset = new UNOACharacterSet();

					var lineNumber = 1;
					foreach (var line in sourceMessageText.TrimEnd(charset.SegmentDelimiterChar).Split(charset.SegmentDelimiterChar))
					{
						var lineNum = lineNumber.ToString(CultureInfo.InvariantCulture);
						var lineErrors = errors.Where(err => err.LineNumber == lineNum).ToList();
						if (lineErrors.Count > 0)
						{
							builder.Append(string.Format("\r\n## {0} - {1}'", lineNumber, line));
							lineErrors.Aggregate(builder, (b, error) => b.Append("## " + error));
							builder.Append(string.Empty);
						}
						else
						{
							builder.Append(string.Format("{0} - {1}'", lineNumber, line));
						}
						lineNumber++;
					}

					errors.Where(err => err.LineNumber.IsEmpty).Aggregate(builder, (b, error) => b.Append("\r\n## " + error));
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}

			#region SyntaxError

			internal class SyntaxError : ITableInterpretation
			{
				internal SyntaxError(ZString code)
				{
					errorMessage = GetErrorMessage(code);
				}

				internal SyntaxError(ZString sourceMessage, UCSSegment ucs, UCDSegment ucd)
				{
					Argument.NotNull(ucs, "ucs");

					LineNumber = ucs.SegmentPositionInMessage;
					errorMessage = GetErrorMessage(ucd != null ? ucd.SyntaxErrorCoded : ucs.SyntaxErrorCoded);

					var positionBuilder = new StringBuilder();
					positionBuilder.AppendFormat("L: {0}", LineNumber);
					if (!sourceMessage.IsEmpty)
					{
						var charset = new UNOACharacterSet();
						sourceLine = sourceMessage.Split(charset.SegmentDelimiterChar).ElementAtOrDefault(ZInt.ParseEmptyAsZero(LineNumber) - 1);

						if (ucd != null)
						{
							var element = ucd.DataElementIdentification.ErroneousDataElementPositionInSegment;
							var component = ucd.DataElementIdentification.ErroneousComponentDataElementPosition;
							var elementValue = sourceLine.Split(charset.ElementDelimiterChar).ElementAtOrDefault(ZInt.ParseEmptyAsZero(element) - 1);
							componentValue = elementValue.Split(charset.SubElementDelimiterChar).ElementAtOrDefault(ZInt.ParseEmptyAsZero(component) - 1);
							positionBuilder.AppendFormat("; P: {0},{1}", element, component);
						}
					}
					position = positionBuilder.ToString();
				}

				static ZString GetErrorMessage(string code)
				{
					var syntaxError = new Enterprise.Messaging.MessageProcessors.SyntaxError(code);
					var builder = new ZStringBuilder();
					builder.AppendIfNotEmpty(syntaxError.Code);
					builder.AppendIfNotEmpty(syntaxError.Description);
					return builder.ToStringWithDelimiterBetweenAppends(" - ");
				}

				public override string ToString()
				{
					var builder = new StringBuilder();
					builder.AppendFormat("Error Message: {0}", errorMessage);
					if (!position.IsEmpty)
					{
						builder.AppendFormat(",  Component Value: '{0}', Position: {1}.", componentValue, position);
					}
					return builder.ToString();
				}

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return "Syntax Error Messages"; }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get
					{
						yield return "Error Message";
						yield return "Component Value";
						yield return "Source Line";
						yield return "Position";
					}
				}

				IEnumerable<object> ITableValues.Values
				{
					get { return new object[] { errorMessage, componentValue, sourceLine, position }; }
				}

				#endregion

				internal ZString LineNumber { get; private set; }
				readonly ZString errorMessage;
				readonly ZString componentValue;
				readonly ZString sourceLine;
				readonly ZString position;
			}

			#endregion

			readonly UCMSegment ucm;
			readonly IEnumerable<SyntaxErrorGroup> syntaxErrors;
		}

		#endregion

		readonly CONTRLMessage contrl;
	}

	#region SyntaxErrorsProvider

	class SyntaxErrorsProvider : ISyntaxErrorsProvider
	{
		public string GetSourceMessageTextWithErrorMarks(IEDIMessage ediMessage)
		{
			var result = string.Empty;
			var message = ediMessage as SyntaxErrorMessage;
			if (message != null)
			{
				var contrl = message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet()) as CONTRLMessage;
				if (contrl != null)
				{
					var original = message.OriginalMessage;
					var wrapper = new SyntaxAndServiceReportMessageWrapper(contrl);
					var response = wrapper.MessageResponses.FirstOrDefault(
						r => r.MessageNumber == original.EM_MessageNum);

					if (response != null)
					{
						var sourceMessageText = original.EM_MessageText;
						var syntaxErrors = response.GetSyntaxErrors(sourceMessageText);
						result = response.GetSourceMessageTextWithErrorMarks(sourceMessageText, syntaxErrors);
					}
				}
			}
			return result;
		}
	}

	#endregion
}
