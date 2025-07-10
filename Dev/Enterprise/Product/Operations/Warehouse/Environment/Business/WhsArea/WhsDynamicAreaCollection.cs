using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsDynamicAreaCollection : WhsAreaCollection, IWhsDynamicAreaCollection
	{
		public WhsDynamicAreaCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults);
		}

		public WhsDynamicAreaCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults);
		}

		public static WhsDynamicAreaCollection GetDynamicPickingAreas(BusinessObjectFactory factory, WhsWarehouse whs)
		{
			var areas = new WhsDynamicAreaCollection(factory, whs, new ZQuery(WhsAreaSchema.WA_IsPickingArea, true));
			areas.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.IsPickingArea, "Property0", ZBool.True, false));
			return areas;
		}

		void AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults filterDefaults)
		{
			var dpf = (ZString)CodeLists.AreaTypes.Codes.DynamicPickFace;
			AdditionalFilter = new ZQuery(WhsAreaSchema.WA_AreaType, dpf);
			filterDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.AreaType, "Property", dpf, false));
		}

		WhsDynamicAreaCollection(BusinessObjectFactory factory, BusinessObject whs, ZQuery filter)
			: base(factory, whs, filter, WhsAreaSchema.WA_WW_Whs)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			errors.Add(Res.GetString("62f3a307-399a-46c5-aac2-04853ff58397", "Only Dynamic Areas are valid."));
		}

		protected override void SetDefaultsForNewElementCore(WhsArea area)
		{
			base.SetDefaultsForNewElementCore(area);
			area.WA_AreaType = CodeLists.AreaTypes.Codes.DynamicPickFace;
		}
	}
}
