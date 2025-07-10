using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PersonIdentityInformationDataCollection))]
	class PersonIdentityInformationDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PersonIdentityInformationDataCollection>
	{
		#region Implementation

		protected override PersonIdentityInformationDataCollection GetCollectionToTest()
		{
			return new PersonIdentityInformationDataCollection(MessageData);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PersonIdentityInformationData(Factory, MessageData);
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
