using Enterprise.Core;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.TaxDateDefaultingOptionLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxDateDefaultingOption))]
	class TaxDateDefaultingOptionTest : ChargeGroupSettingTest
	{
		#region Implementation

		protected new TaxDateDefaultingOption BizObj
		{
			get { return (TaxDateDefaultingOption)base.BizObj; }
			set { base.BizObj = value; }
		}

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new TaxDateDefaultingOption();

			result.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Constants.TransportModes.Air;
			result.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;
			result.Ledger = LedgerTypeAdditionalCodes.All;

			return result;
		}

		#endregion
	}

	sealed class TaxDateDefOptionValidationTest : TaxDateDefaultingOptionValidationTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new TaxDateDefaultingOption();
			}
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get
			{
				return new TaxDateDefaultingOptionCollection();
			}
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.TaxDateOption = "XYZ";
			BizObj.Ledger = "AB";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.TaxDateOptionInfo);
			AssertHasErrors(BizObj.LedgerInfo);
		}
	}

	sealed class TaxDateDefOptionLookupsTest : TaxDateDefaultingOptionLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new TaxDateDefaultingOption();
			}
		}
	}
}
