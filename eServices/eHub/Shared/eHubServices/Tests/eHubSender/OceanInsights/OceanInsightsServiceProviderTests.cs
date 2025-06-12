using System.IO;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider;
using CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.USDIS
{
	[TestClass]
	public class OceanInsightsServiceProviderTests : BaseTest
	{
		[TestMethod]
		public void OceanInsightsServiceProvider_BookingReference_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionCarrierBookingReference.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CBR12345555\",\"request_carrier_code\":\"OOLU\",\"request_type\":\"b_id\",\"descriptive_name\":\"1e5717a3-f3bc-413a-a73e-0a3ff19d73f0\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "OOLU";
			configuration.Subscription.RequestKey = "CBR12345555";
			configuration.Subscription.RequestType = "b_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'OOCL','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'OOLU401862065','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_BookingReference_Short_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.ProviderSubscriptionCBR.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>00000000-0000-0000-0000-000000000001</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CARRIERSBOOKINGREFERENCE1\",\"request_carrier_code\":\"COD1\",\"request_type\":\"b_id\",\"descriptive_name\":\"00000000-0000-0000-0000-000000000001\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "COD1";
			configuration.Subscription.RequestKey = "CARRIERSBOOKINGREFERENCE1";
			configuration.Subscription.RequestType = "b_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'COD1','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'CARRIERSBOOKINGREFERENCE1','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_BookingReference_Error()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionCarrierBookingReference.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - Error this is error.</Reference></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CBR12345555\",\"request_carrier_code\":\"OOLU\",\"request_type\":\"b_id\",\"descriptive_name\":\"1e5717a3-f3bc-413a-a73e-0a3ff19d73f0\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "OOLU";
			configuration.Subscription.RequestKey = "CBR12345555";
			configuration.Subscription.RequestType = "b_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Error, "Error this is error.");

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_MasterBillNumber_Short_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.ProviderSubscriptionMBN.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>00000000-0000-0000-0000-000000000001</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"MBOLNUMBER1\",\"request_carrier_code\":\"COD1\",\"request_type\":\"m_bl\",\"descriptive_name\":\"00000000-0000-0000-0000-000000000001\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "COD1";
			configuration.Subscription.RequestKey = "MBOLNUMBER1";
			configuration.Subscription.RequestType = "m_bl";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'COD1','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'MBOLNUMBER1','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_EmptySubscriptionType()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionEmptySubscriptionType.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.ReplyText = @"<ClientDeliveryNotification xmlns=""http://cargowise.com/ehub/product/2013/04""><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - SubscriptionType is empty in ContextCollection. Subscription rejected.</Reference></ClientDeliveryNotification>";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_SubscriptionTypeNotInTheList()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionTypeNotInTheList.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.ReplyText = @"<ClientDeliveryNotification xmlns=""http://cargowise.com/ehub/product/2013/04""><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - SubscriptionType is not one of allowed values: CarrierBookingReference, MasterBillNumber, ContainerNumber. Subscription rejected.</Reference></ClientDeliveryNotification>";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_EmptyEventType()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionEmptyEventType.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.ReplyText = @"<ClientDeliveryNotification xmlns=""http://cargowise.com/ehub/product/2013/04""><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey /><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - Message is not a subscription. Event Type should be 'SBR'.</Reference></ClientDeliveryNotification>";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_EventTypeNotSBR()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionEventTypeNotSBR.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.ReplyText = @"<ClientDeliveryNotification xmlns=""http://cargowise.com/ehub/product/2013/04""><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey /><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - Message is not a subscription. Event Type should be 'SBR'.</Reference></ClientDeliveryNotification>";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_BookingReferenceType_BookingReferenceIsEmpty()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionCarrierBookingReferenceEmpty.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.ReplyText = @"<ClientDeliveryNotification xmlns=""http://cargowise.com/ehub/product/2013/04""><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>REJ</EventTypeCode><Reference>Message Processing Error - SubscriptionType = 'CarriersBookingReference',  CarriersBookingReference is empty in ContextCollection. Subscription rejected.</Reference></ClientDeliveryNotification>";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();

			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}



		[TestMethod]
		public void OceanInsightsServiceProvider_ContainerNumber_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionContainerNumber.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CON7772066\",\"request_carrier_code\":\"OOLU\",\"request_type\":\"c_id\",\"descriptive_name\":\"1e5717a3-f3bc-413a-a73e-0a3ff19d73f0\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "OOLU";
			configuration.Subscription.RequestKey = "CON7772066";
			configuration.Subscription.RequestType = "c_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'OOCL','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'OOLU401862065','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");


			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_ContainerNumber_Short_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.ProviderSubscriptionCON.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>00000000-0000-0000-0000-000000000001</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CONTAINER1\",\"request_carrier_code\":\"COD1\",\"request_type\":\"c_id\",\"descriptive_name\":\"00000000-0000-0000-0000-000000000001\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "COD1";
			configuration.Subscription.RequestKey = "CONTAINER1";
			configuration.Subscription.RequestType = "c_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'OOCL','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'OOLU401862065','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");


			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}

		[TestMethod]
		public void OceanInsightsServiceProvider_ContainerNumberAndCarrierBookingReference_Success()
		{
			string messageString = new StreamReader(GetEmbeddedResource("eHubSender.OceanInsights.TestFiles.SubscriptionContainerNumberAndCarrierBookingReference.xml")).ReadToEnd();
			var configuration = new OceanInsightsServiceProviderTest.Configuration();
			configuration.ApiBaseUrl = "http://capi.ocean-insights.com/containertracking/v2/";
			configuration.AuthorizationToken = "d8f02f25b28f31b95856d4ce6ca2cb02e8f90690";
			configuration.ReplyText = "<ClientDeliveryNotification xmlns=\"http://cargowise.com/ehub/product/2013/04\"><SenderId>OceanInsights</SenderId><RecepientId>HYEDAUIKB</RecepientId><TargetBOType>Container Event Subscription</TargetBOType><TargetBOKey>1e5717a3-f3bc-413a-a73e-0a3ff19d73f0</TargetBOKey><EventTypeCode>CRT</EventTypeCode><Reference>Subscription created</Reference><ContextCollection><Context><Type>SubscriptionId</Type><Value>5785</Value></Context></ContextCollection></ClientDeliveryNotification>";
			configuration.DataToSendString = "{\"request_key\":\"CON7772066\",\"request_carrier_code\":\"OOLU\",\"request_type\":\"c_id\",\"descriptive_name\":\"1e5717a3-f3bc-413a-a73e-0a3ff19d73f0\"}";
			configuration.Subscription = new OceanInsightsServiceProvider.Subscription();
			configuration.Subscription.RequestCarrierCode = "OOLU";
			configuration.Subscription.RequestKey = "CON7772066";
			configuration.Subscription.RequestType = "c_id";
			configuration.ServiceReplyResult = () => new ServiceReply(ServiceReply.Action.Success, "[{'id':5785,'request_carrier_name':'OOCL','containershipments':[],'url':'http://capi.ocean-insights.com/containertracking/v2/subscriptions/5785/','request_type':'m_bl','request_key':'OOLU401862065','descriptive_name':null,'created':'2015-08-11T08:00:50Z','modified':'2015-08-11T08:01:04Z','last_carrier_update':'2015-08-11T08:00:55Z','last_apc_update':null,'status':0}]");


			var provider = new OceanInsightsServiceProviderTest(Logger, "HYEDAUIKB", "OceanInsights", messageString, configuration);
			provider.Process();
		}
	}


}

