using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class InBondNumberSetting : NumberSetting
	{
		#region Schema

		public new class Schema : NumberSetting.Schema
		{
			public const string CompanyPK = "CompanyPK";
			public const string BranchPK = "BranchPK";
		}

		#endregion

		public const int ExpirationYear = 3;

		protected InBondNumberSetting(BusinessObjectFactory factory, ZGuid branchPK, ZGuid companyPK)
			: base(factory)
		{
			this.branchPK = branchPK;
			this.companyPK = companyPK;
		}
		public static InBondNumberSetting New(GlbBranch branch)
		{
			return (branch != null) ? new InBondNumberSetting(branch.Factory, branch.PK, branch.GB_GC) : null;
		}

		public static InBondNumberSetting New(GlbCompany company)
		{
			return (company != null) ? new InBondNumberSetting(company.Factory, ZGuid.Empty, company.PK) : null;
		}

		#region Overrides

		public override ZDecimal StartNumber
		{
			get { return CurrentCompanyOrBranchInBondNumberRange != null ? CurrentCompanyOrBranchInBondNumberRange.StartNumber : ZDecimal.Zero; }
		}

		public override ZDecimal LastNumber
		{
			get { return CurrentCompanyOrBranchInBondNumberRange != null ? CurrentCompanyOrBranchInBondNumberRange.LastNumber : ZDecimal.Zero; }
		}

		public override ZDecimal WarningLimitMark
		{
			get { return CurrentCompanyOrBranchInBondNumberRange != null ? CurrentCompanyOrBranchInBondNumberRange.RunOutWarningLimitNumber : ZDecimal.Zero; }
		}

		public override CusEntryNumber GetCusEntryNumberMatching(ZString number)
		{
			return GetCusEntryNumberMatching(number, false);
		}

		public CusEntryNumber GetCusEntryNumberMatching(ZString number, bool matchLatestTime)
		{
			CusEntryNumber result = null;

			if (!number.IsEmpty)
			{
				var cusEntryNums = CusEntryNumber.Load(Factory, CusEntryHeaderMessageTypeList.Codes.InBond, number, Core.Constants.CountryCodes.UnitedStates);
				if (matchLatestTime)
				{
					cusEntryNums = cusEntryNums.Where(x => x.CE_SystemCreateTimeUtc > ZDateTime.UtcNow.AddYears(-ExpirationYear)).ToArray();
				}
				else
				{
					cusEntryNums = cusEntryNums.OrderByDescending(c => c.CE_SystemCreateTimeUtc).ToArray();
				}

				foreach (var entryNumObject in cusEntryNums)
				{
					if (MatchCriteria(entryNumObject))
					{
						result = entryNumObject;
						break;
					}
				}
			}

			return result;
		}

		bool MatchCriteria(CusEntryNumber entryNumObject)
		{
			var result = false;
			var branch = Branch;
			if (entryNumObject.GetCompany() is GlbCompany entryNumCompany && InBondNumberGenerator.IsInBondNumberRangeByCompany(CompanyPK))
			{
				if (entryNumCompany == Company)
				{
					result = true;
				}
			}
			else if (entryNumObject.GetBranch() is GlbBranch entryNumBranch && branch != null)
			{
				if (entryNumBranch == branch)
				{
					result = true;
				}
			}
			else
			{
				if (entryNumObject.CE_ParentTable == CusInBondMoveHeaderSchema.Constants.TableName)
				{
					result = true;
				}
			}
			return result;
		}

		#endregion

		#region Properties

		#region Branch

		[List(nameof(CurrentCompanyBranches))]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				if (BranchPK != value)
				{
					SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
					ReloadInBondNumberDetails();
					ValidateNextNumber();
				}
			}
		}
		ZGuid branchPK;

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(Schema.BranchPK); }
		}

		public GlbBranch Branch
		{
			get { return Factory.Load<GlbBranch>(BranchPK); }
		}

		public GlbBranchCollection CurrentCompanyBranches
		{
			get { return new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, companyPK)); }
		}

		#endregion

		#region Company

		[List(nameof(CurrentCompanys))]
		public ZGuid CompanyPK
		{
			get { return companyPK; }
			set
			{
				if (CompanyPK != value)
				{
					SetNonPersistentPropertyValue(CompanyPKInfo, ref companyPK, value);
					ReloadInBondNumberDetails();
					ValidateNextNumber();
				}
			}
		}
		ZGuid companyPK;

		public ZPropertyInfo CompanyPKInfo
		{
			get { return GetZPropertyInfo(Schema.CompanyPK); }
		}

		public GlbCompany Company
		{
			get { return Factory.Load<GlbCompany>(CompanyPK); }
		}

		public GlbCompanyCollection CurrentCompanys
		{
			get { return new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.PK, CompanyPK)); }
		}

		#endregion

		#endregion

		#region Implementation

		protected override ZString GenerateNumberWithCheckDigit(ZDecimal number) => GetNumberWithCheckDigit(number);

		public ZString GetNumberWithCheckDigit(ZDecimal number)
		{
			ZString result = number.ToString().PadLeft(8, '0');
			result += InBondNumberCheckDigitCalculator.GetCheckDigit(result);
			return result;
		}

		protected override INumberFountainProxy NumberFountain
		{
			get
			{
				INumberFountainProxy result = null;
				if (Branch == null && Company != null && CurrentCompanyOrBranchInBondNumberRange != null && CurrentCompanyOrBranchInBondNumberRange.IsRangeValidForNumberFountain)
				{
					result = Env.NumberFountains.USInBondNumberFountain(Company.PK.ToGuid());
				}
				else if (Branch != null && CurrentCompanyOrBranchInBondNumberRange != null && CurrentCompanyOrBranchInBondNumberRange.IsRangeValidForNumberFountain)
				{
					result = Env.NumberFountains.USInBondNumberFountain(Branch.PK.ToGuid());
				}
				return result;
			}
		}

