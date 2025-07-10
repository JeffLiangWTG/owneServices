using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(Schema.CR8_Preference), DescriptionProperty(Schema.CR8_Description)]
	public class CusRefPreference : AutoCusRefPreference
	{
		#region Schema

		public new abstract class Schema : AutoCusRefPreference.Schema
		{
			public new const int CR8_PreferenceMaxLength = 8;
		}

		#endregion

		#region Properties
		public CusRefPreference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefPreference|CR8_RN_NKCountryCode", Caption = "Country/Region Code", ShortCaption = "Ctry./Rgn.")]
		public override ZString CR8_RN_NKCountryCode
		{
			get => base.CR8_RN_NKCountryCode;
			set => base.CR8_RN_NKCountryCode = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.CusRefPreference|CR8_Preference", Caption = "Preference")]
		[MaxLength(Schema.CR8_PreferenceMaxLength)]
		public override ZString CR8_Preference
		{
			get => base.CR8_Preference;
			set => base.CR8_Preference = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.CusRefPreference|CR8_Description", Caption = "Description")]
		public override ZString CR8_Description
		{
			get => base.CR8_Description;
			set => base.CR8_Description = value;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CR8_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CR8_Preference = ZString.Empty;
		}
	}
}
