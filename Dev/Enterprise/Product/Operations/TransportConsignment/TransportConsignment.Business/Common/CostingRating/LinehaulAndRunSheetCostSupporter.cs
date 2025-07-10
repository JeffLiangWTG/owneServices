using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	abstract class LinehaulAndRunSheetCostSupporter<T> : IGenericJobCostSupporter where T : BusinessObject
	{
		protected LinehaulAndRunSheetCostSupporter(T parent)
		{
			this.Parent = parent;
		}

		public T Parent { get; private set; }

		ZString IGenericJobCostSupporter.ConsolMode
		{
			get { return ZString.Empty; }
		}

		DocumentSupporter IGenericJobCostSupporter.DocumentSupporter
		{
			get
			{
				var docSupportable = Parent as IDocumentSupportable;
				return docSupportable != null ? docSupportable.DocumentSupporter : null;
			}
		}

		ZDateTime IGenericJobCostSupporter.ETA
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IGenericJobCostSupporter.ETD
		{
			get { return ZDateTime.Empty; }
		}

		bool IGenericJobCostSupporter.HasChanges
		{
			get { return Parent.HasChanges; }
		}

		bool IGenericJobCostSupporter.IsBuyersConsol
		{
			get { return false; }
		}

		Directions IGenericJobCostSupporter.Direction => Directions.Unknown;

		bool IGenericJobCostSupporter.IsInDatabase
		{
			get { return Parent.IsInDatabase; }
		}

		ZString IGenericJobCostSupporter.MasterBillNum
		{
			get { return ZString.Empty; }
		}

		IEnumerable<ZString> DefaultChargeGroups()
		{
			return new ZString[] { ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Codes.TransportBooking };
		}

		ZGuid IGenericJobCostSupporter.GetCreditorPK(ZString chargeCode, ZGuid rateProviderOrgPK)
		{
			return DefaultChargeGroups().Contains(chargeCode) ? CreditorPK : ZGuid.Empty;
		}

		protected abstract ZGuid CreditorPK { get; }

		ZGuid IGenericJobCostSupporter.PK
		{
			get { return Parent.PK; }
		}

		ZString IGenericJobCostSupporter.PortOfDischarge
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.PortOfLoading
		{
			get { return ZString.Empty; }
		}

		public virtual OrgHeader ReceivingForwarder
		{
			get { return null; }
		}

		public virtual OrgHeader SendingForwarder
		{
			get { return null; }
		}

		ManyToManyBusinessObjectCollection IGenericJobCostSupporter.Shipments
		{
			get { return null; }
		}

		protected abstract IEnumerable<IJobInvoicingPlugIn> Consignments { get; }

		IJobInvoicingPlugIn[] IGenericJobCostSupporter.ShipmentsList
		{
			get { return shipmentList ?? (shipmentList = Consignments.Distinct().ToArray()); }
		}

		IJobInvoicingPlugIn[] shipmentList;

		ZGuid[] IGenericJobCostSupporter.ShipmentsListPKs
		{
			get { return ((IGenericJobCostSupporter)this).ShipmentsList.Select(b => b.PK).ToArray(); }
		}

		ZString IGenericJobCostSupporter.TotalChargeableUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		ZString IGenericJobCostSupporter.TransportMode
		{
			get { return ZString.Empty; }
		}

		ZString IGenericJobCostSupporter.Type
		{
			get { return Parent.TablePrefix; }
		}

		IEnumerable<ZString> IGenericJobCostSupporter.ExcludedApportionmentMethods
		{
			get { yield return AllocationMethod.FreeSpaceContribution; }
		}

		bool IGenericJobCostSupporter.IsApportionmentFilterEnabled
		{
			get { return true; }
		}

		ZDecimal IGenericJobCostSupporter.FreeSpace => ZDecimal.Zero;

		SecurityCheckpoint IGenericJobCostSupporter.JobConsolCostingCheckPoint => Env.Security.None;
	}
}
