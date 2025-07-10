using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterHelper : IEDIMessageContentFilterHelper
	{
		public IEDIMessageContentFilter GetEDIMessageContentFilter(IFactory factory, IEDIMessagePurpose purpose)
		{
			if (factory == null)
			{
				return null;
			}

			return EDIMessageContentFilter.Load(factory, purpose);
		}

		public IEDIMessagePurpose GetEDIMessagePurpose(IFactory factory, string purposeCode)
		{
			if (factory == null)
			{
				return null;
			}

			return EDIMessagePurpose.Load(factory, purposeCode);
		}
	}
}
