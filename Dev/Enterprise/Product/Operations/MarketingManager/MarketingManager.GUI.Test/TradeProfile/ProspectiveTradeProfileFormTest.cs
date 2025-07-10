using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(ProspectiveTradeProfileForm))]
	public class ProspectiveTradeProfileFormTest : ZFormBasherTest
	{
		public void TestAllowNew()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			using (var form = new ProspectiveTradeProfileForm(opportunity))
			{
				IPostingButtonsProvider postingProvider = form;
				AssertEquals(false, postingProvider.AllowNew);
			}
		}

		public void TestSaveButton()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Future Industries";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_PostCode = "2015";
			header.MainAddress.OA_Address1 = "STREET1";
			var sales = header.SalesCollection.AddNew();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).PK;
			var opportunity = header.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityDescription = "Test";
			opportunity.P8_OpportunityType = "UDF";
			opportunity.P8_Stage = "UDF";
			opportunity.P8_Status = "CRT";
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);

			Factory.Save();

			using (ProspectiveTradeProfileForm form = new ProspectiveTradeProfileForm(opportunity))
			{
				form.Show();
				header.SalesCollection[0].OW_OriginID = ZGuid.Invalid;
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.SalesCollection[0].OW_OriginID = ausyd.PK;
				header.SalesCollection[0].OW_DestinationID = uslax.PK;
				header.SalesCollection[0].OW_OH_Supplier = header.PK;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (ProspectiveTradeProfileForm form = new ProspectiveTradeProfileForm(header))
			{
				form.Show();
				var salesHeaderCollection = (SalesHeaderCollection)header.ActualAndProspectiveSalesHeaderCollection;
				var tradeDetail = salesHeaderCollection[0].EntitySalesCollectionProductView[0].EntityTradeDetailsCollection.AddNew();
				tradeDetail.PA_TradeMode = "XXX";
				tradeDetail.CurrencyCode = "AUD";
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				tradeDetail.PA_TradeMode = Constants.TransportModes.Air;
				tradeDetail.PA_Status = OrgTradeDetail.TradeLaneStatus.Shipped;
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConcurrencyError_ShouldShowCorrectDialogs()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Future Industries";
			header.OH_RL_NKClosestPort = "AUSYD";
			header.MainAddress.OA_PostCode = "2015";
			header.MainAddress.OA_Address1 = "STREET1";
			var sales = header.SalesCollection.AddNew();
			sales.OW_MP_Product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment).PK;
			var opportunity = header.SalesOpportunities.AddNew();
			opportunity.P8_OpportunityDescription = "Test";
			opportunity.P8_OpportunityType = "UDF";
			opportunity.P8_Stage = "UDF";
			opportunity.P8_Status = "CRT";
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);
			Factory.Save();

			using (ProspectiveTradeProfileForm form = new ProspectiveTradeProfileForm(opportunity))
			{
				form.Show();
				header.SalesCollection[0].OW_OriginID = ZGuid.Invalid;
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.SalesCollection[0].OW_OriginID = ausyd.PK;
				header.SalesCollection[0].OW_DestinationID = uslax.PK;
				header.SalesCollection[0].OW_OH_Supplier = header.PK;

				OrgSales salesInFactory2 = new BusinessObjectFactory().Load<OrgSales>(header.SalesCollection[0].PK);
				salesInFactory2.OW_OriginID = uslax.PK;
				header.Factory.RefreshEnabled = false;
				salesInFactory2.Factory.RefreshEnabled = false;
				salesInFactory2.Factory.Save();

				form.FireSaveButton();
				AssertContains("While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.SalesCollection.AddNew();
			Factory.Save();
			return new ProspectiveTradeProfileForm(header);
		}

		#endregion
	}
}
