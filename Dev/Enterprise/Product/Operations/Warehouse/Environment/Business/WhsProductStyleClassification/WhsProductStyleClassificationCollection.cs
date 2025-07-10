using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleClassificationCollection : ActiveBusinessObjectCollection<WhsProductStyleClassification>
	{
		public WhsProductStyleClassificationCollection(WhsProductStyle productStyle)
			: base(productStyle.Factory, productStyle, new ZQuery(), WhsProductStyleClassificationSchema.WSS_WST_ProductStyle)
		{
		}
	}
}
