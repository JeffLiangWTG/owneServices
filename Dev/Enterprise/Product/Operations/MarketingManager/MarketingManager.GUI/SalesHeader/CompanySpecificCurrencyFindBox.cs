using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MarketingManager.GUI
{
	public class CompanySpecificCurrencyFindBoxColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		#region Properties

		[DefaultValue("")]
		[ZColumnBindingMemberType(typeof(GlbCompany))]
		public string CompanyColumnName
		{
			get { return companyColumnName; }
			set { companyColumnName = value; }
		}
		string companyColumnName;

		#endregion

		public override Type ColumnStyleType
		{
			get { return typeof(CompanySpecificCurrencyFindBoxColumnStyle); }
		}
	}

	public class CompanySpecificCurrencyFindBoxColumnStyle : ZCodeFindBoxColumnStyle
	{
		public CompanySpecificCurrencyFindBoxColumnStyle(CompanySpecificCurrencyFindBoxColumnStyleInfo columnInfo)
			: base(() => new CompanySpecificCurrencyFindBox() { CompanyColumnName = columnInfo.CompanyColumnName }, columnInfo)
		{
		}
	}

	public class CompanySpecificCurrencyFindBox : ZGridFindBox
	{
		[DefaultValue("")]
		[ZColumnBindingMemberType(typeof(GlbCompany))]
		public string CompanyColumnName
		{
			get { return companyColumnName; }
			set { companyColumnName = value; }
		}
		string companyColumnName;

		public GlbCompany Company
		{
			get
			{
				var bizObj = (BusinessObject)DataSource ?? (BusinessObject)CurrentItem;
				if (bizObj == null)
				{
					return null;
				}

				var company = (GlbCompany)bizObj[CompanyColumnName];
				return company;
			}
		}

		protected override void ShowEditOrViewForm()
		{
			ShowIncorrectCompanyWarningIfNotMatching();
			base.ShowEditOrViewForm();
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			ShowIncorrectCompanyWarningIfNotMatching();
			base.SelectFromPopupForm(autoSelect);
		}

		void ShowIncorrectCompanyWarningIfNotMatching()
		{
			var company = Company;
			if (company != null && company.PK != GlbCompany.CurrentCompany.PK)
			{
				var message = Res.GetString("6770EE67-1420-403C-B681-58601042B888", "The currency exchange rates shown here are for your currently logged in company. Please keep in mind that the used exchange rates will be those entered in {0} ({1}).",
					company.GC_Name,
					company.GC_Code);

				Globals.Message.ShowWarning(message);
			}
		}
	}
}
