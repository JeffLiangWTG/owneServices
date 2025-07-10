using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbPortDeliveryTimeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG1_DaysDelayFromArrivalToDeliver()
		{
			//			DeliveryTime.G1_DaysDelayFromArrivalToDeliver = 0;
			//			Assert("Invalid number of delaying days", DeliveryTime.G1_DaysDelayFromArrivalToDeliverInfo.HasErrors());

			DeliveryTime.G1_DaysDelayFromArrivalToDeliver = 0;
			Assert("Nnumber of delaying days no errors", !DeliveryTime.G1_DaysDelayFromArrivalToDeliverInfo.HasErrors());
		}

		public void TestCheckG1_GC_Company()
		{
			AssertEquals("Company is the current one", (ZGuid)Env.CurrentCompany.PK, DeliveryTime.G1_GC_Company);
		}

		public void TestCheckG1_RL_NKDischargePort()
		{
			DeliveryTime.G1_RL_NKDischargePort = "xxxxx";
			Assert("Invalid Discharge Port - it doesn't exist!", DeliveryTime.G1_RL_NKDischargePortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDischargePort = "yyy";
			Assert("Discharge Port Length should be 5 characters!", DeliveryTime.G1_RL_NKDischargePortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDischargePort = ZString.Empty;
			Assert("Discharge Port should not be empty!", DeliveryTime.G1_RL_NKDischargePortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDischargePort = "AUSYD";
			Assert("Discharge Port should not have errors", !DeliveryTime.G1_RL_NKDischargePortInfo.HasErrors());
		}

		public void TestCheckG1_RL_NKDestinationPort()
		{
			DeliveryTime.G1_RL_NKDestinationPort = "xxxxx";
			Assert("Invalid Destination Port - it doesn't exist!", DeliveryTime.G1_RL_NKDestinationPortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDestinationPort = "yyy";
			Assert("Destination Port Length should be 5 characters!", DeliveryTime.G1_RL_NKDestinationPortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDestinationPort = ZString.Empty;
			Assert("Destination Port should not be empty!", DeliveryTime.G1_RL_NKDestinationPortInfo.HasErrors());

			DeliveryTime.G1_RL_NKDestinationPort = "AUSYD";
			Assert("Destination Port should not have errors", !DeliveryTime.G1_RL_NKDestinationPortInfo.HasErrors());
		}

		public void TestValidateFreightMode()
		{
			DeliveryTime.G1_FreightMode = "ABC";
			Assert("Invalid Freight Mode. Error expected.", DeliveryTime.G1_FreightModeInfo.HasErrors());

			DeliveryTime.G1_FreightMode = "";
			Assert("Empty Freight Mode. Error expected.", DeliveryTime.G1_FreightModeInfo.HasErrors());

			DeliveryTime.G1_FreightMode = Constants.TransportModes.Sea;
			Assert("Valid Freight Mode. No error expected.", !DeliveryTime.G1_FreightModeInfo.HasErrors());
		}

		public void TestValidateForExistsPortDeliveryTime()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var clientPk = client.PK;

			DeliveryTime.G1_FreightMode = "FCL";
			DeliveryTime.G1_JobMode = "FWD";
			DeliveryTime.G1_RL_NKDestinationPort = "USLAX";
			DeliveryTime.G1_RL_NKDischargePort = "USCHI";
			DeliveryTime.G1_OH_ClientOverride = clientPk;
			Factory.Save();

			var duplicateDelivery = Factory.New<GlbPortDeliveryTime>();
			duplicateDelivery.G1_OH_ClientOverride = clientPk;
			duplicateDelivery.G1_FreightMode = "FCL";
			duplicateDelivery.G1_JobMode = "FWD";
			duplicateDelivery.G1_RL_NKDestinationPort = "USLAX";
			duplicateDelivery.G1_RL_NKDischargePort = "USCHI";

			Assert("Freight Mode should not have errors", !duplicateDelivery.G1_FreightModeInfo.HasErrors());
			Assert("Job Mode should not have errors", !duplicateDelivery.G1_JobModeInfo.HasErrors());
			Assert("Destination Port should not have errors", !duplicateDelivery.G1_RL_NKDestinationPortInfo.HasErrors());
			Assert("Client Override should not have errors", !duplicateDelivery.G1_OH_ClientOverrideInfo.HasErrors());
			Assert("Dicharge Port should have errors", duplicateDelivery.G1_RL_NKDischargePortInfo.HasErrors());

			duplicateDelivery.Validation.ValidateAll();
			Assert("Freight Mode should have errors", duplicateDelivery.G1_FreightModeInfo.HasErrors());
			Assert("Job Mode should have errors", duplicateDelivery.G1_JobModeInfo.HasErrors());
			Assert("Destination Port should have errors", duplicateDelivery.G1_RL_NKDestinationPortInfo.HasErrors());
			Assert("Dicharge Port should have errors", duplicateDelivery.G1_RL_NKDischargePortInfo.HasErrors());
			Assert("Client Override should have errors", duplicateDelivery.G1_OH_ClientOverrideInfo.HasErrors());

			duplicateDelivery.G1_RL_NKDischargePort = "USLAX";
			duplicateDelivery.Validation.ValidateAll();
			Assert("Freight Mode should not have errors", !duplicateDelivery.G1_FreightModeInfo.HasErrors());
			Assert("Job Mode should not have errors", !duplicateDelivery.G1_JobModeInfo.HasErrors());
			Assert("Destination Port should not have errors", !duplicateDelivery.G1_RL_NKDestinationPortInfo.HasErrors());
			Assert("Dicharge Port should not have errors", !duplicateDelivery.G1_RL_NKDischargePortInfo.HasErrors());
			Assert("Client Override should not have errors", !duplicateDelivery.G1_OH_ClientOverrideInfo.HasErrors());

			duplicateDelivery.G1_RL_NKDischargePort = "USCHI";
			duplicateDelivery.G1_OH_ClientOverride = ZGuid.NewZGuid();
			duplicateDelivery.Validation.ValidateAll();
			Assert("Freight Mode should not have errors", !duplicateDelivery.G1_FreightModeInfo.HasErrors());
			Assert("Job Mode should not have errors", !duplicateDelivery.G1_JobModeInfo.HasErrors());
			Assert("Destination Port should not have errors", !duplicateDelivery.G1_RL_NKDestinationPortInfo.HasErrors());
			Assert("Dicharge Port should not have errors", !duplicateDelivery.G1_RL_NKDischargePortInfo.HasErrors());
			Assert("Client Override should not have errors", !duplicateDelivery.G1_OH_ClientOverrideInfo.HasErrors());
		}

		#region Implementation

		GlbPortDeliveryTime fDeliveryTime;
		GlbPortDeliveryTime DeliveryTime
		{
			get
			{
				if (fDeliveryTime == null)
				{
					fDeliveryTime = Factory.New<GlbPortDeliveryTime>();
				}
				return fDeliveryTime;
			}
		}

		#endregion
	}
}
