using WinzorTestFramework;

namespace System.Windows.Forms.Test
{
	internal class WinFormsTestContext : WinzorTestContext
	{
		public WinFormsTestContext()
			: base(WinFormsTestSetup.WinzorDispatcher)
		{
		}
	}
}
