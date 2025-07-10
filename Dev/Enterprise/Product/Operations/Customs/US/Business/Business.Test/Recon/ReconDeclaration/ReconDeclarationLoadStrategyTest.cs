using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconDeclarationLoadStrategyTest : TestCaseWithFactory
	{
		public void TestLoadThrowInterface()
		{
			var declaration = Factory.New<JobDeclaration>();
			var recon = ReconDeclaration.Get(declaration);
			var strategies = ((Hashtable)ObjectFactory.Get("BusinessObjectLoadStrategyList")).Cast<DictionaryEntry>().ToDictionary((kvp) => (string)kvp.Key, (kvp) => (ObjectHandle)kvp.Value);
			IBusinessObjectLoadStrategy strategy = null;
			if (strategies.TryGetValue(recon.GetType().FullName, out ObjectHandle handle))
			{
				strategy = (IBusinessObjectLoadStrategy)handle.GetObject();
			}

			AssertNotNull(strategy);
			Assert(strategy is ReconDeclarationLoadStrategy);
		}

		public void TestLoadReconDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var recon = ReconDeclaration.Get(declaration);
			Factory.Save();
			var reconDeclarationBuilder = new ReconDeclarationLoadStrategy();
			var factory = new BusinessObjectFactory();
			var newRecon = reconDeclarationBuilder.Load(factory, recon.PK);
			AssertNotNull(newRecon);
			AssertEquals(recon.PK, newRecon.PK);
		}
	}
}
