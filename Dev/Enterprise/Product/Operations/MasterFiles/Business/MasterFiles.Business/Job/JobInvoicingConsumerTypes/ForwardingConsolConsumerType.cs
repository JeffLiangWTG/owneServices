using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ForwardingConsolConsumerType : ConsolConsumerType
	{
		public ForwardingConsolConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override bool IsActive
		{
			get { return false; }
		}
	}
}
