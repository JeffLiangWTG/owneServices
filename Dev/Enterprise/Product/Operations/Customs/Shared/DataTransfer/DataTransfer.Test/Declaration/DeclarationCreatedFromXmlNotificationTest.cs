using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Testing
{
	[TestedType(typeof(DeclarationCreatedFromXmlNotification))]
	sealed class DeclarationCreatedFromXmlNotificationTest : NotificationTest<DeclarationCreatedFromXmlNotification>
	{
		public void TestDisplayMessage()
		{
			var notification = new DeclarationCreatedFromXmlNotification(JobDeclaration);
			AssertNotNull("The message is available after the factory is saved", notification);
			var expectedMesg = "Declaration B00001000 (HouseBill='HouseBill' Branch='" + JobDeclaration.Branch.GB_Code + "') Created.";
			AssertEquals("Display Message", expectedMesg, notification.Message);
		}

		protected override DeclarationCreatedFromXmlNotification NewTestNotification() => new DeclarationCreatedFromXmlNotification(JobDeclaration);

		protected override bool IsSerializable => false;

		BaseJobDeclaration JobDeclaration
		{
			get
			{
				if (jobDeclaration == null)
				{
					jobDeclaration = BaseJobDeclaration.New(Factory);
					jobDeclaration.JE_HouseBill = "HouseBill";
					Factory.Save();
				}

				return jobDeclaration;
			}
		}
		BaseJobDeclaration jobDeclaration;
	}
}
