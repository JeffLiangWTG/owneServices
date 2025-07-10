using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.Customs.Business.ArchiveManager
{
	public abstract class CustomsArchiveableBusinessObjectProviderAssistant : IArchiveableBusinessObjectProviderAssistant
	{
		public virtual IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get { yield break; }
		}

		public virtual IEnumerable<string> TableNamesSupported
		{
			get { yield break; }
		}

		public abstract IArchiveableBusinessObject LoadArchiveableBusinessObject(BusinessObject businessObjectLoadedByProvider);

		public abstract string CountryCode { get; }
	}
}
