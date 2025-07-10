using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobConsolFilterBusinessObject))]
	public class JobConsolFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Job Consol Shipments Filter

		class TestPopupFindBox : ZPopupFindBox
		{
			public IFindBoxPopup PopForm
			{
				get
				{
					return base.PopupForm;
				}
			}
		}

		[RequiresSTA]
		public void TestAddRelatedShipmentsFiltersWithNoRight()
		{
			AssertNoExceptionThrown(() =>
			{
				using (JobConsolModule module = (JobConsolModule)ZModuleFactory.Instance.Create(ModuleIDs.JobConsol))
				using (TestPopupFindBox pop = new TestPopupFindBox())
				{
					Env.Security.MaintainConsol.IsAllowed = false;
					var list = (module.FilterBusinessObject.ToList()
															.FirstOrDefault(o => o.GetType() == typeof(ShipmentsOfConsolFilter)) as ModuleFilterWithList).List;
					pop.ModuleID = ModuleIDs.JobConsol;
					pop.List = list;
					AssertNotNull(pop.List);

					var form = pop.PopForm;
				}
			});
		}

		#endregion

		#region Numbers and References

		public void TestReferenceNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "consol1";
			Asserter.AddToScope(consol1);

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "consol2";
			Asserter.AddToScope(consol2);

			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_UniqueConsignRef = "consol3";
			Asserter.AddToScope(consol3);

			ForwardingConsol consol4 = Factory.New<ForwardingConsol>();
			consol4.JK_UniqueConsignRef = "consol4";
			Asserter.AddToScope(consol4);

			ForwardingConsol consol5 = Factory.New<ForwardingConsol>();
			consol5.JK_UniqueConsignRef = "consol5";
			Asserter.AddToScope(consol5);

			NewReferenceNumber(consol1, "AU", "COC", "MUNDANE");
			NewReferenceNumber(consol2, "US", "COC", "MAGIC");
			NewReferenceNumber(consol3, "AU", "COC", "MAGIC");
			NewReferenceNumber(consol4, "AU", "COC", "INSANE");

			NewReferenceNumber(consol1, "AU", "ASL", "MAGIC");
			NewReferenceNumber(consol2, "AU", "ASL", "");

			AssertNotNull("lazy load", consol5);

			Factory.Save();

			Asserter.AddFieldOfInterest("AU:COC", (s) => GetValue(s, "AU", "COC"));
			Asserter.AddFieldOfInterest("US:COC", (s) => GetValue(s, "US", "COC"));
			Asserter.AddFieldOfInterest("AU:ASL", (s) => GetValue(s, "AU", "ASL"));

			ReferenceNumberFilter filter = (ReferenceNumberFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AdditionalReferenceNumbers];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			Asserter.AssertMatches("empty", filter, consol1, consol2, consol3, consol4, consol5);

			SetFilter(filter, "AU", "COC", "MAGIC");
			Asserter.AssertMatches("AU:COC:MAGIC*", filter, consol3);

			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("COC:MAGIC*", filter, consol2, consol3);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			SetFilter(filter, "", "ASL", "");
			Asserter.AssertMatches("Without ASL", filter, consol2, consol3, consol4, consol5);

			SetFilter(filter, "", "ASL", "");
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			Asserter.AssertMatches("With ASL", filter, consol1);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("Without COC:MAGIC", filter, consol1, consol4, consol5);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			SetFilter(filter, "", "COC", "ANE");
			Asserter.AssertMatches("Without COC:*ANE", filter, consol2, consol3, consol5);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			SetFilter(filter, "", "COC", "M");
			Asserter.AssertMatches("Without COC:M*", filter, consol4, consol5);
		}

		#region Test Common Numbers Filter

		public void TestCommonNumbersFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "C66666666";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C66666622";
			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Common Numbers and References"];

			AssertEquals(consolFilter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(35));
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = "C6666";
			results.Load(FilterStripBizO.Filter);
			Assert("Both consols have numbers starting with C6666", results.Contains(consol1.PK));
			Assert("Both consols have numbers starting with C6666", results.Contains(consol2.PK));

			consolFilter.Property = "C66666622";
			results.Load(FilterStripBizO.Filter);
			Assert("There is no such number in this consol", !results.Contains(consol1.PK));
			Assert("This consol has such number", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Booking Reference Number

		public void TestBookingReferenceFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();

			consol1.JK_BookingReference = "1234567890";
			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol2.JK_BookingReference = "0987654321";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol3.JK_BookingReference = "1234567890";
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol4.JK_BookingReference = "1234567890";
			consol4.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol4.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterStripBizO["Booking Reference #"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			AssertEquals("Prefix should be B", "B", consolFilter.Prefix);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));
			Assert("Not filtered - should be included", results.Contains(consol3.PK));
			Assert("Not filtered - should be included", results.Contains(consol4.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));
			Assert("Should include Consol3", results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));

			consolFilter.Property = "0987";
			results.Load(FilterStripBizO.Filter);
			Assert("Should NOT include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.Contains(consol4.PK));
		}

		#endregion

		#region Test Co-Load Master Bill Number

		public void TestCoLoadMasterBillFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();
			var consol5 = Factory.New<ForwardingConsol>();
			var consol6 = Factory.New<ForwardingConsol>();
			var consol7 = Factory.New<ForwardingConsol>();

			consol1.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol1.JK_CoLoadMasterBill = "1234567890";
			consol2.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol2.JK_CoLoadMasterBill = "123-4567890";
			consol3.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol3.JK_CoLoadMasterBill = "0987654321";
			consol4.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol4.JK_CoLoadMasterBill = "123-456-7890";
			consol5.JK_AgentType = Enterprise.Core.Constants.AgentType.Direct;
			consol5.JK_CoLoadMasterBill = "1234567890";
			consol6.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol6.JK_CoLoadMasterBill = "1234567890";
			consol7.JK_AgentType = Enterprise.Core.Constants.AgentType.CoLoad;
			consol7.JK_CoLoadMasterBill = "123-456-7890";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterStripBizO["Co-Load Master Bill #"];

			AssertEquals("Prefix should be L", "L", consolFilter.Prefix);

			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));
			Assert("Not filtered - should be included", results.Contains(consol3.PK));
			Assert("Not filtered - should be included", results.Contains(consol4.PK));
			Assert("Not filtered - should be included", results.Contains(consol5.PK));
			Assert("Not filtered - should be included", results.Contains(consol6.PK));
			Assert("Not filtered - should be included", results.Contains(consol7.PK));

			consolFilter.Property = "123-4567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.Contains(consol5.PK));
			Assert("Should include Consol6", results.Contains(consol6.PK));
			Assert("Should NOT include Consol7", !results.Contains(consol7.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.Contains(consol5.PK));
			Assert("Should include Consol6", results.Contains(consol6.PK));
			Assert("Should NOT include Consol7", !results.Contains(consol7.PK));

			consolFilter.Property = "123-456-7890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.Contains(consol5.PK));
			Assert("Should include Consol6", results.Contains(consol6.PK));
			Assert("Should include Consol7", results.Contains(consol7.PK));

			consolFilter.Property = "123";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));
			Assert("Should NOT include Consol5", !results.Contains(consol5.PK));
			Assert("Should include Consol6", results.Contains(consol6.PK));
			Assert("Should include Consol7", results.Contains(consol7.PK));
		}

		#endregion

		#region Test Co-Load Booking Reference

		public void TestCoLoadBookingReferenceFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();
			var consol5 = Factory.New<ForwardingConsol>();

			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			consol1.JK_CoLoadBookingReference = "1234567890";

			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			consol2.JK_CoLoadBookingReference = "0987654321";

			consol3.JK_AgentType = Constants.AgentType.Direct;
			consol3.JK_CoLoadBookingReference = "1234567890";

			consol4.JK_AgentType = Constants.AgentType.CoLoad;
			consol4.JK_CoLoadBookingReference = "1234567890";

			consol5.JK_AgentType = Constants.AgentType.CoLoad;
			consol5.JK_CoLoadBookingReference = "0987654321";

			Factory.Save();

			var consolFilter = (ModuleNumberFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.CoLoadBookingReference];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));
			Assert("Not filtered - should be included", results.Contains(consol3.PK));
			Assert("Not filtered - should be included", results.Contains(consol4.PK));
			Assert("Not filtered - should be included", results.Contains(consol5.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should not include Consol2", !results.Contains(consol2.PK));
			Assert("Should not include Consol3 as its AgentType is Direct", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));
			Assert("Should not include Consol5", !results.Contains(consol5.PK));

			consolFilter.Property = "123";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should not include Consol2", !results.Contains(consol2.PK));
			Assert("Should not include Consol3 as its AgentType is Direct", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));
			Assert("Should not include Consol5", !results.Contains(consol5.PK));
		}

		#endregion

		#region Test Consolidation Number

		public void TestConsolNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C12345678";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C12345666";
			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Consol #"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = "C123";
			results.Load(FilterStripBizO.Filter);
			Assert("Both consols numbers start with C123", results.Contains(consol1.PK));
			Assert("Both consols numbers start with C123", results.Contains(consol2.PK));

			consolFilter.Property = "C12345678";
			results.Load(FilterStripBizO.Filter);
			Assert("This consol must be in the result collection", results.Contains(consol1.PK));
			Assert("This consol has other number", !results.Contains(consol2.PK));
		}

		#endregion

		#region Test Container Number

		public void TestContainerNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "AAB";

			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Container #"];
			AssertEquals("Prefix should be T", "T", consolFilter.Prefix);
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = "AA";
			results.Load(FilterStripBizO.Filter);
			Assert("Both consols have containers starting with AA", results.Contains(consol1.PK));
			Assert("Both consols have containers starting with AA", results.Contains(consol2.PK));

			consolFilter.Property = "AAB";
			results.Load(FilterStripBizO.Filter);
			Assert("Container in this consol has other number", !results.Contains(consol1.PK));
			Assert("Its container from this consol", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Vessel Flight/Voyage # Filter

		public void TestVoyageVesselFilter()
		{
			ForwardingConsol consol1 = NewConsol("tvfvf1", false, "AUMEL", "SGSIN");
			Transport transport1 = consol1.Transports[0];
			transport1.JW_Vessel = Factory.NewWithValidTestData<RefVessel>().RV_FK;
			transport1.JW_VoyageFlight = "2222";

			ForwardingConsol consol2 = NewConsol("tvfvf2", false, "SGSIN", "NZAKL");
			Transport transport2 = consol2.Transports[0];
			transport2.JW_Vessel = "NKVessel";
			transport2.JW_VoyageFlight = "2233";
			Factory.Save();

			var consolFilter = (VoyageVesselModuleFilter)FilterStripBizO["Flight/Voyage # and Vessel"];
			AssertEquals("Prefix should be V", "V", consolFilter.Prefix);
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be in collection", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.Contains(consol2.PK));

			consolFilter.VoyageFlightNo = "22";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol should be in collection", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.Contains(consol2.PK));

			consolFilter.VoyageFlightNo = "2233";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol should not be in collection", !results.Contains(consol1.PK));
			Assert("Consol2 should be in collection", results.Contains(consol2.PK));

			consolFilter.VoyageFlightNo = "";
			consolFilter.Vessel = transport1.JW_Vessel;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be in collection", results.Contains(consol1.PK));
			Assert("Consol2 should not be in collection", !results.Contains(consol2.PK));

			consolFilter.Vessel = "NK";
			consolFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should not be in the collection", true, !results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));

			consolFilter.VoyageFlightNo = "";
			consolFilter.Vessel = "";
			consolFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in collection", true, results.Contains(consol2.PK));

			consolFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should not be in collection", true, !results.Contains(consol1.PK));
			AssertEquals("Consol2 should not be in collection", true, !results.Contains(consol2.PK));
		}

		#endregion

		#region Test House Bill Number

		public void TestHouseBillNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB6666666";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB6666777";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterStripBizO["House Bill"];
			AssertEquals("Prefix should be H", "H", consolFilter.Prefix);
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property = "HB6";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));

			consolFilter.Property = "HB6666777";
			results.Load(FilterStripBizO.Filter);
			Assert("Should not include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Master Bill Number

		public void TestMasterBillNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol4 = Factory.New<ForwardingConsol>();
			consol1.JK_MasterBillNum = "1234567890";
			consol2.JK_MasterBillNum = "123-4567890";
			consol3.JK_MasterBillNum = "0987654321";
			consol4.JK_MasterBillNum = "123-456-7890";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterStripBizO["Master Bill"];
			AssertEquals("Prefix should be M", "M", consolFilter.Prefix);
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));
			Assert("Not filtered - should be included", results.Contains(consol3.PK));
			Assert("Not filtered - should be included", results.Contains(consol4.PK));

			consolFilter.Property = "123-4567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.Contains(consol4.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should NOT include Consol4", !results.Contains(consol4.PK));

			consolFilter.Property = "123-456-7890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));

			consolFilter.Property = "123";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
			Assert("Should NOT include Consol3", !results.Contains(consol3.PK));
			Assert("Should include Consol4", results.Contains(consol4.PK));
		}

		#endregion

		#region Test Carrier Contract Number

		public void TestCarrierContractNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol1.JK_CarrierContractNumber = "1234567890";
			consol2.JK_CarrierContractNumber = "0987654321";
			Factory.Save();

			var consolFilter = (ModuleNumberFilter)FilterStripBizO["Carrier Contract #"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));

			consolFilter.Property = "123";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));

			consolFilter.Property = "0987654321";
			results.Load(FilterStripBizO.Filter);
			Assert("Should NOT include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));

			consolFilter.Property = "098";
			results.Load(FilterStripBizO.Filter);
			Assert("Should NOT include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Shipment Number

		public void TestShipmentNumberFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S66666666";
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S66666777";
			Factory.Save();

			ModuleNumberFilter consolFilter = (ModuleNumberFilter)FilterStripBizO["Shipment #"];
			AssertEquals("Prefix should be S", "S", consolFilter.Prefix);
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property = "S66";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));

			consolFilter.Property = "S66666777";
			results.Load(FilterStripBizO.Filter);
			Assert("Should not include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
		}

		#endregion

		#region TestSendingarnumer

		public void TestSendingarnumer()
		{
			const string Sendingarnumer = "N-T88-4158-7-KT-VOD-B440-7";
			GlbCompany.CurrentCompany.SetCountry("IS");
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			CusEntryNumber cusEntryNum = consol.CusEntryNums.AddNew();
			cusEntryNum.CE_EntryNum = Sendingarnumer;
			cusEntryNum.CE_ParentID = consol.PK;
			cusEntryNum.CE_ParentTable = "JobConsol";
			cusEntryNum.CE_EntryType = "CRN";
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			JobConsolFilterBusinessObject filter = new JobConsolFilterBusinessObject();
			((ModuleTextFilter)filter["Sendingarnumer"]).Property = Sendingarnumer;
			((ModuleTextFilter)filter["Sendingarnumer"]).IsActive = true;

			ForwardingConsolCollection collection = new ForwardingConsolCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(consol.PK, collection[0].PK);
		}

		#endregion

		#region Test Agent Reference Number

		public void TestAgentReferenceNumber()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			consol1.JK_AgentsReference = "1234567890";
			consol2.JK_AgentsReference = "0987654321";

			Factory.Save();

			var consolFilter = (ModuleNumberFilter)FilterStripBizO["Agent Reference #"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property = "1234567890";
			results.Load(FilterStripBizO.Filter);
			Assert("Should include Consol1", results.Contains(consol1.PK));
			Assert("Should NOT include Consol2", !results.Contains(consol2.PK));

			consolFilter.Property = "0987";
			results.Load(FilterStripBizO.Filter);
			Assert("Should NOT include Consol1", !results.Contains(consol1.PK));
			Assert("Should include Consol2", results.Contains(consol2.PK));
		}

		#endregion

		#region TestFilterByAllocationId

		public void TestFilterByAllocationId()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingConsol = Factory.New<ForwardingConsol>();
				var allocationLine = forwardingConsol.AllocationLineCollection.AddNew();
				allocationLine.FillWithValidTestData();
				allocationLine.RCA_AllocationLineID = "ABC";
				forwardingConsol.JK_RCA_AllocationLine = allocationLine.PK;
				Factory.Save();
				ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AllocationID];
				filter.Property = "ABC";
				ForwardingConsol[] forwardingConsols = Factory.Load<ForwardingConsol>(filter.Query);
				AssertCollectionContains("Should find forwardingConsol", forwardingConsol, forwardingConsols);
			}
		}

		#endregion

		#region TestFilterByAllocationId_ExceedLength_NoException
		[ExpectNoExceptions]
		public void TestFilterByAllocationId_ExceedLength_NoException()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var forwardingConsol = Factory.New<ForwardingConsol>();
				ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AllocationID];
				filter.Property = "D00148347";
				ForwardingConsol[] forwardingConsols = Factory.Load<ForwardingConsol>(filter.Query);
				AssertCollectionNotContains("Should not find forwardingConsol", forwardingConsol, forwardingConsols);
			}
		}

		public void TestAllocationIDFilterHiddenForCCAWithPenaltiesOnly()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightConfigurationRegistry.Instance.EnableCarrierContractAllocations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNull(FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AllocationID]);
			}
		}

		#endregion

		#endregion

		#region Status and Flags

		#region Test Consols Without Shipments Filter

		public void TestNoShipmentsFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol1.Shipments.AddNew();
			Factory.Save();

			ModuleFlagsFilter consolFilter = (ModuleFlagsFilter)FilterStripBizO["Consols without Shipments"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be in collection.", !results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Consols With Shipments Filter

		public void TestConsolsWithShipmentsFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			Factory.Save();

			ModuleFlagsFilter consolFilter = (ModuleFlagsFilter)FilterStripBizO["Consols with Shipments"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should not be in collection.", !results.Contains(consol2.PK));

			consolFilter.IsActive = true;
			consolFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Routing Status

		public void TestRoutingStatus()
		{
			ForwardingConsol completeLinked = NewConsol("CompleteLinked", true, "AUBNE", "SGSIN", "USLAX");
			ForwardingConsol completeUnLinked = NewConsol("CompleteUnLinked", false, "AUBNE", "SGSIN", "USLAX");

			ForwardingConsol incomplete1 = NewConsol("Incomplete1", false, "AUBNE", "USLAX");
			incomplete1.Transports.RemoveAndDeleteAll();

			ForwardingConsol incomplete2 = NewConsol("Incomplete2", false, "AUBNE", "USLAX");
			incomplete2.Transports[0].JW_RL_NKDiscPort = "";

			ForwardingConsol incomplete3 = NewConsol("Incomplete3", false, "AUBNE", "USLAX");
			incomplete3.Transports[0].JW_RL_NKLoadPort = "";

			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Routing Status"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = "All";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have found CompleteLinked", true, results.Contains(completeLinked));
			AssertEquals("Should have found CompleteUnLinked", true, results.Contains(completeUnLinked));
			AssertEquals("Should have found Incomplete1", true, results.Contains(incomplete1));
			AssertEquals("Should have found Incomplete2", true, results.Contains(incomplete2));
			AssertEquals("Should have found Incomplete3", true, results.Contains(incomplete3));

			consolFilter.Property = "INC";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should not have found CompleteLinked", false, results.Contains(completeLinked));
			AssertEquals("Should not have found CompleteUnLinked", false, results.Contains(completeUnLinked));
			AssertEquals("Should have found Incomplete1", true, results.Contains(incomplete1));
			AssertEquals("Should have found Incomplete2", true, results.Contains(incomplete2));
			AssertEquals("Should have found Incomplete3", true, results.Contains(incomplete3));

			consolFilter.Property = "COM";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have found CompleteLinked", true, results.Contains(completeLinked));
			AssertEquals("Should have found CompleteUnLinked", true, results.Contains(completeUnLinked));
			AssertEquals("Should not have found Incomplete1", false, results.Contains(incomplete1));
			AssertEquals("Should not have found Incomplete2", false, results.Contains(incomplete2));
			AssertEquals("Should not have found Incomplete3", false, results.Contains(incomplete3));
		}

		#endregion

		#region Test Is Cargo Only

		public void TestIsCargoOnly()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport1 = consol1.Transports.AddNew();
			var transport2 = consol2.Transports.AddNew();

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_IsCargoOnly = true;
			voyage1.JV_VoyageFlight = "DI56";
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_IsCargoOnly = false;
			voyage2.JV_VoyageFlight = "DI99";
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage2.GenerateSailings();

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = voyage1.Sailings[0].PK;
			transport2.JW_JX = voyage2.Sailings[0].PK;
			Factory.Save();

			var consolFilter = (ModuleFlagsFilter)FilterStripBizO["Is Cargo Only"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			Assert("Consol1 should not be in collection.", !results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));

			consolFilter.IsActive = true;
			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should not be in collection.", !results.Contains(consol2.PK));

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var transport3 = consol3.Transports.AddNew();
			var voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_RV_NKVessel = "MANGO VESSEL";
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			voyage3.GenerateSailings();

			transport3.JW_JX = voyage3.Sailings[0].PK;
			transport3.JW_IsLinked = true;

			Factory.Save();

			consolFilter.IsActive = true;
			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should not be in collection.", !results.Contains(consol2.PK));
			Assert("Consol3 should not be in collection.", !results.Contains(consol3.PK));

			consolFilter.IsActive = true;
			consolFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			Assert("Consol1 should not be in collection.", !results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));
			Assert("Consol3 should be in collection.", results.Contains(consol3.PK));
		}

		#endregion

		#region Test Is Temperature Controlled

		public void TestIsTemperatureControlledFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RequiresTemperatureControl = true;
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RequiresTemperatureControl = false;
			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_RequiresTemperatureControl = true;

			Factory.Save();

			var results = new MainFormConsolCollection(Factory);
			var consolFilter = (ModuleFlagsFilter)FilterStripBizO["Is Temperature Controlled"];
			consolFilter.IsActive = true;
			consolFilter.Property0 = true;

			results.Load(FilterStripBizO.Filter);

			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should not be in collection.", !results.Contains(consol2.PK));
			Assert("Consol3 should be in collection.", results.Contains(consol3.PK));
		}

		#endregion

		#region Test Is Hazardous

		public void TestIsHazardous()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;

			consol1.JK_IsHazardous = true;
			consol2.JK_IsHazardous = false;

			Factory.Save();

			var consolFilter = (ModuleFlagsFilter)FilterStripBizO["Is Hazardous"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering consolidations for 'Is Hazardous' = true", () =>
			{
				Assert("Consol1 should be in collection as it is marked Hazardous.", results.Contains(consol1.PK));
				Assert("Consol2 should not be in the collection as it is not marked Hazardous.", !results.Contains(consol2.PK));
			});

			consolFilter.IsActive = true;
			consolFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering consolidations for 'Is Hazardous' = false", () =>
			{
				Assert("Consol1 should not be in collection as it is marked Hazardous.", !results.Contains(consol1.PK));
				Assert("Consol2 should be in the collection as it is not marked Hazardous.", results.Contains(consol2.PK));
			});
		}

		#endregion

		#region PreAllocated Amount Exceeded

		public void TestPreAllocatedAmountExceeded()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var currentDepartment = GlbDepartment.CurrentDepartment;

			var anotherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, currentBranch.GB_Code));
			var anotherDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.NotEqual, currentDepartment.GE_Code));

			var preAllocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			preAllocationChecks.Weight.Action = PreAllocationCheck.Actions.Warning;
			preAllocationChecks.Weight.Percentage = 50m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, currentBranch.PK.ToGuid(), currentDepartment.PK.ToGuid(), preAllocationChecks))
			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, anotherBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid(), preAllocationChecks))
			{
				var anotherFactory = new BusinessObjectFactory();
				var someConsol = anotherFactory.New<ForwardingConsol>();
				anotherFactory.Save();

				var exceededConsol1 = CreatePAAConsol(true, currentBranch, currentDepartment);
				var exceededConsol2 = CreatePAAConsol(true, anotherBranch, anotherDepartment);
				var notExceededConsol1 = CreatePAAConsol(false, currentBranch, currentDepartment);
				var notExceededConsol2 = CreatePAAConsol(false, anotherBranch, anotherDepartment);

				AssertPreAllocatedAmountExceededFilter("Should contain all 5 consols", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.All, someConsol, exceededConsol1, exceededConsol2, notExceededConsol1, notExceededConsol2);
				AssertPreAllocatedAmountExceededFilter("Should contain all pre-allocated amount exceeded consols", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.Exceeded, exceededConsol1, exceededConsol2);
				AssertPreAllocatedAmountExceededFilter("Should contain all pre-allocated amount not exceeded consols", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.NotExceeded, notExceededConsol1, notExceededConsol2, someConsol);

				AssertPreAllocatedAmountExceededFilter("Contains consol with pre-allocation exceeded for current Branch/Dept", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.ExceededUnderCurrentBranchAndDepartment, exceededConsol1);
				AssertPreAllocatedAmountExceededFilter("Contains consols with pre-allocation not exceeded for current Branch/Dept", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.NotExceededUnderCurrentBranchAndDepartment, notExceededConsol1, someConsol);

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), anotherBranch.PK.ToGuid(), anotherDepartment.PK.ToGuid()))
				{
					AssertPreAllocatedAmountExceededFilter("Contains consol with pre-allocation exceeded for other Branch/Dept", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.ExceededUnderCurrentBranchAndDepartment, exceededConsol2);
					AssertPreAllocatedAmountExceededFilter("Contains consol with pre-allocation not exceeded for other Branch/Dept", JobConsolFilterBusinessObject.PreAllocatedAmountExceededCodes.NotExceededUnderCurrentBranchAndDepartment, notExceededConsol2);
				}
			}
		}

		void AssertPreAllocatedAmountExceededFilter(string message, string preAllocatedAmountExceededCode, params ForwardingConsol[] expectedConsols)
		{
			var preAllocatedAmountExceededfilter = (ModuleTextFilter)FilterStripBizO["Pre-Allocated Amount Exceeded"];
			var preAllocatedAmountExceededResults = new ForwardingConsolCollection(Factory);

			preAllocatedAmountExceededfilter.IsActive = true;
			preAllocatedAmountExceededfilter.Property = preAllocatedAmountExceededCode;
			preAllocatedAmountExceededResults.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(message, expectedConsols.Select(c => c.PK), preAllocatedAmountExceededResults.Select(r => r.PK));
		}

		ForwardingConsol CreatePAAConsol(bool isPAAExceeded, GlbBranch branch, GlbDepartment department)
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				if (isPAAExceeded)
				{
					consol.JK_TotalShipmentActWeightCheck = 1000m;

					var shipment = consol.Shipments.AddNew();
					shipment.JS_ActualWeight = 600m;
				}

				Factory.Save();

				return consol;
			}
		}

		#endregion

		#region VGM Status

		public void TestVGMStatus()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var container1 = consol1.Containers.AddNew();
			consol1.JK_TransportMode = "SEA";
			container1.JC_ContainerNum = "AAA";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			container1.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;

			var consol2 = Factory.New<ForwardingConsol>();
			var container2 = consol2.Containers.AddNew();
			consol2.JK_TransportMode = "SEA";
			container2.JC_ContainerNum = "AAB";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;
			container2.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected;

			var consol3 = Factory.New<ForwardingConsol>();
			var container3 = consol3.Containers.AddNew();
			consol3.JK_TransportMode = "SEA";
			container3.JC_ContainerNum = "CCC";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			var consol4 = Factory.New<ForwardingConsol>();
			var container4 = consol4.Containers.AddNew();
			consol4.JK_TransportMode = "SEA";
			container4.JC_ContainerNum = "DDD";
			container4.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;
			container4.JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["VGM Status"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));
			Assert("Not filtered", results.Contains(consol3.PK));
			Assert("Not filtered", results.Contains(consol4.PK));

			consolFilter.Property = Constants.ContainerGrossWeightVerificationStatuses.Codes.AmendedNotSent;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));
			Assert("Consol3 should not be included", !results.Contains(consol3.PK));
			Assert("Consol4 should not be included", !results.Contains(consol4.PK));

			consolFilter.Property = Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
			Assert("Consol3 should not be included", !results.Contains(consol3.PK));
			Assert("Consol4 should be included", results.Contains(consol4.PK));

			consolFilter.Property = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotVerified;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));
			Assert("Consol3 should be included", results.Contains(consol3.PK));
			Assert("Consol4 should not be included", !results.Contains(consol4.PK));
		}

		#endregion

		#region Test Security Status

		public void TestSecurityStatus()
		{
			var consolWithSecurityStatus = Factory.New<ForwardingConsol>();
			consolWithSecurityStatus.JK_TransportMode = Constants.TransportModes.Air;
			consolWithSecurityStatus.SecurityStatusCode = "SCO";

			var consolWithoutSecurityStatus = Factory.New<ForwardingConsol>();
			consolWithoutSecurityStatus.JK_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirfreightSecurityStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Nothing should be filtered out.", results.Contains(consolWithSecurityStatus.PK));
			Assert("Nothing should be filtered out.", results.Contains(consolWithoutSecurityStatus.PK));

			consolFilter.Property = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol with security status 'SCO' should be included.", results.Contains(consolWithSecurityStatus.PK));
			Assert("Consol with blank security status should not be included.", !results.Contains(consolWithoutSecurityStatus.PK));
		}

		#endregion

		#region Test Air Booking Status

		public void TestAirBookingStatus_All()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			var consol4 = Factory.New<ForwardingConsol>();
			var consol5 = Factory.New<ForwardingConsol>();
			var consol6 = Factory.New<ForwardingConsol>();
			var consol7 = Factory.New<ForwardingConsol>();
			var consol8 = Factory.New<ForwardingConsol>();
			var consol9 = Factory.New<ForwardingConsol>();
			var notAirConsol = Factory.New<ForwardingConsol>();

			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.CancellationRequested);
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Cancelled);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.FlightNotOperating);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Held);
			AddAirBookingStatusToBooking(consol6, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Queued);
			AddAirBookingStatusToBooking(consol8, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol9, Constants.TransportStatus.Unable);

			var transport = notAirConsol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Status = Constants.TransportStatus.Confirmed;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.All;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol1.PK, consol2.PK, consol3.PK, consol4.PK, consol5.PK, consol6.PK, consol7.PK, consol8.PK, consol9.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		public void TestAirBookingStatus_AllConfirmed()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.Confirmed);

			var consol2 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Planned);

			var consol3 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.CancellationRequested);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Cancelled);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.FlightNotOperating);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Held);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Queued);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Unable);

			var consol4 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Confirmed);

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.AllFlightsConfirmed;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol1.PK, consol4.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		public void TestAirBookingStatus_ConfirmationPending()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.Confirmed);

			var consol2 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Requested);

			var consol3 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Queued);

			var consol4 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Queued);

			var consol5 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.CancellationRequested);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Cancelled);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.FlightNotOperating);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Held);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Unable);

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.ConfirmationPending;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol2.PK, consol3.PK, consol4.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		public void TestAirBookingStatus_CancellationPending()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.CancellationRequested);

			var consol2 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.CancellationRequested);
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Requested);

			var consol3 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Cancelled);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Queued);

			var consol4 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Queued);

			var consol5 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.FlightNotOperating);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Held);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Unable);

			var consol6 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol6, Constants.TransportStatus.Cancelled);

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.CancellationPending;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol1.PK, consol2.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		public void TestAirBookingStatus_NotRequested()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.Planned);

			var consol2 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.Cancelled);

			var consol3 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol3, string.Empty);

			var consol4 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Requested);

			var consol5 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Cancelled);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Planned);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Unable);

			var consol6 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol6, string.Empty);
			AddAirBookingStatusToBooking(consol6, Constants.TransportStatus.Unable);

			var consol7 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Unable);

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.NotRequested;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol1.PK, consol2.PK, consol3.PK, consol4.PK, consol5.PK, consol6.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		public void TestAirBookingStatus_Rejected()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol1, Constants.TransportStatus.Unable);

			var consol2 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol2, Constants.TransportStatus.FlightNotOperating);

			var consol3 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Unable);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol3, Constants.TransportStatus.Queued);

			var consol4 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.FlightNotOperating);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Requested);
			AddAirBookingStatusToBooking(consol4, Constants.TransportStatus.Queued);

			var consol5 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.CancellationRequested);
			AddAirBookingStatusToBooking(consol5, Constants.TransportStatus.Cancelled);

			var consol6 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol6, Constants.TransportStatus.Confirmed);
			AddAirBookingStatusToBooking(consol6, Constants.TransportStatus.Confirmed);

			var consol7 = Factory.New<ForwardingConsol>();
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Unable);
			AddAirBookingStatusToBooking(consol7, Constants.TransportStatus.Unable);
			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.AirBookingStatus];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.AirBookingStatus.Code.Rejected;
			results.Load(FilterStripBizO.Filter);

			var actual = results.Select(x => x.PK).OrderBy(x => x);
			var expected = new ZGuid[] { consol1.PK, consol2.PK, consol3.PK, consol4.PK, consol7.PK }.OrderBy(x => x);
			AssertSequencesEqual("Invalid Results", expected, actual);
		}

		void AddAirBookingStatusToBooking(CommonConsol consol, string status)
		{
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_Status = status;
		}

		#endregion

		#region Test Flight Status

		public void TestFlightStatus()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var transport1 = consol1.Transports.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;

			var consol2 = Factory.New<ForwardingConsol>();
			var transport2 = consol2.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			var consol3 = Factory.New<ForwardingConsol>();
			var transport3 = consol2.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport3.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Flight Status"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.FlightScheduleStatus.Matched;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering consolidations for 'Flight Status' = Matched", () =>
			{
				Assert("Consol1 should be in collection as it has a transport with Matched ('MTD') Flight Status.", results.Contains(consol1.PK));
				Assert("Consol2 should not be in collection as it does not have a transport with Matched ('MTD') Flight Status.", !results.Contains(consol2.PK));
				Assert("Consol3 should not be in collection as it does not have a transport with Air Transport Mode.", !results.Contains(consol3.PK));
			});

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.FlightScheduleStatus.PartiallyMatched;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering consolidations for 'Flight Status' = PMD", () =>
			{
				Assert("Consol1 should not be in collection as it does not have a transport with PartiallyMatched ('PMD') Flight Status.", !results.Contains(consol1.PK));
				Assert("Consol2 should be in collection as it has a transport with PartiallyMatched ('PMD') Flight Status.", results.Contains(consol2.PK));
				Assert("Consol3 should not be in collection as it does not have a transport with Air Transport Mode.", !results.Contains(consol3.PK));
			});
		}

		#endregion

		#region Test Carrier Booking Office Status

		public void TestCarrierBookingOfficeStatusFilter()
		{
			var shippingInstructionEventParameter = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.ShippingInstruction)
			};
			var bookingRequestEventParameter = new[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.MessageType, ConsolDocumentNames.BookingRequest)
			};

			ForwardingConsol CreateConsolWithEvent(KeyValuePair<string, string>[] parameters = null, params Event[] eventTypes)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

				var documentData = Factory.NewWithValidTestData<DummyAutoJobDocumentData>();
				documentData[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
				documentData[JobDocumentDataSchema.JDD_ParentID] = consol.PK;
				documentData[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

				var dateTime = new ZDateTimeOffset(2022, 03, 21);
				eventTypes?.ForEach(eventType => AddLog(documentData, eventType, dateTime.AddSeconds(1), parameters));
				return consol;
			}

			void AddLog(DummyAutoJobDocumentData documentData, Event @event, ZDateTimeOffset dateTimeOffset, KeyValuePair<string, string>[] eventParameters)
			{
				documentData.Logs.CreateOrRecreateEventLog(
					@event,
					EstimateActual.Actual,
					dateTimeOffset,
					ZString.Empty,
					eventParameters);

				Factory.Save();
			}

			var withoutDocumentDataConsol = Factory.New<ForwardingConsol>();
			withoutDocumentDataConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var notSentConsol = CreateConsolWithEvent();
			var brResetConsol = CreateConsolWithEvent(bookingRequestEventParameter, Events.StatusUpdated);
			var siResetConsol = CreateConsolWithEvent(bookingRequestEventParameter, Events.StatusUpdated);
			var brMessageSentConsol = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageSent);
			var brInterchangeReceiptAcknowledged = CreateConsolWithEvent(bookingRequestEventParameter, Events.InterchangeReceiptAcknowledged);
			var brInterchangeRejected = CreateConsolWithEvent(bookingRequestEventParameter, Events.InterchangeRejected);
			var brMessageAccepted = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageAccepted);
			var brMessageRejected = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageRejected);
			var brMessageWithdrawCancelRequest = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageWithdrawCancelRequest);
			var brMessageWithdrawCancelRejected = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageWithdrawCancelRequest, Events.MessageRejected);
			var brMessageWithdrawCancelAccepted = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageWithdrawCancelAccepted);
			var brMessagePendingProcessing = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessagePendingProcessing);
			var siMessageSentConsol = CreateConsolWithEvent(shippingInstructionEventParameter, Events.MessageSent);
			var siInterchangeReceiptAcknowledgedConsol = CreateConsolWithEvent(shippingInstructionEventParameter, Events.InterchangeReceiptAcknowledged);
			var siInterchangeRejectedConsol = CreateConsolWithEvent(shippingInstructionEventParameter, Events.InterchangeRejected);
			var siMessageAccepted = CreateConsolWithEvent(shippingInstructionEventParameter, Events.MessageAccepted);
			var siMessageRejected = CreateConsolWithEvent(shippingInstructionEventParameter, Events.MessageRejected);
			var siMessagePendingProcessing = CreateConsolWithEvent(shippingInstructionEventParameter, Events.MessagePendingProcessing);

			Factory.Save();

			List<ForwardingConsol> allConsol = new List<ForwardingConsol>()
				{
					withoutDocumentDataConsol,
					notSentConsol,
					brResetConsol,
					siResetConsol,
					brMessageSentConsol,
					brInterchangeReceiptAcknowledged,
					brInterchangeRejected,
					brMessageAccepted,
					brMessageRejected,
					brMessageWithdrawCancelRequest,
					brMessageWithdrawCancelRejected,
					brMessageWithdrawCancelAccepted,
					brMessagePendingProcessing,
					siMessagePendingProcessing,
					siMessageSentConsol,
					siInterchangeReceiptAcknowledgedConsol,
					siInterchangeRejectedConsol,
					siMessageAccepted,
					siMessageRejected
				};

			void AssertContainerConsol(ZString message, MainFormConsolCollection resultConsolCollection, params ForwardingConsol[] containerConsols)
			{
				var notContainerConsol = new List<ForwardingConsol>(allConsol);

				containerConsols?.ForEach(containerConsol => notContainerConsol.Remove(containerConsol));

				CombineAssertions(() =>
				{
					notContainerConsol.ForEach(consol => AssertEquals(message, false, resultConsolCollection.Contains(consol.PK)));
					containerConsols?.ForEach(consol => AssertEquals(message, true, resultConsolCollection.Contains(consol.PK)));
				});
			}

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Carrier Booking Status - Ocean"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("Not filtered", results, allConsol.ToArray());

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.NotSent;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("NotSent", results, notSentConsol, brResetConsol, siResetConsol, withoutDocumentDataConsol);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.Sent", results, brMessageSentConsol);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Acknowledged;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.Acknowledged", results, brInterchangeReceiptAcknowledged);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.RejectedByInterchange;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.RejectedByInterchange", results, brInterchangeRejected);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Confirmed;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.Confirmed", results, brMessageAccepted);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.Rejected", results, brMessageRejected);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.WithdrawalSent", results, brMessageWithdrawCancelRequest);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.WithdrawalRejected", results, brMessageWithdrawCancelRejected);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalAccepted;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.WithdrawalAccepted", results, brMessageWithdrawCancelAccepted);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("BookingRequest.PendingProcessing", results, brMessagePendingProcessing);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.Sent", results, siMessageSentConsol);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Acknowledged;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.Acknowledged", results, siInterchangeReceiptAcknowledgedConsol);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.RejectedByInterchange;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.RejectedByInterchange", results, siInterchangeRejectedConsol);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Confirmed;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.Confirmed", results, siMessageAccepted);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Rejected;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.Rejected", results, siMessageRejected);

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.PendingProcessing;
			results.Load(FilterStripBizO.Filter);
			AssertContainerConsol("ShippingInstruction.PendingProcessing", results, siMessagePendingProcessing);

			var brResetThenMessageWithdrawCancelRejected = CreateConsolWithEvent(bookingRequestEventParameter, Events.StatusUpdated, Events.MessageWithdrawCancelRequest, Events.MessageRejected);
			var brMessageWithdrawCancelRejectedThenReset = CreateConsolWithEvent(bookingRequestEventParameter, Events.MessageWithdrawCancelRequest, Events.StatusUpdated, Events.MessageRejected);

			Factory.Save();

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("MessageRejected", false, results.Contains(brResetThenMessageWithdrawCancelRejected.PK));
			AssertEquals("MessageRejected", true, results.Contains(brMessageWithdrawCancelRejectedThenReset.PK));

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("WithdrawalRejected", true, results.Contains(brResetThenMessageWithdrawCancelRejected.PK));
			AssertEquals("WithdrawalRejected", false, results.Contains(brMessageWithdrawCancelRejectedThenReset.PK));

			var hasBRAndSRSendConsol = Factory.New<ForwardingConsol>();
			hasBRAndSRSendConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var hasBRAndSRSendConsolDocumentData = Factory.NewWithValidTestData<DummyAutoJobDocumentData>();
			hasBRAndSRSendConsolDocumentData[JobDocumentDataSchema.JDD_ParentTableCode] = hasBRAndSRSendConsol.TablePrefix;
			hasBRAndSRSendConsolDocumentData[JobDocumentDataSchema.JDD_ParentID] = hasBRAndSRSendConsol.PK;
			hasBRAndSRSendConsolDocumentData[JobDocumentDataSchema.JDD_Name] = ConsolDocumentDataStoreNames.SeaBookingRequest2;

			hasBRAndSRSendConsolDocumentData.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 03, 21), shippingInstructionEventParameter);
			hasBRAndSRSendConsolDocumentData.Logs.AddNew(Events.MessageSent, new ZDateTimeOffset(2022, 03, 22), bookingRequestEventParameter);

			Factory.Save();

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent;
			results.Load(FilterStripBizO.Filter);

			AssertEquals("Booking Request and Shipping Instruction", false, results.Contains(hasBRAndSRSendConsol.PK));

			consolFilter.Property = FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent;
			results.Load(FilterStripBizO.Filter);

			AssertEquals("Booking Request and Shipping Instruction", true, results.Contains(hasBRAndSRSendConsol.PK));
		}

		class DummyAutoJobDocumentData : AutoJobDocumentData
		{
			public DummyAutoJobDocumentData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#endregion

		#region Release Type

		public void TestReleaseTypeFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_ReleaseType = Constants.ShipmentReleaseTypes.Indemnity;
			var consol4 = Factory.New<ForwardingConsol>();
			consol4.JK_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Release Type"];
			var results = new MainFormConsolCollection(Factory);
			consolFilter.IsActive = true;

			consolFilter.Property = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have 2 consols", 2, results.Count);
			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));

			consolFilter.Property = Constants.ShipmentReleaseTypes.Indemnity;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have 1 consols", 1, results.Count);
			Assert("Consol3 should be in collection.", results.Contains(consol3.PK));

			consolFilter.Property = Constants.ShipmentReleaseTypes.ExpressBofL;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have 1 consols", 1, results.Count);
			Assert("Consol4 should be in collection.", results.Contains(consol4.PK));
		}

		#endregion

		#region Possible Oversize

		public void TestPossibleOversizeFilter()
		{
			var consol1 = Factory.New<ForwardingModuleConsol>();
			var shipmentRail = consol1.Shipments.AddNew();
			shipmentRail.JS_TransportMode = Constants.TransportModes.Rail;
			var shipmentRoad = consol1.Shipments.AddNew();
			shipmentRoad.JS_TransportMode = Constants.TransportModes.Road;
			var shipmentCourier = consol1.Shipments.AddNew();
			shipmentCourier.JS_TransportMode = Constants.TransportModes.Courier;
			Assert("No shipment's transportation mode is AIR or SEA", !consol1.JK_Calc_PossibleOversize);

			var consol2 = Factory.New<ForwardingModuleConsol>();
			var shipment21Air = consol2.Shipments.AddNew();
			shipment21Air.JS_TransportMode = Constants.TransportModes.Air;
			shipment21Air.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			Assert("Air shipment's PackingMode is BreakBulk", !consol2.JK_Calc_PossibleOversize);
			var shipment22Air = consol2.Shipments.AddNew();
			shipment22Air.JS_TransportMode = Constants.TransportModes.Air;
			shipment22Air.JS_PackingMode = Constants.ContainerModes.Bulk;
			Assert("Air shipment's PackingMode is Bulk", !consol2.JK_Calc_PossibleOversize);
			var shipment23Air = consol2.Shipments.AddNew();
			shipment23Air.JS_TransportMode = Constants.TransportModes.Air;
			shipment23Air.JS_PackingMode = Constants.ContainerModes.Liquid;
			Assert("Air shipment's PackingMode is Liquid", !consol2.JK_Calc_PossibleOversize);
			var shipment24Air = consol2.Shipments.AddNew();
			shipment24Air.JS_TransportMode = Constants.TransportModes.Air;
			shipment24Air.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			Assert("Air shipment's PackingMode is RollOnRollOff", !consol2.JK_Calc_PossibleOversize);

			var consol3 = Factory.New<ForwardingModuleConsol>();
			var shipment31Sea = consol3.Shipments.AddNew();
			shipment31Sea.JS_TransportMode = Constants.TransportModes.Sea;
			shipment31Sea.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			Assert("Sea shipment's PackingMode is BreakBulk", !consol3.JK_Calc_PossibleOversize);
			var shipment32Sea = consol3.Shipments.AddNew();
			shipment32Sea.JS_TransportMode = Constants.TransportModes.Sea;
			shipment32Sea.JS_PackingMode = Constants.ContainerModes.Bulk;
			Assert("Sea shipment's PackingMode is Bulk", !consol3.JK_Calc_PossibleOversize);
			var shipment33Sea = consol3.Shipments.AddNew();
			shipment33Sea.JS_TransportMode = Constants.TransportModes.Sea;
			shipment33Sea.JS_PackingMode = Constants.ContainerModes.Liquid;
			Assert("Sea shipment's PackingMode is Liquid", !consol3.JK_Calc_PossibleOversize);
			var shipment34Sea = consol3.Shipments.AddNew();
			shipment34Sea.JS_TransportMode = Constants.TransportModes.Sea;
			shipment34Sea.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			Assert("Sea shipment's PackingMode is RollOnRollOff", !consol3.JK_Calc_PossibleOversize);

			var consol4 = Factory.New<ForwardingModuleConsol>();
			var shipment4Air1 = consol4.Shipments.AddNew();
			shipment4Air1.JS_TransportMode = Constants.TransportModes.Air;
			var pack41 = shipment4Air1.OuterPackLines.AddNew();
			pack41.JL_Length = 200m;
			pack41.JL_Width = 100m;
			pack41.JL_Height = 80m;
			pack41.JL_UnitOfDimension = Constants.Length.Centimetres;
			var shipments4Air2 = consol4.Shipments.AddNew();
			shipments4Air2.JS_TransportMode = Core.Constants.TransportModes.Air;
			var pack42 = shipments4Air2.OuterPackLines.AddNew();
			pack42.JL_Length = 150m;
			pack42.JL_Width = 150m;
			pack42.JL_Height = 300m;
			pack42.JL_UnitOfDimension = Core.Constants.Length.Centimetres;
			Assert(consol4.JK_Calc_PossibleOversize);

			var consol5 = Factory.New<ForwardingModuleConsol>();
			var shipment5Sea1 = consol5.Shipments.AddNew();
			shipment5Sea1.JS_TransportMode = Constants.TransportModes.Sea;
			var pack51 = shipment5Sea1.OuterPackLines.AddNew();
			pack51.JL_Length = 241m;
			pack51.JL_Width = 96m;
			pack51.JL_Height = 100m;
			pack51.JL_UnitOfDimension = Core.Constants.Length.Inches;
			Assert(consol5.JK_Calc_PossibleOversize);

			var consol6 = Factory.New<ForwardingModuleConsol>();
			var shipment6Sea1 = consol6.Shipments.AddNew();
			shipment6Sea1.JS_TransportMode = Constants.TransportModes.Sea;
			var pack61 = shipment5Sea1.OuterPackLines.AddNew();
			pack61.JL_Length = 240m;
			pack61.JL_Width = 96m;
			pack61.JL_Height = 102m;
			pack61.JL_UnitOfDimension = Core.Constants.Length.Inches;
			Assert(!consol6.JK_Calc_PossibleOversize);

			var consol7 = Factory.New<ForwardingModuleConsol>();
			var shipment7Air1 = consol7.Shipments.AddNew();
			shipment7Air1.JS_TransportMode = Constants.TransportModes.Air;
			var pack71 = shipment7Air1.OuterPackLines.AddNew();
			pack71.JL_Length = 300m;
			pack71.JL_Width = 200m;
			pack71.JL_Height = 160m;
			pack71.JL_UnitOfDimension = Constants.Length.Centimetres;
			Assert(!consol7.JK_Calc_PossibleOversize);

			var consol8 = Factory.New<ForwardingModuleConsol>();
			consol8.JK_MaximumAllowablePackageLength = 8;
			consol8.JK_MaximumAllowablePackageWidth = 4;
			consol8.JK_MaximumAllowablePackageHeight = 4;
			consol8.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;
			var shipment8Air1 = consol8.Shipments.AddNew();
			shipment8Air1.JS_TransportMode = Constants.TransportModes.Air;
			var pack81 = shipment8Air1.OuterPackLines.AddNew();
			pack81.JL_Length = 601m;
			pack81.JL_Width = 100m;
			pack81.JL_Height = 100m;
			pack81.JL_UnitOfDimension = Constants.Length.Centimetres;
			Assert(!consol8.JK_Calc_PossibleOversize);

			var consol9 = Factory.New<ForwardingModuleConsol>();
			consol9.JK_MaximumAllowablePackageLength = 6;
			consol9.JK_MaximumAllowablePackageWidth = 4;
			consol9.JK_MaximumAllowablePackageHeight = 4;
			consol9.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Metres;
			consol9.Shipments.Add(shipment8Air1);
			Assert(consol9.JK_Calc_PossibleOversize);

			Factory.Save();

			var consolFilter = (ModuleFlagsFilter)FilterStripBizO["Possible Oversize"];
			var results = new MainFormConsolCollection(Factory);
			consolFilter.IsActive = true;

			consolFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have 3 consols", 3, results.Count);
			Assert("Consol9 should be in collection.", results.Contains(consol9.PK));
			Assert("Consol5 should be in collection.", results.Contains(consol5.PK));
			Assert("Consol4 should be in collection.", results.Contains(consol4.PK));

			consolFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have 6 consols", 6, results.Count);
			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));
			Assert("Consol3 should be in collection.", results.Contains(consol3.PK));
			Assert("Consol6 should be in collection.", results.Contains(consol6.PK));
			Assert("Consol7 should be in collection.", results.Contains(consol7.PK));
			Assert("Consol8 should be in collection.", results.Contains(consol8.PK));
		}

		#endregion

		#region CTO Storage Start

		public void TestCTOStorageStartFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			var container1 = consol1.Containers.AddNew();
			container1.JC_ArrivalCTOStorageStartDate = new ZDateTime(2012, 12, 04, 09, 50, 00);

			var container2 = consol2.Containers.AddNew();
			container2.JC_ArrivalCTOStorageStartDate = new ZDateTime(2012, 12, 03, 05, 45, 00);

			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO["CTO Storage Start"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);

			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;

			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 01, 12, 00, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 08, 12, 00, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 fall in range", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 04, 08, 30, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 04, 23, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 will not fall in range", !results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 02, 11, 50, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 03, 18, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 will not fall in range", !results.Contains(consol1.PK));
			Assert("Consol2 will fall in range", results.Contains(consol2.PK));
		}

		#endregion

		#region Empty Return Req. By

		public void TestEmptyReturnReqByFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			var container1 = consol1.Containers.AddNew();
			container1.JC_EmptyReturnedBy = new ZDateTime(2012, 12, 04, 09, 50, 00);

			var container2 = consol2.Containers.AddNew();
			container2.JC_EmptyReturnedBy = new ZDateTime(2012, 12, 03, 05, 45, 00);

			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO["Empty Return Req. By"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);

			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;

			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 01, 12, 00, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 08, 12, 00, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 fall in range", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 04, 08, 30, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 04, 23, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 will not fall in range", !results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 02, 11, 50, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 03, 18, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 will not fall in range", !results.Contains(consol1.PK));
			Assert("Consol2 will fall in range", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Electronic Bill Of Lading

		public void TestElectronicBillOfLading_Status()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consol_OBR = Factory.New<ForwardingConsol>();
				AddBillStatusUpdatedEvent(consol_OBR, BillStatusUpdatedTypes.OriginalBillPublished);

				var consol_OBA = Factory.New<ForwardingConsol>();
				AddBillStatusUpdatedEvent(consol_OBA, BillStatusUpdatedTypes.AmendmentRequested);

				var consol_OBT = Factory.New<ForwardingConsol>();
				AddBillStatusUpdatedEvent(consol_OBT, BillStatusUpdatedTypes.OriginalBillTransferred);

				var consol_STP = Factory.New<ForwardingConsol>();
				AddBillStatusUpdatedEvent(consol_STP, BillStatusUpdatedTypes.SwitchedToPaper);

				var consol_SUR = Factory.New<ForwardingConsol>();
				AddBillStatusUpdatedEvent(consol_SUR, BillStatusUpdatedTypes.Surrendered);

				var consol = Factory.New<ForwardingConsol>();

				Factory.Save();

				var consolFilter = (ModuleTextFilter)FilterStripBizO["Electronic Bill Status"];
				consolFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				var results = new MainFormConsolCollection(Factory);

				consolFilter.IsActive = true;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' = OBR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' = OBA", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' = OBT", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' = STP", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' = SUR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' != OBR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' != OBA", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' != OBT", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' != STP", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' != SUR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith OBR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith OBA", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith OBT", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith STP", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith SUR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith OBR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith OBA", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith OBT", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith STP", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith SUR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains OBR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains OBA", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains OBT", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains STP", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains SUR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains OBR", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains OBA", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains OBT", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains STP", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.Surrendered;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains SUR", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' IsBlank", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' IsNotBlank", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				consolFilter.Property = "O";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith O", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				consolFilter.Property = "O";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith O", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				consolFilter.Property = "O";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains O", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				consolFilter.Property = "O";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains O", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				consolFilter.Property = "X";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' StartsWith X", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				consolFilter.Property = "X";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' DoesNotStartWith X", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				consolFilter.Property = "X";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' Contains X", () =>
				{
					Assert(!results.Contains(consol_OBR));
					Assert(!results.Contains(consol_OBA));
					Assert(!results.Contains(consol_OBT));
					Assert(!results.Contains(consol_STP));
					Assert(!results.Contains(consol_SUR));
				});

				consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				consolFilter.Property = "X";
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Status' NotContains X", () =>
				{
					Assert(results.Contains(consol_OBR));
					Assert(results.Contains(consol_OBA));
					Assert(results.Contains(consol_OBT));
					Assert(results.Contains(consol_STP));
					Assert(results.Contains(consol_SUR));
				});
			}
		}

		void AddBillStatusUpdatedEvent(ForwardingConsol consol, string type, bool isCancel = false)
		{
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, type)
			};

			var log = consol.Logs.AddNew(Events.BillStatusUpdated, eventParameters);

			if (isCancel)
			{
				log.Cancel();
			}

			Factory.Save();
			Thread.Sleep(1);
		}

		public void TestElectronicBillOfLading_Terms()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consol_NTR = Factory.New<ForwardingConsol>();
				consol_NTR.JK_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.NonTransferable;

				var consol_TRA = Factory.New<ForwardingConsol>();
				consol_TRA.JK_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;

				var consol = Factory.New<ForwardingConsol>();

				Factory.Save();

				var consolFilter = (ModuleTextFilter)FilterStripBizO["Electronic Bill Terms"];
				var results = new MainFormConsolCollection(Factory);

				consolFilter.IsActive = true;
				consolFilter.Property = Constants.BillOfLadingBillTerms.Codes.NonTransferable;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Terms' = NTR", () =>
				{
					Assert(results.Contains(consol_NTR));
					Assert(!results.Contains(consol_TRA));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = Constants.BillOfLadingBillTerms.Codes.Transferable;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Terms' = TRA", () =>
				{
					Assert(!results.Contains(consol_NTR));
					Assert(results.Contains(consol_TRA));
				});
			}
		}

		public void TestElectronicBillOfLading_Type()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consol_STR = Factory.New<ForwardingConsol>();
				consol_STR.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;

				var consol_TOR = Factory.New<ForwardingConsol>();
				consol_TOR.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;

				var consol_BLE = Factory.New<ForwardingConsol>();
				consol_BLE.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;

				var consol = Factory.New<ForwardingConsol>();

				Factory.Save();

				var consolFilter = (ModuleTextFilter)FilterStripBizO["Electronic Bill Type"];
				var results = new MainFormConsolCollection(Factory);

				consolFilter.IsActive = true;
				consolFilter.Property = Constants.BillOfLadingBillType.Codes.Straight;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Type' = STR", () =>
				{
					Assert(results.Contains(consol_STR));
					Assert(!results.Contains(consol_TOR));
					Assert(!results.Contains(consol_BLE));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = Constants.BillOfLadingBillType.Codes.ToOrder;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Type' = TOR", () =>
				{
					Assert(!results.Contains(consol_STR));
					Assert(results.Contains(consol_TOR));
					Assert(!results.Contains(consol_BLE));
				});

				consolFilter.IsActive = true;
				consolFilter.Property = Constants.BillOfLadingBillType.Codes.BlankEndorse;
				results.Load(FilterStripBizO.Filter);

				CombineAssertions("Filtering consolidations for 'Electronic Bill Type' = BLE", () =>
				{
					Assert(!results.Contains(consol_STR));
					Assert(!results.Contains(consol_TOR));
					Assert(results.Contains(consol_BLE));
				});
			}
		}

		#endregion

		#endregion

		#region Dates

		#region Date Tests

		public void TestATAFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ATA.Name, "ATA");
			TestDateTimeFilter(JobConsolTransportSchema.JW_ATA.Name, "ATA");
		}

		public void TestATDFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ATD.Name, "ATD");
			TestDateTimeFilter(JobConsolTransportSchema.JW_ATD.Name, "ATD");
		}

		public void TestETAFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ETA.Name, "ETA");
			TestDateTimeFilter(JobConsolTransportSchema.JW_ETA.Name, "ETA");
		}

		public void TestETDFilter()
		{
			TestDateFilter(JobConsolTransportSchema.JW_ETD.Name, "ETD");
			TestDateTimeFilter(JobConsolTransportSchema.JW_ETD.Name, "ETD");
		}

		public void TestETDLoadFilter()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ETD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, "ETD / Load Port");
			TestDateTimeLocationFilter(JobConsolTransportSchema.JW_ETD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, "ETD / Load Port");
		}

		public void TestATDLoadFilter()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ATD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, "ATD / Load Port");
			TestDateTimeLocationFilter(JobConsolTransportSchema.JW_ATD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, "ATD / Load Port");
		}

		public void TestETADischargeFilter()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ETA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, "ETA / Discharge Port");
			TestDateTimeLocationFilter(JobConsolTransportSchema.JW_ETA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, "ETA / Discharge Port");
		}

		public void TestATADischargeFilter()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ATA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, "ATA / Discharge Port");
			TestDateTimeLocationFilter(JobConsolTransportSchema.JW_ATA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, "ATA / Discharge Port");
		}

		public void TestETALoadFilter()
		{
			TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(Transport.Schema.JW_JX_Load_ETA, "ETA / Load Port");
		}

		public void TestATALoadFilter()
		{
			TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(Transport.Schema.JW_JX_Load_ATA, "ATA / Load Port");
		}

		public void TestConsolCutOffDateFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			consol1.JK_ConsolCutOffDate = new ZDateTime(2012, 12, 04, 09, 50, 00);
			consol2.JK_ConsolCutOffDate = new ZDateTime(2012, 12, 03, 05, 45, 00);

			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO["Cut Off Date"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);

			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;

			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 01, 12, 00, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 08, 12, 00, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 fall in range", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 04, 08, 30, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 04, 23, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Consol2 will not fall in range", !results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2012, 12, 02, 11, 50, 00);
			consolFilter.Property2 = new ZDateTime(2012, 12, 03, 18, 10, 00);

			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 will not fall in range", !results.Contains(consol1.PK));
			Assert("Consol2 will fall in range", results.Contains(consol2.PK));
		}

		public void TestCutoffDateFilterUseConsideringDateOnly()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			consol1.JK_ConsolCutOffDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2013, 02, 21, 16, 40, 00).ToDateTime());
			consol2.JK_ConsolCutOffDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2013, 02, 21, 16, 47, 00).ToDateTime());
			consol3.JK_ConsolCutOffDate = Env.Time.GetUtcFromLocalTime(new ZDateTime(2013, 02, 21, 17, 40, 00).ToDateTime());

			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO["Cut Off Date"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			consolFilter.Property1 = new ZDateTime(2013, 02, 21, 00, 00, 00);
			consolFilter.Property2 = new ZDateTime(2013, 02, 21, 23, 59, 59);

			results.Load(FilterStripBizO.Filter);
			Assert("Same Date - Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Same Date - Consol2 fall in range", results.Contains(consol2.PK));
			Assert("Same Date - Consol3 fall in range", results.Contains(consol3.PK));

			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			consolFilter.Property1 = new ZDateTime(2013, 02, 21, 16, 00, 00);
			consolFilter.Property2 = new ZDateTime(2013, 02, 21, 17, 00, 00);

			results.Load(filterStripBizO.Filter);

			Assert("Date Time - Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Date Time - Consol2 fall in range", results.Contains(consol2.PK));
			Assert("Date Time - Consol3 will not fall in range", !results.Contains(consol3.PK));

			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			results.Load(filterStripBizO.Filter);

			Assert("Date - Consol1 fall in range", results.Contains(consol1.PK));
			Assert("Date - Consol2 fall in range", results.Contains(consol2.PK));
			Assert("Date - Consol3 fall in range", results.Contains(consol3.PK));
		}

		public void TestConsolCutOffDateFilterUsesLocalTime()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();

			consol1.JK_ConsolCutOffDate = ZDateTime.Today;
			consol2.JK_ConsolCutOffDate = ZDateTime.Today.AddDays(-1);

			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO["Cut Off Date"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			results.Load(FilterStripBizO.Filter);

			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			results.Load(FilterStripBizO.Filter);

			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));
		}

		#region TestDateOrganizationFilter

		public void TestArrivalCFS_ReceiptRequested()
		{
			TestDateOrganizationFilter(JobConsolFilterBusinessObject.Descriptions.ArrivalCFSReceiptRequested);
		}

		public void TestDepartureCFS_ReceiptRequested()
		{
			TestDateOrganizationFilter(JobConsolFilterBusinessObject.Descriptions.DepartureCFSReceiptRequested);
		}

		public void TestArrivalCFS_DispatchRequested()
		{
			TestDateOrganizationFilter(JobConsolFilterBusinessObject.Descriptions.ArrivalCFSDispatchRequested);
		}

		public void TestDepartureCFS_DispatchRequested()
		{
			TestDateOrganizationFilter(JobConsolFilterBusinessObject.Descriptions.DepartureCFSDispatchRequested);
		}

		public void TestDateOrganizationFilter(ZString filterStripDescription)
		{
			var filterToColumnMappings = new Dictionary<ZString, (SchemaGuidColumn, SchemaDateTimeColumn)>()
			{
				{
					JobConsolFilterBusinessObject.Descriptions.ArrivalCFSReceiptRequested,
					(JobConsolSchema.JK_OA_UnpackDepotAddress, JobConsolSchema.JK_UnpackDepotReceiptRequested)
				},
				{
					JobConsolFilterBusinessObject.Descriptions.DepartureCFSReceiptRequested,
					(JobConsolSchema.JK_OA_PackDepotAddress, JobConsolSchema.JK_PackDepotReceiptRequested)
				},
				{
					JobConsolFilterBusinessObject.Descriptions.ArrivalCFSDispatchRequested,
					(JobConsolSchema.JK_OA_UnpackDepotAddress, JobConsolSchema.JK_UnpackDepotDispatchRequested)
				},
				{
					JobConsolFilterBusinessObject.Descriptions.DepartureCFSDispatchRequested,
					(JobConsolSchema.JK_OA_PackDepotAddress, JobConsolSchema.JK_PackDepotDispatchRequested)
				}
			};

			var today = ZDateTime.Today;
			var expectedConsol = NewConsol("target-consol", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");

			var cfs = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = cfs.Addresses.AddNew();
			address1.Address1 = "Some depot address";

			var someOtherCFS = Factory.NewWithValidTestData<OrgHeader>();
			var someOtherAddress = someOtherCFS.Addresses.AddNew();
			someOtherAddress.Address1 = "Some other depot address";

			var (targetDepot, targetDate) = filterToColumnMappings[filterStripDescription];

			expectedConsol[targetDepot] = address1.PK;
			expectedConsol[targetDate] = today.AddDays(-2);

			var shouldNotBeInResults = new List<CommonConsol>();
			var consolCounter = 0;

			foreach (var (depot, date) in filterToColumnMappings.Where(mapping => mapping.Key != filterStripDescription).Select(mapping => mapping.Value))
			{
				var consol = NewConsol($"consol-{consolCounter}", false, "USNYC", "SGSIN", "AUSYD");
				consol[depot] = address1.PK;
				consol[date] = today.AddDays(-2);

				shouldNotBeInResults.Add(consol);
				consolCounter++;
			}

			Factory.Save();

			var filter = FilterStripBizO[filterStripDescription] as DateOrganizationFilter;
			filter.IsActive = true;

			var results = new MainFormConsolCollection(Factory);
			results.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(
				"Pre-condition: Search is not filtered - should return all results",
				new[] { expectedConsol }.Concat(shouldNotBeInResults),
				results
			);

			filter.OrganizationPK = someOtherCFS.PK;
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder("No consols should be returned: wrong cfs", new MainFormConsolCollection(Factory), results);

			filter.OrganizationPK = cfs.PK;
			filter.Property1 = today.AddDays(-2);
			filter.Property2 = today.AddDays(-2);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			results.Load(filter.Query);

			AssertCollectionContains("Should appear: matches CFS and date", expectedConsol, results);
			AssertCollectionNotContains("Should not appear: does not match specific CFS", shouldNotBeInResults, results);
		}

		#endregion

		#endregion

		#region Common Part

		void TestDateFilter(ZString dateProperty, ZString filterProperty)
		{
			ForwardingConsol consol1 = NewConsol("tedf1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			ForwardingConsol consol2 = NewConsol("tedf2", false, "USNYC", "SGSIN", "AUSYD");
			consol1.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1);
			consol1.Transports[1][dateProperty] = new ZDateTime(2007, 1, 3);
			consol1.Transports[2][dateProperty] = new ZDateTime(2007, 1, 5);

			consol2.Transports[0][dateProperty] = new ZDateTime(2007, 1, 2);
			consol2.Transports[1][dateProperty] = new ZDateTime(2007, 1, 8);
			Factory.Save();

			ModuleDateFilter consolFilter = (ModuleDateFilter)FilterStripBizO[filterProperty];

			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be in collection", results.Contains(consol1.PK));
			Assert("Not filtered - should be in collection", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1);
			consolFilter.Property2 = new ZDateTime(2007, 1, 2);
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " falls in range", results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " falls in range", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 6);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " not falls in range", !results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not falls in range", !results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 5);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " falls in range", results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not falls in range", !results.Contains(consol2.PK));
		}

		void TestDateTimeFilter(ZString dateProperty, ZString filterProperty)
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");
			consol1.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1, 9, 0, 0);
			consol1.Transports[1][dateProperty] = new ZDateTime(2007, 1, 1, 10, 0, 0);
			consol1.Transports[2][dateProperty] = new ZDateTime(2007, 1, 1, 11, 0, 0);

			consol2.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1, 13, 0, 0);
			consol2.Transports[1][dateProperty] = new ZDateTime(2007, 1, 1, 17, 0, 0);
			Factory.Save();

			var consolFilter = (ModuleDateFilter)FilterStripBizO[filterProperty];
			var filterResults = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be in collection", filterResults.Contains(consol1.PK));
			Assert("Not filtered - should be in collection", filterResults.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 0, 0, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 23, 59, 59);
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " fall in range", filterResults.Contains(consol2.PK));

			var scheme = Factory.New<GridColourScheme>();
			var gridColourStripBusinessObject = new GridColourStripBusinessObject(FilterStripBizO, scheme, null);
			var gridColourModuleFilters = gridColourStripBusinessObject.ModuleFilters;

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 11, 30, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 12, 30, 0);
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " not fall in range", !filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !filterResults.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 11, 0, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 12, 30, 0);
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !filterResults.Contains(consol2.PK));
		}

		void TestDateLocationFilter(ZString dateProperty, ZString locationProperty, ZString filterProperty)
		{
			ForwardingConsol consol1 = NewConsol("tedf1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			ForwardingConsol consol2 = NewConsol("tedf2", false, "USNYC", "SGSIN", "AUSYD");
			consol1.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1);
			consol1.Transports[0][locationProperty] = "AUSYD";
			consol1.Transports[1][dateProperty] = new ZDateTime(2007, 1, 3);
			consol1.Transports[1][locationProperty] = "HKHKG";
			consol1.Transports[2][dateProperty] = new ZDateTime(2007, 1, 5);
			consol1.Transports[2][locationProperty] = "USLAX";

			consol2.Transports[0][dateProperty] = new ZDateTime(2007, 1, 2);
			consol2.Transports[0][locationProperty] = "AUSYD";
			consol2.Transports[1][dateProperty] = new ZDateTime(2007, 1, 8);
			consol2.Transports[1][locationProperty] = "DEBRE";

			Factory.Save();

			DateLocationFilter consolFilter = (DateLocationFilter)FilterStripBizO[filterProperty];

			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;
			consolFilter.Property3 = ZString.Empty;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be in collection", results.Contains(consol1.PK));
			Assert("Not filtered - should be in collection", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1);
			consolFilter.Property2 = new ZDateTime(2007, 1, 2);
			consolFilter.Property3 = "AUSYD";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " fall in range", results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 6);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			consolFilter.Property3 = "AUMEL";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " not fall in range", !results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !results.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 5);
			consolFilter.Property2 = new ZDateTime(2007, 1, 7);
			consolFilter.Property3 = "USLAX";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", results.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !results.Contains(consol2.PK));
		}

		void TestDateTimeLocationFilter(ZString dateProperty, ZString locationProperty, ZString filterProperty)
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");
			consol1.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1, 9, 0, 0);
			consol1.Transports[0][locationProperty] = "AUSYD";
			consol1.Transports[1][dateProperty] = new ZDateTime(2007, 1, 1, 10, 0, 0);
			consol1.Transports[1][locationProperty] = "HKHKG";
			consol1.Transports[2][dateProperty] = new ZDateTime(2007, 1, 1, 11, 0, 0);
			consol1.Transports[2][locationProperty] = "USLAX";

			consol2.Transports[0][dateProperty] = new ZDateTime(2007, 1, 1, 13, 0, 0);
			consol2.Transports[0][locationProperty] = "AUSYD";
			consol2.Transports[1][dateProperty] = new ZDateTime(2007, 1, 1, 17, 0, 0);
			consol2.Transports[1][locationProperty] = "DEBRE";
			Factory.Save();

			var consolFilter = (DateLocationFilter)FilterStripBizO[filterProperty];
			var filterResults = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			consolFilter.Property1 = ZDateTime.Empty;
			consolFilter.Property2 = ZDateTime.Empty;
			consolFilter.Property3 = ZString.Empty;
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be in collection", filterResults.Contains(consol1.PK));
			Assert("Not filtered - should be in collection", filterResults.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 0, 0, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 23, 59, 59);
			consolFilter.Property3 = "AUSYD";
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " fall in range", filterResults.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 11, 30, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 12, 30, 0);
			consolFilter.Property3 = "AUMEL";
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " not fall in range", !filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !filterResults.Contains(consol2.PK));

			consolFilter.Property1 = new ZDateTime(2007, 1, 1, 11, 0, 0);
			consolFilter.Property2 = new ZDateTime(2007, 1, 1, 12, 30, 0);
			consolFilter.Property3 = "USLAX";
			filterResults.Load(FilterStripBizO.Filter);
			Assert("Consol1 " + filterProperty + " fall in range", filterResults.Contains(consol1.PK));
			Assert("Consol2 " + filterProperty + " not fall in range", !filterResults.Contains(consol2.PK));
		}

		public void TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(ZString dateProperty, ZString filterProperty)
		{
			var today = ZDateTime.Today;

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage1.Origins[0].JA_A_ARV = today;
			voyage1.Origins[0].JA_E_ARV = today;
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.GenerateSailings();

			var voyage3 = Factory.New<JobVoyage>();
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "GBLON";
			voyage3.Origins[0].JA_A_ARV = today;
			voyage3.Origins[0].JA_E_ARV = today;
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage3.GenerateSailings();

			var consol1 = Factory.New<CommonConsol>();
			var consol2 = Factory.New<CommonConsol>();
			var consol3 = Factory.New<CommonConsol>();
			var consol4 = Factory.New<CommonConsol>();
			var consol5 = Factory.New<CommonConsol>();
			consol1.Transports[0].JW_JX = voyage1.Sailings[0].PK;
			consol2.Transports[0].JW_JX = voyage2.Sailings[0].PK;
			consol3.Transports[0].JW_JX = voyage3.Sailings[0].PK;
			consol4.Transports[0].JW_JX = voyage1.Sailings[0].PK;
			consol1.Transports[0].JW_IsLinked = true;
			consol2.Transports[0].JW_IsLinked = true;
			consol3.Transports[0].JW_IsLinked = true;
			consol4.Transports[0].JW_IsLinked = true;

			Factory.Save();

			consol4.Transports[0].JW_IsLinked = false;
			consol4.Transports[0][dateProperty] = ZDateTime.Empty;

			Factory.Save();

			var filter = (DateLocationFilter)FilterStripBizO[filterProperty];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.Property3 = ZString.Empty;
			filter.IsActive = true;

			var results = new MainFormConsolCollection(Factory);

			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Pre-condition: Should return all shipments when filter is blank", new[] { consol1, consol2, consol3, consol4, consol5 }, results);

			filter.Property3 = "AUMEL";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return all 3 consols that are loaded from Melbourne", new[] { consol1, consol2, consol4 }, results);

			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today.AddDays(1);
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return consol loaded in Melbourne with a date", new[] { consol1 }, results);

			filter.Property3 = ZString.Empty;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should return 2 matching consols where the transports are linked, regardless of load port", new[] { consol1, consol3 }, results);

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 3 consols with no date on a transport or has no linked transport", new[] { consol2, consol4, consol5 }, results);

			filter.Property3 = "AUMEL";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 2 consols with no date and loads in Melbourne", new[] { consol2, consol4 }, results);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return a consol with an ETA and from the same port", new[] { consol1 }, results);

			filter.Property3 = ZString.Empty;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 2 consols with an ETA entered", new[] { consol1, consol3 }, results);
		}

		#endregion

		#endregion

		#region Locations

		#region TestEndPortsLoadDischargeFilter

		public void TestEndPortsLoadDischargeFilter()
		{
			ForwardingConsol consol1 = NewConsol("tepldf1", false, "AUBNE", "SGSIN", "USNYC", "USLAX");
			ForwardingConsol consol2 = NewConsol("tepldf2", false, "SGSIN", "USNYC");

			Factory.Save();

			ModuleLocationFilter consolFilter = (ModuleLocationFilter)FilterStripBizO["End Ports (First Load / Last Disch.)"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - should be included", true, results.Contains(consol1));
			AssertEquals("Not filtered - should be included", true, results.Contains(consol2));

			consolFilter.Property1 = "AU";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1));
			AssertEquals("Consol2 has nothing to do with brisbane", false, results.Contains(consol2));

			consolFilter.Property1 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 has a transport loading here but the consol it's self loads elsewere.", false, results.Contains(consol1));
			AssertEquals("Consol2 should be in collection.", true, results.Contains(consol2));

			consolFilter.Property1 = "";
			consolFilter.Property2 = "USNYC";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 has a transport discharge here but the consol it's self discharges elsewere.", false, results.Contains(consol1));
			AssertEquals("Consol2 should be in collection.", true, results.Contains(consol2));

			consolFilter.Property2 = "USLAX";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1));
			AssertEquals("Consol2 is going to another port", false, results.Contains(consol2));

			consolFilter.Property2 = "US";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in collection", true, results.Contains(consol1));
			AssertEquals("Consol2 should be in collection", true, results.Contains(consol2));

			consolFilter.Property1 = "AUSR";
			consolFilter.Property2 = "";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 loads in the AUSR Zone", true, results.Contains(consol1));
			AssertEquals("Consol2 doesn't load in the AUSR Zone", false, results.Contains(consol2));
		}

		#endregion

		#region TestLoadDischargePortFilter

		public void TestLoadDischargePortFilter()
		{
			ForwardingConsol consol1 = NewConsol("tldpf1", false, "USLAX", "SGSIN", "NZAKL", "AUMEL");

			ForwardingConsol consol2 = NewConsol("tldpf2", false, "USNYC", "SGSIN", "NZAKL");
			Factory.Save();

			ModuleLocationFilter consolFilter = (ModuleLocationFilter)FilterStripBizO["Load / Discharge"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(consol1.PK));
			Assert("Not filtered - should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should be in collection.", results.Contains(consol2.PK));

			consolFilter.Property1 = "NZ";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol should be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 isn't loading anywhere in NZ", !results.Contains(consol2.PK));

			consolFilter.Property1 = "";
			consolFilter.Property2 = "NZAKL";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should  be in collection.", results.Contains(consol1.PK));
			Assert("Consol2 should  be in collection.", results.Contains(consol2.PK));

			consolFilter.Property2 = "US";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 isn't discharging in US", !results.Contains(consol1.PK));
			Assert("Consol2 isn't discharging in US", !results.Contains(consol2.PK));
		}

		#endregion

		#region TestOriginDestinationFilter

		public void TestOriginDestinationFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "AUBNE";
			shipment2.JS_RL_NKDestination = "SGSIN";

			Factory.Save();

			ModuleLocationFilter consolFilter = (ModuleLocationFilter)FilterStripBizO["Origin / Destination"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - should be included", true, results.Contains(consol1.PK));
			AssertEquals("Not filtered - should be included", true, results.Contains(consol2.PK));

			consolFilter.Property1 = "AU";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol2.PK));

			consolFilter.Property1 = "AUBNE";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 has nothing to do with brisbane", false, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));

			consolFilter.Property1 = "";
			consolFilter.Property2 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 isn't going to Singapore", false, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in collection", true, results.Contains(consol2.PK));

			consolFilter.Property2 = "US";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 isn't going to US", false, results.Contains(consol2.PK));
		}

		#endregion

		#region Test CarrierBookingOfficeFilter

		public void TestCarrierBookingOfficeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.OA_RL_NKRelatedPortCode = "CNBJS";

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.OA_RL_NKRelatedPortCode = "AUSYD";

			var cnbjsConsol = Factory.New<ForwardingConsol>();
			cnbjsConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			cnbjsConsol.JK_AgentType = Constants.AgentType.CoLoad;
			cnbjsConsol.JK_OA_CreditorAddress = org1.PK;

			var ausydConsol = Factory.New<ForwardingConsol>();
			ausydConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			ausydConsol.JK_AgentType = Constants.AgentType.Agent;
			ausydConsol.JK_OA_ShippingLineAddress = org2.PK;

			var seaConsol = Factory.New<ForwardingConsol>();
			seaConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var airConsol = Factory.New<ForwardingConsol>();
			airConsol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("CNBJS", cnbjsConsol.JK_RL_NKCarrierBookingOffice);
			AssertEquals("AUSYD", ausydConsol.JK_RL_NKCarrierBookingOffice);
			AssertEquals(ZString.Empty, seaConsol.JK_RL_NKCarrierBookingOffice);
			AssertEquals(ZString.Empty, airConsol.JK_RL_NKCarrierBookingOffice);

			Factory.Save();

			var consolFilter = (ModuleGuidFilter)FilterStripBizO["Carrier Booking Office"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Not filtered - should be included", true, results.Contains(cnbjsConsol.PK));
				AssertEquals("Not filtered - should be included", true, results.Contains(ausydConsol.PK));
				AssertEquals("Not filtered - should be included", true, results.Contains(seaConsol.PK));
				AssertEquals("Not filtered - should be included", true, results.Contains(airConsol.PK));
			});

			consolFilter.Property = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USLAX")).PK;
			consolFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("USLAX Equal", false, results.Contains(cnbjsConsol.PK));
				AssertEquals("USLAX Equal", false, results.Contains(ausydConsol.PK));
				AssertEquals("USLAX Equal", false, results.Contains(seaConsol.PK));
				AssertEquals("USLAX Equal", false, results.Contains(airConsol.PK));
			});

			consolFilter.Property = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "CNBJS")).PK;
			consolFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("CNBJS Equal", true, results.Contains(cnbjsConsol.PK));
				AssertEquals("CNBJS Equal", false, results.Contains(ausydConsol.PK));
				AssertEquals("CNBJS Equal", false, results.Contains(seaConsol.PK));
				AssertEquals("CNBJS Equal", false, results.Contains(airConsol.PK));
			});

			consolFilter.Property = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).PK;
			consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("AUSYD NotEqual", true, results.Contains(cnbjsConsol.PK));
				AssertEquals("AUSYD NotEqual", false, results.Contains(ausydConsol.PK));
				AssertEquals("AUSYD NotEqual", true, results.Contains(seaConsol.PK));
				AssertEquals("AUSYD NotEqual", false, results.Contains(airConsol.PK));
			});

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("IsBlank", false, results.Contains(cnbjsConsol.PK));
				AssertEquals("IsBlank", false, results.Contains(ausydConsol.PK));
				AssertEquals("IsBlank", true, results.Contains(seaConsol.PK));
				AssertEquals("IsBlank", false, results.Contains(airConsol.PK));
			});

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("IsNotBlank", true, results.Contains(cnbjsConsol.PK));
				AssertEquals("IsNotBlank", true, results.Contains(ausydConsol.PK));
				AssertEquals("IsNotBlank", false, results.Contains(seaConsol.PK));
				AssertEquals("IsNotBlank", false, results.Contains(airConsol.PK));
			});
		}

		#endregion

		#endregion

		#region Organisations

		#region Test Carrier Filter

		public void TestCarrierFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			consol1.JK_OA_ShippingLineAddress = org1.MainAddress.PK;

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			consol2.JK_OA_ShippingLineAddress = org2.MainAddress.PK;

			var consol3 = Factory.New<ForwardingConsol>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;
			consol3.JK_OA_ShippingLineAddress = org3.MainAddress.PK;
			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO["Carrier"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol1 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol1 should be included", results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert("Consol3 should be included", results.Contains(consol3.PK));
		}

		#endregion

		#region TestCSTerminFilter

		public void TestCSTerminFilter()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			AssertBizOFilterText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertBizOFilterText("TestTestTest");
		}

		void AssertBizOFilterText(string expectedValue)
		{
			var filter = new JobConsolFilterBusinessObject();
			AssertNotNull("FilterCollection must be contained filter with name \"" + expectedValue + " / Consignee\"", filter[expectedValue + " / Consignee"]);
		}

		#endregion

		#region Test Consignor Consignee Filter

		[ExpectNoExceptions]
		public void TestConsignorConsigneeFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsActive = false;

			var consol1 = Factory.New<ForwardingConsol>();
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.ConsignorPK = org1.PK;
			shipment1.ConsigneePK = org2.PK;

			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.ConsignorPK = org2.PK;
			shipment2.ConsigneePK = org1.PK;

			var consol3 = Factory.New<ForwardingConsol>();
			var shipment3 = consol3.Shipments.AddNew();
			shipment3.ConsignorPK = org3.PK;
			shipment3.ConsigneePK = org4.PK;

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, " ");

			Factory.Save();

			var consolFilter = (ModuleGuidsFilterForOrg)FilterStripBizO[FreightDataRegistry.Instance.ConsignorShipperTerminology.Value + " / Consignee"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property1 = org1.PK;
			consolFilter.Property2 = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property1 = org2.PK;
			consolFilter.Property2 = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = org2.PK;
			consolFilter.Property2 = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.Property1Info, "Organization is in-active.");
			AssertNoWarning(consolFilter.Property2Info, "Organization is in-active.");
			consolFilter.Property1 = org3.PK;
			consolFilter.Property2 = org4.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.Property1Info, "Organization is in-active.");
			AssertHasWarning(consolFilter.Property2Info, "Organization is in-active.");
			Assert("Consol3 should be included", results.Contains(consol3.PK));

			var bindingLists = BindToLists.GetCachedLists(FilterStripBizO.Factory);
			AssertEquals("consignor lookup is correct", bindingLists.OrgConsignor_FilterList, consolFilter.List1);
			AssertEquals("consignee lookup is correct", bindingLists.OrgConsignee_FilterList, consolFilter.List2);
		}

		#endregion

		#region Test Send Receive Agent Filter

		public void TestSendReceiveAgentFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_IsActive = false;

			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol1.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_OA_SendingForwarderAddress = org2.MainAddress.PK;
			consol2.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_OA_SendingForwarderAddress = org3.MainAddress.PK;
			consol3.JK_OA_ReceivingForwarderAddress = org4.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidsFilterForOrg)FilterStripBizO["Send / Receive Agents"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property1 = org1.PK;
			consolFilter.Property2 = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property1 = org2.PK;
			consolFilter.Property2 = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));

			consolFilter.Property1 = org2.PK;
			consolFilter.Property2 = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.Property1Info, "Organization is in-active.");
			AssertNoWarning(consolFilter.Property2Info, "Organization is in-active.");
			consolFilter.Property1 = org3.PK;
			consolFilter.Property2 = org4.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.Property1Info, "Organization is in-active.");
			AssertHasWarning(consolFilter.Property2Info, "Organization is in-active.");
			Assert("Consol3 should be included", results.Contains(consol3.PK));
		}

		#endregion

		#region Test Related Parties Filters

		public void TestSendingAgentRelatedPartiesFilter()
		{
			var consol1 = GetConsolWithSendingAgent("con1");
			var consol2 = GetConsolWithSendingAgent("con2");
			var emptyConsol = GetConsol("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(consol1.SendingForwarder, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(consol1.SendingForwarder, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(consol2.SendingForwarder, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(consol2.SendingForwarder, party3);

			var newConsol1 = GetConsolWithSendingAgent("newClient1");
			var newConsol2 = GetConsolWithSendingAgent("newClient2");
			var newConsol3 = GetConsolWithSendingAgent("newClient3");
			var newConsol4 = GetConsolWithSendingAgent("newClient4");
			var newConsol5 = GetConsolWithSendingAgent("newClient5");
			var newConsol6 = GetConsolWithSendingAgent("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newConsol1.SendingForwarder, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newConsol2.SendingForwarder, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newConsol3.SendingForwarder, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newConsol4.SendingForwarder, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newConsol5.SendingForwarder, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newConsol6.SendingForwarder, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			Asserter.AddFieldOfInterest("JK_MasterBillNum");

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Sending Agent Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, consol1, consol2, emptyConsol, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, consol1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, consol1, consol2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, consol2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newConsol1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newConsol3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newConsol4, newConsol5, newConsol6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TranaportMode=SEA, ContainerMode=FCL", filter, newConsol5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TranaportMode=SEA, ContainerMode=LCL", filter, newConsol6);
		}

		public void TestReceivingAgentRelatedPartiesFilter()
		{
			var consol1 = GetConsolWithReceivingAgent("con1");
			var consol2 = GetConsolWithReceivingAgent("con2");
			var emptyConsol = GetConsol("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(consol1.ReceivingForwarder, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(consol1.ReceivingForwarder, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(consol2.ReceivingForwarder, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(consol2.ReceivingForwarder, party3);

			var newConsol1 = GetConsolWithReceivingAgent("newClient1");
			var newConsol2 = GetConsolWithReceivingAgent("newClient2");
			var newConsol3 = GetConsolWithReceivingAgent("newClient3");
			var newConsol4 = GetConsolWithReceivingAgent("newClient4");
			var newConsol5 = GetConsolWithReceivingAgent("newClient5");
			var newConsol6 = GetConsolWithReceivingAgent("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newConsol1.ReceivingForwarder, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newConsol2.ReceivingForwarder, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newConsol3.ReceivingForwarder, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newConsol4.ReceivingForwarder, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newConsol5.ReceivingForwarder, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newConsol6.ReceivingForwarder, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			Asserter.AddFieldOfInterest("JK_MasterBillNum");

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Receiving Agent Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, consol1, consol2, emptyConsol, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, consol1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, consol1, consol2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, consol2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newConsol1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newConsol3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newConsol4, newConsol5, newConsol6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newConsol5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newConsol6);
		}

		public void TestCarrierRelatedPartiesFilter()
		{
			var consol1 = GetConsolWithCarrier("client1");
			var consol2 = GetConsolWithCarrier("client2");
			var emptyConsol = GetConsol("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(consol1.ShippingLine, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(consol1.ShippingLine, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(consol2.ShippingLine, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(consol2.ShippingLine, party3);

			var newConsol1 = GetConsolWithCarrier("newClient1");
			var newConsol2 = GetConsolWithCarrier("newClient2");
			var newConsol3 = GetConsolWithCarrier("newClient3");
			var newConsol4 = GetConsolWithCarrier("newClient4");
			var newConsol5 = GetConsolWithCarrier("newClient5");
			var newConsol6 = GetConsolWithCarrier("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newConsol1.ShippingLine, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newConsol2.ShippingLine, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newConsol3.ShippingLine, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newConsol4.ShippingLine, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newConsol5.ShippingLine, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newConsol6.ShippingLine, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			Asserter.AddFieldOfInterest("JK_MasterBillNum");

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO["Carrier Related Parties"];

			Asserter.AssertMatches("Empty Filter", filter, consol1, consol2, emptyConsol, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, consol1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, consol1, consol2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, consol2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newConsol1, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newConsol1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newConsol2, newConsol3, newConsol4, newConsol5, newConsol6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newConsol3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newConsol4, newConsol5, newConsol6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newConsol5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newConsol6);
		}

		#endregion

		public void TestDepartureCTOFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_DepartureCTOAddress = org1.MainAddress.PK;
			consol2.JK_OA_DepartureCTOAddress = org2.MainAddress.PK;
			consol3.JK_OA_DepartureCTOAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.DepartureCTO];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestDepartureCFSFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_PackDepotAddress = org1.MainAddress.PK;
			consol2.JK_OA_PackDepotAddress = org2.MainAddress.PK;
			consol3.JK_OA_PackDepotAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.DepartureCFS];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestDepartureContainerYardFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_ContainerYardEmptyPickupAddress = org1.MainAddress.PK;
			consol2.JK_OA_ContainerYardEmptyPickupAddress = org2.MainAddress.PK;
			consol3.JK_OA_ContainerYardEmptyPickupAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.DepartureContainerYard];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestDepartureTransportProviderFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_DeparturePackCFSTransportAddress = org1.MainAddress.PK;
			consol2.JK_OA_DeparturePackCFSTransportAddress = org2.MainAddress.PK;
			consol3.JK_OA_DeparturePackCFSTransportAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.DepartureTransportProvider];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestArrivalCTOFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_ArrivalCTOAddress = org1.MainAddress.PK;
			consol2.JK_OA_ArrivalCTOAddress = org2.MainAddress.PK;
			consol3.JK_OA_ArrivalCTOAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilterForOrg)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.ArrivalCTO];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestArrivalCFSFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_UnpackDepotAddress = org1.MainAddress.PK;
			consol2.JK_OA_UnpackDepotAddress = org2.MainAddress.PK;
			consol3.JK_OA_UnpackDepotAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.ArrivalCFS];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestArrivalContainerYardFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_ContainerYardEmptyReturnAddress = org1.MainAddress.PK;
			consol2.JK_OA_ContainerYardEmptyReturnAddress = org2.MainAddress.PK;
			consol3.JK_OA_ContainerYardEmptyReturnAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.ArrivalContainerYard];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		public void TestArrivalTransportProviderFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			consol1.JK_OA_ArrivalUnpackCFSTransportAddress = org1.MainAddress.PK;
			consol2.JK_OA_ArrivalUnpackCFSTransportAddress = org2.MainAddress.PK;
			consol3.JK_OA_ArrivalUnpackCFSTransportAddress = org3.MainAddress.PK;

			Factory.Save();

			var consolFilter = (ModuleGuidFilter)FilterStripBizO[JobConsolFilterBusinessObject.Descriptions.ArrivalTransportProvider];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			consolFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(consol1.PK));
			Assert(!results.Contains(consol2.PK));

			consolFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			Assert(!results.Contains(consol1.PK));
			Assert(results.Contains(consol2.PK));

			AssertNoWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			consolFilter.Property = org3.PK;
			results.Load(FilterStripBizO.Filter);
			AssertHasWarning(consolFilter.PropertyInfo, "Organization is in-active.");
			Assert(results.Contains(consol3.PK));
		}

		#endregion

		#region Modes and Types

		#region Test Container Mode Filter

		public void TestContainerModeFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_ConsolMode = "FCL";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_ConsolMode = "LCL";
			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Container Mode"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = "FCL";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = "LCL";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Container Type Filter

		public void TestContainerTypeFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol1.Containers.AddNew();
			consol1.JK_TransportMode = "SEA";
			container1.JC_ContainerNum = "AAA";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingContainer container2 = consol2.Containers.AddNew();
			consol2.JK_TransportMode = "SEA";
			container2.JC_ContainerNum = "AAB";
			container2.JC_RC = container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			Factory.Save();

			ModuleGuidFilter consolFilter = (ModuleGuidFilter)FilterStripBizO["Container Type"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Transport Mode Filter

		public void TestTransportModeFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = "AIR";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = "SEA";
			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Transport Mode"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = "AIR";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = "SEA";
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}

		#endregion

		#region Service Level

		public void TestServiceLevelFilterList()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			OrgCarrierServiceLevel carrier1Lvl1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrier1Lvl1.PL_Code = "ABC";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgCarrierServiceLevel carrier2Lvl1 = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrier2Lvl1.PL_Code = "DEF";

			Factory.Save();

			ModuleTextFilter svcLvlFilter = (ModuleTextFilter)FilterStripBizO["Carrier Service Level"];
			AssertEquals(0, svcLvlFilter.List.Count);

			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)FilterStripBizO["Carrier"];
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;
			AssertEquals("ABC", ((OrgCarrierServiceLevel)svcLvlFilter.List[0]).PL_Code);

			carrierFilter.Property = ZGuid.Empty;
			AssertEquals(0, svcLvlFilter.List.Count);

			carrierFilter.Property = carrier2.PK;
			AssertEquals("DEF", ((OrgCarrierServiceLevel)svcLvlFilter.List[0]).PL_Code);
		}

		#endregion

		#region Test Gateway Service Level

		public void TestGatewayServiceLevelFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RS_NKGatewayServiceLevel = "";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RS_NKGatewayServiceLevel = "STD";

			var consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_RS_NKGatewayServiceLevel = "DIR";

			Factory.Save();

			var gatewayServiceLevelFilter = (ModuleNkFilter)FilterStripBizO["Gateway Service Level"];
			var results = new MainFormConsolCollection(Factory);

			gatewayServiceLevelFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { consol1.PK, consol2.PK, consol3.PK }, results.Select(x => x.PK));

			gatewayServiceLevelFilter.Property = "STD";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { consol2.PK }, results.Select(x => x.PK));

			gatewayServiceLevelFilter.Property = "DEF";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZGuid>(), results.Select(x => x.PK));
		}

		#endregion

		#region Test Phase Filter

		public void TestPhaseFilter()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_Phase = "ALL";

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_Phase = "XXX";

			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			consol3.JK_Phase = "XXX";

			Factory.Save();

			ModuleTextFilter consolFilter = (ModuleTextFilter)FilterStripBizO["Phase"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { consol1.PK, consol2.PK, consol3.PK }, results.Select(x => x.PK));

			consolFilter.Property = "ALL";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { consol1.PK }, results.Select(x => x.PK));

			consolFilter.Property = "XXX";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { consol2.PK, consol3.PK }, results.Select(x => x.PK));
		}

		#endregion

		#region Test Consol Type Filter

		public void TestConsolType()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Consol Type"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = Core.Constants.AgentType.Agent;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = Core.Constants.AgentType.CoLoad;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}

		#endregion

		#region Test Sending / Receiving Handling Types

		public void TestSendingAgentType()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Sending Agent Type"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = AgentStatusList.Codes.GatewayAgent;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = AgentStatusList.Codes.GatewayAgentWithTariff;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}

		public void TestReceivingAgentType()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Receiving Agent Type"];
			var results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered", results.Contains(consol1.PK));
			Assert("Not filtered", results.Contains(consol2.PK));

			consolFilter.Property = AgentStatusList.Codes.GatewayAgent;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should be included", results.Contains(consol1.PK));
			Assert("Consol2 should not be included", !results.Contains(consol2.PK));

			consolFilter.Property = AgentStatusList.Codes.GatewayAgentWithTariff;
			results.Load(FilterStripBizO.Filter);
			Assert("Consol1 should not be included", !results.Contains(consol1.PK));
			Assert("Consol2 should be included", results.Contains(consol2.PK));
		}
		#endregion

		#region Test DGClass / DGSubstance

		public void TestDGClassDGSubstance()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var dgRestriction1 = consol1.ConsolDGRestrictionCollection.AddNew();
			dgRestriction1.JKD_Class = "1";
			dgRestriction1.JKD_UNNO = "9999";
			dgRestriction1.JKD_Variant = "a";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var dgRestriction2 = consol2.ConsolDGRestrictionCollection.AddNew();
			dgRestriction2.JKD_Class = "1.2B";
			dgRestriction2.JKD_UNNO = "9999";
			dgRestriction2.JKD_Variant = "b";

			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			var dgRestriction3 = consol3.ConsolDGRestrictionCollection.AddNew();
			dgRestriction3.JKD_Class = "3";
			dgRestriction3.JKD_UNNO = "9999";
			dgRestriction3.JKD_Variant = "c";

			var consol4 = Factory.NewWithValidTestData<ForwardingConsol>();

			Factory.Save();

			var consolFilter = (DGClassDGSubstanceFilter)FilterStripBizO["DG Class / DG Substance"];
			MainFormConsolCollection results = new MainFormConsolCollection(Factory);

			consolFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - Consol1 should be included", true, results.Contains(consol1.PK));
			AssertEquals("Not filtered - Consol2 should be included", true, results.Contains(consol2.PK));
			AssertEquals("Not filtered - Consol3 should be included", true, results.Contains(consol3.PK));

			consolFilter.DGClass = "1";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 does not have a 1 in its DG Class", false, results.Contains(consol3.PK));

			consolFilter.DGClass = "1.2B";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 does not have a 1.2B DG Class", false, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 does not have a 1.2B DG Class", false, results.Contains(consol3.PK));

			consolFilter.DGClass = "1";
			consolFilter.DGSubstance = "9999a";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 doesnt have a 123a DG Substance", false, results.Contains(consol2.PK));
			AssertEquals("Consol3 does not have a 1 in its DG Class or 123a DG Substance", false, results.Contains(consol3.PK));

			consolFilter.DGClass = "1.2B";
			consolFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 doesnt have a 123b DG Substance", false, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 does not have a 1.2 DG Class or 123b DG Substance", false, results.Contains(consol3.PK));

			consolFilter.DGClass = ZString.Empty;
			consolFilter.DGSubstance = ZString.Empty;

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 should be in the collection", true, results.Contains(consol3.PK));
			AssertEquals("Consol4 should not be in the collection", false, results.Contains(consol4.PK));

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should not be in the collection", false, results.Contains(consol1.PK));
			AssertEquals("Consol2 should not be in the collection", false, results.Contains(consol2.PK));
			AssertEquals("Consol3 should not be in the collection", false, results.Contains(consol3.PK));
			AssertEquals("Consol4 should be in the collection", true, results.Contains(consol4.PK));

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			consolFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should not be in the collection because it has 1.2B DG Substance", false, results.Contains(consol2.PK));
			AssertEquals("Consol3 should be in the collection", true, results.Contains(consol3.PK));
			AssertEquals("Consol4 should be in the collection", true, results.Contains(consol4.PK));

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			consolFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 should be in the collection", true, results.Contains(consol3.PK));
			AssertEquals("Consol4 should be in the collection", true, results.Contains(consol4.PK));

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			consolFilter.DGClass = ZString.Empty;
			consolFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should not be in the collection because it has 9999b substance", false, results.Contains(consol2.PK));
			AssertEquals("Consol3 should be in the collection", true, results.Contains(consol3.PK));
			AssertEquals("Consol4 should be in the collection", true, results.Contains(consol4.PK));

			consolFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			consolFilter.DGClass = ZString.Empty;
			consolFilter.DGSubstance = "XXXX";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Consol1 should be in the collection", true, results.Contains(consol1.PK));
			AssertEquals("Consol2 should be in the collection", true, results.Contains(consol2.PK));
			AssertEquals("Consol3 should be in the collection", true, results.Contains(consol3.PK));
			AssertEquals("Consol4 should not be in the collection because it is blank", false, results.Contains(consol4.PK));
		}

		#endregion

		#endregion

		#region Test Workflow Filters

		public void TestWorkflowFiltersPresent()
		{
			JobConsolFilterBusinessObject milestoneFilter = new JobConsolFilterBusinessObject();
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}

		#region Test Custom Fields Filters

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new JobConsolFilterBusinessObject().ModuleFilters;

			AssertNull(collection["C11"]);
			AssertNull(collection["C12"]);
			AssertNull(collection["C21"]);
			AssertNull(collection["C22"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["C31"]);

			PrepareTemplates();
			WorkflowCustomFieldsFilter.ClearCache();
			collection = new JobConsolFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["C12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["C21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["C22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertNull(collection["C31"]);
		}

		void PrepareTemplates()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			var def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			var def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			var def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			var def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();
		}

		#endregion

		#endregion

		#region CO2 Emission

		public void TestJobConsolCO2Filter()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var consol1 = Factory.New<ForwardingConsol>();
				consol1.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
				consol1.JK_IsForwarding = true;
				consol1.JK_IsCancelled = false;
				var shipment1 = consol1.Shipments.AddNew();
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
				shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
				shipment1.SetCO2ePerTonneInKg(200m);
				shipment1.JS_ActualWeight = 2000m;
				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				filter.LoadLayout(null);
				var filterOneOffQuoteCO2Total = ((CO2eStatusAndCO2eKgRangeNumberFilter)filter[JobConsolFilterBusinessObject.Descriptions.CO2e]);
				filterOneOffQuoteCO2Total.Property1 = 0.0;
				filterOneOffQuoteCO2Total.Property2 = 10.0;
				filterOneOffQuoteCO2Total.CO2eStatus = CO2eStatusList.Codes.Current;
				filterOneOffQuoteCO2Total.IsActive = true;

				var collection = new ForwardingConsolCollection(new BusinessObjectFactory());
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 0 JobConsol because status is NOT CURRENT", 0, collection.Count);

				consol1.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 JobConsol", 1, collection.Count);

				consol1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				Factory.Save();
				filterOneOffQuoteCO2Total.CO2eStatus = CO2eStatusList.Codes.Pending;
				filterOneOffQuoteCO2Total.Property1 = 0;
				filterOneOffQuoteCO2Total.Property2 = 0;
				collection.Load(filter.Filter);

				AssertEquals("Should have loaded 1 JobConsol", 1, collection.Count);
			}
		}

		#endregion

		#region Related Shipments

		public void TestRelatedShipmentsFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;

			var shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;

			var consol2 = Factory.New<ForwardingConsol>();

			var shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			shipment3.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;

			var shipment4 = consol2.Shipments.AddNew();
			shipment4.JS_TransportMode = Constants.TransportModes.Air;
			shipment4.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			var consol3 = Factory.New<ForwardingConsol>();

			var shipment5 = consol3.Shipments.AddNew();
			shipment5.JS_TransportMode = Constants.TransportModes.Sea;
			shipment5.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			var shipment6 = consol3.Shipments.AddNew();
			shipment6.JS_TransportMode = Constants.TransportModes.Sea;
			shipment6.JS_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			var consol4 = Factory.New<ForwardingConsol>();

			Factory.Save();

			var relatedShipmentsFilter = (ShipmentsOfConsolFilter)FilterStripBizO["Related Shipments"];
			relatedShipmentsFilter.IsActive = true;

			var transportModeFilter = relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", Constants.TransportModes.Air);
			transportModeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var releaseTypeFilter = relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Release Type", Constants.ShipmentReleaseTypes.BankLetterOfCredit);
			releaseTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var consols = new ForwardingConsolCollection(Factory);

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, consols);

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol4 }, consols);

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			consols.Load(FilterStripBizO.Filter);

			var pks = consols.Select(c => c.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consol3.PK, consol4.PK }, pks);
		}

		#endregion

		#region Related Shipments OSMG Filter

		public void TestRelatedShipmentsOSMGFilter_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToTrue()
		{
			AssertRelatedShipmentsOSMGFilter(true);
		}

		public void TestRelatedShipmentsOSMGFilter_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToFalse()
		{
			AssertRelatedShipmentsOSMGFilter(false);
		}

		void AssertRelatedShipmentsOSMGFilter(bool consolAllowAccessRegardlessOfShipmentsOSMGRights)
		{
			FreightDataRegistry.Instance.ConsolAllowAccessRegardlessOfShipmentsOSMGRights.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consolAllowAccessRegardlessOfShipmentsOSMGRights);

			var crmSecurityProvider = new JobShipmentCRMSecurityProvider();
			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			var testData = ConsolRelatedShipmentsOSMGSecurityCheckpointTest.GetTestData(Factory);

			var filterStripBizO = GetNewFilterStripBusinessObject();
			filterStripBizO.LoadLayout(null);
			AssertNull("Shipments Security Filter should not be there absent by default", filterStripBizO[JobConsolFilterBusinessObject.Descriptions.RelatedShipmentsSecurity]);

			var consols = new ForwardingConsolCollection(Factory);
			consols.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(testData.AllConsols, consols);

			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			filterStripBizO = GetNewFilterStripBusinessObject();
			filterStripBizO.LoadLayout(null);

			if (consolAllowAccessRegardlessOfShipmentsOSMGRights)
			{
				AssertNull("Shipments Security Filter should not be there absent by default", filterStripBizO[JobConsolFilterBusinessObject.Descriptions.RelatedShipmentsSecurity]);

				consols = new ForwardingConsolCollection(Factory);
				consols.Load(filterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(testData.AllConsols, consols);
			}
			else
			{
				var relatedShipmentsOSMGFilter = (ShipmentsOfConsolFilter)filterStripBizO[JobConsolFilterBusinessObject.Descriptions.RelatedShipmentsSecurity];
				AssertEquals(false, relatedShipmentsOSMGFilter.IsPublishedOnWeb);
				AssertEquals(FilterCategories.CRMSecurity, relatedShipmentsOSMGFilter.Category);
				AssertEquals(FilterOrCategory.None, relatedShipmentsOSMGFilter.GroupOrCategory);
				AssertEquals(true, relatedShipmentsOSMGFilter.IsGroupOrCategoryReadOnly);
				AssertEquals(ModuleTextFilter.ComparisonConstants.AllMatch, relatedShipmentsOSMGFilter.ComparisonOperator);
				AssertEquals(FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible, relatedShipmentsOSMGFilter.Visibility);
				AssertEquals(true, relatedShipmentsOSMGFilter.ReadOnly);
				AssertEquals(FilterOrCategory.MandatoryFilterOrCategory, relatedShipmentsOSMGFilter.OrCategory);
				AssertEquals(true, relatedShipmentsOSMGFilter.IsOrCategoryReadOnly);
				AssertEquals(true, relatedShipmentsOSMGFilter.IsMandatorySecurityFilter);

				consols = new ForwardingConsolCollection(Factory);
				consols.Load(filterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(testData.ConsolsWithAccessGranted, consols);
			}
		}

		#endregion

		#region Related Transport Legs

		public void TestRelatedTransportLegsFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();

			var transport1 = consol1.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_TransportType = Constants.TransportPlanningType.Flight1;

			var transport2 = consol1.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportType = Constants.TransportPlanningType.Flight1;

			var consol2 = Factory.New<ForwardingConsol>();

			var transport3 = consol2.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Air;
			transport3.JW_TransportType = Constants.TransportPlanningType.Flight1;

			var transport4 = consol2.Transports.AddNew();
			transport4.JW_TransportMode = Constants.TransportModes.Air;
			transport4.JW_TransportType = Constants.TransportPlanningType.Other;

			var consol3 = Factory.New<ForwardingConsol>();

			var transport5 = consol3.Transports.AddNew();
			transport5.JW_TransportMode = Constants.TransportModes.Sea;
			transport5.JW_TransportType = Constants.TransportPlanningType.Other;

			var transport6 = consol3.Transports.AddNew();
			transport6.JW_TransportMode = Constants.TransportModes.Sea;
			transport6.JW_TransportType = Constants.TransportPlanningType.Other;

			var consol4 = Factory.New<ForwardingConsol>();

			Factory.Save();

			var relatedShipmentsFilter = (RelatedTransportLegsOfConsolFilter)FilterStripBizO["Related Transport Legs"];
			relatedShipmentsFilter.IsActive = true;

			var transportModeFilter = relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", Constants.TransportModes.Air);
			transportModeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var releaseTypeFilter = relatedShipmentsFilter.SelectedFilters.AddTextFilterStrip("Transport Type", Constants.TransportPlanningType.Flight1);
			releaseTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var consols = new ForwardingConsolCollection(Factory);

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, consols);

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;

			consols.Load(FilterStripBizO.Filter);
			AssertEquals(0, consols.Count);

			relatedShipmentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			consols.Load(FilterStripBizO.Filter);

			var pks = consols.Select(c => c.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consol3.PK, consol4.PK }, pks);
		}

		#endregion

		[RequiresSTA]
		public void TestEmbeddedModulePopupFiltersHavePropertySet()
		{
			AssertFilterModuleHasPropertySet("House Bill", "H:ONETWO");
			AssertFilterModuleHasPropertySet("Master Bill", "M:THREEFOUR");
			AssertFilterModuleHasPropertySet("Booking Reference #", "B:FIVESIX");
			AssertFilterModuleHasPropertySet("Co-Load Master Bill #", "L:SEVENEIGHT");
			AssertFilterModuleHasPropertySet("Container #", "T:ABC");
			AssertFilterModuleHasPropertySet("Shipment #", "S:S00001000");
		}

		void AssertFilterModuleHasPropertySet(ZString description, ZString codeBoxText)
		{
			using (ZForm form = new ZForm())
			using (ZCodeFindBox findbox = new ZCodeFindBox())
			{
				form.Controls.Add(findbox);
				findbox.ModuleID = ModuleIDs.JobConsol;
				findbox.CodeBox.Text = codeBoxText;
				using (JobConsolModule module = new JobConsolModule())
				using (EmbeddedModulePopup popup = new EmbeddedModulePopup(module))
				{
					popup.ShowModal(findbox, form);

					System.Windows.Forms.Application.DoEvents();

					ZString prefix = codeBoxText.Split(':')[0];
					ZString code = codeBoxText.Split(':')[1];

					ModuleTextBaseFilter filter = module.FilterBusinessObject.ModuleFilters[description] as ModuleTextBaseFilter;
					AssertNotNull(description + " ModuleFilter", filter);
					AssertEquals(description + " ModuleFilter Prefix", prefix, filter.Prefix);

					ModuleTextAndNkFilter filterTextNK = module.FilterBusinessObject.ModuleFilters[description] as ModuleTextAndNkFilter;
					if (filterTextNK != null)
					{
						ZString[] values = code.Split('/');
						AssertEquals(description + " ModuleFilter Property", values[0], filter.Property);
						if (values.Length > 1)
						{
							AssertEquals(description + " ModuleFilter NkProperty", values[1], filterTextNK.NkProperty);
						}
					}
					else
					{
						AssertEquals(description + " ModuleFilter Property", code, filter.Property);
					}

					int count = 0;
					foreach (ModuleFilter filter2 in module.FilterBusinessObject.ModuleFilters)
					{
						if (filter2.Description.StartsWith(description))
						{
							count++;
						}
					}

					popup.Close();

					AssertEquals("Should only be one " + description + " ModuleFilter", 1, count);
				}
			}
		}

		public void TestConsolModeListHasSameElementsAsConsolHas()
		{
			var consol = GetConsol("Empty");
			List<string> possibleModes = new List<string>();

			foreach (ICodeDescription transportMode in consol.JK_TransportMode_List)
			{
				consol.JK_TransportMode = transportMode.Code;
				foreach (ICodeDescription agentType in consol.JK_AgentType_List)
				{
					consol.JK_AgentType = agentType.Code;
					foreach (ICodeDescription mode in consol.JK_ConsolMode_List)
					{
						if (!possibleModes.Contains(mode.Code))
						{
							possibleModes.Add(mode.Code);
						}
					}
				}
			}

			Assert(possibleModes.Count > 0);
			AssertContainsExactElementsInAnyOrder(possibleModes, FreightCodePairLists.ConsolModeList(string.Empty, string.Empty).Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestSearchOfUnlocoOutsideLoginBranchesRestriction()
		{
			Action<ModuleLocationFilter, bool, bool> assertFilterValidation = (moduleLocationFilter, errorsExpectedOnProperty1, errorsExpectedOnProperty2) =>
			{
				moduleLocationFilter.Validation.ValidateAll();
				AssertEquals(errorsExpectedOnProperty1, moduleLocationFilter.Property1Info.HasErrors());
				AssertEquals(errorsExpectedOnProperty2, moduleLocationFilter.Property2Info.HasErrors());
			};

			Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
			JobConsolFilterBusinessObject filterStripBizOSG = new JobConsolFilterBusinessObject();
			Factory.Save();
			ModuleLocationFilter originDestinationFilter = (ModuleLocationFilter)filterStripBizOSG[JobConsolFilterBusinessObject.Descriptions.OriginDestination];
			ModuleLocationFilter endPortsFilter = (ModuleLocationFilter)filterStripBizOSG[JobConsolFilterBusinessObject.Descriptions.EndPorts];

			originDestinationFilter.IsActive = true;
			endPortsFilter.IsActive = true;

			AssertEquals(originDestinationFilter.Visibility, FilterVisibility.AlwaysVisible);
			AssertEquals(endPortsFilter.Visibility, FilterVisibility.AlwaysVisible);

			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = ZString.Empty;
			endPortsFilter.Property1 = ZString.Empty;
			endPortsFilter.Property2 = "AUMEL";
			assertFilterValidation(originDestinationFilter, true, true);
			assertFilterValidation(endPortsFilter, true, true);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			endPortsFilter.Property1 = "AUSYD";
			endPortsFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(endPortsFilter, false, false);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			endPortsFilter.Property1 = "AUMEL";
			endPortsFilter.Property2 = ZString.Empty;
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(endPortsFilter, false, false);
		}

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "ETA";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();

			var collection = new JobConsolFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
			AssertNotNull(collection[customFieldName + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestGetModuleFiltersDoesNotDuplicateBooleanCustomFields()
		{
			var customFieldName = "Custom Boolean Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.Boolean,
				new[] { customFieldName },
				new[] { new Regex($@"{customFieldName} \(\d+\)") });
		}

		public void TestGetModuleFiltersDoesNotDuplicateComboCustomFields()
		{
			var customFieldName = "Custom Combo Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.ComboBox,
				new[] { $"{customFieldName} Code", $"{customFieldName} Description" },
				new[] { new Regex($@"{customFieldName} Code \(\d+\)"), new Regex($@"{customFieldName} Description \(\d+\)") });
		}

		public void TestGetModuleFiltersDoesNotDuplicateDecimalCustomFields()
		{
			var customFieldName = "Custom Decimal Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.Decimal,
				new[] { customFieldName },
				new[] { new Regex($@"{customFieldName} \({WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix}\)") });
		}

		public void TestGetModuleFiltersDoesNotDuplicateStringCustomFields()
		{
			var customFieldName = "Custom String Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.String,
				new[] { customFieldName },
				new[] { new Regex($@"{customFieldName} \({WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix}\)") });
		}

		public void TestGetModuleFiltersDoesNotDuplicateDateTimeCustomFields()
		{
			var customFieldName = "Custom DT Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.Datetime,
				new[] { customFieldName },
				new[] { new Regex($@"{customFieldName} \({WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix}\)") });
		}

		public void TestGetModuleFiltersDoesNotDuplicateIntegerCustomFields()
		{
			var customFieldName = "Custom Integer Field For Testing";

			AssertModuleFiltersDoesNotDuplicateForFieldType(
				customFieldName,
				AddOnColumnDataType.Codes.Integer,
				new[] { customFieldName },
				new[] { new Regex($@"{customFieldName} \({WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix}\)") });
		}

		void AssertModuleFiltersDoesNotDuplicateForFieldType(string fieldName, string dataType, string[] expectedValues, Regex[] invalidPatterns)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = fieldName;
			columnDef.XC_Type = dataType;

			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			var collection = new JobConsolFilterBusinessObject().ModuleFilters;

			// Make sure we have all the expected values, and none of the invalid ones
			foreach (string expected in expectedValues)
			{
				var msg = $"Unexpected number of occurrences for {expected}";
				AssertEquals(msg, 1, collection.Count(f => f.Description == expected));
			}

			foreach (var regex in invalidPatterns)
			{
				foreach (var filter in collection)
				{
					AssertNoMatch(regex, filter.Description);
				}
			}
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var org1 = GetOrgHeader("APRIS");
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org1.PK;

			var consol1 = GetConsolWithSendingAgent("comp1");
			consol1.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol2 = GetConsolWithSendingAgent("comp2");
			consol2.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol3 = GetConsolWithSendingAgent("comp3");
			consol3.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = consol1.PK;
			job1.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = consol2.PK;
			job2.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_ParentID = consol3.PK;
			job3.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStripBizO["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;
			var consols = new ForwardingConsolCollection(Factory);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol2, consol3 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol3 }, consols);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			consols.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, consols);
		}

		#region Template Records

		public void TestGetModuleFilters_AddsTemplateRecordFilterIfApplicable()
		{
			var expectedFilter = "Template Records";
			AssertNull(new JobConsolFilterBusinessObject().ModuleFilters[expectedFilter]);
			AssertNotNull(new JobConsolFilterBusinessObject(allowTemplateRecords: true).ModuleFilters[expectedFilter]);
		}

		public void TestApplyTemplateRecordFiltersLayout()
		{
			var filters = new JobConsolFilterBusinessObject(allowTemplateRecords: true);
			(filters as ITemplateRecordFilterProvider).ShouldApplyTemplateRecordFiltersLayout = true;

			var templateRecordsFilter = (ModuleTextFilter)filters[FilterStripBusinessObject.TemplateRecordsDescription];
			AssertNotNull(templateRecordsFilter);
			AssertEquals(FilterVisibility.AlwaysVisible, templateRecordsFilter.Visibility);
			Assert(templateRecordsFilter.ReadOnly);

			var templateActiveFilter = (ModuleTextFilter)filters[FilterStripBusinessObject.TemplateRecordsActive];
			AssertNotNull(templateActiveFilter);
			AssertEquals(FilterVisibility.AlwaysVisible, templateActiveFilter.Visibility);
			Assert(templateActiveFilter.ReadOnly);
		}

		public void TestTemplateRecordWarningMessage()
		{
			var filtersBO = new JobConsolFilterBusinessObject(allowTemplateRecords: true);

			var templateRecordsFilter = (ModuleTextFilter)filtersBO[FilterStripBusinessObject.TemplateRecordsDescription];
			AssertNotNull(templateRecordsFilter);
			templateRecordsFilter.IsActive = true;
			SetActiveModuleFiltersProvider(templateRecordsFilter);
			templateRecordsFilter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded;

			var templateNameFilter = (ModuleTextFilter)filtersBO[FilterStripBusinessObject.TemplateRecordsTemplateName];
			AssertNotNull(templateNameFilter);
			templateNameFilter.IsActive = true;
			SetActiveModuleFiltersProvider(templateNameFilter);
			templateNameFilter.Property = "A";

			var tempalteActiveFilter = (ModuleTextFilter)filtersBO[FilterStripBusinessObject.TemplateRecordsActive];
			AssertNotNull(tempalteActiveFilter);
			tempalteActiveFilter.IsActive = true;
			SetActiveModuleFiltersProvider(tempalteActiveFilter);
			tempalteActiveFilter.Property = "All";

			templateNameFilter.Validation.ValidateAll();
			AssertNoWarnings(templateNameFilter.PropertyInfo);

			templateRecordsFilter.Validation.ValidateAll();
			AssertNoWarnings(templateRecordsFilter.PropertyInfo);

			tempalteActiveFilter.Validation.ValidateAll();
			AssertNoWarnings(tempalteActiveFilter.PropertyInfo);

			void SetActiveModuleFiltersProvider(ModuleTextFilter filter)
			{
				var propertyInfo = filter.GetType().GetProperty("ActiveModuleFiltersProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				AssertNotNull(propertyInfo);

				propertyInfo.SetValue(filter, filtersBO);
			}
		}

		#endregion

		#region Implementation

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();

			result.Add(TableFilter("JobConsolTransport", "Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobSailing", "Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobVoyage", "Flight/Voyage # and Vessel"));
			result.Add(TableFilter("JobVoyDestination", "Flight/Voyage # and Vessel"));

			result.Add(TableFilter("CusEntryNum", "Additional Reference #"));

			result.Add(TableFilter("JobDocAddress", "Consignor / Consignee"));
			result.Add(TableFilter("OrgAddress", "Consignor / Consignee"));

			result.Add(TableFilter("OrgAddress", "Send / Receive Agents"));
			result.Add(TableFilter("OrgAddress", "Sending Agent Related Parties"));
			result.Add(TableFilter("OrgHeader", "Sending Agent Related Parties"));
			result.Add(TableFilter("OrgRelatedParty", "Sending Agent Related Parties"));
			result.Add(TableFilter("OrgAddress", "Receiving Agent Related Parties"));
			result.Add(TableFilter("OrgHeader", "Receiving Agent Related Parties"));
			result.Add(TableFilter("OrgRelatedParty", "Receiving Agent Related Parties"));
			result.Add(TableFilter("OrgAddress", "Carrier Related Parties"));
			result.Add(TableFilter("OrgHeader", "Carrier Related Parties"));
			result.Add(TableFilter("OrgRelatedParty", "Carrier Related Parties"));

			result.Add(TableFilter("JobConsolTransport", "Load / Discharge"));
			result.Add(TableFilter("JobSailing", "Load / Discharge"));
			result.Add(TableFilter("JobVoyOrigin", "Load / Discharge"));

			result.Add(TableFilter("AsycudaManifestHeader", "Manifest Registration Number"));
			result.Add(TableFilter("CusEntryNum", "Manifest Registration Number"));
			result.Add(TableFilter("AsycudaManifestHeader", "Manifest Registration Status"));
			result.Add(TableFilter("CusEntryNum", "Manifest Registration Status"));

			result.Add(TableFilter("JobConsolTransport", "Is Cargo Only"));
			result.Add(TableFilter("JobSailing", "Is Cargo Only"));
			result.Add(TableFilter("JobVoyage", "Is Cargo Only"));
			result.Add(TableFilter("JobVoyDestination", "Is Cargo Only"));

			result.Add(TableFilter("JPAFRBills", "AFR Bill Status"));
			result.Add(TableFilter("JPAFRHeader", "AFR Bill Status"));
			result.Add(TableFilter("CusInBondHeader", "AMS Bill Status"));
			result.Add(TableFilter("EDIMessage", "AMS Bill Status"));
			result.Add(TableFilter("GlbBranch", "AMS Bill Status"));
			result.Add(TableFilter("GlbCompany", "AMS Bill Status"));
			result.Add(TableFilter("JobConsol", "AMS Bill Status"));
			result.Add(TableFilter("JobConsolTransport", "AMS Bill Status"));
			result.Add(TableFilter("OrgCusCode", "AMS Bill Status"));

			result.Add(TableFilter("CusCAeMHMaster", "CA Close Status"));
			result.Add(TableFilter("CusCAeMHMaster", "CA Close Msg. Sta"));
			result.Add(TableFilter("CusCAeMHHouse", "CA House Status"));
			result.Add(TableFilter("CusCAeMHMaster", "CA House Status"));
			result.Add(TableFilter("CusCAeMHHouse", "CA House Msg. Sta"));
			result.Add(TableFilter("CusCAeMHMaster", "CA House Msg. Sta"));

			result.Add(TableFilter("JobConShipLink", "Related Shipments"));
			result.Add(TableFilter("JobShipment", "Related Shipments"));
			result.Add(TableFilter("JobContainer", "Related Containers"));
			result.Add(TableFilter("JobConsolTransport", "Related Transport Legs"));

			result.Add(TableFilter("JobContainer", "Container Type"));

			result.Add(TableFilter("ProcessTasks", "Milestone Completed"));
			result.Add(TableFilter("ProcessTasks", "Any Open Task Assigned To"));
			result.Add(TableFilter("ProcessTasks", "Next Task Assigned To"));
			result.Add(TableFilter("ProcessTasks", "Tasks"));
			result.Add(TableFilter("ProcessTasks", "Exceptions"));
			result.Add(TableFilter("ProcessTasks", "Milestones"));
			result.Add(TableFilter("ProcessTasks", "Triggers"));

			return result;
		}

		static string GetValue(ForwardingConsol consol, string country, string type)
		{
			foreach (CusEntryNumber number in consol.Numbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		static CusEntryNumber NewReferenceNumber(ForwardingConsol shipment, string countryCode, string type, string number)
		{
			CusEntryNumber result = shipment.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
		}

		ForwardingConsol NewConsol(string name, bool isLinked, string port1, string port2, params string[] otherports)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = name;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = (otherports.Length == 0 ? port2 : otherports[otherports.Length - 1]);

			Transport lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;
			lastTransport.JW_IsLinked = isLinked;

			foreach (string nextPort in otherports)
			{
				string lastPort = lastTransport.JW_RL_NKDiscPort;
				lastTransport = consol.Transports.AddNew();
				lastTransport.JW_IsLinked = isLinked;
				lastTransport.JW_RL_NKLoadPort = lastPort;
				lastTransport.JW_RL_NKDiscPort = nextPort;
			}

			return consol;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobConsolFilterBusinessObject();
		}

		#region Setup Related Parties

		OrgHeader GetOrgHeader(ZString fullName)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = fullName;

			return result;
		}

		OrgRelatedParty GetOrgRelatedParty(OrgHeader parent, OrgHeader relatedParty)
		{
			OrgRelatedParty result = Factory.NewWithValidTestData<OrgRelatedParty>();
			result.PR_OH_Parent = parent.PK;
			result.PR_OH_RelatedParty = relatedParty.PK;
			result.PR_PartyType = ZString.Empty;
			result.PR_FreightDirection = ZString.Empty;
			result.PR_FreightTransportMode = ZString.Empty;
			result.PR_FreightContainerMode = ZString.Empty;

			return result;
		}

		ForwardingConsol GetConsolWithSendingAgent(ZString companyName)
		{
			OrgHeader orgHeader = GetOrgHeader(companyName);

			ForwardingConsol result = GetConsol((++consolNumberIndex).ToString());
			result.JK_OA_SendingForwarderAddress = orgHeader.MainAddress.PK;

			return result;
		}
		ForwardingConsol GetConsolWithReceivingAgent(ZString companyName)
		{
			OrgHeader orgHeader = GetOrgHeader(companyName);

			ForwardingConsol result = GetConsol((++consolNumberIndex).ToString());
			result.JK_OA_ReceivingForwarderAddress = orgHeader.MainAddress.PK;

			return result;
		}
		ForwardingConsol GetConsolWithCarrier(ZString companyName)
		{
			OrgHeader orgHeader = GetOrgHeader(companyName);

			ForwardingConsol result = GetConsol((++consolNumberIndex).ToString());
			result.JK_OA_ShippingLineAddress = orgHeader.MainAddress.PK;

			return result;
		}

		ForwardingConsol GetConsol(ZString numberSuffix)
		{
			ForwardingConsol result = Factory.NewWithValidTestData<ForwardingConsol>();
			result.JK_MasterBillNum = "Consol" + numberSuffix;
			Asserter.AddToScope(result);

			return result;
		}

		int consolNumberIndex;

		FilterStripAsserter<ForwardingConsol> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<ForwardingConsol>(Factory, (s) => s.JK_MasterBillNum)); }
		}
		FilterStripAsserter<ForwardingConsol> asserter;

		#endregion

		protected FilterStripBusinessObject FilterStripBizO
		{
			get { return filterStripBizO ?? (filterStripBizO = GetNewFilterStripBusinessObject()); }
		}
		FilterStripBusinessObject filterStripBizO;

		#endregion
	}
}
