using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalRequestInfoTemplateLookups : AutoExternalRequestInfoTemplateLookups
	{
		public ExternalRequestInfoTemplateLookups(AutoExternalRequestInfoTemplate parent) : base(parent)
		{
		}

		public CodeDescriptionPairList JobTypeList => new ExternalRequestTypeJobTypes();
	}
}
