using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class ForwardingDocsAndCartage : JobDocsAndCartage, Integration.Forwarding.IForwardingDocsAndCartage, IWorkflowTriggerFieldChangeSource, MasterFiles.Business.DIS.IDISHostProvider
	{
		public ForwardingDocsAndCartage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation

		protected override JobDocsAndCartageValidation GetNewValidation()
		{
			JobDocsAndCartageValidation result = new ForwardingDocsAndCartageValidation(this);
			if (Parent != null)
			{
				JobDocsAndCartageValidation piggyBackValidation = Parent.PiggyBackedValidation;
				if (piggyBackValidation != null)
				{
					result.Add(piggyBackValidation);
				}
			}
			return result;
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			if (RequireCalculateShipmentDeliveryDueDate())
			{
				ShipmentInParent.CalculateDeliveryDueDateIfNecessary();
			}
		}

		bool RequireCalculateShipmentDeliveryDueDate()
		{
			if (ShipmentInParent == null)
			{
				return false;
			}

			if (JP_PickupRequiredByInfo.HasChanges &&
				((ShipmentInParent.IsHBLContainerPackModeDOOR_X && JP_PickupCartageCompleted.IsEmpty)
				|| (ShipmentInParent.IsHBLContainerPackModeCFS_X && ShipmentInParent.JS_A_RCV.IsEmpty)))
			{
				return true;
			}

			return JP_PickupCartageCompletedInfo.HasChanges && ShipmentInParent.IsHBLContainerPackModeDOOR_X;
		}

		#region Properties

		#region Delivery Cartage Completed

		public override ZDateTime JP_DeliveryCartageCompleted
		{
			get { return base.JP_DeliveryCartageCompleted; }
			set
			{
				base.JP_DeliveryCartageCompleted = value;
				if (Parent is IAttachOrders)
				{
					var orderMightExist = Parent.IsInDatabase;
					if (!orderMightExist)
					{
						var query = new ZQuery { FetchOnlyFromLocalCache = true };
						orderMightExist = Factory.LoadTop1<Order>(query) != null;
					}
					if (orderMightExist)
					{
						foreach (Order order in ((IAttachOrders)Parent).AttachedOrders)
						{
							order.AttemptToUpdateOrderStatus();
						}
					}
				}
			}
		}
		[LightValidationTestExempt]
		public override ZGuid JP_ParentID { get => base.JP_ParentID; set => base.JP_ParentID = value; }
		[LightValidationTestExempt]
		public override ZString JP_ParentTableCode { get => base.JP_ParentTableCode; set => base.JP_ParentTableCode = value; }
		#endregion

		#region Has Orders

		public bool HasOrders
		{
			get
			{
				var attachOrders = Parent as IAttachOrders;
				var attachJobs = Parent as IAttachGenericOrders;
				return (attachJobs != null && attachJobs.GenericOrders.Count > 0)
					|| (attachOrders != null && attachOrders.AttachedOrders.Count > 0);
			}
		}

		#endregion

		#region AWB Print Options

		public override ZString JP_PrintOptionForPackagesOnAWB
		{
			get { return base.JP_PrintOptionForPackagesOnAWB; }
			set
			{
				if (base.JP_PrintOptionForPackagesOnAWB != value)
				{
					base.JP_PrintOptionForPackagesOnAWB = value;
					if (ShipmentInParent != null && ShipmentInParent.IsAir)
					{
						ShipmentInParent.PopulateAWB();
					}
				}
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		public new ForwardingShipment ShipmentInParent
		{
			get { return Parent != null ? Parent.Shipment as ForwardingShipment : null; }
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				return Parent == null ? System.Array.Empty<IWorkflowProvider>() : new IWorkflowProvider[] { Parent as IWorkflowProvider };
			}
		}

		#endregion

		#region Read Only Security

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			if (ShipmentInParent != null)
			{
				result = ShipmentInParent.IsChildPropertyReadOnlyDueToPhase(PK, property.Name);
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected virtual void RegisterEditableChildObject(IBusiness child, ZString childName)
		{
			RegisterEditableChildObject(child);

			bool readOnlyDueToPhase = ShipmentInParent?.IsChildPropertyReadOnlyDueToPhase(PK, childName) ?? false;
			if (readOnlyDueToPhase)
			{
				child.IncrementReadOnlyIncludingChildren();
			}
		}

		#endregion

		#region Phase Dependant Mandatory Validation

		internal void InitialisePhaseDependantMandatoryValidation()
		{
			if (ShipmentInParent != null && PhaseResolver != null && mandatoryValidationPhase != ShipmentInParent.JS_Phase)
			{
				mandatoryValidationPhase = ShipmentInParent.JS_Phase;
				var distinctOnes = PhaseResolver.InitialisePhaseDependantMandatoryValidation(ZPropertyInfoHash.Cast<ZPropertyInfo>(), PK)
					.OfType<ZWrappedPropertyInfo>()
					.Select(p => p.InnerInfo.BizObj)
					.DistinctBy(bo => bo.PK);
#if NETFRAMEWORK
				distinctOnes.ForEach(bo => bo.MarkAsNeedingValidation());
#else
				foreach (var bo in distinctOnes)
				{
					bo.MarkAsNeedingValidation();
				}
#endif
			}
		}
		ZString mandatoryValidationPhase = ZString.Empty;

		internal IPhaseSecurityResolver PhaseResolver
		{
			get
			{
				if (phaseResolver == null && ShipmentInParent != null)
				{
					phaseResolver = ShipmentInParent.PhaseResolver;
				}

				return phaseResolver;
			}
		}
		IPhaseSecurityResolver phaseResolver;

#endregion

		MasterFiles.Business.DIS.IDISHost MasterFiles.Business.DIS.IDISHostProvider.DISHost
		{
			get
			{
				var parentAsHostProvider = Parent as MasterFiles.Business.DIS.IDISHostProvider;
				return parentAsHostProvider != null ? parentAsHostProvider.DISHost : null;
			}
		}
	}
}
