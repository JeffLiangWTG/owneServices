using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocEntryHeaderCommercialChargesCollection : DocumentWrapperCollection, IBusinessObjectCollection
	{
		public DocEntryHeaderCommercialChargesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocEntryHeaderCommercialCharge this[int index]
		{
			get
			{
				return (DocEntryHeaderCommercialCharge)Elements[index];
			}
		}
	}
}
