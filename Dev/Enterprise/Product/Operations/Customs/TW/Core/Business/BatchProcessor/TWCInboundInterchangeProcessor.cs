using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	public class TWCInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public TWCInboundInterchangeProcessor() : base()
		{
		}

		public TWCInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIInterchange.ApplicationCodes.TaiwanCustoms }; }
		}

		protected override Type TypeOfInterchangeToCreate()
		{
			return typeof(TWCInterchange);
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new InboundMessageCreator(Logger));
		}
		IInboundMessageCreator messageCreator;

		#region InboundMessageCreator Class

		class InboundMessageCreator : IInboundMessageCreator
		{
			public InboundMessageCreator(LoggingInformation logger)
			{
				this.logger = logger;
			}

			readonly LoggingInformation logger;

			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				var twinterchange = interchange as TWCInterchange;
				if (twinterchange != null)
				{
					var messageCreated = twinterchange.CreateMessagesFromInterchageXml(logger);
					if (!messageCreated)
					{
						logger?.LogWarning(Res.GetString("84E0FD41-BD00-40FE-BAFC-87B0C06AD8B8", "No message has been created for interchange {0}", interchange.EI_InterchangeNum));
					}
				}
			}
		}

		#endregion
	}
}
