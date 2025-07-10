using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.CNReferenceData.Services
{
	#region HS -- 税号

	/// <summary>
	/// 税号
	/// </summary>
	public sealed class HSData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// 父级ID
		/// </summary>
		public string PARENT_ID { get; set; }

		/// <summary>
		/// 层级hscode
		/// </summary>
		public string HS_ETCN { get; set; }

		/// <summary>
		/// hscode
		/// </summary>
		public string HS_CODE
		{
			get
			{
				var hscode = MainData?.HS_CODE ?? fHS_CODE;
				return (hscode.Length != 10 && fHS_CODE.Length == 10) ? fHS_CODE : hscode;
			}
			set => fHS_CODE = value;
		}
		string fHS_CODE;

		/// <summary>
		/// 税则书hscode_与税则书同步
		/// </summary>
		public string HS_BOOK
		{
			get => MainData?.HS_BOOK ?? fHS_BOOK;
			set => fHS_BOOK = value;
		}
		string fHS_BOOK;

		/// <summary>
		/// 层级hscode搜索用_报关使用
		/// </summary>
		public string HS_ETCN_SEARC
		{
			get => MainData?.HS_ETCN_SEARCH ?? fHS_ETCN_SEARCH;
			set => fHS_ETCN_SEARCH = value;
		}
		string fHS_ETCN_SEARCH;

		/// <summary>
		/// 计算层级hscode使用
		/// </summary>
		public string HS_STRUCTURE
		{
			get => MainData?.HS_STRUCTURE ?? fHS_STRUCTURE;
			set => fHS_STRUCTURE = value;
		}
		string fHS_STRUCTURE;

		/// <summary>
		/// HS位数
		/// </summary>
		public int DIGIT_MARK
		{
			get => MainData?.DIGIT_MARK ?? fDIGIT_MARK;
			set => fDIGIT_MARK = value;
		}
		int fDIGIT_MARK;

		/// <summary>
		/// 是否显示
		/// </summary>
		public string HS_VISIBLE
		{
			get => MainData?.HS_VISIBLE ?? fHS_VISIBLE;
			set => fHS_VISIBLE = value;
		}
		string fHS_VISIBLE;

		/// <summary>
		/// 商品描述-中文
		/// </summary>
		public string DESCRIPTION_CN
		{
			get => MainData?.DESCRIPTION_CN ?? fDESCRIPTION_CN;
			set => fDESCRIPTION_CN = value;
		}
		string fDESCRIPTION_CN;

		/// <summary>
		/// 商品描述-英文
		/// </summary>
		public string DESCRIPTION_EN
		{
			get => MainData?.DESCRIPTION_EN ?? fDESCRIPTION_EN;
			set => fDESCRIPTION_EN = value;
		}
		string fDESCRIPTION_EN;

		/// <summary>
		/// 第一计量单位
		/// </summary>
		public string UNIT1_CN
		{
			get => MainData?.UNIT1_CN ?? fUNIT1_CN;
			set => fUNIT1_CN = value;
		}
		string fUNIT1_CN;

		/// <summary>
		/// 第二计量单位
		/// </summary>
		public string UNIT2_CN
		{
			get => MainData?.UNIT2_CN ?? fUNIT2_CN;
			set => fUNIT2_CN = value;
		}
		string fUNIT2_CN;

		/// <summary>
		/// 第一计量单位-英文
		/// </summary>
		public string UNIT1_EN
		{
			get => MainData?.UNIT1_EN ?? fUNIT1_EN;
			set => fUNIT1_EN = value;
		}
		string fUNIT1_EN;

		/// <summary>
		/// 第二计量单位-英文
		/// </summary>
		public string UNIT2_EN
		{
			get => MainData?.UNIT2_EN ?? fUNIT2_EN;
			set => fUNIT2_EN = value;
		}
		string fUNIT2_EN;

		/// <summary>
		/// hs状态：0_失效;1_有效;2_即将失效
		/// </summary>
		public int VHS { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VHS_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VHS_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VHS_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		public List<MainData> MAIN_LIST { get; set; }
		public List<YSData> YS_LIST { get; set; }
		public List<DOCData> DOC_LIST { get; set; }
		public List<SCData> SC_LIST { get; set; }
		public List<SJData> SJ_LIST { get; set; }
		public List<CIQData> CIQ_LIST { get; set; }
		public List<PRData> PR_LIST { get; set; }
		public List<MFNData> MFN_LIST { get; set; }
		public List<GENData> GEN_LIST { get; set; }
		public List<VATData> VAT_LIST { get; set; }
		public List<EDData> ED_LIST { get; set; }
		public List<CADData> CAD_LIST { get; set; }
		public List<ISDData> ISD_LIST { get; set; }
		public List<QUAData> QUA_LIST { get; set; }
		public List<EXPTData> EXPT_LIST { get; set; }
		public List<EXPPData> EXPP_LIST { get; set; }
		public List<AUData> AU_LIST { get; set; }
		public List<CCCData> CCC_LIST { get; set; }
		public List<PTData> PT_LIST { get; set; }
		public List<DUMData> DUM_LIST { get; set; }
		public List<SUBData> SUB_LIST { get; set; }

		MainData MainData => MAIN_LIST?.FirstOrDefault();

		DateTime IJsonData.ValidFrom => VHS_FROM;
		DateTime IJsonData.ValidTo => VHS_TO;
		string IJsonData.Operator => VHS_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region MAIN -- 主表

	/// <summary>
	/// 主表
	/// </summary>
	public sealed class MainData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// hscode
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 税则书hscode_与税则书同步
		/// </summary>
		public string HS_BOOK { get; set; }

		/// <summary>
		/// 层级hscode搜索用_报关使用
		/// </summary>
		public string HS_ETCN_SEARCH { get; set; }

		/// <summary>
		/// 计算层级hscode使用
		/// </summary>
		public string HS_STRUCTURE { get; set; }

		/// <summary>
		/// HS位数
		/// </summary>
		public int? DIGIT_MARK { get; set; }

		/// <summary>
		/// 是否显示
		/// </summary>
		public string HS_VISIBLE { get; set; }

		/// <summary>
		/// 商品描述-中文
		/// </summary>
		public string DESCRIPTION_CN { get; set; }

		/// <summary>
		/// 商品描述-英文
		/// </summary>
		public string DESCRIPTION_EN { get; set; }

		/// <summary>
		/// 第一计量单位
		/// </summary>
		public string UNIT1_CN { get; set; }

		/// <summary>
		/// 第二计量单位
		/// </summary>
		public string UNIT2_CN { get; set; }

		/// <summary>
		/// 第一计量单位-英文
		/// </summary>
		public string UNIT1_EN { get; set; }

		/// <summary>
		/// 第二计量单位-英文
		/// </summary>
		public string UNIT2_EN { get; set; }

		/// <summary>
		/// hs状态：0_失效;1_有效;2_即将失效
		/// </summary>
		public int VMAIN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VMAIN_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VMAIN_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VMAIN_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VMAIN_FROM;
		DateTime IJsonData.ValidTo => VMAIN_TO;
		string IJsonData.Operator => VMAIN_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region YS -- 申报要素

	/// <summary>
	/// 申报要素
	/// </summary>
	public sealed class YSData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 申报要素_中文
		/// </summary>
		public string YS_CN { get; set; }

		/// <summary>
		/// 申报要素_英文
		/// </summary>
		public string YS_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VYS_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VYS_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VYS_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VYS_FROM;
		DateTime IJsonData.ValidTo => VYS_TO;
		string IJsonData.Operator => VYS_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region DOC -- 报关单证

	/// <summary>
	/// 报关单证
	/// </summary>
	public sealed class DOCData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 报关单证代码
		/// </summary>
		public string DOC_CODE { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VDOC_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VDOC_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VDOC_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VDOC_FROM;
		DateTime IJsonData.ValidTo => VDOC_TO;
		string IJsonData.Operator => VDOC_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region SC -- 监管条件

	/// <summary>
	/// 监管条件
	/// </summary>
	public sealed class SCData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 监管条件代码
		/// </summary>
		public string SC_CODE { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSC_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSC_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VSC_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VSC_FROM;
		DateTime IJsonData.ValidTo => VSC_TO;
		string IJsonData.Operator => VSC_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region SJ -- 检验检疫代码

	/// <summary>
	/// 检验检疫代码
	/// </summary>
	public sealed class SJData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 检验检疫条件
		/// </summary>
		public string SJ_CODE { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSJ_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSJ_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VSJ_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VSJ_FROM;
		DateTime IJsonData.ValidTo => VSJ_TO;
		string IJsonData.Operator => VSJ_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region CIQ -- 检验检疫编码

	/// <summary>
	/// 检验检疫编码
	/// </summary>
	public sealed class CIQData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 检验检疫编码
		/// </summary>
		public string CIQ_CODE { get; set; }

		/// <summary>
		/// 检验检疫编码描述-中文
		/// </summary>
		public string CIQ_DESCRIPTION_CN { get; set; }

		/// <summary>
		/// 检验检疫编码描述-英文
		/// </summary>
		public string CIQ_DESCRIPTION_EN { get; set; }

		/// <summary>
		/// 检验检疫扩展编码描述-中文
		/// </summary>
		public string CIQ_EXTEND_CN { get; set; }

		/// <summary>
		/// 检验检疫扩展编码描述-英文
		/// </summary>
		public string CIQ_EXTEND_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCIQ_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCIQ_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VCIQ_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VCIQ_FROM;
		DateTime IJsonData.ValidTo => VCIQ_TO;
		string IJsonData.Operator => VCIQ_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region PR -- 暂定税率

	/// <summary>
	/// 暂定税率
	/// </summary>
	public sealed class PRData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 暂定标记:0_无;1_有
		/// </summary>
		public string PROVISIONAL { get; set; }

		/// <summary>
		/// 暂定税率-中文
		/// </summary>
		public string PROVISIONAL_CN { get; set; }

		/// <summary>
		/// 暂定税率-英文
		/// </summary>
		public string PROVISIONAL_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VPR_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VPR_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VPR_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VPR_FROM;
		DateTime IJsonData.ValidTo => VPR_TO;
		string IJsonData.Operator => VPR_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => PROVISIONAL_CN;
	}

	#endregion

	#region MFN -- 最惠国

	/// <summary>
	/// 最惠国
	/// </summary>
	public sealed class MFNData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 最惠国-中文
		/// </summary>
		public string MFN_CN { get; set; }

		/// <summary>
		/// 最惠国-英文
		/// </summary>
		public string MFN_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VMFN_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VMFN_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VMFN_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VMFN_FROM;
		DateTime IJsonData.ValidTo => VMFN_TO;
		string IJsonData.Operator => VMFN_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => MFN_CN;
	}

	#endregion

	#region GEN -- 普通关税

	/// <summary>
	/// 普通关税
	/// </summary>
	public sealed class GENData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 普通关税-中文
		/// </summary>
		public string GEN_CN { get; set; }

		/// <summary>
		/// 普通关税-英文
		/// </summary>
		public string GEN_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VGEN_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VGEN_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VGEN_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VGEN_FROM;
		DateTime IJsonData.ValidTo => VGEN_TO;
		string IJsonData.Operator => VGEN_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => GEN_CN;
	}

	#endregion

	#region VAT -- 增值税

	/// <summary>
	/// 增值税
	/// </summary>
	public sealed class VATData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 增值税
		/// </summary>
		public string VAT { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VVAT_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VVAT_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VVAT_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VVAT_FROM;
		DateTime IJsonData.ValidTo => VVAT_TO;
		string IJsonData.Operator => VVAT_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region ED -- 出口退税

	/// <summary>
	/// 出口退税
	/// </summary>
	public sealed class EDData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 出口退税
		/// </summary>
		public string ED { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VED_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VED_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VED_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VED_FROM;
		DateTime IJsonData.ValidTo => VED_TO;
		string IJsonData.Operator => VED_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region CAD -- 消费税

	/// <summary>
	/// 消费税
	/// </summary>
	public sealed class CADData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 消费税-中文
		/// </summary>
		public string CONSUMPTION_CN { get; set; }

		/// <summary>
		/// 消费税-英文
		/// </summary>
		public string CONSUMPTION_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCAD_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCAD_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VCAD_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VCAD_FROM;
		DateTime IJsonData.ValidTo => VCAD_TO;
		string IJsonData.Operator => VCAD_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => CONSUMPTION_CN;
	}

	#endregion

	#region ISD -- 特殊消费税

	/// <summary>
	/// 特殊消费税
	/// </summary>
	public sealed class ISDData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 特殊消费税-中文
		/// </summary>
		public string IMP_NUM_TAX_CN { get; set; }

		/// <summary>
		/// 特殊消费税-英文
		/// </summary>
		public string IMP_NUM_TAX_EN { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string REMARKS { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VISD_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VISD_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VISD_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VISD_FROM;
		DateTime IJsonData.ValidTo => VISD_TO;
		string IJsonData.Operator => VISD_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => IMP_NUM_TAX_CN;
	}

	#endregion

	#region QUA -- 进口配额

	/// <summary>
	/// 进口配额
	/// </summary>
	public sealed class QUAData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 商品类别
		/// </summary>
		public string PRODUCT_TYPE { get; set; }

		/// <summary>
		/// 配额外税率_普通税率（％）
		/// </summary>
		public string OUTSIDE_GEN { get; set; }

		/// <summary>
		/// 配额外税率_最惠国税率（％）
		/// </summary>
		public string OUTSIDE_MFN { get; set; }

		/// <summary>
		/// 配额内税率(％)
		/// </summary>
		public string WITHIN_TAXRATE { get; set; }

		/// <summary>
		/// 新西兰配额
		/// </summary>
		public string NEWZEALAND_QUOTA { get; set; }

		/// <summary>
		/// 澳大利亚配额
		/// </summary>
		public string AUSTRALIA_QUOTA { get; set; }

		/// <summary>
		/// 进口配额量
		/// </summary>
		public string EN_NUM { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VQUA_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VQUA_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VQUA_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VQUA_FROM;
		DateTime IJsonData.ValidTo => VQUA_TO;
		string IJsonData.Operator => VQUA_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region EXPT -- 出口关税

	/// <summary>
	/// 出口关税
	/// </summary>
	public sealed class EXPTData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 出口关税
		/// </summary>
		public string EXPORT_RATE { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VEXPT_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VEXPT_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VEXPT_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VEXPT_FROM;
		DateTime IJsonData.ValidTo => VEXPT_TO;
		string IJsonData.Operator => VEXPT_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => EXPORT_RATE;
	}

	#endregion

	#region EXPP -- 出口暂定

	/// <summary>
	/// 出口暂定
	/// </summary>
	public sealed class EXPPData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 出口暂定-中文
		/// </summary>
		public string EXPORT_PROVISIONAL_CN { get; set; }

		/// <summary>
		/// 出口暂定-英文
		/// </summary>
		public string EXPORT_PROVISIONAL_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VEXPP_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VEXPP_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VEXPP_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VEXPP_FROM;
		DateTime IJsonData.ValidTo => VEXPP_TO;
		string IJsonData.Operator => VEXPP_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => EXPORT_PROVISIONAL_CN;
	}

	#endregion

	#region AU -- 美国加征关税

	/// <summary>
	/// 美国加征关税
	/// </summary>
	public sealed class AUData : IRateData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 美国加征关税
		/// </summary>
		public string ADDRATE_USA { get; set; }

		/// <summary>
		/// 美国加征关税说明
		/// </summary>
		public string VAU_DESCRIPTION { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VAU_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VAU_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VAU_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VAU_FROM;
		DateTime IJsonData.ValidTo => VAU_TO;
		string IJsonData.Operator => VAU_OPT;
		DateTime IJsonData.CreateTime => CTIME;
		string IRateData.Rate => ADDRATE_USA;
	}

	#endregion

	#region CCC -- CCC

	/// <summary>
	/// CCC
	/// </summary>
	public sealed class CCCData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// 唯一标识_关联导入ID
		/// </summary>
		public string CCC_CODE { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 商品分类_中文
		/// </summary>
		public string PRODUCT_CATEGORY_CN { get; set; }

		/// <summary>
		/// 商品分类_英文
		/// </summary>
		public string PRODUCT_CATEGORY_EN { get; set; }

		/// <summary>
		/// 例举和说明_中文
		/// </summary>
		public string CATEGORIAL_DESCRIPTION_CN { get; set; }

		/// <summary>
		/// 例举和说明_英文
		/// </summary>
		public string CATEGORIAL_DESCRIPTION_EN { get; set; }

		/// <summary>
		/// 商品适用范围_中文
		/// </summary>
		public string APPLICABLE_PRODUCT_CN { get; set; }

		/// <summary>
		/// 商品适用范围_英文
		/// </summary>
		public string APPLICABLE_PRODUCT_EN { get; set; }

		/// <summary>
		/// 适用标准_中文
		/// </summary>
		public string APPLICABLE_STANDARD_CN { get; set; }

		/// <summary>
		/// 适用标准_英文
		/// </summary>
		public string APPLICABLE_STANDARD_EN { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCCC_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VCCC_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VCCC_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VCCC_FROM;
		DateTime IJsonData.ValidTo => VCCC_TO;
		string IJsonData.Operator => VCCC_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region PT -- 协定特惠

	/// <summary>
	/// 协定特惠
	/// </summary>
	public sealed class PTData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// MFN
		/// </summary>
		public string MFN { get; set; }

		/// <summary>
		/// ASAEN
		/// </summary>
		public string ASAEN { get; set; }

		/// <summary>
		/// 亚太5个国家
		/// </summary>
		public string FTA_ASIA_PACIFIC_COUNTRIES_5 { get; set; }

		/// <summary>
		/// 巴基斯坦
		/// </summary>
		public string FTA_PAKISTAN { get; set; }

		/// <summary>
		/// 智利
		/// </summary>
		public string FTA_CHILE { get; set; }

		/// <summary>
		/// 新西兰
		/// </summary>
		public string FTA_NEWZEALAND { get; set; }

		/// <summary>
		/// 新加坡
		/// </summary>
		public string FTA_SINGAPORE { get; set; }

		/// <summary>
		/// 秘鲁
		/// </summary>
		public string FTA_PERU { get; set; }

		/// <summary>
		/// 塞尔维亚
		/// </summary>
		public string FTA_SERBIA { get; set; }

		/// <summary>
		/// 哥斯达黎加
		/// </summary>
		public string FTA_COSTARICA { get; set; }

		/// <summary>
		/// 瑞士
		/// </summary>
		public string FTA_SWITZERLAND { get; set; }

		/// <summary>
		/// 澳大利亚
		/// </summary>
		public string FTA_AUSTRALIA { get; set; }

		/// <summary>
		/// 韩国
		/// </summary>
		public string FTA_SOUTHKOREA { get; set; }

		/// <summary>
		/// 格鲁吉亚
		/// </summary>
		public string FTA_GEORGIA { get; set; }

		/// <summary>
		/// 冰岛
		/// </summary>
		public string FTA_ICELAND { get; set; }

		/// <summary>
		/// 洪都拉斯
		/// </summary>
		public string FTA_HONDURAS { get; set; }

		/// <summary>
		/// 香港
		/// </summary>
		public string FTA_HONGKONG { get; set; }

		/// <summary>
		/// 澳门
		/// </summary>
		public string FTA_MACAU { get; set; }

		/// <summary>
		/// 毛里求斯
		/// </summary>
		public string FTA_MAURITIUS { get; set; }

		/// <summary>
		/// 马尔代夫
		/// </summary>
		public string FTA_MALDIVES { get; set; }

		/// <summary>
		/// 台湾
		/// </summary>
		public string FTA_TAIWAN { get; set; }

		/// <summary>
		/// 柬埔寨
		/// </summary>
		public string FTA_CAMBODIA { get; set; }

		/// <summary>
		/// 尼加拉瓜
		/// </summary>
		public string FTA_NICARAGUA { get; set; }

		/// <summary>
		/// 厄瓜多尔
		/// </summary>
		public string FTA_ECUADOR { get; set; }

		/// <summary>
		/// 亚太2个国家
		/// </summary>
		public string SP_ASIA_PACIFIC_COUNTRIES_2 { get; set; }

		/// <summary>
		/// 缅甸
		/// </summary>
		public string SP_MYANMAR { get; set; }

		/// <summary>
		/// 老挝
		/// </summary>
		public string SP_LAOS { get; set; }

		/// <summary>
		/// 柬埔寨
		/// </summary>
		public string SP_CAMBODIA { get; set; }

		/// <summary>
		/// LDC
		/// </summary>
		public string SP_LDC { get; set; }

		/// <summary>
		/// [Obsoleted] LDC1
		/// </summary>
		public string SP_LDC1 { get; set; }

		/// <summary>
		/// [Obsoleted] LDC2
		/// </summary>
		public string SP_LDC2 { get; set; }

		/// <summary>
		/// [Obsoleted] LDC3
		/// </summary>
		public string SP_LDC3 { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VPT_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VPT_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VPT_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VPT_FROM;
		DateTime IJsonData.ValidTo => VPT_TO;
		string IJsonData.Operator => VPT_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region DUM -- 反倾销

	/// <summary>
	/// 反倾销
	/// </summary>
	public sealed class DUMData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// 唯一标识_关联导入ID
		/// </summary>
		public string DUM_CODE { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 商品描述_中文
		/// </summary>
		public string COMMODITY_CN { get; set; }

		/// <summary>
		/// 商品描述_英文
		/// </summary>
		public string COMMODITY_EN { get; set; }

		/// <summary>
		/// 国家
		/// </summary>
		public string COUNTRY_CODE { get; set; }

		/// <summary>
		/// 公司
		/// </summary>
		public string COMPANY { get; set; }

		/// <summary>
		/// 反倾销关税
		/// </summary>
		public string DUTY_RATE_DUMPING { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VDUM_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VDUM_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VDUM_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VDUM_FROM;
		DateTime IJsonData.ValidTo => VDUM_TO;
		string IJsonData.Operator => VDUM_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion

	#region SUB -- 反补贴

	/// <summary>
	/// 反补贴
	/// </summary>
	public sealed class SUBData : IJsonData
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// 唯一标识_关联导入ID
		/// </summary>
		public string SUB_CODE { get; set; }

		/// <summary>
		/// HS代码
		/// </summary>
		public string HS_CODE { get; set; }

		/// <summary>
		/// 商品描述_中文
		/// </summary>
		public string COMMODITY_CN { get; set; }

		/// <summary>
		/// 商品描述_英文
		/// </summary>
		public string COMMODITY_EN { get; set; }

		/// <summary>
		/// 国家
		/// </summary>
		public string COUNTRY_CODE { get; set; }

		/// <summary>
		/// 公司
		/// </summary>
		public string COMPANY { get; set; }

		/// <summary>
		/// 反补贴关税
		/// </summary>
		public string SUTY_RATE_SUBSIDY { get; set; }

		/// <summary>
		/// 生效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSUB_FROM { get; set; }

		/// <summary>
		/// 失效时间，格式为YYYY-MM-DD
		/// </summary>
		public DateTime VSUB_TO { get; set; }

		/// <summary>
		/// 操作类型：a_新增;d_删除;n_未改动(辅助字段)
		/// </summary>
		public string VSUB_OPT { get; set; }

		/// <summary>
		/// 创建时间，格式为YYYY-MM-DD HH:MM:SS
		/// </summary>
		public DateTime CTIME { get; set; }

		DateTime IJsonData.ValidFrom => VSUB_FROM;
		DateTime IJsonData.ValidTo => VSUB_TO;
		string IJsonData.Operator => VSUB_OPT;
		DateTime IJsonData.CreateTime => CTIME;
	}

	#endregion
}
