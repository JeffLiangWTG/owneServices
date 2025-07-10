using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedBusinessData))]
	class RelatedBusinessDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLineNumber()
		{
			var relatedBusiness1 = MessageData.RelatedBusinessItems.AddNew();
			AssertEquals(1, relatedBusiness1.US_LineNo);
			var relatedBusiness2 = MessageData.RelatedBusinessItems.AddNew();
			AssertEquals(2, relatedBusiness2.US_LineNo);
			var relatedBusiness3 = MessageData.RelatedBusinessItems.AddNew();
			AssertEquals(3, relatedBusiness3.US_LineNo);
			MessageData.RelatedBusinessItems.RemoveAndDelete(relatedBusiness2);
			AssertEquals(1, relatedBusiness1.US_LineNo);
			AssertEquals(2, relatedBusiness3.US_LineNo);
			var relatedBusiness4 = MessageData.RelatedBusinessItems.AddNew();
			AssertEquals(3, relatedBusiness4.US_LineNo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RelatedBusinessData(Factory, MessageData);
		}

		OrgAddressMessageData MessageData
		{
			get
			{
				if (messageData == null)
				{
					var organization = Factory.New<OrgHeader>();
					var wrapper = OrgHeaderWrapper.New(organization);
					messageData = new OrgAddressMessageData(wrapper);
				}

				return messageData;
			}
		}
		OrgAddressMessageData messageData;

		#endregion
	}
}
