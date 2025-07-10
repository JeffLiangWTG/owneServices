//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSTTBLineAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSTTBLineAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USTTBLineAddInfoLookups : AutoUSTTBLineAddInfoLookups
	{
		public USTTBLineAddInfoLookups(AutoUSTTBLineAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList PermitExemptionCodes
		{
			get { return TTBExemptionCodeList.GetListForPermit(Factory, Parent.US_ProgramCode); }
		}

		public TTBProgramCodeList ProgramCodes
		{
			get { return Factory.GetCachedValue<TTBProgramCodeList>(); }
		}

		public ICodeDescriptionPairList ProcessingCodes
		{
			get
			{
				switch (Parent.US_ProgramCode)
				{
					case TTBProgramCodeList.Codes.Beverage:
						return Factory.GetCachedValue<TTBBERProcessingCodeList>();
					case TTBProgramCodeList.Codes.DistilledSpirits:
						return Factory.GetCachedValue<TTBDSPProcessingCodeList>();
					case TTBProgramCodeList.Codes.Tobacco:
						return Factory.GetCachedValue<TTBTOBProcessingCodeList>();
					case TTBProgramCodeList.Codes.Wine:
						return Factory.GetCachedValue<TTBWINProcessingCodeList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		protected new USTTBLineAddInfo Parent
		{
			get { return (USTTBLineAddInfo)base.Parent; }
		}
	}
}
