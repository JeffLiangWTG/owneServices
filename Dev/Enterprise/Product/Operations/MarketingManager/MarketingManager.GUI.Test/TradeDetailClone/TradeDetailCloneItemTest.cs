using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCloneItem))]
	public class TradeDetailCloneItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();

			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 100m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = "KG";
			detail.CurrentProspectPeriod.PAS_Volume = 5m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = "M3";
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_EstimatedProfit = 2000m;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			var item = new TradeDetailCloneItem(detail);

			CombineAssertions(() =>
			{
				AssertEquals("Recurrence type", OrgTradeProspectRecurrenceTypeList.Codes.OneOff, item.RecurrenceType);
				AssertEquals("Container type", "20GP", item.ContainerType);
				AssertEquals("Container count", 2L, item.ContainerCount);
				AssertEquals("Weight", 100m, item.Weight);
				AssertEquals("Weight UQ", "KG", item.WeightUQ);
				AssertEquals("Volume", 5m, item.Volume);
				AssertEquals("Volume UQ", "M3", item.VolumeUQ);
				AssertEquals("Rate offered", 500m, item.RateOffered);
				AssertEquals("Job count", 4m, item.JobCount);
				AssertEquals("Estimated value", 2000m, item.EstimatedValue);
				AssertEquals("Currency", "USD", item.Currency);
			});
		}

		public void TestSelected()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();

			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 100m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = "KG";
			detail.CurrentProspectPeriod.PAS_Volume = 5m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = "M3";
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_EstimatedProfit = 2000m;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			var item = new TradeDetailCloneItem(detail);

			CombineAssertions(() =>
			{
				AssertEquals("Origin Recurrence type", OrgTradeProspectRecurrenceTypeList.Codes.OneOff, item.RecurrenceType);
				AssertEquals("Origin Container type", "20GP", item.ContainerType);
				AssertEquals("Origin Container count", 2L, item.ContainerCount);
				AssertEquals("Origin Weight", 100m, item.Weight);
				AssertEquals("Origin Weight UQ", "KG", item.WeightUQ);
				AssertEquals("Origin Volume", 5m, item.Volume);
				AssertEquals("Origin Volume UQ", "M3", item.VolumeUQ);
				AssertEquals("Origin Rate offered", 500m, item.RateOffered);
				AssertEquals("Origin Job count", 4m, item.JobCount);
				AssertEquals("Origin Estimated value", 2000m, item.EstimatedValue);
				AssertEquals("Origin Currency", "USD", item.Currency);
			});

			item.Selected = true;
			item.RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			item.ContainerType = "40GP";
			item.ContainerCount = 5L;
			item.Weight = 0m;
			item.WeightUQ = "";
			item.Volume = 0m;
			item.VolumeUQ = "";
			item.RateOffered = 0m;
			item.JobCount = 1m;
			item.EstimatedValue = 1000m;
			item.Currency = "AUD";

			item.Selected = false;
			CombineAssertions(() =>
			{
				AssertEquals("Reset to origin Recurrence type", OrgTradeProspectRecurrenceTypeList.Codes.OneOff, item.RecurrenceType);
				AssertEquals("Reset to origin Container type", "20GP", item.ContainerType);
				AssertEquals("Reset to origin Container count", 2L, item.ContainerCount);
				AssertEquals("Reset to origin Weight", 100m, item.Weight);
				AssertEquals("Reset to origin Weight UQ", "KG", item.WeightUQ);
				AssertEquals("Reset to origin Volume", 5m, item.Volume);
				AssertEquals("Reset to origin Volume UQ", "M3", item.VolumeUQ);
				AssertEquals("Reset to origin Rate offered", 500m, item.RateOffered);
				AssertEquals("Reset to origin Job count", 4m, item.JobCount);
				AssertEquals("Reset to origin Estimated value", 2000m, item.EstimatedValue);
				AssertEquals("Reset to origin Currency", "USD", item.Currency);
			});
		}

		public void TestTotalEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();

			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 100m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_Volume = 5m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 2;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			AssertEquals("Precondition", OrgTradeProspectRecurrenceTypeList.Codes.OneOff, detail.ProspectDetail.PAP_RecurrenceType);
			AssertEquals("Precondition", 2m, detail.CurrentProspectPeriod.PAS_RepeatsMnth);
			AssertEquals("Precondition", 500m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);
			AssertEquals("Precondition", 1000m, detail.PA_Calc_EstimatedAnnualValue);

			var item = new TradeDetailCloneItem(detail);

			AssertEquals("Should perform the same as an active trade detail", 1000m, item.TotalEstimatedValue);

			item.RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Monthly;
			AssertEquals("Should perform the same as an active trade detail", 12000m, item.TotalEstimatedValue);
			item.RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Weekly;
			AssertEquals("Should perform the same as an active trade detail", 52000m, item.TotalEstimatedValue);

			item.RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			item.JobCount = 0;
			AssertEquals("Should perform the same as an active trade detail", 500m, item.TotalEstimatedValue);
			item.JobCount = 4;
			AssertEquals("Should perform the same as an active trade detail", 2000m, item.TotalEstimatedValue);

			item.JobCount = 2;
			item.EstimatedValue = 250m;
			AssertEquals("Should perform the same as an active trade detail", 500m, item.TotalEstimatedValue);
		}

		public void TestSetRateOfferedShouldRecalculateEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();

			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 100m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_Volume = 5m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			AssertEquals("Precondition", false, detail.IsFreight);
			AssertEquals("Precondition", true, detail.CurrentProspectPeriod.ShouldDefaultProperties);
			AssertEquals("Precondition", 500m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_RateOffered = 250m;
			AssertEquals("Estimated value is recalculated when PAS_RateOffered is changed", 250m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;

			var item = new TradeDetailCloneItem(detail);

			AssertEquals("Precondition", 500m, item.EstimatedValue);

			item.RateOffered = 250m;
			AssertEquals("Estimated value should be recalculated when RateOffered is changed - it should match the behaviour of the normal trade detail above", 250m, item.EstimatedValue);
		}

		public void TestSetContainerCountShouldRecalculateEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			sales.OW_MP_Product = product.PK;

			detail.PA_TradeMode = Enterprise.Core.Constants.TransportModes.Sea;
			detail.PA_TradeType = Enterprise.Core.Constants.ContainerModes.FCL;
			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 100m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_Volume = 5m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			detail.CurrentProspectPeriod.PAS_RateOffered = 500m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			AssertEquals("Precondition", true, detail.IsFreight);
			AssertEquals("Precondition", true, detail.IsSeaFcl);
			AssertEquals("Precondition", true, detail.CurrentProspectPeriod.ShouldDefaultProperties);
			AssertEquals("Precondition", 1000m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_Units = 4;
			AssertEquals("Estimated value is recalculated when PAS_Units is changed", 2000m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);
			detail.CurrentProspectPeriod.PAS_Units = 2;

			var item = new TradeDetailCloneItem(detail);

			AssertEquals("Precondition", 1000m, item.EstimatedValue);

			item.ContainerCount = 4;
			AssertEquals("Estimated value should be recalculated when ContainerCount is changed - it should match the behaviour of the normal trade detail above", 2000m, item.EstimatedValue);
		}

		public void TestSetWeightOrVolumeShouldRecalculateEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			sales.OW_MP_Product = product.PK;

			detail.PA_TradeMode = Enterprise.Core.Constants.TransportModes.Air;
			detail.PA_TradeType = Enterprise.Core.Constants.ContainerModes.LCL;
			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 0.1m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_Volume = 0.1m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			detail.CurrentProspectPeriod.PAS_RateOffered = 12m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			AssertEquals("Precondition", true, detail.IsFreight);
			AssertEquals("Precondition", false, detail.IsSeaFcl);
			AssertEquals("Precondition", true, detail.CurrentProspectPeriod.ShouldDefaultProperties);
			AssertEquals("Precondition", 200.004m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_Weight = 20m;
			AssertEquals("Estimated value is recalculated when PAS_Weight is changed", 240m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);
			detail.CurrentProspectPeriod.PAS_Volume = 0.2m;
			AssertEquals("Estimated value is recalculated when PAS_Volume is changed", 399.996m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_Weight = 0.1m;
			detail.CurrentProspectPeriod.PAS_Volume = 0.1m;
			AssertEquals("Precondition", 200.004m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			var item = new TradeDetailCloneItem(detail);

			AssertEquals("Precondition", 200.004m, item.EstimatedValue);

			item.Weight = 20m;
			AssertEquals("Estimated value should be recalculated when Weight is changed - it should match the behaviour of the normal trade detail above", 240m, item.EstimatedValue);
			item.Volume = 0.2m;
			AssertEquals("Estimated value should be recalculated when Volume is changed - it should match the behaviour of the normal trade detail above", 399.996m, item.EstimatedValue);
		}

		public void TestSetWeightUQOrVolumeUQShouldRecalculateEstimatedValue()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			sales.OW_MP_Product = product.PK;

			detail.PA_TradeMode = Constants.TransportModes.Air;
			detail.PA_TradeType = Constants.ContainerModes.LCL;
			detail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.OneOff;
			detail.ProspectDetail.PAP_RC_NKContainer = "20GP";
			detail.CurrentProspectPeriod.PAS_Units = 2;
			detail.CurrentProspectPeriod.PAS_Weight = 0.1m;
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_Volume = 0.1m;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			detail.CurrentProspectPeriod.PAS_RateOffered = 12m;
			detail.CurrentProspectPeriod.PAS_RepeatsMnth = 4;
			detail.CurrentProspectPeriod.PAS_RX_NKCurrency = "USD";

			AssertEquals("Precondition", true, detail.IsFreight);
			AssertEquals("Precondition", false, detail.IsSeaFcl);
			AssertEquals("Precondition", true, detail.CurrentProspectPeriod.ShouldDefaultProperties);
			AssertEquals("Precondition", 200.004m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicDecimetres;
			AssertEquals("Estimated value is recalculated when PAS_VolumeUQ is changed", 1.2m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);
			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Tonnes;
			AssertEquals("Estimated value is recalculated when PAS_WeightUQ is changed", 1200m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			detail.CurrentProspectPeriod.PAS_WeightUQ = Constants.Weight.Kilograms;
			detail.CurrentProspectPeriod.PAS_VolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Precondition", 200.004m, detail.CurrentProspectPeriod.PAS_EstimatedProfit);

			var item = new TradeDetailCloneItem(detail);

			AssertEquals("Precondition", 200.004m, item.EstimatedValue);

			item.VolumeUQ = Constants.Volume.CubicDecimetres;
			AssertEquals("Estimated value should be recalculated when VolumeUQ is changed - it should match the behaviour of the normal trade detail above", 1.2m, item.EstimatedValue);
			item.WeightUQ = Constants.Weight.Tonnes;
			AssertEquals("Estimated value should be recalculated when WeightUQ is changed - it should match the behaviour of the normal trade detail above", 1200m, item.EstimatedValue);
		}

		public void TestValidation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();
			var item = new TradeDetailCloneItem(detail);
			item.Selected = true;

			AssertNumberNotNegative(item.ContainerCountInfo);
			AssertNumberNotNegative(item.WeightInfo);
			AssertNumberNotNegative(item.VolumeInfo);
			AssertNumberNotNegative(item.RateOfferedInfo);
			AssertNumberNotNegative(item.JobCountInfo);
			AssertNumberNotNegative(item.EstimatedValueInfo);

			AssertValidValue(item.RecurrenceTypeInfo, false, OrgTradeProspectRecurrenceTypeList.Codes.Monthly);
			AssertValidValue(item.ContainerTypeInfo, true, "20GP");
			AssertValidValue(item.WeightUQInfo, true, "KG");
			AssertValidValue(item.VolumeUQInfo, true, "M3");
			AssertValidValue(item.CurrencyInfo, false, "AUD");
		}

		void AssertNumberNotNegative(ZPropertyInfo info)
		{
			SetValue(info, 1m);
			AssertNoErrors(info);

			SetValue(info, -1m);

			string prefix = Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName);
			AssertHasError(info, String.Format("Please enter {0}'{1}' greater than or equal to 0.", prefix, info.HumanReadableName));
		}

		void SetValue(ZPropertyInfo info, decimal value)
		{
			if (info.Value is ZDecimal)
			{
				info.Value = (ZDecimal)value;
			}
			else if (info.Value is ZLong)
			{
				info.Value = (ZLong)value;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		void AssertValidValue(ZPropertyInfo info, bool emptyEnabled, ZString validValue)
		{
			info.Value = (ZString)"##";
			Assert(info.HasError("Enter a valid selection.") || info.HasError("Enter a valid code.") || info.HasError(String.Format("Enter a valid {0}.", info.Description)));

			info.Value = ZString.Empty;
			if (emptyEnabled)
			{
				AssertNoErrors(info);
			}
			else
			{
				Assert(info.HasError("Please enter a value.") || info.HasError(String.Format("Please enter a {0}.", info.Description)));
			}

			info.Value = validValue;
			AssertNoErrors(info);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var detail = sales.TradeDetails.AddNew();
			return new TradeDetailCloneItem(detail);
		}
	}
}
