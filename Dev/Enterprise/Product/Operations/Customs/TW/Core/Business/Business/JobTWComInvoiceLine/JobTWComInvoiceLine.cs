using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public partial class JobTWComInvoiceLine : AutoJobTWComInvoiceLine, Integration.Customs.TW.IJobTWComInvoiceLine, IClusterKeyWorker, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public JobTWComInvoiceLine(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		IDisposable GetNewLinePriceCalculationFieldSettingSupporter(object type) => invoiceLine?.GetNewLinePriceCalculationFieldSettingSupporter(type);

		[List(nameof(Lookups) + "." + nameof(JobTWComInvoiceLineLookups.CustomsUQList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_DocumentaryUQ", Caption = "Documentary Quantity Unit", MediumCaption = "Doc. Qty Unit", ShortCaption = "UQ")]
		public override ZString TWL_DocumentaryUQ
		{
			get => base.TWL_DocumentaryUQ;
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(InvoiceLine?.DocumentaryUnitCalculationFieldSettingType))
				{
					var oldValue = base.TWL_DocumentaryUQ;
					base.TWL_DocumentaryUQ = value;
					if (oldValue != value)
					{
						InvoiceLine.DocumentaryQuantityConverter.CalculateCustomsFactorAndQty();
						CalculateDocumentaryUnitPriceIfChangedFromDocumentaryUnit();
						if (!IsCopying)
						{
							InvoiceLine?.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		[DecimalPlaces(6)]
		[DecimalPrecision(18)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_DocumentaryUnitPrice", Caption = "Documentary Unit Price", ShortCaption = "Doc. Unit Price")]
		public override ZDecimal TWL_DocumentaryUnitPrice
		{
			get => base.TWL_DocumentaryUnitPrice;
			set
			{
				if (value >= 0)
				{
					var oldValue = base.TWL_DocumentaryUnitPrice;
					base.TWL_DocumentaryUnitPrice = value;
					if (!IsCopying && oldValue != value)
					{
						InvoiceLine?.MarkAsNeedingValidation();
					}
				}
			}
		}

		[DecimalPlaces(6)]
		[DecimalPrecision(18)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_DeclarationUnitPrice", Caption = "Declaration Unit Price", ShortCaption = "Decl. Unit Price")]
		public override ZDecimal TWL_DeclarationUnitPrice
		{
			get => base.TWL_DeclarationUnitPrice;
			set
			{
				if (value >= 0)
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(InvoiceLine.DocumentaryQuantityCalculationFieldSettingType))
					{
						var oldValue = base.TWL_DeclarationUnitPrice;
						base.TWL_DeclarationUnitPrice = value;
						if (!IsCopying && oldValue != value)
						{
							InvoiceLine?.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		[DecimalPlaces(4)]
		[DecimalPrecision(16)]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_DocumentaryQty", Caption = "Documentary Quantity", MediumCaption = "Doc. Quantity", ShortCaption = "Doc. Qty")]
		public override ZDecimal TWL_DocumentaryQty
		{
			get => base.TWL_DocumentaryQty;
			set
			{
				if (value >= 0)
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(InvoiceLine.DocumentaryQuantityCalculationFieldSettingType))
					{
						value = ZArchitecture.Core.Utilities.Round(value, 4);
						var oldValue = base.TWL_DocumentaryQty;
						base.TWL_DocumentaryQty = value;
						if (oldValue != value)
						{
							CalculateDocumentaryUnitPriceIfChangedFromDocumentaryQuantity();
							if (!IsCopying)
							{
								InvoiceLine?.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobTWComInvoiceLineLookups.CategoryCodesOfCAAAircraftPartsList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_AircraftPartsCategory", Caption = "Category")]
		public override ZString TWL_AircraftPartsCategory
		{
			get => base.TWL_AircraftPartsCategory;
			set
			{
				var oldValue = base.TWL_AircraftPartsCategory;
				base.TWL_AircraftPartsCategory = value;
				if (!IsCopying && oldValue != value)
				{
					if (!TWL_AircraftPartsCode.IsEmpty)
					{
						TWL_AircraftPartsCode = ZString.Empty;
					}
					InvoiceLine?.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool ReportAircraftPartsIsNeeded => !TWL_AircraftPartsCategory.IsEmpty || !TWL_AircraftPartsCode.IsEmpty || !TWL_AircraftIPC.IsEmpty;

		[ReadOnlyMember(nameof(AircraftPartsCodeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(JobTWComInvoiceLineLookups.CAAAircraftPartsCodesList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.JobTWComInvoiceLine|TWL_AircraftPartsCode", ShortCaption = "Seq.", Caption = "Sequence")]
		public override ZString TWL_AircraftPartsCode
		{
			get => base.TWL_AircraftPartsCode;
			set
			{
				var oldValue = base.TWL_AircraftPartsCode;
				base.TWL_AircraftPartsCode = value;
				if (!IsCopying && oldValue != value)
				{
					InvoiceLine?.MarkAsNeedingValidation();
				}
			}
		}

		public ZBool AircraftPartsCodeReadOnly => TWL_AircraftPartsCategory.IsEmpty;

		public override ZString TWL_AircraftIPC
		{
			get => base.TWL_AircraftIPC;
			set
			{
				var oldValue = base.TWL_AircraftIPC;
				base.TWL_AircraftIPC = value;
				if (!IsCopying && oldValue != value)
				{
					InvoiceLine?.MarkAsNeedingValidation();
				}
			}
		}

		public JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.Load<JobComInvoiceLine>(TWL_JI));
		JobComInvoiceLine invoiceLine;

		#region IClusterKeyWorker Implementation

		public Type ParentBizObjType => typeof(JobComInvoiceLine);
		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)TWL_JIInfo;
		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;
		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)TWL_ClusterKeyInfo;

		#endregion

		#region IAddInfoChildUniqueIndexFailureHandlerSupporter Members
		string IAddInfoChildUniqueIndexFailureHandlerSupporter.UniqueIndexName => JobTWComInvoiceLineSchema.Constants.Indexes.FK_UX__TWL_JI;
		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => InvoiceLine;
		ZString IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditUser => TWL_SystemLastEditUser;
		ZDateTime IAddInfoChildUniqueIndexFailureHandlerSupporter.SystemLastEditTimeUtc => TWL_SystemLastEditTimeUtc;
		#endregion

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AddInfoChildUniqueIndexFailureHandler(this); }
		}

		public void CalculateDocumentaryUnitPriceIfChangedFromDocumentaryQuantity()
		{
			if (InvoiceLine.IsChangedFromDocumentaryQuantity)
			{
				CalculateDocumentaryUnitPriceUsingLinePriceAndDocumentaryQuantity();
			}
		}

		public void CalculateDocumentaryUnitPriceIfChangedFromDocumentaryUnit()
		{
			if (InvoiceLine.IsChangedFromDocumentaryUnit)
			{
				CalculateDocumentaryUnitPriceUsingUnitPriceAndConversionFactor();
			}
		}

		public void CalculateDocumentaryUnitPriceUsingUnitPriceAndConversionFactor()
		{
			var sameQuantityAndUnit = TWL_DocumentaryUQ == InvoiceLine.JI_InvoiceUQ && TWL_DocumentaryQty == InvoiceLine.JI_InvoiceQuantity;
			var conversionFactor = InvoiceLine.UnitConverter.ConversionFactor(InvoiceLine.JI_InvoiceUQ, TWL_DocumentaryUQ);
			TWL_DocumentaryUnitPrice = (sameQuantityAndUnit || conversionFactor <= 0) ? InvoiceLine.JI_EnteredUnitPrice : (ZDecimal)Utilities.Round(InvoiceLine.JI_EnteredUnitPrice / conversionFactor, 6);
		}

		public void CalculateDocumentaryUnitPriceUsingLinePriceAndDocumentaryQuantity()
		{
			var sameQuantityAndUnit = TWL_DocumentaryUQ == InvoiceLine.JI_InvoiceUQ && TWL_DocumentaryQty == InvoiceLine.JI_InvoiceQuantity;
			TWL_DocumentaryUnitPrice = (sameQuantityAndUnit || TWL_DocumentaryQty <= 0) ? InvoiceLine.JI_EnteredUnitPrice : (ZDecimal)Utilities.Round(InvoiceLine.JI_LinePrice / TWL_DocumentaryQty, 6);
		}
	}
}
