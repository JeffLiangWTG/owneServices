using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHLineCollection : DocBaseWrapperCollection<NODocSADHLine>
{
	public NODocSADHLineCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public NODocSADHLineCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory)
	{
	}
}
