using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentStatusProviderTest : TestCaseWithFactory
	{
		public void TestDefaultType()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes._TemplateCountryName_);
			CFSShipmentStatusProvider provider = CFSShipmentStatusProvider.New(Shipment);
			AssertEquals(typeof(BlankCFSShipmentStatusProvider), provider.GetType());
		}

		public void TestAustraliaType()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			CFSShipmentStatusProvider provider = CFSShipmentStatusProvider.New(Shipment);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.ICFSShipmentStatusProvider>(), provider.GetType());
		}

		public void TestCanadianType()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			CFSShipmentStatusProvider provider = CFSShipmentStatusProvider.New(Shipment);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICFSShipmentStatusProvider>(), provider.GetType());
		}

		public void TestCanSaveAndPrintCoreOverride()
		{
			var mock = new Mock<ISaveAndPrintUI>();
			CFSShipmentStatusProviderDummyObject provider = new CFSShipmentStatusProviderDummyObject(Shipment);

			mock.Setup(m => m.Ask("foo!")).Returns(true);
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			mock.Setup(m => m.Ask("foo!")).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();
		}

		public void TestDetailsFromMessages()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();
			CFSShipmentStatusProviderDummyObject dummy = new CFSShipmentStatusProviderDummyObject(shipment);
			AssertEquals("Cuckoo Squeaker of Message Details", dummy.DetailsFromMessages);
		}

		#region Properties

		protected GatePassShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = GatePassShipment.New(Factory);
				}
				return shipment;
			}
		}
		GatePassShipment shipment;

		#endregion

		#region Implementation

		protected virtual string GetCountryCode()
		{
			return Enterprise.Core.Constants.CountryCodes._TemplateCountryName_;
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(GetCountryCode());
		}

		#endregion
	}
}
