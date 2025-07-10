using System;
using System.Windows.Forms;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(DeniedPartySecurityOverrideForm))]
	class DeniedPartyLoginFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DeniedPartySecurityOverrideForm(new DpsSecurityOverride(new Func<SecurityCore, SecurityCheckpoint>[] { s => s.DpsAllowUpdateToClear }));
		}
	}
}
