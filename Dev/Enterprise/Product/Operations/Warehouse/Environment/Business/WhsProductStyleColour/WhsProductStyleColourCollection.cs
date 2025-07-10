using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleColourCollection : ActiveBusinessObjectCollection<WhsProductStyleColour>
	{
		public WhsProductStyleColourCollection(WhsProductStyle productStyle)
			: base(productStyle.Factory, productStyle, new ZQuery(), WhsProductStyleColourSchema.WSC_WST_ProductStyle)
		{
		}
	}
}
