using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class GroupCompanyChargesIntegrationTest : BaseRatingIntegrationTest
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAcceptActionIsSetToUpdateWhenChargeModified()
		{
			var creditorCompany = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			var debtorCompany = TestObjectCreator.CreateCompanyAndBranch("GBSUN");

			var shipment = CreateForwardingShipment(Constants.TransportModes.Sea, Consignor.PK, Consignee.PK, "GBSUN", "AUSYD", 400m, 8m);

			var globalChargeCode1 = "FLTCHRG";
			var globalChargeCode2 = "UNTCHRG";
			Helper.ChargeCodes.CreateGlobalCharge(globalChargeCode1);
			Helper.ChargeCodes.CreateGlobalCharge(globalChargeCode2, UnitCalculator.Code);
			Factory.Save();

			var debtor = debtorCompany.OrgProxy;
			ZGuid charge1PK;
			ZGuid charge2PK;

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex())
			{
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;

				var clientRate = Helper.NewClientRate(debtor);
				var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "GBSUN", "AUSYD");
				entry.RateLines.RemoveAndDeleteAll();

				var line1 = entry.AddRateLine(globalChargeCode1, FlatCalculator.Code, "", Constants.CurrencyCodes.Australia);
				line1.GetCalculator<FlatCalculator>().BaseRate = 200;

				var line2 = entry.AddRateLine(globalChargeCode2, UnitCalculator.Code, QuantityUnit.M3, Constants.CurrencyCodes.Australia);
				line2.GetCalculator<UnitCalculator>().PerUnit = 11;
				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = globalChargeCode1,
						JR_OSSellAmt = 200m,
						SellAccountCode = debtor.OH_Code
					},
					new AssertionCharge
					{
						ChargeCode = globalChargeCode2,
						JR_OSSellAmt = 88m,
						SellAccountCode = debtor.OH_Code
					}
				};

				AutorateAndAssert(expected, shipment, Consignor, job: job);

				charge1PK = job.Charges.Cast<Charge>().Single(x => x.JR_OSSellAmt == 200m).PK;
				charge2PK = job.Charges.Cast<Charge>().Single(x => x.JR_OSSellAmt == 88m).PK;
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				var expected = new[] { charge1PK, charge2PK };
				var actual = groupCompanySellCharges.Select(x => x.GroupCompanySellCharge.PK);

				AssertContainsExactElementsInAnyOrder("Pre-condition: should find all charges", expected, actual);
				Assert(groupCompanySellCharges.All(x => x.AcceptActionDescription == "Create"));

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(groupCompanySellCharges);

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				Assert(groupCompanySellCharges.All(x => x.AcceptActionDescription == "No Action"));

				Factory.Save();
			}

			shipment.JS_ActualVolume = 25m;

			using (Env.SetTemporaryUserContext(TestObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex())
			{
				job.Charges.RemoveAndDeleteAll();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = globalChargeCode1,
						JR_OSSellAmt = 200m,
						SellAccountCode = debtor.OH_Code
					},
					new AssertionCharge
					{
						ChargeCode = globalChargeCode2,
						JR_OSSellAmt = 275m,
						SellAccountCode = debtor.OH_Code
					}
				};

				AutorateAndAssert(expected, shipment, debtor, job: job);

				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			using (Env.SetTemporaryUserContext(new TestObjectCreator(newFactory).GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(newFactory, shipment).TryLoadOrCreateWithMutex())
			{
				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				AssertEquals("Should still find two group company charges", 2, groupCompanySellCharges.Length);

				var groupCompanyCharge1 = groupCompanySellCharges[0];
				AssertNotNull("Still expect global charge to exist", groupCompanyCharge1);
				AssertEquals("Chargeable and rate unchanged", "No Action", groupCompanyCharge1.AcceptActionDescription);

				var groupCompanyCharge2 = groupCompanySellCharges[1];
				AssertNotNull("Still expect global charge to exist", groupCompanyCharge2);
				AssertEquals("Chargable has changed but rate has not so we can match it", "Update", groupCompanyCharge2.AcceptActionDescription);

				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(new[] { groupCompanyCharge2 });

				groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				groupCompanySellCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				groupCompanyCharge2 = groupCompanySellCharges.Single(x => x.CostCompanyChargeCode.AC_Code == globalChargeCode2);
				AssertEquals(275m, groupCompanyCharge2.GroupCompanySellCharge.JR_OSSellAmt);
				AssertEquals("Now that the charge has been accepted, no further action is required", "No Action", groupCompanyCharge2.AcceptActionDescription);
			}
		}

		TestObjectCreator TestObjectCreator => new TestObjectCreator(Factory);
	}
}
