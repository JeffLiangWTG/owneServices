using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	public abstract class BaseEDIMessage : EDIMessage
	{
		protected BaseEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EDIMessage.Schema
		{
			public const string EM_RelatedMessageFormattedMessageText = "EM_RelatedMessageFormattedMessageText";
			public const string EM_RelatedMessageInterpretationText = "EM_RelatedMessageInterpretationText";
			public const string EM_RelatedMessageCreateTime = "EM_RelatedMessageCreateTime";
			public const string EM_RelatedMessageCreateUser = "EM_RelatedMessageCreateUser";
			public const string MessageTypeDescription = "MessageTypeDescription";
		}

		#region Property

		#region EM_RelatedMessageFormattedMessageText

		public ZString EM_RelatedMessageFormattedMessageText => RelatedMessage?.EM_FormattedMessageText ?? ZString.Empty;

		public ZPropertyInfo EM_RelatedMessageFormattedMessageTextInfo => GetZPropertyInfo(Schema.EM_RelatedMessageFormattedMessageText);

		#endregion

		#region EM_RelatedMessageInterpretationText

		public ZString EM_RelatedMessageInterpretationText => RelatedMessage?.EM_MessageInterpretation ?? ZString.Empty;

		public ZPropertyInfo EM_RelatedMessageInterpretationTextInfo => GetZPropertyInfo(Schema.EM_RelatedMessageInterpretationText);

		#endregion

		#region EM_RelatedMessageCreateTime

		public ZDateTime EM_RelatedMessageCreateTime => RelatedMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty;

		public ZPropertyInfo EM_RelatedMessageCreateTimeInfo => GetZPropertyInfo(Schema.EM_RelatedMessageCreateTime);

		#endregion

		#region EM_RelatedMessageCreateUser

		public ZString EM_RelatedMessageCreateUser => RelatedMessage?.EM_User ?? ZString.Empty;

		public ZPropertyInfo EM_RelatedMessageCreateUserInfo => GetZPropertyInfo(Schema.EM_RelatedMessageCreateUser);

		#endregion

		#region EM_SendWithMessageErrorsFormatted

		[ResourceStringData("EDIMessage|EM_SendWithMessageErrorsFormatted", Caption = "Sent With Errors")]
		public ZString EM_SendWithMessageErrorsFormatted
		{
			get
			{
				var result = EM_SendWithMessageErrors ? "Yes" : "No";
				if (EM_ReceiveTransmit != ReceiveTransmitList.Codes.Transmit)
				{
					result = "N/A";
				}
				return result;
			}
		}

		#endregion

		#region MessageTypeDescriptionCore

		public ZString MessageTypeDescription => MessageTypeDescriptionCore;

		protected abstract ZString MessageTypeDescriptionCore { get; }

		public ZPropertyInfo MessageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.MessageTypeDescription); }
		}

		#endregion

		public virtual bool IsInterpretationInHtmlFormat => false;

		public virtual bool HasStatusOrErrors => true;

		#endregion

		#region Related Business Object

		#region RelatedMessage

		public BaseEDIMessage RelatedMessage => GetRelatedMessageCore();

		protected virtual BaseEDIMessage GetRelatedMessageCore() => this;

		public bool HasRelatedMessage => RelatedMessage != null;

		#endregion

		#endregion

		#region Statuses And Errors

		public StatusErrorsDataViewCollection StatusesAndErrors
		{
			get
			{
				if (statusesAndErrors == null)
				{
					try
					{
						statusesAndErrors = GetStatusErrorsDataViewCollection();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						statusesAndErrors = new StatusErrorsDataViewCollection(this.Factory);
					}
				}
				return statusesAndErrors;
			}
		}
		StatusErrorsDataViewCollection statusesAndErrors;

		protected virtual StatusErrorsDataViewCollection GetStatusErrorsDataViewCollection()
		{
			return new StatusErrorsDataViewCollection(Factory);
		}

		public bool StatusesAndErrorsVisible => IsTransmitMessage;

		public ZString StatusesErrorsExist => IsTransmitMessage ? "No Statuses/Errors available on outgoing messages" : string.Empty;

		public virtual ZString CountOfReconOriginalEntries => ZString.Empty;

		#endregion

		#region Type Decider

		public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

		#endregion
	}
}
