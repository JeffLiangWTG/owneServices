using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class CusGoodsCatalogWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusGoodsCatalogWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CusGoodsCatalogWorkflowDescriptor", "Goods Catalog");

		public override Type WorkflowProviderType => typeof(BaseCusGoodsCatalog);

		public override ControllerID ControllerID => ControllerIDs.Customs.GoodsCatalog;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsUniversalTemplates => false;
	}
}

