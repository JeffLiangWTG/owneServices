using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefAirlineCommodityCodeSchema.Constants.RAC_Code), DescriptionProperty(RefAirlineCommodityCodeSchema.Constants.RAC_Description)]
	public class RefAirlineCommodityCode : AutoRefAirlineCommodityCode
	{
		public RefAirlineCommodityCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("2BD9B848-ECDC-4083-BC2F-BA383347DB44", "Airline Commodity Code - {0}", CalculateShortcutName());

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			RAC_Description = "Test airline commodity code description";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

	}
}
