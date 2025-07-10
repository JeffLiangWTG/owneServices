using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(RelatedBusinessDataCollection))]
	class RelatedBusinessDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RelatedBusinessDataCollection>
	{
		#region Implementation

		protected override RelatedBusinessDataCollection GetCollectionToTest()
		{
			return new RelatedBusinessDataCollection(MessageData);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
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
