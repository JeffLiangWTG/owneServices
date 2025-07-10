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
	class ConsolsOfShipmentFilter : ModuleGuidPivotFilter
	{
		public ConsolsOfShipmentFilter(ZString description, GetList listDelegate)
			: base(description, ModuleIDs.JobConsol, JobConShipLinkSchema.JN_JK, JobConShipLinkSchema.JN_JS, listDelegate, typeof(ForwardingShipment), typeof(JobConShipLink))
		{
			SetDefaultProperties();
		}

		void SetDefaultProperties()
		{
			MultilingualDescription = DefaultDescription;
		}

		static MultilingualString DefaultDescription => ResString.GetMultilingualString("6c87919a-2109-4d4a-af8c-4b94e06bb22e", "Related Consolidations");
		protected override FilterCategory DefaultCategory => FilterCategories.Other;
	}
}
