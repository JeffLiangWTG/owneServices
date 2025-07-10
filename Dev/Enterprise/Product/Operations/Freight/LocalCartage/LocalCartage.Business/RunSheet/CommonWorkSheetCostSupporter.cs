using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetCostSupporter : IGenericJobCostSupporter
	{
		readonly IEnumerable<ZString> DefaultChargeGroups = new List<ZString> { ChargeCodeGroupList.Codes.Transport };

		readonly CommonWorkSheet WorkSheet;

		public CommonWorkSheetCostSupporter(CommonWorkSheet workSheet)
		{
			WorkSheet = workSheet;
		}

		ZGuid IGenericJobCostSupporter.GetCreditorPK(ZString chargeGroup, ZGuid rateProviderOrgPK)
		{
			return DefaultChargeGroups.Contains(chargeGroup) && WorkSheet.TransportCo != null
				? WorkSheet.TransportCo.PK
				: ZGuid.Empty;
		}

		ZGuid IGenericJobCostSupporter.PK
		{
			get { return WorkSheet.PK; }
		}

		ZString IGenericJobCostSupporter.Type
		{
			get { return WorkSheet.TablePrefix; }
		}

		ZGuid[] IGenericJobCostSupporter.ShipmentsListPKs
		{
			get
			{
				return WorkSheet
					.CartageLegs
					.Select(leg => leg.Cartage.PK)
					.Distinct()
					.ToArray();
			}
		}

		IJobInvoicingPlugIn[] IGenericJobCostSupporter.ShipmentsList
		{
			get
			{
				return WorkSheet
					.CartageLegs
					.Select(leg => leg.Cartage)
					.Distinct()
					.Select(cartage => new CommonWorkSheetInvoicingPlugIn(WorkSheet, cartage))
					.ToArray();
			}
		}

		bool IGenericJobCostSupporter.HasChanges
		{
			get { return WorkSheet.HasChanges; }
		}

		bool IGenericJobCostSupporter.IsInDatabase
		{
			get { return WorkSheet.IsInDatabase; }
		}

		DocumentSupporter IGenericJobCostSupporter.DocumentSupporter
		{
			get { return null; }
		}

		ZString IGenericJobCostSupporter.MasterBillNum
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.TotalChargeableUnit
		{
			get { return ZString.Empty; }
		}

		Directions IGenericJobCostSupporter.Direction => Directions.Unknown;

		bool IGenericJobCostSupporter.IsBuyersConsol
		{
			get { return false; }
		}

		ZString IGenericJobCostSupporter.PortOfLoading
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.PortOfDischarge
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.ConsolMode
		{
			get { return ZString.Empty; }
		}

		OrgHeader IGenericJobCostSupporter.SendingForwarder
		{
			get { return null; }
		}

		OrgHeader IGenericJobCostSupporter.ReceivingForwarder
		{
			get { return null; }
		}

		ManyToManyBusinessObjectCollection IGenericJobCostSupporter.Shipments
		{
			get { return null; }
		}

		ZDateTime IGenericJobCostSupporter.ETD
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IGenericJobCostSupporter.ETA
		{
			get { return ZDateTime.Empty; }
		}

		IEnumerable<ZString> IGenericJobCostSupporter.ExcludedApportionmentMethods
		{
			get
			{
				yield return AllocationMethod.Revenue;
				yield return AllocationMethod.CapacityPerContainer;
				yield return AllocationMethod.FreeSpaceContribution;
			}
		}

		bool IGenericJobCostSupporter.IsApportionmentFilterEnabled
		{
			get { return false; }
		}

		ZDecimal IGenericJobCostSupporter.FreeSpace => ZDecimal.Zero;

		SecurityCheckpoint IGenericJobCostSupporter.JobConsolCostingCheckPoint => Env.Security.None;
	}
}
