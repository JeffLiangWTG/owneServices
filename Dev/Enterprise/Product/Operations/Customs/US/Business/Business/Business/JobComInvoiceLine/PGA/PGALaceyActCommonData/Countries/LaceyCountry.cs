using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class LaceyCountry : Customs.Business.MultiLineAddInfos.CusAddInfo<LaceyCountryAddInfo>, ILaceyCountry
	{
		public LaceyCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<LaceyCountryAddInfo>.Schema
		{
			public const string US_CountryCode = USCountriesAddInfoSchema.Constants.US_CountryCode;
		}

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USCountriesAddInfoLookups.USCountryList))]
		public ZString US_CountryCode
		{
			get { return AddInfo.US_CountryCode; }
			set { AddInfo.US_CountryCode = value; }
		}

		public ZPropertyInfo US_CountryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CountryCode, x => AddInfo.US_CountryCodeInfo); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "Lacey Country"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (LaceyCountry)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USCountriesAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USCountriesAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		LaceyCountryAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new LaceyCountryAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		LaceyCountryAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region ILaceyCountry Members

		ZString ILaceyCountry.CountryCode
		{
			get { return US_CountryCode; }
		}

		#endregion
	}
}
