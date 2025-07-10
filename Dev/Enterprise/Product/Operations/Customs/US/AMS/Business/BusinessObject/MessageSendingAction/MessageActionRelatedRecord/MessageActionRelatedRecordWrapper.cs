using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageActionRelatedRecordWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MessageActionRelatedRecordWrapper(IMessageAttacheeInHeader relatedRecord)
			: base(relatedRecord.Factory)
		{
			this.relatedRecord = relatedRecord;
		}

		public static class Schema
		{
			public const string RecordIdentifier = "RecordIdentifier";
			public const string RecordTypeDescription = "RecordTypeDescription";
		}

		public readonly IMessageAttacheeInHeader relatedRecord;

		#region Bound Properties

		#region RecordIdentifier

		public ZString RecordIdentifier
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.RecordIdentifier; }
		}

		public ZPropertyInfo RecordIdentifierInfo
		{
			get { return GetZPropertyInfo(Schema.RecordIdentifier); }
		}

		#endregion

		#region RecordTypeDescription

		public ZString RecordTypeDescription
		{
			get { return relatedRecord == null ? ZString.Empty : relatedRecord.RecordTypeDescription; }
		}

		public ZPropertyInfo RecordTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.RecordTypeDescription); }
		}

		#endregion

		#endregion

		public AMSEDIMessageCollection Messages
		{
			get { return fMessages ?? (fMessages = (AMSEDIMessageCollection)relatedRecord.Messages); }
		}
		AMSEDIMessageCollection fMessages;

		public MessageAttacheeRecordType RecordType
		{
			get { return relatedRecord != null ? relatedRecord.RecordType : MessageAttacheeRecordType.VesselMovement; }
		}
	}
}
