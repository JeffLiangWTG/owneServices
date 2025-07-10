using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefVesselFilterBusinessObject))]
	sealed class RefVesselFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestVesselName()
		{
			Vessel1.RV_Code = "Bob";
			Vessel2.RV_Code = "Fread";
			Factory.Save();

			RefVessel[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselCollection.FilterConstants.VesselName];

			strip.Property = "";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestCarrierCode()
		{
			Vessel1.RV_CarrierCode = "Bob";
			Vessel2.RV_CarrierCode = "XXX";
			Factory.Save();

			RefVessel[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselCollection.FilterConstants.CarrierCode];

			strip.Property = "";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestLloydsNumber()
		{
			Vessel1.RV_LloydsNumber = "Bob";
			Vessel2.RV_LloydsNumber = "Fread";
			Factory.Save();

			RefVessel[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselCollection.FilterConstants.LloydsNumber];

			strip.Property = "";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestRadioCallSign()
		{
			Vessel1.RV_RadioCallSign = "Bob";
			Vessel2.RV_RadioCallSign = "Fread";
			Factory.Save();

			RefVessel[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselCollection.FilterConstants.RadioCallSign];
			strip.Property = "";

			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = "Bob";
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestVesselType()
		{
			Vessel1.RV_VesselType = Constants.VesselType.BulkCarrier;
			Vessel2.RV_VesselType = Constants.VesselType.ContainerisedVessel;
			Factory.Save();

			RefVessel[] result;
			ModuleTextFilter strip = (ModuleTextFilter)Filter[RefVesselCollection.FilterConstants.VesselType];
			strip.Property = "";

			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Constants.VesselType.BulkCarrier;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestCountryOfRegistration()
		{
			Vessel1.RV_RN_NKCountryOfReg = Constants.CountryCodes.Australia;
			Vessel2.RV_RN_NKCountryOfReg = Constants.CountryCodes.China;
			Factory.Save();

			RefVessel[] result;
			ModuleNkFilter strip = (ModuleNkFilter)Filter[RefVesselCollection.FilterConstants.CountryOfRegistration];

			strip.Property = ZString.Empty;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Constants.CountryCodes.Australia;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestCarrier()
		{
			Vessel1.RV_OH = Carrier1.PK;
			Vessel2.RV_OH = Carrier2.PK;
			Factory.Save();

			RefVessel[] result;
			ModuleGuidFilter strip = (ModuleGuidFilter)Filter[RefVesselCollection.FilterConstants.Carrier];

			strip.Property = ZGuid.Empty;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Carrier1.PK;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestConsortium()
		{
			Vessel1.RV_RG = Consortium1.PK;
			Vessel2.RV_RG = Consortium2.PK;
			Factory.Save();

			RefVessel[] result;
			ModuleGuidFilter strip = (ModuleGuidFilter)Filter[RefVesselCollection.FilterConstants.Consortium];

			strip.Property = ZGuid.Empty;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionContains("Vessel2", Vessel2, result);

			strip.Property = Consortium1.PK;
			result = Factory.Load<RefVessel>(GetQuery(strip));
			AssertCollectionContains("Vessel1", Vessel1, result);
			AssertCollectionNotContains("Vessel2", Vessel2, result);
		}

		public void TestVesselNameFilterIsNotExclusive()
		{
			var vesselNameFilter = Filter[RefVesselCollection.FilterConstants.VesselName];
			var isExclusiveInfo = typeof(ModuleFilter).GetProperty("IsExclusive", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("Vessel name filter is not exclusive, as RV_Code is not unique.", false, isExclusiveInfo.GetValue(vesselNameFilter));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ClearVesselRecordsBeforeTesting();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefVesselFilterBusinessObject();
		}

		ZQuery GetQuery(ModuleFilter strip)
		{
			var query = new ZQuery();
			query.AddToFilter(strip.Query);
			return query;
		}

		void ClearVesselRecordsBeforeTesting()
		{
			TestCaseHelper.ClearTable(RefVesselSchema.Constants.TableName);
		}

		RefVessel Vessel1
		{
			get
			{
				if (vessel1 == null)
				{
					vessel1 = Factory.New<RefVessel>();
					vessel1.RV_Code = "Random Vessel 1";
				}
				return vessel1;
			}
		}
		RefVessel vessel1;

		RefVessel Vessel2
		{
			get
			{
				if (vessel2 == null)
				{
					vessel2 = Factory.New<RefVessel>();
					vessel2.RV_Code = "Random Vessel 2";
				}
				return vessel2;
			}
		}
		RefVessel vessel2;

		OrgHeader Carrier1
		{
			get
			{
				if (carrier1 == null)
				{
					carrier1 = Factory.NewWithValidTestData<OrgHeader>();
					carrier1.OH_IsShippingLine = true;
				}
				return carrier1;
			}
		}
		OrgHeader carrier1;

		OrgHeader Carrier2
		{
			get
			{
				if (carrier2 == null)
				{
					carrier2 = Factory.NewWithValidTestData<OrgHeader>();
					carrier2.OH_IsShippingLine = true;
				}
				return carrier2;
			}
		}
		OrgHeader carrier2;

		RefCarrierConsortium Consortium1
		{
			get
			{
				if (consortium1 == null)
				{
					consortium1 = Factory.New<RefCarrierConsortium>();
					consortium1.RG_Code = "Consortium 1";
				}
				return consortium1;
			}
		}
		RefCarrierConsortium consortium1;

		RefCarrierConsortium Consortium2
		{
			get
			{
				if (consortium2 == null)
				{
					consortium2 = Factory.New<RefCarrierConsortium>();
					consortium2.RG_Code = "Consortium 2";
				}
				return consortium2;
			}
		}
		RefCarrierConsortium consortium2;

		RefVesselFilterBusinessObject Filter
		{
			get { return filter ?? (filter = new RefVesselFilterBusinessObject()); }
		}
		RefVesselFilterBusinessObject filter;

		#endregion
	}
}
