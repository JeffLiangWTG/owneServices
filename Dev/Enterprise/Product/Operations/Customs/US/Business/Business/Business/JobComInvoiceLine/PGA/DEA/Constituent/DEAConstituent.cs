using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class DEAConstituent : CusAddInfo<DEAConstituentAddInfo>, IDEAConstituent
	{
		public DEAConstituent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<DEAConstituentAddInfo>.Schema
		{
			public const string US_ProductCode = USDEAConstituentAddInfoSchema.Constants.US_ProductCode;
			public const string US_Weight = USDEAConstituentAddInfoSchema.Constants.US_Weight;
			public const string US_WeightUQ = USDEAConstituentAddInfoSchema.Constants.US_WeightUQ;
		}

		#endregion

		#region AddInfo Properties

		public DEAHeader Header
		{
			get { return Factory.Load<DEAHeader>(B7_ParentID); }
		}

		public ZBool IsExport
		{
			get { return Header != null && Header.IsExport; }
		}

		#region US_ProductCode

		[ResourceStringData("Enterprise.Customs.US.Business.DEAConstituent|US_ProductCode", Caption = "Product Code")]
		public ZString US_ProductCode
		{
			get { return AddInfo.US_ProductCode; }
			set { AddInfo.US_ProductCode = value; }
		}

		public ZPropertyInfo US_ProductCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductCode, x => AddInfo.US_ProductCodeInfo); }
		}

		#endregion

		#region US_Weight

		[ResourceStringData("Enterprise.Customs.US.Business.DEAConstituent|US_Weight", Caption = "Weight")]
		[DecimalPlaces(nameof(WeightDecimalPlaces))]
		public ZDecimal US_Weight
		{
			get { return AddInfo.US_Weight; }
			set { AddInfo.US_Weight = value; }
		}

		int WeightDecimalPlaces
		{
			get { return IsExport ? 4 : 2; }
		}

		public ZPropertyInfo US_WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Weight, x => AddInfo.US_WeightInfo); }
		}

		#endregion

		#region US_WeightUQ

		[List(nameof(AddInfoLookups) + "." + nameof(USDEAConstituentAddInfoLookups.WeightUQList))]
		[ResourceStringData("Enterprise.Customs.US.Business.DEAConstituent|US_WeightUQ", Caption = "Weight UQ", ShortCaption = "UQ")]
		public ZString US_WeightUQ
		{
			get { return AddInfo.US_WeightUQ; }
			set { AddInfo.US_WeightUQ = value; }
		}

		public ZPropertyInfo US_WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_WeightUQ, x => AddInfo.US_WeightUQInfo); }
		}

		#endregion

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USDEAConstituentAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USDEAConstituentAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		DEAConstituentAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new DEAConstituentAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}

		DEAConstituentAddInfo fAddInfo;

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region IDEAConstituent

		ZString IDEAConstituent.ProductCode
		{
			get { return US_ProductCode; }
		}

		ZDecimal IDEAConstituent.Weight
		{
			get { return US_Weight > 9999999999.99m ? ZDecimal.Zero : US_Weight; }
		}

		ZString IDEAConstituent.WeightUQ
		{
			get { return US_WeightUQ; }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "DEA Constituent"; }
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (DEAConstituent)base.CloneInternal(args);
			return result;
		}

		#endregion
	}
}
