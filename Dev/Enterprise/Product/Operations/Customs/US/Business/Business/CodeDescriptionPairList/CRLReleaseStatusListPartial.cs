using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class CRLReleaseStatusList : CodeDescriptionPairList,
		Integration.Customs.US.IReleaseStatusCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		#region ICodeDescriptionPairListProvider Members

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

		public static bool IsExmOrHld(ZString code)
		{
			return code == Codes.EXM || code == Codes.HLD;
		}

		public static bool IsREL(ZString code)
		{
			return code == Codes.REL;
		}
	}
}
