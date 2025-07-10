using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class SGCustomsNumberViewStmNumsCompanyProvider : CustomsNumberViewStmNumsCompanyProvider, ICustomsNumberViewStmNumsGuiProvider
	{
		public SGCustomsNumberViewStmNumsCompanyProvider(BusinessObjectFactory factory, ZGuid ownerPk)
			: base(factory, Core.Constants.CountryCodes.Singapore, ownerPk)
		{
		}

		public new SGCustomsNumberViewStmNumsWrapperCollection CustomsNumberWrappers => (SGCustomsNumberViewStmNumsWrapperCollection)base.CustomsNumberWrappers;

		protected override CustomsNumberViewStmNumsWrapperCollection NewCustomsNumberWrappers()
		{
			return new SGCustomsNumberViewStmNumsWrapperCollection(CustomsNumbers);
		}

		protected override CustomsNumberViewStmNumsSetting GetSettingCore(ZString rangeType)
		{
			return new SGCustomsNumberViewStmNumsSetting(Company, rangeType);
		}

		public Control GetUserControl()
		{
			return new CustomsNumberViewStmNumsUserControl(CustomsNumberWrappers);
		}

		public Form GetEditorForm(CustomsNumberViewStmNumsWrapper wrapper)
		{
			return new CustomsNumberViewStmNumsEditorForm((SGCustomsNumberViewStmNumsWrapper)wrapper);
		}

		protected override Type WrapperType => typeof(SGCustomsNumberViewStmNumsWrapper);

		protected override ZString GetDetailCore(CustomsNumberViewStmNumsWrapper wrapper) => Res.GetString("SGCustomsNumberViewStmNumsWrapper|Detail", "Range Type: {0}, Customs Registration Number: {1}", wrapper.SN_Type, wrapper.SN_FountainName);

		protected override CustomsNumberViewStmNumsLookups GetNewLookups(CustomsNumberViewStmNums stmNums)
		{
			return new SGCustomsNumberViewStmNumsLookups(stmNums);
		}

		protected override CustomsNumberViewStmNumsWrapper CreateWrapperCore(CustomsNumberViewStmNums stmNums)
		{
			var result = new SGCustomsNumberViewStmNumsWrapper(stmNums);
			if (!stmNums.IsInDatabase)
			{
				result.SN_FountainName = Company.GC_CustomsRegistrationNo.Left(CustomsNumberViewStmNums.Schema.SN_FountainNameMaxLength);
			}
			return result;
		}

		protected override void SetupRelatedDataAndNotificationCore()
		{
			base.SetupRelatedDataAndNotificationCore();
			Company.RegisterEditableChildObject(CustomsNumberWrappers);
			Company.GC_CustomsRegistrationNoInfo.AdditionalValidation += ValidateGC_CustomsRegistrationNo;
		}

		protected override void ClearRelatedDataAndNotificationCore()
		{
			Company.UnRegisterEditableChildObject(CustomsNumberWrappers);
			Company.GC_CustomsRegistrationNoInfo.AdditionalValidation -= ValidateGC_CustomsRegistrationNo;
			base.ClearRelatedDataAndNotificationCore();
		}

		void ValidateGC_CustomsRegistrationNo()
		{
			var registrationNumber = Company.GC_CustomsRegistrationNo;
			if (!registrationNumber.IsEmpty && Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore)
			{
				var otherCompaniesPKs = GetOtherCompanyPKsWithSameCustomsRegistrationNumber().ToArray();
				if (otherCompaniesPKs.Any())
				{
					if (SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(Company) == null)
					{
						Company.GC_CustomsRegistrationNoInfo.AddWarning(Res.GetString("9b2316cb-140f-496a-9955-24f33f7ef3e4", "An SMN Number Range must be setup for this Company as its Customs Registration Number is shared with other Companies."));
					}
					else
					{
						var noSMNCompanies = GetOtherCompanyCodesWithoutSMN(otherCompaniesPKs).ToArray();
						if (noSMNCompanies.Any())
						{
							Company.GC_CustomsRegistrationNoInfo.AddWarning(Res.GetString("d4f94bfe-f345-46d4-8d7e-165e20d63ed8", "The following Companies are sharing this Customs Registration Number, but does not have an SMN Number Range setup: ({0}). Please record an SMN Number Range for all other Companies which share this Customs Registration Number.", ZString.Join(",", noSMNCompanies)));
						}
					}
				}

				if (CustomsNumberWrappers.Cast<CustomsNumberViewStmNumsWrapper>().Any(x => x.SN_Type == NumberRangeTypeList.Codes.SingaporeMessageNumber && x.SN_FountainName != registrationNumber))
				{
					Company.GC_CustomsRegistrationNoInfo.AddWarning(Res.GetString("f6f339d2-5df1-42ea-ab1d-2c87dc7391df", "There is an SMN Number Range setup that doesn't match this Customs Registration Number"));
				}
			}
		}

		IEnumerable<ZString> GetOtherCompanyCodesWithoutSMN(IEnumerable<ZGuid> companyPKs)
		{
			var registrationNumber = Company.GC_CustomsRegistrationNo;
			var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			companyQuery.AddToFilter(GlbCompanySchema.PK, companyPKs);

			var stmNumsSubQuery = new ZDBOnlySubQuery(typeof(CustomsNumberViewStmNums), ViewStmNumsSchema.SN_Owner, true);
			stmNumsSubQuery.AddToFilter(ViewStmNumsSchema.SN_Type, NumberRangeTypeList.Codes.SingaporeMessageNumber);
			stmNumsSubQuery.AddToFilter(ViewStmNumsSchema.SN_Prefix, SQLComparisonOperator.StartsWith, registrationNumber + CustomsNumberViewStmNums.Schema.SequenceSeparator);
			companyQuery.AddSubQuery(stmNumsSubQuery, JoinCondition.And);

			return Factory.Load<GlbCompany>(companyQuery).Select(x => new ZString(x.GC_Code + " - " + x.GC_Name));
		}

		IEnumerable<ZGuid> GetOtherCompanyPKsWithSameCustomsRegistrationNumber()
		{
			var registrationNumber = Company.GC_CustomsRegistrationNo;
			var companyQuery = new ZDBOnlyQuery(typeof(GlbCompany));
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
			companyQuery.AddToFilter(GlbCompanySchema.GC_CustomsRegistrationNo, registrationNumber);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Company.GC_RN_NKCountryCode);
			companyQuery.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Company.PK);

			return Factory.Load<GlbCompany>(companyQuery).Select(x => x.PK);
		}
	}
}
