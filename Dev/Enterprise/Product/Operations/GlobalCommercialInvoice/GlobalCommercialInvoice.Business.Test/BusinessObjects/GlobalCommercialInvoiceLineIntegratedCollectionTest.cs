using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceLineIntegratedCollection))]
	sealed class GlobalCommercialInvoiceLineIntegratedCollectionTest : ActiveBusinessObjectCollectionTestCase<GlobalCommercialInvoiceLineIntegratedCollection>
	{
		public void TestCollectionCreation()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(2, [2, 3]);

			AssertContainsExactElementsInAnyOrder(
				"Collection should contain shipment invoice lines",
				lineCollection,
				[lines[0][0], lines[0][1], lines[1][0], lines[1][1], lines[1][2]]);
		}

		public override void TestAdd()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
			var header = headerCollection.AddNew();
			var line1 = lineCollection.AddNew();
			line1.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			line1.GIL_GIH_Header = header.PK;

			var line2 = lineCollection.AddNew();
			line2.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			line2.GIL_GIH_Header = header.PK;

			Factory.Save();

			AssertEquals("lineCollection count", checked(2), lineCollection.Count);
		}

		public override void TestDelete()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
			var header = headerCollection.AddNew();

			var line1 = lineCollection.AddNew();
			line1.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			line1.GIL_GIH_Header = header.PK;

			var line2 = lineCollection.AddNew();
			line2.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			line2.GIL_GIH_Header = header.PK;
			Factory.Save();

			lineCollection.Delete(line1);
			Factory.Save();

			AssertEquals("lineCollection count", checked(1), lineCollection.Count);
		}

		public void TestCollectionLineAdding()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(1, [1]);
			var header1 = headers[0];
			var line1 = lines[0][0];
			line1.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, header1.PK, header1.TablePrefix);
			line1.GIL_GIH_Header = header1.PK;

			var line2 = lineCollection.AddNew();
			line2.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, header1.PK, header1.TablePrefix);
			line2.GIL_Description = "Invoice Line Description";
			line2.GIL_GIH_Header = header1.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should add line by calling {nameof(GlobalCommercialInvoiceLineIntegratedCollection)}.{nameof(GlobalCommercialInvoiceLineIntegratedCollection.AddNew)}()",
				lineCollection,
				[line1, line2]);

			var line3 = Factory.CreateInvoiceLine(header1);
			line3.Headers = line2.Headers;
			line3.GIL_GIH_Header = header1.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should add line by calling the shipment {nameof(BusinessObjectFactory)}.{nameof(BusinessObjectFactory.New)}()",
				lineCollection,
				[line1, line2, line3]);
		}

		public override void TestTypedget_Item()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var header = headerCollection.AddNew();
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
			var line = lineCollection.AddNew();
			line.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);

			Factory.Save();

			var type = lineCollection.GetType();
			AssertEquals(typeof(GlobalCommercialInvoiceLineIntegratedCollection), type);
		}

		public void TestCollectionLineDelete()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(1, [4]);
			var line1 = lines[0][0];
			var line2 = lines[0][1];
			var line3 = lines[0][2];
			var line4 = lines[0][3];

			lineCollection.Delete(line2);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should delete line by calling {nameof(GlobalCommercialInvoiceLineIntegratedCollection)}.{nameof(GlobalCommercialInvoiceLineIntegratedCollection.Delete)}()",
				lineCollection,
				[line1, line3, line4]);

			line4.Delete();
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should delete line by calling {nameof(GlobalCommercialInvoiceLine)}.{nameof(GlobalCommercialInvoiceLine.Delete)}()",
				lineCollection,
				[line1, line3]);

			Assert("Deleted line should be marked as deleted", line2.IsDeleted && line4.IsDeleted);
		}

		public void TestCollectionLineDeleteOnHeaderDelete()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(2, [2, 3]);

			Factory.Save();

			headerCollection.Delete(headers[0]);

			AssertContainsExactElementsInAnyOrder(
				"Collection should delete lines when header is deleted",
				lineCollection,
				lines[1]);

			Assert("Deleted lines should be marked as deleted", lines[0].All((x) => x.IsDeleted));
		}

		public void TestLineNumberInitialAssignment()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections();

			Factory.Save();

			CombineAssertions("Line numbers should be initially assigned correctly", () =>
			{
				for (var i = 0; i < headers.Length; i++)
				{
					for (var j = 0; j < lines[i].Length; j++)
					{
						AssertEquals((short)(j + 1), lines[i][j].GIL_LineNo);
					}
				}
			});
		}

		public void TestLineNumberChangeOnDelete()
		{
			CombineAssertions(() =>
			{
				for (var i = 0; i < 5; i++)
				{
					var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(2, [5, 2]);

					Factory.Save();

					var headerLines = lines[0];
					lineCollection.Delete(headerLines[i]);

					var active = headerLines.Where((l) => !l.IsDeleted).ToArray();

					for (var j = 0; j < active.Length; j++)
					{
						AssertEquals((short)(j + 1), active[j].GIL_LineNo);
					}
				}
			});
		}

		public void TestLineNumberChange()
		{
			CombineAssertions(() =>
			{
				Test(-100, [2, 3, 1, 4, 5]);
				Test(0, [2, 3, 1, 4, 5]);
				Test(1, [2, 3, 1, 4, 5]);
				Test(3, [1, 2, 3, 4, 5]);
				Test(4, [1, 2, 3, 4, 5]);
				Test(5, [1, 2, 4, 3, 5]);
				Test(500, [1, 2, 5, 3, 4]);
			});

			return;

			// header and line are 1-based for convenience to align with the line numbers
			void Test(short newNumber, short[] expectedValues)
			{
				var (lines, _, _, _) = CreateTestLineCollections(2, [5, 2]);

				Factory.Save();

				var headerLines = lines[0]; 
				headerLines[2].GIL_LineNo = newNumber;

				for (var i = 0; i < headerLines.Length; i++)
				{
					AssertEquals(expectedValues[i], headerLines[i].GIL_LineNo);
				}
			}
		}

		public void TestLineNumberOnHeaderChange()
		{
			CombineAssertions(() =>
			{
				Test(1, [1, 1, 2, 3, 4], [2, 3, 4, 5, 6]);
				Test(3, [1, 2, 3, 3, 4], [1, 2, 4, 5, 6]);
				Test(5, [1, 2, 3, 4, 5], [1, 2, 3, 4, 6]);
			});

			return;

			// header and line are 1-based for convenience to align with the line numbers
			void Test(int line, short[] oldHeaderExpectedValues, short[] newHeaderExpectedValues)
			{
				var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections();

				Factory.Save();

				var headerLines = lines[0];
				var newHeaderLines = lines[1];
				headerLines[line - 1].GIL_GIH_Header = headers[1].PK;

				for (var i = 0; i < headerLines.Length; i++)
				{
					AssertEquals(oldHeaderExpectedValues[i], headerLines[i].GIL_LineNo);
				}

				for (var i = 0; i < newHeaderLines.Length; i++)
				{
					AssertEquals(newHeaderExpectedValues[i], newHeaderLines[i].GIL_LineNo);
				}
			}
		}

		public void TestDefaultHeaderChange()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections();

			Factory.Save();

			CombineAssertions(() =>
			{
				var newLine = lineCollection.AddNew();
				AssertEquals("1. If we create a new line, its default header should be the last header", headers[1].PK, newLine.GIL_GIH_Header);

				lines[1][3].GIL_GIH_Header = headers[0].PK;
				newLine = lineCollection.AddNew();
				AssertEquals("2. If the user changes the header value of an existing line, it should become default", headers[0].PK, newLine.GIL_GIH_Header);

				newLine.GIL_GIH_Header = headers[1].PK;
				newLine = lineCollection.AddNew();
				AssertEquals("3. If the user changes the header value of a new line, it should become default", headers[1].PK, newLine.GIL_GIH_Header);

				var newHeader = headerCollection.AddNew();
				newLine = lineCollection.AddNew();
				AssertEquals("4. If the user adds a new header, it should become default", newHeader.PK, newLine.GIL_GIH_Header);

				var linesToDelete = lineCollection.Where((x) => x.GIL_GIH_Header == newHeader.PK).ToArray();
				foreach (var line in linesToDelete)
				{
					lineCollection.Delete(line);
				}
				headerCollection.Delete(newHeader);
				newLine = lineCollection.AddNew();
				AssertEquals("5. If the user deletes a header, the last header in the collection should become default", headers[1].PK, newLine.GIL_GIH_Header);

				lineCollection.DeleteAll();
				headerCollection.DeleteAll();
				newLine = lineCollection.AddNew();
				AssertEquals("6. If the user deletes all headers, an empty ZGuid should be the default header ID", ZGuid.Empty, newLine.GIL_GIH_Header);
			});
		}

		public void TestLineDescriptionOnHeaderChange()
		{
			var (lines, headers, lineCollection, headerCollection) =
				CreateTestLineCollections(2, [3, 3], false);

			Factory.Save();

			CombineAssertions(() =>
			{
				for (var i = 0; i < headers.Length; i++)
				{
					foreach (var line in lines[i])
					{
						AssertEquals(headers[i].GIH_Description, line.GIL_Description);
					}
				}
			});

			lines[1][1].GIL_GIH_Header = headers[0].PK;
			AssertEquals(headers[0].GIH_Description, lines[1][1].GIL_Description);

			lines[0][0].GIL_Description = "Changed By User";
			lines[0][0].GIL_GIH_Header = headers[1].PK;
			AssertEquals("Shouldn't update if the user has changed the line description", "Changed By User", lines[0][0].GIL_Description);
		}

		public void TestLineDescriptionOnHeaderDescriptionChange()
		{
			var (lines, headers, lineCollection, headerCollection) =
				CreateTestLineCollections(2, [3, 3], false);

			Factory.Save();

			headers[1].GIH_Description = "Changed Header 1 Description";

			CombineAssertions(() =>
			{
				foreach (var line in lines[1])
				{
					AssertEquals("Changed Header 1 Description", line.GIL_Description);
				}
			});

			lines[0][0].GIL_Description = "Changed By User";
			headers[0].GIH_Description = "Changed Header 0 Description";
			AssertEquals("Shouldn't update if the user has changed the line description", "Changed By User", lines[0][0].GIL_Description);
			AssertEquals("Otherwise, it should", "Changed Header 0 Description", lines[0][1].GIL_Description);
		}

		public void TestCollectionFilter()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(1, [3]);
			var line1 = lines[0][0];
			var line2 = lines[0][1];
			var line3 = lines[0][2];

			lineCollection.AdditionalFilter = new ZQuery(GlobalCommercialInvoiceLineSchema.PK, new[]
			{
				line1.PK,
				line3.PK
			});

			AssertContainsExactElementsInAnyOrder(
				"Collection filter should keep provided items only when filtered",
				lineCollection,
				[line1, line3]);

			lineCollection.AdditionalFilter = new ZQuery();

			AssertContainsExactElementsInAnyOrder(
				"Collection filter should contain all items when filter is reset",
				lineCollection,
				[line1, line2, line3]);
		}

		public void TestCollectionSorting()
		{
			var (lines, headers, lineCollection, headerCollection) = CreateTestLineCollections(1, [3]);
			var line1 = lines[0][0];
			var line2 = lines[0][1];
			var line3 = lines[0][2];

			line1.GIL_Description = "A";
			line2.GIL_Description = "B";
			line3.GIL_Description = "C";

			lineCollection.ApplySort(nameof(GlobalCommercialInvoiceLine.GIL_Description), ListSortDirection.Descending);

			AssertContainsExactElementsInExactOrder(
				$"Collection should be sorted when called {nameof(GlobalCommercialInvoiceLineIntegratedCollection)}.{nameof(GlobalCommercialInvoiceLineIntegratedCollection.ApplySort)}()",
				lineCollection,
				[line3, line2, line1]);

			lineCollection.RemoveSort();

			AssertNull(
				$"$Collection comparer should be set to null when called {nameof(GlobalCommercialInvoiceLineIntegratedCollection)}.{nameof(GlobalCommercialInvoiceLineIntegratedCollection.RemoveSort)}()",
				lineCollection.SortComparer);
		}

		protected override GlobalCommercialInvoiceLineIntegratedCollection GetCollectionToTest()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix);
			_ = headerCollection.AddNew();

			return new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
		}

		(GlobalCommercialInvoiceLine[][] lines,
			GlobalCommercialInvoiceHeader[] headers,
			GlobalCommercialInvoiceLineIntegratedCollection lineCollection,
			GlobalCommercialInvoiceHeaderCollection headerCollection)
			CreateTestLineCollections(int headerCount = 2, int[] lineCounts = null, bool setLineDescription = true)
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(shipment, headerCollection);
			var headers = Factory.CreateInvoiceHeader(shipment, headerCount);
			var lines = headers
				.Select((h, headerIdx) => Enumerable
					.Repeat(() => lineCollection.CreateInvoiceLine(h, setLineDescription), lineCounts is null ? 5 : lineCounts[headerIdx])
					.Select((f) => f())
					.ToArray())
				.ToArray();

			return (lines, headers, lineCollection, headerCollection);
		}
	}
}
