using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class ProductPermitCusSupporting : CusSupportingInfo
	{
		public ProductPermitCusSupporting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.ProductPermitCusSupporting.CSI_ReferenceNumber", Caption = "Permit Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set
			{
				var oldValue = CSI_ReferenceNumber;
				base.CSI_ReferenceNumber = value;
				var parent = Parent;
				if (parent != null && value != oldValue && !IsCopying && !((ISupportDataImporting)parent).IsImportingData)
				{
					parent.RefreshBinding();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.ProductPermitCusSupporting.CSI_LineNo", Caption = "Permit Item Number", ShortCaption = "Line No.")]
		public override ZInt CSI_LineNo
		{
			get => base.CSI_LineNo;
			set
			{
				var parent = Parent;
				var oldValue = CSI_LineNo;
				base.CSI_LineNo = value;
				if (parent != null && value != oldValue && !IsCopying && !((ISupportDataImporting)parent).IsImportingData)
				{
					var matchIndex = parent.SortedProductPermitCusSupportingCollection.FindIndex(a => a.PK == PK);
					if (matchIndex >= 0 && matchIndex < 5)
					{
						var infoArray = new ZPropertyInfo[] { parent.PermitCusSupportingLineNo1Info, parent.PermitCusSupportingLineNo2Info, parent.PermitCusSupportingLineNo3Info, parent.PermitCusSupportingLineNo4Info, parent.PermitCusSupportingLineNo5Info };
						infoArray[matchIndex].RefreshBinding(oldValue);
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PermitNumber;
			CSI_ParentTableCode = CusClassPartPivotSchema.Constants.Prefix;
		}

		public new ProductPermitCusSupportingValidation Validation => (ProductPermitCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new ProductPermitCusSupportingValidation(this);
		}

		public new CusClassPartPivot Parent => base.Parent as CusClassPartPivot;
	}
}
