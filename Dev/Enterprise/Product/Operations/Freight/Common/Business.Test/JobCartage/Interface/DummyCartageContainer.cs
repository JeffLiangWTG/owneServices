using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Common.Business.Testing
{
	public sealed class DummyCartageContainer : NonPersistentBusinessObject, ICartageContainer
	{
		public DummyCartageContainer(BusinessObjectFactory factory, ZString containerNumber, ZString containerMode, ZGuid containerRC)
			: base(factory)
		{
			this.containerNumber = containerNumber;
			this.containerMode = containerMode;
			this.containerRC = containerRC;
		}

		public DummyCartageContainer(BusinessObjectFactory factory, ZString containerNumber, ZString containerMode, ZGuid containerRC, ZDecimal netWeight, ZString seal)
			: this(factory, containerNumber, containerMode, containerRC)
		{
			this.netWeight = netWeight;
			this.seal = seal;
		}

		public DummyCartageContainer(BusinessObjectFactory factory, ZString containerNumber, ZString containerMode, ZGuid containerRC, ZDecimal netWeight, ZString seal, ZGuid jobContainerPK)
			: this(factory, containerNumber, containerMode, containerRC, netWeight, seal)
		{
			this.jobContainerPK = jobContainerPK;
		}

		#region ICartageContainer Members

		ZGuid ICartageContainer.JobContainerPK
		{
			get { return jobContainerPK; }
		}
		readonly ZGuid jobContainerPK = ZGuid.Empty;

		ZString ICartageContainer.ContainerNumber
		{
			get { return containerNumber; }
		}
		readonly ZString containerNumber = "";

		ZString ICartageContainer.ContainerMode
		{
			get { return containerMode; }
		}
		readonly ZString containerMode = "";

		ZGuid ICartageContainer.ContainerRC
		{
			get { return containerRC; }
		}
		readonly ZGuid containerRC = ZGuid.Empty;

		ZDecimal ICartageContainer.NetWeight
		{
			get { return netWeight; }
		}
		readonly ZDecimal netWeight = 0m;

		ZString ICartageContainer.Seal
		{
			get { return seal; }
		}
		readonly ZString seal = "";

		IReadOnlyCollection<ICartageLooseCargo> ICartageContainer.LooseCargo
		{
			get { return System.Array.Empty<ICartageLooseCargo>(); }
		}

		#endregion
	}
}