#if DEBUG
		protected override string NumberFountainNameForTestig => Enterprise.NumberFountain.NumberFountains.USInBondNumber;
		protected override Guid NumberFountainOwnerForTestig
		{
			get
			{
				var result = Guid.Empty;
				if (Branch == null && Company != null && CurrentCompanyOrBranchInBondNumberRange != null && CurrentCompanyOrBranchInBondNumberRange.IsRangeValidForNumberFountain)
				{
					result = Company.PK.ToGuid();
				}
				else if (Branch != null && CurrentCompanyOrBranchInBondNumberRange != null && CurrentCompanyOrBranchInBondNumberRange.IsRangeValidForNumberFountain)
				{
					result = Branch.PK.ToGuid();
				}
				return result;
			}
		}
#endif

		internal protected override long MaximumAllowForNumberFountain
		{
			get { return NumberFountains.USMaximumInBondNumber; }
		}

		protected override void ValidateNextNumberCore()
		{
			var error = GetBranchError();
			if (string.IsNullOrEmpty(error))
			{
				ValidateNextInBondNumber();
			}
			else
			{
				NextNumberInfo.AddError(error);
			}
		}

		void ValidateNextInBondNumber()
		{
			if (!IsNextNumberValid)
			{
				if (LastNumber > MaximumNextNumberRestriction)
				{
					NextNumberInfo.AddError("Because of a restriction in the System, the Next Number must be greater than or equal to " + StartNumber + " and less than or equal to " + MaximumNextNumberRestriction + ".");
				}
				else
				{
					NextNumberInfo.AddError("The next In-Bond Number must be greater than or equal to " + StartNumber + " and less than or equal to " + LastNumber + ".");
				}
			}
		}

		public static string BranchInBondNumberHasReachedLimitWarning(long availableNumbers, string branchCode, string branchName)
		{
			return ResString.GetMultilingualString("E2907D0B-8A0D-4FEA-9599-EC3F16A24DBC", "The In-Bond Number Range from which this Job will be allocated a number is running out.\r\nThere are only {0} numbers remaining.\r\nPlease allocate a new number range to branch: '{1} – {2}'.", availableNumbers, branchCode, branchName);
		}

		public static string CompanyInBondNumberHasReachedLimitWarning(long availableNumbers, string companyCode, string companyName)
		{
			return ResString.GetMultilingualString("FD9C0448-B03D-4ACD-B3B7-AA09793ED07B", "The In-Bond Number Range set up for company ('{1} - {2}'), from which this Job will be allocated a number, is running out.\r\nThere are only {0} numbers remaining.\r\nYou will need to prepare to allocate a new number range.", availableNumbers, companyCode, companyName);
		}

		string GetBranchError()
		{
			var result = "";

			if (Branch == null && !(InBondNumberGenerator.IsInBondNumberRangeByCompany(companyPK)))
			{
				result = "Please select a valid Branch. A Branch is needed for setting the next In-Bond Number.";
			}
			else
			{
				if (CurrentCompanyOrBranchInBondNumberRange == null)
				{
					result = "Cannot use this Company or Branch In-Bond Number Range as it has currently not been setup in the registry '" + RegistryLocation(USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange);
				}
				else
				{
					CurrentCompanyOrBranchInBondNumberRange.RunPreSaveValidation();
					if (CurrentCompanyOrBranchInBondNumberRange.HasErrors || (CurrentCompanyOrBranchInBondNumberRange.StartNumber == 0 && CurrentCompanyOrBranchInBondNumberRange.LastNumber == 0))
					{
						result = "Cannot use this Company or Branch range as the In-Bond Number Range setup in the registry '" + RegistryLocation(USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange) + "' has errors.";
					}
				}
			}

			return result;
		}

		void ReloadInBondNumberDetails()
		{
			currentCompanyOrBranchInBondNumberRangeCached = null;
			NextNumber = CurrentNextNumber;
		}

		InBondNumberRange CurrentCompanyOrBranchInBondNumberRange
		{
			get
			{
				if (currentCompanyOrBranchInBondNumberRangeCached == null)
				{
					currentCompanyOrBranchInBondNumberRangeCached = new CachedProperty<InBondNumberRange>(Factory, delegate
					{
						return CompanyNumberRange ?? (Branch != null ? BranchNumberRange : null);
					});
				}
				return currentCompanyOrBranchInBondNumberRangeCached.Value;
			}
		}
		CachedProperty<InBondNumberRange> currentCompanyOrBranchInBondNumberRangeCached;

		InBondNumberRange CompanyNumberRange
		{
			get
			{
				if (CompanyPK.IsValid)
				{
					var companyRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
					if (companyRange != null && companyRange.StartNumber > 0)
					{
						return companyRange;
					}
				}

				return null;
			}
		}

		InBondNumberRange BranchNumberRange
		{
			get { return USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, BranchPK.ToGuid(), Guid.Empty); }
		}

		#endregion
	}
}
