using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Common
{
	[TestFixture]
	public class ODTFileHelperTests
	{
		[Test]
		public void GetDataTebleFromODT()
		{
			var data = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocUnion.odt");

			Assert.That(data, Is.Not.Null.And.Not.Empty);

			var tbl = ODTFileHelper.GetTableFromCellContent(data, "Document Code to be declared");

			Assert.That(tbl, Is.Not.Null);
			Assert.That(tbl.Rows.Count, Is.GreaterThan(0));

			var row = tbl.Rows[0].ItemArray;
			Assert.That(row, Is.Not.Null);
			Assert.That(row.Length, Is.EqualTo(4));

			tbl = ODTFileHelper.GetTableFromCellContent(data, "This does not exist");

			Assert.That(tbl, Is.Not.Null);
			Assert.That(tbl.Rows.Count, Is.EqualTo(0));
		}

		[Test]
		public void GetDataTableFromODS()
		{
			var data = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.CDSStandingData.TestFiles.Input.SupportingDocumentStatusCodes.ods");

			Assert.That(data, Is.Not.Null.And.Not.Empty);

			var tbl = ODTFileHelper.GetTableFromCellContent(data, "Status");

			Assert.That(tbl, Is.Not.Null);
			Assert.That(tbl.Rows.Count, Is.GreaterThan(0));

			var row = tbl.Rows[0].ItemArray;
			Assert.That(row, Is.Not.Null);
			Assert.That(row.Length, Is.EqualTo(2));

			tbl = ODTFileHelper.GetTableFromCellContent(data, "This does not exist");

			Assert.That(tbl, Is.Not.Null);
			Assert.That(tbl.Rows.Count, Is.EqualTo(0));
		}
	}
}
