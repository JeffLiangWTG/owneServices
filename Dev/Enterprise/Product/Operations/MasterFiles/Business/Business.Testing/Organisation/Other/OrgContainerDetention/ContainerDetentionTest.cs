using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContainerDetention))]
	sealed class ContainerDetentionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlySecurity()
		{
			bool oldSecurityValue = Env.Security.OrgCarrierModify.IsAllowed;
			OrgHeader org = Factory.New<OrgHeader>();

			try
			{
				OrgContainerDetention testDetention = org.CarrierContainerPenalties.AddNew();

				Env.Security.OrgCarrierModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !testDetention.PD_FreeDaysInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testDetention.PD_OH_ClientInfo.ReadOnly);
				Assert("Access Allowed - Not ReadOnly", !testDetention.PD_OriginPortOrCountryInfo.ReadOnly);

				Env.Security.OrgCarrierModify.IsAllowed = false;
				Assert("Access NOT Allowed - ReadOnly", testDetention.PD_FreeDaysInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetention.PD_OH_ClientInfo.ReadOnly);
				Assert("Access NOT Allowed - ReadOnly", testDetention.PD_OriginPortOrCountryInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgCarrierModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestPenaltyDescription()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContainerDetention testDetention = org.CarrierContainerPenalties.AddNew();

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			AssertEquals("Import Detention", testDetention.PenaltyDescription);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			AssertEquals("Export Detention", testDetention.PenaltyDescription);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals("Import Demurrage", testDetention.PenaltyDescription);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals("Export Demurrage", testDetention.PenaltyDescription);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			AssertEquals("Import Merged Demurrage and Detention", testDetention.PenaltyDescription);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			AssertEquals("Export Merged Demurrage and Detention", testDetention.PenaltyDescription);
		}

		public void TestPD_OH_CTO_ReadOnly()
		{
			var org = Factory.New<OrgHeader>();
			var testDetention = org.CarrierContainerPenalties.AddNew();

			Assert(!testDetention.PD_OH_CTOInfo.ReadOnly);

			var cto = Factory.New<OrgHeader>();
			testDetention.PD_OH_CTO = cto.PK;

			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			Assert(testDetention.PD_OH_CTOInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, testDetention.PD_OH_CTO);

			testDetention.PD_OH_CTO = cto.PK;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			Assert(!testDetention.PD_OH_CTOInfo.ReadOnly);
			AssertEquals(cto.PK, testDetention.PD_OH_CTO);
		}

		public void TestSetDefaultFreeDayType()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContainerDetention testDetention = org.CarrierContainerPenalties.AddNew();
			AssertEquals(Core.Constants.ContainerDetentionFreeDayType.CTOAvailable, testDetention.PD_FreeDayType);

			testDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			AssertEquals(Core.Constants.ContainerDetentionFreeDayType.FCLLoad, testDetention.PD_FreeDayType);

			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			AssertEquals(Core.Constants.ContainerDetentionFreeDayType.WharfGateIn, testDetention.PD_FreeDayType);

			testDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			AssertEquals(Core.Constants.ContainerDetentionFreeDayType.FCLLoad, testDetention.PD_FreeDayType);
		}

		public void TestFirstFreeDayType()
		{
			var org = Factory.New<OrgHeader>();

			var penalty = org.CarrierContainerPenalties.AddNew();

			penalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			Assert("PD_FirstFreeDayType is readonly for EXP penalties", penalty.PD_FirstFreeDayType_ReadOnly);

			penalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			Assert("PD_FirstFreeDayType is not readonly for IMP penalties", !penalty.PD_FirstFreeDayType_ReadOnly);

			penalty.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			penalty.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.CTOAvailable;

			AssertEquals("PD_FirstFreeDayType is a proxy for PD_FreeDayType",
				Core.Constants.ContainerDetentionFreeDayType.CTOAvailable, penalty.PD_FirstFreeDayType);
		}

		public void TestLastFreeDayType()
		{
			var org = Factory.New<OrgHeader>();

			var penalty = org.CarrierContainerPenalties.AddNew();

			penalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			Assert("PD_LastFreeDayType is readonly for IMP penalties", penalty.PD_LastFreeDayType_ReadOnly);

			penalty.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			Assert("PD_LastFreeDayType is not readonly for EXP penalties", !penalty.PD_LastFreeDayType_ReadOnly);

			penalty.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			penalty.PD_FreeDayType = Core.Constants.ContainerDetentionFreeDayType.WharfGateIn;

			AssertEquals("PD_LastFreeDayType is a proxy for PD_FreeDayType",
				Core.Constants.ContainerDetentionFreeDayType.WharfGateIn, penalty.PD_LastFreeDayType);
		}
	}
}
