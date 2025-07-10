using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentAddressConfig : AutoJobDeclarationDocumentAddressConfig, ISequenceNumberHeader
	{
		public JobDeclarationDocumentAddressConfig(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
			Declarations = new List<JobDeclaration>();
		}

		const string enableCustomizeSectionBodyRowDocumentID = $"Customs : {JobDeclarationDocumentSupporter.DocumentName.ImportCustomsDeclarationProof} : Declaration Documents : CNE";

		internal JobDeclaration Declaration { get; }

		public List<JobDeclaration> Declarations { get; }

		public void CopyConfigParams(JobDeclarationDocumentAddressConfig configToCopy)
		{
			DocumentName = configToCopy.DocumentName;
			HideEXPExporterTradChineseAddr = configToCopy.HideEXPExporterTradChineseAddr;
			HideEXPExporterEnglishAddr = configToCopy.HideEXPExporterEnglishAddr;
			HideEXPBuyerTradChineseAddr = configToCopy.HideEXPBuyerTradChineseAddr;
			HideEXPBuyerEnglishAddr = configToCopy.HideEXPBuyerEnglishAddr;
			HideIMPImporterTradChineseAddr = configToCopy.HideIMPImporterTradChineseAddr;
			HideIMPImporterEnglishAddr = configToCopy.HideIMPImporterEnglishAddr;
			HideIMPSellerTradChineseAddr = configToCopy.HideIMPSellerTradChineseAddr;
			CustomizeSectionBodyRow = configToCopy.CustomizeSectionBodyRow;
			HideCustomizeSectionBodyRow = configToCopy.HideCustomizeSectionBodyRow;

			GoodsDescriptionConfigs.RemoveAndDeleteAll();
			GoodsDescriptionConfigs.AddRange(configToCopy.GoodsDescriptionConfigs);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AddressConfigPK = ZGuid.NewZGuid();
		}

		[ReadOnly(true)]
		public override ZString DocumentName { get => base.DocumentName; set => base.DocumentName = value; }

		[ChildEditable(true)]
		public JobDeclarationDocumentGoodsDescriptionConfigCollection GoodsDescriptionConfigs
		{
			get
			{
				if (goodsDescriptionConfigs == null)
				{
					goodsDescriptionConfigs = new JobDeclarationDocumentGoodsDescriptionConfigCollection(this);
					RegisterEditableChildObject(goodsDescriptionConfigs);
				}
				return goodsDescriptionConfigs;
			}
		}

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(GoodsDescriptionConfigs);

		ShortSequenceNumberGenerator positionNumberGenerator;

		internal ShortSequenceNumberGenerator PositionNumberGenerator => positionNumberGenerator ??= new ShortSequenceNumberGenerator(this);

		#endregion

		JobDeclarationDocumentGoodsDescriptionConfigCollection goodsDescriptionConfigs;

		public void SetDefaultJobDeclarationDocumentOptions()
		{
			using (SuspendSettingHasChanges())
			{
				var goodsDescriptionConfigs = GoodsDescriptionConfigs;
				goodsDescriptionConfigs.RemoveAndDeleteAll();
				List<(ZString caption, ZString field)> goodsDescriptionConfigDefaultDatas;
				var supplierHeader = Declaration.SupplierDocumentaryAddress.Address?.Header;
				var importerHeader = Declaration.ImporterDocumentaryAddress.Address?.Header;
				var supplierOrgImpAddInfo = TWOrgImpAddInfo.Get(supplierHeader);
				var importerOrgImpAddInfo = TWOrgImpAddInfo.Get(importerHeader);

				var documentCurrentCommand = Declaration.DocumentSupporter.CurrentCommand;
				DocumentName = documentCurrentCommand?.SU_MenuName ?? ZString.Empty;
				if ((documentCurrentCommand?.DocumentId ?? ZString.Empty) == enableCustomizeSectionBodyRowDocumentID)
				{
					HideCustomizeSectionBodyRow = ZBool.False;
				}
				else
				{
					HideCustomizeSectionBodyRow = ZBool.True;
					CustomizeSectionBodyRow = ZString.Empty;
				}

				if (Declaration.IsExport)
				{
					if (supplierOrgImpAddInfo != null)
					{
						HideEXPExporterTradChineseAddr = supplierOrgImpAddInfo.ZO_TWHideEXPExporterZHTAddr;
						HideEXPExporterEnglishAddr = supplierOrgImpAddInfo.ZO_TWHideEXPExporterAddr;
					}
					if (importerOrgImpAddInfo != null)
					{
						HideEXPBuyerTradChineseAddr = importerOrgImpAddInfo.ZO_TWHideEXPBuyerZHTAddr;
						HideEXPBuyerEnglishAddr = importerOrgImpAddInfo.ZO_TWHideEXPBuyerAddr;
					}

					if (supplierHeader != null && GetCustomLabels(supplierHeader, OrgConstants.CustomLabelType.OverrideExportDoc) is IOrderedEnumerable<MasterFiles.Business.OrgCustomLabels> customLabels && customLabels.Any())
					{
						goodsDescriptionConfigDefaultDatas = new List<(ZString caption, ZString field)>();
						customLabels.ForEach(c => goodsDescriptionConfigDefaultDatas.Add((c.OT_Caption, c.OT_FieldName)));
					}
					else
					{
						goodsDescriptionConfigDefaultDatas = ExportGoodsDescriptionConfigDefaultData;
					}
				}
				else
				{
					if (importerOrgImpAddInfo != null)
					{
						HideIMPImporterTradChineseAddr = importerOrgImpAddInfo.ZO_TWHideIMPImporterZHTAddr;
						HideIMPImporterEnglishAddr = importerOrgImpAddInfo.ZO_TWHideIMPImporterAddr;
					}
					HideIMPSellerTradChineseAddr = supplierOrgImpAddInfo?.ZO_TWHideIMPSellerZHTAddr ?? false;
					if (importerHeader != null && GetCustomLabels(importerHeader, OrgConstants.CustomLabelType.OverrideImportDoc) is IOrderedEnumerable<MasterFiles.Business.OrgCustomLabels> customLabels && customLabels.Any())
					{
						goodsDescriptionConfigDefaultDatas = new List<(ZString caption, ZString field)>();
						customLabels.ForEach(c => goodsDescriptionConfigDefaultDatas.Add((c.OT_Caption, c.OT_FieldName)));
					}
					else
					{
						goodsDescriptionConfigDefaultDatas = ImportGoodsDescriptionConfigDefaultData;
					}
				}

				foreach (var goodsDescriptionConfigDefaultData in goodsDescriptionConfigDefaultDatas)
				{
					var goodsDescriptionConfig = goodsDescriptionConfigs.AddNew();
					goodsDescriptionConfig.Caption = goodsDescriptionConfigDefaultData.caption;
					goodsDescriptionConfig.Field = goodsDescriptionConfigDefaultData.field;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default data")]
		List<(ZString caption, ZString field)> ExportGoodsDescriptionConfigDefaultData
		{
			get
			{
				exportGoodsDescriptionConfigDefaultData ??= new()
				{
					("賣方料號:", ExportDeclarationDocumentFieldList.Codes.SupplierPartNumber),
					("買方料號:", ExportDeclarationDocumentFieldList.Codes.OwnerPartNumber),
					("", ExportDeclarationDocumentFieldList.Codes.GoodsDescription),
					("型號:", ExportDeclarationDocumentFieldList.Codes.Model),
					("規格:", ExportDeclarationDocumentFieldList.Codes.Specification),
					("原進倉報單號碼/項次:", ExportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber),
					("原報單號碼/項次:", ExportDeclarationDocumentFieldList.Codes.PreviousEntryNumber),
					("輸出入許可文件號碼/項次:", ExportDeclarationDocumentFieldList.Codes.Permits),
					("產地證明書號碼/項次:", ExportDeclarationDocumentFieldList.Codes.CertificateOfOrigin),
					("主管機關指定代號:", ExportDeclarationDocumentFieldList.Codes.AssignedNumbers),
					("生產國別:", ExportDeclarationDocumentFieldList.Codes.GoodsOrigin)
				};

				return exportGoodsDescriptionConfigDefaultData;
			}
		}
		List<(ZString caption, ZString field)> exportGoodsDescriptionConfigDefaultData;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default data")]
		List<(ZString caption, ZString field)> ImportGoodsDescriptionConfigDefaultData
		{
			get
			{
				importGoodsDescriptionConfigDefaultData ??= new()
				{
					("買方料號:", ImportDeclarationDocumentFieldList.Codes.OwnerPartNumber),
					("賣方料號:", ImportDeclarationDocumentFieldList.Codes.SupplierPartNumber),
					("", ImportDeclarationDocumentFieldList.Codes.GoodsDescription),
					("商標(牌名):", ImportDeclarationDocumentFieldList.Codes.Brand),
					("型號:", ImportDeclarationDocumentFieldList.Codes.Model),
					("規格:", ImportDeclarationDocumentFieldList.Codes.Specification),
					("原進倉報單號碼/項次:", ImportDeclarationDocumentFieldList.Codes.PreviousBondedEntryNumber),
					("原報單號碼/項次:", ImportDeclarationDocumentFieldList.Codes.PreviousEntryNumber),
					("輸出入許可文件號碼/項次:", ImportDeclarationDocumentFieldList.Codes.Permits),
					("產地證明書號碼/項次:", ImportDeclarationDocumentFieldList.Codes.CertificateOfOrigin),
					("戰略性高科技貨品國際進口證明號碼:", ImportDeclarationDocumentFieldList.Codes.SHTCImportPermit),
					("華盛頓公約進口許可證號碼:", ImportDeclarationDocumentFieldList.Codes.CITESImportPermit),
					("主管機關指定代號:", ImportDeclarationDocumentFieldList.Codes.AssignedNumbers)
				};

				return importGoodsDescriptionConfigDefaultData;
			}
		}
		List<(ZString caption, ZString field)> importGoodsDescriptionConfigDefaultData;

		IOrderedEnumerable<MasterFiles.Business.OrgCustomLabels> GetCustomLabels(OrgHeader orgHeader, ZString otType) => orgHeader.CustomLabels.Cast<MasterFiles.Business.OrgCustomLabels>().Where(c => c.OT_Type == otType).OrderBy(x => x.OT_Position);

		[List(nameof(CustomizeSectionBodyRowList))]
		public override ZString CustomizeSectionBodyRow { get => base.CustomizeSectionBodyRow; set => base.CustomizeSectionBodyRow = value; }

		public CodeDescriptionPairList CustomizeSectionBodyRowList => Factory.GetCachedValue("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|Empty", "Empty"));
			list.AddPair("4", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|4", "4"));
			list.AddPair("5", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|5", "5"));
			list.AddPair("6", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|6", "6"));
			list.AddPair("7", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|7", "7"));
			list.AddPair("8", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|8", "8"));
			list.AddPair("9", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|9", "9"));
			list.AddPair("10", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|10", "10"));
			list.AddPair("11", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|11", "11"));
			list.AddPair("12", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|12", "12"));
			list.AddPair("13", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|13", "13"));
			list.AddPair("14", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|14", "14"));
			list.AddPair("15", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|15", "15"));
			list.AddPair("16", ResString.GetMultilingualString("Enterprise.Customs.TW.Business.JobDeclarationDocumentAddressConfig|CustomizeSectionBodyRowList|16", "16"));
			return list;
		});
	}
}
