using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business
{
	public class MessageChooserLookups : ASYCUDA.Business.MessageChooserLookups
	{
		public MessageChooserLookups(MessageChooser parent)
			: base(parent)
		{
		}

		protected new MessageChooser Parent => (MessageChooser)base.Parent;

		public CodeDescriptionPairList CycleNumbers => Parent.Header.Factory.GetCycleNumbers();
	}
}
