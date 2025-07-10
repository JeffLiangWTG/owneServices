using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationViewStmNumsValidation : ViewStmNumsValidation
	{
		public OrganisationViewStmNumsValidation(OrganisationViewStmNums parent)
			: base(parent)
		{
		}

		protected new OrganisationViewStmNums Parent
		{
			get { return (OrganisationViewStmNums)base.Parent; }
		}

		#region SN_Type

		protected override void CheckSN_Type()
		{
			base.CheckSN_Type();
			ValidateSN_ClientPrefix();
			ValidateSN_ZoneIDPrefix();
			EnsureOrgTypeMatchesFountainType(Parent.SN_TypeInfo);
		}

		void EnsureOrgTypeMatchesFountainType(ZPropertyInfo sN_TypeInfo)
		{
			var org = Parent.Header;
			if (org != null)
			{
				if (!org.OH_IsWarehouseClient && sN_TypeInfo.Value.ToString() == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumberForWarehouse)
				{
					sN_TypeInfo.AddError(Res.GetString("ViewStmNumsValidation.MustBeWarehouse", "Organization must be a Warehouse to use this fountain type"));
				}
			}
		}

		#endregion

		#region SN_Prefix

		protected override void CheckSN_Prefix()
		{
			base.CheckSN_Prefix();
			var header = Parent.Header;
			if (Parent.SN_Type == OrgConstants.NumberFountains.Code.SSCCBarCodeNumbers)
			{
				if (!Enterprise.NumberFountain.SSCCBarCodeChecker.IsSSCCBarCodePrefix(Parent.SN_Prefix))
				{
					int prefixMinLength = Enterprise.NumberFountain.SSCCBarCodeChecker.PrefixMinLength;
					int prefixMaxLength = Enterprise.NumberFountain.SSCCBarCodeChecker.PrefixMaxLength;

					var message = ResString.GetMultilingualString("abcbdfff-e330-47c9-8231-d916c3e2de70", "SSCC Prefix must be {0}-{1} digits.", prefixMinLength, prefixMaxLength);
					Parent.SN_PrefixInfo.AddError(message);
				}
				else if (header != null && !header.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.CodeTypes.GS1 && x.OK_CustomsRegNo == Parent.SN_Prefix))
				{
					Parent.SN_PrefixInfo.AddError(ResString.GetMultilingualString("af398fc5-325f-4d1e-9087-aeb7bf65c18a", "SSCC Prefix must match at least one GS1 code in Config->Registration Numbers / Codes."));
				}
			}
		}

		public void ValidateSN_ZoneIDPrefix()
		{
			ValidateCalculatedProperty(Parent.SN_ZoneIDPrefixInfo);
		}

		protected virtual void CheckSN_ZoneIDPrefix()
		{
			if (Parent.SN_Type == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber)
			{
				var length = Parent.SN_ZoneIDPrefix.Length;
				var useNewFormat = length != OrganisationViewStmNums.Schema.OldSN_ZoneIDPrefixMaxLength;
				if (!IsZoneIdValid(Parent.SN_ZoneIDPrefix, useNewFormat))
				{
					var message = useNewFormat ?
						ResString.GetMultilingualString("B6F6F96B-ACBC-4CBF-AC56-D76348C1946D", "Zone ID. Prefix must be 3 digits + 3 alpha numeric + 3 alpha numeric or  3 digits + 2 alpha numeric + 2 digits.") :
						ResString.GetMultilingualString("D5675853-68E0-4AA0-951F-624F1B025149", "Zone ID. Prefix must be 3 digits + 2 alpha numeric + 2 digits.");
					Parent.SN_ZoneIDPrefixInfo.AddError(message);
				}
				else
				{
					var header = Parent.Header;
					if (header != null)
					{
						var allStmNums = Parent.Factory.Load<OrganisationViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Owner, header.PK));
						PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.SN_ZoneIDPrefixInfo, allStmNums.Where(num => num.PK != Parent.PK));
					}
				}
			}
		}

		bool IsZoneIdValid(ZString zoneIDPrefix, bool useNewFormat)
		{
			var result = false;
			if (!zoneIDPrefix.IsEmpty)
			{
				result = useNewFormat ? ZoneIDRegex9Digits.IsMatch(zoneIDPrefix) : ZoneIDRegex7Digits.IsMatch(zoneIDPrefix);
			}
			return result;
		}

		public void ValidateSN_ClientPrefix()
		{
			ValidateCalculatedProperty(Parent.SN_ClientPrefixInfo);
		}

		protected virtual void CheckSN_ClientPrefix()
		{
			if (Parent.SN_Type == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber)
			{
				if (Parent.SN_ClientPrefix.IsEmpty
					|| Parent.SN_ClientPrefix.Length != OrganisationViewStmNums.Schema.SN_ClientPrefixMaxLength
					|| !ClientRegex.IsMatch(Parent.SN_ClientPrefix))
				{
					var message = ResString.GetMultilingualString("50086CDB-EF7F-46F7-90DC-82E85DDE96D8", "Client Prefix must be 3 alpha numeric.");
					Parent.SN_ClientPrefixInfo.AddError(message);
				}
			}
		}

		Regex ZoneIDRegex7Digits
		{
			get
			{
				if (zoneIDRegex7Digits == null)
				{
					zoneIDRegex7Digits = new Regex("^[0-9]{3}[0-9A-Z]{2}[0-9]{2}$", RegexOptions.Compiled);
				}
				return zoneIDRegex7Digits;
			}
		}
		Regex zoneIDRegex7Digits;

		Regex ZoneIDRegex9Digits
		{
			get
			{
				if (zoneIDRegex9Digits == null)
				{
					zoneIDRegex9Digits = new Regex("^[0-9]{3}[0-9A-Z]{3}[0-9A-Z]{3}$", RegexOptions.Compiled);
				}
				return zoneIDRegex9Digits;
			}
		}
		Regex zoneIDRegex9Digits;

		Regex ClientRegex
		{
			get
			{
				if (clientRegex == null)
				{
					var regexed = new Regex("^[0-9A-Z]{3}$", RegexOptions.Compiled);
					clientRegex = regexed;
				}
				return clientRegex;
			}
		}
		Regex clientRegex;

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSN_ClientPrefix();
			ValidateSN_ZoneIDPrefix();
		}

		#endregion
	}
}
