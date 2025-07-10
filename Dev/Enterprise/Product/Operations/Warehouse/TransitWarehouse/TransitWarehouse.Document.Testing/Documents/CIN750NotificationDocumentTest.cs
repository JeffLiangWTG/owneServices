using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public abstract class CIN750NotificationDocumentTest<TSource> : DocumentVisualizer.Testing.StandardDocumentContentTest where TSource : BusinessObject
	{
		[TestDate(2023, 12, 01, 01, 02, 03)]
		public override void TestDocumentContent()
		{
			AssertDocumentContent();
		}

		public abstract ZGuid GetDocumentID();

		public void AssertDocumentContent()
		{
			var source = CreateSource();
			Factory.Save();

			var pivotPK = GetDocumentID();

			var documentContent = CreateContent();

			AssertContents(source, pivotPK, documentContent);
		}

		protected abstract TSource CreateSource();

		protected abstract ZString CreateContent();

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
