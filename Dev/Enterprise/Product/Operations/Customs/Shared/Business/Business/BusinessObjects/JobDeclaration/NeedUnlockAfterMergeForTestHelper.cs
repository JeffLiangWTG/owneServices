#if DEBUG
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	public static class NeedUnlockAfterMergeForTestHelper
	{
		public static bool NeedUnlockAfterMergeForTest { get => needUnlockAfterMergeForTest; set => needUnlockAfterMergeForTest = value; }

		[ThreadSafe]
		static bool needUnlockAfterMergeForTest = true;
	}
}
#endif
