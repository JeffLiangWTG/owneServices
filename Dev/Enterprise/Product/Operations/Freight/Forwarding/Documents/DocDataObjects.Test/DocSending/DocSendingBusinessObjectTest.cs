using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DocSending
{
	[TestedType(typeof(DocSendingBusinessObject))]
	sealed class DocSendingBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new DocSendingBusinessObject
		{
		};

		public void Test_Include_Unticked_UntickCertify()
		{
			var docSending = new DocSendingBusinessObject();
			docSending.Include = true;
			docSending.Certify = true;

			docSending.Include = false;

			AssertEquals(expected: false, docSending.Certify);
		}

		public void Test_Certify_Ticked_TickInclude()
		{
			var docSending = new DocSendingBusinessObject();
			docSending.Include = false;
			docSending.Certify = false;

			docSending.Certify = true;

			AssertEquals(expected: true, docSending.Include);
		}
	}
}
