using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

public sealed class PLOrgHeaderValidationHelperTest : TestCaseWithFactory
{
	public static void AssertValidationOrganizationName(ZPropertyInfo info, OrgHeader orgHeaderBUS, OrgHeader orgHeaderNAT, OrgHeader orgHeaderNATEmpty, OrgAddress orgAddress = null)
	{
		AssertFullNameMaxLength(info, orgHeaderBUS, orgHeaderNAT, orgAddress);
		AssertLastNameMaxLength(info, orgHeaderNAT, orgHeaderBUS, orgAddress);
		AssertLastNameNotEmpty(info, orgHeaderNATEmpty, orgHeaderBUS, orgAddress);
	}

	static void AssertFullNameMaxLength(ZPropertyInfo info, OrgHeader orgHeaderBUS, OrgHeader orgHeaderNAT, OrgAddress orgAddress = null)
	{
		const int fullNameMaxLengthForBUS = 70;

		var messageFullNameMaxLength = "The length of the organization name exceeds a maximum of 70 characters and will be truncated in the XML message.";

		CombineAssertions(() =>
		{
			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderBUS.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderBUS.PK;
			}
			AssertHasMessageError($"When {info.HumanReadableName} Organisation Full Name is more than {fullNameMaxLengthForBUS} characters", info, messageFullNameMaxLength);

			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderNAT.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderNAT.PK;
			}
			AssertNoMessageError($"When {info.HumanReadableName} Organisation Last Name is more than {fullNameMaxLengthForBUS} characters", info, messageFullNameMaxLength);
		});
	}

	static void AssertLastNameMaxLength(ZPropertyInfo info, OrgHeader orgHeaderNAT, OrgHeader orgHeaderBUS, OrgAddress orgAddress = null)
	{
		const int lastNameMaxLengthForNAT = 30;

		var messageLastNameMaxLength = "The length of the Name/Surname exceeds a maximum of 30 characters and will be truncated in the XML message.";

		CombineAssertions(() =>
		{
			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderNAT.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderNAT.PK;
			}
			AssertHasMessageError($"When {info.HumanReadableName} Organisation Full Name is more than {lastNameMaxLengthForNAT} characters", info, messageLastNameMaxLength);

			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderBUS.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderBUS.PK;
			}
			AssertNoMessageError($"When {info.HumanReadableName} Organisation Last Name is more than {lastNameMaxLengthForNAT} characters", info, messageLastNameMaxLength);
		});
	}

	static void AssertLastNameNotEmpty(ZPropertyInfo info, OrgHeader orgHeaderNATEmpty, OrgHeader orgHeaderBUS, OrgAddress orgAddress = null)
	{
		var messageLastNameEmpty = "Surname is missing in Organization Full name.";

		CombineAssertions(() =>
		{
			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderNATEmpty.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderNATEmpty.PK;
			}
			AssertHasMessageError($"When {info.HumanReadableName} Organisation Last Name is Empty", info, messageLastNameEmpty);

			if (orgAddress != null)
			{
				orgAddress.OA_OH = orgHeaderBUS.PK;
				info.Value = orgAddress.PK;
			}
			else
			{
				info.Value = orgHeaderBUS.PK;
			}
			AssertNoMessageError($"When {info.HumanReadableName} Organisation Last Name is Empty", info, messageLastNameEmpty);
		});
	}
}
