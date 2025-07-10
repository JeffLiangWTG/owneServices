using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class DetentionAdviceHeader : AutoDetentionAdviceHeader, IDocumentSupportable
	{
		public DetentionAdviceHeader(OrgHeader client)
			: base(client == null ? null : client.Factory)
		{
			if (client == null)
			{
				throw new ArgumentNullException(nameof(client));
			}

			this.client = client;
		}

		public DetentionAdviceHeader(BusinessObjectFactory factory)
			: base(factory)
		{
			this.client = null;
		}

		#region Related Business Objects

		public OrgHeader Client
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return client; }
		}

		public BillOfLadingContainerCollection Containers
		{
			get { return containers ?? (containers = new BillOfLadingContainerCollection(Factory, new AdhocCollectionRelationship(typeof(BillOfLadingContainer)))); }
		}
		BillOfLadingContainerCollection containers;

		public ContainerMovementCollection Movements
		{
			get { return movements ?? (movements = new ContainerMovementCollection(Factory, false, new AdhocCollectionRelationship(typeof(ContainerMovement)))); }
		}
		ContainerMovementCollection movements;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new DetentionAdviceHeaderDocumentSupporter(this); }
		}

		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OrgHeader client;
	}
}


