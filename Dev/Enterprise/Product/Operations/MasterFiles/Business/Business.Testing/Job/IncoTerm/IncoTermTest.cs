using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IncoTermTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			IncoTermChargeCodesCollection.ClearDefaultIncoTermChargeCodesForTesting();
		}

		#region Charges Paid By Decision

		#region ExWorks

		public void TestExWorks()
		{
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.ExWorks, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.ExWorks, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Free Carrier

		public void TestFreeCarrier()
		{
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeCarrier, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Free Alongside Ship

		public void TestFreeAlongsideShip()
		{
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeAlongsideShip, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Free On Board

		public void TestFreeOnBoard()
		{
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.FreeOnBoard, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Cost and Freight

		public void TestCostAndFreight()
		{
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostAndFreight, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Cost Insrauce and Freight

		public void TestCostInsuranceAndFreight()
		{
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CostInsuranceAndFreight, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Carriage Paid To

		public void TestCarriagePaidTo()
		{
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriagePaidTo, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Carriage and Insurance Paid To

		public void TestCarriageInsurancePaidTo()
		{
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.CarriageAndInsurancePaidTo, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Delivered At Frontier

		public void TestDeliveredAtFrontier()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.Agent);

			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredAtFrontier, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);
		}

		#endregion

		#region Delivered Ex Ship

		public void TestDeliveredExShip()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExShip, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Delivered Ex Quay

		public void TestDeliveredExQuay()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredExQuay, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Delivered Duty Unpaid

		public void TestDeliveredDutyUnpaid()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);

			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyUnpaid, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.None);
		}

		#endregion

		#region Delivered Duty Paid

		[TestDate(2021, 1, 1)]
		public void TestDeliveredDutyPaid()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Origin, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Loading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Freight, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Insurance, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Destination, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.Agent);

			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Origin, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.OriginBrokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Loading, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Freight, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Insurance, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Unloading, ChargedParty.None);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Destination, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.CustomsDuty, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.Brokerage, ChargedParty.LocalClient);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, false, ChargeCodeGroupList.Codes.BrokerageOnly, ChargedParty.LocalClient);
		}

		[TestDate(2021, 1, 1)]
		public void TestDeliveredAtPlaceAfter2020_ShouldReturnLocalClientAsChargeParty_After2020()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredAtPlace, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.LocalClient);
		}

		[TestDate(2011, 1, 1)]
		public void TestDeliveredDutyPaidAndDeliveredAtPlace_ShouldReturnAgentAsChargeParty_Before2020()
		{
			AssertChargesPaidBy(IncoTerms.DeliveredAtPlace, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
			AssertChargesPaidBy(IncoTerms.DeliveredDutyPaid, true, ChargeCodeGroupList.Codes.Unloading, ChargedParty.Agent);
		}

		#endregion

		#region Warehouse Charges

		public void TestWarehouseCharges()
		{
			foreach (string inco in IncoTermRegistry.Keys)
			{
				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.WHSInwards, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.WHSInwards, ChargedParty.LocalClient);

				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.WHSOutwards, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.WHSOutwards, ChargedParty.LocalClient);

				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.WHSStorage, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.WHSStorage, ChargedParty.LocalClient);
			}
		}

		#endregion

		#region Container Yard Charges

		public void TestContainerYardCharges()
		{
			foreach (string inco in IncoTermRegistry.Keys)
			{
				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.YardGateIn, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.YardGateIn, ChargedParty.LocalClient);

				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.YardGateOut, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.YardGateOut, ChargedParty.LocalClient);

				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.YardStorage, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.YardStorage, ChargedParty.LocalClient);
			}
		}

		#endregion

		#region CFS Charges

		public void TestCFSCharges()
		{
			foreach (string inco in IncoTermRegistry.Keys)
			{
				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.CFSLoadList, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.CFSLoadList, ChargedParty.LocalClient);

				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.CFSShipment, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.CFSShipment, ChargedParty.LocalClient);
			}
		}

		#endregion

		#region Transport Charges

		public void TestTransportCharges()
		{
			foreach (string inco in IncoTermRegistry.Keys)
			{
				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.Transport, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.Transport, ChargedParty.LocalClient);
			}
		}

		#endregion

		#region Transport Booking Charges

		public void TestTransportBookingCharges()
		{
			foreach (string inco in IncoTermRegistry.Keys)
			{
				AssertChargesPaidBy(inco, false, ChargeCodeGroupList.Codes.TransportBooking, ChargedParty.LocalClient);
				AssertChargesPaidBy(inco, true, ChargeCodeGroupList.Codes.TransportBooking, ChargedParty.LocalClient);
			}
		}

		#endregion

		#region Overseas Agent Applicability

		public void TestOverseasAgentApplicability()
		{
			IncoTerm inco;
			Assert(IncoTermRegistry.TryGetValue(IncoTerms.ExWorks, out inco));

			AssertEquals(ChargedParty.Agent, inco.GetLocalClientOrAgent(Directions.Export, ChargeCodeGroupList.Codes.Origin, overseasAgentApplicable: true));
			AssertEquals(ChargedParty.None, inco.GetLocalClientOrAgent(Directions.Export, ChargeCodeGroupList.Codes.Origin, overseasAgentApplicable: false));
		}

		#endregion

		#endregion

		#region Charges Paid By Regardless Of IncoTerm Decision

		public void TestChargesPaidByRegardlessOfIncoTerm()
		{
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(null));
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(null, false));

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode5 = Factory.NewWithValidTestData<AccChargeCode>();

			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty, Guid.Empty, chargeCode1.PK.ToString() + "," + chargeCode2.PK.ToString());
			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty, Guid.Empty, chargeCode3.PK.ToString() + "," + chargeCode4.PK.ToString());

			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode5));
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode5, false));

			AssertEquals(ChargedParty.LocalClient, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode1));
			AssertEquals(ChargedParty.LocalClient, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode2));
			AssertEquals(ChargedParty.LocalClient, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode1, false));
			AssertEquals(ChargedParty.LocalClient, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode2, false));

			AssertEquals(ChargedParty.Agent, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode3));
			AssertEquals(ChargedParty.Agent, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode4));
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode3, false));
			AssertEquals(ChargedParty.None, IncoTermRegistry.GetLocalClientOrAgentRegardlessOfIncoterm(chargeCode4, false));
		}

		#endregion

		#region Implementation

		void AssertChargesPaidBy(ZString incoTerm, bool isImport, ZString chargeCodeGroup, ChargedParty expectedChargedParty)
		{
			IncoTerm inco;
			Assert(IncoTermRegistry.TryGetValue(incoTerm, out inco));
			var party = inco.GetLocalClientOrAgent(isImport ? Directions.Import : Directions.Export, chargeCodeGroup);
			AssertEquals("Expected " + chargeCodeGroup + " charges to be paid by " + expectedChargedParty.ToString() + " for an " + (isImport ? "import" : "export"), expectedChargedParty, party);
		}

		#endregion
	}
}
