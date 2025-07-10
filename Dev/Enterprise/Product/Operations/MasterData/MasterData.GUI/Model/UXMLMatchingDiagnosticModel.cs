using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public class UXMLMatchingDiagnosticModel
	{
		public UXMLMatchingDiagnosticModel()
		{
		}

		public UXMLMatchingDiagnosticModel(string matchedOrgCode, string matchedOrgName, ZBool matchOrgActiveStatus, string matchedAddressCode, double orgScore, double addressScore, string result, ZGuid orgPK)
		{
			MatchedOrgName = matchedOrgName;
			MatchedOrgCode = matchedOrgCode;
			MatchOrgActiveStatus = matchOrgActiveStatus;

			MatchedAddressCode = matchedAddressCode;

			OrgScoreValue = orgScore;
			AddressScoreValue = addressScore;

			Result = result;

			OrgPK = orgPK;
		}

		public UXMLMatchingDiagnosticModel(OrgHeader matchedOrgHeader, OrgAddress matchedOrgAddress, double orgScore, double addressScore, string result)
			: this(matchedOrgHeader.OH_Code, matchedOrgHeader.OH_FullName, matchedOrgHeader.OH_IsActive, matchedOrgAddress.OA_Code, orgScore, addressScore, result, matchedOrgHeader.PK)
		{
			MatchedOrg = matchedOrgHeader;
			MatchedAddress = matchedOrgAddress;
			MatchedAddressDict = new Dictionary<DataTypes, string>
			{
				{ DataTypes.AddressShortCode, matchedOrgAddress.OA_Code },
				{ DataTypes.AdditionalAddress, matchedOrgAddress.OA_AdditionalAddressInformation },
				{ DataTypes.Address1, matchedOrgAddress.OA_Address1 },
				{ DataTypes.Address2, matchedOrgAddress.OA_Address2 },
				{ DataTypes.Country, matchedOrgAddress.OA_RN_NKCountryCode },
				{ DataTypes.City, matchedOrgAddress.OA_City },
				{ DataTypes.PostCode, matchedOrgAddress.OA_PostCode },
				{ DataTypes.State, matchedOrgAddress.OA_State },
				{ DataTypes.Email, matchedOrgAddress.OA_Email }
			};
		}

		public UXMLMatchingDiagnosticModel(OrgHeader matchedOrgHeader, double orgScore, string result)
			: this(matchedOrgHeader.OH_Code, matchedOrgHeader.OH_FullName, matchedOrgHeader.OH_IsActive, string.Empty, orgScore, 0, result, matchedOrgHeader.PK)
		{
			MatchedOrg = matchedOrgHeader;
		}

		public UXMLMatchingDiagnosticModel(string matchedOrgCode, double orgScore, ZBool matchedOrgActiveStatus, string result, ZGuid orgPK)
			: this(matchedOrgCode, string.Empty, matchedOrgActiveStatus, string.Empty, orgScore, 0, result, orgPK)
		{
		}

		public OrgHeader MatchedOrg { get; }
		public OrgAddress MatchedAddress { get; }

		public string MatchedOrgCode { get; }
		public string MatchedOrgName { get; }

		public Dictionary<DataTypes, string> MatchedAddressDict { get; }
		public string MatchedAddressCode { get; }

		public double OrgScoreValue { get; }
		public string OrgScore => string.Format(CultureInfo.InvariantCulture, "{0}%", OrgScoreValue * 100);
		public double AddressScoreValue { get; }
		public string AddressScore => string.Format(CultureInfo.InvariantCulture, "{0}%", AddressScoreValue * 100);

		public ZBool MatchOrgActiveStatus { get; }

		public string Result { get; }

		string[] orgMatchParts;
		string[] OrgMatchParts => orgMatchParts ?? (orgMatchParts = BuildInfoStringComponents(new[] { Labels.OrgCode, Labels.OrgName }, new[] { MatchedOrgCode, MatchedOrgName }));

		public string orgMatchInfo;
		public string OrgMatchInfo => orgMatchInfo ?? (orgMatchInfo = string.Join("\t\t", OrgMatchParts));

		public string MatchCode => string.Concat(MatchedOrgCode, MatchedAddressCode);

		public ZGuid OrgPK { get; set; }

		public enum DataTypes
		{
			OrgMatchInfo, AddressShortCode, AdditionalAddress,
			Address1, Address2, Country, City, PostCode, State, Email, OrgScore, AddressScore,
			Result, OrgMatchActiveStatus
		}

		string[] BuildInfoStringComponents(IEnumerable<string> labels, IEnumerable<string> values)
		{
			return labels.Zip(values, (string first, string second) => (label: first, value: second))
				.Where(t => !string.IsNullOrEmpty(t.value))
				.Select(t => string.Format(CultureInfo.InvariantCulture, "{0}: {1}", t.label, t.value))
				.ToArray();
		}

		public static class Labels
		{
			public const string OrgCode = "CODE";
			public const string OrgName = "NAME";
		}
	}
}
