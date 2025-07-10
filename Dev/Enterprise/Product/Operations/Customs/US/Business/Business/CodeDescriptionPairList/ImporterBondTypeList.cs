
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class ImporterBondTypeList : CodeDescriptionPairList,
		Integration.Customs.US.IImporterBondTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static class Codes
		{
			public const string ContinuousBond = BondTypeList.Codes.ContinuousBond;
			public const string SingleTransactionBond = BondTypeList.Codes.SingleTransactionBond;
		}

		public static class Descriptions
		{
			public const string ContinuousBond = BondTypeList.Descriptions.ContinuousBond;
			public const string SingleTransactionBond = BondTypeList.Descriptions.SingleTransactionBond;
		}

		public ImporterBondTypeList()
		{
			AddPair(Codes.ContinuousBond, Descriptions.ContinuousBond);
			AddPair(Codes.SingleTransactionBond, Descriptions.SingleTransactionBond);
		}

		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion
	}
}
