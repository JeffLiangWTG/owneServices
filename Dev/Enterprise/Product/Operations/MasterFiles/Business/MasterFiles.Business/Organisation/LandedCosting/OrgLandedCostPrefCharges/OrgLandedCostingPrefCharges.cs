using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgLandedCostingPrefCharges : AutoOrgLandedCostingPrefCharges
	{
		public OrgLandedCostingPrefCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log reference")]
		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = (NoResString)"Charge Group / Code combination";
				logReference += (NoResString)" Charge Code: " + ChargeCodeDesc;
				logReference += " Charge Group: " + ChargeGroupDescription;

				return logReference;
			}
		}

		#endregion

		#region Properties

		#region Overrides

		[List("Lookups.IncoTermChargeGroups")]
		public override ZString O0_ChargeGroup
		{
			get
			{
				return base.O0_ChargeGroup;
			}
			set
			{
				base.O0_ChargeGroup = value;
			}
		}

		[List("Lookups.ChargeCodes")]
		public override ZGuid O0_AC_ChargeCode
		{
			get
			{
				return base.O0_AC_ChargeCode;
			}
			set
			{
				base.O0_AC_ChargeCode = value;
			}
		}

		#endregion

		#region Chage Group Description

		public ZString ChargeGroupDescription
		{
			get { return Lookups.IncoTermChargeGroups.GetDescriptionFromCode(O0_ChargeGroup); }
		}

		public ZPropertyInfo ChargeGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeGroupDescription)); }
		}

		#endregion

		#region Chage Code Description

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007",
			Justification = "Translation is not required")]
		public ZString ChargeCodeDesc
		{
			get { return ChargeCode != null ? ChargeCode.AC_Desc : ZString.Empty; }
		}

		public ZPropertyInfo ChargeCodeDescInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCodeDesc)); }
		}

		#endregion

		[ResourceStringData("Enterprise.MasterFiles.Business|CompanyName", Caption = "Company Name")]
		public ZString CompanyName => Factory.GetValue(ref companyCodeCached, () => ChargeCode?.Company?.GC_Name ?? ZString.Empty);
		CachedProperty<ZString> companyCodeCached;

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (LandedCostingPrefs != null && LandedCostingPrefs.Header != null &&
				!LandedCostingPrefs.Header.SecurityProvider.HasModifyConsigneeLandedCostingSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
