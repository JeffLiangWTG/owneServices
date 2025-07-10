using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentRNSStatusProviderTest : TestCaseWithFactory
	{
		public void TestDefaultType()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes._TemplateCountryName_);
			CFSShipmentRNSStatusProvider provider = CFSShipmentRNSStatusProvider.New(Shipment);
			AssertEquals(typeof(CFSShipmentRNSStatusProvider.BlankCFSShipmentRNSStatusProvider), provider.GetType());
		}

		public void TestCanadianType()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			CFSShipmentRNSStatusProvider provider = CFSShipmentRNSStatusProvider.New(Shipment);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICFSShipmentRNSStatusProvider>(), provider.GetType());
		}

		public void TestProperties()
		{
			var dummyStatusProvider = new CFSShipmentRNSStatusProviderDummyObject(Shipment);
			dummyStatusProvider.TransactionNumber_Exposed = "123478956456";
			dummyStatusProvider.ReleaseStatusCode_Exposed = "CLR";
			dummyStatusProvider.ReleaseStatus_Exposed = "Goods Released";
			dummyStatusProvider.ReleaseDate_Exposed = new DateTime(2014, 12, 4);
			dummyStatusProvider.ArrivalCertificationStatusCode_Exposed = "REJ";
			dummyStatusProvider.ArrivalCertificationStatus_Exposed = "Rejected";
			dummyStatusProvider.ArrivalCertificationDate_Exposed = new DateTime(2014, 12, 5);

			CFSShipmentRNSStatusProvider.DummyForTest = dummyStatusProvider;
			var statusProvider = CFSShipmentRNSStatusProvider.New(Shipment);
			AssertEquals("TransactionNumber", "123478956456", statusProvider.TransactionNumber);
			AssertEquals("ReleaseStatusCode", "CLR", statusProvider.ReleaseStatusCode);
			AssertEquals("ReleaseStatus", "Goods Released", statusProvider.ReleaseStatus);
			AssertEquals("ReleaseDate", new DateTime(2014, 12, 4), statusProvider.ReleaseDate);
			AssertEquals("ArrivalCertificationStatusCode", "REJ", statusProvider.ArrivalCertificationStatusCode);
			AssertEquals("ArrivalCertificationStatus", "Rejected", statusProvider.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", new DateTime(2014, 12, 5), statusProvider.ArrivalCertificationDate);
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

		public class CFSShipmentRNSStatusProviderDummyObject : CFSShipmentRNSStatusProvider
		{
			public CFSShipmentRNSStatusProviderDummyObject(CFSShipment shipment)
				: base(shipment)
			{
			}

			public ZString TransactionNumber_Exposed;
			protected override ZString TransactionNumberCore()
			{
				return TransactionNumber_Exposed;
			}

			public ZString ReleaseStatus_Exposed;
			protected override ZString ReleaseStatusCore()
			{
				return ReleaseStatus_Exposed;
			}

			public ZDateTime ReleaseDate_Exposed;
			protected override ZDateTime ReleaseDateCore()
			{
				return ReleaseDate_Exposed;
			}

			public ZString ReleaseStatusCode_Exposed;
			protected override ZString ReleaseStatusCodeCore()
			{
				return ReleaseStatusCode_Exposed;
			}

			public ZString ArrivalCertificationStatus_Exposed;
			protected override ZString ArrivalCertificationStatusCore()
			{
				return ArrivalCertificationStatus_Exposed;
			}

			public ZDateTime ArrivalCertificationDate_Exposed;
			protected override ZDateTime ArrivalCertificationDateCore()
			{
				return ArrivalCertificationDate_Exposed;
			}

			public ZString ArrivalCertificationStatusCode_Exposed;
			protected override ZString ArrivalCertificationStatusCodeCore()
			{
				return ArrivalCertificationStatusCode_Exposed;
			}
		}
	}
}
