using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSUnallocatedPackLinesView))]
	public class CFSUnallocatedPackLinesViewBOCollectionTest : BusinessObjectCollectionViewTestCase<CFSUnallocatedPackLinesView>
	{
		protected JobSailing Sailing
		{
			get
			{
				if (fSailing == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					voyage.JV_VoyageFlight = "435";
					voyage.JV_RV_NKVessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_FK;
					VoyageOrigin origin = voyage.Origins.AddNew();
					origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					VoyageDestination destination = voyage.Destinations.AddNew();
					voyage.GenerateSailings();
					fSailing = voyage.Sailings[0];
				}
				return fSailing;
			}
		}
		JobSailing fSailing;

		protected override CFSUnallocatedPackLinesView GetCollectionToTest()
		{
			CFSPackLineNonDependentCollection collection = new CFSPackLineNonDependentCollection(Factory);
			return new CFSUnallocatedPackLinesView(null, collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CFSPackLine result = Factory.New<CFSPackLine>();
			result.JL_FreightMode = FreightConstants.OuterPackType;
			result.JL_PackageCount = 2;
			return result;
		}
	}
}
