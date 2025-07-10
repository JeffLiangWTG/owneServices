using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInvoicingSupporter : JobInvoicingSupporter
	{
		protected DtbTransportInvoicingSupporter(DtbTransport transport)
			: base(transport)
		{
			Transport = transport;
		}

		protected readonly DtbTransport Transport;

		#region Packages

		public override ZString ContainerMode
		{
			get { return Transport.IsContainerisedOnly ? Constants.ContainerModes.FCL : Constants.ContainerModes.LCL; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Transport.IsLooseOnly ? Transport.GetTotalLoosePackageVolume().Amount : Transport.GetTotalContainerisedVolume().Amount; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Transport.IsLooseOnly ? Transport.GetTotalLoosePackageVolume().Unit : Transport.GetTotalContainerisedVolume().Unit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Transport.IsLooseOnly ? Transport.GetTotalLoosePackageWeight().Amount : Transport.GetTotalContainerisedWeight().Amount; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Transport.IsLooseOnly ? Transport.GetTotalLoosePackageWeight().Unit : Transport.GetTotalContainerisedWeight().Unit; }
		}

		public override int ContainerCount
		{
			get { return Transport.Containers.Sum(c => c.Package.KP_PackageQty); }
		}

		public override ZDecimal TEUCount
		{
			get { return Transport.Containers.Sum(c => c.Package.Container.ContainerType.RC_TEU); }
		}

		public override int OuterPackTotal
		{
			get { return Transport.LoosePackages.Sum(p => p.Package.KP_PackageQty); }
		}

		#endregion

		#region Other

		#region ConsolType

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		#endregion

		#region CreateAccountingJobOnSavingOfOperationsJob

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Transport.IsInDatabaseIncludingChildren; }
		}

		#endregion

		#region OperationsBranch

		public override GlbBranch OperationsBranch
		{
			get
			{
				GlbBranch result = null;

				if (UseRegistryFallbackForBranch)
				{
					var orderRulesForDefaulting = AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.Value;
					if (orderRulesForDefaulting.DefaultToBranchRelatedToPortOrWarehouseBranch > 0 && Transport.Instructions.Count > 0)
					{
						var instructions = Transport.Instructions.Cast<DtbTransportInstruction>();
						var pickupInstruction = instructions.FirstOrDefault(i => i.IsPickUp);
						if (pickupInstruction != null)
						{
							result = pickupInstruction.DepotForAddress;
						}
					}
				}
				else
				{
					result = GlbBranch.CurrentBranch;
				}

				return result;
			}
		}

		protected abstract bool UseRegistryFallbackForBranch { get; }

		#endregion

		#region TransportMode

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		#endregion

		#region Consignor

		public override OrgHeader Consignor
		{
			get { return null; } // bill to party?
		}

		#endregion

		#region OverriddenDepartmentPK

		public override ZGuid OverriddenDepartmentPK
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.TransportBookingJobsDefaultDept.Value;
			}
		}

		#endregion

		#region	OverriddenDefaultLocalClient

		public override OrgHeader OverriddenDefaultLocalClient
		{
			get
			{
				var bookingOrSendingPartyOrgPK = Transport.BookedByOrganisationPK;
				return !bookingOrSendingPartyOrgPK.IsEmpty ? Transport.Factory.Load<OrgHeader>(bookingOrSendingPartyOrgPK) : null;
			}
		}

		#endregion

		#endregion
	}
}
