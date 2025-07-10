using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class ContainerCommodityCodeCollection : RefCommodityCodeCollection
	{
		protected ContainerCommodityCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults();
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add(GetErrorMessageWhenAdditionalFilterNotMet());
		}

		protected abstract string GetErrorMessageWhenAdditionalFilterNotMet();

		void SetFilterBusinessObjectDefaults()
		{
			var filterBoDefault = new FilterBusinessObjectDefault("Commodity Type", "Property1", ZBool.True);
			FilterBusinessObjectDefaults.Add(filterBoDefault);
		}
	}
}
