using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefVesselZZFilterBusinessObject))]
	sealed class RefVesselZZFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestVesselName()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel1.ZZO_Code = "Bob";
			Vessel2.ZZO_Code = "Fread";
			Factory.Save();

			RefVesselZZ[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.VesselName];

			strip.Property = "";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestLloydsNumber()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel1.ZZO_LloydsNumber = "Bob";
			Vessel2.ZZO_LloydsNumber = "Fread";
			Factory.Save();

			RefVesselZZ[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.LloydsNumber];

			strip.Property = "";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestRadioCallSign()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel1.ZZO_RadioCallSign = "Bob";
			Vessel2.ZZO_RadioCallSign = "Fread";
			Factory.Save();

			RefVesselZZ[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.RadioCallSign];
			strip.Property = "";

			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestVesselType()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel1.ZZO_VesselType = Constants.VesselType.BulkCarrier;
			Vessel2.ZZO_VesselType = Constants.VesselType.ContainerisedVessel;
			Factory.Save();

			RefVesselZZ[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.VesselType];
			strip.Property = "";

			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Constants.VesselType.BulkCarrier;
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestCountryOfRegistration()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			Vessel1.ZZO_RN_NKCountryOfReg = Constants.CountryCodes.Australia;
			Vessel2.ZZO_RN_NKCountryOfReg = Constants.CountryCodes.China;
			Factory.Save();

			RefVesselZZ[] result;
			ModuleNkFilter strip = (ModuleNkFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.CountryOfRegistration];

			strip.Property = ZString.Empty;
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Constants.CountryCodes.Australia;
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
		}

		public void TestCarrierCodeSearchReturnsCorrectVessels()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var vessel1 = helper.CreateVesselZZ("HS BIZET", "030W", "VA", "ZA");
			var vessel2 = helper.CreateVesselZZ("HS BIZET", "A8MI3", "VA", "ZA");
			var vessel3 = helper.CreateVesselZZ("HS BIZET", "A8MI3", "VA", "GB");
			var carrier1 = helper.CreateCarrierCode("CSSC", "Carrier1", "ZA");
			var carrier2 = helper.CreateCarrierCode("KMM", "Carrier2", "ZA");
			var carrier3 = helper.CreateCarrierCode("COS", "Carrier3", "ZA");
			var carrier4 = helper.CreateCarrierCode("BDAS", "Carrier4", "ZA");
			var carrier5 = helper.CreateCarrierCode("BDAS", "Carrier4", "GB");
			_ = helper.CreateCarrierVesselPivot(carrier1.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier2.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier3.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier4.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier3.PK, vessel2.PK);
			Factory.Save();

			RefVesselZZ[] result;
			var strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.CarrierCodes];

			strip.Property = "";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertEquals("default filter - retrieve all 3 vessels", 3, result.Length);
			AssertCollectionContains("Vessel1", vessel1, result);
			AssertCollectionContains("Vessel2", vessel2, result);
			AssertCollectionContains("Vessel3", vessel3, result);

			strip.Property = "COS";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertEquals("Only 2 ZA vessels that contain COS carrier code", 2, result.Length);
			AssertCollectionContains("Vessel1", vessel1, result);
			AssertCollectionContains("Vessel2", vessel2, result);

			strip.Property = "KMM";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertEquals("Only 1 ZA vessel that contain KKM carrier code", 1, result.Length);
			AssertCollectionContains("Vessel1", vessel1, result);
		}

		public void TestCarrierNameSearchReturnsCorrectVessels()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var vessel1 = helper.CreateVesselZZ("HS BIZET", "030W", "VA", "ZA");
			var vessel2 = helper.CreateVesselZZ("HS BIZET", "A8MI3", "VA", "ZA");
			var vessel3 = helper.CreateVesselZZ("HS BIZET", "A8MI3", "VA", "GB");
			var carrier1 = helper.CreateCarrierCode("CSSC", "Carrier1", "ZA");
			var carrier2 = helper.CreateCarrierCode("KMM", "Carrier2", "ZA");
			var carrier3 = helper.CreateCarrierCode("COS", "Carrier3", "ZA");
			var carrier4 = helper.CreateCarrierCode("BDAS", "Carrier4", "ZA");
			var carrier5 = helper.CreateCarrierCode("BDAS", "Carrier4", "GB");
			_ = helper.CreateCarrierVesselPivot(carrier1.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier2.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier3.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier4.PK, vessel1.PK);
			_ = helper.CreateCarrierVesselPivot(carrier3.PK, vessel2.PK);
			Factory.Save();

			RefVesselZZ[] result;
			var strip = (ModuleTextFilter)Filter[RefVesselZZFilterBusinessObject.Descriptions.CarrierNames];

			strip.Property = "";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertEquals("default filter - retrieve all 3 vessels", 3, result.Length);
			AssertCollectionContains("Vessel1", vessel1, result);
			AssertCollectionContains("Vessel2", vessel2, result);
			AssertCollectionContains("Vessel3", vessel3, result);

			strip.Property = "Carrier3";
			result = Factory.Load<RefVesselZZ>(GetQuery(strip));
			AssertEquals(2, result.Length);
			AssertCollectionContains("Vessel1", vessel1, result);
			AssertCollectionContains("Vessel1", vessel2, result);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ClearVesselRecordsBeforeTesting();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefVesselZZFilterBusinessObject();
		}

		ZQuery GetQuery(ModuleFilter strip)
		{
			var query = new ZQuery();
			query.AddToFilter(strip.Query);
			return query;
		}

		void ClearVesselRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable(RefVesselZZSchema.Constants.TableName);
		}

		RefVesselZZ Vessel1
		{
			get
			{
				if (vessel1 == null)
				{
					vessel1 = Factory.New<RefVesselZZ>();
					vessel1.ZZO_Code = "Random Vessel 1";
					vessel1.ZZO_ZZZ_NKDataGrouping = "ZA";
				}
				return vessel1;
			}
		}
		RefVesselZZ vessel1;

		RefVesselZZ Vessel2
		{
			get
			{
				if (vessel2 == null)
				{
					vessel2 = Factory.New<RefVesselZZ>();
					vessel2.ZZO_Code = "Random Vessel 2";
					vessel2.ZZO_ZZZ_NKDataGrouping = "ZA";
				}
				return vessel2;
			}
		}
		RefVesselZZ vessel2;

		RefVesselZZFilterBusinessObject Filter
		{
			get { return filter ?? (filter = new RefVesselZZFilterBusinessObject()); }
		}
		RefVesselZZFilterBusinessObject filter;

		#endregion
	}
}
