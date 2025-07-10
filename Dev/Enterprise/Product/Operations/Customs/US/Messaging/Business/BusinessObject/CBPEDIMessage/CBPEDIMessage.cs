using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business
{
	public interface ICBPEDIMessageMessageTextNumberPlaceHolderFiller
	{
		string Fill(CBPEDIMessage message);
	}

	public abstract class CBPEDIMessage : BaseEDIMessage
	{
		protected CBPEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static ZQuery AMSFilter => new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.AMS);

		public static ZQuery AllCBPFilter
		{
			get
			{
				var result = new ZQuery(EDIMessageSchema.EM_ApplicationCode, new ZString[] {
					CBPEDIMessage.ApplicationCodes.USCustomsExport,
					CBPEDIMessage.ApplicationCodes.USCustomsImport
				});
				result.AddToFilter(AMSFilter, JoinCondition.Or);
				return result;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!AllowExceeding9999Limit && !IsInDatabase && EM_ReceiveTransmit == Direction.Transmit && HasExceeded9999Limit)
			{
				string message = string.Format(HasExceeded9999LimitExceptionMessageFormat, EM_MessageType, MaxNumMessageBlocksInMessageAllowed);

				throw new ZCannotSaveException(message, HasExceeded9999LimitExceptionHeading);
			}
		}

		public const string HasExceeded9999LimitExceptionMessageFormat =
			"During generation of messages, at least one message '{0}' has exceeded the maximum number message blocks ({1}) allowed by customs.\r\n" +
			"Please modify the job to reduce the size of the messages that are generated.\r\n" +
			"This can be done by changing the merge method or reducing the number of invoice lines on the job.";
		public const string HasExceeded9999LimitExceptionHeading = "Invalid Message";

		#region Related Message Object

		public CBPEDIMessage OriginalMessage
		{
			get
			{
				if (IsTransmitMessage)
				{
					throw new InvalidOperationException("Original Message is only available for response messages. This message is a transmit");
				}
				if (!EM_MessageNum.IsEmpty && (fOriginalMessage == null || fOriginalMessage.EM_MessageNum != EM_MessageNum))//some messages don't have outbound messages to respond to and therefore EM_MessageNum is empty
				{
					fOriginalMessage = new Loader(Factory).LoadTop1WithDirectionOrderByCreatTime(EM_ApplicationCode, EM_MessageNum, CBPEDIMessage.Direction.Transmit, GetExtraOriginalMessageFilter());
				}
				return fOriginalMessage;
			}
		}
		CBPEDIMessage fOriginalMessage;

		public CBPEDIMessage ResponseMessage
		{
			get
			{
				if (!IsTransmitMessage)
				{
					throw new InvalidOperationException("ResponseMessage Message is only available for transmit messages.");
				}
				if (fResponseMessage == null && !EM_MessageNum.IsEmpty)
				{
					fResponseMessage = new Loader(Factory).LoadTop1WithDirectionOrderByCreatTime(EM_ApplicationCode, EM_MessageNum, CBPEDIMessage.Direction.Receive, GetExtraResponseMessageFilter());
				}
				return fResponseMessage;
			}
		}
		CBPEDIMessage fResponseMessage;

		protected override BaseEDIMessage GetRelatedMessageCore() => IsTransmitMessage ? ResponseMessage : OriginalMessage;

		#endregion

		#region New Properties

		public bool HasExceeded9999Limit
		{
			//+2 because A and Y blocks are in the interchange not the message....
			get { return (Math.Ceiling(EM_MessageText.Length / (decimal)BlockControlGenerator.MessageBlockLengthInChars) + 2) > MaxNumMessageBlocksInMessageAllowed; }
		}
		public const int MaxNumMessageBlocksInMessageAllowed = 9999;

		#endregion

		public List<T> GetMessageBlocks<T>() where T : MessageBlock
		{
			return GetMessageBlocks<T>(x => true);
		}

		public List<T> GetMessageBlocks<T>(Predicate<T> match) where T : MessageBlock
		{
			List<T> result = new List<T>();
			if (match != null)
			{
				if (typeof(IControlMessageBlockB).IsAssignableFrom(typeof(T)))
				{
					T block = (T)(object)MessageBlock.B;
					if (match(block))
					{
						result.Add(block);
					}
				}
				else if (typeof(IControlMessageBlockY).IsAssignableFrom(typeof(T)))
				{
					T block = (T)(object)MessageBlock.Y;
					if (match(block))
					{
						result.Add(block);
					}
				}
				else
				{
					MessageBlock.MessageBlocks.ForEach((MessageBlock mb) =>
					{
						T mbasT = mb as T;
						if (mbasT != null && match(mbasT))
						{
							result.Add(mbasT);
						}
					});
				}
			}
			return result;
		}

		public string GetMessageBlockApplicationCode() => GetMessageBlockApplicationCodeCore();

		protected virtual string GetMessageBlockApplicationCodeCore() => EM_ApplicationCode;

		public BlockControlGenerator MessageBlock
		{
			get { return fMessageBlock ?? (fMessageBlock = GetMessageBlockCore()); }
		}
		BlockControlGenerator fMessageBlock;

		BlockControlGenerator GetMessageBlockCore()
		{
			try
			{
				return GetMessageBlock();
			}
			catch (InvalidMessageFormatException ex)
			{
				var innerEx = ex.InnerException;
				if (innerEx != null)
				{
					// Append Message Type to the inner exception by replacing it.
					var replacementMessage = innerEx.Message + "\r\nMessageType=" + EM_MessageType;
					var replacementEx = new InvalidMessageFormatException(replacementMessage, innerEx.InnerException);
					throw new InvalidMessageFormatException(ex.Message, replacementEx);
				}
				else
				{
					// this is not the exception we were looking for.
					throw;
				}
			}
		}

		#region TypeDecider

		public new static readonly CBPEDIMessageTypeDecider TypeDecider = new CBPEDIMessageTypeDecider();

		public class CBPEDIMessageTypeDecider : EDIMessageTypeDecider
		{
			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
#if DEBUG
				var applicationCode = row[EDIMessage.Schema.EM_ApplicationCode].ToString().Trim();

				if (applicationCode == CBPEDIInterchange.ApplicationCodeForTesting)
				{
					return ObjectFactory.GetType<Integration.Customs.US.ICBPMessageForTesting>();
				}
				else
#endif
				{
					return base.GetTypeForLoad(row, factory);
				}
			}
		}

		#endregion

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : EDIMessage.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CBPEDIMessage LoadTop1WithDirectionOrderByCreatTime(ZString applicationCode, ZString messageNum, ZString receiveTransmit, ZQuery extraFilter = null)
			{
				ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode)
				{
					OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " DESC",
				};
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, receiveTransmit);

				if (extraFilter != null)
				{
					query.AddToFilter(extraFilter);
				}
				return Factory.LoadTop1<CBPEDIMessage>(query);
			}

			public CBPEDIMessage LoadTop1(ZString applicationCode, ZString messageType, ZString messageNum)
			{
				ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, applicationCode);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);
				return Factory.LoadTop1<CBPEDIMessage>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CBPEDIMessage);
			}
		}

		#endregion

		#region Overriden Properties

		public override ZString EM_MessageType
		{
			get { return base.EM_MessageType; }
			set
			{
				ZString oldValue = EM_MessageType;
				base.EM_MessageType = value;
				if (oldValue != EM_MessageType)
				{
					fMessageBlock = null;
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get
			{
				if (fEM_MessageInterpretation.IsEmpty && !EM_MessageText.IsEmpty)
				{
					try
					{
						fEM_MessageInterpretation = MessageBlock.Serialise(true, GetOriginalApplicationIdentifierIfNeeded());
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						fEM_MessageInterpretation = GetExceptionMessage(ex);
					}
				}

				return fEM_MessageInterpretation;
			}
			set
			{
				throw new NotSupportedException("Setting EM_MessageInterpretation is not supported");
			}
		}
		ZString fEM_MessageInterpretation;

		ZString GetExceptionMessage(Exception ex)
		{
			var result = new ZStringBuilder();
			var exToCheck = ex;
			while (exToCheck != null)
			{
				result.AppendIfNotEmpty(exToCheck.Message);
				exToCheck = exToCheck.InnerException as InvalidMessageFormatException;
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		string GetOriginalApplicationIdentifierIfNeeded()
		{
			string result = "";
			if (EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)
			{
				var originalMessage = OriginalMessage;
				result = originalMessage != null ? originalMessage.EM_MessageType : ZString.Empty;
			}
			return result;
		}
		#endregion

		#region Implementation

		protected virtual ZQuery GetExtraOriginalMessageFilter()
		{
			return null;
		}

		protected virtual ZQuery GetExtraResponseMessageFilter()
		{
			return null;
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			int originalLength = EM_MessageText.Length;
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			if (originalLength != EM_MessageText.Length)
			{
				ErrorReporter.ReportOnce("CBPEDIMessage.GetNumberFountainNumbersAndFillInPlaceHoldersBase" + GetType().FullName, string.Format(CultureInfo.InvariantCulture, "Replacing placeholders in '{0} (PK:{1}) ' changed message length", GetType().FullName, PK));
			}

			ICBPEDIMessageMessageTextNumberPlaceHolderFiller filler = EM_LinkedObject as ICBPEDIMessageMessageTextNumberPlaceHolderFiller;
			if (filler != null)
			{
				originalLength = EM_MessageText.Length;
				var information = filler.Fill(this);
				if (originalLength != EM_MessageText.Length)
				{
					ErrorReporter.ReportOnce("CBPEDIMessage.GetNumberFountainNumbersAndFillInPlaceHolders" + GetType().FullName, string.Format(CultureInfo.InvariantCulture, "Replacing placeholders in '{0}' changed message length. Detail: {1}", filler.GetType().FullName, information));
				}
			}
		}

		internal bool IsMessageSendWithMessageErrorsCorrect()
		{
			return IsMessageSendWithMessageErrorsCorrectCore();
		}

		protected virtual bool IsMessageSendWithMessageErrorsCorrectCore()
		{
			return true;
		}

		protected abstract BlockControlGenerator GetMessageBlock();

		protected abstract bool AllowExceeding9999Limit { get; }

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return new CBPEDMessageStreamFormatter();
			}
		}

		protected override bool ResetToQueuedStatusPreservesMessageType => true;

		protected override bool ResetToQueuedStatusPreservesMessageSubType => true;

		protected override string MessageNumberPlaceHolderOverride
		{
			get { return MessageNumberPlaceHolder; }
		}

		protected override void PopulateMessageNumber()
		{
			PopulateNumberPropertyIfRequired<ZString>(EM_MessageNumInfo, x => MessageNumberStrategy?.GetMessageReferenceNumber() ?? GetMessageReferenceNumber());

			if (EM_MessageText.Contains(MessageNumberPlaceHolderOverride))
			{
				originalMessageTextWithPlaceHolder = EM_MessageText;
				EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolderOverride, EM_MessageNum.PadRight(MessageNumberPlaceHolderOverride.Length));
			}
		}
		ZString originalMessageTextWithPlaceHolder;

		public override ZString EM_MessageText
		{
			get { return base.EM_MessageText; }
			set
			{
				ZString oldValue = EM_MessageText;
				base.EM_MessageText = ShouldConvertMessageTextToUpper ? value.ToUpper() : value;
				if (oldValue != EM_MessageText)
				{
					fMessageBlock = null;
					fEM_MessageInterpretation = ZString.Empty;
					EM_MessageInterpretationInfo.RefreshBinding();
				}
			}
		}

		protected virtual bool ShouldConvertMessageTextToUpper
		{
			get { return true; }
		}

		protected override ZString MessageTypeDescriptionCore
		{
			get { return ZString.Empty; }
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				EM_MessageNum = ZString.Empty;

				if (!originalMessageTextWithPlaceHolder.IsEmpty)
				{
					EM_MessageText = originalMessageTextWithPlaceHolder;
				}
			}

			base.OnSaved(saveSucceeded);
		}

		#endregion
	}
}
