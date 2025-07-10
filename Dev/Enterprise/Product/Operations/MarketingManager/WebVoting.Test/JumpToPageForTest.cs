using System;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class JumpToPageForTest : JumpToPage
	{
		internal void OnLoadExposed() => OnLoad(EventArgs.Empty);
	}
}
