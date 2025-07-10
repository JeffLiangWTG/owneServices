using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class RTUSBatchPreprocessorTest : TestCase
	{
		public void TestProcessItemNoMissing()
		{
			var batchXus = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			batchXus.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			batchXus.DataContext.AddDataSource(DataContextType.WarehouseOrder, "ORDER1");
			batchXus.DataContext.AddDataSource(DataContextType.WarehouseOrder, "ORDER2");

			var order1Xus = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			order1Xus.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			batchXus.SubShipmentCollection.Add(order1Xus);
			order1Xus.DataContext.AddDataSource(DataContextType.WarehouseOrder, "ORDER1");
			order1Xus.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ItemNo = 1,
				ReferenceNumber = "PACKAGE1",
			});
			order1Xus.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ReferenceNumber = "PACKAGE2",
			});

			var order2Xus = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			order2Xus.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			batchXus.SubShipmentCollection.Add(order2Xus);
			order2Xus.DataContext.AddDataSource(DataContextType.WarehouseOrder, "ORDER2");
			order2Xus.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ItemNo = 2,
				ReferenceNumber = "PACKAGE3",
			});

			AssertExceptionThrown<FormatException>(() => RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, batchXus, null, ShortSegmentProcessor.Instance));
		}

		public void TestProcessPartialSegment()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE3"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(UniversalShipmentString(input), UniversalShipmentString(processed.Single()));
		}

		public void TestProcessExactSegment()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE6"),
							(4, "PACKAGE4"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(UniversalShipmentString(input), UniversalShipmentString(processed.Single()));
		}

		public void TestProcessMultiSegment()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(6, "PACKAGE3"),
							(4, "PACKAGE4"),
							(5, "PACKAGE5"),
							(2, "PACKAGE6"),
							(7, "PACKAGE7"),
						}
					),
				}
			);

			var expected1 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE6"),
							(4, "PACKAGE4"),
						}
					),
				}
			);

			var expected2 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(5, "PACKAGE5"),
							(6, "PACKAGE3"),
							(7, "PACKAGE7"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(2, processed.Count);
			AssertEquals(UniversalShipmentString(expected1), UniversalShipmentString(processed.First()));
			AssertEquals(UniversalShipmentString(expected2), UniversalShipmentString(processed.Last()));
		}

		public void TestProcessCustomSegment()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(6, "PACKAGE3"),
							(4, "PACKAGE4"),
							(5, "PACKAGE5"),
							(2, "PACKAGE6"),
							(7, "PACKAGE7"),
						}
					),
					("ORDER3", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(8, "PACKAGE8"),
						}
					),
				}
			);

			var expected1 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE6"),
						}
					),
				}
			);

			var expected2 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(3, "PACKAGE2"),
						}
					),
				}
			);

			var expected3 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(4, "PACKAGE4"),
							(5, "PACKAGE5"),
							(6, "PACKAGE3"),
							(7, "PACKAGE7"),
						}
					),
				}
			);

			var expected4 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER3", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(8, "PACKAGE8"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, new List<int>() { 2, 3 }, ShortSegmentProcessor.Instance);
			AssertEquals(4, processed.Count);
			AssertEquals(UniversalShipmentString(expected1), UniversalShipmentString(processed.First()));
			AssertEquals(UniversalShipmentString(expected2), UniversalShipmentString(processed.ElementAt(1)));
			AssertEquals(UniversalShipmentString(expected3), UniversalShipmentString(processed.ElementAt(2)));
			AssertEquals(UniversalShipmentString(expected4), UniversalShipmentString(processed.Last()));
		}

		public void TestProcessLargeOrder()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
							(6, "PACKAGE3"),
							(4, "PACKAGE4"),
							(5, "PACKAGE5"),
							(2, "PACKAGE6"),
							(7, "PACKAGE7"),
							(8, "PACKAGE8"),
							(9, "PACKAGE9"),
							(10, "PACKAGE10"),
							(11, "PACKAGE11"),
							(12, "PACKAGE12"),
							(13, "PACKAGE13"),
							(14, "PACKAGE14"),
						}
					),
				}
			);

			var expected1 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(2, "PACKAGE6"),
							(3, "PACKAGE2"),
							(4, "PACKAGE4"),
						}
					),
				}
			);

			var expected2 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(5, "PACKAGE5"),
							(6, "PACKAGE3"),
							(7, "PACKAGE7"),
							(8, "PACKAGE8"),
						}
					),
				}
			);

			var expected3 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(9, "PACKAGE9"),
							(10, "PACKAGE10"),
							(11, "PACKAGE11"),
							(12, "PACKAGE12"),
						}
					),
				}
			);

			var expected4 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(13, "PACKAGE13"),
							(14, "PACKAGE14"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(4, processed.Count);
			AssertEquals(UniversalShipmentString(expected1), UniversalShipmentString(processed.First()));
			AssertEquals(UniversalShipmentString(expected2), UniversalShipmentString(processed.ElementAt(1)));
			AssertEquals(UniversalShipmentString(expected3), UniversalShipmentString(processed.ElementAt(2)));
			AssertEquals(UniversalShipmentString(expected4), UniversalShipmentString(processed.Last()));
		}

		public void TestProcessManySmallOrders()
		{
			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER12", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE01"),
						}
					),
					("ORDER11", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(3, "PACKAGE03"),
						}
					),
					("ORDER10", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(5, "PACKAGE05"),
						}
					),
					("ORDER09", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(7, "PACKAGE07"),
						}
					),
					("ORDER08", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(9, "PACKAGE09"),
						}
					),
					("ORDER07", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(11, "PACKAGE11"),
						}
					),
					("ORDER06", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(12, "PACKAGE12"),
						}
					),
					("ORDER05", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(10, "PACKAGE10"),
						}
					),
					("ORDER04", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(8, "PACKAGE08"),
						}
					),
					("ORDER03", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(6, "PACKAGE06"),
						}
					),
					("ORDER02", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(4, "PACKAGE04"),
						}
					),
					("ORDER01", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE02"),
						}
					),
				}
			);

			var expected1 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER12", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE01"),
						}
					),
					("ORDER01", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE02"),
						}
					),
					("ORDER11", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(3, "PACKAGE03"),
						}
					),
					("ORDER02", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(4, "PACKAGE04"),
						}
					),
				}
			);

			var expected2 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER10", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(5, "PACKAGE05"),
						}
					),
					("ORDER03", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(6, "PACKAGE06"),
						}
					),
					("ORDER09", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(7, "PACKAGE07"),
						}
					),
					("ORDER04", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(8, "PACKAGE08"),
						}
					),
				}
			);

			var expected3 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER08", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(9, "PACKAGE09"),
						}
					),
					("ORDER05", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(10, "PACKAGE10"),
						}
					),
					("ORDER07", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(11, "PACKAGE11"),
						}
					),
					("ORDER06", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(12, "PACKAGE12"),
						}
					),
				}
			);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(3, processed.Count);
			AssertEquals(UniversalShipmentString(expected1), UniversalShipmentString(processed.First()));
			AssertEquals(UniversalShipmentString(expected2), UniversalShipmentString(processed.ElementAt(1)));
			AssertEquals(UniversalShipmentString(expected3), UniversalShipmentString(processed.Last()));
		}

		public void TestOrderDataPreserved()
		{
			// adding some random order fields to ensure that all order fields are preserved after the split
			void AddOrder1TestData(UniversalShipment order)
			{
				order.PickupMode = new CodeDescriptionPair()
				{
					Code = "YES",
					Description = "I do so like green eggs and ham."
				};
				order.RequiredTemperatureMaximum = 42;
				order.RequiresRefrigeration = true;
				order.Order = new Order
				{
					ClientReference = "My name Jeff"
				};
			}

			void AddOrder2TestData(UniversalShipment order)
			{
				order.PickupMode = new CodeDescriptionPair()
				{
					Code = "NOP",
					Description = "I do not like green eggs and ham."
				};
				order.RequiredTemperatureMaximum = -9;
				order.RequiresRefrigeration = false;
				order.Order = new Order
				{
					ClientReference = "My name Geoff"
				};
			}

			var input = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(6, "PACKAGE3"),
							(4, "PACKAGE4"),
							(5, "PACKAGE5"),
							(2, "PACKAGE6"),
							(7, "PACKAGE7"),
						}
					),
				}
			);

			AddOrder1TestData(input.SubShipmentCollection[0]);
			AddOrder2TestData(input.SubShipmentCollection[1]);

			var expected1 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER1", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(1, "PACKAGE1"),
							(3, "PACKAGE2"),
						}
					),
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(2, "PACKAGE6"),
							(4, "PACKAGE4"),
						}
					),
				}
			);

			AddOrder1TestData(expected1.SubShipmentCollection[0]);
			AddOrder2TestData(expected1.SubShipmentCollection[1]);

			var expected2 = BuildUniversalShipment(
				new List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)>()
				{
					("ORDER2", new List<(ZShort ItemNo, string ReferenceNumber)>()
						{
							(5, "PACKAGE5"),
							(6, "PACKAGE3"),
							(7, "PACKAGE7"),
						}
					),
				}
			);

			AddOrder2TestData(expected2.SubShipmentCollection[0]);

			var processed = RTUSBatchPreprocessor.Process(DefaultDataObjectWriterStrategy.TestInstance, input, null, ShortSegmentProcessor.Instance);
			AssertEquals(2, processed.Count);
			AssertEquals(UniversalShipmentString(expected1), UniversalShipmentString(processed.First()));
			AssertEquals(UniversalShipmentString(expected2), UniversalShipmentString(processed.Last()));
		}

		UniversalShipment BuildUniversalShipment(List<(string OrderID, List<(ZShort ItemNo, string ReferenceNumber)> Packages)> orders)
		{
			var segment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
			};
			segment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			foreach (var (orderID, packages) in orders)
			{
				segment.DataContext.AddDataSource(DataContextType.WarehouseOrder, orderID);
				var orderXus = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = DataContextFactory.New(),
				};
				orderXus.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
				orderXus.DataContext.AddDataSource(DataContextType.WarehouseOrder, orderID);
				segment.SubShipmentCollection.Add(orderXus);
				foreach (var (itemNo, referenceNumber) in packages)
				{
					orderXus.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ItemNo = itemNo,
						ReferenceNumber = referenceNumber,
					});
				}
			}

			return segment;
		}

		string UniversalShipmentString(UniversalShipment xus)
		{
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				ObjectFactory.New<IXmlWriter>().WriteXML(xus, stream);
				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}

		protected private class ShortSegmentProcessor : IRTUSProcessor
		{
			ShortSegmentProcessor()
			{
			}

			public static IRTUSProcessor Instance { get; } = new ShortSegmentProcessor();

			public int IdealPackagesInBatchSegment => 4;

			T IRTUSProcessor.PushMessage<T>(RequestType<T> requestType, IHttpClientFactory httpClientFactory, Stream universalXmlStream, RTUSCBA rtusType, Uri rtusUrl)
			{
				return RTUSProcessor.PushMessage(requestType, httpClientFactory, universalXmlStream, rtusType, rtusUrl);
			}
		}
	}
}
