using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclarationUniversalCopyTest : Customs.Business.Testing.BaseAddInfoUniversalCopyTest
	{
		protected override void AssertHasOtherNodes(string[] allNodeNames)
		{
			var expectedNodeNames = new[]
			{
				"CustomFields", "WHSPackFilteredLines", "WHSPackLines", "WHSPacks", "DispositionCodes", "FTZDispositionCodes", "DeliveryOrderHeaders","LinkedEntryNumbers", "FSISLines", "OGADispositionCodes"
			};

			CombineAssertions(() =>
			{
				expectedNodeNames.ForEach(c => AssertCollectionContains($"Should contains {c}.", c, allNodeNames));
			});
		}

		protected override IAddInfoManager GetManager()
		{
			return Factory.New<JobDeclaration>();
		}

		public void TestUniversalCopyWithExtendedEntitiesAttributeFinishCopyMethod()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var addressPK = orgHeader.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(ZGuid.Empty, declaration.IOROrgPK);
			using (declaration.JE_OA_DeclarantAddressInfo.SuspendOnValueChanged())
			{
				declaration.JE_OA_DeclarantAddress = addressPK;
			}
			AssertEquals(ZGuid.Empty, declaration.IOROrgPK);

			var attribute = typeof(JobDeclaration).GetCustomAttributes(true).OfType<UniversalCopyWithExtendedEntitiesAttribute>().Single();
			var method = typeof(JobDeclaration).GetMethod(attribute.FinishCopyMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			method.Invoke(declaration, null);

			AssertEquals(orgHeader.PK, declaration.IOROrgPK);
		}
	}
}
