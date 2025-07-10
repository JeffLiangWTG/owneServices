using System.Linq;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(USeManifestRegistry))]
	sealed class USeManifestRegistryTest : RegistryItemSetTestCaseWithFactory<USeManifestRegistry>
	{
		public void TestSendMessageAcknowledgements()
		{
			TestRegistryItem(
				ItemSet.SendMessageAcknowledgements,
				"eManifestSendMessageAcknowledgements",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send e-Manifest Message Acknowledgements",
				"Send e-Manifest message acknowledgements to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendMessageAcknowledgements.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}

		public void TestSendHVLVMessageAcknowledgements()
		{
			TestRegistryItem(
				ItemSet.SendHVLVMessageAcknowledgements,
				"eManifestSendHVLVMessageAcknowledgements",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send HVLV e-Manifest Message Acknowledgements",
				"Send HVLV e-Manifest message acknowledgements to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.NoEmails);
			Assert(ItemSet.SendMessageAcknowledgements.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}

		public void TestSendMessageAcknowledgementsToGroup()
		{
			TestRegistryItem(
				ItemSet.SendMessageAcknowledgementsToGroup,
				"eManifestSendMessageAcknowledgementsToGroup",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send e-Manifest Message Acknowledgements To Group",
				"Send e-Manifest message acknowledgements to selected group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.GlbGroup,
				Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendMessageAcknowledgementsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}

		public void TestSendMessageErrors()
		{
			TestRegistryItem(
				ItemSet.SendMessageErrors,
				"eManifestSendMessageErrors",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send e-Manifest Message Errors",
				"Send e-Manifest message errors to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendMessageErrors.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}

		public void TestSendHVLVMessageErrors()
		{
			TestRegistryItem(
				ItemSet.SendHVLVMessageErrors,
				"eManifestSendHVLVMessageErrors",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send HVLV e-Manifest Message Errors",
				"Send HVLV e-Manifest message errors to staff member, nominated group or combination of both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Constants.EmailTo.NoEmails);
			Assert(ItemSet.SendMessageErrors.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}

		public void TestSendMessageErrorsToGroup()
		{
			TestRegistryItem(
				ItemSet.SendMessageErrorsToGroup,
				"eManifestSendMessageErrorsToGroup",
				USeManifestRegistry.Categories.Customs_UnitedStatesofAmerica_eManifest_Notifications,
				"Send e-Manifest Message Errors To Group",
				"Send e-Manifest message errors to selected group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.GlbGroup,
				Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendMessageErrorsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.UnitedStates));
		}
	}
}
