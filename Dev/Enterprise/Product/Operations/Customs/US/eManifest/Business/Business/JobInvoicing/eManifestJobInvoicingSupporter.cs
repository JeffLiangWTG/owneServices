using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Customs.US.eManifest.Business
{
	class eManifestJobInvoicingSupporter : JobInvoicingSupporter
	{
		internal eManifestJobInvoicingSupporter(Trip trip)
			: base(trip)
		{
			this.trip = trip;
		}

		public override OrgHeader Consignee
		{
			get
			{
				OrgHeader result;
				if (trip.Importer != null)
				{
					result = trip.Importer.Header;
				}
				else
				{
					var consignees = trip.Shipments.Select(s => s.Consignee).ToArray();
					var first = consignees.FirstOrDefault();
					result = first != null
							 && first.HasRealOrganisation
							 && consignees.All(c => c.OrganisationPK == first.OrganisationPK)
								? first.Organisation : null;
				}
				return result;
			}
		}

		public override ZDateTime ETA
		{
			get { return trip.BH_ETA; }
		}

		public override ZDecimal ActualChargeable
		{
			get { return ActualWeight; }
		}

		public override ZString ActualChargeableUnit
		{
			get { return ActualWeightUnit; }
		}

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		public override bool IsImport
		{
			get { return true; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.eManifest; }
		}

		public override ZDecimal ActualWeight
		{
			get
			{
				var unit = ActualWeightUnit;
				return trip.Shipments.Select(s => Constants.Weight.Convert(s.B0_Weight, s.B0_WeightUQ, unit)).Sum();
			}
		}

		public override ZString ActualWeightUnit
		{
			get
			{
				var units = trip.Shipments.Select(s => s.B0_WeightUQ).ToArray();
				var first = units.FirstOrDefault();
				return units.All(u => u == first) ? first : (ZString)Constants.Weight.Kilograms;
			}
		}

		public override ZDecimal ActualVolume
		{
			get
			{
				var unit = ActualVolumeUnit;
				return trip.Shipments.Select(s => Constants.Volume.Convert(s.B0_Volume, s.B0_VolumeUQ, unit)).Sum();
			}
		}

		public override ZString ActualVolumeUnit
		{
			get
			{
				var units = trip.Shipments.Select(s => s.B0_VolumeUQ).ToArray();
				var first = units.FirstOrDefault();
				return units.All(u => u == first) ? first : (ZString)Constants.Volume.CubicMetres;
			}
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !trip.IsInDatabaseIncludingChildren; }
		}

		public override GlbBranch OperationsBranch
		{
			get { return trip.Branch; }
		}

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.USeManifestAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.USeManifestJobInvoicing;
		}

		readonly Trip trip;
	}
}
