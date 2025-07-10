using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(Transport))]
	internal abstract class TransportBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		[TestedType(typeof(Transport))]
		public class Linked : TransportBOTest
		{
			protected override bool IsLinked
			{
				get { return true; }
			}
		}

		[TestedType(typeof(Transport))]
		public class Unlinked : TransportBOTest
		{
			protected override bool IsLinked
			{
				get { return false; }
			}
		}

		protected abstract bool IsLinked { get; }

		protected override BusinessObject GetNewBusinessObject()
		{
			Transport transport = Factory.New<CommonShipment>().Transports.AddNew();
			transport.JW_IsLinked = IsLinked;
			return transport;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consolType = ObjectFactory.GetType(typeof(IForwardingConsol));
			var consol = (CommonConsol)factory.NewWithValidTestData(consolType);

			var transport = consol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = IsLinked;

			return transport;
		}

		protected override void InstallBizoForTestDbHits(BusinessObject bizo)
		{
			var transport = bizo as Transport;
			if (transport != null)
			{
				transport.ParentType = ObjectFactory.GetType(typeof(IForwardingConsol));
			}
		}

		#endregion
	}
}
