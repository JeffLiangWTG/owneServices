using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentValidation_OLD : DtbTransportValidation
	{
		public DtbBookingConsignmentValidation_OLD(DtbBookingConsignment parent)
			: base(parent)
		{
		}

		#region CheckKM_TransportReference

		protected override void CheckKM_TransportReference()
		{
			base.CheckKM_TransportReference();

			var consignment = (DtbBookingConsignment)Parent;
			if (!consignment.KM_TransportReferenceInfo.HasErrors() && consignment.ConsolidationSingleJob != null)
			{
				var bookingParty = consignment.ConsolidationSingleJob.BookedByAddress;
				var bookingPartyAddress = bookingParty != null ? bookingParty.Address : null;
				if (bookingPartyAddress != null && !consignment.KM_TransportReference.IsEmpty)
				{
					var matchingConsignments = GetJobsWithMatchingConnnote(bookingPartyAddress, consignment.KM_TransportReference);
					if (matchingConsignments.Any())
					{
						var warning = GetDuplicateConnoteMessage(bookingParty.E2_CompanyName, consignment.KM_TransportReference, matchingConsignments.ToArray());
						consignment.KM_TransportReferenceInfo.AddWarning(warning);
					}
				}
			}
		}

		ZString GetDuplicateConnoteMessage(ZString companyName, ZString connote, params ZString[] matchingConsignments)
		{
			var matchingConnoteNumbers = new ZStringBuilder(matchingConsignments).ToStringWithNewLineBetweenAppends();
			return Res.GetString("8019053a-f1bb-49ca-b6ac-9a6e9ce6fbe4", @"{0} has already created consignments using Connote Number {1}. The consignment numbers are:
{2}", companyName, connote, matchingConnoteNumbers);
		}

		#endregion

		#region GetJobsWithMatchingConnnote

		IEnumerable<ZString> GetJobsWithMatchingConnnote(OrgAddress address, ZString connote)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddToFilter(DtbBookingSchema.KM_JobType, TransportConsolidationJobTypes.Codes.Consignment);
			consignmentQuery.AddToFilter(DtbBookingSchema.KM_TransportReference, connote);
			consignmentQuery.AddToFilter(DtbBookingSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			var addressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			addressQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, address.PK);
			var consolidationQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConsolidation), DtbBookingSchema.KM_KB_Booking);
			consolidationQuery.AddSubQuery(addressQuery, JoinCondition.And);
			consignmentQuery.AddSubQuery(consolidationQuery, JoinCondition.And);

			var matchingConsignments = Parent.Factory.Load<DtbBookingConsignment>(consignmentQuery);
			return matchingConsignments.Select(c => c.KM_JobID);
		}

		#endregion
	}
}
