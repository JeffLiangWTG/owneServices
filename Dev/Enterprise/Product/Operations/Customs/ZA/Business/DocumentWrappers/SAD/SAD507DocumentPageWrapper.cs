using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class SAD507DocumentPageWrapper : NonPersistentBusinessObject
	{
		public SAD507DocumentPageWrapper(IEnumerable<CusContainerDocWrapper> containers, IEnumerable<AdditionalInformationDocWrapperWithLineNumber> additionalInformations)
		{
			this.containers = new BusinessObjectCollectionWrapper<CusContainerDocWrapper>(containers);
			this.additionalInformations = new BusinessObjectCollectionWrapper<AdditionalInformationDocWrapperWithLineNumber>(additionalInformations);
		}

		public BusinessObjectCollectionWrapper<CusContainerDocWrapper> Containers => containers;

		readonly BusinessObjectCollectionWrapper<CusContainerDocWrapper> containers;

		public BusinessObjectCollectionWrapper<AdditionalInformationDocWrapperWithLineNumber> AdditionalInformations => additionalInformations;

		readonly BusinessObjectCollectionWrapper<AdditionalInformationDocWrapperWithLineNumber> additionalInformations;
	}
}
