using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[UniversalDataContext(DataContextType.TransportConsignmentConsolidation)]
	public class DtbConsignmentConsolidation : DtbTransportConsolidation, IDtbConsignmentConsolidation
	{
		public DtbConsignmentConsolidation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region SetDefaultValues

		protected override string DefaultJobType
		{
			get { return TransportConsolidationJobTypes.Codes.Consignment; }
		}

		#endregion

		#region Related Entities

		#region Bookings

		[ChildEditable]
		public new DtbBookingConsignmentCollection Bookings
		{
			get { return (DtbBookingConsignmentCollection)base.Bookings; }
		}

		protected override IDtbTransportCollection GetNewTransportsCollection()
		{
			return new DtbBookingConsignmentCollection(this);
		}

		#endregion

		#region Parent

		public AutoDtbBooking Parent
		{
			get { return (AutoDtbBooking)Factory.Load(KB_ParentTableCode, KB_ParentID); }
		}

		#endregion

		#endregion

		#region Properties

		#region KB_JobType

		public override ZString KB_JobType
		{
			get { return base.KB_JobType; }
			set
			{
				if (value == TransportConsolidationJobTypes.Codes.Booking
					|| value == TransportConsolidationJobTypes.Codes.HighVolumeLowValue
					|| value == TransportConsolidationJobTypes.Codes.QuotedBooking
					|| value == TransportConsolidationJobTypes.Codes.BookingTransportConsolidation)
				{
					throw new InvalidOperationException(string.Format(Culture.Invariant, "{0} is not a valid {1} in a Consignment Consolidation.", value, DtbBookingConsolidationSchema.Constants.KB_JobType));
				}

				base.KB_JobType = value;
			}
		}

		#endregion

		// calculated

		#region ParentJobDescription

		protected override ZString ParentJobDescriptionCore
		{
			get
			{
				var parent = Parent;
				return (parent != null)
					? Res.GetString("0d0ec46f-0634-4c0e-8bec-c727ce0b1f54", "Booking {0}", parent.KM_JobID)
					: "";
			}
		}

		#endregion

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KB_JobID.IsEmpty
					? Res.GetString("230cfe89-f7fb-4bcf-b4b4-c778d0abbf5b", "Transport Consignment Consolidation")
					: Res.GetString("022bd3b2-a381-4f58-8a7e-50996bb5a1a8", "Transport Consignment Consolidation {0}", KB_JobID);
			}
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbConsignmentConsolidationFetchStrategy(this);
		}

		#endregion

		#region Lookups

		public new DtbConsignmentConsolidationLookups Lookups
		{
			get { return (DtbConsignmentConsolidationLookups)base.Lookups; }
		}

		protected override DtbTransportConsolidationLookups GetNewLookupsCore()
		{
			return new DtbConsignmentConsolidationLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentConsolidationValidation Validation
		{
			get { return (DtbConsignmentConsolidationValidation)base.Validation; }
		}

		protected override DtbTransportConsolidationValidation GetNewValidationCore()
		{
			return new DtbConsignmentConsolidationValidation(this);
		}

		#endregion

		#region Unique Index Failure Handler

		protected override INumberFountainProxy GetNumberFountainForUniqueID()
		{
			return Env.NumberFountains.DtbConsignmentConsolidationID;
		}

		#endregion

		#region OnDocAddressChanged

		protected override void OnDocAddressChanged(JobDocAddress docAddress)
		{
			if (docAddress.DocAddressType == DocAddressType.BookingPartyDocumentaryAddress && docAddress.HasRealAddress)
			{
				foreach (var booking in Bookings)
				{
					booking.Validation.ValidateKM_TransportReference();
				}
			}
		}

		#endregion
	}
}
