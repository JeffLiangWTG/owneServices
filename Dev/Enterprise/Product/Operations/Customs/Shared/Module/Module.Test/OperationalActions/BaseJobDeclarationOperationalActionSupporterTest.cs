using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(BaseJobDeclarationOperationalActionSupporter))]
	sealed class BaseJobDeclarationOperationalActionSupporterTest : Services.OperationalActions.Support.Testing.OperationalActionSupporterTest<BaseJobDeclarationOperationalActionSupporter>
	{
		public void TestPopulateMethods()
		{
			var supporter = new BaseJobDeclarationOperationalActionSupporter();
			var reulst = new ActionMethodProviderID[] { ActionMethodProviderIDs.General,
				ActionMethodProviderIDs.GbJobDeclaration,
				ActionMethodProviderIDs.GbPickupDropOff,
				ActionMethodProviderIDs.USJobDeclaration,
				ActionMethodProviderIDs.CAJobDeclaration,
				ActionMethodProviderIDs.Accounting,
				ActionMethodProviderIDs.JobDeclaration,
				ActionMethodProviderIDs.FrJobDeclaration,
				ActionMethodProviderIDs.TWJobDeclaration,
				ActionMethodProviderIDs.EUJobDeclaration
			};
			AssertContainsExactElementsInAnyOrder(reulst, supporter.Methods.GetAllIds());
		}

		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Customs.JobDeclaration;
			}
		}
	}
}
