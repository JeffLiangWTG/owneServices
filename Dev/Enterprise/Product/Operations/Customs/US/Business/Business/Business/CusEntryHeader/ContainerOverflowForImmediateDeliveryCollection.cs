using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ContainerOverflowForImmediateDeliveryCollection : NonPersistentBusinessObjectCollection<ContainerOverflowForImmediateDelivery>
	{
		public ContainerOverflowForImmediateDeliveryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ContainerOverflowForImmediateDeliveryCollection(ArrayList extraContainers, BusinessObjectFactory factory)
			: base(factory)
		{
			this.extraContainers = extraContainers;
			PopulateOverflowContainers();
		}

		readonly ArrayList extraContainers = new ArrayList();
		const int containersPerLine = 7;

		void PopulateOverflowContainers()
		{
			ZInt row = ZInt.Zero;
			ZInt containerCountThisLine = ZInt.Zero;
			ZString containerString = ZString.Empty;

			foreach (ZString containerNo in extraContainers)
			{
				containerCountThisLine++;
				containerString = containerString + containerNo + ", ";

				if (containerCountThisLine > containersPerLine)
				{
					this.Add(new ContainerOverflowForImmediateDelivery());
					this[row].ContainerString = containerString;
					containerString = ZString.Empty;
					containerCountThisLine = ZInt.Zero;
					row++;
				}
			}

			if (containerCountThisLine > 0)
			{
				this.Add(new ContainerOverflowForImmediateDelivery());
				this[row].ContainerString = containerString.Remove(containerString.Length - 2, 2);
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}
	}
}
