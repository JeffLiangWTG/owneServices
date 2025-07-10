using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class EDIClientApplicationDescriptorList : CodeDescriptionPairList
	{
		public EDIClientApplicationDescriptorList()
		{
			foreach (var descriptor in ObjectFactory.Get<IEDIClientApplicationDescriptors>().Values)
			{
				AddPair(descriptor.Code, descriptor.Description);
			}
		}
	}
}
