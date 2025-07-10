using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class XmlWriterHelperTest
	{
		[Test]
		public void TestWriteXml()
		{
			var dataSource = "Japan Export Approval Certificate Type";
			var publicationDate = new DateTime(2021, 1, 4);
			var updateType = UpdateType.Full;
			var fileNameWithoutExtension = "FileNameWithoutExtension";

			var refCusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "JPEAC");
			refCusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, StaticResources.DefaultZZD_StartDate);
			refCusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, StaticResources.DefaultZZD_EndDate);
			refCusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "JP");
			refCusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var xmlWriterConfig = new XmlWriterConfiguration();
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusCodeListConfig);

			var refCusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			refCusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			refCusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, false);
			xmlWriterConfig.IncludeEntityTypeConfiguration(refCusCodeListAttributeConfig);

			XmlWriterHelper helper = new XmlWriterHelper(xmlWriterConfig, dataSource, publicationDate, updateType, fileNameWithoutExtension);

			List<RefCusCodeList> refCusCodeLists = new List<RefCusCodeList>()
			{
				new RefCusCodeList()
				{
				ZZD_Code = "ADNO",
				ZZD_Description = "覚醒剤原料輸出許可書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "覚醒剤取締法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "AEOH",
				ZZD_Description = "特定委託輸出申告包括申出受理番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "特定委託輸出申告（包括）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "AEOM",
				ZZD_Description = "認定製造者承認番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "特定製造貨物輸出申告"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "AEOU",
				ZZD_Description = "特定保税運送者の利用者コード",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "特定委託輸出申告（個別）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "AMNO",
				ZZD_Description = "アルコール売渡証番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "アルコール事業法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ANNO",
				ZZD_Description = "輸出検疫証明書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "家畜伝染病予防法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "CANO",
				ZZD_Description = "輸出許可書番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "大麻取締法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "CPNO",
				ZZD_Description = "輸出許可書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "文化財保護法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "CTNO",
				ZZD_Description = "コンテナ番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "コンテナ番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "EDNO",
				ZZD_Description = "輸出申告番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "１インボイスで複数申告になる場合の他申告番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "EINO",
				ZZD_Description = "輸出引取承認書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "輸出入取引法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ELNJ",
				ZZD_Description = "輸出承認証番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "輸出承認証番号（外為法関連機能を利用する場合）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ELNO",
				ZZD_Description = "輸出承認証番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "輸出承認証番号（外為法関連機能を利用しない場合）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "FENJ",
				ZZD_Description = "外国為替及び外国貿易法第48条第１項許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "外国為替及び外国貿易法関係（外為法関連機能を利用する場合）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "FENO",
				ZZD_Description = "外国為替及び外国貿易法第48条第１項許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "外国為替及び外国貿易法関係（外為法関連機能を利用しない場合）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "FONO",
				ZZD_Description = "関係番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "林業種苗法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "FTNO",
				ZZD_Description = "外為令第６条、第８条又は第17条第２項許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "外国為替及び外国貿易法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "HFNN",
				ZZD_Description = "本船・ふ中扱い承認申請番号（システム）",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "システムによる本船・ふ中扱い承認申請"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "HFNO",
				ZZD_Description = "本船・ふ中扱い承認申請番号（マニュアル）",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "マニュアルによる本船・ふ中扱い承認申請"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "HUNO",
				ZZD_Description = "適法捕獲等証明書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "鳥獣の保護及び管理並びに狩猟の適正化に関する法律関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "IANO",
				ZZD_Description = "総保入承認申請番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "総保入承認申請番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "IMNO",
				ZZD_Description = "移入承認申請番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "保税工場からの積戻し"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "INVN",
				ZZD_Description = "複数インボイスに係る他のインボイス番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "インボイス番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "IPNO",
				ZZD_Description = "戻税貨物における輸入許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "輸入申告番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ISNO",
				ZZD_Description = "蔵入承認申請番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "蔵入承認申請番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ITNO",
				ZZD_Description = "展示等申告番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "展示等申告番号展示等積戻し申告"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "IYAK",
				ZZD_Description = "違約品等保税地域搬入番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "定率法第20条関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "JIZN",
				ZZD_Description = "輸出申告前検査申請番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "輸出申告前検査申請番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "MOTS",
				ZZD_Description = "輸出自動車情報登録番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "道路運送車両法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "NANO",
				ZZD_Description = "免許証番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "麻薬及び向精神薬取締法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OLNO",
				ZZD_Description = "その他のライセンス番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "その他のライセンス"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OLTN",
				ZZD_Description = "保税運送承認番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "保税運送承認番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OPNO",
				ZZD_Description = "輸出委託証明書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "あへん法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OTHN",
				ZZD_Description = "その他の参考情報",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "その他の参考情報"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OTPL",
				ZZD_Description = "指定地外貨物検査許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "指定地外貨物検査許可番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "OTST",
				ZZD_Description = "保税地域コード",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "保税地域コード"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "PAYL",
				ZZD_Description = "支払手段等の輸出許可証番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "外国為替令関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "PLNO",
				ZZD_Description = "植物検査合格証明書番号等",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "植物防疫法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "PLNT",
				ZZD_Description = "プラント関係",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "プラント関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "PRNO",
				ZZD_Description = "定率法第19条に係る製造証明書番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "定率法第19条に係る製造証明書番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "PTNO",
				ZZD_Description = "再輸入免税貨物のパーツ番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "再輸入免税貨物のパーツ番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "RANO",
				ZZD_Description = "犬の輸出検疫証明書等番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "狂犬病予防法関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "SINO",
				ZZD_Description = "再輸入免税貨物のシリアル番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "再輸入免税貨物のシリアル番号"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "SYAJ",
				ZZD_Description = "車上通関受理番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "車上通関扱い"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "TASY",
				ZZD_Description = "他所蔵置許可申請番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "他所蔵置許可申請"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "WANA",
				ZZD_Description = "CITES許可番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "ワシントン条約関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ZAN8",
				ZZD_Description = "加工組立輸出貨物確認申告書番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "暫定法第８条関係"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "ZKNO",
				ZZD_Description = "在外公館公用品証明書番号",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = new DateTime(1900, 01, 01, 00, 00, 00),
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "少額貨物簡易通関扱いする貨物の輸出申告（外務省から在外公館あてに送付する公用品の場合）"
				}
				}
				},
				new RefCusCodeList()
				{
				ZZD_Code = "TOKG",
				ZZD_Description = "BUNKAZAI",
				ZZD_ZZK_NKCodeType = "JPEAC",
				ZZD_ZZZ_NKDataGrouping = "JP",
				ZZD_StartDate = StaticResources.DefaultZZD_StartDate,
				ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				RefCusCodeListAttributes = new RefCusCodeListAttribute[]
				{
				new RefCusCodeListAttribute()
				{
				ZZE_ZXE_NKName =  "Remarks",
				ZZE_Value = "文化財保護法【非該当】（「古美術品輸出鑑査証明」を提出する場合）"
				}
				}
			}};

			helper.PopulateAndSave(refCusCodeLists);

			var actualFileAsString = File.ReadAllText(Path.Combine(AppConfig.Shared.OutputDirectory, $"{fileNameWithoutExtension}.xml"));
			var expectedFileAsString = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"TestFiles\ExpectedExportApprovalCertificateTypeXml.xml"));
			Assert.That(actualFileAsString, Is.EqualTo(expectedFileAsString));
		}
	}
}
