using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLHeaderSubAccountCollection))]
	sealed class AccGLHeaderSubAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccGLHeaderSubAccountCollection(Factory.New<AccGLHeader>());
		}

		public void TestSort()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AssertSort(glHeader, () => { glHeader.SubAccountTypes.Load(); });
		}

		public void TestResort()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AssertSort(glHeader, () => { glHeader.SubAccountTypes.Resort(); });
		}

		void AssertSort(AccGLHeader glHeader, Action sortAction)
		{
			var subAccountType1 = glHeader.SubAccountTypes.AddNew();
			var subAccountType2 = glHeader.SubAccountTypes.AddNew();
			var subAccountType3 = glHeader.SubAccountTypes.AddNew();
			var subAccountType4 = glHeader.SubAccountTypes.AddNew();

			subAccountType1.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffGroup;
			subAccountType2.ASA_SubClassDisplayName = Core.Constants.SubAccountType.StaffAndResources;
			subAccountType3.ASA_SubClassDisplayName = Core.Constants.SubAccountType.SalesGroup;
			subAccountType4.ASA_SubClassDisplayName = Core.Constants.SubAccountType.Organization;

			var glHeaderSubAccountCollection = glHeader.SubAccountTypes.Cast<AccGLHeaderSubAccount>();

			AssertEquals("1st sub account", subAccountType1, glHeaderSubAccountCollection.FirstOrDefault());
			AssertEquals("2nd sub account", subAccountType2, glHeaderSubAccountCollection.Skip(1).FirstOrDefault());
			AssertEquals("3rd sub account", subAccountType3, glHeaderSubAccountCollection.Skip(2).FirstOrDefault());
			AssertEquals("4th sub account", subAccountType4, glHeaderSubAccountCollection.Skip(3).FirstOrDefault());

			sortAction.Invoke();

			AssertEquals("1st sub account", subAccountType4, glHeaderSubAccountCollection.FirstOrDefault());
			AssertEquals("2nd sub account", subAccountType3, glHeaderSubAccountCollection.Skip(1).FirstOrDefault());
			AssertEquals("3rd sub account", subAccountType2, glHeaderSubAccountCollection.Skip(2).FirstOrDefault());
			AssertEquals("4th sub account", subAccountType1, glHeaderSubAccountCollection.Skip(3).FirstOrDefault());
		}
	}
}
