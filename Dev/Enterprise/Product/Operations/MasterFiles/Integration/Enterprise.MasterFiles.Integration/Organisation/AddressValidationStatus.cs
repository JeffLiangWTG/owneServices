using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct AddressValidationStatus
	{
		public const string Verified = "VAD";
		public const string ManuallyVerified = "MAN";
		public const string VerifiedToStreet = "VST";
		public const string Unverifiable = "UNV";
		public const string ToBeVerified = "NYV";
		public const string Invalid = "INV";
		public const string CountryNotAvailable = "CNA";
		public const string ExcludeBackgroundValidation = "EBV";
		public const string NotRequired = "NRQ";
	}

	public struct AddressValidationStatusList
	{
		public static CodeDescriptionPairList AddressValidationStatuses
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(AddressValidationStatus.CountryNotAvailable, Res.GetString("a176ec3e-9920-4162-9598-b59190bcc8ec", "Country/Region not available"));
				list.AddPair(AddressValidationStatus.Invalid, Res.GetString("cc3c8f56-7e40-4455-8e82-c810bdab124c", "Invalid"));
				list.AddPair(AddressValidationStatus.ManuallyVerified, Res.GetString("d7f22a4f-2333-473c-b98e-01529b0e0adb", "Manually verified"));
				list.AddPair(AddressValidationStatus.ToBeVerified, Res.GetString("012c48d2-1ee9-4edf-8b95-3733a2a9dead", "To be verified"));
				list.AddPair(AddressValidationStatus.Unverifiable, Res.GetString("2e7ef45d-17b6-4f67-9ffc-53142da7218a", "Unverifiable"));
				list.AddPair(AddressValidationStatus.Verified, Res.GetString("1484c891-ba4f-4dd5-bdbc-b636f45dc556", "Verified"));
				list.AddPair(AddressValidationStatus.VerifiedToStreet, Res.GetString("44210c74-6ba0-4a2a-a7e7-8e8d2b10a3ab", "Verified to street address"));
				list.AddPair(AddressValidationStatus.ExcludeBackgroundValidation, Res.GetString("56ca2967-2915-4bfe-a630-ca70f95d204e", "Excluded from background validation"));
				list.AddPair(AddressValidationStatus.NotRequired, Res.GetString("94a08561-29a7-41aa-aa76-976988b2c741", "Not required"));
				return list;
			}
		}

		[SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0661 error")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		[SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0660 error")]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public static bool operator ==(AddressValidationStatusList list1, AddressValidationStatusList list2)
		{
			return list1 == list2;
		}

		public static bool operator !=(AddressValidationStatusList list1, AddressValidationStatusList list2)
		{
			return list1 != list2;
		}
	}
}
