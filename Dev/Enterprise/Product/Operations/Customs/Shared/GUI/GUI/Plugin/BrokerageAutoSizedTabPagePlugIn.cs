using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI
{
	public class BrokerageAutoSizedTabPagePlugIn : ZTabPagePlugIn
	{
		protected override bool IsAutoSized => true;

		public BrokerageAutoSizedTabPagePlugIn(ZPlugIn plugIn)
			: base(plugIn)
		{
		}
	}
}
