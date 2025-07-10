using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface ICusEntryHeaderMessageAttachee : ICusEntryHeader, IMessageAttacheeInDeclaration
	{
	}

	public interface ICargoReleaseCusEntryHeader : ICusEntryHeaderMessageAttachee
	{
		ZString EntryDateElectionCode { get; }
		ZDate PresentationDate { get; }
		ZBool IsConsigneeNameAddressUsed { get; }

		IAddressDetails ImporterWrapperAddressDetails { get; }

		new ZString UltimateConsigneeNumber { get; }

		new IEnumerable<ICargoReleaseCusEntryLine> EntryLines { get; }
	}

	//CusEntryHeader
	public interface IACECargoReleaseHeader : ICusEntryHeaderMessageAttachee
	{
		ZString DeclarationReferenceNumber { get; }
		ZBool IsExpressConsignment { get; }
		ZBool IsDomesticCargo { get; }
		ZBool KnownImporterIndicator { get; }
		ZBool ImmediateDelivery { get; }
		ZString RailReferenceNumber { get; }
		ZString ConsolidatedFilterCodeAndEntryNumber { get; }

		ZString ImporterOfRecordType { get; }
		ZString PortOfUnlading { get; }

		ZString ElectedExamSite { get; }
		ZString GeneralOrderNumber { get; }
		ZString EntryDateElectionCode { get; }
		ZDate ElectedEntryDate { get; }
		ZDate EstimatedDateOfArrivalForEntryType86 { get; }
		ZString CBPBondedWarehouseFIRMS { get; }
		ZString CurrentFirmsCodeForWarehousingEntry { get; }

		//SE12
		ZString ADDCVDBondType { get; }
		ZDecimal ADDCVDSingleTransactionBondAmount { get; }
		ZString ADDCVDSingleTransactionBondAccNo { get; }

		IEnumerable<ISimplifiedEntryOrganisationDetails> Entities { get; }
	}

	public interface IHeaderCommon
	{
		ZString EntryType { get; }//Parse to ZInt in MessageBuilder. EntryType.ToString() to compare with EntryTypeList.Codes loses the leading zero.
		ZString ImporterOfRecordNumber { get; }
		ZString ModeOfTransportationCode { get; }
		ZString BondType { get; }
		ZString DistrictPortOfEntry { get; }
		ZDecimal TotalValueOfEntrySummary { get; }
	}

	public interface ISEAdditionalData : IAcknowledgeAndSign
	{
		ZString ContactName { get; }
		ZString ContactPhone { get; }
		ZString ReasonCode { get; }
		ZString MultipleCargoDispositionsIndicator { get; }
		ZString ReferenceIdentifier { get; }
		ZString ReferenceIdentifierQualifier { get; }
		ZBool DISIndicator { get; }
		ZString DISIDRefNo { get; }
	}
}
