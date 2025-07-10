using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Warehouse.Cartonisation.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Business.Testing
{
	public class CartonisationAlgorithmTest : TransactionedTestCase
	{
		#region Cost Test

		public void TestPickLowerCost() => TestPickLowerCost(box2IsLarger: false);
		public void TestPickLowerCost_Box2IsLarger() => TestPickLowerCost(box2IsLarger: true);
		void TestPickLowerCost(bool box2IsLarger)
		{
			var box1 = CartonisationTestHelper.CreateCarton(1, 1, 1, 9999, 10, 9999, 1, "M", "CC", "KG", 50);
			var box2 = box2IsLarger ? CartonisationTestHelper.CreateCarton(1.5m, 1.5m, 1.5m, 14999, 15, 14999, 1, "M", "CC", "KG", 25)
				: CartonisationTestHelper.CreateCarton(1, 1, 1, 9999, 10, 9999, 1, "M", "CC", "KG", 25);

			var product = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 5m, 0.1m);

			var boxes = new List<ICartonDefinition> { box1, box2 };
			var products = new List<ICartonisableItem>();

			for (int i = 0; i < 4; i++)
			{
				products.AddNew(1, product);
			}

			var result = Algorithm.CartoniseItems(products, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box2.PK },
				new CartonWithItems { CartonPK = box2.PK }
			};

			AssertResult(expect, result, boxes);
		}

		#endregion

		#region Equal Cost Test

		public void TestEqualCostSelectLessCartons()
		{
			var box1 = CartonisationTestHelper.CreateCarton(1m, 1m, 1m, 9999m, 10m, 9999, 1m);
			var box2 = CartonisationTestHelper.CreateCarton(1m, 1m, 1m, 9999m, 5m, 9999, 1m);
			var product = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 5m, 0.1m);

			var boxes = new List<ICartonDefinition> { box1, box2 };
			var products = new List<ICartonisableItem>();

			for (int i = 0; i < 4; i++)
			{
				products.AddNew(1, product);
			}

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box1.PK },
				new CartonWithItems { CartonPK = box1.PK }
			};

			var result = Algorithm.CartoniseItems(products, boxes);

			AssertResult(expect, result, boxes);
		}

		public void TestEqualCostEqualCartonsSelectLowerVolume()
		{
			var box1 = CartonisationTestHelper.CreateCarton(1, 1, 1, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var box2 = CartonisationTestHelper.CreateCarton(1, 1, 1, 9999, 5, 9999, 1, "M", "CC", "KG", 5);
			var product = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m);

			var boxes = new List<ICartonDefinition> { box1, box2 };
			var products = new List<ICartonisableItem>();

			products.AddNew(1, product);
			products.AddNew(1, product);

			var result = Algorithm.CartoniseItems(products, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box1.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void TestEqualCost_KeepSameProductsTogether()
		{
			var box = CartonisationTestHelper.CreateCarton(2, 2, 2, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var boxes = new List<ICartonDefinition> { box };
			var product1 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product1");
			var product2 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product2");

			var items = new List<ICartonisableItem>();
			var product1_1 = items.AddNew(1, product1);
			var product2_1 = items.AddNew(1, product2);
			var product1_2 = items.AddNew(1, product1);
			var product2_2 = items.AddNew(1, product2);

			// Minimise impact of randomness from stochastic optimisation techniques
			for (int i = 0; i < 10; i++)
			{
				var result = Algorithm.CartoniseItems(items, boxes);

				var expect = new List<ICartonWithItems>
				{
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
				};

				AssertResult(expect, result, boxes);
				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == product1_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == product1_2.PK)));

				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == product2_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == product2_2.PK)));
			}
		}

		public void TestEqualCost_KeepSameProductsTogether_StressTest()
		{
			// See CS01837724 - GVILOGCHC - Cartonisation Splitting Product Codes
			var box1 = CartonisationTestHelper.CreateCarton(380, 280, 70, 25, 7448, 9999, 75m, "MM", "CC", "KG", 1);
			var box2 = CartonisationTestHelper.CreateCarton(510, 390, 330, 25, 65637, 9999, 90m, "MM", "CC", "KG", 1);
			var box3 = CartonisationTestHelper.CreateCarton(325, 255, 70, 25, 5801.25m, 9999, 80m, "MM", "CC", "KG", 1);
			var box4 = CartonisationTestHelper.CreateCarton(440, 400, 90, 25, 15840, 9999, 85m, "MM", "CC", "KG", 1);
			var box5 = CartonisationTestHelper.CreateCarton(380, 270, 280, 25, 28728, 9999, 95m, "MM", "CC", "KG", 1);
			var boxes = new List<ICartonDefinition> { box1, box2, box3, box4, box5 };

			var product402 = CartonisationTestHelper.CreateProduct(125m, 40m, 100m, 500m, 0.11m, "MM", "CC", "KG", name: "402");
			var product406 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "406");
			var product407 = CartonisationTestHelper.CreateProduct(140m, 50m, 160m, 1120m, 0.2m, "MM", "CC", "KG", name: "407");
			var product412 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "412");
			var product416 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "416");
			var product502 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "502");
			var product506 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "506");
			var product510 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "510");
			var product514 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "514");
			var product516 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "516");
			var product518 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "518");
			var product532 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "532");
			var product741 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "741");
			var product742 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "742");
			var product803 = CartonisationTestHelper.CreateProduct(125.00m, 40.00m, 100.00m, 500.00m, 0.11m, "MM", "CC", "KG", name: "803");
			var product652 = CartonisationTestHelper.CreateProduct(100m, 20m, 90m, 180m, 0.1m, "MM", "CC", "KG", name: "652");
			var product653 = CartonisationTestHelper.CreateProduct(100.00m, 20.00m, 90.00m, 180.00m, 0.10m, "MM", "CC", "KG", name: "653");
			var product654 = CartonisationTestHelper.CreateProduct(100.00m, 20.00m, 90.00m, 180.00m, 0.10m, "MM", "CC", "KG", name: "654");
			var product409 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "409");
			var product413 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "413");
			var product417 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "417");
			var product420 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "420");
			var product421 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "421");
			var product427 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "427");
			var product429 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "429");
			var product503 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "503");
			var product511 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "511");
			var product515 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "515");
			var product517 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "517");
			var product525 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "525");
			var product533 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "533");
			var product626 = CartonisationTestHelper.CreateProduct(140.00m, 50.00m, 160.00m, 1120.00m, 0.20m, "MM", "CC", "KG", name: "626");
			var product676 = CartonisationTestHelper.CreateProduct(145m, 35m, 100m, 507.5m, 0.18m, "MM", "CC", "KG", name: "676");
			var product677 = CartonisationTestHelper.CreateProduct(145.00m, 35.00m, 100.00m, 507.50m, 0.11m, "MM", "CC", "KG", name: "677");
			var productO201 = CartonisationTestHelper.CreateProduct(200m, 60m, 110m, 1320m, 0.21m, "MM", "CC", "KG", name: "O201");
			var productO204 = CartonisationTestHelper.CreateProduct(200.00m, 60.00m, 110.00m, 1320.00m, 0.21m, "MM", "CC", "KG", name: "O204");
			var productO211 = CartonisationTestHelper.CreateProduct(200.00m, 60.00m, 110.00m, 1320.00m, 0.21m, "MM", "CC", "KG", name: "O211");
			var productO212 = CartonisationTestHelper.CreateProduct(200.00m, 60.00m, 110.00m, 1320.00m, 0.21m, "MM", "CC", "KG", name: "O212");
			var product667 = CartonisationTestHelper.CreateProduct(240m, 80m, 135m, 2592m, 0.78m, "MM", "CC", "KG", name: "667");
			var product706 = CartonisationTestHelper.CreateProduct(240.00m, 80.00m, 135.00m, 2592.00m, 0.78m, "MM", "CC", "KG", name: "706");

			var items = new List<ICartonisableItem>();
			items.AddNew(2, product402);
			items.AddNew(4, product406);
			items.AddNew(4, product407);
			items.AddNew(7, product412);
			items.AddNew(7, product416);
			items.AddNew(3, product502);
			items.AddNew(8, product506);
			items.AddNew(2, product510);
			items.AddNew(4, product514);
			items.AddNew(4, product516);
			items.AddNew(6, product518);
			items.AddNew(5, product532);
			items.AddNew(3, product741);
			items.AddNew(4, product742);
			items.AddNew(5, product803);
			items.AddNew(9, product652);
			items.AddNew(9, product653);
			items.AddNew(9, product654);
			items.AddNew(4, product409);
			items.AddNew(5, product413);
			items.AddNew(4, product417);
			items.AddNew(4, product420);
			items.AddNew(4, product421);
			items.AddNew(3, product427);
			items.AddNew(3, product429);
			items.AddNew(5, product503);
			items.AddNew(2, product511);
			items.AddNew(2, product515);
			items.AddNew(5, product517);
			items.AddNew(4, product525);
			items.AddNew(2, product533);
			items.AddNew(5, product626);
			items.AddNew(5, product676);
			items.AddNew(3, product677);
			items.AddNew(5, productO201);
			items.AddNew(4, productO204);
			items.AddNew(5, productO211);
			items.AddNew(5, productO212);
			items.AddNew(3, product667);
			items.AddNew(3, product706);
			items.AddNew(1, product402);
			items.AddNew(1, product406);
			items.AddNew(1, product407);
			items.AddNew(1, product412);
			items.AddNew(1, product416);
			items.AddNew(1, product502);
			items.AddNew(1, product506);
			items.AddNew(1, product510);
			items.AddNew(1, product514);
			items.AddNew(1, product516);
			items.AddNew(1, product518);
			items.AddNew(1, product532);
			items.AddNew(1, product741);
			items.AddNew(1, product742);
			items.AddNew(1, product803);
			items.AddNew(1, product652);
			items.AddNew(1, product653);
			items.AddNew(1, product654);
			items.AddNew(1, product409);
			items.AddNew(1, product413);
			items.AddNew(1, product417);
			items.AddNew(1, product420);
			items.AddNew(1, product421);
			items.AddNew(1, product427);
			items.AddNew(1, product429);
			items.AddNew(1, product503);
			items.AddNew(1, product511);
			items.AddNew(1, product515);
			items.AddNew(1, product517);
			items.AddNew(1, product525);
			items.AddNew(1, product533);
			items.AddNew(1, product626);
			items.AddNew(1, product676);
			items.AddNew(1, product677);
			items.AddNew(1, productO201);
			items.AddNew(1, productO204);
			items.AddNew(1, productO211);
			items.AddNew(1, productO212);
			items.AddNew(1, product667);
			items.AddNew(1, product706);

			var productLookup = items.ToDictionary(i => i.PK, i => i.ItemDefinition.PK);

			var result = Algorithm.CartoniseItems(items, boxes);
			var expect = new List<ICartonWithItems>
				{
					new CartonWithItems { CartonPK = box3.PK },
					new CartonWithItems { CartonPK = box4.PK },
				};

			AssertResult(expect, result, boxes);
			AssertEquals(
				"Should have unique products in all boxes.",
				items.Select(i => i.ItemDefinition.PK).Distinct().Count(),
				result.Sum(r => r.Items.Select(i => productLookup[i.CartonisableItemPK]).Distinct().Count()));
		}

		public void TestEqualCost_KeepSameProductsTogether_WithDifferentLocations()
		{
			var box = CartonisationTestHelper.CreateCarton(2, 2, 2, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var boxes = new List<ICartonDefinition> { box };
			var product1 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product1");
			var product2 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product2");

			var location1 = "LOCATION1";
			var location2 = "LOCATION2";

			var items = new List<ICartonisableItem>();
			var product1_1 = items.AddNew(1, product1, location1);
			var product2_1 = items.AddNew(1, product2, location1);
			var product1_2 = items.AddNew(1, product1, location2);
			var product2_2 = items.AddNew(1, product2, location2);

			// Minimise impact of randomness from stochastic optimisation techniques
			for (int i = 0; i < 10; i++)
			{
				var result = Algorithm.CartoniseItems(items, boxes);

				var expect = new List<ICartonWithItems>
				{
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
				};

				AssertResult(expect, result, boxes);
				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == product1_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == product1_2.PK)));

				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == product2_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == product2_2.PK)));
			}
		}

		public void TestEqualCost_KeepSameLocationsTogether()
		{
			var box = CartonisationTestHelper.CreateCarton(2, 2, 2, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var boxes = new List<ICartonDefinition> { box };
			var product = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m);

			var location1 = "LOCATION1";
			var location2 = "LOCATION2";

			var items = new List<ICartonisableItem>();
			var location1_1 = items.AddNew(1, product, location1);
			var location2_1 = items.AddNew(1, product, location2);
			var location1_2 = items.AddNew(1, product, location1);
			var location2_2 = items.AddNew(1, product, location2);

			// Minimise impact of randomness from stochastic optimisation techniques
			for (int i = 0; i < 10; i++)
			{
				var result = Algorithm.CartoniseItems(items, boxes);

				var expect = new List<ICartonWithItems>
				{
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
				};

				AssertResult(expect, result, boxes);
				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == location1_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == location1_2.PK)));

				AssertEquals(
					true,
					result.Any(r =>
								r.Items.Count() == 2 &&
								r.Items.Any(item => item.CartonisableItemPK == location2_1.PK) &&
								r.Items.Any(item => item.CartonisableItemPK == location2_2.PK)));
			}
		}

		public void TestEqualCost_KeepSameProductsAndLocationsTogether()
		{
			const int Iterations = 100;

			var box = CartonisationTestHelper.CreateCarton(2, 2, 2, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var boxes = new List<ICartonDefinition> { box };
			var product1 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product1");
			var product2 = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 4m, 0.1m, name: "Product2");

			var location1 = "LOCATION1";
			var location2 = "LOCATION2";

			var items = new List<ICartonisableItem>();
			var product1_location1_1 = items.AddNew(1, product1, location1);
			var product1_location1_2 = items.AddNew(1, product1, location1);
			var product2_location1_1 = items.AddNew(1, product2, location1);
			var product2_location1_2 = items.AddNew(1, product2, location1);
			var product1_location2_1 = items.AddNew(1, product1, location2);
			var product1_location2_2 = items.AddNew(1, product1, location2);
			var product2_location2_1 = items.AddNew(1, product2, location2);
			var product2_location2_2 = items.AddNew(1, product2, location2);

			var expectedResults = new[]
			{
				(product1_location1_1, product1_location1_2),
				(product2_location1_1, product2_location1_2),
				(product1_location2_1, product1_location2_2),
				(product2_location2_1, product2_location2_2),
			};

			// Minimise impact of randomness from stochastic optimisation techniques
			for (int i = 0; i < Iterations; i++)
			{
				var rng = new Random();
				var shuffledItems = items.OrderBy(item => rng.Next()).ToArray();
				var result = Algorithm.CartoniseItems(shuffledItems, boxes);

				var expect = new List<ICartonWithItems>
				{
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
					new CartonWithItems { CartonPK = box.PK },
				};

				AssertResult(expect, result, boxes);

				foreach (var expectedResult in expectedResults)
				{
					var (item1, item2) = expectedResult;
					AssertEquals(
						true,
						result.Any(r =>
									r.Items.Count() == 2 &&
									r.Items.Any(item => item.CartonisableItemPK == item1.PK) &&
									r.Items.Any(item => item.CartonisableItemPK == item2.PK)));
				}
			}
		}

		#endregion

		#region Boundaries

		#region Dimension

		public void TestValidHeight()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(5, 1, 1, 1, 1));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(4, 4, 4, 1, 1));
		}

		public void TestValidLength()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(1, 5, 1, 1, 1));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(4, 4, 4, 1, 1));
		}

		public void TestValidWidth()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 5, 1, 1));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(4, 4, 4, 1, 1));
		}

		public void TestValid_Height_Width()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			var box2 = CartonisationTestHelper.CreateCarton(4, 4, 5, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(5, 4, 5, 1, 1));
			AssertProductCanNotFitInBox(box2, CartonisationTestHelper.CreateProduct(5, 4, 5, 1, 1));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(4, 4, 4, 1, 1));
		}

		#endregion

		#region Volume

		public void TestValidVolume()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 20, 1));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 10, 1));
		}

		public void TestValidMaxFillPercent()
		{
			var box1 = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 0.8m);
			AssertProductCanNotFitInBox(box1, CartonisationTestHelper.CreateProduct(1, 1, 1, 10, 1));
			var box2 = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanFitInBox(box2, CartonisationTestHelper.CreateProduct(1, 1, 1, 10, 1));
		}

		#endregion

		#region Weight

		public void TestValidWeight()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 20));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 10));
		}

		public void TestValidEmptyWeight()
		{
			var box = CartonisationTestHelper.CreateCarton(4, 4, 4, 20, 10, 10, 10, 1.0m);
			AssertProductCanNotFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 20));
			AssertProductCanFitInBox(box, CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 10));
		}

		#endregion

		#region Assert

		void AssertProductCanNotFitInBox(ICartonDefinition box, ICartonisableItemDefinition productNotFit)
		{
			var boxes = new List<ICartonDefinition> { box };
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(10, productNotFit);
			var order2 = orders.AddNew(10, productNotFit);
			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems> { };
			AssertResult(expect, result, boxes);
		}

		void AssertProductCanFitInBox(ICartonDefinition box, ICartonisableItemDefinition productFit)
		{
			var boxes = new List<ICartonDefinition> { box };
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(10, productFit);
			var order2 = orders.AddNew(10, productFit);
			var result = Algorithm.CartoniseItems(orders, boxes);

			AssertEquals(true, result.Any());
		}

		#endregion

		#endregion

		#region Split

		//#region Dim

		//public void TestOrderSplitByDim_Failed()
		//{
		//    var boxes = new List<ICartonDefinition>();
		//    var orders = new List<ICartonisableItem>();

		//    var order = orders.AddNew(qty: 2);

		//    var box = boxes.AddNew(maxVolume: 2, maxWeight: 2, maxUnits: 2);
		//    var result = Algorithm.CartoniseItems(orders, boxes);
		//    var expect = new List<ICartonWithItems>
		//    {
		//        new CartonWithItems { CartonPK = box.PK },
		//        new CartonWithItems { CartonPK = box.PK }
		//    };
		//    AssertResult(expect, result, boxes);
		//}
		//
		//#endregion

		#region Volume

		public void TestOrderSplitByVolume()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 20);

			var box = boxes.AddNew(10, 10, 10, 10, 10, 10, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};
			AssertResult(expect, result, boxes);
		}

		public void TestOrderSplitByFillPercent()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 10);

			var box = boxes.AddNew(10, 10, 10, 10, 10, 10, 0.4m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};
			AssertResult(expect, result, boxes);
		}

		#endregion

		#region Weight

		public void TestOrderSplitByWeight()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 10);
			var box = boxes.AddNew(10, 10, 10, 5, 10, 10, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};
			AssertResult(expect, result, boxes);
		}

		public void TestOrderSplitByEmptyWeight()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 10);
			var box = CartonisationTestHelper.CreateCarton(10, 10, 10, 7, 2, 10, 10, 1.0m);
			boxes.Add(box);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};
			AssertResult(expect, result, boxes);
		}

		#endregion

		#region MaxUnits

		public void TestOrderSplitByMaxUnits()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 10);
			var box = boxes.AddNew(10, 10, 10, 10, 10, 2, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};
			AssertResult(expect, result, boxes);
		}

		#endregion

		#endregion

		#region Validate Results

		#region Dim

		public void TestFindOptimalBox_DIM_OneDimension()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(height: 10);
			var order = orders.AddNew(1, product);

			var box5 = boxes.AddNew(length: 5, width: 5, height: 5, maxFillPercent: 0.8m);
			var box6 = boxes.AddNew(length: 6, width: 6, height: 6, maxFillPercent: 0.8m);
			var box10 = boxes.AddNew(length: 2, width: 2, height: 10, maxFillPercent: 0.8m);// Valid choice 

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box10.PK }
			};
			AssertResult(expect, result, boxes);

			AssertResultContains(result, box10.PK, order.PK, 1);
		}

		public void TestFindOptimalBox_DIM_TwoDimension()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(height: 10, width: 10);
			var order = orders.AddNew(1, product);

			boxes.AddNew(length: 5, width: 5, height: 5, maxFillPercent: 0.8m);
			boxes.AddNew(length: 6, width: 6, height: 6, maxFillPercent: 0.8m);
			boxes.AddNew(length: 9, width: 9, height: 10, maxFillPercent: 0.8m);
			var box10_10 = boxes.AddNew(length: 10, width: 10, height: 10, maxFillPercent: 0.8m);// Valid choice 

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box10_10.PK }
			};

			AssertResult(expect, result, boxes);
			AssertResultContains(result, box10_10.PK, order.PK, 1);
		}

		public void TestFindOptimalBox_DIM_ThreeDimension()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(height: 10, width: 10, length: 10);
			var order = orders.AddNew(1, product);

			boxes.AddNew(length: 5, width: 5, height: 5);
			boxes.AddNew(length: 6, width: 6, height: 6);
			boxes.AddNew(length: 9, width: 12, height: 10);
			var box20 = boxes.AddNew(length: 20, width: 20, height: 10);// Valid choice 

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box20.PK }
			};

			AssertResult(expect, result, boxes);
			AssertResultContains(result, box20.PK, order.PK, 1);
		}

		//public void TestSplitLines_ShouldSplitOrderToPutInBox_Failed()
		//{
		//    var boxes = new List<ICartonDefinition>();
		//    var orders = new List<ICartonisableItem>();

		//    var order = orders.AddNew(2, CartonisationTestHelper.CreateProduct(2, 2, 2));

		//    var box1 = boxes.AddNew(length: 1, width: 1, height: 1);
		//    var box3 = boxes.AddNew(length: 3, width: 3, height: 3);// Valid choice ( 2 * Box )

		//    var result = Algorithm.CartoniseItems(orders, boxes);

		//    var expect = new List<ICartonWithItems>
		//    {
		//        new CartonWithItems { CartonPK = box3.PK },
		//        new CartonWithItems { CartonPK = box3.PK }
		//    };
		//    AssertResult(expect, result, boxes);
		//}

		#endregion

		#region Weight

		public void TestValidResultWeight()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(weight: 10);
			var order = orders.AddNew(1, product);

			var box5 = boxes.AddNew(maxWeight: 1);
			var box6 = boxes.AddNew(maxWeight: 1);
			var box10 = boxes.AddNew(maxWeight: 10);// Valid choice 

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box10.PK }
			};
			AssertResult(expect, result, boxes);

			AssertResultContains(result, box10.PK, order.PK, 1);
		}

		public void TestValidResultEmptyWeight()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var weight = 5m;
			var emptyWeight = 1m;

			var order = orders.AddNew(qty: 10);

			var box = CartonisationTestHelper.CreateCarton(10, 10, 10, weight, emptyWeight, 10, 10, 1.0m);
			boxes.Add(box);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void TestValidResultEmptyWeight2()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(weight: 5);
			var order = orders.AddNew(1, product);

			var box2 = CartonisationTestHelper.CreateCarton(1, 1, 1, 10, 8, 1, 1, 1);
			var box4 = CartonisationTestHelper.CreateCarton(1, 1, 1, 10, 6, 1, 1, 1);
			var box5 = CartonisationTestHelper.CreateCarton(1, 1, 1, 10, 5, 1, 1, 1); // valid
			boxes.Add(box2);
			boxes.Add(box4);
			boxes.Add(box5);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box5.PK }
			};
			AssertResult(expect, result, boxes);

			AssertResultContains(result, box5.PK, order.PK, 1);
		}

		#endregion

		#region Max unit

		public void TestValidResultMaxUnit()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct();
			var order = orders.AddNew(10, product);

			var box5 = boxes.AddNew(length: 10, maxVolume: 10, maxUnits: 5);
			var box6 = boxes.AddNew(length: 10, maxVolume: 10, maxUnits: 6);
			var box10 = boxes.AddNew(length: 10, maxVolume: 10, maxWeight: 10);// Valid choice 

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box10.PK }
			};
			AssertResult(expect, result, boxes);

			AssertResultTotals(result, box10.PK, order.PK, 10);
		}

		#endregion

		#endregion

		#region Optimal box

		#region  simple case

		public void TestOneOrderOneBox()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew();
			var box = boxes.AddNew(1, 1, 1, 1, 1, 1, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void TestOneOrderTwoBoxes()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(50);
			var box = boxes.AddNew(1000, 1000, 1000, 1000, 1000, 25, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void Test2Orders1Boxs()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			orders.AddNew(1, product);
			orders.AddNew(1, product);

			var box = boxes.AddNew(1, 1, 1, 1, 1, 1, 1.0m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void Test2Orders2Boxs()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			orders.AddNew(1, product);
			orders.AddNew(1, product);

			var boxSmall = boxes.AddNew(1, 1, 1, 1, 1, 1, 1.0m);
			var boxMedium = boxes.AddNew(2, 2, 2, 2, 4, 2, 1.0m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxMedium.PK },
			};

			AssertResult(expect, result, boxes);
		}

		public void TestSmallBoxCostHigherThanMedium()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			orders.AddNew(1, product);
			orders.AddNew(1, product);

			var boxSmall = CartonisationTestHelper.CreateCarton(1, 1, 1, 1, 1, 1, 1, "M", "CC", "KG", 1);
			var boxMedium = CartonisationTestHelper.CreateCarton(2, 2, 2, 2, 4, 2, 1.0m, "M", "CC", "KG", 1);
			boxes.Add(boxSmall);
			boxes.Add(boxMedium);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxMedium.PK },
			};

			AssertResult(expect, result, boxes);
		}

		public void Test2Orders2Boxs90Percent()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			orders.AddNew(1, product);
			orders.AddNew(1, product);

			var boxSmall = boxes.AddNew(2, 2, 2, 2, 2, 2, .90m);
			var boxMedium = boxes.AddNew(2, 2, 2, 2, 4, 2, .90m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxMedium.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void Test1BigOrders1Boxs()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			orders.AddNew(qty: 3);

			var box = boxes.AddNew(1, 1, 1, 1, 1, 1, 1.0m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void Test4Orders3Boxs()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(qty: 9, product: CartonisationTestHelper.CreateProduct(1, 2, 3, 1, 1));
			var order2 = orders.AddNew(qty: 7, product: CartonisationTestHelper.CreateProduct(2, 1, 3, 2, 1));
			var order3 = orders.AddNew(qty: 17, product: CartonisationTestHelper.CreateProduct(2, 2, 3, 4, 1));
			var order4 = orders.AddNew(qty: 5, product: CartonisationTestHelper.CreateProduct(2, 2, 1, 2, 1));

			var boxSmall = boxes.AddNew(36, 24, 24, 15, 12, 99999, 0.80m);
			var boxMedium = boxes.AddNew(36, 36, 36, 25, 27, 99999, 0.75m);
			var boxLarg = boxes.AddNew(48, 48, 48, 40, 64, 99999, 0.75m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxSmall.PK },
				new CartonWithItems { CartonPK = boxLarg.PK },
				new CartonWithItems { CartonPK = boxLarg.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void TestMustFindFitBox()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(10, 10, 10, 1000, 1);
			var order1 = orders.AddNew(3, product1);
			var boxSize1 = boxes.AddNew(10, 10, 10, 10, 1000, 1, 1.0m);
			var boxSize2 = boxes.AddNew(11, 11, 11, 11, 1100, 1, 1.0m);
			var boxSize3 = boxes.AddNew(12, 12, 12, 12, 1200, 1, 1.0m);
			var result1 = Algorithm.CartoniseItems(orders, boxes);
			var expect1 = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxSize1.PK },
				new CartonWithItems { CartonPK = boxSize1.PK },
				new CartonWithItems { CartonPK = boxSize1.PK }
			};

			AssertResult(expect1, result1);

			orders.Clear();
			var product2 = CartonisationTestHelper.CreateProduct(11, 11, 11, 1100, 1);
			var order2 = orders.AddNew(3, product2);
			var result2 = Algorithm.CartoniseItems(orders, boxes);

			var expect2 = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxSize2.PK },
				new CartonWithItems { CartonPK = boxSize2.PK },
				new CartonWithItems { CartonPK = boxSize2.PK }
			};

			AssertResult(expect2, result2);
		}

		public void TestMaxFillPercent()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 10);
			var box100FillPercent = boxes.AddNew(10, 10, 10, 10, 10, 10, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box100FillPercent.PK }
			};

			AssertResult(expect, result, boxes);

			boxes.Clear();
			var box99FillPercent = boxes.AddNew(10, 10, 10, 10, 10, 10, 0.99m);

			result = Algorithm.CartoniseItems(orders, boxes);

			expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box99FillPercent.PK },
				new CartonWithItems { CartonPK = box99FillPercent.PK }
			};

			AssertResult(expect, result, boxes);
		}

		#region FindOptimalBox

		public void TestFindBestFitMultiBoxSize()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 12);
			var boxFit2Items = boxes.AddNew(1, 1, 2, 2, 2, 2, 1.0m);
			var boxFit3Items = boxes.AddNew(1, 1, 3, 3, 3, 3, 1.0m);
			var boxFit10Items = boxes.AddNew(1, 1, 10, 10, 10, 10, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxFit10Items.PK },
				new CartonWithItems { CartonPK = boxFit2Items.PK },
			};

			AssertResult(expect, result, boxes);
			AssertResultTotals(result, boxFit2Items.PK, order.PK, 2);
			AssertResultTotals(result, boxFit10Items.PK, order.PK, 10);
		}

		public void TestFindOptimalBox_1()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order = orders.AddNew(qty: 3);

			var box2 = boxes.AddNew(1, 1, 2, 2, 2, 2, 1.0m);
			var box3 = boxes.AddNew(1, 1, 3, 3, 3, 3, 1.0m);
			var box5 = boxes.AddNew(1, 1, 5, 5, 5, 5, 1.0m);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box3.PK }
			};
			AssertResult(expect, result, boxes);

			AssertResultTotals(result, box3.PK, order.PK, 3);
		}

		public void TestFindOptimalBox_2()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(qty: 3);
			var order2 = orders.AddNew(qty: 2);

			var box2 = boxes.AddNew(1, 1, 2, 2, 2, 2, 1.0m);
			var box3 = boxes.AddNew(1, 1, 3, 3, 3, 3, 1.0m);
			var box5 = boxes.AddNew(1, 1, 5, 5, 5, 5, 1.0m);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box5.PK }
			};

			AssertResult(expect, result, boxes);
			AssertResultTotals(result, box5.PK, order1.PK, 3);
			AssertResultTotals(result, box5.PK, order2.PK, 2);
		}

		public void TestFindOptimalBox_3()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(qty: 3);
			var order2 = orders.AddNew(qty: 1);

			var box2 = boxes.AddNew(1, 1, 2, 2, 2, 2, 0.80m);
			var box4 = boxes.AddNew(1, 1, 4, 4, 4, 4, 0.80m);
			var box5 = boxes.AddNew(1, 1, 5, 5, 5, 5, 0.80m);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box5.PK }
			};

			AssertResult(expect, result, boxes);
			AssertResultTotals(result, box5.PK, order1.PK, 3);
			AssertResultTotals(result, box5.PK, order2.PK, 1);
		}

		public void TestFindOptimalBox_4()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(qty: 3);
			var order2 = orders.AddNew(qty: 5);

			var box2 = boxes.AddNew(1, 1, 2, 2, 3, 2, 0.70m);
			var box3 = boxes.AddNew(1, 1, 4, 4, 4, 4, 0.70m);
			var box5 = boxes.AddNew(1, 1, 5, 5, 6, 5, 0.70m);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box5.PK },
				new CartonWithItems { CartonPK = box5.PK }
			};

			AssertResult(expect, result, boxes);
			AssertResultTotals(result, box5.PK, order1.PK, 3);
			AssertResultTotals(result, box5.PK, order2.PK, 5);
		}

		//public void TestFindOptimalBox_OneByOneAreFitButNotTogether_Failed()
		//{
		//    var boxes = new List<ICartonDefinition>();
		//    var orders = new List<ICartonisableItem>();

		//    var order1 = orders.AddNew(1, CartonisationTestHelper.CreateProduct(1, 1, 10));
		//    var order2 = orders.AddNew(1, CartonisationTestHelper.CreateProduct(2, 2, 2));

		//    var box1 = boxes.AddNew(length: 2, width: 2, height: 2);
		//    var box2 = boxes.AddNew(length: 2, width: 2, height: 4);
		//    var box3 = boxes.AddNew(length: 2, width: 2, height: 10);
		//    var box4 = boxes.AddNew(length: 3, width: 3, height: 10); // Valid choice

		//    var result = Algorithm.CartoniseItems(orders, boxes);

		//    var expect = new List<ICartonWithItems>
		//    {
		//        new CartonWithItems { CartonPK = box4.PK }
		//    };

		//    AssertResult(expect, result, boxes);
		//    AssertResultContains(result, box4.PK, order1.PK, 1);
		//    AssertResultContains(result, box4.PK, order2.PK, 1);
		//}

		public void TestOnlyWhenMaxFillPercentIs100CanFitItemsToBoxes()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var order1 = orders.AddNew(1, CartonisationTestHelper.CreateProduct(10, 10, 10));
			var order2 = orders.AddNew(1, CartonisationTestHelper.CreateProduct(10, 10, 10));

			var box9 = boxes.AddNew(length: 9, width: 9, height: 9, maxFillPercent: 1);
			var box10 = boxes.AddNew(length: 10, width: 10, height: 10, maxFillPercent: 1);// Valid choice 
			var box11 = boxes.AddNew(length: 11, width: 11, height: 11, maxFillPercent: 1);

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box10.PK },
				new CartonWithItems { CartonPK = box10.PK }
			};

			AssertResult(expect, result, boxes);

			boxes.Clear();
			var box9_80 = boxes.AddNew(length: 9, width: 9, height: 9, maxFillPercent: 0.5m);
			var box10_80 = boxes.AddNew(length: 10, width: 10, height: 10, maxFillPercent: 0.5m);
			var box11_80 = boxes.AddNew(length: 11, width: 11, height: 11, maxFillPercent: 0.5m);

			result = Algorithm.CartoniseItems(orders, boxes);

			expect = new List<ICartonWithItems>
			{
			};

			AssertResult(expect, result, boxes);
		}

		public void TestFindOptimalBox_DoesNotFitInAnyOrientation_KeepUprightEnabled() => TestFindOptimalBox_DoesNotFitInAnyOrientation(keepUpright: true);
		public void TestFindOptimalBox_DoesNotFitInAnyOrientation_KeepUprightDisabled() => TestFindOptimalBox_DoesNotFitInAnyOrientation(keepUpright: false);

		void TestFindOptimalBox_DoesNotFitInAnyOrientation(bool keepUpright)
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 1m, width: 2m, height: 7m, keepUpright: keepUpright);
			var order1 = orders.AddNew(1, product1);

			var smallBox = boxes.AddNew(length: 1m, width: 1m, height: 7m);
			var bigBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, bigBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_NoRotationRequired_ExactDimensions_KeepUprightEnabled() => TestFindOptimalBox_NoRotationRequired(exactDimensions: true, keepUpright: true);
		public void TestFindOptimalBox_NoRotationRequired_ExactDimensions_KeepUprightDisabled() => TestFindOptimalBox_NoRotationRequired(exactDimensions: true, keepUpright: false);
		public void TestFindOptimalBox_NoRotationRequired_CartonLarger_KeepUprightEnabled() => TestFindOptimalBox_NoRotationRequired(exactDimensions: false, keepUpright: true);
		public void TestFindOptimalBox_NoRotationRequired_CartonLarger_KeepUprightDisabled() => TestFindOptimalBox_NoRotationRequired(exactDimensions: false, keepUpright: false);

		void TestFindOptimalBox_NoRotationRequired(bool exactDimensions, bool keepUpright)
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 1m, width: 2m, height: 7m, keepUpright: keepUpright);
			var order1 = orders.AddNew(1, product1);

			var smallBox = boxes.AddNew(length: exactDimensions ? 1m : 2m, width: exactDimensions ? 2m : 5m, height: exactDimensions ? 7m : 10m);
			var bigBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, smallBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_LengthRequiresRotationToWidth_KeepUprightEnabled() => TestFindOptimalBox_LengthRequiresRotationToWidth(keepUpright: true);
		public void TestFindOptimalBox_LengthRequiresRotationToWidth_KeepUprightDisabled() => TestFindOptimalBox_LengthRequiresRotationToWidth(keepUpright: false);

		void TestFindOptimalBox_LengthRequiresRotationToWidth(bool keepUpright)
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 2m, width: 1m, height: 3m, keepUpright: keepUpright);
			var order1 = orders.AddNew(1, product1);

			var smallBox = boxes.AddNew(length: 1m, width: 2m, height: 3m);
			var bigBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, smallBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_WidthRequiresRotationToLength_KeepUprightEnabled() => TestFindOptimalBox_WidthRequiresRotationToLength(keepUpright: true);
		public void TestFindOptimalBox_WidthRequiresRotationToLength_KeepUprightDisabled() => TestFindOptimalBox_WidthRequiresRotationToLength(keepUpright: false);

		void TestFindOptimalBox_WidthRequiresRotationToLength(bool keepUpright)
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 1m, width: 2m, height: 3m, keepUpright: keepUpright);
			var order1 = orders.AddNew(1, product1);

			var smallBox = boxes.AddNew(length: 2m, width: 1m, height: 3m);
			var bigBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, smallBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_HeightRequiresRotation_KeepUprightEnabled()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 1m, width: 1m, height: 7m, keepUpright: true);
			var order1 = orders.AddNew(1, product1);

			var wideBox = boxes.AddNew(length: 7m, width: 1m, height: 1m); // Valid with rotation, but not valid due to height
			var tallBox = boxes.AddNew(length: 2m, width: 2m, height: 7m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, tallBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_HeightRequiresRotation_KeepUprightDisabled()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 1m, width: 1m, height: 7m, keepUpright: false);
			var order1 = orders.AddNew(1, product1);

			var wideBox = boxes.AddNew(length: 7m, width: 1m, height: 1m);
			var tallBox = boxes.AddNew(length: 2m, width: 2m, height: 7m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, wideBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_LengthRequiresRotationToHeight_KeepUprightEnabled()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 3m, width: 5m, height: 2m, keepUpright: true);
			var order1 = orders.AddNew(1, product1);

			var invalidBox = boxes.AddNew(length: 2m, width: 3m, height: 5m); // Valid with some rotation, but other dimensions are in contention for height
			var validBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, validBox.PK, order1.PK, 1);
		}

		public void TestFindOptimalBox_LengthRequiresRotationToHeight_KeepUprightDisabled()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product1 = CartonisationTestHelper.CreateProduct(length: 3m, width: 5m, height: 2m, keepUpright: false);
			var order1 = orders.AddNew(1, product1);

			var invalidBoxIfKeepUpright = boxes.AddNew(length: 2m, width: 3m, height: 5m);
			var validBox = boxes.AddNew(length: 10m, width: 10m, height: 10m);

			var result = Algorithm.CartoniseItems(orders, boxes);
			AssertResultContains(result, invalidBoxIfKeepUpright.PK, order1.PK, 1);
		}

		#endregion

		#region  not valid for any boxes

		public void TestOrderNotFitInBox()
		{
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var product = CartonisationTestHelper.CreateProduct(1, 2, 1, 1, 1);
			var order = orders.AddNew(1, product);
			var box = boxes.AddNew(1, 1, 1, 1, 1, 1, 1.0m);
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>();
			AssertResult(expect, result, boxes);

			// just fit
			orders.Clear();
			var productCanFit = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			var orderCanFit = orders.AddNew(1, productCanFit);
			var resultCanFit = Algorithm.CartoniseItems(orders, boxes);
			var expectCanFit = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK }
			};

			AssertResult(expectCanFit, resultCanFit);
		}

		#endregion

		#endregion

		#region Multi Orders

		#region TestFindBestBoxForCombinationOfTheOrdersByVolume

		public void TestFindBestBoxForCombinationOfTheOrdersByVolume()
		{
			var product1 = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			var product2 = CartonisationTestHelper.CreateProduct(1, 1, 1, 2, 1);
			var product3 = CartonisationTestHelper.CreateProduct(1, 1, 1, 5, 1);
			var product4 = CartonisationTestHelper.CreateProduct(1, 1, 1, 7, 1);
			var order1 = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1, product1);
			var order2 = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1, product2);
			var order3 = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1, product3);
			var order4 = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1, product4);

			var boxes = new List<ICartonDefinition>();
			for (var i = 3; i <= 12; i = i + 3)
			{
				boxes.Add(CartonisationTestHelper.CreateCarton(2, 2, 2, 2, i, 2, 1.0m));
			}

			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order1, order2, order3, order4 }, boxes.Where(b => b.Volume != 6m));

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxes.Single(b => b.Volume == 3m).PK },
				new CartonWithItems { CartonPK = boxes.Single(b => b.Volume == 12m).PK },
			};
			AssertResult(expect, result, boxes);

			result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order1, order2, order3, order4 }, boxes.Where(b => b.Volume != 3m));

			expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = boxes.Single(b => b.Volume == 9m).PK },
				new CartonWithItems { CartonPK = boxes.Single(b => b.Volume == 6m).PK },
			};

			AssertResult(expect, result, boxes);
		}

		#endregion

		#region TestCombinationShouldBeSplit

		public void TestCombinationShouldbeSplit()
		{
			var orders = new List<ICartonisableItem>();
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1);
			var order1 = orders.AddNew();
			var order2 = orders.AddNew();

			//maxWeight limitation
			AssertCombinationAreNotFit(orders, CartonisationTestHelper.CreateCarton(1, 1, 1, 1.5m, 2, 2, 1.0m), 2);

			//maxVolume limitation
			AssertCombinationAreNotFit(orders, CartonisationTestHelper.CreateCarton(1, 1, 1, 2, 1.5m, 2, 1.0m), 2);

			//maxUnits limitation
			AssertCombinationAreNotFit(orders, CartonisationTestHelper.CreateCarton(1, 1, 1, 2, 2, 1, 1.0m), 2);

			//maxFillPercent limitation
			AssertCombinationAreNotFit(orders, CartonisationTestHelper.CreateCarton(1, 1, 1, 2, 2, 2, 0.9m), 2);

			//no limitation
			AssertCombinationAreNotFit(orders, CartonisationTestHelper.CreateCarton(1, 1, 1, 2, 2, 2, 1.0m), 1);
		}

		void AssertCombinationAreNotFit(List<ICartonisableItem> orders, ICartonDefinition box, int numberOfResultBoxes)
		{
			var boxes = new List<ICartonDefinition> { box };
			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>();
			for (int i = 0; i < numberOfResultBoxes; i++)
			{
				expect.Add(new CartonWithItems { CartonPK = box.PK });
			}
			AssertResult(expect, result, boxes);
		}

		#endregion

		#region TestProductFitIntoBoxesNoSpaceLeft

		public void TestProductFitIntoBoxesNoSpaceLeft()
		{
			var box1 = CartonisationTestHelper.CreateCarton(10m, 10m, 10m, 30m, 100m, 500m, 1m);
			var box2 = CartonisationTestHelper.CreateCarton(5m, 5m, 5m, 30m, 80m, 500m, 1m);
			var product1 = CartonisationTestHelper.CreateProduct(2m, 2m, 2m, 10m, 0.1m);
			var product2 = CartonisationTestHelper.CreateProduct(3m, 3m, 3m, 8m, 0.1m);

			var boxes = new List<ICartonDefinition> { box1, box2 };
			var products = new List<ICartonisableItem>();

			for (int i = 0; i < 20; i++)
			{
				products.AddNew(1, product1);
				products.AddNew(1, product2);
			}

			var expect = new List<ICartonWithItems>();

			for (int i = 0; i < 2; i++)
			{
				expect.Add(new CartonWithItems { CartonPK = box1.PK });
				expect.Add(new CartonWithItems { CartonPK = box2.PK });
			}

			var result = Algorithm.CartoniseItems(products, boxes);

			AssertResult(expect, result, boxes);
		}

		#endregion

		#endregion

		#region test with different orders and box size

		#region TestFindBestBoxForOrders

		public void TestFindBestBoxOrders_1()
		{
			var boxes = new List<ICartonDefinition>();
			var box2 = boxes.AddNew(2, 2, 2, 2, 2, 2, 1.0m);
			var box3 = boxes.AddNew(3, 3, 3, 3, 3, 3, 1.0m);
			var orders = new List<ICartonisableItem>();

			for (int i = 0; i < 6; i++)
			{
				orders.AddNew();
			}

			var result = Algorithm.CartoniseItems(orders, boxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box3.PK },
				new CartonWithItems { CartonPK = box3.PK }
			};

			AssertResult(expect, result, boxes);
		}

		public void TestFindBestBoxOrders_2()
		{
			var orders = new List<ICartonisableItem>();

			orders.AddNew(7, TestData.Products.ProductVolume1_5Weight1_5);
			orders.AddNew(3, TestData.Products.ProductVolume1Weight1);
			orders.AddNew(4, TestData.Products.ProductVolume2Weight2);
			orders.AddNew(2, TestData.Products.ProductVolume3Weight1);
			orders.AddNew(3, TestData.Products.ProductVolume0_5Weight0_5);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = TestData.Boxes.Box36.PK },
				new CartonWithItems { CartonPK = TestData.Boxes.Box8.PK }
			};

			var result = Algorithm.CartoniseItems(orders, TestData.GetBoxes);
			AssertResult(expect, result, TestData.GetBoxes);
		}

		public void TestFindBestBoxOrders_3()
		{
			var orders = new List<ICartonisableItem>();
			orders.AddNew(2, TestData.Products.ProductVolume0_5Weight0_5);
			orders.AddNew(3, TestData.Products.ProductVolume0_5Weight0_5);

			var result = Algorithm.CartoniseItems(orders, TestData.GetBoxes);
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = TestData.Boxes.Box4_629.PK }
			};

			AssertResult(expect, result, TestData.GetBoxes);
		}

		public void TestFindBestBoxOrders_4()
		{
			var orders = new List<ICartonisableItem>();
			orders.AddNew(4, TestData.Products.ProductVolume0_5Weight0_5);
			orders.AddNew(2, TestData.Products.ProductVolume1Weight1);
			orders.AddNew(3, TestData.Products.ProductVolume2Weight2);
			orders.AddNew(7, TestData.Products.ProductVolume3Weight1);

			var result = Algorithm.CartoniseItems(orders, TestData.GetBoxes);

			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = TestData.Boxes.Box36.PK },
				new CartonWithItems { CartonPK = TestData.Boxes.Box4_629.PK }
			};

			AssertResult(expect, result, TestData.GetBoxes);
		}

		#endregion

		#endregion

		#endregion

		#region Convert

		#region ConvertLength

		public void TestConvertLengthProduct()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 2, 1, 1, Constants.Length.Metres, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 2, product);
			var box = CartonisationTestHelper.CreateCarton(100, 100, 200, 1, 1, 1, 1.0m, Constants.Length.Centimetres, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode);

			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK  }
			};

			AssertResult(expect, result);
		}

		public void TestConvertLengthBox()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1, Constants.Length.Centimetres, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 2, product);
			var box = CartonisationTestHelper.CreateCarton(1, 1, 2, 2, 2, 2, 1.0m, Constants.Length.Metres, CartonisationAlgorithm.DefaultVolumeUnitCode, CartonisationAlgorithm.DefaultWeightUnitCode);
			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK  }
			};
			AssertResult(expect, result);
		}

		#endregion

		#region ConvertVolume

		public void TestConvertVolumeProduct()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1, CartonisationAlgorithm.DefaultLengthUnitCode, Constants.Volume.CubicDecimetres, CartonisationAlgorithm.DefaultWeightUnitCode);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 2, product);
			var box = CartonisationTestHelper.CreateCarton(1, 1, 1, 1, 1000, 1, 1.0m, CartonisationAlgorithm.DefaultLengthUnitCode, Constants.Volume.CubicCentimeters, CartonisationAlgorithm.DefaultWeightUnitCode);
			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK  }
			};
			AssertResult(expect, result);
		}

		public void TestConvertVolumeBox()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1000, 1, CartonisationAlgorithm.DefaultLengthUnitCode, Constants.Volume.CubicCentimeters, CartonisationAlgorithm.DefaultWeightUnitCode);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1, product);
			var box = CartonisationTestHelper.CreateCarton(1, 1, 1, 1, 1, 1, 1.0m, CartonisationAlgorithm.DefaultLengthUnitCode, Constants.Volume.CubicDecimetres, CartonisationAlgorithm.DefaultWeightUnitCode);

			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK  }
			};

			AssertResult(expect, result);
		}

		#endregion

		#region ConvertWeight

		public void TestConvertWeightProduct()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, Constants.Weight.Kilograms);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 2, product);
			var box = CartonisationTestHelper.CreateCarton(1, 1, 1, 1000, 2, 2, 1.0m, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, Constants.Weight.Grams);
			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK },
				new CartonWithItems { CartonPK = box.PK  }
			};
			AssertResult(expect, result);
		}

		public void TestConvertWeightBox()
		{
			var product = CartonisationTestHelper.CreateProduct(1, 1, 1, 1, 1, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, Constants.Weight.Grams);
			var order = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 1000000, product);
			var box = CartonisationTestHelper.CreateCarton(1, 1, 1, 1, 1000000, 1000000, 1.0m, CartonisationAlgorithm.DefaultLengthUnitCode, CartonisationAlgorithm.DefaultVolumeUnitCode, Constants.Weight.Tonnes);

			var result = Algorithm.CartoniseItems(new List<ICartonisableItem> { order }, new List<ICartonDefinition> { box });
			var expect = new List<ICartonWithItems>
			{
				new CartonWithItems { CartonPK = box.PK  }
			};

			AssertResult(expect, result);
		}

		#endregion

		#endregion

		#region TestCartoniseItems_RandomizeLockIssue

		public void TestCartoniseItems_RandomizeLockIssue()
		{
			const int numberOfTimesToRun = 50;

			var ctnSml = CartonisationTestHelper.CreateCarton(34.5m, 32.2m, 33.7m, 12m, 37437.33m, 99999m, 0.8m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, Constants.Weight.Kilograms);
			var ctnMed = CartonisationTestHelper.CreateCarton(36.9m, 35.9m, 41.7m, 14m, 55240m, 99999m, 0.8m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, Constants.Weight.Kilograms);
			var ctnLrg = CartonisationTestHelper.CreateCarton(38.9m, 40.4m, 48.1m, 28m, 75592.036m, 99999m, 0.8m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, Constants.Weight.Kilograms);

			var a = CartonisationTestHelper.CreateProduct(12m, 14m, 16m, 2688m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var b = CartonisationTestHelper.CreateProduct(12m, 14m, 21m, 3228m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var c = CartonisationTestHelper.CreateProduct(24m, 16m, 22m, 8448m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);

			var boxes = new List<ICartonDefinition> { ctnSml, ctnMed, ctnLrg };

			var orders = new List<ICartonisableItem>();
			orders.AddNew(58, a);
			orders.AddNew(28, b);
			orders.AddNew(84, c);

			// run test multiple times to increase odds of catching deadlocks since it was happening only once every 30-50 runs.
			for (int i = 0; i < numberOfTimesToRun; i++)
			{
				var algorithm = new CartonisationAlgorithm();
				AssertNoExceptionThrown("Should not get deadlock or timeout exception.", () =>
				{
					var result = algorithm.CartoniseItems(orders, boxes);
					AssertEquals(7, result.Count());
				});
			}
		}

		#endregion

		#region TestCartonisationProductOrderBug

		public void TestCartonisationProductOrderBug()
		{
			var carton = CartonisationTestHelper.CreateCarton(999m, 999m, 999m, 999m, 997.003m, 12m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicMetres, Constants.Weight.Kilograms);
			var p1 = CartonisationTestHelper.CreateProduct(0, 0, 0, 0.990m, 0.5m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var p2 = CartonisationTestHelper.CreateProduct(0, 0, 0, 0.990m, 0.5m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var p3 = CartonisationTestHelper.CreateProduct(0, 0, 0, 0.990m, 0.5m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var p4 = CartonisationTestHelper.CreateProduct(0, 0, 0, 0.990m, 0.5m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);

			var boxes = new List<ICartonDefinition>();
			boxes.Add(carton);

			var orders = new List<ICartonisableItem>();
			orders.AddNew(12, p1);
			orders.AddNew(3, p2);
			orders.AddNew(12, p3);
			orders.AddNew(3, p4);

			var algorithm = new CartonisationAlgorithm();
			var result = algorithm.CartoniseItems(orders, boxes);
			AssertEquals(3, result.Count());

			result = algorithm.CartoniseItems(orders, boxes);
			AssertEquals(3, result.Count());
		}

		#endregion

		#region TestCartonisationWhenOneDimentionIsNotFitWithBox_Bug

		public void TestCartonisationWhenOneDimentionIsNotFitWithBox_Bug()
		{
			var sml = CartonisationTestHelper.CreateCarton(56m, 56m, 56m, 99m, 199999m, 9999m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var med = CartonisationTestHelper.CreateCarton(64m, 64m, 64m, 99m, 250000m, 9999m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var lar = CartonisationTestHelper.CreateCarton(64m, 78m, 64m, 99m, 300000m, 9999m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var boxes = new List<ICartonDefinition> { sml, med, lar };

			var productFitInLargeBox = CartonisationTestHelper.CreateProduct(24m, 24m, 78m, 20000m, 2m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			var orders = new List<ICartonisableItem>();
			orders.AddNew(1, productFitInLargeBox);

			var result = Algorithm.CartoniseItems(orders, boxes);
			var expect = new List<ICartonWithItems>
						{
								new CartonWithItems { CartonPK = lar.PK  }
						};
			AssertResult(expect, result, boxes);

			// add small item to order to make sure algorithm check all items
			var smallProduct = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 1m, 1m, Constants.Length.Centimetres, Constants.Volume.CubicCentimeters, Constants.Weight.Kilograms);
			orders.AddNew(1, smallProduct);

			// create new instanse
			var algorithm = new CartonisationAlgorithmForTesting();
			result = algorithm.CartoniseItems(orders, boxes);
			AssertResult(expect, result, boxes);
		}

		#endregion

		#region TestCartonisation_InconsistentMaxBoxWeigthAndVolume

		public void TestCartonisation_InconsistentMaxBoxWeigthAndVolume()
		{
			var boxesToUse = new[]
			{
				CartonisationTestHelper.CreateCarton(20m, 20m, 20m, 30m, 8000m, 99999m, 0.80m), // big carton - max volume
				CartonisationTestHelper.CreateCarton(10m, 10m, 10m, 50m, 1000m, 99999m, 0.80m), // small crate - max weight
			};

			var product = CartonisationTestHelper.CreateProduct(9m, 9m, 9m, 729m, 10m);
			var itemsToCartonise = new[]
			{
				CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 4m, product) // 3 items can fit in bigger box and 1 item in smaller box.
			};

			var result = Algorithm.CartoniseItems(itemsToCartonise, boxesToUse);
			AssertEquals("All items should fit in 2 boxes.", 2, result.Count());

			var biggerBox = result.Single(b => b.CartonPK == boxesToUse[0].PK);
			AssertEquals("Should pack up to maximum weight into the big box.", 1, biggerBox.Items.Count());
			biggerBox.Items.Single(i => i.CartonisableItemPK == itemsToCartonise[0].PK && i.Quantity == 3m);

			var smallerBox = result.Single(b => b.CartonPK == boxesToUse[1].PK);
			AssertEquals("Should pack the remainder into smaller box.", 1, smallerBox.Items.Count());
			smallerBox.Items.Single(i => i.CartonisableItemPK == itemsToCartonise[0].PK && i.Quantity == 1m);
		}

		#endregion

		#region TestCartonisation_InconsistentMaxBoxWeightVolumeAndQty

		public void TestCartonisation_InconsistentMaxBoxWeightVolumeAndQty()
		{
			var boxesToUse = new[]
			{
				CartonisationTestHelper.CreateCarton(20m, 20m, 20m, 50m, 8000m, 3m, 0.80m), // max volume and weight
				CartonisationTestHelper.CreateCarton(10m, 10m, 10m, 20m, 1000m, 99999m, 0.80m), // max qty
			};

			var product = CartonisationTestHelper.CreateProduct(9m, 9m, 9m, 729m, 1m);
			var itemsToCartonise = new[]
			{
				CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 4m, product) // can only have 3 items in biggest box
			};

			var result = Algorithm.CartoniseItems(itemsToCartonise, boxesToUse);
			AssertEquals("All items should fit in 2 boxes.", 2, result.Count());

			var biggerBox = result.Single(b => b.CartonPK == boxesToUse[0].PK);
			AssertEquals("Should pack up to maximum quantity into the big box.", 1, biggerBox.Items.Count());
			biggerBox.Items.Single(i => i.CartonisableItemPK == itemsToCartonise[0].PK && i.Quantity == 3m);

			var smallerBox = result.Single(b => b.CartonPK == boxesToUse[1].PK);
			AssertEquals("Should pack the remainder into smaller box.", 1, smallerBox.Items.Count());
			smallerBox.Items.Single(i => i.CartonisableItemPK == itemsToCartonise[0].PK && i.Quantity == 1m);
		}

		#endregion

		#region TestChunkItemsIntoSmallerPieces_ReturnsSplitByGreatestCommonDivisor

		public void TestChunkItemsIntoSmallerPieces_ReturnsSplitByGreatestCommonDivisor()
		{
			var box1 = CartonisationTestHelper.CreateCarton(100m, 100m, 100m, 100m, 100m, 12m, 1m);
			var box2 = CartonisationTestHelper.CreateCarton(100m, 100m, 100m, 100m, 100m, 18m, 1m);
			var box3 = CartonisationTestHelper.CreateCarton(100m, 100m, 100m, 100m, 100m, 24m, 1m);
			var product = CartonisationTestHelper.CreateProduct(1m, 1m, 1m, 1m, 1m);

			var item = CartonisationTestHelper.CreateOrderLine(Guid.NewGuid(), 15m, product);

			var result = CartonisationAlgorithm.ChunkItemsIntoSmallerPieces(
				CartonisationAlgorithm.Convert(new[] { item }),
				CartonisationAlgorithm.Convert(new[] { box1, box2, box3 }));

			AssertEquals("Item should be split into two lines with six units, and three lines with a single unit.", 5, result.Count);
			AssertEquals("Two lines with six units should exist.", 2, result.Where(i => i.OriginalPK == item.PK && i.QTY == 6m).Count());
			AssertEquals("Three lines with one unit should exist.", 3, result.Where(i => i.OriginalPK == item.PK && i.QTY == 1m).Count());
		}

		#endregion

		#region TestFirstFitItemsAlgorithm_PutSameItemsIntoSameBox

		public void TestFirstFitItemsAlgorithm_PutSameItemsIntoSameBox()
		{
			// can fit only 3 items per box
			var box = new Carton(Guid.NewGuid(), 3m, 3m, 3m, 27m, 1.0m, 3m, 3m, 1);

			var product = new CartonisableItemDefinition(Guid.NewGuid(), 1m, 1m, 1m, 1m, 1m);
			var itemA = new CartonisableItem(product) { OriginalPK = Guid.NewGuid(), QTY = 1m };
			var itemB = new CartonisableItem(product) { OriginalPK = Guid.NewGuid(), QTY = 1m };
			var itemC = new CartonisableItem(product) { OriginalPK = Guid.NewGuid(), QTY = 1m };

			var sameItemGuid = Guid.NewGuid(); // will have same guid when a single item with big qty gets split into multiple smaller chunks based on size of boxes
			var itemX1 = new CartonisableItem(product) { OriginalPK = sameItemGuid, QTY = 1m };
			var itemX2 = new CartonisableItem(product) { OriginalPK = sameItemGuid, QTY = 1m };
			var itemX3 = new CartonisableItem(product) { OriginalPK = sameItemGuid, QTY = 1m };
			var itemX4 = new CartonisableItem(product) { OriginalPK = sameItemGuid, QTY = 1m };
			var itemX5 = new CartonisableItem(product) { OriginalPK = sameItemGuid, QTY = 1m };

			// test (A, X, X), (B, C)
			var result1 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemX1, itemX2, itemB, itemC }, Enumerable.Range(0, 5));
			AssertEquals("Should be able to pack everyting into 2 boxes.", 2, result1.Count);
			AssertRoute(result1, new[] { itemA, itemX1, itemX2 });
			AssertRoute(result1, new[] { itemB, itemC });

			// test (A, B), (X, X, C)
			var result2 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemB, itemX1, itemX2, itemC }, Enumerable.Range(0, 5));
			AssertEquals("Should be able to pack everyting into 2 boxes.", 2, result2.Count);
			AssertRoute(result2, new[] { itemA, itemB });
			AssertRoute(result2, new[] { itemX1, itemX2, itemC });

			// test (A, X, B), (X, C)
			var result3 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemX1, itemB, itemX2, itemC }, Enumerable.Range(0, 5));
			AssertEquals("Should be able to pack everyting into 2 boxes.", 2, result3.Count);
			AssertRoute(result3, new[] { itemA, itemX1, itemB });
			AssertRoute(result3, new[] { itemX2, itemC });

			// test (X, A, X), (B, X)
			var result4 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemX1, itemA, itemX2, itemB, itemX3 }, Enumerable.Range(0, 5));
			AssertEquals("Should be able to pack everyting into 2 boxes.", 2, result4.Count);
			AssertRoute(result4, new[] { itemX1, itemA, itemX2 });
			AssertRoute(result4, new[] { itemB, itemX3 });

			// test (A), (X, X, X), (B)
			var result5 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemX1, itemX2, itemX3, itemB }, Enumerable.Range(0, 5));
			AssertEquals("Should be able to pack everyting into 3 boxes.", 3, result5.Count);
			AssertRoute(result5, new[] { itemA });
			AssertRoute(result5, new[] { itemX1, itemX2, itemX3 });
			AssertRoute(result5, new[] { itemB });

			// test (A, X, X), (X, X, B)
			var result6 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemX1, itemX2, itemX3, itemX4, itemB }, Enumerable.Range(0, 6));
			AssertEquals("Should be able to pack everyting into 2 boxes.", 2, result6.Count);
			AssertRoute(result6, new[] { itemA, itemX1, itemX2 });
			AssertRoute(result6, new[] { itemX3, itemX4, itemB });

			// test (A, B, X), (X, X, X), (C)
			var result7 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemB, itemX1, itemX2, itemX3, itemX4, itemC }, Enumerable.Range(0, 7));
			AssertEquals("Should be able to pack everyting into 3 boxes.", 3, result7.Count);
			AssertRoute(result7, new[] { itemA, itemB, itemX1 });
			AssertRoute(result7, new[] { itemX2, itemX3, itemX4 });
			AssertRoute(result7, new[] { itemC });

			// test (A, X, X), (X, X, X), (B)
			var result8 = Algorithm.FirstFitItemsAlgorithm(box, new List<CartonisableItem> { itemA, itemX1, itemX2, itemX3, itemX4, itemX5, itemB }, Enumerable.Range(0, 7));
			AssertEquals("Should be able to pack everyting into 3 boxes.", 3, result8.Count);
			AssertRoute(result8, new[] { itemA, itemX1, itemX2 });
			AssertRoute(result8, new[] { itemX3, itemX4, itemX5 });
			AssertRoute(result8, new[] { itemB });
		}

		void AssertRoute(IEnumerable<ItemsGroup> allResults, IReadOnlyCollection<CartonisableItem> expectedOrders)
		{
			var isCorrectCartonFound = false;
			var routes = allResults.Where(c => c.ItemNodes.Count == expectedOrders.Count);
			foreach (var route in routes)
			{
				var isCorrectRoute = expectedOrders.All(o => route.ItemNodes.Contains(o));
				if (isCorrectRoute)
				{
					isCorrectCartonFound = true;
					break;
				}
			}
			Assert("Wrong orders were packed into the carton.", isCorrectCartonFound);
		}

		#endregion

		#region TestCartoniseItems_RobustToSortingOfCartons

		public void TestCartoniseItems_RobustToSortingOfCartons() => TestCartoniseItems_RobustToSortingOfCartons(c => c);
		public void TestCartoniseItems_RobustToSortingOfCartons_Volume_Ascending() => TestCartoniseItems_RobustToSortingOfCartons(c => c.OrderBy(b => b.Volume));
		public void TestCartoniseItems_RobustToSortingOfCartons_Volume_Descending() => TestCartoniseItems_RobustToSortingOfCartons(c => c.OrderByDescending(b => b.Volume));

		void TestCartoniseItems_RobustToSortingOfCartons(Func<IEnumerable<ICartonDefinition>, IEnumerable<ICartonDefinition>> modifyCartons)
		{
			// These cases are pulled straight from CS01598840 - GVILOGCHC - Inconsistent Cartonisation Results
			// We identified two issues:
			// 1. Algorithm was not robust to the sorting of cartons
			// 2. Algorithm was not keeping products together
			var boxes = new List<ICartonDefinition>();
			var orders = new List<ICartonisableItem>();

			var line1 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "410", keepUpright: false));
			var line2 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "412", keepUpright: false));
			var line3 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "414", keepUpright: false));
			var line4 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "418", keepUpright: false));
			var line5 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 50m, width: 140m, height: 160m, volume: 1120m, weight: 0.20m, name: "419", keepUpright: false));
			var line6 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 50m, width: 140m, height: 160m, volume: 1120m, weight: 0.17m, name: "427", keepUpright: false));
			var line7 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 50m, width: 140m, height: 160m, volume: 1120m, weight: 0.17m, name: "453", keepUpright: false));
			var line8 = orders.AddNew(15, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "502", keepUpright: false));
			var line9 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 100m, height: 125m, volume: 500m, weight: 0.11m, name: "504", keepUpright: false));
			var line10 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "506", keepUpright: false));
			var line11 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "510", keepUpright: false));
			var line12 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "514", keepUpright: false));
			var line13 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "518", keepUpright: false));
			var line14 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "520", keepUpright: false));
			var line15 = orders.AddNew(10, CartonisationTestHelper.CreateProduct(length: 50m, width: 140m, height: 160m, volume: 1120m, weight: 0.17m, name: "626", keepUpright: false));
			var line16 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "638", keepUpright: false));
			var line17 = orders.AddNew(4, CartonisationTestHelper.CreateProduct(length: 35m, width: 145m, height: 100m, volume: 507.50m, weight: 0.18m, name: "673", keepUpright: false));
			var line18 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 35m, width: 145m, height: 100m, volume: 507.50m, weight: 0.18m, name: "676", keepUpright: false));
			var line19 = orders.AddNew(8, CartonisationTestHelper.CreateProduct(length: 35m, width: 100m, height: 145m, volume: 507.50m, weight: 0.11m, name: "677", keepUpright: false));
			var line20 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "741", keepUpright: false));
			var line21 = orders.AddNew(5, CartonisationTestHelper.CreateProduct(length: 40m, width: 125m, height: 100m, volume: 500m, weight: 0.11m, name: "742", keepUpright: false));

			// Put larger boxes at the end by default
			var stlfBox = boxes.AddNew(length: 440m, width: 70m, height: 400m, maxVolume: 12320m, maxWeight: 25m, maxFillPercent: 0.95m);
			var stfsBox = boxes.AddNew(length: 380m, width: 70m, height: 280m, maxVolume: 7448m, maxWeight: 25m, maxFillPercent: 0.95m);
			var stdlBox = boxes.AddNew(length: 240m, width: 70m, height: 133m, maxVolume: 2096m, maxWeight: 25m, maxFillPercent: 0.95m);
			var sta5Box = boxes.AddNew(length: 280m, width: 70m, height: 180m, maxVolume: 3528m, maxWeight: 25m, maxFillPercent: 0.95m);
			var sta4Box = boxes.AddNew(length: 325m, width: 70m, height: 255m, maxVolume: 5801.25m, maxWeight: 25m, maxFillPercent: 0.95m);
			var bcmdBox = boxes.AddNew(length: 380m, width: 280m, height: 270m, maxVolume: 28728m, maxWeight: 25m, maxFillPercent: 0.95m);
			var bclgBox = boxes.AddNew(length: 510m, width: 330m, height: 390m, maxVolume: 65637m, maxWeight: 25m, maxFillPercent: 0.95m);

			var result = Algorithm.CartoniseItems(orders, modifyCartons(boxes).ToArray());
			AssertEquals(2, result.Count());
			AssertEquals(bclgBox.PK, result.ElementAt(0).CartonPK);
			AssertEquals(bclgBox.PK, result.ElementAt(1).CartonPK);
			AssertEquals(orders.Count, result.Sum(c => c.Items.Select(i => i.CartonisableItemPK).Distinct().Count()));
		}

		#endregion

		#region TestCartonisation_ZeroVolumedProducts

		public void TestCartonisation_ZeroVolumedProducts()
		{
			var box = CartonisationTestHelper.CreateCarton(2, 2, 2, 9999, 9, 9999, 1, "M", "CC", "KG", 10);
			var boxes = new List<ICartonDefinition> { box };
			var product1 = CartonisationTestHelper.CreateProduct(0m, 0m, 0m, 0m, 0.1m, name: "Product1");
			var product2 = CartonisationTestHelper.CreateProduct(0m, 0m, 0m, 0m, 0.1m, name: "Product2");

			var items = new List<ICartonisableItem>();
			var product1_1 = items.AddNew(1, product1);
			var product2_1 = items.AddNew(1, product2);
			var product1_2 = items.AddNew(1, product1);
			var product2_2 = items.AddNew(1, product2);

			IEnumerable<ICartonWithItems> result = null;
			AssertNoExceptionThrown(() => result = Algorithm.CartoniseItems(items, boxes));
			Assert("Still managed to cartonise.", result.Any());
		}

		#endregion

		#region Implementation

		public CartonisationAlgorithmForTesting Algorithm
		{
			get { return algorithm ?? (algorithm = new CartonisationAlgorithmForTesting()); }
		}

		CartonisationAlgorithmForTesting algorithm;

		#region TestData

		static class TestData
		{
			public static ProductTypes Products = new ProductTypes();
			public static BoxTypes Boxes = new BoxTypes();
			public static List<ICartonDefinition> GetBoxes
			{
				get { return new List<ICartonDefinition> { Boxes.Box36, Boxes.Box18, Boxes.Box8, Boxes.Box4_629, Boxes.Box2_625 }; }
			}
		}

		class ProductTypes
		{
			public ICartonisableItemDefinition ProductVolume1_5Weight1_5
			{
				get { return productVolume1_5Weight1_5 ?? (productVolume1_5Weight1_5 = CartonisationTestHelper.CreateProduct(1, 1, 1, 1.5m, 1.5m)); }
			}
			ICartonisableItemDefinition productVolume1_5Weight1_5;

			public ICartonisableItemDefinition ProductVolume1Weight1
			{
				get { return productVolume1Weight1 ?? (productVolume1Weight1 = CartonisationTestHelper.CreateProduct(1, 1, 1, 1.0m, 1.0m)); }
			}
			ICartonisableItemDefinition productVolume1Weight1;

			public ICartonisableItemDefinition ProductVolume2Weight2
			{
				get { return productVolume2Weight2 ?? (productVolume2Weight2 = CartonisationTestHelper.CreateProduct(1, 1, 1, 2.0m, 2.0m)); }
			}
			ICartonisableItemDefinition productVolume2Weight2;

			public ICartonisableItemDefinition ProductVolume3Weight1
			{
				get { return productVolume3Weight1 ?? (productVolume3Weight1 = CartonisationTestHelper.CreateProduct(1, 1, 1, 3.0m, 1.0m)); }
			}
			ICartonisableItemDefinition productVolume3Weight1;

			public ICartonisableItemDefinition ProductVolume0_5Weight0_5
			{
				get { return productVolume0_5Weight0_5 ?? (productVolume0_5Weight0_5 = CartonisationTestHelper.CreateProduct(1, 1, 1, 0.5m, 0.5m)); }
			}
			ICartonisableItemDefinition productVolume0_5Weight0_5;
		}

		class BoxTypes
		{
			public ICartonDefinition Box36
			{
				get { return box36 ?? (box36 = CartonisationTestHelper.CreateCarton(40, 36, 42, 20, 36, 99999, 0.80m)); }
			}
			ICartonDefinition box36;

			public ICartonDefinition Box18
			{
				get { return box18 ?? (box18 = CartonisationTestHelper.CreateCarton(36, 24, 36, 15, 18, 99999, 0.80m)); }
			}
			ICartonDefinition box18;

			public ICartonDefinition Box8
			{
				get { return box8 ?? (box8 = CartonisationTestHelper.CreateCarton(24, 24, 24, 10, 8, 99999, 0.80m)); }
			}
			ICartonDefinition box8;

			public ICartonDefinition Box4_629
			{
				get { return box4_629 ?? (box4_629 = CartonisationTestHelper.CreateCarton(20, 20, 20, 8, 4.62962963m, 99999, 0.90m)); }
			}
			ICartonDefinition box4_629;

			public ICartonDefinition Box2_625
			{
				get { return box2_625 ?? (box2_625 = CartonisationTestHelper.CreateCarton(18, 18, 14, 10, 2.625m, 99999, 0.80m)); }
			}
			ICartonDefinition box2_625;
		}

		#endregion

		#region Assert

		void AssertResult(List<ICartonWithItems> expect, IEnumerable<ICartonWithItems> results, List<ICartonDefinition> boxes = null)
		{
			var msg = "";
			if (boxes != null)
			{
				IEnumerable<string> expectBoxes;
				if (expect.Count == 0)
				{
					expectBoxes = new List<string> { "NO BOX" };
				}
				else
				{
					expectBoxes = expect.SelectMany(r => boxes.Where(b => r.CartonPK == b.PK).Select(b => b.ToString()));
				}
				IEnumerable<string> resultBoxes;
				if (!results.Any())
				{
					resultBoxes = new List<string> { "NO BOX" };
				}
				else
				{
					resultBoxes = results.SelectMany(r => boxes.Where(b => r.CartonPK == b.PK).Select(b => b.ToString()));
				}
				msg = string.Format(" expectVolumes : [{0}] but the result is :[{1}]", string.Join(",", expectBoxes), string.Join(",", resultBoxes));
			}

			AssertContainsExactElementsInAnyOrder(msg, GetCartonInfo(expect), GetCartonInfo(results));

			static IEnumerable<(Guid CartonPK, int Count)> GetCartonInfo(IEnumerable<ICartonWithItems> cartons)
			{
				return cartons
					.GroupBy(l => l.CartonPK)
					.Select(n => (n.Key, n.Count()))
					.OrderBy(n => n.Item2);
			}
		}

		void AssertResultContains(IEnumerable<ICartonWithItems> results, Guid cartonPK, Guid orderPK, decimal qty)
		{
			Assert($"Provided carton should contain {qty} products for provided order.",
				results.Where(l => l.CartonPK == cartonPK)
					.SelectMany(r => r.Items)
						.Any(b => b.CartonisableItemPK == orderPK && b.Quantity == qty));
		}

		void AssertResultTotals(IEnumerable<ICartonWithItems> results, Guid cartonPK, Guid orderPK, decimal qty)
		{
			AssertEquals($"Provided carton should contain {qty} products for provided order.", qty,
				results.Where(l => l.CartonPK == cartonPK)
					.SelectMany(r => r.Items)
						.Where(b => b.CartonisableItemPK == orderPK)
							.Sum(c => c.Quantity));
		}

		#endregion

		#endregion
	}

	#region CartonisationAlgorithmForTesting

	public class CartonisationAlgorithmForTesting : CartonisationAlgorithm
	{
		protected override IRandom CompositionRandom
		{
			get { return compositionRandom ?? (compositionRandom = new FixSeedRandom(100)); }
		}
		IRandom compositionRandom;

		protected override IRandom ShufflingListRandom
		{
			get { return shufflingListRandom ?? (shufflingListRandom = new FixSeedRandom(123)); }
		}
		IRandom shufflingListRandom;

		protected override int MaxNumberOfParallelProcessing
		{
			get { return UseMultipleThreads ? -1 : 1; }
		}

		public bool UseMultipleThreads;
	}

	#endregion
}
