using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentContainerCollection : NonPersistentBusinessObjectCollection<DocumentContainer>
	{
		public DocumentContainerCollection(IEnumerable<CommonContainer> containers, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CommonContainer container in containers)
			{
				Add(new DocumentContainer(container));
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentContainer(Factory.New<CommonContainer>());
		}
	}
}
