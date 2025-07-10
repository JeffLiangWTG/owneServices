
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitWarehousePackagePlanner : NonPersistentBusinessObject
	{
		public TransitWarehousePackagePlanner(BusinessObjectFactory factory, ITransitWarehouseParent parent) : base(factory)
		{
			Parent = parent;
		}

		public ITransitWarehouseParent Parent { get; }

		public WhsWarehouse Warehouse
		{
			get
			{
				return TransitWarehouseHelper.GetTransitWarehouse(Factory, Parent.GetPickupCFSOrgAddressPK());
			}
		}

		public bool HasMultipleTransitWarehousesForAddress
		{
			get
			{
				return TransitWarehouseHelper.GetAllTransitWarehousesForAddressPK(Factory, Parent.GetPickupCFSOrgAddressPK()).Count() > 1;
			}
		}

		public bool HasSentReceiveInstructions
		{
			get
			{
				return Factory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ParentID, Parent.PK)) != null;
			}
		}

		public bool HasSentDispatchInstructions
		{
			get
			{
				return Factory.LoadTop1<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, Parent.PK)) != null;
			}
		}

		public WhsItemPackageStateCollection ModuleFilterPackageCollection
		{
			get
			{
				if (moduleFilterPackageCollection == null)
				{
					moduleFilterPackageCollection = new WhsItemPackageStateCollection(this);
				}

				return moduleFilterPackageCollection;
			}
		}

		WhsItemPackageStateCollection moduleFilterPackageCollection;

		public bool IsSkipWhsItemPackageStateCollection { get; set; }

		public AdhocReceivedPackagesCollection RemovedPackages
		{
			get
			{
				if (removedPackages == null)
				{
					removedPackages = new AdhocReceivedPackagesCollection(this);
				}

				return removedPackages;
			}
		}
		AdhocReceivedPackagesCollection removedPackages;

		public WhsItemPackageStateCollection AttachedPackages
		{
			get
			{
				if (attachedPackLines == null)
				{
					attachedPackLines = new WhsItemPackageStateCollection(this, Parent.JobNumber);

					attachedPackLines.CountChanged += AttachedPackLines_CountChanged;
				}

				return attachedPackLines;
			}
		}

		void AttachedPackLines_CountChanged(object sender, EventArgs e)
		{
			RefreshBinding();
		}

		WhsItemPackageStateCollection attachedPackLines;

		public void AssignPackageState(IEnumerable<WhsItemPackageState> packageStates, bool assignAllPackage = false)
		{
			foreach (var packageState in packageStates)
			{
				if (packageState.WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit)
				{
					AssignChildPackagesForHandlingUnit(packageState);
				}
				else if (packageState.HandlingUnit != null)
				{
					if(assignAllPackage)
					{
						AssignChildPackagesForHandlingUnit(packageState.HandlingUnit);
					}
					else if (!AttachedPackages.Contains(packageState))
					{
						AttachedPackages.Add(packageState);
						packageState.AttachedPackageStateStatus = AttachedPackageStateStatus.Added;
					}
				}
				else if (!AttachedPackages.Contains(packageState))
				{
					AttachedPackages.Add(packageState);
					packageState.AttachedPackageStateStatus = AttachedPackageStateStatus.Added;
				}
			}

			OnPackageStateCollectionChanged?.Invoke();
		}

		public void PutBackPackageState(IEnumerable<WhsItemPackageState> packageStates)
		{
			foreach (var packageState in packageStates)
			{
				RemovedPackages.RemoveFromRelationship(packageState);
			}
		}

		void AssignChildPackagesForHandlingUnit(WhsItemPackageState handlingUnitPackageState)
		{
			var childPackages = handlingUnitPackageState.HandlingUnitChildPackagesCache ?? handlingUnitPackageState.GetHandlingUnitChildPackages();
			var unattachedChildPackages = childPackages.Where(p => !AttachedPackages.Contains(p) && p.WPS_WDC_TransitDispatchConsignment.IsEmpty);
			if (unattachedChildPackages.Any())
			{
				unattachedChildPackages.ForEach(p => p.AttachedPackageStateStatus = AttachedPackageStateStatus.Added);
				AttachedPackages.AddRange(unattachedChildPackages);
			}
		}

		public void RemovePackageStates(IEnumerable<WhsItemPackageState> packageStates)
		{
			foreach (var package in packageStates)
			{
				package.AttachedPackageStateStatus = AttachedPackageStateStatus.Removed;

				if (AttachedPackages.Contains(package))
				{
					AttachedPackages.RemoveFromRelationship(package);
					RemovedPackages.Add(package);
				}
			}

			OnPackageStateCollectionChanged?.Invoke();
		}

		public Action OnPackageStateCollectionChanged;

		#region Package Totals

		public ZInt TotalPackages => AttachedPackages.Sum(ps => ps.Package.KP_PackageQty);

		public ZPropertyInfo TotalPackagesInfo => GetZPropertyInfo(nameof(TotalPackages));

		public ZString TotalPackagesLabel => Invariant($"{TotalPackages},");  // To show totals on attach packages form

		public ZPropertyInfo TotalPackagesLabelInfo => GetZPropertyInfo(nameof(TotalPackagesLabel));

		public ZDecimal TotalWeight
		{
			get
			{
				var targetUQ = TotalWeightUQ;
				var packages = AttachedPackages.Where(p => p.Package.KP_Weight > 0 && !p.Package.KP_WeightUQ.IsEmpty).Select(p => p.Package).ToArray();
				return TransitWarehouseHelper.CalculateTotalWeight(packages, targetUQ);
			}
		}

		public ZPropertyInfo TotalWeightInfo => GetZPropertyInfo(nameof(TotalWeight));

		public ZDecimal TotalVolume
		{
			get
			{
				var targetUQ = TotalVolumeUQ;
				var packages = AttachedPackages.Where(p => p.Package.KP_Volume > 0 && !p.Package.KP_VolumeUQ.IsEmpty).Select(p => p.Package).ToArray();
				return TransitWarehouseHelper.CalculateTotalVolume(targetUQ, packages);
			}
		}

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(nameof(TotalVolume));

		public ZString TotalVolumeUQ
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		public ZPropertyInfo TotalVolumeUQInfo => GetZPropertyInfo(nameof(TotalVolumeUQ));

		public ZString TotalWeightUQ
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}

		public ZPropertyInfo TotalWeightUQInfo => GetZPropertyInfo(nameof(TotalWeightUQ));

		public ZString TotalWeightUQLabel => Invariant($"{string.Format(CultureInfo.InvariantCulture, "{0:n0}", TotalWeight)} {TotalWeightUQ},"); // To show totals on attach packages form

		public ZPropertyInfo TotalWeightUQLabelInfo => GetZPropertyInfo(nameof(TotalWeightUQLabel));

		public ZString TotaVolumeUQLabel => Invariant($"{string.Format(CultureInfo.InvariantCulture, "{0:n0}", TotalVolume)} {TotalVolumeUQ}");  // To show totals on attach packages form

		public ZPropertyInfo TotalVolumeUQLabelInfo => GetZPropertyInfo(nameof(TotaVolumeUQLabel));

		#endregion

		#region RemovePacklineFromPreviousParent

		public void RemovePacklineFromPreviousParent()
		{
			var attachingRCNs = AttachedPackages.Where(a => string.IsNullOrEmpty(a.ParentJobNumber)).Select(p => p.ReceiveConsignment).Distinct();
			var shipmentCodes = attachingRCNs.Where(r => !string.IsNullOrEmpty(r.ShipmentNumber)).Select(r => r.ShipmentNumber).Distinct();
			if (shipmentCodes.Any())
			{
				var forwardingShipments = Factory.Load<IForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentCodes));
				foreach (var forwardingShipment in forwardingShipments)
				{
					var rcn = attachingRCNs.FirstOrDefault(r => r.ShipmentNumber == forwardingShipment.JS_UniqueConsignRef);
					var packageInThisShipment = AttachedPackages.Where(p => p.WPS_WRC_TransitReceiveConsignment == (rcn?.PK ?? ZGuid.Empty));
					if (packageInThisShipment.Any())
					{
						(forwardingShipment as ITransitWarehouseParent).RemovePackages(packageInThisShipment.ToArray());
					}
				}
			}
		}

		#endregion

		#region CreatePackLines

		public void CreatePackLines(bool removeOnly = false)
		{
			AttachPackages(AttachedPackages);
			var packagesToRemove = removeOnly ? RemovedPackages.Where(p => p.IsRemovedFromAttachedPacklinesAndHasJobNumber).ToArray() : ModuleFilterPackageCollection.Where(p => p.IsRemovedFromAttachedPacklinesAndHasJobNumber).ToArray();
			RemovePackage(packagesToRemove);

			var packagesToAdd = AttachedPackages.Where(p => p.IsAddedToAttachedPackageCollection).ToArray();
			ResetAttachPackages(packagesToAdd, packagesToRemove);
		}

		static void ResetAttachPackages(WhsItemPackageState[] packagesToAdd, WhsItemPackageState[] packagesToRemove)
		{
			packagesToAdd.ForEach(p => p.ResetPackageStateStatus());
			packagesToRemove.ForEach(p => p.ResetPackageStateStatus());
		}

		void AttachPackages(IEnumerable<WhsItemPackageState> packagesToAttach)
		{
			foreach (var package in packagesToAttach)
			{
				AddJobNumberAdditionalReference(package);
			}
			Parent.AttachPackages(packagesToAttach.ToArray());
		}

		void AddJobNumberAdditionalReference(WhsItemPackageState packageState)
		{
			if (!packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Any(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference && c.CE_EntryNum == Parent.JobNumber))
			{
				var allExistingBookingPartyReferences = packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Where(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference).ToArray();
				allExistingBookingPartyReferences.ForEach(a => a.Delete());

				var reference = packageState.AdditionalReferenceNumbers.AddNew();
				reference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
				reference.CE_EntryNum = Parent.JobNumber;
			}
		}

		void RemovePackage(IEnumerable<WhsItemPackageState> packageStatesToRemove)
		{
			var packageStatesToBeRemoved = new List<WhsItemPackageState>();
			foreach (var packageStateToRemove in packageStatesToRemove)
			{
				var isRemoved = RemoveReference(packageStateToRemove);
				if (isRemoved)
				{
					packageStatesToBeRemoved.Add(packageStateToRemove);
					packageStateToRemove.ParentJobNumber = ZString.Empty;
				}

				var handlingUnit = packageStateToRemove.HandlingUnit;
				if (handlingUnit != null)
				{
					RemoveReference(handlingUnit);
					handlingUnit.ParentJobNumber = ZString.Empty;
				}
			}

			if (packageStatesToBeRemoved.Any())
			{
				Parent.RemovePackages(packageStatesToBeRemoved.ToArray());
			}
		}

		bool RemoveReference(WhsItemPackageState packageState)
		{
			var result = false;
			var references = packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Where(c => c.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference && c.CE_EntryNum == Parent.JobNumber).ToArray();
			if (references.Any())
			{
				references.ForEach(r => ((BusinessObject)r).Delete());

				result = true;
			}

			return result;
		}

		#endregion

		#region SetParentTranportCompany

		public void SetParentTranportCompany()
		{
			var transportCompanies = AttachedPackages
									.Select(p => p.ReceiveTransportationUnit)
									.WhereNotNull()
									.Cast<WhsItemReceiveTransportationUnit>()
									.Select(rtu => rtu.TransportCompany);
			if (transportCompanies.Any() && !transportCompanies.Any(c => c.OrganisationPK.IsEmpty))
			{
				var firstTransportCompanyDocAddress = transportCompanies.First();
				var firstTransportCompany = firstTransportCompanyDocAddress.Organisation;
				if (firstTransportCompany.OH_IsShippingProvider && firstTransportCompany.OH_IsLocalTransport
					&& !transportCompanies.Any(c => c.OrganisationPK != firstTransportCompany.PK))
				{
					Parent.SetTransportCompany(firstTransportCompanyDocAddress.E2_OA_Address);
				}
			}
		}

		#endregion

		#region Cancel

		public void Cancel()
		{
			ModuleFilterPackageCollection.ForEach(p => p.ResetPackageStateStatus());
			AttachedPackages.ForEach(p => p.ResetPackageStateStatus());
		}

		#endregion
	}
}
