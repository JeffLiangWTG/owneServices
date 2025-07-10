using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationSynchroniserForTest : JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniserForTest(BaseJobDeclaration destination)
			 : base(destination)
		{
		}

		public new ZBool ShouldSynchroniseContainerModeWithPackingMode => base.ShouldSynchroniseContainerModeWithPackingMode;
	}
}
