using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public static class MessageAttacheeRecordTypeDescriptions
	{
		public const string Bill = "Bill";
		public const string Container = "Container";
		public const string Entry = "Entry";
		public const string InBond = "InBond";
		public const string ITNumber = "ITNumber";
		public const string ElectronicInvoice = "Elec. Invoice";
		public const string Declaration = "Declaration";
		public const string CargoRelease = "Cargo Release";
		public const string Liquidation = "Liquidation";
		public const string SimplifiedEntry = "ACE Cargo Release";
		public const string BIRD = "BIRD";
		public const string TemporaryImportationBond = "Temporary Importation Bond";
	}

	public enum MessageAttacheeRecordType { MasterBillOfLading, HouseBillOfLading, Container, Entry, InBondEntry, ElectronicInvoice, JobDeclaration, CargoRelease, BorderCargoRelease, ITNumber, Liquidation, SimplifiedEntry, BIRD }

	public interface IMessageAttacheeWithCBPSenderReference : IMessageAttachee
	{
		ZString EntryFilerCode { get; }
		ZString ProcessingDistrictPort { get; }
		ZString ProcessingOfficeCode { get; }
		ZString TransportMode { get; }
		System.Guid CompanyPK { get; }
	}

	/// <summary>
	/// CusEntryHeader or HouseBill or Container
	/// Inherited by
	/// 1/ IInBondQPHeader - CusEntryHeader
	/// 2/ IInBondWPHeader - CusEntryHeader, HouseBill, Container
	/// </summary>
	public interface IMessageAttacheeInDeclaration : IMessageAttacheeWithCBPSenderReference, IMessageResponseNotificator
	{
		bool IsActive { get; }

		MessageAttacheeRecordType RecordType { get; }

		ZString MessageStatusDescription { get; }
		ZString RecordTypeDescription { get; }
		ZString EntryStatus { get; }
		ZString HumanFriendlyReference { get; }
		ZString JobReferenceNumber { get; }
		ZGuid DeclarationPK { get; }
		ValidationModes ValidationModes { get; set; }
		ZString EntryNumber { get; }
		ZDateTime ReleaseDate { get; }
		ZDateTime TIBExpiryDate { get; }
		ZInt TIBNumOfExtensions { get; }

		/// <summary>
		/// CusEntryHeader is not directly exposed on the form and only wrappers for entries are exposed.
		/// Notifications from dbo.CusEntryHeader should be added to those wrappers.
		/// </summary>
		/// <returns>EntryHeader returns Notifications while others return null</returns>
		IEnumerable<INotification> GetBusinessLayerNotificationsToAddToWrapper();

		/// <summary>
		/// Declaration may have more than 1 Liquidation.
		/// This collection will be used for exposing all Liquidations messages in one row in Messages Grid
		/// </summary>
		IReadOnlyList<ZGuid> ParentPKsOfMessages { get; }
	}
}
