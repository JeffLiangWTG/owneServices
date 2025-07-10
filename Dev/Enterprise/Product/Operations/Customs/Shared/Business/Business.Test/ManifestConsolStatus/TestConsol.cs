using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class TestConsol : ForwardingConsol
	{
		public TestConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		TestMessageCollection fMessagess;
		public new TestMessageCollection Messages
		{
			get
			{
				if (fMessagess == null)
				{
					fMessagess = new TestMessageCollection(this, Factory);
				}
				return fMessagess;
			}
		}
	}
}
