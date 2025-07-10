using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using FilterConstants = Enterprise.TransportConsignment.Module.DtbCarrierFilterBusinessObject.FilterConstants;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbCarrierFilterBusinessObject))]
	public class DtbCarrierFilterBusinessObjectTest : DtbChildFilterBusinessObjectTest
	{
		#region Test Text Filters Max Length

		public void TestTextFiltersMaxLength()
		{
			CombineAssertions(() =>
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				TextFilterNameAndMaxLengthDictionary.ForEach(pair => AssertEquals($"The max length of filter {pair.Key} should be set as {pair.Value}.", Math.Min(pair.Value, ModuleFilter.MaxMaximumLength), filterBizO[pair.Key].MaxLength));
			});
		}

		IDictionary<string, int> TextFilterNameAndMaxLengthDictionary => new Dictionary<string, int>
		{
			{ FilterConstants.State, OrgAddressSchema.OA_State.MaxLength }
		};

		#endregion

		#region TestCarrierBranch

		public void TestCarrierBranch()
		{
			var unloco1 = Factory.New<RefUNLOCO>();
			var unloco2 = Factory.New<RefUNLOCO>();
			var unloco3 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "LOCO1";
			unloco2.RL_Code = "LOCO2";
			unloco3.RL_Code = "LOCO3";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_IsActive = true;
			branch2.GB_IsActive = true;
			branch3.GB_IsActive = true;
			branch1.GB_RL_NKHomePort = unloco1.RL_Code;
			branch2.GB_RL_NKHomePort = unloco2.RL_Code;
			branch3.GB_RL_NKHomePort = unloco3.RL_Code;

			var carrier1 = Helper.CreateOrganisation("CAR1");
			var carrier2 = Helper.CreateOrganisation("CAR2");
			var carrier3 = Helper.CreateOrganisation("CAR3");
			var carrier4 = Helper.CreateOrganisation("CAR4");

			carrier1.OH_RL_NKClosestPort = unloco1.RL_Code;
			carrier2.OH_RL_NKClosestPort = unloco2.RL_Code;
			carrier3.OH_RL_NKClosestPort = unloco3.RL_Code;
			carrier4.OH_RL_NKClosestPort = unloco3.RL_Code;

			AssertEquals("Precondition", branch1, carrier1.Branch);
			AssertEquals("Precondition", branch2, carrier2.Branch);
			AssertEquals("Precondition", branch3, carrier3.Branch);
			AssertEquals("Precondition", branch3, carrier4.Branch);

			Factory.Save();

			Asserter.AddToScope(carrier1);
			Asserter.AddToScope(carrier2);
			Asserter.AddToScope(carrier3);
			Asserter.AddToScope(carrier4);

			var filterBizO = GetNewFilterStripBusinessObject();
			var branchFilter = (ModuleGuidFilter)filterBizO[DtbCarrierFilterBusinessObject.FilterConstants.Branch];
			AssertEquals(DtbCarrierFilterBusinessObject.FilterConstants.CarrierCategory, branchFilter.Category);

			branchFilter.IsActive = true;
			branchFilter.Property = branch1.PK;
			Asserter.AssertMatches("carrier1 has branch of branch1.", filterBizO.Filter, carrier1);

			branchFilter.Property = branch2.PK;
			Asserter.AssertMatches("carrier2 has branch of branch2.", filterBizO.Filter, carrier2);

			branchFilter.Property = branch3.PK;
			Asserter.AssertMatches("carrier3, carrier4 has branch of branch3.", filterBizO.Filter, carrier3, carrier4);
		}

		#endregion

		#region TestCarrierState

		public void TestCarrierState()
		{
			var carrier1 = Helper.CreateOrganisation("CAR1");
			var carrier2 = Helper.CreateOrganisation("CAR2");
			var carrier3 = Helper.CreateOrganisation("CAR3");
			var carrier4 = Helper.CreateOrganisation("CAR4");

			carrier1.MainAddress.OA_State = "QLD";
			carrier2.MainAddress.OA_State = "VIC";
			carrier3.MainAddress.OA_State = "NSW";
			carrier4.MainAddress.OA_State = "NSW";

			carrier1.MainAddress.OA_Address1 = "QLD ADDRESS 1";
			carrier2.MainAddress.OA_Address1 = "VIC ADDRESS";
			carrier3.MainAddress.OA_Address1 = "NSW ADDRESS 1";
			carrier4.MainAddress.OA_Address1 = "NSW ADDRESS 2";

			var secondAddress = carrier4.Addresses.AddNew(OrgAddressType.Office, false);
			secondAddress.OA_Address1 = "QLD ADDRESS 2";
			secondAddress.OA_State = "QLD";

			Factory.Save();

			Asserter.AddToScope(carrier1);
			Asserter.AddToScope(carrier2);
			Asserter.AddToScope(carrier3);
			Asserter.AddToScope(carrier4);

			var filterBizO = GetNewFilterStripBusinessObject();
			var stateFilter = (ModuleTextFilter)filterBizO[DtbCarrierFilterBusinessObject.FilterConstants.State];
			AssertEquals(DtbCarrierFilterBusinessObject.FilterConstants.CarrierCategory, stateFilter.Category);

			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			Asserter.AssertMatches("carrier1, carrier4 have addresses in QLD.", filterBizO.Filter, carrier1, carrier4);

			stateFilter.Property = "VIC";
			Asserter.AssertMatches("carrier2 has address in VIC.", filterBizO.Filter, carrier2);

			stateFilter.Property = "NSW";
			Asserter.AssertMatches("carrier3, carrier4 have addresses in NSW.", filterBizO.Filter, carrier3, carrier4);
		}

		#endregion

		#region TestOrCategoryFilters

		public void TestOrCategoryFilters()
		{
			var carrier1 = Helper.CreateOrganisation("CAR1");
			var carrier2 = Helper.CreateOrganisation("CAR2");
			var carrier3 = Helper.CreateOrganisation("CAR3");
			carrier1.MainAddress.OA_State = "QLD";
			carrier2.MainAddress.OA_State = "VIC";
			carrier3.MainAddress.OA_State = "NSW";

			Asserter.AddToScope(carrier1);
			Asserter.AddToScope(carrier2);
			Asserter.AddToScope(carrier3);

			Factory.Save();

			var filterBizO = new DtbRoutePlannerFilterBusinessObject();
			var childFilterBizO = new DtbCarrierFilterBusinessObject();
			filterBizO.AddChildFilterBusinessObject(childFilterBizO);

			var stateFilter = (ModuleTextFilter)filterBizO[DtbCarrierFilterBusinessObject.FilterConstants.State];
			stateFilter.IsActive = true;
			stateFilter.Property = "QLD";
			stateFilter.OrCategory = FilterOrCategory.Red;

			var stateFilter2Description = stateFilter.Description + " (1) ";
			var stateFilter2 = new ModuleTextFilter(stateFilter2Description, childFilterBizO.GetStateQueryForTest());
			stateFilter2.Category = DtbCarrierFilterBusinessObject.FilterConstants.CarrierCategory;
			stateFilter2.IsActive = true;
			stateFilter2.Property = "VIC";
			stateFilter2.OrCategory = FilterOrCategory.Red;
			filterBizO.ModuleFilters.AddFilter(stateFilter2);
			Asserter.AssertMatches("should return both of carrier1  and carrier2.", childFilterBizO.Filter, carrier1, carrier2);

			var stateFilter3Description = stateFilter.Description + " (2) ";
			var stateFilter3 = new ModuleTextFilter(stateFilter3Description, childFilterBizO.GetStateQueryForTest());
			stateFilter3.Category = DtbCarrierFilterBusinessObject.FilterConstants.CarrierCategory;
			stateFilter3.IsActive = true;
			stateFilter3.Property = "NSW";
			stateFilter3.OrCategory = FilterOrCategory.Green;
			filterBizO.ModuleFilters.AddFilter(stateFilter3);
			Asserter.AssertMatches("should not return any carrier.", childFilterBizO.Filter);
		}

		#endregion

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		protected FilterStripAsserter<OrgHeader> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<OrgHeader>(Factory, c => c.OH_Code)); }
		}

		FilterStripAsserter<OrgHeader> asserter;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new DtbCarrierFilterBusinessObject();
		}

		protected override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(OrgHeader); }
		}

		protected override ModuleTextFilter DuplicateFilter(string description)
		{
			var stateFilterDescription = description + " (1) ";
			return new ModuleTextFilter(stateFilterDescription, OrgAddressSchema.OA_State);
		}

		protected override FilterCategory FilterCategoryOfChildFilter
		{
			get { return DtbCarrierFilterBusinessObject.FilterConstants.CarrierCategory; }
		}

		protected override string ChildFilterName
		{
			get { return DtbCarrierFilterBusinessObject.FilterConstants.State; }
		}

		protected override DtbChildFilterBusinessObject GetNewChildFilterBusinessObject
		{
			get { return (DtbChildFilterBusinessObject)GetNewFilterStripBusinessObject(); }
		}

		protected override SchemaColumn ExpectedFieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_OH_TransportCo; }
		}

		protected override SchemaColumn ExpectedChildBizOPKOrNK
		{
			get { return OrgHeaderSchema.PK; }
		}

		#endregion
	}
}
