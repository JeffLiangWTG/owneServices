using NUnit.Framework;
using static Enterprise.MasterFiles.Business.EInvoicingCredentialsRegistryItem;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class EInvoicingCredentialsRegistryItemBehaviorExtensionsTest : TestCase
	{
		public void TestHasApiKeyFlag()
		{
			var behavior = Behavior.HasAPIKey;
			Assert(behavior.HasAPIKeyFlag());

			behavior = Behavior.Default;
			Assert(!behavior.HasAPIKeyFlag());

			behavior = Behavior.SetPasswordChar;
			Assert(behavior.SetPasswordCharFlag());

			behavior = Behavior.Default;
			Assert(!behavior.SetPasswordCharFlag());

			behavior = Behavior.HasAPIKey | Behavior.SetPasswordChar;
			Assert(behavior.HasAPIKeyFlag());

			behavior = Behavior.HasAPIKey | Behavior.SetPasswordChar;
			Assert(behavior.SetPasswordCharFlag());
		}
	}
}
