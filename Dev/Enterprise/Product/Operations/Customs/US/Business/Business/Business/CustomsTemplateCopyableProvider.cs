using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;

namespace Enterprise.Customs.US.Business
{
	class CustomsTemplateCopyableProvider : Integration.Customs.US.ICustomsTemplateCopyableProvider
	{
		public void CloneCountrySpecificData(ICommonShipment originalObject, ICommonShipment clonedObject)
		{
			if (originalObject is ForwardingShipment originalShipment
				&& originalShipment.InBondHeader is CusInBondHeader originalHeader
				&& clonedObject is ForwardingShipment clonedShipment)
			{
				var copiedTemplate = ((ITemplateCopyable)originalHeader).TemplateCopy();
				var newHeader = (CusInBondHeader)copiedTemplate;
				newHeader.BH_ParentID = clonedShipment.PK;
				newHeader.BH_ParentTableCode = clonedShipment.TablePrefix;
			}
		}
	}
}
