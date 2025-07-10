using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public sealed class ContainerParentMatch<T, U>
		where T : BusinessObject
		where U : IReferencesParent
	{
		public ContainerParentMatch(T parent, U reference)
		{
			this.parent = Argument.NotNull(parent, "parent");
			this.references = new List<U>
			{
				Argument.NotNull(reference, "reference")
			};
		}

		readonly T parent;
		readonly List<U> references;

		public T Parent => parent;

		public IReadOnlyCollection<U> References => references;

		public void AddReference(U reference)
		{
			if (reference != null)
			{
				references.Add(reference);
			}
		}
	}
}
