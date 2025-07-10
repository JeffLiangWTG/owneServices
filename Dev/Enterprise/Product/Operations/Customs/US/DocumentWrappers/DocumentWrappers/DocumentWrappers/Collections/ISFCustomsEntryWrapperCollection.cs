using CargoWise.EntityFramework;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class ISFCustomsEntryWrapperCollection : CustomsEntryWrapperCollection
	{
		public ISFCustomsEntryWrapperCollection(CusISFHeader headerBO, BusinessObjectFactory factory)
			: base(factory)
		{
			if (headerBO != null && !headerBO.BF_CustomsReference.IsEmpty)
			{
				Add(new CustomsEntryWrapperFromCusISFHeader(headerBO, Factory));
			}
		}
	}
}
