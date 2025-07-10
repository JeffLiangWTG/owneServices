using System.Linq;
using CargoWise.Common;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	internal class ZAJobDeclarationIAccIntegrationDataProvider : JobDeclarationIAccIntegrationDataProvider
	{
		public ZAJobDeclarationIAccIntegrationDataProvider(JobDeclaration declaration) : base(declaration, true)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override void OnIntegratedWithAccountingSuccessfully()
		{
			base.OnIntegratedWithAccountingSuccessfully();
			if ((this.actions & ChargePosterBehaviours.AutoRateDSB) == ChargePosterBehaviours.AutoRateDSB)
			{
				declaration.ActiveEntryHeaders.OfType<CusEntryHeader>()
					.Where(x => this.entryHeaderPKs.Contains(x.PK))
					.ForEach(x => x.ClearNeedsAutoRateDSB());
			}
		}
	}
}
