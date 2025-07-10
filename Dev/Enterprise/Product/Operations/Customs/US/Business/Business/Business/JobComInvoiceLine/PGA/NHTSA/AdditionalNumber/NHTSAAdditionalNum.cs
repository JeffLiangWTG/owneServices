using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSAAdditionalNum : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAAdditionalNumAddInfo>, INHTSAAdditionalNumber
	{
		public NHTSAAdditionalNum(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAAdditionalNumAddInfo>.Schema
		{
			public const string US_NHTAdditionalIdentityNumber = USNHTSAAdditionalNumAddInfoSchema.Constants.US_NHTAdditionalIdentityNumber;
			public const string US_NHTAdditionalIdentityNumQualifier = USNHTSAAdditionalNumAddInfoSchema.Constants.US_NHTAdditionalIdentityNumQualifier;
		}

		#endregion

		#region Related

		public NHTSADetails Details
		{
			get { return Factory.Load<NHTSADetails>(B7_ParentID); }
		}

		public NHTSAHeader Header
		{
			get { return Details != null ? Details.Header : null; }
		}

		#endregion

		#region AddInfo Properties

		public ZString US_NHTAdditionalIdentityNumber
		{
			get { return AddInfo.US_NHTAdditionalIdentityNumber; }
			set { AddInfo.US_NHTAdditionalIdentityNumber = value; }
		}

		public ZPropertyInfo US_NHTAdditionalIdentityNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTAdditionalIdentityNumber, x => AddInfo.US_NHTAdditionalIdentityNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAdditionalNumAddInfoLookups.NumberTypes))]
		public ZString US_NHTAdditionalIdentityNumQualifier
		{
			get { return AddInfo.US_NHTAdditionalIdentityNumQualifier; }
			set { AddInfo.US_NHTAdditionalIdentityNumQualifier = value; }
		}

		public ZPropertyInfo US_NHTAdditionalIdentityNumQualifierInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTAdditionalIdentityNumQualifier, x => AddInfo.US_NHTAdditionalIdentityNumQualifierInfo); }
		}

		#endregion

		#region New Properties

		public ZBool IsVehicleIdentificationNumber
		{
			get { return Header != null && Header.IsMotorVehicles && US_NHTAdditionalIdentityNumQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN; }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSAAdditionalNumber"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (NHTSAAdditionalNum)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USNHTSAAdditionalNumAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USNHTSAAdditionalNumAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USNHTSAAdditionalNumAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public USNHTSAAdditionalNumAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USNHTSAAdditionalNumAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		#endregion

		#region INHTSAAdditionalNumber Members

		ZString INHTSAAdditionalNumber.NumberType
		{
			get
			{
				var numberType = US_NHTAdditionalIdentityNumQualifier == ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN ? (ZString)"AKG" : US_NHTAdditionalIdentityNumQualifier;
				return !US_NHTAdditionalIdentityNumber.IsEmpty ? numberType : ZString.Empty;
			}
		}

		ZString INHTSAAdditionalNumber.Number
		{
			get { return US_NHTAdditionalIdentityNumber; }
		}

		#endregion
	}
}
