//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSImportMessageSendingActionLookups
//
//    This class should be used for overriding collections in AutoUSImportMessageSendingActionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USImportMessageSendingActionLookups : ZLookups
	{
		public USImportMessageSendingActionLookups(AutoUSImportMessageSendingAction parent)
			: base(parent)
		{
		}

		public AIITitleOfDeclarant TitleOfDeclarantList
		{
			get { return Factory.GetCachedValue<AIITitleOfDeclarant>(); }
		}

		public CodeDescriptionPairList CollectionBillInformationCodesList
		{
			get { return Factory.GetCachedValue<CollectionBillInformationCodesList>(); }
		}

		public CodeDescriptionPairList YesNoList
		{
			get
			{
				return Factory.GetCachedValue("YesNoList", delegate
				{
					var result = new YesNoDefaultList();
					result.RemoveCode(YesNoDefaultList.Codes.Default);
					return result;
				});
			}
		}

		public ReasonCodeList ReasonCodeList
		{
			get
			{
				var messageSendingAction = Parent as ImportMessageSendingAction;
				if (messageSendingAction != null)
				{
					var entryType = messageSendingAction.Declaration.US_EntryType;
					return Factory.GetCachedValue("ReasonCodeList" + entryType,
					delegate
					{
						var result = new ReasonCodeList();
						if (entryType != EntryTypeList.Codes.ConsumptionFTZ)
						{
							result.RemoveCode(Business.ReasonCodeList.Codes.NoForeignStatusGoodsRemovedFromFTZ);
						}
						return result;
					});
				}

				return Factory.GetCachedValue<ReasonCodeList>();
			}
		}

		public CertificationOptionsList CertificationOptionsList
		{
			get { return Factory.GetCachedValue<CertificationOptionsList>(); }
		}

		public CodeDescriptionPairList ActionTypeList
		{
			get { return Factory.GetCachedValue<ACECargoReleaseActionType>(); }
		}

		public ACEPNActionCodeList ACEPNActionCodeList
		{
			get { return Factory.GetCachedValue<ACEPNActionCodeList>(); }
		}
	}
}
