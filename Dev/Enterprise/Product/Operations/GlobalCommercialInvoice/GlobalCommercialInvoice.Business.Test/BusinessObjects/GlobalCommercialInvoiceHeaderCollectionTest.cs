using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceHeaderCollection))]
	sealed class GlobalCommercialInvoiceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<GlobalCommercialInvoiceHeaderCollection>
	{
		public void TestCollectionCreation()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var header2 = Factory.CreateInvoiceHeader(shipment);

			Factory.CreateInvoiceLine(header1); // Shouldn't be included in the collection

			var collection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);

			AssertContainsExactElementsInAnyOrder(
				"Collection should contain shipment invoice headers only",
				collection,
				[header1, header2]);
		}

		public void TestCollectionHeaderAdding()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var collection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);

			var header2 = collection.AddNew();
			header2.GIH_Description = "Invoice Header Description";
			header2.GIH_InvoiceNumber = nameof(header2);
			header2.GIH_InvoiceDate = ZDate.Today.AddDays(1);
			header2.GIH_ParentID = shipment.PK;
			header2.GIH_ParentTableCode = shipment.TablePrefix;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should add header by calling {nameof(GlobalCommercialInvoiceHeaderCollection)}.{nameof(GlobalCommercialInvoiceHeaderCollection.AddNew)}()",
				collection,
				[header1, header2]);

			var header3 = Factory.CreateInvoiceHeader(shipment);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should add header by calling the shipment {nameof(BusinessObjectFactory)}.{nameof(BusinessObjectFactory.New)}()",
				collection,
				[header1, header2, header3]);
		}

		public void TestCollectionHeaderDelete()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var header2 = Factory.CreateInvoiceHeader(shipment);
			var header3 = Factory.CreateInvoiceHeader(shipment);
			var header4 = Factory.CreateInvoiceHeader(shipment);
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			headerCollection.Delete(header2);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should delete header by calling {nameof(GlobalCommercialInvoiceHeaderCollection)}.{nameof(GlobalCommercialInvoiceHeaderCollection.Delete)}()",
				headerCollection,
				[header1, header3, header4]);

			header4.Delete();

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				$"Collection should delete header by calling {nameof(GlobalCommercialInvoiceHeader)}.{nameof(GlobalCommercialInvoiceHeader.Delete)}()",
				headerCollection,
				[header1, header3]);

			Assert("Deleted header should be marked as deleted", header2.IsDeleted && header4.IsDeleted);
		}

		public void TestCollectionFilter()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var header2 = Factory.CreateInvoiceHeader(shipment);
			var header3 = Factory.CreateInvoiceHeader(shipment);
			var collection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			collection.AdditionalFilter = new ZQuery(GlobalCommercialInvoiceHeaderSchema.PK, new[]
			{
				header1.PK,
				header3.PK
			});

			AssertContainsExactElementsInAnyOrder(
				"Collection filter should keep provided items only when filtered",
				collection,
				[header1, header3]);

			collection.AdditionalFilter = new ZQuery();

			AssertContainsExactElementsInAnyOrder(
				"Collection filter should contain all items when filter is reset",
				collection,
				[header1, header2, header3]);
		}

		public void TestCollectionSorting()
		{
			var shipment = (BusinessObject)Factory.CreateNewShipment();
			var header1 = Factory.CreateInvoiceHeader(shipment);
			var header2 = Factory.CreateInvoiceHeader(shipment);
			var header3 = Factory.CreateInvoiceHeader(shipment);

			header2.GIH_Description = "B";
			header1.GIH_Description = "A";
			header3.GIH_Description = "C";

			var collection = new GlobalCommercialInvoiceHeaderCollection(Factory, shipment.PK, shipment.TablePrefix);
			collection.ApplySort(nameof(GlobalCommercialInvoiceHeader.GIH_Description), ListSortDirection.Descending);

			AssertContainsExactElementsInExactOrder(
				$"Collection should be sorted when called {nameof(GlobalCommercialInvoiceHeaderCollection)}.{nameof(GlobalCommercialInvoiceHeaderCollection.ApplySort)}()",
				collection,
				[header3, header2, header1]);

			collection.RemoveSort();

			AssertNull(
				$"$Collection comparer should be set to null when called {nameof(GlobalCommercialInvoiceHeaderCollection)}.{nameof(GlobalCommercialInvoiceHeaderCollection.RemoveSort)}()",
				collection.SortComparer);
		}

		protected override GlobalCommercialInvoiceHeaderCollection GetCollectionToTest()
		{
			return new GlobalCommercialInvoiceHeaderCollection(Factory, ZGuid.NewZGuid(), JobShipmentSchema.Constants.Prefix);
		}
	}
}
