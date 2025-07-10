using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	class JobNumberHelperTest : TestCaseWithFactory
	{
		public void TestInstance() => AssertType<JobNumberHelper>(JobNumberHelper);

		public void TestGetJobNumberCustomisationRegistries()
		{
			var expectedValues = new Dictionary<string, BillCustomisationRegistryItem> {
				{ "S", FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation }
				, { "(B|G)", ObjectFactory.Get<ICustomsDataRegistry>().DeclarationNumberCustomisation as BillCustomisationRegistryItem }
				, { "C", ObjectFactory.Get<Enterprise.Integration.Freight.IFreightConfigurationRegistry>().ConsolNumberCustomisation as BillCustomisationRegistryItem }
			};
			Assert("PreCondtiion", expectedValues.Values.All(x => x != null));

			CombineAssertions(() => {
				var actualValue = JobNumberHelper.GetJobNumberCustomisationRegistries();
				AssertEquals("Item number", expectedValues.Count, actualValue.Count);
				actualValue.ForEach(x => {
					var expectedValue = expectedValues.TryGetValue(x.Prefix, out var expectedRegistryItem)
						? expectedRegistryItem
						: null;
					AssertNotNull($"[JobType:{x.Prefix}] Registry item", expectedValue);
					AssertEquals($"[JobType:{x.Prefix}] Registry item", expectedValue, x.Registry);
				});
			});
		}

		public void TestRegExOutputForRealCases()
		{
			var customisation = GetNumberCustomisation(FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation
				, (BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, "8"));
			customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;

			var regExPattern = JobNumberHelper.GetJobNumberRegEx(Env.NumberFountains.JobShipmentNumber.Prefix, customisation);
			AssertEquals("PreCondition"
				, @"(^S|[\s]S)(([0-9]{8}[\.\,]?[\s])|([0-9]{8}[\.\,]?)$)"
				, regExPattern
			);

			var regEx = new Regex(regExPattern);

			var jobNumsShouldBeMatched = new[] {
				"S00001001"
				, "\tS00001001"
				, "S00001001\t"
				, "\tS00001001\t"
				, " S00001001"
				, "S00001001 "
				, " S00001001 "
				, "aaaa S00001001 aaaa"
				, "This is a test of a Shipment Number S00001001"
				, "This is a test of another Shipment Number\tS00001001\there"
				, "And another S00001001."
				, "And another S00001001. This is another words."
				, "And another S00001001, this is second part of line."
			};
			jobNumsShouldBeMatched.ForEach(x => {
				var matchedResult = regEx.Match(x);
				CombineAssertions($"Input string:\"{x}\"", () => {
					Assert("Is Matched", matchedResult.Success);
					AssertEquals("Extracted Result", "S00001001", matchedResult.Value.Trim().Replace(".", "").Replace(",", ""));
				});
			});

			var jobNumsShouldNotBeMatched = new[] {
				"SDAU00001001"
				,"ShouldnotmatchnospaceS00001001here"
				,"ShouldnotmatchnospaceS00001001 here"
				, "\r\nShouldnotmatchnospace S00001001here."
				, "A/S00001001"
				, "S00001001.0"
			};
			Assert(jobNumsShouldNotBeMatched.All(x => !regEx.Match(x).Success));
		}

		public void TestGetJobNumberRegEx()
		{
			var customisation = GetNumberCustomisation(FreightDataRegistry.Instance.HouseBillShipmentNumberCustomisation
				, (BillOfLadingNumberCustomisationElement.Keys.ClientCoded1, "CV1")
				, (BillOfLadingNumberCustomisationElement.Keys.ClientCoded2, "CV2")
				, (BillOfLadingNumberCustomisationElement.Keys.ClientCoded3, "CV3")
				, (BillOfLadingNumberCustomisationElement.Keys.MonthAs2Digits, null)
				, (BillOfLadingNumberCustomisationElement.Keys.YearAsDigit, "2")
				, (BillOfLadingNumberCustomisationElement.Keys.DestinationIATA, null)
				, (BillOfLadingNumberCustomisationElement.Keys.SequenceNumber, null));

			customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.None;
			AssertEquals("RegEx for shipment number registry"
				, @"(^S|[\s]S)((CV1CV2CV3(0[1-9]|1[0-2])[0-9]{2}[0-9a-zA-Z]{,3}[0-9]{8}[\.\,]?[\s])|(CV1CV2CV3(0[1-9]|1[0-2])[0-9]{2}[0-9a-zA-Z]{,3}[0-9]{8}[\.\,]?)$)"
				, JobNumberHelper.GetJobNumberRegEx(Env.NumberFountains.JobShipmentNumber.Prefix, customisation)
			);

			customisation.CheckDigitAlgorithm = CheckDigitAlgorithmList.Codes.Standard;
			AssertEquals("RegEx for shipment number registry"
				, @"(^S|[\s]S)((CV1CV2CV3(0[1-9]|1[0-2])[0-9]{2}[0-9a-zA-Z]{,3}[0-9]{8}[0-9a-zA-Z]{1}[\.\,]?[\s])|(CV1CV2CV3(0[1-9]|1[0-2])[0-9]{2}[0-9a-zA-Z]{,3}[0-9]{8}[0-9a-zA-Z]{1}[\.\,]?)$)"
				, JobNumberHelper.GetJobNumberRegEx(Env.NumberFountains.JobShipmentNumber.Prefix, customisation)
			);
		}

		public void TestGetGenericJobTypeRegEx()
		{
			var expectedPrefix = "(CB|CN|CR|CST|DC|DI|H|I|ISF|L|PAI|RC|RO|RS|SC|SK|T|TB|TD|TPU|TR|U|V|VA|W|WI|WV)";
			var expectedRegExRule = "([0-9]{8})[\\.\\,]?";
			var expectedRegex = $"(^{expectedPrefix}|[\\s]{expectedPrefix})(({expectedRegExRule}[\\s])|({expectedRegExRule})$)";

			AssertEquals(expectedRegex, JobNumberHelper.GetGenericJobTypeRegEx());
		}

		BillOfLadingNumberCustomisation GetNumberCustomisation(
			BillCustomisationRegistryItem registryItem
			, params (string ElelmentKey, string Detail)[] enabledSettings)
		{
			var customisation = registryItem.DefaultValue;

			foreach (BillOfLadingNumberCustomisationElement element in customisation.UnFilteredElements)
			{
				element.Include = false;
			}

			for (var index = 0; index < enabledSettings.Length; index++)
			{
				var enabledSetting = enabledSettings[index];
				EnableCustomisationElement(index + 1
					, customisation.UnFilteredElements[enabledSetting.ElelmentKey]
					, enabledSetting.Detail);
			}

			return customisation;
		}

		void EnableCustomisationElement(int order, BillOfLadingNumberCustomisationElement element, string value = null)
		{
			element.Include = true;
			element.Order = (byte)order;
			if (value != null)
			{
				element.Detail = value;
			}
		}

		public IJobNumberHelper JobNumberHelper => jobNumberHelper ?? (jobNumberHelper = ObjectFactory.Get<IJobNumberHelper>());
		IJobNumberHelper jobNumberHelper;
	}
}