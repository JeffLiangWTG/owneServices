using System.ComponentModel;
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
	public class PermitCusSupporting : CusSupportingInfo, IHugeSequenceNumberLine
	{
		public PermitCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.PermitCusSupporting.CSI_ReferenceNumber", Caption = "Permit No.", FullDescription = "The export/import permit number issued (including pre-allocation) by the controlling agency.")]
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

		[ResourceStringData("Enterprise.Customs.TW.Business.PermitCusSupporting.CSI_LineNo", Caption = "Permit Item Number", ShortCaption = "Line No.", FullDescription = "The line numbers of the permit number issued (including pre-allocation) by the controlling agency.")]
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
					var matchIndex = parent.SortedPermitCusSupportingCollection.FindIndex(a => a.PK == PK);
					if (matchIndex >= 0 && matchIndex < 5)
					{
						var infoArray = new ZPropertyInfo[] { parent.PermitCusSupportingLineNo1Info, parent.PermitCusSupportingLineNo2Info, parent.PermitCusSupportingLineNo3Info, parent.PermitCusSupportingLineNo4Info, parent.PermitCusSupportingLineNo5Info };
						infoArray[matchIndex].RefreshBinding(oldValue);
					}
				}
			}
		}

		public override ZGuid CSI_ParentID
		{
			get => base.CSI_ParentID;
			set
			{
				var oldValue = CSI_ParentID;
				base.CSI_ParentID = value;
				if (oldValue != CSI_ParentID && !CSI_ParentID.IsEmpty && !IsCopying && !SetterSuspender.IsSetterSuspended(Schema.CSI_ItemNumber))
				{
					Parent?.PermitItemNumberGenerator.RecalculateWhenAdded(this);
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.PermitCusSupporting.CSI_ItemNumber", Caption = "Seq #")]
		public override ZInt CSI_ItemNumber
		{
			get => base.CSI_ItemNumber;
			set
			{
				var oldValue = CSI_ItemNumber;

				base.CSI_ItemNumber = value;
				if (oldValue != CSI_ItemNumber && !IsCopying && !SetterSuspender.IsSetterSuspended(Schema.CSI_ItemNumber))
				{
					Parent?.PermitItemNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		public override void Delete()
		{
			var parent = Parent;
			if (parent != null)
			{
				parent.PermitItemNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				parent.RefreshBinding();
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PermitNumber;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new PermitCusSupportingValidation Validation => (PermitCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new PermitCusSupportingValidation(this);
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		#region ISequenceNumberLine

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get => CSI_ItemNumber;
			set => CSI_ItemNumber = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => Parent?.PK ?? ZGuid.Empty;

		#endregion
	}
}
