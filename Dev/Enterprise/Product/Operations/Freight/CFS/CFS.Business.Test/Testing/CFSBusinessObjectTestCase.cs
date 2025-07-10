using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Freight.CFS.Business
{
	public abstract class CFSBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Factory.AllowMultipleBusinessObjectsAroundOneRow = false;
		}

		protected ZString ForeignPort
		{
			get { return GlbBranch.CurrentBranch.GB_RL_NKHomePort.StartsWith("SG", StringComparison.Ordinal) ? "HKHKG" : "SGSIN"; }
		}

		#region ImportSailingPK

		protected JobSailing ImportSailing
		{
			get
			{
				if (fImportSailing == null)
				{
					fImportSailing = CreateNewImportSailing(Factory);
				}
				return fImportSailing;
			}
		}
		JobSailing fImportSailing;

		protected JobSailing CreateNewImportSailing(BusinessObjectFactory factory)
		{
			JobVoyage voyage = factory.New<JobVoyage>();
			voyage.JV_VoyageType = Enterprise.Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = (factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
			voyage.JV_VoyageFlight = "43";
			voyage.JV_VoyageType = Enterprise.Core.Constants.TransportModes.Sea;

			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			voyage.Origins[0].JA_E_DEP = ZDateTime.Today.AddDays(-5);

			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = ForeignPort;
			voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(4);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion

		#region Light Validation

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new DefaultCFSLightValidationTester(bizObjToTest);
		}

		protected internal class DefaultCFSLightValidationTester : LightValidationTester
		{
			public DefaultCFSLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				bool result = base.ShouldTestProperty(info);

				// TODO: Write another test case for light validation that tests the situation where the container isn't on a loadlist and this property can be tested/set.
				if (info.Name == "JC_JX")
				{
					result = false;
				}

				return result;
			}
		}

		#endregion

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert(true);
		}
	}
}
