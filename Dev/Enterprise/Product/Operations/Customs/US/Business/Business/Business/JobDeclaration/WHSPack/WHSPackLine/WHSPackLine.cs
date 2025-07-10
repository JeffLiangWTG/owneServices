using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	public class WHSPackLine : AutoWHSPackLine
	{
		public WHSPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoWHSPackLine.Schema
		{
			public const string B7_Calc_PartNo = "B7_Calc_PartNo";
			public const string B7_Calc_NoOfPackages = "B7_Calc_NoOfPackages";
			public const string B7_Calc_QtyInSinglePackage = "B7_Calc_QtyInSinglePackage";
		}

		public new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public bool IsInwardBondedWarehousingEnabled
		{
			get
			{
				var declaration = Parent;
				return declaration != null && declaration.IsInwardBondedWarehousingEnabled;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|B7_Calc_PartNo", Caption = "Product Code")]
		public ZString B7_Calc_PartNo
		{
			get
			{
				if (!b7_Calc_PartNoCached.HasValue)
				{
					var invoiceLine = InvoiceLine;
					b7_Calc_PartNoCached = invoiceLine == null ? ZString.Empty : invoiceLine.JI_PartNo;
				}
				return b7_Calc_PartNoCached.Value;
			}
		}
		ZString? b7_Calc_PartNoCached;

		public ZPropertyInfo B7_Calc_PartNoInfo
		{
			get { return GetZPropertyInfo(Schema.B7_Calc_PartNo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|US_JI_InvoiceLine", Caption = "Invoice Line Reference", ShortCaption = "Inv. Line Ref.")]
		public override ZGuid US_JI_InvoiceLine
		{
			get { return base.US_JI_InvoiceLine; }
			set
			{
				var oldInvoiceLine = InvoiceLine;
				var oldPartNo = B7_Calc_PartNo;
				base.US_JI_InvoiceLine = value;
				if (!IsCopying)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != oldInvoiceLine)
					{
						if (oldInvoiceLine != null)
						{
							oldInvoiceLine.RefreshWHSPackLines();
						}
						if (invoiceLine != null)
						{
							invoiceLine.RefreshWHSPackLines();
							if (US_PackedQty.IsEmpty && invoiceLine.JI_InvoiceQuantity > 0)
							{
								US_PackedQty = Math.Max(invoiceLine.JI_InvoiceQuantity - invoiceLine.JI_Calc_AllocatedQty, ZDecimal.Zero);
							}
						}
						b7_Calc_PartNoCached = null;
						B7_Calc_PartNoInfo.RefreshBinding(oldPartNo);
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|US_B7_WHSPack", Caption = "Package Reference", ShortCaption = "Pkg. Ref.")]
		public override ZGuid US_B7_WHSPack
		{
			get { return base.US_B7_WHSPack; }
			set { base.US_B7_WHSPack = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|US_PackedQty", Caption = "Allocated on Pack Lines Qty", ShortCaption = "Allocated Qty")]
		public override ZDecimal US_PackedQty
		{
			get { return base.US_PackedQty; }
			set { base.US_PackedQty = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|B7_Calc_NoOfPackages", Caption = "Number Of Packages", ShortCaption = "No. Of Pkgs.")]
		public ZInt B7_Calc_NoOfPackages
		{
			get
			{
				var whsPack = WHSPack;
				return whsPack == null ? ZInt.Zero : whsPack.US_PackageQty;
			}
		}

		public ZPropertyInfo B7_Calc_NoOfPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.B7_Calc_NoOfPackages); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.WHSPackLine|B7_Calc_QtyInSinglePackage", Caption = "Qty In Single Package", ShortCaption = "Qty In Single Pkg")]
		public ZDecimal B7_Calc_QtyInSinglePackage
		{
			get
			{
				var result = ZDecimal.Zero;
				var noOfPackages = B7_Calc_NoOfPackages;
				if (noOfPackages != ZInt.Zero)
				{
					result = US_PackedQty / noOfPackages;
				}
				return result;
			}
		}

		public ZPropertyInfo B7_Calc_QtyInSinglePackageInfo
		{
			get { return GetZPropertyInfo(Schema.B7_Calc_QtyInSinglePackage); }
		}

		public override void Delete()
		{
			var invoiceLine = InvoiceLine;
			var whsPack = WHSPack;
			base.Delete();
			if (invoiceLine != null)
			{
				invoiceLine.RefreshWHSPackLines();
			}
			if (whsPack != null)
			{
				whsPack.RefreshWHSPackLines();
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "WHS Pack Line Details"; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();
			return base.CloneInternal(args);
		}

		protected void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}
	}
}
