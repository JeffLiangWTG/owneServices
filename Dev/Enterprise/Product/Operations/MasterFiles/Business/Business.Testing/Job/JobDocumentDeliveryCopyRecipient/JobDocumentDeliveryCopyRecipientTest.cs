using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentDeliveryCopyRecipient))]
	public class JobDocumentDeliveryCopyRecipientTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var copyRecipient = Factory.NewWithValidTestData<JobDocumentDeliveryCopyRecipient>();
			copyRecipient.DocumentDelivery.JDC_DocumentGroup = ContactType.All.ToString();
			copyRecipient.DocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			return copyRecipient;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		#endregion
	}
}
