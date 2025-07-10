using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthorityFilterIssueTest : BaseAgencyTest
	{
		#region TestFactory

		public void TestFactory()
		{
			AssertNull(new PortMessageIssue(ZGuid.Empty, "", "", "").Factory);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			ZGuid pk2 = ZGuid.NewZGuid();

			PortMessageIssue issue1 = new PortMessageIssue(pk1, "JS", "Text 1", "Daedalus");
			PortMessageIssue issue2 = new PortMessageIssue(pk2, "RV", "Text 2", "Icarus");

			AssertEquals("notification1.TargetPK", pk1, issue1.TargetPK);
			AssertEquals("notification1.TargetCode", "JS", issue1.TargetCode);
			AssertEquals("notification1.Text", "Text 1", issue1.Text);
			AssertEquals("notification1.Detail", "Daedalus", issue1.Detail);

			AssertEquals("notification2.TargetPK", pk2, issue2.TargetPK);
			AssertEquals("notification2.TargetCode", "RV", issue2.TargetCode);
			AssertEquals("notification2.Text", "Text 2", issue2.Text);
			AssertEquals("notification2.Detail", "Icarus", issue2.Detail);
		}

		#endregion
	}

	[TestedType(typeof(PortMessageIssue))]
	internal class PortAuthorityFilterIssueBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortMessageIssue(ZGuid.Empty, "", "Blat 'O' Matica", "Blat");
		}

		#endregion
	}
}
