using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public sealed class USeManifestRegistry : RegistryItemSet
	{
		USeManifestRegistry() { }

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_UnitedStatesofAmerica { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"United States of America"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_eManifest { get { return CombineCategories(Customs_UnitedStatesofAmerica, (NoResString)"e-Manifest"); } }
			public static MultilingualString Customs_UnitedStatesofAmerica_eManifest_Notifications { get { return CombineCategories(Customs_UnitedStatesofAmerica_eManifest, (NoResString)"Notifications"); } }
		}

		#endregion

		#region Notification Groups

		public CodePairRegistryItem SendMessageAcknowledgements
		{
			get
			{
				return GetItem(
					"eManifestSendMessageAcknowledgements",
					() => new CodePairRegistryItem(
							"eManifestSendMessageAcknowledgements",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("7253827B-4382-47B1-8BCD-FECAD17E39C2", "Send e-Manifest Message Acknowledgements"),
							ResString.GetMultilingualString("3B1D707A-42A6-4452-B104-7810510F2793", "Send e-Manifest message acknowledgements to staff member, nominated group or combination of both"),
							new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction });
			}
		}

		public CodePairRegistryItem SendHVLVMessageAcknowledgements
		{
			get
			{
				return GetItem(
					"eManifestSendHVLVMessageAcknowledgements",
					() => new CodePairRegistryItem(
							"eManifestSendHVLVMessageAcknowledgements",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("20D3B7AC-2821-438A-ADBA-E1CDE2C55D7D", "Send HVLV e-Manifest Message Acknowledgements"),
							ResString.GetMultilingualString("C4038890-7962-4813-99C6-DDD9E2864E09", "Send HVLV e-Manifest message acknowledgements to staff member, nominated group or combination of both"),
							new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.EmailTo.NoEmails)
					{ CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction });
			}
		}

		public GuidRegistryItem SendMessageAcknowledgementsToGroup
		{
			get
			{
				return GetItem(
					"eManifestSendMessageAcknowledgementsToGroup",
					() => new GuidRegistryItem(
							"eManifestSendMessageAcknowledgementsToGroup",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("648190C8-41B3-41E0-98D1-CEEF094CA957", "Send e-Manifest Message Acknowledgements To Group"),
							ResString.GetMultilingualString("9B8175C7-F959-466B-8C29-35629917BB03", "Send e-Manifest message acknowledgements to selected group"),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.Groups.PostMastersGroupPK)
					{
						CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction,
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public CodePairRegistryItem SendMessageErrors
		{
			get
			{
				return GetItem(
					"eManifestSendMessageErrors",
					() => new CodePairRegistryItem(
							"eManifestSendMessageErrors",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("59987030-6AB8-4C72-B916-D4C6A43589F2", "Send e-Manifest Message Errors"),
							ResString.GetMultilingualString("34FCC5D6-C1D3-45AD-944D-6FF61B26B98B", "Send e-Manifest message errors to staff member, nominated group or combination of both"),
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction });
			}
		}

		public CodePairRegistryItem SendHVLVMessageErrors
		{
			get
			{
				return GetItem(
					"eManifestSendHVLVMessageErrors",
					() => new CodePairRegistryItem(
							"eManifestSendHVLVMessageErrors",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("185C2623-1E81-4120-8080-B918299428E1", "Send HVLV e-Manifest Message Errors"),
							ResString.GetMultilingualString("7E4C196B-67B4-4E85-8DC3-CF61BBCDC875", "Send HVLV e-Manifest message errors to staff member, nominated group or combination of both"),
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.EmailTo.NoEmails)
					{ CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction });
			}
		}

		public GuidRegistryItem SendMessageErrorsToGroup
		{
			get
			{
				return GetItem(
					"eManifestSendMessageErrorsToGroup",
					() => new GuidRegistryItem(
							"eManifestSendMessageErrorsToGroup",
							Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
							ResString.GetMultilingualString("1FC72954-C1DC-4DD5-9E55-62CDCD54B815", "Send e-Manifest Message Errors To Group"),
							ResString.GetMultilingualString("283FC346-541A-49B2-B5A5-4B72DDC8029A", "Send e-Manifest message errors to selected group"),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Constants.Groups.PostMastersGroupPK)
					{
						CountryFilterPKs = CountryGuids.CountriesUnderUSCustomsJurisdiction,
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		#endregion

		#region Implementation

		public static USeManifestRegistry Instance
		{
			get { return instance ?? (instance = new USeManifestRegistry()); }
		}

		[ThreadStatic]
		static USeManifestRegistry instance;

		#endregion
	}
}
