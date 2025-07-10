using CargoWise.Types;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class DummyAUDec : DummyScreeningPartyProviderWithJobDeclaration, Enterprise.Integration.Customs.AU.IJobDeclaration
	{
		public bool IsQuarantine
		{
			get { return false; }
		}

		public ZBool IsNEXDOCSActive => false;

		public bool DeclarationHasEntryHeader
		{
			get { return false; }
		}
	}
}
