using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PesticideLine : Customs.Business.MultiLineAddInfos.CusAddInfo<USPSTLineAddInfo>, IPSTLine, Integration.Customs.US.IPesticideLine
	{
		public PesticideLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USPSTLineAddInfo>.Schema
		{
			public const string US_LPCOType = USPSTLineAddInfoSchema.Constants.US_LPCOType;
			public const string US_LPCONumber = USPSTLineAddInfoSchema.Constants.US_LPCONumber;
			public const string US_NameOfActiveIngredient = USPSTLineAddInfoSchema.Constants.US_NameOfActiveIngredient;
			public const string US_ActiveIngredientPercentage = USPSTLineAddInfoSchema.Constants.US_ActiveIngredientPercentage;
		}

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USPSTLineAddInfoLookups.ProductQualifiers))]
		public ZString US_LPCOType
		{
			get { return AddInfo.US_LPCOType; }
			set { AddInfo.US_LPCOType = value; }
		}

		public ZPropertyInfo US_LPCOTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LPCOType, x => AddInfo.US_LPCOTypeInfo); }
		}

		public ZString US_LPCONumber
		{
			get { return AddInfo.US_LPCONumber; }
			set { AddInfo.US_LPCONumber = value; }
		}

		public ZPropertyInfo US_LPCONumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LPCONumber, x => AddInfo.US_LPCONumberInfo); }
		}

		public ZString US_NameOfActiveIngredient
		{
			get { return AddInfo.US_NameOfActiveIngredient; }
			set { AddInfo.US_NameOfActiveIngredient = value; }
		}

		public ZPropertyInfo US_NameOfActiveIngredientInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NameOfActiveIngredient, x => AddInfo.US_NameOfActiveIngredientInfo); }
		}

		public ZDecimal US_ActiveIngredientPercentage
		{
			get { return AddInfo.US_ActiveIngredientPercentage; }
			set { AddInfo.US_ActiveIngredientPercentage = value; }
		}

		public ZPropertyInfo US_ActiveIngredientPercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ActiveIngredientPercentage, x => AddInfo.US_ActiveIngredientPercentageInfo); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "PesticideLine"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (PesticideLine)base.CloneInternal(args);
			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USPSTLineAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USPSTLineAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USPSTLineAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USPSTLineAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USPSTLineAddInfo fAddInfo;

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

		Integration.Customs.US.IPesticideLineAddInfo Integration.Customs.US.IPesticideLine.AddInfo
		{
			get { return AddInfo; }
		}
		#endregion

		#region IPSTDetail Members

		ZString IPSTLine.LPCOType
		{
			get { return US_LPCOType; }
		}

		ZString IPSTLine.LPCONumber
		{
			get { return US_LPCONumber; }
		}

		ZString IPSTLine.NameOfActiveIngredient
		{
			get { return US_NameOfActiveIngredient; }
		}

		ZDecimal IPSTLine.ActiveIngredientPercentage
		{
			get { return US_ActiveIngredientPercentage; }
		}

		#endregion
	}
}
