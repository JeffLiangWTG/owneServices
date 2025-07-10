using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.Customs.US.ISF.Business
{
	[DependentBusinessObject(typeof(CusISFHeader), "Lines")]
	[UserDefinedValues]
	[SingleObjectAroundARow]
	public class CusISFLine : AutoCusISFLine, IHaveAdditionalDataForBorderWise, ICusISFLine
	{
		public CusISFLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusISFLine.Schema
		{
			public const string CustomAttribute1 = "CustomAttribute1";
			public const string CustomAttribute2 = "CustomAttribute2";
			public const string BL_FormattedHarmonisedNum = "BL_FormattedHarmonisedNum";
			public const string BL_ManufacturerShortAddress = "BL_ManufacturerShortAddress";

			public const int BL_FormattedHarmonisedNumMaxLength = BL_HarmonisedNumMaxLength + 2;
		}

		#region Related Business Objects

		public CusISFHeader Header
		{
			get { return Factory.Load<CusISFHeader>(BL_BF); }
		}

		public ISFDocAddress ManufacturerDocAddress
		{
			get { return Factory.Load<ISFDocAddress>(BL_ManufacturerDocAddressPK); }
		}

		IUSISFDocAddress ICusISFLine.ManufacturerDocAddress => ManufacturerDocAddress;

		#endregion

		#region Properties

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.SupplierParts))]
		[ReadOnlyMember(nameof(BL_TextProductCode_ReadOnly))]
		public override ZString BL_TextProductCode
		{
			get
			{
				var parentTariffLine = ParentTariffLine;
				return parentTariffLine != null ? parentTariffLine.BL_TextProductCode : base.BL_TextProductCode;
			}
			set
			{
				if (BL_TextProductCode_CanBeSetByCustomer)
				{
					using (GetValidationSuspender())
					{
						bool reloadNeeded = BL_TextProductCode != value;
						base.BL_TextProductCode = value;
						if (reloadNeeded && !IsCopying)
						{
							if (BL_LineType.IsEmpty)
							{
								PartSyncManager.Refresh();
							}

							if (BL_TextProductCode.IsEmpty)
							{
								BL_PartAttrib1 = ZString.Empty;
								BL_PartAttrib2 = ZString.Empty;
								BL_PartAttrib3 = ZString.Empty;
							}
						}
					}
					if (!IsValidationSuspended && !IsCopying)
					{
						Validation.ValidateBL_TextProductCode();
					}
				}
			}
		}

		bool BL_TextProductCode_ReadOnly
		{
			get { return !BL_TextProductCode_CanBeSetByCustomer; }
		}

		public bool BL_TextProductCode_CanBeSetByCustomer
		{
			get { return BL_BL_Parent.IsEmpty; }
		}

		public override ZGuid BL_BF
		{
			get { return base.BL_BF; }
			set
			{
				ZGuid oldValue = BL_BF;
				base.BL_BF = value;
				if (!IsCopying && oldValue != BL_BF)
				{
					PartSyncManager.Refresh();
				}
			}
		}

		public override ZGuid BL_OP
		{
			get { return base.BL_OP; }
			set
			{
				ZGuid oldValue = BL_OP;
				base.BL_OP = value;
				if (!IsCopying && oldValue != BL_OP)
				{
					PartSyncManager.ReloadPart = true;
					UpdateProductRelatedData();
				}
			}
		}

		public US.Business.OrgSupplierPart USSupplierPart
		{
			get { return (US.Business.OrgSupplierPart)SupplierPart; }
		}

		public sealed override MasterFiles.Business.OrgSupplierPart SupplierPart
		{
			get
			{
				US.Business.OrgSupplierPart result = null;
				if (fPartSyncManager == null)
				{
					CargoWise.Common.ErrorReporter.ReportOnce("CusISFLine's PartSyncManager", "PartSyncManager has not been initialised");
				}
				else
				{
					if (PartSyncManager.Part != null && !PartSyncManager.Part.IsDeleted)
					{
						result = PartSyncManager.Part;
					}
				}
				return result;
			}
		}

		public CusClassPartPivot Pivot
		{
			get
			{
				if (cachedPivot == null)
				{
					cachedPivot = new CachedProperty<CusClassPartPivot>(Factory, GetPivot);
				}

				return cachedPivot.Value;
			}
		}
		CachedProperty<CusClassPartPivot> cachedPivot;

		CusClassPartPivot GetPivot()
		{
			CusClassPartPivot result = null;
			var part = USSupplierPart;
			var header = Header;
			if (part != null && header != null)
			{
				var partAttribs = GetPartAttribs();
				var isfDate = ISFDate;
				var importerPK = header.BF_OH_Importer;
				var supplier = SellingParty;
				var supplierPK = supplier != null ? supplier.PK : ZGuid.Empty;
				result = part.PivotsForBinding.GetImportMatch(importerPK, supplierPK, isfDate, partAttribs);
			}
			return result;
		}

		public OrgHeader SellingParty
		{
			get { return Header?.SellingParty?.Organisation; }
		}

		internal ZDate ISFDate
		{
			get
			{
				var result = ZDateTime.Empty;
				var transport = Header == null ? null : Header.FirstUSTransport;
				if (transport != null)
				{
					result = transport.JW_ATA;
					if (result.IsEmpty)
					{
						result = transport.JW_ETA;
					}
				}
				if (result.IsEmpty)
				{
					result = ZDateTime.Today;
				}

				return result.Date;
			}
		}

		Customs.Business.CusAttributeFilter.AttributeValue[] GetPartAttribs()
		{
			var result = new List<Customs.Business.CusAttributeFilter.AttributeValue>();
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT1, Value = BL_PartAttrib1 });
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT2, Value = BL_PartAttrib2 });
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT3, Value = BL_PartAttrib3 });
			return result.ToArray();
		}

		[List(nameof(Header) + "." + nameof(CusISFHeader.ManufacturerAddresses))]
		[RelatedBusinessObject("ManufacturerDocAddress")]
		public override ZGuid BL_ManufacturerDocAddressPK
		{
			get { return base.BL_ManufacturerDocAddressPK; }
			set
			{
				base.BL_ManufacturerDocAddressPK = value;
				SetCountryOfOriginFromManufacturer();
			}
		}

		public void SetCountryOfOriginFromManufacturer()
		{
			if (ManufacturerDocAddress != null && ManufacturerDocAddress.Country != null)
			{
				BL_RN_NKGoodsOrigin = ManufacturerDocAddress.Country.Code;
			}
		}

		public ZString BL_ManufacturerShortAddress
		{
			get
			{
				var manufacturerAddress = ManufacturerDocAddress;
				return manufacturerAddress != null ? manufacturerAddress.E2_ShortAddress : (ZString)"";
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.Tariffs))]
		public override ZString BL_HarmonisedNum
		{
			get { return base.BL_HarmonisedNum; }
			set
			{
				ZString oldFormattedValue = BL_FormattedHarmonisedNum;
				ZString newHarmonisedNum = IsCopying ? value : TariffFormatter.Format(value).Left(BL_HarmonisedNumInfo.MaxLength);
				base.BL_HarmonisedNum = newHarmonisedNum;
				BL_FormattedHarmonisedNumInfo.RefreshBinding(oldFormattedValue);
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.Tariffs))]
		[MaxLength(Schema.BL_FormattedHarmonisedNumMaxLength)]
		public ZString BL_FormattedHarmonisedNum
		{
			get { return TariffFormatter.DisplayFormat(BL_HarmonisedNum); }
			set
			{
				ZString oldValue = BL_FormattedHarmonisedNum;
				BL_HarmonisedNum = TariffFormatter.Format(value);
				BL_FormattedHarmonisedNumInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BL_FormattedHarmonisedNumInfo
		{
			get { return GetZPropertyInfo(Schema.BL_FormattedHarmonisedNum); }
		}

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString CustomAttribute1
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.CustomAttribute1); }
			set
			{
				this.SetUserDefinedValue(Schema.CustomAttribute1, value);
				CustomAttribute1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomAttribute1Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttribute1); }
		}

		[MaxLength(GenAddOnColumn.Schema.XA_DataMaxLength)]
		public ZString CustomAttribute2
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.CustomAttribute2); }
			set
			{
				this.SetUserDefinedValue(Schema.CustomAttribute2, value);
				CustomAttribute2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomAttribute2Info
		{
			get { return GetZPropertyInfo(Schema.CustomAttribute2); }
		}

		public ZString HarmonisedNumToReportToCustoms
		{
			get
			{
				CusISFHeader header = Header;
				return header == null ? BL_HarmonisedNum : header.GetHarmonisedNumAsRequired(BL_HarmonisedNum);
			}
		}

		TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		public ZDate EffectiveDateForDutyRate
		{
			get { return ZDate.Today; }
		}

		public override ZString BL_LineType
		{
			get { return base.BL_LineType; }
			set
			{
				var oldValue = base.BL_LineType;
				if (oldValue != value)
				{
					var parentLine = ParentTariffLine;
					if (parentLine != null)
					{
						parentLine.RefreshChildAndProductRelatedLines();
					}
				}

				base.BL_LineType = value;
			}
		}

		[ReadOnlyMember(nameof(BL_BL_Parent_ReadOnly))]
		public override ZGuid BL_BL_Parent
		{
			get { return base.BL_BL_Parent; }
			set
			{
				bool shouldRefreshParent = base.BL_BL_Parent != value && !IsCopying;

				if (shouldRefreshParent)
				{
					RefereshParentTariffLine();
				}

				base.BL_BL_Parent = value;

				if (shouldRefreshParent)
				{
					RefereshParentTariffLine();
				}
			}
		}

		void RefereshParentTariffLine()
		{
			var parentTariffLine = ParentTariffLine;
			if (parentTariffLine != null)
			{
				parentTariffLine.RefreshChildAndProductRelatedLines();
			}
		}

		bool BL_BL_Parent_ReadOnly
		{
			get { return true; }
		}

		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.PartAttrib1List))]
		[ReadOnlyMember(nameof(BL_TextProductCode_ReadOnly))]
		public override ZString BL_PartAttrib1
		{
			get { return base.BL_PartAttrib1; }
			set
			{
				bool hasChanges = base.BL_PartAttrib1 != value;

				base.BL_PartAttrib1 = value;
				if (hasChanges && !IsCopying && BL_TextProductCode_CanBeSetByCustomer)
				{
					PartSyncManager.Refresh();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.PartAttrib2List))]
		[ReadOnlyMember(nameof(BL_TextProductCode_ReadOnly))]
		public override ZString BL_PartAttrib2
		{
			get { return base.BL_PartAttrib2; }
			set
			{
				bool hasChanges = base.BL_PartAttrib2 != value;

				base.BL_PartAttrib2 = value;
				if (hasChanges && !IsCopying && BL_TextProductCode_CanBeSetByCustomer)
				{
					PartSyncManager.Refresh();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusISFLineLookups.PartAttrib3List))]
		[ReadOnlyMember(nameof(BL_TextProductCode_ReadOnly))]
		public override ZString BL_PartAttrib3
		{
			get { return base.BL_PartAttrib3; }
			set
			{
				bool hasChanges = base.BL_PartAttrib3 != value;

				base.BL_PartAttrib3 = value;
				if (hasChanges && !IsCopying && BL_TextProductCode_CanBeSetByCustomer)
				{
					PartSyncManager.Refresh();
				}
			}
		}

		[ReadOnlyMember(nameof(BL_RN_NKGoodsOrigin_ReadOnly))]
		public override ZString BL_RN_NKGoodsOrigin
		{
			get
			{
				var origin = base.BL_RN_NKGoodsOrigin;
				return origin.IsEmpty && BL_BL_Parent.IsValid ? ParentTariffLine.BL_RN_NKGoodsOrigin : origin;
			}
			set { base.BL_RN_NKGoodsOrigin = value; }
		}

		bool BL_RN_NKGoodsOrigin_ReadOnly
		{
			get { return IsComponentLine; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		public CusISFLinePartSynchronisationManager PartSyncManager
		{
			get { return fPartSyncManager; }
		}
		CusISFLinePartSynchronisationManager fPartSyncManager;

		public override void OnLoaded()
		{
			base.OnLoaded();
			InitialisePartSyncManager();
			RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered();
		}

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			InitialisePartSyncManager();
		}

		void RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered()
		{
			if (!BL_TextProductCode.IsEmpty && BL_OP.IsEmpty && BL_LineType.IsEmpty)
			{
				using (GetValidationSuspender())
				{
					if (BL_HarmonisedNum.IsEmpty)
					{
						PartSyncManager.Refresh();
					}
					else
					{
						PartSyncManager.OnlySetBL_OP();
					}
				}
			}
		}

		void InitialisePartSyncManager()
		{
			if (fPartSyncManager == null)
			{
				fPartSyncManager = new CusISFLinePartSynchronisationManager(this);
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		void UpdateProductRelatedData()
		{
			US.Business.OrgSupplierPart part = USSupplierPart;

			if (part != null)
			{
				BL_TextProductCode = part.OP_PartNum;
			}
		}

		public CusISFLine ParentTariffLine
		{
			get
			{
				if (fParentTariffLine == null || fParentTariffLine.IsDeleted || fParentTariffLine.PK != BL_BL_Parent)
				{
					fParentTariffLine = BL_BL_Parent.IsValid ? Factory.Load<CusISFLine>(BL_BL_Parent) : null;
				}
				return fParentTariffLine;
			}
		}
		CusISFLine fParentTariffLine;

		#region ChildLines

		public bool IsComponentLine
		{
			get { return BL_LineType == ISFLineTypeList.Codes.Component; }
		}

		public IEnumerable<CusISFLine> ChildLines
		{
			get
			{
				if (childLines == null)
				{
					LoadChildAndProductRelatedLines();
				}

				return childLines.Where(line => !line.IsDeleted);
			}
		}
		List<CusISFLine> childLines;

		void LoadChildAndProductRelatedLines()
		{
			childLines = new List<CusISFLine>();
			productRelatedLines = new List<CusISFLine>();
			var header = this.Header;
			if (BL_BL_Parent.IsEmpty && header != null)
			{
				foreach (var line in header.Lines.OfType<CusISFLine>().Where(l => l.BL_BL_Parent == PK))
				{
					switch (line.BL_LineType)
					{
						case ISFLineTypeList.Codes.Component:
							childLines.Add(line);
							break;
						case ISFLineTypeList.Codes.Related:
							productRelatedLines.Add(line);
							break;
					}
				}
			}
		}

		public void RefreshChildAndProductRelatedLines()
		{
			childLines = null;
			productRelatedLines = null;
		}

		internal void DeleteProductLines()
		{
			if (!(Header?.SuspendDeleteProductLines ?? false))
			{
				foreach (var line in ChildLines)
				{
					line.Delete();
				}

				foreach (var line in ProductRelatedLines)
				{
					line.Delete();
				}
				RefreshChildAndProductRelatedLines();
			}
		}

		internal CusISFLine AddChildLine()
		{
			var result = Header.Lines.AddNew();
			result.BL_LineType = ISFLineTypeList.Codes.Component;
			result.BL_BL_Parent = PK;
			return result;
		}

		#endregion

		#region ProductRelatedLines

		public bool IsRelatedLine
		{
			get { return BL_LineType == ISFLineTypeList.Codes.Related; }
		}

		public IEnumerable<CusISFLine> ProductRelatedLines
		{
			get
			{
				if (productRelatedLines == null)
				{
					LoadChildAndProductRelatedLines();
				}

				return productRelatedLines.Where(line => !line.IsDeleted);
			}
		}
		List<CusISFLine> productRelatedLines;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				var parentTariffLine = ParentTariffLine;
				var lineType = BL_LineType;
				DeleteProductLines();
				base.Delete();

				if (parentTariffLine != null && !parentTariffLine.IsDeleted)
				{
					if (lineType == ISFLineTypeList.Codes.Component || lineType == ISFLineTypeList.Codes.Related)
					{
						parentTariffLine.RefreshChildAndProductRelatedLines();
					}
				}
			}
		}

		internal CusISFLine AddProductRelatedLine()
		{
			var result = Header.Lines.AddNew();
			result.BL_LineType = ISFLineTypeList.Codes.Related;
			result.BL_BL_Parent = PK;
			return result;
		}

		#endregion

		#region IHaveAdditionalDataForBorderWise Members

		AdditionalDataForBorderWise IHaveAdditionalDataForBorderWise.GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise("I", EffectiveDateForDutyRate, x => TariffFormatter.DisplayFormat(x));
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.Tariffs.TypeOfElements; }
		}

		#endregion
	}
}
