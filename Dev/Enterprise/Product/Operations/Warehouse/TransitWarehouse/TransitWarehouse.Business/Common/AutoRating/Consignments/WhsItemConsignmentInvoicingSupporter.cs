using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemConsignmentInvoicingSupporter<T> : JobInvoicingSupporter
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitJobForConsolCosting,
		ITransitJobInvoicingPlugIn
	{
		public WhsItemConsignmentInvoicingSupporter(T consignment)
			: base(consignment)
		{
			this.consignment = consignment;
		}

		readonly T consignment;

		#region Override

		public override ZString HouseBillNumber => consignment.HouseBillNumber;

		public override RefUNLOCO Destination => consignment.Destination;

		public override JobInvoicingConsumerType ConsumerType => consignment.ConsumerType;

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments) => true;

		IEnumerable<WhsItemPackageState> PackageStates => packageStates ?? (packageStates = consignment.PackageStatesForConsolCosting);
		IEnumerable<WhsItemPackageState> packageStates;

		public override ZDecimal ActualWeight
		{
			get
			{
				if (!actualWeight.HasValue)
				{
					actualWeight = PackageStates.Sum(t => Constants.Weight.Convert(t.Package.KP_Weight, t.Package.KP_WeightUQ, ActualWeightUnit));
				}

				return actualWeight.Value;
			}
		}
		ZDecimal? actualWeight;

		public override ZString ActualWeightUnit
		{
			get
			{
				if (actualWeightUnit.IsEmpty)
				{
					if (PackageStates.Any())
					{
						actualWeightUnit = PackageStates.First().Package.KP_WeightUQ;
					}

					if (actualWeightUnit.IsEmpty)
					{
						actualWeightUnit = Constants.Weight.Kilograms;
					}
				}

				return actualWeightUnit;
			}
		}
		ZString actualWeightUnit;

		public override ZDecimal ActualVolume
		{
			get
			{
				if (!actualVolume.HasValue)
				{
					actualVolume = PackageStates.Sum(t => Constants.Volume.Convert(t.Package.KP_Volume, t.Package.KP_VolumeUQ, ActualVolumeUnit));
				}

				return actualVolume.Value;
			}
		}
		ZDecimal? actualVolume;

		public override ZString ActualVolumeUnit
		{
			get
			{
				if (actualVolumeUnit.IsEmpty)
				{
					if (PackageStates.Any())
					{
						actualVolumeUnit = PackageStates.First().Package.KP_VolumeUQ;
					}

					if (actualVolumeUnit.IsEmpty)
					{
						actualVolumeUnit = Constants.Volume.CubicMetres;
					}
				}
				return actualVolumeUnit;
			}
		}
		ZString actualVolumeUnit;

		public override int OuterPackTotal
		{
			get
			{
				if (!outerPackTotal.HasValue)
				{
					outerPackTotal = PackageStates.Sum(t => t.Package.KP_PackageQty);
				}

				return outerPackTotal.Value;
			}
		}
		int? outerPackTotal;

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore() => consignment.AuditSecurity;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => consignment.JobInvoicingSecurity;

		#endregion

		#endregion
	}
}
