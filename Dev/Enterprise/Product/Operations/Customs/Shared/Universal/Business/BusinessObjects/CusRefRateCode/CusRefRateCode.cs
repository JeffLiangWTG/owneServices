using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(nameof(CR7_RateCode)), DescriptionProperty(nameof(CR7_Description))]
	public class CusRefRateCode : AutoCusRefRateCode
	{
		public CusRefRateCode(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefRateCode|CR7_RateCode", Caption = "Rate Code")]
		public override ZString CR7_RateCode { get => base.CR7_RateCode; set => base.CR7_RateCode = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefRateCode|CR7_RateType", Caption = "Rate Type")]
		[List(nameof(Lookups) + "." + nameof(CusRefRateCodeLookups.RateTypeList))]
		public override ZString CR7_RateType { get => base.CR7_RateType; set => base.CR7_RateType = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefRateCode|CR7_Description", Caption = "Description")]
		public override ZString CR7_Description { get => base.CR7_Description; set => base.CR7_Description = value; }

		[ReadOnly(true)]
		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(CusRefRateCodeLookups.Countries))]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefRateCode|CR7_RN_NKCountryCode", Caption = "Country/Region", ShortCaption = "Ctry./Rgn.")]
		public override ZString CR7_RN_NKCountryCode { get => base.CR7_RN_NKCountryCode; set => base.CR7_RN_NKCountryCode = value; }

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => Res.GetString("26742978-6391-4B23-893E-12083AE46228", "Rate Code {0}", CR7_RateCode);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CR7_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();

			ClearRowNotifications();
			CheckDuplication();
		}

		void CheckDuplication()
		{
			var query = new ZQuery();
			query.AddToFilter(CusRefRateCodeSchema.CR7_RN_NKCountryCode, CR7_RN_NKCountryCode);
			query.AddToFilter(CusRefRateCodeSchema.CR7_RateCode, CR7_RateCode);
			query.AddToFilter(CusRefRateCodeSchema.PK, SQLComparisonOperator.NotEqual, PK);

			var existingItem = Factory.LoadTop1<CusRefRateCode>(query);
			if (existingItem != null)
			{
				AddRowError(Res.GetString("E5E667E4-9006-4FD2-9E58-5442E6ECE525", "The combination of Rate Code and Country/Region should be unique."));
			}
		}
	}
}
