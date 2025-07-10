using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolForTest : CommonConsol, IJobHeaderParent
	{
		public CommonConsolForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool IsPropertyReadOnlyDueToPhaseCore(ZString propertyName)
		{
			return base.IsPropertyReadOnlyDueToPhaseCore(propertyName) || PropertiesForcedToReadOnlyDueToPhase.Contains(propertyName);
		}
		public List<string> PropertiesForcedToReadOnlyDueToPhase = new List<string>();

		public void SetShippingLineBaseOnAirlineForTest(RefAirline airline)
		{
			base.SetShippingLineBaseOnAirline(airline);
		}

		public ShipmentCollectionView PrepaidShipmentForTesting
		{
			get { return base.PrePaidShipments; }
		}

		public ShipmentCollectionView CollectShipmentsForTesting
		{
			get { return base.CollectShipments; }
		}

		public ZBool PrepaidIsMultiCurrency
		{
			get { return base.PrepaidShipmentsAreMultiCurrency; }
		}

		public ZBool CollectIsMultiCurrency
		{
			get { return base.CollectShipmentsAreMultiCurrency; }
		}

		public CommonConsolDocumentSupporterForTest DocumentSupporterForTest
		{
			get { return new CommonConsolDocumentSupporterForTest(this); }
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			return new ContainerDocumentSupportForTestCollection(this);
		}

		class ContainerDocumentSupportForTestCollection : CommonContainerCollection
		{
			public ContainerDocumentSupportForTestCollection(CommonConsolForTest consol)
				: base(consol, consol.Factory)
			{ }

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				return typeof(ContainerDocumentSupportForTest);
			}
		}

		public class ContainerDocumentSupportForTest : CommonContainer, IDocumentSupportable
		{
			public ContainerDocumentSupportForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			#region IDocumentSupportable Members

			public DocumentSupporter DocumentSupporter
			{
				get { return null; }
			}

			#endregion
		}

		#region IJobHeaderParent

		bool IJobHeaderParent.AllowInvoiceDeletion { get { return false; } }

		public string JobNumber { get { return PK.ToStringKey(); } }

		void IJobHeaderParent.OnJobCreated(JobHeader job) { }

		void IJobHeaderParent.OnJobCreating(JobHeader job) { }

		void IJobHeaderParent.OnJobDeleted(JobHeader job) { }

		void IJobHeaderParent.OnJobDeleting(JobHeader job) { }

		void IJobHeaderParent.SetJobNumberFieldOnSaving() { }

		#endregion
	}
}
