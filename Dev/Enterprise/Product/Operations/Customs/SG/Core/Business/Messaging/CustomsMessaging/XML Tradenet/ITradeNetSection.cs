using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	#region Base

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ITradeNetSection
	{
	}

	public interface ITradeNetSectionParent
	{
		string MessageVersion { get; set; }
		string SenderID { get; set; }
		string RecipientID { get; set; }
		int TotalNumbers { get; set; }
	}

	public interface ITradeNetPartyIdentification
	{
		string ID { get; set; }
	}

	public interface ITradeNetParty
	{
		ITradeNetPartyIdentification PartyIdentification { get; }

		string[] PartyName { get; }
	}

	#endregion

	#region Out Section

	[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ITradeNetOutSection : ITradeNetSection
	{
	}

	public interface ITradeNetOutPermitSection : ITradeNetOutSection
	{
		ITradeNetInSection Declaration { get; }
		Permit Permit { get; }
	}

	public interface ITradeNetOutUpdatePermitSection : ITradeNetOutPermitSection
	{
		Update Update { get; }
	}

	#endregion

	#region In Section

	public interface ITradeNetInSection : ITradeNetSection
	{
		Header Header { get; set; }

		Party Party { get; set; }

		Summary Summary { get; set; }

		Transport Transport { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		Item[] Item { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		SupportingDocumentReference[] SupportingDocumentReference { get; set; }
	}

	public interface ITradeNetInSectionWithCargo : ITradeNetInSection
	{
		Cargo Cargo { get; set; }
	}

	public interface ITradeNetInSectionWithCertificate : ITradeNetInSection
	{
		Certificate Certificate { get; set; }
	}

	public interface ITradeNetInSectionWithLicence : ITradeNetInSection
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		Licence[] Licence { get; set; }
	}

	public interface ITradeNetInSectionWithInvoice : ITradeNetInSection
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		Invoice[] Invoice { get; set; }
	}

	#endregion

	#region Header Section

	public interface ITradeNetHeaderSection
	{
		string MessageReference { get; set; }
		string DeclarantID { get; set; }
		string CommonAccessReference { get; set; }
		bool DeclarationIndicator { get; set; }
		bool DeclarationIndicatorSpecified { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		string[] AdditionalRecipientID { get; set; }
		UniqueReferenceNumber UniqueReferenceNumber { get; set; }
	}

	#endregion

	#region Approval Condition

	public interface IApprovalCondition
	{
		string AgencyCode { get; set; }
		string ConditionCode { get; set; }
		string ConditionDescription { get; set; }
	}

	#endregion

	#region Message

	public interface ITradeNetMessage : ICusMessage
	{
		void Build(ITradeNetSectionParent sectionParent);
		void SetMessageText(string text);
	}

	#endregion
}
