using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class AdditionalReferencesParent : IReferencesParent
	{
		protected AdditionalReferencesParent() { }

		public List<KeyValuePair<ZString, ZString>> AdditionalReferences { get; set; }
	}
}
