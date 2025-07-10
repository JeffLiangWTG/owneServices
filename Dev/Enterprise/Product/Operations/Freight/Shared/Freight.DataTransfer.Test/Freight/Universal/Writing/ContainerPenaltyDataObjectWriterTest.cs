using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using ContainerPenaltyBO = Enterprise.Freight.Business.ContainerPenalty;
using ContainerPenaltyDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerPenalty;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class ContainerPenaltyDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateDataObject()
		{
			var container = Factory.New<CommonContainer>();
			var penalty = container.ImportPenalties.AddNew();
			SetupTestdata(penalty);

			var writer = new ContainerPenaltyDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, penalty)));
			var dataObject = writer.GetDataObject(penalty);
			AssertResult(dataObject);
		}

		void SetupTestdata(ContainerPenaltyBO penalty)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG1";

			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "TESTADDR1";

			penalty.CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_OH_Creditor = org.PK;
			penalty.CPY_RL_NKLocation = "AUSYD";
			penalty.CPY_FreeTime = new ZDateTime(ZDateTime.Now.Year, 1, 4);
			penalty.CPY_Duration = new ZDateTime(ZDateTime.Now.Year, 1, 10);
			penalty.CPY_TimeUnit = Core.Constants.ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_PerUnitCost = 3.2m;
			penalty.CPY_TotalCost = 500m;
			penalty.CPY_RX_NKCurrency = "AUD";
			penalty.CPY_ProcessType = "IMP";
		}

		void AssertResult(ContainerPenaltyDataObject penaltyDataObject)
		{
			AssertEquals(Core.Constants.ContainerPenaltyPenaltyType.Codes.Detention, penaltyDataObject.PenaltyType.Code);
			AssertEquals(Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier, penaltyDataObject.CreditorType.Code);

			AssertNotNull(penaltyDataObject.Creditor);
			AssertEquals(nameof(DocAddressType.Creditor), penaltyDataObject.Creditor.AddressType);
			AssertEquals("TESTORG1", penaltyDataObject.Creditor.OrganizationCode);

			AssertEquals("AUSYD", penaltyDataObject.Location.Code);
			AssertEquals((ZDateTime)TimeSpan.FromDays(3), penaltyDataObject.FreeTime);
			AssertEquals((ZDateTime)TimeSpan.FromDays(9), penaltyDataObject.Duration);
			AssertEquals(TimeUnit.Days, penaltyDataObject.TimeUnit);
			AssertEquals(3.2m, penaltyDataObject.PerUnitCost);
			AssertEquals(500m, penaltyDataObject.TotalCost);
			AssertEquals("AUD", penaltyDataObject.Currency.Code);
			AssertEquals("IMP", penaltyDataObject.ProcessType.Code);
		}
	}
}
