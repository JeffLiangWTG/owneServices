using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Freight.Business.Testing
{
	public class MockJobDocsAndCartageParent : NonPersistentBusinessObject, IShipmentWithDocsAndCartage
	{
		public MockJobDocsAndCartageParent(BusinessObjectFactory factory) : base(factory)
		{
		}

		CommonShipment IShipmentProvider.Shipment
		{
			get { return fShipment; }
		}

		public override string TablePrefix => "JS";

		public void SetShipment(CommonShipment shipment)
		{
			fShipment = shipment;
		}

		CommonShipment fShipment;

		public ArrayList Containers = new ArrayList();

		#region IDocsAndCartageParent Members

		IHaveRequiredDocuments IDocsAndCartageParent.RequiredDocumentsProvider
		{
			get { return null; }
		}

		public JobDocsAndCartage DocsAndCartage
		{
			get { return null; }
		}

		Type IDocsAndCartageParent.DocsAndCartageType
		{
			get { return typeof(JobDocsAndCartage); }
		}

		Type IDocsAndCartageParent.DocsAndCartageParentType
		{
			get { return this.GetType(); }
		}

		ZGuid IHaveInternalCartage.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		IContainer[] IHaveInternalCartage.GetContainers()
		{
			return (IContainer[])Containers.ToArray(typeof(IContainer));
		}

		ZString IHaveInternalCartage.OwnerRef
		{
			get { return ZString.Empty; }
		}

		bool IHaveInternalCartage.IsAllowedToUpdateAdviseDates
		{
			get { return false; }
		}

		ZString IHaveInternalCartage.ServiceLevel
		{
			get { return ""; }
		}

		IPackLineInfo[] IHaveInternalCartage.GetPackLines()
		{
			return null;
		}

		OrgHeader IHaveInternalCartage.PickupCartageOrg
		{
			get { return null; }
		}

		OrgHeader IHaveInternalCartage.DeliveryCartageOrg
		{
			get { return null; }
		}

		ZBool IHaveInternalCartage.DeliveryAndPickupCartageApplicable
		{
			get { return ZBool.True; }
		}

		ZBool IHaveInternalCartage.PackedAtDepot
		{
			get { return ZBool.False; }
		}

		public ZString ContainerMode;
		ZString IHaveInternalCartage.ContainerMode
		{
			get { return this.ContainerMode; }
		}

		public ZString TransportMode;
		ZString IHaveInternalCartage.TransportMode
		{
			get { return this.TransportMode; }
		}

		public ZString CartageTypeOverride
		{
			get { return ZString.Empty; }
		}

		bool IHaveInternalCartage.IsForPickupCartage
		{
			get { return true; }
		}

		ZGuid IHaveInternalCartage.CartagePickupDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryDepotAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartagePickupCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryCTOAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartagePickupContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		ZGuid IHaveInternalCartage.CartageDeliveryContainerYardAddress
		{
			get { return ZGuid.Empty; }
		}

		JobDocAddress IHaveInternalCartage.CartageExporterDocAddress
		{
			get { return null; }
		}

		JobDocAddress IHaveInternalCartage.CartageImporterDocAddress
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupDepotAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryDepotAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupCTOAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryCTOAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartagePickupContainerYardAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.CartageDeliveryContainerYardAddressInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.DeliveryCartageAdvisedInfo
		{
			get { return null; }
		}

		ZPropertyInfo IHaveInternalCartage.PickupCartageAdvisedInfo
		{
			get { return null; }
		}

		bool IHaveInternalCartage.TypeSpecificPreCreationCheck()
		{
			return true;
		}

		ZBool IHaveInternalCartage.InternalCartageEnabled
		{
			get { return IsInternalCartageAllowed; }
		}

		protected virtual ZBool IsInternalCartageAllowed
		{
			get { return ZBool.False; }
		}

		event EventHandler IShipmentWithDocsAndCartage.TransportModeChanged
		{
			add { }
			remove { }
		}

		public bool ShouldValidateDeliveryAndPickupCartageCoBeingSame = true;
		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryAndPickupCartageCoBeingSame()
		{
			return this.ShouldValidateDeliveryAndPickupCartageCoBeingSame;
		}

		public bool ShouldValidateDeliveryCo = true;
		bool IShipmentWithDocsAndCartage.ShouldValidateDeliveryCoPK()
		{
			return this.ShouldValidateDeliveryCo;
		}

		public bool RequiresOrderNumbersOnDocs;
		bool IShipmentWithDocsAndCartage.RequiresOrderNumbersOnDocs()
		{
			return this.RequiresOrderNumbersOnDocs;
		}

		public bool RequiresOrderTrackLink;
		bool IShipmentWithDocsAndCartage.RequiresOrderTrackLink()
		{
			return this.RequiresOrderTrackLink;
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsigneeDocumentaryAddress
		{
			get { return null; }
		}

		JobDocAddress IShipmentWithDocsAndCartage.ConsignorDocumentaryAddress
		{
			get { return null; }
		}

		JobDocsAndCartageValidation IShipmentWithDocsAndCartage.PiggyBackedValidation
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.UniqueConsignRef
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.HouseBillNumber
		{
			get { return null; }
		}

		string IShipmentWithDocsAndCartage.MasterBillNumber
		{
			get { return null; }
		}

		OrgHeader IShipmentWithDocsAndCartage.ExportBroker
		{
			get { return null; }
		}

		#endregion

		#region ICartageXmlExportSupport

		public ZString JobNumber;
		string ICartageExportSupport.JobNumber
		{
			get { return this.JobNumber; }
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get
			{
				if (IsImport)
				{
					return Directions.Import;
				}

				if (IsExport)
				{
					return Directions.Export;
				}

				return Directions.Unknown;
			}
		}

		public ZBool IsImport;
		public bool IsExport;

		#endregion

		#region IDocAddresses

		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
				}
				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return Array.Empty<DocAddressType>(); }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IDocAddresses Members

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
			throw new NotImplementedException();
		}

		public void DocAddressChanged(JobDocAddress docAddress)
		{
			throw new NotImplementedException();
		}

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			throw new NotImplementedException();
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
			throw new NotImplementedException();
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
