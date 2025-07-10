using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortMessageIssueCollectionTest : BaseAgencyTest
	{
		#region TestFactory

		public void TestFactory()
		{
			AssertNull(new PortMessageIssueCollection().Factory);
		}

		#endregion

		#region TestAddNew

		public void TestAddNew()
		{
			ZGuid pk = ZGuid.NewZGuid();

			PortMessageIssue issue = new PortMessageIssueCollection().AddNew(pk, "JS", "random Text", "snth");

			AssertEquals(pk, issue.TargetPK);
			AssertEquals("random Text", issue.Text);
			AssertEquals("snth", issue.Detail);
		}

		#endregion
	}

	[TestedType(typeof(PortMessageIssueCollection))]
	internal class PortMessageIssueCollectionBOTest : NonPersistentBusinessObjectCollectionTestCase<PortMessageIssueCollection>
	{
		#region Implementation

		protected override PortMessageIssueCollection GetCollectionToTest()
		{
			return new PortMessageIssueCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortMessageIssue(ZGuid.Empty, "JS", "Blat", "Bob");
		}

		#endregion
	}
}
