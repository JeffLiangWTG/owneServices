using System.Collections.Generic;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class ZOrganisationControlWithMiscellaneousTest : ZArchitecture.GUI.Testing.ZControlBaseTestCase<ZOrganisationControlWithMiscellaneous>
	{
		protected override string[] BindablePropertyNames
		{
			get
			{
				List<string> all = new List<string>(base.BindablePropertyNames);
				all.Add("MiscellaneousFields");
				return all.ToArray();
			}
		}

		protected override bool UsesControlDataBindings => false;
	}
}
