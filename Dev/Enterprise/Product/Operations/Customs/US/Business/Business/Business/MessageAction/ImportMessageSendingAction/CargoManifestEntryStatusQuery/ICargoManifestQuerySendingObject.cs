using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public enum CargoManifestQueryActionType { Invalid, BillOfLading, MAWB, HAWB, Entry, InBond, HouseBill, SubHouseBill }

	public interface ICargoManifestQuerySendingObject
	{
		void LinkToMessage(EDIMessage message);
		BusinessObjectFactory Factory { get; }

		ZBool UpdateEntryWithResults { get; }
		ZBool RequestForRelatedBOL { get; }
		ZString LimitOutputOption { get; }

		ZString ActionCode { get; }

		ZString EntryOrInBondNumber { get; }
		ZString BillIssuerCode { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
	}

	/// <summary>
	/// Declaration, CusInBondHeader
	/// </summary>
	public interface ICargoManifestStatusQueryHeader : IControllerIDProvider
	{
		ZString EntryFilerCode { get; }
		BusinessObjectFactory Factory { get; }

		CodeDescriptionPairList ActionList { get; }

		IEnumerable<ICargoManifestStatusQueryData> ObjectsForQuery { get; }

		ZString TransportMode { get; }

		ZString ProcessingOfficeCode { get; }
		ZString ProcessingPortCode { get; }

		ZBool IsACEQuery { get; }
	}

	/// <summary>
	/// CusEntryHeader, Bill, IT NUmber, In-Bond Movement, In-Bond Bill
	/// </summary>
	public interface ICargoManifestStatusQueryData : IControllerIDProvider
	{
		ZString EntryOrInBondNumber { get; }

		ZString BillIssuerCode { get; }

		ZString MasterAirWayBillNumber { get; }
		ZString HouseAirWayBillNumber { get; }
		ZString BillNumber { get; }
		bool HasPGAData { get; }

		//BusinessObjectFactory Factory { get; }

		void LinkToMessage(EDIMessage message);

		/// <summary>
		/// Master Bill Number or House Bill Number
		/// </summary>
		ZString HumanFriendlyReference { get; }
		ZGuid MessageAttacheePK { get; }
		string TableCode { get; }
		bool IsInDatabase { get; }

		/// <summary>
		/// Shipment or Declaration or Consol Number
		/// </summary>
		ZString JobReferenceNumber { get; }

		CargoManifestQueryActionType QueryActionType { get; }
		ZBool IsRelevantFor(ZString actionCode);
	}
}
