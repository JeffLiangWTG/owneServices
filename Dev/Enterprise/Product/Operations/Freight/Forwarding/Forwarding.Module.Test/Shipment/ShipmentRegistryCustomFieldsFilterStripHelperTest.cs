using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class ShipmentRegistryCustomFieldsFilterStripHelperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			AssertEquals(typeof(ForwardingShipment), helper.BusinessObjectType);
			AssertEquals("ShipmentRegistryCustomFieldsFilterStripHelperAutomaticFilterTest", helper.GetAutomaticFilterTestCaseName_ForObjectFactory());
			Assert(helper.CanAddFilters());
			Assert(helper.IsApplicableToBizOTypeIsAssignableFrom());
		}

		public void TestInitialise()
		{
			helper.Initialise(typeof(CommonContainer), Factory);
			AssertEquals("Type is harcoded and passing parameter does not matter", typeof(ForwardingShipment), helper.BusinessObjectType);
			AssertEquals(Factory, helper.Factory);
		}

		public void TestAddFilterStrips_TextFilters()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			Factory.New<ForwardingShipment>();

			SetupCustomTextFields(shipment1, attr1: "cus value");
			SetupCustomTextFields(shipment2, attr1: "cus value", attr2: "other value");
			SetupCustomTextFields(shipment3, attr1: "val1", attr2: "val2");

			Factory.Save();

			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ModuleTextFilter)filters["Custom 1 (Custom)"];
			filter.Property = "cus";

			var query = filters.GetFilterQuery(new[] { filter });
			var shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			filter.Property = "val";
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);

			filter = (ModuleTextFilter)filters["Custom 2 (Custom)"];
			filter.Property = "other";
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);
		}

		public void TestAddFilterStrips_DateTimeFilters()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();

			var datetime = ZDateTime.Today;
			SetupCustomTextFields(shipment1, date1: datetime.AddDays(-5));
			SetupCustomTextFields(shipment2, date1: datetime.AddDays(-3), date2: datetime.AddDays(1));
			SetupCustomTextFields(shipment3, date1: datetime.AddDays(-12), date2: datetime.AddDays(2));

			Factory.Save();

			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ModuleDateFilter)filters["Date 1 (Custom)"];
			filter.PropertySearch = ModuleDateFilter.Past;

			var query = filters.GetFilterQuery(new[] { filter });
			var shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);

			filter.PropertySearch = ModuleDateFilter.Future;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			Assert(!shipments.Any());

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment4 }, shipments);

			filter.Property1 = datetime.AddDays(-10);
			filter.Property2 = datetime.AddDays(-2);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			filter = (ModuleDateFilter)filters["Date 2 (Custom)"];
			filter.PropertySearch = ModuleDateFilter.Future;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);
		}

		public void TestAddFilterStrips_FlagFilters()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();

			SetupCustomTextFields(shipment1, flag1: ZBool.True);
			SetupCustomTextFields(shipment2, flag1: ZBool.True, flag2: ZBool.True);
			SetupCustomTextFields(shipment3, flag2: ZBool.True);

			Factory.Save();

			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ModuleFlagsFilter)filters["Bool 1 (Custom)"];
			filter.Property0 = true;

			var query = filters.GetFilterQuery(new[] { filter });
			var shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			filter.Property0 = false;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3, shipment4 }, shipments);

			filter = (ModuleFlagsFilter)filters["Bool 2 (Custom)"];
			filter.Property0 = true;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);
		}

		public void TestAddFilterStrips_DecimalFilters()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();

			SetupCustomTextFields(shipment1, decimal1: 3.14m);
			SetupCustomTextFields(shipment2, decimal1: 2.71m, decimal2: 3);
			SetupCustomTextFields(shipment3, decimal2: 6.67m);

			Factory.Save();

			var filters = new ModuleFilterCollection();
			helper.AddFilterStrips(filters);

			var filter = (ModuleNumberRangeFilter)filters["Decimal 1 (Custom)"];
			filter.Property1 = 1;
			filter.Property2 = 4;

			var query = filters.GetFilterQuery(new[] { filter });
			var shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			filter.Property1 = 0;
			filter.Property2 = 1;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment3, shipment4 }, shipments);

			filter = (ModuleNumberRangeFilter)filters["Decimal 2 (Custom)"];
			filter.Property1 = 4;
			filter.Property2 = 10;
			query = filters.GetFilterQuery(new[] { filter });
			shipments = Factory.Load<ForwardingShipment>(query);
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);
		}

		public void TestAddFilterAddsCustomSuffix()
		{
			using (FreightDataRegistry.Instance.ShipmentCustomText1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("    ETD         ", "has whitespace")))
			{
				var jobShipmentBizo = new JobShipmentFilterBusinessObject();
				AssertNotNull("Custom filter should be named 'ETD (Custom)' without whitespace", jobShipmentBizo.ModuleFilters["ETD (Custom)"]);
			}
		}

		public void TestAddFilterWhenNameMatchesExistingFilter()
		{
			using (FreightDataRegistry.Instance.ShipmentCustomText1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("ETD", "Etd is a duplicate")))
			{
				var jobShipmentBizo = new JobShipmentFilterBusinessObject();
				AssertNoExceptionThrown("Should be able to handle duplicate filters", () => { _ = jobShipmentBizo.ModuleFilters; });
				AssertNotNull("Custom filter should be named 'ETD (Custom)'", jobShipmentBizo.ModuleFilters["ETD (Custom)"]);
			}
		}

		public void TestAddFilterWhenMultipleRegistyNamesMatch()
		{
			using (FreightDataRegistry.Instance.ShipmentCustomText1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("What are the chances these match", "")))
			using (FreightDataRegistry.Instance.ShipmentCustomText2.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("What are the chances these match", "")))
			using (FreightDataRegistry.Instance.ShipmentCustomDate1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("What are the chances these match", "")))
			{
				var jobShipmentBizo = new JobShipmentFilterBusinessObject();
				AssertNoExceptionThrown("Should be able to handle duplicate filters", () => { _ = jobShipmentBizo.ModuleFilters; });
			}
		}

		JobDocsAndCartage SetupCustomTextFields(ForwardingShipment shipment, string attr1 = null, string attr2 = null, ZDateTime? date1 = null, ZDateTime? date2 = null, ZBool? flag1 = null, ZBool? flag2 = null, ZDecimal? decimal1 = null, ZDecimal? decimal2 = null)
		{
			var jobDocsAndCartage = JobDocsAndCartage.New(shipment);
			jobDocsAndCartage.JP_CustomAttrib1 = attr1;
			jobDocsAndCartage.JP_CustomAttrib2 = attr2;
			jobDocsAndCartage.JP_CustomDate1 = date1 ?? ZDateTime.Empty;
			jobDocsAndCartage.JP_CustomDate2 = date2 ?? ZDateTime.Empty;
			jobDocsAndCartage.JP_CustomFlag1 = flag1 ?? ZBool.False;
			jobDocsAndCartage.JP_CustomFlag2 = flag2 ?? ZBool.False;
			jobDocsAndCartage.JP_CustomDecimal1 = decimal1 ?? ZDecimal.Zero;
			jobDocsAndCartage.JP_CustomDecimal2 = decimal2 ?? ZDecimal.Zero;

			return jobDocsAndCartage;
		}

		ShipmentRegistryCustomFieldsFilterStripHelper helper;

		protected override void SetUp()
		{
			base.SetUp();

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Custom 1", "cusom field 1"));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Custom 2", "cusom field 2"));
			FreightDataRegistry.Instance.ShipmentCustomDate1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date 1", "cusom date 1"));
			FreightDataRegistry.Instance.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date 2", "cusom date 2"));
			FreightDataRegistry.Instance.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Bool 1", "cusom flag 1"));
			FreightDataRegistry.Instance.ShipmentCustomFlag2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Bool 2", "cusom flag 2"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal 1", "cusom decimal 1"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal 2", "cusom decimal 2"));

			helper = new ShipmentRegistryCustomFieldsFilterStripHelper();
		}
	}
}
