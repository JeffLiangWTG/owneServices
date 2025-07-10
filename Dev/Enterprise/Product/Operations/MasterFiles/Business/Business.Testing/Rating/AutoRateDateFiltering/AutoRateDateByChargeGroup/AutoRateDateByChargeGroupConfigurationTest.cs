using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AutoRateDateByChargeGroupConfiguration))]
	sealed class AutoRateDateByChargeGroupConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestGetAutoRateDates()
		{
			var config = new AutoRateDateByChargeGroupConfiguration();

			var autoRateDateByChargeGroup1 = config.AutoRateDateByChargeGroups
				.Cast<AutoRateDateByChargeGroup>()
				.Single(x => x.ChargeGroup == ChargeCodeGroupList.Codes.CFSShipment);

			var chargeGroupSetting1 = autoRateDateByChargeGroup1.ChargeGroupSettings.AddNew();
			chargeGroupSetting1.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			chargeGroupSetting1.DirectionCode = FreightShipmentDirection.Code.Export;
			chargeGroupSetting1.Mode = RateMode.SEA;
			chargeGroupSetting1.DateType = JobDateTypes.Codes.ArrivalDate;

			AssertContainsExactElementsInAnyOrder
			(
				new[] { "FCN-EXP-SEA-ARV" },
				((IAutoRateDateByChargeGroupConfiguration)config).GetAutoRateDates(ChargeCodeGroupList.Codes.CFSShipment).Select(x => $"{x.JobType}-{x.DirectionCode}-{x.Mode}-{x.DateType}")
			);
		}

		public void TestNonApplicableChargeGroupsExcluded()
		{
			var config = new AutoRateDateByChargeGroupConfiguration();

			var expectedChargeGroups = new ChargeCodeGroupList();
			expectedChargeGroups.RemoveCode(ChargeCodeGroupList.Codes.CustomsDuty);
			expectedChargeGroups.RemoveCode(ChargeCodeGroupList.Codes.NonJobRelated);
			expectedChargeGroups.RemoveCode(ChargeCodeGroupList.Codes.NotGrouped);

			var expectedCodes = expectedChargeGroups.Cast<CodeDescriptionPair>().Select(x => x.Code);
			var actualCodes = config.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>().Select(x => x.ChargeGroup.ToString());

			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new AutoRateDateByChargeGroupConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AutoRateDateByChargeGroupConfiguration();
		}
	}
}
