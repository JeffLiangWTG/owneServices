using System;
using Enterprise.Edifact;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	static class NzEdifactMessageFactory
	{
		[ThreadStatic]
		static MessageFactory fNZCMessageFactory;

		public static MessageFactory NZCMessageFactory
		{
			get
			{
				if (fNZCMessageFactory == null)
				{
					fNZCMessageFactory = new MessageFactory(
							new Edifact.D03A.EdifactD03AMessageFactory(),
							new Edifact.D96B.EdifactD96BMessageFactory(),
							new Edifact.D98A.EdifactD98AMessageFactory());
				}
				return fNZCMessageFactory;
			}
		}
	}
}

