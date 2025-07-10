using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class MessageActionRelatedRecordWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MessageActionRelatedRecordWrapper(IMessageAttacheeInDeclaration relatedRecord, MessagesToShowCollection.MessagesStatus messageStatus)
				: base(relatedRecord.Factory)
		{
			this.relatedRecord = relatedRecord;
			this.messageStatus = messageStatus;
		}

		/// <summary>
		/// DO NOT USE THIS! This is to satisfy GUI tests that attempt to add a new element. Users cannot add an element
		/// </summary>
		/// <param name="factory"></param>
		public MessageActionRelatedRecordWrapper(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string HumanFriendlyReference = "HumanFriendlyReference";
			public const string ShouldSendMessage = "ShouldSendMessage";
			public const string Status = "Status";
			public const string StatusDesc = "StatusDesc";
			public const string EntryStatus = "EntryStatus";
			public const string EntryStatusDesc = "EntryStatusDesc";
			public const string ReleaseDate = "ReleaseDate";
			public const string TIBExpiryDate = "TIBExpiryDate";
			public const string TIBNumOfExtensions = "TIBNumOfExtensions";
		}

		#endregion

		internal readonly IMessageAttacheeInDeclaration relatedRecord;
		readonly MessagesToShowCollection.MessagesStatus messageStatus;

		#region Bound Properties

		#region RecordTypeDescription

		public ZString RecordTypeDescription
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.RecordTypeDescription; }
		}

		public ZPropertyInfo RecordTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RecordTypeDescription)); }
		}

		#endregion

		#region HumanFriendlyReference

		public ZString HumanFriendlyReference
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.HumanFriendlyReference; }
		}

		public ZPropertyInfo HumanFriendlyReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.HumanFriendlyReference); }
		}

		#endregion

		#region ShouldSendMessage

		public ZBool ShouldSendMessage
		{
			get { return fShouldSendMessage; }
			set
			{
				SetNonPersistentPropertyValue(ShouldSendMessageInfo, ref fShouldSendMessage, value);
			}
		}
		ZBool fShouldSendMessage;

		public ZPropertyInfo ShouldSendMessageInfo
		{
			get { return GetZPropertyInfo(Schema.ShouldSendMessage); }
		}

		#endregion

		#region Status

		#region Message Status

		public ZString Status
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.MessageStatus; }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		public ZString StatusDesc
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.MessageStatusDescription; }
		}

		public ZPropertyInfo StatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.StatusDesc); }
		}

		#endregion

		#region Entry Status

		public ZString EntryStatus
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.EntryStatus; }
		}

		public ZPropertyInfo EntryStatusInfo
		{
			get { return GetZPropertyInfo(Schema.EntryStatus); }
		}

		public ZString EntryStatusDesc
		{
			get { return EntryStatusList.GetDescriptionFromCode(EntryStatus); }
		}

		public ZPropertyInfo EntryStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.EntryStatusDesc); }
		}

		ImportEntryStatusList EntryStatusList
		{
			get { return Factory.GetCachedValue<ImportEntryStatusList>(); }
		}

		#endregion

		#endregion

		#region Release Date

		public ZDateTime ReleaseDate
		{
			get { return relatedRecord == null ? ZDateTime.Empty : relatedRecord.ReleaseDate; }
		}

		public ZPropertyInfo ReleaseDateInfo
		{
			get { return GetZPropertyInfo(Schema.ReleaseDate); }
		}

		#endregion

		#region TIB Details

		public ZDateTime TIBExpiryDate
		{
			get { return relatedRecord == null ? ZDateTime.Empty : relatedRecord.TIBExpiryDate; }
		}

		public ZPropertyInfo TIBExpiryDateInfo
		{
			get { return GetZPropertyInfo(Schema.TIBExpiryDate); }
		}

		public ZInt TIBNumOfExtensions
		{
			get { return relatedRecord == null ? ZInt.Zero : relatedRecord.TIBNumOfExtensions; }
		}

		public ZPropertyInfo TIBNumOfExtensionsInfo
		{
			get { return GetZPropertyInfo(Schema.TIBNumOfExtensions); }
		}

		#endregion

		#endregion

		#region Collections

		public MessagesToShowCollection MessagesToShow
		{
			get
			{
				if (messagesToShow == null)
				{
					var parentPKsOfMessages = relatedRecord != null ? relatedRecord.ParentPKsOfMessages : System.Array.Empty<ZGuid>();

					var sqlOperator = relatedRecord.RecordType == MessageAttacheeRecordType.BIRD ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
					var birdApplicationCodesList = new List<string>(ApplicationIdentifierCodeList.GetBIRDApplicationIdentifierCodes());
					birdApplicationCodesList.Add(ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction);
					var additionalFilter = new ZQuery(EDIMessageSchema.EM_MessageType, sqlOperator, birdApplicationCodesList);

					messagesToShow = new MessagesToShowCollection(Factory, parentPKsOfMessages, messageStatus, additionalFilter);
				}
				return messagesToShow;
			}
		}
		MessagesToShowCollection messagesToShow;

		#endregion

		public MessageAttacheeRecordType RecordType
		{
			get { return relatedRecord != null ? relatedRecord.RecordType : MessageAttacheeRecordType.Entry; }
		}
	}
}
