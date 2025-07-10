using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class NonPersistentBusinessObjectCollectionHolder<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public NonPersistentBusinessObjectCollectionHolder(VAT404DocumentInstruction parent, Func<VAT404DocumentInstruction, T> createNPBO) : base(parent.Factory)
		{
			createNonPersistentBO = createNPBO;
			this.parent = parent;
		}

		readonly VAT404DocumentInstruction parent;
		readonly Func<VAT404DocumentInstruction, T> createNonPersistentBO;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return createNonPersistentBO?.Invoke(parent);
		}
	}
}
