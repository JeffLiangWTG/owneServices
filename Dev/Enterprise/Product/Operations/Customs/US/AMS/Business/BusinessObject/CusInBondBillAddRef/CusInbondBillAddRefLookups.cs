using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInbondBillAddRefLookups : Customs.Business.CusInbondBillAddRefLookups
	{
		public CusInbondBillAddRefLookups(CusInbondBillAddRef parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ReferenceList
		{
			get { return BillReferenceList.GetCachedValue(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection ScheduleKList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today); }
		}

		public RefUNLOCOCollection UNLOCOCollection
		{
			get { return new RefUNLOCOCollection(Factory); }
		}
	}
}
