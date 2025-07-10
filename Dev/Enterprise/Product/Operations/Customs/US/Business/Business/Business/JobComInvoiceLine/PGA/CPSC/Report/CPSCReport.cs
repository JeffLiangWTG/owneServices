using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CPSCReport : CusAddInfo<CPSCReportAddInfo>
	{
		public CPSCReport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<CPSCReportAddInfo>.Schema
		{
			public const string US_RemarksText = USCPSCLabReportAddInfoSchema.Constants.US_RemarksText;
			public const string US_RemarksType = USCPSCLabReportAddInfoSchema.Constants.US_RemarksType;
		}

		#endregion

		#region AddInfo Properties

		public ZString US_RemarksText
		{
			get { return AddInfo.US_RemarksText; }
			set { AddInfo.US_RemarksText = value; }
		}

		public ZPropertyInfo US_RemarksTextInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RemarksText, x => AddInfo.US_RemarksTextInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCLabReportAddInfoLookups.LabReportInformationTypeList))]
		public ZString US_RemarksType
		{
			get { return AddInfo.US_RemarksType; }
			set { AddInfo.US_RemarksType = value; }
		}

		public ZPropertyInfo US_RemarksTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RemarksType, x => AddInfo.US_RemarksTypeInfo); }
		}

		bool AllFieldIsEmpty => GetUsedFieldsInfos().All(x => x.Value.IsEmpty);

		IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return US_RemarksTypeInfo;
			yield return US_RemarksTextInfo;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USCPSCLabReportAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USCPSCLabReportAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		CPSCReportAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CPSCReportAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		CPSCReportAddInfo fAddInfo;

		#endregion

		#region Override

		public override void OnSaving()
		{
			base.OnSaving();
			if (AllFieldIsEmpty)
			{
				Delete();
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (CPSCReport)base.CloneInternal(args);
			return result;
		}

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion
	}
}
