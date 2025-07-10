using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefShippingLineFilterBusinessObject))]
	sealed class RefShippingLineFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestNameFilter()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Dummy Shipping Line";
			AssertForTextFilter("Name", "Dummy Name", "Dummy Shipping Line");
		}

		public void TestSCACFilter()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "WOWO";
			AssertForTextFilter("SCAC", "DUMM", "WOWO");
		}

		public void TestC1CFilter()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "CW1C";
			AssertForTextFilter("C1C", "DUMM", "CW1C");
		}

		public void TestSCACOrC1CFilter()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "WOWO";
			shippingLine.RSL_CargoWiseOneCode = "CW1C";
			AssertForTextFilter("SCAC or C1C", "DUMM", "WOWO");
			AssertForTextFilter("SCAC or C1C", "DUMM", "CW1C");
		}

		public void TestIntegrationsEnabledFilter()
		{
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_OceanCarrierMessagingAvailable = v, (u, v) => u.Property0 = v, u => u.FlagNames[0] == "Ocean Carrier Messaging");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_BookingRequestAvailable = v, (u, v) => u.Property1 = v, u => u.FlagNames[1] == "Booking Request");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_ShippingInstructionAvailable = v, (u, v) => u.Property2 = v, u => u.FlagNames[2] == "Shipping Instruction");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_VerifiedGrossContainerWeightAvailable = v, (u, v) => u.Property3 = v, u => u.FlagNames[3] == "Verified Gross Container Weight");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_ShippingOrderAvailable = v, (u, v) => u.Property4 = v, u => u.FlagNames[4] == "Shipping Order (China)");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_EManifestAvailable = v, (u, v) => u.Property5 = v, u => u.FlagNames[5] == "eManifest (China)");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_GlobalSailingScheduleAvailable = v, (u, v) => u.Property6 = v, u => u.FlagNames[6] == "Global Sailing Schedule");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_ContainerAutomationAvailable = v, (u, v) => u.Property7 = v, u => u.FlagNames[7] == "Container Automation");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_CargoSphereRatesAvailable = v, (u, v) => u.Property8 = v, u => u.FlagNames[8] == "Cargo Sphere Rates");
			AssertIntegrationsEnabledFilterResult((u, v) => u.RSL_InvoiceAvailable = v, (u, v) => u.Property9 = v, u => u.FlagNames[9] == "Invoice");
		}

		public void TestNVOStatusFilter()
		{
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_IsNVO = true;
			shippingLine1.RSL_IsShippingLine = false;
			shippingLine1.RSL_CargoWiseOneCode = "DMM1";

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_IsNVO = false;
			shippingLine2.RSL_IsShippingLine = true;
			shippingLine2.RSL_CargoWiseOneCode = "DMM2";

			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine3.RSL_IsNVO = true;
			shippingLine3.RSL_IsShippingLine = true;
			shippingLine3.RSL_CargoWiseOneCode = "DMM3";

			var shippingLine4 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine4.RSL_IsNVO = false;
			shippingLine4.RSL_IsShippingLine = false;
			shippingLine4.RSL_CargoWiseOneCode = "DMM4";

			var filter = (RefShippingLineFilterBusinessObject)GetNewFilterStripBusinessObject();
			var cw1cFilter = filter["C1C"] as ModuleTextFilter;
			AssertNotNull(cw1cFilter);

			cw1cFilter.IsActive = true;
			cw1cFilter.Property = "DMM";
			cw1cFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			var nvoStatusFilter = filter["NVO Status"] as ModuleTextFilter;
			AssertNotNull(nvoStatusFilter);
			AssertEquals("All", nvoStatusFilter.DefaultProperty);
			AssertEquals("All", nvoStatusFilter.Property);
			AssertEquals("NVO Status", nvoStatusFilter.Description);
			AssertEquals(FilterCategories.StatusAndFlags, nvoStatusFilter.Category);

			nvoStatusFilter.IsActive = true;
			nvoStatusFilter.Property = "NVO";

			var shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shippingLine1.PK, shippingLine3.PK }, shippingLineCollection.Select(u => u.PK));

			nvoStatusFilter.Property = "Not NVO";
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shippingLine2.PK, shippingLine3.PK }, shippingLineCollection.Select(u => u.PK));

			nvoStatusFilter.Property = "All";
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shippingLine1.PK, shippingLine2.PK, shippingLine3.PK, shippingLine4.PK }, shippingLineCollection.Select(u => u.PK));
		}

		public void TestMessagingRequirementsFilter()
		{
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();

			var messagingRequirement = shippingLine1.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory;

			messagingRequirement = shippingLine1.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory;

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();

			messagingRequirement = shippingLine2.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory;

			var shippingLine3 = Factory.NewWithValidTestData<RefShippingLine>();

			messagingRequirement = shippingLine3.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory;

			var shippingLine4 = Factory.NewWithValidTestData<RefShippingLine>();

			messagingRequirement = shippingLine4.ShippingLineMessagingRequirements.AddNew();
			messagingRequirement.RSR_RST_NKType = Core.Constants.ShippingLineMessagingRequirement.Code.AcceptEitherAirflowOrHumidity;

			var shippingLine5 = Factory.NewWithValidTestData<RefShippingLine>();

			Factory.Save();

			var filter = (RefShippingLineFilterBusinessObject)GetNewFilterStripBusinessObject();
			var messagingRequirementsFilter = filter["Messaging Requirements"] as ModuleTextFilter;
			AssertNotNull(messagingRequirementsFilter);
			AssertNullOrEmpty(messagingRequirementsFilter.DefaultProperty);
			AssertNullOrEmpty(messagingRequirementsFilter.Property);
			AssertEquals("Messaging Requirements", messagingRequirementsFilter.Description);
			AssertEquals(FilterCategories.StatusAndFlags, messagingRequirementsFilter.Category);

			AssertEquals(10, messagingRequirementsFilter.List.Count);
			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory,
				Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory,
				Core.Constants.ShippingLineMessagingRequirement.Code.DGNetWeightMandatory,
				Core.Constants.ShippingLineMessagingRequirement.Code.AcceptEitherAirflowOrHumidity,
				Core.Constants.ShippingLineMessagingRequirement.Code.DimensionsMandatoryForOOG,
				Core.Constants.ShippingLineMessagingRequirement.Code.HarmonisedCode,
				Core.Constants.ShippingLineMessagingRequirement.Code.BillOfLadingProvider,
				Core.Constants.ShippingLineMessagingRequirement.Code.AttachFormAsPDFInMessage,
				Core.Constants.ShippingLineMessagingRequirement.Code.SealNumberMandatory,
				Core.Constants.ShippingLineMessagingRequirement.Code.IntegrationViaEmailToCarrierLocalOffice,
			}, (messagingRequirementsFilter.List as CodeDescriptionPairList).GetAllCodes());

			messagingRequirementsFilter.IsActive = true;
			messagingRequirementsFilter.Property = Core.Constants.ShippingLineMessagingRequirement.Code.ContractNumberMandatory;
			var shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shippingLine1.PK, shippingLine2.PK }, shippingLineCollection.Select(u => u.PK));

			messagingRequirementsFilter.Property = Core.Constants.ShippingLineMessagingRequirement.Code.NamedAccountMandatory;
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shippingLine1.PK, shippingLine3.PK }, shippingLineCollection.Select(u => u.PK));

			messagingRequirementsFilter.Property = Core.Constants.ShippingLineMessagingRequirement.Code.AcceptEitherAirflowOrHumidity;
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shippingLine4.PK }, shippingLineCollection.Select(u => u.PK));

			messagingRequirementsFilter.Property = Core.Constants.ShippingLineMessagingRequirement.Code.AttachFormAsPDFInMessage;
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);

			AssertEquals(0, shippingLineCollection.Select(u => u.PK).Count());

			messagingRequirementsFilter.Property = ZString.Empty;
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(5, shippingLineCollection.Select(u => u.PK).Count());
		}

		#region Implementation

		void AssertIntegrationsEnabledFilterResult(Action<RefShippingLine, bool> setAvailableField, Action<ModuleFlagsFilter, bool> setFlagsFilterProperty, Func<ModuleFlagsFilter, bool> assertFlagName)
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Dummy Shipping Line";
			setAvailableField(shippingLine, false);

			var filter = (RefShippingLineFilterBusinessObject)GetNewFilterStripBusinessObject();
			var nameFilter = filter["Name"] as ModuleTextFilter;
			var flagsFilter = filter["Integrations Enabled"] as ModuleFlagsFilter;
			AssertNotNull(nameFilter);
			AssertNotNull(flagsFilter);
			AssertEquals("Integrations Enabled", flagsFilter.Description);
			AssertEquals(true, assertFlagName(flagsFilter));

			nameFilter.IsActive = true;
			nameFilter.Property = "Dummy Shipping Line";

			flagsFilter.IsActive = true;
			setFlagsFilterProperty(flagsFilter, false);

			var shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(1, shippingLineCollection.Count);

			setFlagsFilterProperty(flagsFilter, true);
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(0, shippingLineCollection.Count);

			setAvailableField(shippingLine, true);
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(1, shippingLineCollection.Count);

			shippingLine.Delete();
		}

		void AssertForTextFilter(string filterName, string filterValueForNotFound, string filterValueForFound)
		{
			var filter = (RefShippingLineFilterBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = filter[filterName] as ModuleTextFilter;
			AssertNotNull(textFilter);
			AssertEquals(filterName, textFilter.Description);

			textFilter.IsActive = true;
			textFilter.Property = filterValueForNotFound;

			var shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(0, shippingLineCollection.Count);

			textFilter.IsActive = true;
			textFilter.Property = filterValueForFound;
			shippingLineCollection = new RefShippingLineCollection(Factory, filter.Filter);
			AssertEquals(1, shippingLineCollection.Count);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefShippingLineFilterBusinessObject();
		}

		#endregion
	}
}
