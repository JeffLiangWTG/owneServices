using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingBulkSailingConsolGeneratorTest : BulkSailingConsolGeneratorTest
	{
		public void TestGenerateConsols_FromSailing_MAWB()
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "081";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_MAWB = "55555625";
			mawb.JM_ServiceLevel = "STD";

			Factory.Save();

			ZDateTime now = ZDateTime.Now;
			now = new ZDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

			JobSailing sailing = CreateFlight("QF123", "AUSYD", "USLAX", now.AddHours(10), now.AddHours(14));
			BaseJobSailingCollection sailings = new BaseJobSailingCollection(Factory);
			sailings.Add(sailing);

			ForwardingBulkSailingConsolGenerator generator = new ForwardingBulkSailingConsolGenerator(Factory);
			generator.WeightUnit = "KG";
			generator.Weight = 200m;
			generator.VolumeUnit = "M3";
			generator.Volume = 2.3m;
			generator.AllocateNeutralMaster = true;

			AssertEquals(0, generator.CreatedConsols.Count);
			generator.GenerateConsols(sailings);

			AssertEquals(1, generator.CreatedConsols.Count);
			generator.CreatedConsols[0].Factory.Save();

			AssertEquals(typeof(ForwardingConsol), generator.CreatedConsols[0].GetType());
			AssertEquals(sailing.PK, generator.CreatedConsols[0].Transports[0].JW_JX);
			AssertEquals(Core.Constants.TransportModes.Air, generator.CreatedConsols[0].JK_TransportMode);
			AssertEquals("AUSYD", generator.CreatedConsols[0].JK_RL_NKLoadPort);
			AssertEquals("USLAX", generator.CreatedConsols[0].JK_RL_NKDischargePort);
			AssertEquals(200m, generator.CreatedConsols[0].JK_TotalShipmentActWeightCheck);
			AssertEquals("KG", generator.CreatedConsols[0].JK_TotalShipmentChargeableUnit);
			AssertEquals(2.3m, generator.CreatedConsols[0].JK_TotalShipmentActVolumeCheck);
			AssertEquals("M3", generator.CreatedConsols[0].JK_TotalShipmentActOtherUnit);
			AssertEquals(true, generator.CreatedConsols[0].JK_IsNeutralMaster);
			AssertEquals("08155555625", generator.CreatedConsols[0].JK_MasterBillNum);

			AssertEquals(true, generator.CreatedConsols.ReadOnly);
		}

		public void TestGenerateConsols_FromSailing_MAWB_AllocateNeutralMaster()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			AddMawb(factory, "081", "00000011", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000022", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000033", GlbBranch.CurrentBranch, "STD");
			AddMawb(factory, "081", "00000044", GlbBranch.CurrentBranch, "STD");

			factory.Save();

			factory = new BusinessObjectFactory();

			ForwardingConsol consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0001";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "081";
			consol.JK_IsNeutralMaster = true;

			factory.Save();

			JobMawb allocatedMawb = LoadJobMawbForConsol(factory, consol);
			AssertEquals("should allocate 08100000011 mawb", "08100000011", allocatedMawb.JM_Airline3DigitPrefix + allocatedMawb.JM_MAWB);
			AssertEquals("mawb allocated", consol, allocatedMawb.Parent);

			factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			CommonConsol[] generatedConsols1 = GenerateConsols(factory1, consol);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CommonConsol[] generatedConsols2 = GenerateConsols(factory2, consol);

			AssertEquals(2, generatedConsols1.Length);
			AssertEquals(2, generatedConsols2.Length);

			factory1.Save();

			AssertExceptionThrown("Should not be able to allocate Neutral MAWB", typeof(MAWBAllocationException), () => generatedConsols2[0].Factory.Save());
			AssertExceptionThrown("Should not be able to allocate Neutral MAWB", typeof(MAWBAllocationException), () => generatedConsols2[1].Factory.Save());

			JobMawb mawb1Gen1 = LoadJobMawbForConsol(factory1, generatedConsols1[0]);
			JobMawb mawb2Gen1 = LoadJobMawbForConsol(factory1, generatedConsols1[1]);

			JobMawb mawb1Gen2 = LoadJobMawbForConsol(factory2, generatedConsols2[0]);
			JobMawb mawb2Gen2 = LoadJobMawbForConsol(factory2, generatedConsols2[1]);

			AssertNotNull("should allocate mawb", mawb1Gen1);
			AssertEquals("should allocate 08100000022 mawb", "08100000022", mawb1Gen1.JM_Airline3DigitPrefix + mawb1Gen1.JM_MAWB);
			AssertNotNull("should allocate mawb", mawb2Gen1);
			AssertNotNull("should not allocate mawb", mawb1Gen2);
			AssertNull("should not allocate mawb", mawb2Gen2);
		}

		#region Implementaion

		CommonConsol[] GenerateConsols(BusinessObjectFactory factory, CommonConsol templateConsol)
		{
			ForwardingBulkSailingConsolGenerator generator = new ForwardingBulkSailingConsolGenerator(factory);
			generator.TemplateConsolPK = templateConsol.PK;
			generator.CreateConsol = true;
			generator.CreateConsol_ReadOnly = true;
			generator.AllocateNeutralMaster = true;

			BaseJobSailingCollection sailings = new BaseJobSailingCollection(factory);
			sailings.Add(GetAirSailing(factory, ZDateTime.Today, "AUSYD", "USLAX", ZDateTime.Today, ZDateTime.Today.AddDays(10)));
			sailings.Add(GetAirSailing(factory, ZDateTime.Today, "AUSYD", "USLAX", ZDateTime.Today, ZDateTime.Today.AddDays(11)));

			generator.GenerateConsols(sailings);

			return generator.CreatedConsols.ToArray<CommonConsol>();
		}

		JobMawb LoadJobMawbForConsol(BusinessObjectFactory factory, CommonConsol consol)
		{
			ZQuery query = new ZQuery(JobMawbSchema.JM_ParentID, consol.PK);
			query.AddToFilter(JobMawbSchema.JM_ParentTableCode, "JK");

			return factory.LoadTop1<JobMawb>(query);
		}

		void AddMawb(BusinessObjectFactory factory, string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			var mawb = factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;
		}

		#endregion
	}
}
