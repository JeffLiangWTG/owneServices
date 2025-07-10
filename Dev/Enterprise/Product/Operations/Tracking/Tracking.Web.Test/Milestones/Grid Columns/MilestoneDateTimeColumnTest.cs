using System;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class MilestoneDateTimeColumnTest : ZDateTimeColumnTest
	{
		protected override Type ExpectedColumnType
		{
			get { return typeof(MilestoneDateTimeColumn); }
		}

		protected override bool ExpectedNoWrapDefaultValue
		{
			get { return true; }
		}
	}
}
