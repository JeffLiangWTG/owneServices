using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests.TransformHelpers
{
	[TestClass]
	public class SGCustomsSubscriptionHelperTests
	{
		SGCustomsSubscriptionHelper subscriptionHelper = new SGCustomsSubscriptionHelper();
		XPathNavigator subscription = new XDocument().CreateNavigator();
		static string InitialSubscription;

		[ClassInitialize]
		public static void ClassInitialize(TestContext tc)
		{
			InitialSubscription = ReadResource("TransformHelpers.TestFiles.Input.InitialSubscription.xml");
		}

		[TestInitialize]
		public void Initialize()
		{
			subscription = subscriptionHelper.InitializeSubscription();
		}

		[TestCleanup]
		public void CleanUp()
		{
			subscription = subscriptionHelper.InitializeSubscription();
		}

		[TestMethod]
		public void TestEmptySubscription_Success()
		{
			var emptySubscription = subscriptionHelper.InitializeSubscription();
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.EmptySubscription.xml"), emptySubscription.OuterXml);
		}

		[TestMethod]
		public void TestLoadSubscription()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Input.InitialSubscription.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeShipment()
		{
			var emptySubscription = subscriptionHelper.InitializeSubscription();
			subscriptionHelper.SubscribeShipment(emptySubscription, "MAN000095", "MAWB", "Import");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeShipment.xml"), emptySubscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeHistoryOrder()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribeHistoryOrder(subscription, "PCM", "2017-11-13T03:12:48", "IDT5.1", "IDT5.2", "IDT5.3");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeHistoryOrder.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeExistingHAWB()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribeHAWB(subscription, "HWB1");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Input.InitialSubscription.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeNewHAWB()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribeHAWB(subscription, "HWB4");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeNewHAWB.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeExistingPackline()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribePackLine(subscription, "HWB2", "00025", "2", "Declared", "IDT4", "PCM");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeExistingPackLine.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeNewPackline()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribePackLine(subscription, "HWB2", "00004", "4", "Declared", "IDT4", "PCM");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeNewPackLine.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeInfo()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribeInfo(subscription, "HWB3", "3", "IDT4", "PCM", "PermitNumber", "P2");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeInfo.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSubscribeEvent()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			subscriptionHelper.SubscribeEvent(subscription, "HWB2", "1", "Declared", "IDT4", "PCM");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SubscribeEvent.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSelectHousesByIDT()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			var HAWBs = subscriptionHelper.SelectHousesByIDT(subscription, "IDT4", "PCM");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SelectHousesByIDT.xml"), HAWBs.OuterXml);
		}

		[TestMethod]
		public void TestSelectPackLinesByIDT()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			var HWB2Packs = subscriptionHelper.SelectPackLinesByIDT(subscription, "HWB2", "IDT1", "PCM");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SelectPackLinesByIDT.xml"), HWB2Packs.OuterXml);
		}

		[TestMethod]
		public void TestDeletePackLinesAndHouseByIDTAndHAWB()
		{
			var subscriptionWithError = ReadResource("TransformHelpers.TestFiles.Input.SubscriptionWithError.xml");
			subscriptionHelper.LoadSubscription(subscription, subscriptionWithError);

			// Only delete pack lines
			subscriptionHelper.DeletePackLinesAndHouseByIDTAndHAWB(subscription, "IDT2", "HAWB1");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.DeletePackLinesAndHouseByIDTAndHAWB.xml"), subscription.OuterXml);

			// Delete the whole house because the only pack line is deleted
			subscriptionHelper.DeletePackLinesAndHouseByIDTAndHAWB(subscription, "IDT2", "HAWB2");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.DeletePackLinesAndHouseByIDTAndHAWB_Step2.xml"), subscription.OuterXml);
		}

		[TestMethod]
		public void TestSelectPackLinesByStatus()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			var HWB2Packs = subscriptionHelper.SelectPackLinesByStatus(subscription, "IDT3", "HWB2", "PIN", "Accepted");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SelectPackLinesByStatus.xml"), HWB2Packs.OuterXml);
		}

		[TestMethod]
		public void TestSelectPackLineCurrentStatus()
		{
			subscriptionHelper.LoadSubscription(subscription, InitialSubscription);
			var Pack2CurrentStatus = subscriptionHelper.SelectPackLineCurrentStatus(subscription, "HWB2", "2");
			Assert.AreEqual(ReadResource("TransformHelpers.TestFiles.Output.SelectPackLineCurrentStatus.xml"), Pack2CurrentStatus.OuterXml);
		}

		public static string ReadResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}
	}
}
