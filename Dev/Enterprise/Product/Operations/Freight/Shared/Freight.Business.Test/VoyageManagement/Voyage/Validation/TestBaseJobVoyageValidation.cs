using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	class TestBaseJobVoyageValidation : BusinessObjectValidationTestCase
	{
		public virtual void TestValidateJV_OH_Line()
		{
			var validation = new BaseJobVoyageValidation(Voyage);

			Voyage.JV_OH_Line = ZGuid.Invalid;
			validation.ValidateJV_OH_Line();
			AssertHasErrors("Invalid guid", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = Factory.New<OrgHeader>().PK;
			validation.ValidateJV_OH_Line();
			AssertHasErrors("Not a valid carrier", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = Voyage.Lookups.Lines.AddNew().PK;
			validation.ValidateJV_OH_Line();
			AssertNoErrors(Voyage.JV_OH_LineInfo);
		}

		#region Implementation

		protected JobVoyage Voyage;
		protected RefVessel Vessel1;
		RefVessel Vessel2;
		VoyageOrigin o1;
		VoyageDestination d1;

		protected virtual BaseJobVoyageValidation GetValidationObject()
		{
			return new BaseJobVoyageValidation(Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_VoyageFlight = "123456";

			Vessel1 = Factory.New<RefVessel>();
			Vessel1.RV_Name = "Gabriela2";

			Vessel2 = Factory.New<RefVessel>();
			Vessel2.RV_Name = "Marcionetti2";
		}

		protected JobVoyage GetPopulatedVoyage()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUMEL") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUBNE") as RefUNLOCO;
			RefUNLOCO r4 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			RefUNLOCO r5 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USSEA") as RefUNLOCO;
			RefUNLOCO r6 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USSAN") as RefUNLOCO;

			foreach (VoyageOrigin origin in new VoyageOrigin[] { o1, o2, o3 })
			{
				SlotAllocation allocation = origin.SlotAllocations.GetAllocation(ZGuid.Empty);
				allocation.SetAspect("AS1", 150);
				allocation.SetAspect("AS2", 50);
				allocation.SetAspect("AS3", 400);
			}

			voyage.Origins.Add(o1);
			voyage.Origins.Add(o2);
			voyage.Origins.Add(o3);
			voyage.Destinations.Add(d1);
			voyage.Destinations.Add(d2);
			voyage.Destinations.Add(d3);

			SlotAllocation countryAllocation = voyage.CurrentCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			countryAllocation.SetAspect("AS1", 450);
			countryAllocation.SetAspect("AS2", 1200);

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			d1.JB_RL_NKPortOfDischarge = r4.RL_Code;
			d2.JB_RL_NKPortOfDischarge = r5.RL_Code;
			d3.JB_RL_NKPortOfDischarge = r6.RL_Code;

			voyage.GenerateSailings();

			return voyage;
		}

		#endregion
	}
}
