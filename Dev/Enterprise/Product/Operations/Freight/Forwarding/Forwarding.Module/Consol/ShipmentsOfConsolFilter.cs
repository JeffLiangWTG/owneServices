using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	class ShipmentsOfConsolFilter : ModuleGuidPivotFilter
	{
		public ShipmentsOfConsolFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.JobShipment, JobConShipLinkSchema.JN_JS, JobConShipLinkSchema.JN_JK, listDelegate, typeof(ForwardingConsol), typeof(JobConShipLink))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("f68b0fcc-67d0-4793-9c7c-dece0a461b2a", "Related Shipments");
		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
