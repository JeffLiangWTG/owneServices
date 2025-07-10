using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(JobChargePossibleCarrierSelection))]
	public class JobChargePossibleCarrierSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSelectionResult()
		{
			var charge = new Mock<ICharge>();
			var oneOffShipment = Factory.New<RateOneOffShipment>();
			var possibleCarriers = new RateOneOffCarrierCollection(oneOffShipment);

			var selection = new JobChargePossibleCarrierSelection(charge.Object, possibleCarriers);
			AssertSame(selection.PossibleCreditorsAndCarriers, possibleCarriers);

			var carrier = ZGuid.NewZGuid();
			var creditor = ZGuid.NewZGuid();

			selection.SetPossibleCarrierSelection(carrier, creditor);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.Is<ZGuid>(v => v == creditor), Times.Once);
			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.Is<ZString>(v => v == JobChargeLookups.CreateNewCharge), Times.Once);
			charge.Invocations.Clear();

			selection.SetPossibleCarrierSelection(carrier: ZGuid.Empty, creditor);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.Is<ZGuid>(v => v == creditor), Times.Once);
			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.Is<ZString>(v => v == JobChargeLookups.CreateNewCharge), Times.Once);
			charge.Invocations.Clear();

			selection.SetPossibleCarrierSelection(carrier, creditor: ZGuid.Empty);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.Is<ZGuid>(v => v == carrier), Times.Once);
			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.Is<ZString>(v => v == JobChargeLookups.CreateNewCharge), Times.Once);
			charge.Invocations.Clear();

			// This is the degenerate case which shouldnt happen
			selection.SetPossibleCarrierSelection(carrier: ZGuid.Empty, creditor: ZGuid.Empty);
			charge.VerifySet(x => x.JR_OH_CostAccount = It.Is<ZGuid>(v => v == ZGuid.Empty), Times.Once);
			charge.VerifySet(x => x.JR_Calc_CostRatingBehavior = It.Is<ZString>(v => v == JobChargeLookups.CreateNewCharge), Times.Once);
			charge.Invocations.Clear();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var charge = new Mock<ICharge>();
			var oneOffShipment = Factory.New<RateOneOffShipment>();
			var possibleCarriers = new RateOneOffCarrierCollection(oneOffShipment);
			var selection = new JobChargePossibleCarrierSelection(charge.Object, possibleCarriers);

			return selection;
		}
	}
}
