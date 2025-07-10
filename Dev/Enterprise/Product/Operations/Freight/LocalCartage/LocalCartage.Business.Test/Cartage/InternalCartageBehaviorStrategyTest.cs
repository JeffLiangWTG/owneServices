using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class InternalCartageBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestGetPortOfLoading()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZString.Empty, behaviorStrategy.GetPortOfLoading(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_RL_NKPortOfLoading = "AAA";
			Factory.Save();
			AssertEquals("AAA", behaviorStrategy.GetPortOfLoading(commonCartage));
			AssertEquals(ZString.Empty, behaviorStrategy.GetPortOfLoading(null));
		}

		public void TestGetPortOfDischarge()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZString.Empty, behaviorStrategy.GetPortOfDischarge(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_RL_NKPortOfArrival = "AAA";
			Factory.Save();
			AssertEquals("AAA", behaviorStrategy.GetPortOfDischarge(commonCartage));
			AssertEquals(ZString.Empty, behaviorStrategy.GetPortOfDischarge(null));
		}

		public void TestGetVessel()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZString.Empty, behaviorStrategy.GetVessel(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_VesselName = "AAA";
			Factory.Save();
			AssertEquals("AAA", behaviorStrategy.GetVessel(commonCartage));
			AssertEquals(ZString.Empty, behaviorStrategy.GetVessel(null));
		}

		public void TestGetVoyageFlight()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZString.Empty, behaviorStrategy.GetVoyageFlight(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_VoyageFlightNo = "AAA";
			Factory.Save();
			AssertEquals("AAA", behaviorStrategy.GetVoyageFlight(commonCartage));
			AssertEquals(ZString.Empty, behaviorStrategy.GetVoyageFlight(null));
		}

		public void TestGetE_DEP()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetE_DEP(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_ExportDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetE_DEP(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetE_DEP(null));
		}

		public void TestGetE_ARV()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetE_ARV(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetE_ARV(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetE_ARV(null));
		}

		public void TestGetA_DEP()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetA_DEP(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_ExportDate = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetA_DEP(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetA_DEP(null));
		}

		public void TestGetA_ARV()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetA_ARV(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetA_ARV(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetA_ARV(null));
		}

		public void TestGetFCLReceivalCommences()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLReceivalCommences(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLReceivalCommences(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLReceivalCommences(null));
		}

		public void TestGetLCLReceivalCommences()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLReceivalCommences(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLReceivalCommences(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLReceivalCommences(null));
		}

		public void TestGetFCLCutOff()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLCutOff(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLCutOff(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLCutOff(null));
		}

		public void TestGetLCLCutOff()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLCutOff(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			Factory.Save();
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLCutOff(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLCutOff(null));
		}

		public void TestGetFCLAvailabilityDate()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLAvailabilityDate(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.DocsAndCartage.JP_FCLAvailable = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetFCLAvailabilityDate(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLAvailabilityDate(null));
		}

		public void TestGetLCLAvailabilityDate()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLAvailabilityDate(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.DocsAndCartage.JP_LCLAvailable = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetLCLAvailabilityDate(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLAvailabilityDate(null));
		}

		public void TestGetFCLStorageDate()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLStorageDate(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.DocsAndCartage.JP_FCLStorageCommences = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetFCLStorageDate(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetFCLStorageDate(null));
		}

		public void TestGetLCLStorageDate()
		{
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			var behaviorStrategy = new InternalCartageBehaviorStrategy();
			var parent = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			commonCartage.JJ_ParentTableCode = null;
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLStorageDate(commonCartage));
			commonCartage.JJ_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			commonCartage.JJ_ParentID = parent.PK;
			parent.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.BrettsBirthday;
			Factory.Save();
			AssertEquals(ZDateTime.BrettsBirthday, behaviorStrategy.GetLCLStorageDate(commonCartage));
			AssertEquals(ZDateTime.Empty, behaviorStrategy.GetLCLStorageDate(null));
		}
	}
}
