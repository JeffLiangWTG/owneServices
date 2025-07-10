using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckE2_OA_AddressDoesNotThrowNullReferenceExceptionWhenCheckingAddressForCartageWithNullCartageType()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			((BusinessObject)shipment).FillWithValidTestData();
			Factory.Save();

			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_E3_NKJobType = string.Empty;
			cartage.JJ_Direction = string.Empty;
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = "JS";

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = cartage.PK;
			jobDocAddress.E2_ParentTableCode = cartage.TablePrefix;
			var cartageJobDocAddress = (JobDocAddress)cartage.DocAddresses.First();
			AssertNoExceptionThrown(
				"Validation CommonCartageDocAddressValidation.CheckE2_OA_Address() should not fail due to null reference exception",
				() => cartageJobDocAddress.Validation.ValidateAll());
		}

		public void TestValidateFirstDocAddress()
		{
			var job = Factory.New<CommonCartage>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();
			job.JJ_E3_NKJobType = ZString.Empty;
			AssertNull(job.FirstDocAddress);
			job.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			job.FirstDocAddress.Validation.ValidateAll();
			AssertEquals("Job 1 first address should not have Warnings", true, job.FirstDocAddress.HasWarnings);
			AssertEquals("Job 1 first address should not have Errors", false, job.FirstDocAddress.HasErrors);
		}

		public void TestValidateSecondDocAddress()
		{
			var job = Factory.New<CommonCartage>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();
			job.JJ_E3_NKJobType = ZString.Empty;
			AssertNull(job.SecondDocAddress);
			job.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			job.SecondDocAddress.Validation.ValidateAll();
			AssertEquals("Job 1 second address should have Warnings", true, job.SecondDocAddress.HasWarnings);
			AssertEquals("Job 1 second address should not have Errors", false, job.SecondDocAddress.HasErrors);
			job.SecondDocAddress.Validation.ValidateAll();
			job.SecondDocAddress.E2_OA_Address = address.PK;
			AssertEquals("Job 1 second address should not have Warnings", false, job.SecondDocAddress.HasWarnings);
			AssertEquals("Job 1 second address should not have Errors", false, job.SecondDocAddress.HasErrors);
		}

		public void TestValidateThirdDocAddress()
		{
			var job = Factory.New<CommonCartage>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();
			job.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			job.ThirdDocAddress.Validation.ValidateAll();
			AssertEquals("Job third address should have Warnings", true, job.ThirdDocAddress.HasWarnings);
			AssertEquals("Job third address should not have Errors", false, job.ThirdDocAddress.HasErrors);
			job.ThirdDocAddress.Validation.ValidateAll();
			job.ThirdDocAddress.E2_OA_Address = address.PK;
			AssertEquals("Job third address should not have Warnings", false, job.ThirdDocAddress.HasWarnings);
			AssertEquals("Job third address should not have Errors", false, job.ThirdDocAddress.HasErrors);
		}

		OrgHeader Exporter;
		OrgHeader Importer;
		protected override void SetUp()
		{
			base.SetUp();
			if (Exporter == null)
			{
				Exporter = Factory.New<OrgHeader>();
				Exporter.OH_Code = "HELCNR";
				Exporter.OH_FullName = "HELLO CONSIGNOR";
				Exporter.OH_IsConsignor = true;
				Exporter.MainAddress.OA_Address1 = "ADDRESS";
			}

			if (Importer == null)
			{
				Importer = Factory.New<OrgHeader>();
				Importer.OH_Code = "HELCNE";
				Importer.OH_FullName = "HELLO CONSIGNEE";
				Importer.OH_IsConsignee = true;
				Importer.MainAddress.OA_Address1 = "ADDRESS";
			}

			Factory.Save();
		}

		protected static int GetNextOrganisationNumber()
		{
			return fNextOrganisationNumber++;
		}

		static int fNextOrganisationNumber;
		protected static ZString HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}

				return "AUSYD";
			}
		}

		public OrgHeader LocalCTO
		{
			get
			{
				if (fLocalCTO == null)
				{
					fLocalCTO = Factory.New<OrgHeader>();
					fLocalCTO.OH_Code = "LCLCTO";
					fLocalCTO.OH_IsMiscFreightServices = true;
					fLocalCTO.OH_IsAirCTO = true;
					fLocalCTO.OH_IsSeaCTO = true;
					fLocalCTO.OH_FullName = "Local CTO " + GetNextOrganisationNumber();
					fLocalCTO.MainAddress.OA_Address1 = "Test Address Line";
					fLocalCTO.OH_RL_NKClosestPort = HomePort;
				}

				return fLocalCTO;
			}
		}

		OrgHeader fLocalCTO;
		OrgHeader fLocalContainerYard;
		public OrgHeader LocalContainerYard
		{
			get
			{
				if (fLocalContainerYard == null)
				{
					fLocalContainerYard = Factory.New<OrgHeader>();
					fLocalContainerYard.OH_Code = "LCLCPK";
					fLocalContainerYard.OH_IsMiscFreightServices = true;
					fLocalContainerYard.OH_IsContainerYard = true;
					fLocalContainerYard.OH_FullName = "Local ContainerYard " + GetNextOrganisationNumber();
					fLocalContainerYard.MainAddress.OA_Address1 = "Test Address Line";
					fLocalContainerYard.OH_RL_NKClosestPort = HomePort;
				}

				return fLocalContainerYard;
			}
		}
	}
}
