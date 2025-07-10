using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using JobSailing = Enterprise.Freight.Business.JobSailing;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ShipmentGatePassFilterBusinessObject))]
	sealed class ShipmentGatePassFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Number Filter Tests

		public void TestContainerNumberFilter()
		{
			Container1.JC_ContainerNum = "11100011";
			Container2.JC_ContainerNum = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.ContainerNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestContainerNumberFilterWhenBlank()
		{
			var filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.ContainerNumber];

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5 }, FilterCollection);

			Container1.JC_ContainerNum = "11100011";
			Container2.JC_ContainerNum = "22000222";
			Factory.Save();

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1, Shipment2 }, FilterCollection);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment3, Shipment4, Shipment5 }, FilterCollection);
		}

		public void TestHouseBillFilter()
		{
			Shipment1.JS_HouseBill = "11100011";
			Shipment2.JS_HouseBill = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.HouseBill];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestShipmentIDFilter()
		{
			Shipment1.JS_UniqueConsignRef = "11100011";
			Shipment2.JS_UniqueConsignRef = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.ShipmentID];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestGatePassIDFilter()
		{
			Leg1.EU_GatePassCount = 7;
			Leg2.EU_GatePassCount = 8;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.GatePassID];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "G";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "H";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestGatePassIDFilterValidation()
		{
			var filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.GatePassID];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = ZString.Format("11100011{0}IU", CommonPickupDeliveryConfirm.GatePassIDSeparator);
			AssertNoErrors(filter.PropertyInfo);

			filter.Property = ZString.Format("11100011{0}IV", CommonPickupDeliveryConfirm.GatePassIDSeparator);
			AssertHasErrors("Should have an error if GatePassID is invalid", filter.PropertyInfo);
		}

		public void TestGatePassIDSubMatchFilter()
		{
			Shipment1.JS_UniqueConsignRef = "11100011";
			Shipment2.JS_UniqueConsignRef = "22000222";

			Leg1.EU_GatePassCount = 1;
			Leg2.EU_GatePassCount = 1;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.GatePassID];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZString.Format("11100011{0}A", CommonPickupDeliveryConfirm.GatePassIDSeparator);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = ZString.Format("0222{0}A", CommonPickupDeliveryConfirm.GatePassIDSeparator);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
		}

		public void TestLoadListNumberFilter()
		{
			Consol1.JK_UniqueConsignRef = "11100011";
			Consol2.JK_UniqueConsignRef = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.LoadListNo];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment5", FilterCollection.Contains(Shipment5));
			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
		}

		public void TestLoadListNumberFilterWithoutPackLine()
		{
			Consol1.JK_UniqueConsignRef = "11100011";

			Container1.RemovePackLine(Pack1);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ShipmentGatePassFilterBusinessObject.NumberFilterTypes.LoadListNo];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
		}

		public void TestCommonNumbersFilter()
		{
			Shipment1.JS_UniqueConsignRef = "11111111";
			Shipment2.JS_UniqueConsignRef = "22222222";

			Shipment1.JS_HouseBill = "33333333";
			Shipment2.JS_HouseBill = "44444444";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Common Numbers and References"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestHouseCCNFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var filterBusinessObject = (ShipmentGatePassFilterBusinessObject)GetNewFilterStripBusinessObject();
				var filter = (ModuleNumberFilter)filterBusinessObject["House CCN"];
				AssertNull(filter);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				CusEntryNumber num1 = Shipment1.Numbers.AddNew();
				num1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num1.CE_EntryNum = "1111";

				CusEntryNumber num2 = Shipment2.Numbers.AddNew();
				num2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num2.CE_EntryNum = "1122";

				CusEntryNumber num3 = Shipment3.Numbers.AddNew();
				num3.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
				num3.CE_EntryNum = "3344";

				Factory.Save();

				AssertEquals("Precondition:", "1111", Shipment1.CanadaHouseCCN);
				AssertEquals("Precondition:", "1122", Shipment2.CanadaHouseCCN);
				AssertEquals("Precondition:", "3344", Shipment3.CanadaHouseCCN);
				AssertEquals("Precondition:", ZString.Empty, Shipment4.CanadaHouseCCN);

				var filterBusinessObject = (ShipmentGatePassFilterBusinessObject)GetNewFilterStripBusinessObject();
				var filter = (ModuleNumberFilter)filterBusinessObject["House CCN"];
				AssertNotNull(filter);

				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "11";
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
				Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
				Assert("Expecting collection not to contain Shipment3", !FilterCollection.Contains(Shipment3));
				Assert("Expecting collection not to contain Shipment4", !FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
				Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
				Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
				Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "1111";
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
				Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
				Assert("Expecting collection not to contain Shipment3", !FilterCollection.Contains(Shipment3));
				Assert("Expecting collection not to contain Shipment4", !FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
				Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
				Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
				Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
				filter.Property = "2";
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
				Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
				Assert("Expecting collection not to contain Shipment3", !FilterCollection.Contains(Shipment3));
				Assert("Expecting collection not to contain Shipment4", !FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
				Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
				Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
				Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
				Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
				Assert("Expecting collection not to contain Shipment3", !FilterCollection.Contains(Shipment3));
				Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));

				filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				FilterCollection.Load(filterBusinessObject.Filter);

				Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
				Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
				Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
				Assert("Expecting collection not to contain Shipment4", !FilterCollection.Contains(Shipment4));
			}
		}

		#endregion

		#region Date Filter Tests

		public void TestDeliveryDateFilter()
		{
			Leg1.EU_PickupDeliveryTime = new ZDateTime(2000, 1, 1, 11, 0, 0);       // 2 Jan 11:00
			Leg2.EU_PickupDeliveryTime = new ZDateTime(2000, 2, 2, 22, 0, 0);       // 2 Feb 22:00

			Leg1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;
			Leg2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ShipmentGatePassFilterBusinessObject.DateFilterTypes.Delivery];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestETDFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();

			origin1.JA_E_DEP = new ZDateTime(2000, 1, 1, 11, 0, 0);
			origin2.JA_E_DEP = new ZDateTime(2000, 2, 2, 22, 0, 0);

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			Consol1.Transports.Add(transport1);
			Consol2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Shipment1.Consols.Add(Consol1);
			Shipment2.Consols.Add(Consol2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ShipmentGatePassFilterBusinessObject.DateFilterTypes.ETD];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		#endregion

		#region Organisation Filter Tests

		public void TestClientFilter()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();

			client1.OH_IsDebtor = ZBool.True;
			client2.OH_IsDebtor = ZBool.True;
			client3.OH_IsDebtor = ZBool.True;

			Shipment1.JS_OH_HandledOnBehalfOfForwarder = client1.PK;
			Shipment2.JS_OH_HandledOnBehalfOfForwarder = client2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[ShipmentGatePassFilterBusinessObject.OrgFilterTypes.Client];

			filter.Property = client1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Property = client3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestConsignorFilter()
		{
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor3 = Factory.NewWithValidTestData<OrgHeader>();

			consignor1.OH_IsConsignor = ZBool.True;
			consignor2.OH_IsConsignor = ZBool.True;
			consignor3.OH_IsConsignor = ZBool.True;

			OrgAddress orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Code = "1";
			OrgAddress orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_Code = "2";

			orgAddress1.OA_OH = consignor1.PK;
			orgAddress2.OA_OH = consignor2.PK;

			JobDocAddress docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			JobDocAddress docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();

			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;

			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			docAddress1.E2_ParentID = Shipment1.PK;
			docAddress2.E2_ParentID = Shipment2.PK;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterBO[String.Format("{0} / {1}", ShipmentGatePassFilterBusinessObject.OrgFilterTypes.Consignor, ShipmentGatePassFilterBusinessObject.OrgFilterTypes.Consignee)];

			filter.Property1 = consignor1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Property1 = consignor3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestConsigneeFilter()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee3 = Factory.NewWithValidTestData<OrgHeader>();

			consignee1.OH_IsConsignee = ZBool.True;
			consignee2.OH_IsConsignee = ZBool.True;
			consignee3.OH_IsConsignee = ZBool.True;

			OrgAddress orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Code = "1";
			OrgAddress orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_Code = "2";

			orgAddress1.OA_OH = consignee1.PK;
			orgAddress2.OA_OH = consignee2.PK;

			JobDocAddress docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			JobDocAddress docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();

			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			docAddress1.E2_ParentID = Shipment1.PK;
			docAddress2.E2_ParentID = Shipment2.PK;

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterBO[String.Format("{0} / {1}", ShipmentGatePassFilterBusinessObject.OrgFilterTypes.Consignor, ShipmentGatePassFilterBusinessObject.OrgFilterTypes.Consignee)];

			filter.Property2 = consignee1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Property2 = consignee3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		#endregion

		#region Location Filter Tests

		public void TestLoadDischargeFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageDestination destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			VoyageDestination destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			origin1.JA_RL_NKPortOfLoading = "KRSEL";
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;
			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			Consol1.Transports.Add(transport1);
			Consol2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Shipment1.Consols.Add(Consol1);
			Shipment2.Consols.Add(Consol2);

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ShipmentGatePassFilterBusinessObject.PortFilterTypes.LoadDischarge];

			filter.Property1 = "KR";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Property1 = "AUBNE";
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		public void TestOriginDestinationFilter()
		{
			Shipment1.JS_RL_NKOrigin = "AAAAA";
			Shipment2.JS_RL_NKOrigin = "BBBBB";

			Shipment1.JS_RL_NKDestination = "CCCCC";
			Shipment2.JS_RL_NKDestination = "CCCCC";

			Shipment1.JS_TranshipToOtherCFS = ZBool.True;
			Shipment2.JS_TranshipToOtherCFS = ZBool.True;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ShipmentGatePassFilterBusinessObject.PortFilterTypes.OriginDestination];

			filter.Property1 = "AA";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Property1 = "BBBBB";
			filter.Property2 = "CCCCC";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "CCCCC";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.Property1 = "CCCCC";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
		}

		#endregion

		#region Vessel / Voyage Filter Tests

		public void TestVesselFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();

			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			Consol1.Transports.Add(transport1);
			Consol2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Shipment1.Consols.Add(Consol1);
			Shipment2.Consols.Add(Consol2);

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.Vessel = "XAAAX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Vessel = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.Vessel = "YAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
		}

		public void TestVoyageFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();

			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";
			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			Consol1.Transports.Add(transport1);
			Consol2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport2.JW_IsLinked = true;

			transport1.JW_JX = sailing1.PK;
			transport2.JW_JX = sailing2.PK;

			Shipment1.Consols.Add(Consol1);
			Shipment2.Consols.Add(Consol2);

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.VoyageFlightNo = "XAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "YAAAY";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "AAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "ZZZ";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.VoyageFlightNo = "";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
		}

		#endregion

		#region Status Filter Tests

		public void TestJobInvoicingStatusFilter()
		{
			AssertNotNull(FilterBO["Invoicing Job Status"]);
			var jobstatusFilter = (ModuleTextFilter)FilterBO["Invoicing Job Status"];

			var job = new JobHeader.Loader(Shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);
			AssertCollectionContains(Shipment1, FilterCollection);

			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			FilterCollection.Load(FilterBO.Filter);

			AssertCollectionNotContains(Shipment1, FilterCollection);
		}

		public void TestDeliveryStatusFilter()
		{
			Shipment1.JS_OuterPacks = 10;
			Shipment2.JS_OuterPacks = 20;
			Shipment3.JS_OuterPacks = 30;
			Shipment4.JS_OuterPacks = 40;

			Pack1.JL_Outturn = Shipment1.JS_OuterPacks;
			Pack2.JL_Outturn = Shipment2.JS_OuterPacks;
			Pack3.JL_Outturn = Shipment3.JS_OuterPacks;
			Pack4.JL_Outturn = Shipment4.JS_OuterPacks;

			Leg1.EU_GatePassCount = 7;
			Leg2.EU_GatePassCount = 8;
			Leg3.EU_GatePassCount = 9;

			Leg1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;
			Leg2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;
			Leg3.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;

			Divot1.J8_PackagesDelivered = Shipment1.JS_OuterPacks; //Fully Delivered
			Divot2.J8_PackagesDelivered = Shipment2.JS_OuterPacks + 5; //Not fully delivered
			Divot3.J8_PackagesDelivered = Shipment3.JS_OuterPacks - 5; //Not fully delivered

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Delivery Status"];

			filter.Property = FilterBO.DeliveryStatus_List.FullyDeliveredShipments.Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection not to contain Shipment2", !FilterCollection.Contains(Shipment2));
			Assert("Expecting collection not to contain Shipment3", !FilterCollection.Contains(Shipment3));
			Assert("Expecting collection not to contain Shipment4", !FilterCollection.Contains(Shipment4));

			filter.Property = FilterBO.DeliveryStatus_List.NotFullyDeliveredShipments.Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
			Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
			Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));

			filter.Property = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
			Assert("Expecting collection to contain Shipment2", FilterCollection.Contains(Shipment2));
			Assert("Expecting collection to contain Shipment3", FilterCollection.Contains(Shipment3));
			Assert("Expecting collection to contain Shipment4", FilterCollection.Contains(Shipment4));
		}

		public void TestDeliveryStatusFilterWhenConfirmTypesIsNotDestinationCFSDeparture()
		{
			Divot2.J8_EU_PickupDeliverConfirm = Leg2.PK;
			Divot2.J8_JL = Pack1.PK;

			Shipment1.JS_OuterPacks = 30;
			Pack1.JL_Outturn = Shipment1.JS_OuterPacks;

			Leg1.EU_GatePassCount = 9;
			Leg2.EU_GatePassCount = 10;

			Leg1.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationCFSDeparture;
			Leg2.EU_PickupDeliveryType = Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			Divot1.J8_PackagesDelivered = Shipment1.JS_OuterPacks; //Fully Delivered
			Divot2.J8_PackagesDelivered = Shipment1.JS_OuterPacks; //Fully Delivered

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Delivery Status"];
			filter.Property = FilterBO.DeliveryStatus_List.FullyDeliveredShipments.Code;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));

			filter.Property = FilterBO.DeliveryStatus_List.NotFullyDeliveredShipments.Code;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection not to contain Shipment1", !FilterCollection.Contains(Shipment1));

			filter.Property = ZString.Empty;
			filter.IsActive = true;
			FilterCollection.Load(FilterBO.Filter);
			Assert("Expecting collection to contain Shipment1", FilterCollection.Contains(Shipment1));
		}

		#endregion

		#region Other Tests

		public void TestListProperties()
		{
			AssertNotNull(FilterBO.Client_List);
			AssertNotNull(FilterBO.Consignor_List);
			AssertNotNull(FilterBO.Consignee_List);
			AssertNotNull(FilterBO.Organisation_List);
			AssertNotNull(FilterBO.Location_List);
			AssertNotNull(FilterBO.Vessel_List);
			AssertNotNull(FilterBO.DeliveryStatus_List);
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var job1 = new JobHeader.Loader(Shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = new JobHeader.Loader(Shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = new JobHeader.Loader(Shipment3).TryLoadOrCreateWithoutMutexForTestOnly();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1, Shipment2 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment2, Shipment3 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment2, Shipment3 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment2, Shipment3 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment3 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { Shipment1, Shipment2 }, FilterCollection);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ShipmentGatePassFilterBusinessObject();
		}

		GatePassShipment Shipment1;
		GatePassShipment Shipment2;
		GatePassShipment Shipment3;
		GatePassShipment Shipment4;
		GatePassShipment Shipment5;

		GatePassPackLine Pack1;
		GatePassPackLine Pack2;
		GatePassPackLine Pack3;
		GatePassPackLine Pack4;

		GatePassContainer Container1;
		GatePassContainer Container2;
		GatePassContainer Container3;
		GatePassContainer Container4;

		GatePassLoadListConsol Consol1;
		GatePassLoadListConsol Consol2;
		GatePassLoadListConsol Consol3;
		GatePassLoadListConsol Consol4;

		CommonConfirmDivot Divot1;
		CommonConfirmDivot Divot2;
		CommonConfirmDivot Divot3;

		CommonPickupDeliveryConfirm Leg1;
		CommonPickupDeliveryConfirm Leg2;
		CommonPickupDeliveryConfirm Leg3;

		GatePassShipmentList FilterCollection;
		ShipmentGatePassFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = Factory.NewWithValidTestData<GatePassShipment>();
			Shipment2 = Factory.NewWithValidTestData<GatePassShipment>();
			Shipment3 = Factory.NewWithValidTestData<GatePassShipment>();
			Shipment4 = Factory.NewWithValidTestData<GatePassShipment>();
			Shipment5 = Factory.NewWithValidTestData<GatePassShipment>();

			Shipment1.JS_RL_NKOrigin = "CNSHA";
			Shipment1.JS_RL_NKDestination = "AUSYD";
			Shipment2.JS_RL_NKOrigin = "CNSHA";
			Shipment2.JS_RL_NKDestination = "AUSYD";
			Shipment3.JS_RL_NKOrigin = "CNSHA";
			Shipment3.JS_RL_NKDestination = "AUSYD";
			Shipment4.JS_RL_NKOrigin = "CNSHA";
			Shipment4.JS_RL_NKDestination = "AUSYD";
			Shipment5.JS_RL_NKOrigin = "CNSHA";
			Shipment5.JS_RL_NKDestination = "AUSYD";

			Pack1 = Factory.NewWithValidTestData<GatePassPackLine>();
			Pack2 = Factory.NewWithValidTestData<GatePassPackLine>();
			Pack3 = Factory.NewWithValidTestData<GatePassPackLine>();
			Pack4 = Factory.NewWithValidTestData<GatePassPackLine>();

			Container1 = Factory.NewWithValidTestData<GatePassContainer>();
			Container2 = Factory.NewWithValidTestData<GatePassContainer>();
			Container3 = Factory.NewWithValidTestData<GatePassContainer>();
			Container4 = Factory.NewWithValidTestData<GatePassContainer>();

			Container1.JC_ContainerNum = ZString.Empty;
			Container2.JC_ContainerNum = ZString.Empty;
			Container3.JC_ContainerNum = ZString.Empty;
			Container4.JC_ContainerNum = ZString.Empty;

			Consol1 = Factory.NewWithValidTestData<GatePassLoadListConsol>();
			Consol2 = Factory.NewWithValidTestData<GatePassLoadListConsol>();
			Consol3 = Factory.NewWithValidTestData<GatePassLoadListConsol>();
			Consol4 = Factory.NewWithValidTestData<GatePassLoadListConsol>();

			Consol1.Shipments.Add(Shipment1);
			Consol2.Shipments.Add(Shipment2);
			Consol3.Shipments.Add(Shipment3);
			Consol4.Shipments.Add(Shipment4);

			Divot1 = Factory.NewWithValidTestData<CommonConfirmDivot>();
			Divot2 = Factory.NewWithValidTestData<CommonConfirmDivot>();
			Divot3 = Factory.NewWithValidTestData<CommonConfirmDivot>();

			Divot1.PackLineType = typeof(GatePassPackLine);
			Divot2.PackLineType = typeof(GatePassPackLine);
			Divot3.PackLineType = typeof(GatePassPackLine);

			Leg1 = Factory.NewWithValidTestData<CommonPickupDeliveryConfirm>();
			Leg2 = Factory.NewWithValidTestData<CommonPickupDeliveryConfirm>();
			Leg3 = Factory.NewWithValidTestData<CommonPickupDeliveryConfirm>();

			Leg1.PackLineType = typeof(GatePassPackLine);
			Leg2.PackLineType = typeof(GatePassPackLine);
			Leg3.PackLineType = typeof(GatePassPackLine);
			Leg1.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);
			Leg2.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 26);
			Leg3.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 27);

			Container1.PackLines.Add(Pack1);
			Container2.PackLines.Add(Pack2);
			Container3.PackLines.Add(Pack3);
			Container4.PackLines.Add(Pack4);

			Container1.JC_JK = Consol1.PK;
			Container2.JC_JK = Consol2.PK;
			Container3.JC_JK = Consol3.PK;
			Container4.JC_JK = Consol4.PK;

			Pack1.JL_JS = Shipment1.PK;
			Pack2.JL_JS = Shipment2.PK;
			Pack3.JL_JS = Shipment3.PK;
			Pack4.JL_JS = Shipment4.PK;

			Divot1.J8_EU_PickupDeliverConfirm = Leg1.PK;
			Divot2.J8_EU_PickupDeliverConfirm = Leg2.PK;
			Divot3.J8_EU_PickupDeliverConfirm = Leg3.PK;

			Divot1.J8_JL = Pack1.PK;
			Divot2.J8_JL = Pack2.PK;
			Divot3.J8_JL = Pack3.PK;

			Shipment1.JS_IsCFSRegistered = ZBool.True;
			Shipment2.JS_IsCFSRegistered = ZBool.True;
			Shipment3.JS_IsCFSRegistered = ZBool.True;
			Shipment4.JS_IsCFSRegistered = ZBool.True;
			Shipment5.JS_IsCFSRegistered = ZBool.True;

			Factory.Save();

			FilterCollection = new GatePassShipmentList(Factory);
			FilterBO = (ShipmentGatePassFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion
	}
}
