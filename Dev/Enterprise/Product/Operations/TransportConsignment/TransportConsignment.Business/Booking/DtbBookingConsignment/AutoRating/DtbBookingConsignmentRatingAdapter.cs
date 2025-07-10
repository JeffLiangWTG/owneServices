using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	class DtbBookingConsignmentRatingAdapter : DtbTransportRatingAdapter<DtbBookingConsignment>
	{
		/// <summary>
		/// Consignments will only rate loose for now
		/// </summary>
		public DtbBookingConsignmentRatingAdapter(DtbBookingConsignment consignment, FreightMode freightMode)
			: base(consignment, freightMode)
		{
		}

		#region Related Objects

		DtbBookingConsignment Consignment
		{
			get { return Parent; }
		}

		protected override DtbTransportInstruction FromInstruction
		{
			get { return Consignment.Instructions.FirstOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.PickUp); }
		}

		protected override DtbTransportInstruction ToInstruction
		{
			get { return Consignment.Instructions.FirstOrDefault(i => i.KN_InstructionType == InstructionTypes.Codes.Delivery); }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();
				result.AddRange(GetServiceInfosFromJobServices(Consignment.Services, ChargeCodeGroupList.Codes.TransportBooking));

				return result;
			}
		}

		#endregion

		#region IAutoRating Members

		#region IAutoRating

		public override AdapterType AdapterType => AdapterType.TransportConsignment;

		protected override IJobDatesProvider GetJobDatesProviderCore()
		{
			return new DtbBookingConsignmentJobDatesProvider(Consignment, FromInstruction, ToInstruction);
		}

		protected override bool HasConsolCosts
		{
			get
			{
				var result = false;

				var runSheets = Consignment.AllConfirmations.Where(c => c.RunSheetInstruction != null).Select(c => c.RunSheetInstruction).Select(r => r.RunSheet);
				if (runSheets.Any())
				{
					var costQuery = new ZQuery(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
					costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, runSheets.Select(r => r.PK));
					costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, runSheets.First().TablePrefix);
					result = Consignment.Factory.LoadTop1<IJobConsolCost>(costQuery) != null;
				}

				return result;
			}
		}

		protected override JobInvoicingConsumerType ConsumerTypeCore
		{
			get { return JobInvoicingConsumerTypes.TransportBookingConsignment; }
		}

		#endregion

		#region IAutoRatingOrganisations

		public override OrgHeader Carrier
		{
			get { return GlbCompany.CurrentCompany.OrgProxy; }
		}

		public override Creditors Creditors
		{
			get { return Creditors.New(GetTransportProviders()); }
		}

		IEnumerable<OrgWithSource> GetTransportProviders()
		{
			foreach (var confirmation in Consignment.AllConfirmations)
			{
				var runSheetInstruction = confirmation.RunSheetInstruction;
				if (runSheetInstruction != null)
				{
					var transportCompany = OrgWithSource.NewFrom<OrgHeader>(runSheetInstruction.RunSheet.KG_OH_TransportCoInfo);
					if (transportCompany != null)
					{
						yield return transportCompany;
					}
				}
			}
		}

		#endregion

		#region IAutoRatingFreightInfo

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(Parent.KM_RS_NKServiceLevel, ServiceLevelType.Client), // carrier service level?
					new ServiceLevelInfo(Parent.KM_RS_NKServiceLevel, ServiceLevelType.Carrier));
			}
		}

		public override System.Tuple<ZDecimal, ZString> GetAutoRatingDistanceMeasure()
		{
			return (Consignment != null) ? System.Tuple.Create(Consignment.KM_Distance, Consignment.KM_DistanceUnit) : null;
		}

		#endregion

		#endregion

		#region RateLineConditionsSupporter

		protected override TransportRateLineConditionsSupporter GetConditionsSupporterCore()
		{
			return new DtbBookingConsignmentRateLineConditionsSupporter(Consignment);
		}

		#endregion
	}
}
