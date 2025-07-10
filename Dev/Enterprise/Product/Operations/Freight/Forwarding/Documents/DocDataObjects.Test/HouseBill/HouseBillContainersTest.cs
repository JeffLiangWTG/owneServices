using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillContainersTest : TestCaseWithFactory
	{
		#region TestPopulateContainers

		public void TestPopulateContainers()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertContainsExactElementsInAnyOrder("created containers from departure consol disregarding empty",
					new[]
					{
						"AAAA0000007|2|BAG",
						"BBBB0000004|10|PKG"
					},
					houseBill.Containers.Select(c => $"{c.Number}|{c.PackCount}|{c.PackType.Code}"));
			}
		}

		public void TestPopulateContainers_NoConsol()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertContainsExactElementsInAnyOrder("no containers have been created",
					Array.Empty<ZString>(),
					houseBill.Containers.Select(c => c.Number));
			}
		}

		public void TestPopolateContainers_TwoMoreConsolsDepartingSameCountry()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
					  GlbCompany.CurrentCompany.PK.ToGuid(),
					  Guid.Empty,
					  Guid.Empty,
					  true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "SEA";
				shipment.JS_UniqueConsignRef = "SH0001";
				shipment.JS_HouseBill = "HOUSEBILL001";
				shipment.JS_PackingMode = ContainerModes.FCL;
				shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_HouseBillIssueDate = ZDate.Today;

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_UniqueConsignRef = "CONSOL0001";
				consol1.JK_TransportMode = "SEA";
				consol1.JK_RL_NKLoadPort = "AUSYD";
				consol1.JK_RL_NKDischargePort = "SGSIN";

				var container1 = consol1.Containers.AddNew();
				container1.JC_ContainerNum = "CON1";
				container1.JC_TareWeight = 1000;
				container1.JC_DunnageWeight = 1000;
				container1.JC_GrossWeightUQ = Weight.Pounds;

				var departureConsolMainTransport = consol1.Transports.AddNew();
				departureConsolMainTransport.JW_LegOrder = 1;
				departureConsolMainTransport.JW_TransportMode = TransportModes.Sea;
				departureConsolMainTransport.JW_TransportType = TransportPlanningType.MainVessel;
				departureConsolMainTransport.JW_RL_NKLoadPort = "AUSYD";
				departureConsolMainTransport.JW_RL_NKDiscPort = "SGSIN";
				departureConsolMainTransport.JW_Vessel = "Vessel";
				departureConsolMainTransport.JW_VoyageFlight = "F9999";

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 2;
				packline.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				packline.JL_ActualWeight = 1000;
				packline.JL_ActualWeightUQ = Weight.Kilograms;
				packline.JL_ActualVolume = 1.1;
				packline.JL_ActualVolumeUQ = Volume.CubicMetres;

				container1.PackLines.Add(packline);

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_UniqueConsignRef = "CONSOL0002";
				consol2.JK_TransportMode = "ROA";
				consol2.JK_RL_NKLoadPort = "AUSYD";
				consol2.JK_RL_NKDischargePort = "SGSIN";

				var container2 = consol2.Containers.AddNew();
				container2.JC_ContainerNum = "CON2";
				container2.JC_TareWeight = 2000;
				container2.JC_DunnageWeight = 1000;
				container2.JC_GrossWeightUQ = Weight.Pounds;

				Factory.Save();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertContainsExactElementsInAnyOrder("created containers from consol1",
					new[]
					{
						"CON1|2|BAG"
					},
					houseBill.Containers.Select(c => $"{c.Number}|{c.PackCount}|{c.PackType.Code}"));
			}
		}

		public void TestPopulateContainerVolumeFromPackLines()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
					  GlbCompany.CurrentCompany.PK.ToGuid(),
					  Guid.Empty,
					  Guid.Empty,
					  true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "SEA";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = "SEA";

				var container = consol.Containers.AddNew();

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_ActualVolume = 0.91243172352;
				packLine1.JL_ActualVolumeUQ = "M3";
				packLine1.JL_PackageCount = 1;
				packLine1.JL_Length = 48;
				packLine1.JL_Width = 40;
				packLine1.JL_Height = 29;
				packLine1.JL_UnitOfDimension = Core.Constants.Length.Inches;

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_ActualVolume = 3.20924261376;
				packLine2.JL_ActualVolumeUQ = "M3";
				packLine2.JL_PackageCount = 2;
				packLine2.JL_Length = 48;
				packLine2.JL_Width = 40;
				packLine2.JL_Height = 51;
				packLine2.JL_UnitOfDimension = Core.Constants.Length.Inches;

				container.PackLines.Add(packLine1);
				container.PackLines.Add(packLine2);

				Factory.Save();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				var volume = Utilities.Round(houseBill.Containers.ElementAt(0).Volume.Value, 3);
				shipment.UpdateShipmentFromOuterPackLines();
				AssertEquals("Precondition: Shipment Volume", 4.121m, shipment.JS_ActualVolume);
				AssertEquals("Container Volume should equal Shipment Volume", 4.121m, volume);
			}
		}

		#endregion

		#region TestConvertWeightAndVolumeToMetric

		public void TestConvertWeightAndVolumeToMetric()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with paklines that belongs to the shipment",
					2, houseBill.Containers.Count);

				var container1 = houseBill.Containers.ElementAt(0);
				var container2 = houseBill.Containers.ElementAt(1);

				CombineAssertions(() =>
				{
					AssertEquals("container 1 GoodsWeight.Value", 1000m, container1.GoodsWeight.Value);
					AssertEquals("container 1 GoodsWeight.Unit.Code", "KG", container1.GoodsWeight.Unit.Code);

					AssertEquals("container 1 TareWeight.Value", 907.18474m, container1.TareWeight.Value);
					AssertEquals("container 1 TareWeight.Unit.Code", "KG", container1.TareWeight.Unit.Code);

					AssertEquals("container 1 Dunnage.Value", 453.59237m, container1.Dunnage.Value);
					AssertEquals("container 1 Dunnage.Unit.Code", "KG", container1.Dunnage.Unit.Code);

					AssertEquals("container 2 GoodsWeight.Value", 3907.18474m, container2.GoodsWeight.Value);
					AssertEquals("container 2 GoodsWeight.Unit.Code", "KG", container2.GoodsWeight.Unit.Code);

					AssertEquals("container 2 TareWeight.Value", 2000m, container2.TareWeight.Value);
					AssertEquals("container 2 TareWeight.Unit.Code", "KG", container2.TareWeight.Unit.Code);

					AssertEquals("container 2 Dunnage.Value", 1000m, container2.Dunnage.Value);
					AssertEquals("container 2 Dunnage.Unit.Code", "KG", container2.Dunnage.Unit.Code);
				});
			}
		}

		#endregion

		#region TestPackingLineGoodsDescription

		public void TestPackingLineGoodsDescription()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with packlines that belongs to the shipment",
					2, houseBill.Containers.Count);

				AssertEquals("total number of packinglines",
					3, houseBill.Containers.Sum(p => p.PackingLines.Count));

				string[] GetGoodsDescription(IContainer container)
				{
					return container
						.PackingLines
						.Select(p => p.GoodsDescription.ToString())
						.ToArray();
				}

				AssertContainsExactElementsInAnyOrder("container 1 packline GoodsDescription",
					new[]
					{
						"container 1 packline 1 detailed goods details"
					},
					GetGoodsDescription(houseBill.Containers.ElementAt(0)));

				AssertContainsExactElementsInAnyOrder("container 2 packline GoodsDescription",
					new[]
					{
						"shipment goods description",
						"container 2 packline 2 detailed goods details"
					},
					GetGoodsDescription(houseBill.Containers.ElementAt(1)));
			}
		}

		#endregion

		#region TestPackingLineShortGoodsDescription

		public void TestPackingLineShortGoodsDescription()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with packlines that belongs to the shipment",
					2, houseBill.Containers.Count);

				AssertEquals("total number of packinglines",
					3, houseBill.Containers.Sum(p => p.PackingLines.Count));

				string[] GetShortGoodsDescription(IContainer container)
				{
					return container
						.PackingLines
						.Select(p => p.ShortGoodsDescription.ToString())
						.ToArray();
				}

				AssertContainsExactElementsInAnyOrder("container 1 packline ShortGoodsDescription",
					new[]
					{
						"container 1 packline 1 short goods details"
					},
					GetShortGoodsDescription(houseBill.Containers.ElementAt(0)));

				AssertContainsExactElementsInAnyOrder("container 2 packline ShortGoodsDescription",
					new[]
					{
						string.Empty,
						"container 2 packline 2 short goods details"
					},
					GetShortGoodsDescription(houseBill.Containers.ElementAt(1)));
			}
		}

		#endregion

		#region TestPackingLineDetailedGoodsDescription

		public void TestPackingLineDetailedGoodsDescription()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with packlines that belongs to the shipment",
					2, houseBill.Containers.Count);

				AssertEquals("total number of packinglines",
					3, houseBill.Containers.Sum(p => p.PackingLines.Count));

				string[] GetDetailedGoodsDescription(IContainer container)
				{
					return container
						.PackingLines
						.Select(p => p.DetailedGoodsDescription.ToString())
						.ToArray();
				}

				AssertContainsExactElementsInAnyOrder("container 1 packline DetailedGoodsDescription",
					new[]
					{
						"container 1 packline 1 detailed goods details"
					},
					GetDetailedGoodsDescription(houseBill.Containers.ElementAt(0)));

				AssertContainsExactElementsInAnyOrder("container 2 packline DetailedGoodsDescription",
					new[]
					{
						string.Empty,
						"container 2 packline 2 detailed goods details"
					},
					GetDetailedGoodsDescription(houseBill.Containers.ElementAt(1)));
			}
		}

		#endregion

		#region TestPackingLineMarksAndNumbers

		public void TestPackingLineMarksAndNumbers()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with packlines that belongs to the shipment",
					2, houseBill.Containers.Count);

				AssertEquals("total number of packinglines",
					3, houseBill.Containers.Sum(p => p.PackingLines.Count));

				string[] GetMarksAndNumbers(IContainer container)
				{
					return container
						.PackingLines
						.Select(p => p.MarksAndNumbers.ToString())
						.ToArray();
				}

				AssertContainsExactElementsInAnyOrder("container 1 packline MarksAndNumbers",
					new[]
					{
						"container 1 packline 1 marks & numbers"
					},
					GetMarksAndNumbers(houseBill.Containers.ElementAt(0)));

				AssertContainsExactElementsInAnyOrder("container 2 packline MarksAndNumbers",
					new[]
					{
						"shipment marks & numbers",
						"container 2 packline 2 marks & numbers"
					},
					GetMarksAndNumbers(houseBill.Containers.ElementAt(1)));
			}
		}

		#endregion

		#region TestPackingLineContainerNumber

		public void TestPackingLineContainerNumber()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertEquals(2, houseBill.Containers.Count);

				Assert(houseBill.Containers.First().PackingLines.AllSame(p => p.ContainerNumber));
				AssertEquals("AAAA0000007", houseBill.Containers.First().PackingLines.First().ContainerNumber);
				AssertEquals("AAAA0000007", houseBill.Containers.First().Number);

				Assert(houseBill.Containers.Last().PackingLines.AllSame(p => p.ContainerNumber));
				AssertEquals("BBBB0000004", houseBill.Containers.Last().PackingLines.First().ContainerNumber);
				AssertEquals("BBBB0000004", houseBill.Containers.Last().Number);
			}
		}

		#endregion

		#region TestDangerousGoods

		public void TestDangerousGoods()
		{
			using (FreightDataRegistry.Instance.ShowPackLineDetailsOnHouseBills.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				true))
			{
				var shipment = CreateShipment();
				var parameters = new DummyDocDataObjectParameters
				{
					DocumentTitle = "ORIGINAL"
				};

				var houseBillBuilder = new HouseBillBuilder(shipment, parameters);
				var houseBill = houseBillBuilder.Build();

				AssertNotNull("containers collection has been created",
					houseBill.Containers);

				AssertEquals("containers collection contains containers with paklines that belongs to the shipment",
					2, houseBill.Containers.Count);

				AssertEquals("total number of packinglines",
					3, houseBill.Containers.Sum(p => p.PackingLines.Count));

				var packline1 = houseBill
					.Containers
					.ElementAt(0)
					.PackingLines
					.ElementAt(0);

				var packline2 = houseBill
					.Containers
					.ElementAt(1)
					.PackingLines
					.ElementAt(0);

				var packline3 = houseBill
					.Containers
					.ElementAt(1)
					.PackingLines
					.ElementAt(1);

				AssertEquals("packline1 has no dangerous goods", 0, packline1.DangerousGoods.Count);
				AssertEquals("packline2 has no dangerous goods", 0, packline2.DangerousGoods.Count);
				AssertEquals("packline3 has dangerous goods", 1, packline3.DangerousGoods.Count);

				var dg = packline3.DangerousGoods.Single();

				AssertEquals("Code", "9999", dg.Unno);
				AssertEquals("Variant", "B", dg.Variant);
				AssertEquals("Quantity", 5, dg.Quantity);
				AssertEquals("ProperShippingName", "NUCLEAR BOMB", dg.ProperShippingName);
				AssertEquals("TechnicalName", "TERRAFORM", dg.TechnicalName);
				AssertEquals("IMOClass", "9.9Z", dg.IMOClass);
				AssertEquals("PackingGroup", "III", dg.PackingGroup);
				AssertEquals("SubLabel1", "TEST", dg.SubLabel1);
				AssertEquals("SubLabel2", "AAA", dg.SubLabel2);

				AssertEquals("Weight.Value", 564m, dg.Weight.Value);
				AssertEquals("Weight.Unit.Code", Weight.Kilograms, dg.Weight.Unit.Code);

				AssertEquals("Volume.Value", 1m, dg.Volume.Value);
				AssertEquals("Volume.Unit.Code", Volume.CubicMetres, dg.Volume.Unit.Code);

				AssertEquals("FlashPoint.Value", 11.0m, dg.FlashPoint.Value);
				AssertEquals("FlashPoint.Unit.Code", Temperature.Centigrade, dg.FlashPoint.Unit.Code);

				AssertEquals("PackageType.Code", "BAG", dg.PackageType.Code);
				AssertEquals("MarinePollutant.Code", "Y", dg.MarinePollutant.Code);
			}
		}

		#endregion

		#region Implementation

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_HouseBill = "HOUSEBILL001";
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "JPTYO";
			shipment.JS_HouseBillIssueDate = ZDate.Today;
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "shipment goods description";
			shipment.JS_MarksAndNumbers = "shipment marks & numbers";

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = "SEA";
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "NZAKL";

			var departureConsolMainTransport = departureConsol.Transports.AddNew();
			departureConsolMainTransport.JW_LegOrder = 1;
			departureConsolMainTransport.JW_TransportMode = TransportModes.Sea;
			departureConsolMainTransport.JW_TransportType = TransportPlanningType.MainVessel;
			departureConsolMainTransport.JW_RL_NKLoadPort = "AUSYD";
			departureConsolMainTransport.JW_RL_NKDiscPort = "NZCHC";
			departureConsolMainTransport.JW_Vessel = "Vessel";
			departureConsolMainTransport.JW_VoyageFlight = "F9999";

			var departureConsolOtherTransport = departureConsol.Transports.AddNew();
			departureConsolOtherTransport.JW_LegOrder = 2;
			departureConsolOtherTransport.JW_TransportMode = TransportModes.Sea;
			departureConsolOtherTransport.JW_RL_NKLoadPort = "NZCHC";
			departureConsolOtherTransport.JW_RL_NKDiscPort = "NZAKL";

			var container1 = departureConsol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA0000007";
			container1.JC_TareWeight = 2000;
			container1.JC_DunnageWeight = 1000;
			container1.JC_GrossWeightUQ = Weight.Pounds;

			var container2 = departureConsol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB0000004";
			container2.JC_TareWeight = 2000;
			container2.JC_DunnageWeight = 1000;
			container2.JC_GrossWeightUQ = Weight.Kilograms;

			var container3 = departureConsol.Containers.AddNew();
			container3.JC_ContainerNum = "CCCC0000005";
			container3.JC_TareWeight = 500;
			container3.JC_DunnageWeight = 300;
			container3.JC_GrossWeightUQ = Weight.Kilograms;

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 2;
			packline1.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline1.JL_ActualWeight = 1000;
			packline1.JL_ActualWeightUQ = Weight.Kilograms;
			packline1.JL_ActualVolume = 1.1;
			packline1.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline1.JL_Description = "container 1 packline 1 short goods details";
			packline1.JL_DetailedDescription = "container 1 packline 1 detailed goods details";
			packline1.JL_MarksAndNumbers = "container 1 packline 1 marks & numbers";

			container1.PackLines.Add(packline1);

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 3;
			packline2.JL_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			packline2.JL_ActualWeight = 2000;
			packline2.JL_ActualWeightUQ = Weight.Pounds;
			packline2.JL_ActualVolume = 2.2;
			packline2.JL_ActualVolumeUQ = Volume.CubicFeet;
			packline2.JL_Description = "container 2 packline 2 short goods details";
			packline2.JL_DetailedDescription = "container 2 packline 2 detailed goods details";
			packline2.JL_MarksAndNumbers = "container 2 packline 2 marks & numbers";

			container2.PackLines.Add(packline2);

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 7;
			packline3.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			packline3.JL_ActualWeight = 3000;
			packline3.JL_ActualWeightUQ = Weight.Kilograms;
			packline3.JL_ActualVolume = 3.3;
			packline3.JL_ActualVolumeUQ = Volume.CubicMetres;
			packline3.JL_Description = ZString.Empty;
			packline3.JL_DetailedDescription = ZString.Empty;
			packline3.JL_MarksAndNumbers = ZString.Empty;

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "9999";
			subs.DG_Variant = "B";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var undgSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "9999", "B", "IMO").FirstOrDefault();
			undgSubstance.DG_Class = "9.9Z";
			undgSubstance.DG_FlashPoint = "27.0 c.c";
			undgSubstance.DG_MP = "C";
			undgSubstance.DG_PG = "III";
			undgSubstance.DG_PSN = "NUCLEAR BOMB";
			undgSubstance.DG_SubLabel1 = "TEST";
			undgSubstance.DG_SubLabel2 = "AAA";

			var undgDataItem = packline3.UNDGs.AddNew();
			undgDataItem.LinkDefault(subs);
			undgDataItem.DI_TechnicalName = "TERRAFORM";
			undgDataItem.DI_IsCombustible = true;
			undgDataItem.DI_DGFlashPoint = 11.0m;
			undgDataItem.DI_MPMarinePollutant = "Y";
			undgDataItem.DI_DGVolume = 1m;
			undgDataItem.DI_UnitOfVolume = "M3";
			undgDataItem.DI_DGWeight = 564m;
			undgDataItem.DI_UnitOfWeight = "KG";
			undgDataItem.DI_IsLimitedQuantity = true;
			undgDataItem.DI_PackageCount = 5;
			undgDataItem.DI_F3_NKPackType = "BAG";

			container2.PackLines.Add(packline3);

			var otherConsol = shipment.Consols.AddNew();
			otherConsol.JK_UniqueConsignRef = "CONSOL0002";
			otherConsol.JK_TransportMode = "SEA";
			otherConsol.JK_RL_NKLoadPort = "NZAKL";
			otherConsol.JK_RL_NKDischargePort = "JPTYO";

			var container4 = otherConsol.Containers.AddNew();
			container4.JC_ContainerNum = "DDDD0000003";
			container4.JC_TareWeight = 1000;
			container4.JC_DunnageWeight = 1000;
			container4.JC_GrossWeightUQ = Weight.Kilograms;

			var packline4 = shipment.OuterPackLines.AddNew();
			packline4.JL_ActualWeight = 2000;
			packline4.JL_ActualWeightUQ = Weight.Pounds;
			packline4.JL_ActualVolume = 2.2;
			packline4.JL_ActualVolumeUQ = Volume.CubicFeet;
			packline4.JL_Description = "container 4 packline 1 short goods details";
			packline4.JL_DetailedDescription = "container 4 packline 1 detailed goods details";
			packline4.JL_MarksAndNumbers = "container 4 packline 1 marks & numbers";

			container4.PackLines.Add(packline4);

			// business logic always tries to pack packingline to a container
			container3.PackLines.RemoveAll();

			Factory.Save();

			return shipment;
		}

		#endregion
	}
}
