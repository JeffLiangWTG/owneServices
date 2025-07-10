using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusOutturnHeader))]
	sealed class CusOutturnHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBusinessObjectsWithRelatedEvents()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();

			AssertCollectionContains(outturn, header.BusinessObjectsWithRelatedEvents);
		}

		public void TestCustomsCountryCode()
		{
			AssertEquals("CustomsCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, header.CustomsCountryCode);
		}

		public void TestLoadFromSendersReference()
		{
			header.C6_SendersMessageReference = "123";
			AssertEquals("Header", header, CusOutturnHeader.LoadFromSendersReference(Factory, "123"));
		}

		public void TestSendersReference()
		{
			ISendersMessageReferenceProvider provider = header;
			AssertEquals("empty initially", true, provider.SendersReference.IsEmpty);
			header.C6_SendersMessageReference = "foo";
			AssertEquals("proxies C6_SendersMessageReference", "foo", provider.SendersReference);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("Status Needs Recalculation initially", false, header.StatusNeedsRecalculation);
			header.Messages.AddNew().EM_Status = "foo";
			AssertEquals("Status Needs Recalculation after adding message", true, header.StatusNeedsRecalculation);
		}

		public void TestOutturns()
		{
			AssertNotNull("nullness", header.Outturns);
			AssertEquals("type", typeof(CusOutturnHeaderCusOutturnCollection), header.Outturns.GetType());
			AssertEquals("is loaded", true, header.Outturns.IsLoaded);
			AssertEquals("is registered editable", true, header.IsRegisteredEditableChildObject(header.Outturns));
		}

		public void TestMessages()
		{
			AssertNotNull("nullness", header.Messages);
			AssertEquals("type", typeof(EDIMessageCollection), header.Messages.GetType());
			AssertEquals("is loaded", true, header.Messages.IsLoaded);
		}

		public void TestTypeDecider()
		{
			AssertNotNull("nullness", CusOutturnHeader.TypeDecider);
			AssertEquals("TypDecider.GetType()", typeof(CusOutturnHeaderTypeDecider), CusOutturnHeader.TypeDecider.GetType());
		}

		public void TestIDocManagerSupport()
		{
			AssertEquals(typeof(CusOutturnHeaderDocManagerInfo), ((IDocManagerSupport)header).DocManagerInfo.GetType());
			AssertEquals("COH", ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusOutturnHeader>();
		}
		CusOutturnHeader header;

		#endregion
	}
}
