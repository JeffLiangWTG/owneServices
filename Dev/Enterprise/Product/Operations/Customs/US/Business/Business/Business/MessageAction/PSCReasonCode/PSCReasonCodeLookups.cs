using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class PSCReasonCodeLookups : ZLookups
	{
		public PSCReasonCodeLookups(PSCReasonCode parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ParentTypeIndicatorList
		{
			get { return Parent.Factory.GetCachedValue<PSCReasonCodeParentTypeList>(); }
		}

		public CodeDescriptionPairList ReasonCodeList
		{
			get
			{
				return Parent.Factory.GetCachedValue(Parent.ParentTypeIndicator, delegate
				{
					return Parent.ParentTypeIndicator == PSCReasonCodeParentTypeList.Codes.Entry ?
						new PSCHeaderReasonList() :
						(CodeDescriptionPairList)new PSCLineReasonList();
				});
			}
		}

		#region Implementation

		protected new PSCReasonCode Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PSCReasonCode)base.Parent; }
		}

		#endregion
	}
}
