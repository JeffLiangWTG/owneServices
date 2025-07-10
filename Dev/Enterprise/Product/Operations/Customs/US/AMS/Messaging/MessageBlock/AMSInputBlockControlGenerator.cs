using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSInputBlockControlGenerator : AMSOutputBlockControlGenerator
	{
		public AMSInputBlockControlGenerator()
		{
		}

		public AMSInputBlockControlGenerator(ZString applicationCode)
			: base(applicationCode)
		{
		}

		protected override MessageBlockDeserialiser MessageBlockDeserialiser
		{
			get { return new InputMessageBlockDeserialiser(); }
		}
	}
}
