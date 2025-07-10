using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountDissectionLookups : AutoAccAlternateGLAccountDissectionLookups
	{
		public AccAlternateGLAccountDissectionLookups(AutoAccAlternateGLAccountDissection parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AttributeList => GetAttributeList();

		public static class NonGlobalAttributeCode
		{
			public const string ORG = "ORG";
		}

		public static CodeDescriptionPairList GetAttributeList() => AccountingMasterFilesConstants.GetAttributeList();

		public static CodeDescriptionPairList GetNonGlobalAttributeList() => AccountingMasterFilesConstants.GetNonGlobalAttributeList();
	}
}
