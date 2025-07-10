using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.AWB.TNT.Testing
{
	sealed class TNTCustomsRowDataTest : TestCaseWithFactory
	{
		public void TestAllColumnsComeInFromOneRow()
		{
			var rowData = new TNTCustomsRowData();
			rowData.ReadRow(singleRowFileContent);

			CombineAssertions(delegate
			{
				#region Page 2
				AssertEquals(100, rowData.RECORD_TYPE);
				AssertEquals("WW", rowData.CON_COM_ID);
				AssertEquals("193878238", rowData.CON_ID);
				AssertEquals(999, rowData.CON_SEQ_ID);
				AssertEquals(1, rowData.CNA_CON_SEQ_ID);
				AssertEquals(1, rowData.ADS_CNA_ID);
				AssertEquals("PERSONAL EFFECT", rowData.ADS_DS);
				AssertEquals(400000.00m, rowData.CNA_ART_AM);
				AssertEquals("", rowData.CNA_BOX39_TX);
				AssertEquals("N", rowData.CNA_CERT_ORIG_TYPE_CD);
				AssertEquals("", rowData.CNA_CERT_ORIG_NR);
				AssertEquals("THB", rowData.CNA_CUY_ID_AM);
				AssertEquals("CNA_CUY_ID_INVOICE", "", rowData.CNA_CUY_ID_INVOICE);
				AssertEquals("CNA_CUY_ID_SAMPLE", "USD", rowData.CNA_CUY_ID_SAMPLE);
				AssertEquals("CNA_CUY_ID_STATS", "USD", rowData.CNA_CUY_ID_STATS);
				AssertEquals("CNA_EXPDOC_CD", "", rowData.CNA_EXPDOC_CD);
				AssertEquals("CNA_EXPDOC_CITY_NM", "", rowData.CNA_EXPDOC_CITY_NM);
				AssertEquals("CNA_EXPDOC_DT_CC", "", rowData.CNA_EXPDOC_DT_CC);
				AssertEquals("CNA_EXPDOC_DT_YY", "", rowData.CNA_EXPDOC_DT_YY);
				AssertEquals("CNA_EXPDOC_DT_MM", "", rowData.CNA_EXPDOC_DT_MM);
				AssertEquals("CNA_EXPDOC_DT_DD", "", rowData.CNA_EXPDOC_DT_DD);
				AssertEquals("CNA_EXPDOC_NR", "", rowData.CNA_EXPDOC_NR);
				AssertEquals("CNA_EXPDOC_TYPE_CD", "EX", rowData.CNA_EXPDOC_TYPE_CD);
				AssertEquals("CNA_EXPDOC_UNOFF_NR", "", rowData.CNA_EXPDOC_UNOFF_NR);
				AssertEquals("CNA_GROSS_WT", 88.000M, rowData.CNA_GROSS_WT);
				AssertEquals("CNA_HAZ_CD", "", rowData.CNA_HAZ_CD);
				AssertEquals("CNA_ITEM_QT", 0, rowData.CNA_ITEM_QT);
				AssertEquals("CNA_LIC_ID", "", rowData.CNA_LIC_ID);
				AssertEquals("CNA_MARKS_TX", "ADDRESS", rowData.CNA_MARKS_TX);
				AssertEquals("CNA_NET_WT", 88.000M, rowData.CNA_NET_WT);
				AssertEquals("CNA_ORIG_COU_ID", "TH", rowData.CNA_ORIG_COU_ID);
				AssertEquals("NumericCountryCode", 680, rowData.CNA_ORIG_COU_Code);
				AssertEquals("CountryDescriptionForOrigin", "THAILAND", rowData.CNA_ORIG_COU_DES);
				AssertEquals("CNA_PACK_DS", "", rowData.CNA_PACK_DS);
				AssertEquals("CNA_PREVDOC_CITY_NM", "", rowData.CNA_PREVDOC_CITY_NM);
				AssertEquals("CNA_PREVDOC_DT_CC", "", rowData.CNA_PREVDOC_DT_CC);
				AssertEquals("CNA_PREVDOC_DT_YY", "", rowData.CNA_PREVDOC_DT_YY);
				AssertEquals("CNA_PREVDOC_DT_MM", "", rowData.CNA_PREVDOC_DT_MM);
				AssertEquals("CNA_PREVDOC_DT_DD", "", rowData.CNA_PREVDOC_DT_DD);
				AssertEquals("CNA_PREVDOC_NR", "", rowData.CNA_PREVDOC_NR);
				AssertEquals("CNA_PREVDOC_TYPE_CD", "", rowData.CNA_PREVDOC_TYPE_CD);
				AssertEquals("CNA_PROC_CD_1", "", rowData.CNA_PROC_CD_1);
				AssertEquals("CNA_PROC_CD_2", "", rowData.CNA_PROC_CD_2);
				AssertEquals("CNA_SAMPLE_AM", 0.00M, rowData.CNA_SAMPLE_AM);
				AssertEquals("CNA_SEP_IN", "", rowData.CNA_SEP_IN);
				#endregion

				#region page 3

				AssertEquals("CNA_SIMPL_IN", "", rowData.CNA_SIMPL_IN);
				AssertEquals("CNA_SPEC_MEN_TX_1", "", rowData.CNA_SPEC_MEN_TX_1);
				AssertEquals("CNA_SPEC_MEN_TX_2", "", rowData.CNA_SPEC_MEN_TX_2);
				AssertEquals("CNA_STATS_AM", 13308.88M, rowData.CNA_STATS_AM);

				#endregion

				#region page 4
				AssertEquals("CN_TDOC_CITY_NM", "", rowData.CN_TDOC_CITY_NM);
				AssertEquals("CNA_TDOC_DT_CC", "", rowData.CNA_TDOC_DT_CC);
				AssertEquals("CNA_TDOC_DT_YY", "", rowData.CNA_TDOC_DT_YY);
				AssertEquals("CNA_TDOC_DT_MM", "", rowData.CNA_TDOC_DT_MM);
				AssertEquals("CNA_TDOC_DT_DD", "", rowData.CNA_TDOC_DT_DD);
				AssertEquals("CNA_TDOC_NR", "", rowData.CNA_TDOC_NR);
				AssertEquals("CNA_TDOC_TYPE_CD", "", rowData.CNA_TDOC_TYPE_CD);
				AssertEquals("CNA_TDOC_UNOFF_NR", "", rowData.CNA_TDOC_UNOFF_NR);
				AssertEquals("CNA_TF_ID", "990500", rowData.CNA_TF_ID);
				AssertEquals("CNA_TRANS_TYPE_CD", "", rowData.CNA_TRANS_TYPE_CD);
				AssertEquals("CNA_UN_CD", "", rowData.CNA_UN_CD);
				AssertEquals("CNA_UNITS_DS", "", rowData.CNA_UNITS_DS);
				AssertEquals("CNA_UNITS_NR", 0, rowData.CNA_UNITS_NR);
				AssertEquals("CNA_VALUE_FOR_CUSTOMS", 400000.00M, rowData.CNA_VALUE_FOR_CUSTOMS);
				AssertEquals("CON_ACT_TRADE_CD", "8", rowData.CON_ACT_TRADE_CD);
				AssertEquals("CON_BUL_ID_CON_ORIG", "HKT", rowData.CON_BUL_ID_CON_ORIG);
				AssertEquals("CON_BUL_ID_CLR", "LIN", rowData.CON_BUL_ID_CLR);
				AssertEquals("CON_BUL_ID_CREATE", "HKT", rowData.CON_BUL_ID_CREATE);
				AssertEquals("CON_BUL_ID_DEST", "QPZ", rowData.CON_BUL_ID_DEST);
				AssertEquals("CON_BUL_ID_DOC", "BG5", rowData.CON_BUL_ID_DOC);
				AssertEquals("CON_BUL_ID_NEXT", "", rowData.CON_BUL_ID_NEXT);
				AssertEquals("CON_CLNT_REF_TX", "", rowData.CON_CLNT_REF_TX);
				AssertEquals("CON_COD_AM", 0.00M, rowData.CON_COD_AM);
				AssertEquals("CON_CON_ID_ALT", "", rowData.CON_CON_ID_ALT);
				AssertEquals("CON_CONTROLLED_IN", "Y", rowData.CON_CONTROLLED_IN);
				AssertEquals("CON_COU_ID_GOODS_ORIG", "", rowData.CON_COU_ID_GOODS_ORIG);
				AssertEquals("CON_COU_CODE_GOODS_ORIG", 0, rowData.CON_COU_CODE_GOODS_ORIG);
				AssertEquals("CON_COU_DES_GOODS_ORIG", "", rowData.CON_COU_DES_GOODS_ORIG);
				AssertEquals("CON_CREATE_DATE", "20101210", rowData.CON_CREATE_DATE);
				AssertEquals("CON_CREATE_TIME", "1456", rowData.CON_CREATE_TIME);
				AssertEquals("CON_CUST_DELIV_CD", "CIF", rowData.CON_CUST_DELIV_CD);
				AssertEquals("CON_CUST_DELIV_CITY_NM", "PIACENZA", rowData.CON_CUST_DELIV_CITY_NM);
				AssertEquals("CON_CUST_TRADE_CD", "8", rowData.CON_CUST_TRADE_CD);
				AssertEquals("CON_CUY_ID_VAL_OF_GOODS", "THB", rowData.CON_CUY_ID_VAL_OF_GOODS);
				AssertEquals("CON_CUY_ID_INS", "", rowData.CON_CUY_ID_INS);
				AssertEquals("CON_DELIV_INSTR_CODE", "", rowData.CON_DELIV_INSTR_CODE);
				AssertEquals("CON_DELIV_INST_TX", "", rowData.CON_DELIV_INST_TX);
				AssertEquals("CON_DEPOT_ACTION_DT_CC", "20", rowData.CON_DEPOT_ACTION_DT_CC);
				AssertEquals("CON_DEPOT_ACTION_DT_YY", "10", rowData.CON_DEPOT_ACTION_DT_YY);
				AssertEquals("CON_DEPOT_ACTION_DT_MM", "12", rowData.CON_DEPOT_ACTION_DT_MM);

				#endregion

				#region page 5

				AssertEquals("CON_DEPOT_ACTION_DT_DD", "20", rowData.CON_DEPOT_ACTION_DT_DD);
				AssertEquals("CON_DIV_ID", "G", rowData.CON_DIV_ID);
				AssertEquals("CON_DOCUMENT_IN", "N", rowData.CON_DOCUMENT_IN);
				AssertEquals("CON_EXHIBITION_IN", "N", rowData.CON_EXHIBITION_IN);
				AssertEquals("CON_FINANCE_TX_1", "", rowData.CON_FINANCE_TX_1);
				AssertEquals("CON_FINANCE_TX_2", "", rowData.CON_FINANCE_TX_2);
				AssertEquals("CON_FINANCE_TX_3", "", rowData.CON_FINANCE_TX_3);
				AssertEquals("CON_GOODS_DS", "PERSONAL EFFECT", rowData.CON_GOODS_DS);
				AssertEquals("CON_HAZ_CD", "", rowData.CON_HAZ_CD);
				AssertEquals("CON_INS_AM", 0.00M, rowData.CON_INS_AM);
				AssertEquals("CON_NET_SLVL_CD", "R", rowData.CON_NET_SLVL_CD);

				#endregion

				#region Page 6
				AssertEquals("CON_OA_TOT_VL", 0.001M, rowData.CON_OA_TOT_VL);
				AssertEquals("CON_OC_TGRS_WT", 88.000M, rowData.CON_OC_TGRS_WT);
				AssertEquals("CON_TOTAL_CONTRACTUAL_GROSS_WEIGHT_ROUNDED", 88, rowData.CON_TOTAL_CONTRACTUAL_GROSS_WEIGHT_ROUNDED);
				AssertEquals("CON_OC_TITEM_QT", 4, rowData.CON_OC_TITEM_QT);
				AssertEquals("CON_OC_TOT_VL", 0.001M, rowData.CON_OC_TOT_VL);
				AssertEquals("CON_OPSA_TGRS_WT", 88.000M, rowData.CON_OPSA_TGRS_WT);
				AssertEquals("CON_OPSA_TGRS_WT", 88, rowData.CON_TOTALACTUALGROSSWEIGHTROUNDED);
				AssertEquals("CON_OPTION_1", "", rowData.CON_OPTION_1);
				AssertEquals("CON_OPTION_2", "", rowData.CON_OPTION_2);
				AssertEquals("CON_OPTION_3", "", rowData.CON_OPTION_3);
				AssertEquals("CON_OPTION_4", "", rowData.CON_OPTION_4);
				AssertEquals("CON_PACKING_CD", "CT", rowData.CON_PACKING_CD);
				AssertEquals("CON_PACKING_DS", "CARTON", rowData.CON_PACKING_DS);
				AssertEquals("CON_PRD_ID", "48N", rowData.CON_PRD_ID);
				AssertEquals("CON_PWORK_COMPL_IN", "", rowData.CON_PWORK_COMPL_IN);
				AssertEquals("CON_PWORK_SCAN_IN", "", rowData.CON_PWORK_SCAN_IN);
				AssertEquals("CON_SELF_COLL_IND", "N", rowData.CON_SELF_COLL_IND);
				AssertEquals("CON_TDOC_PRT_IN", "Y", rowData.CON_TDOC_PRT_IN);
				AssertEquals("CON_TDOC_SEP_IN", "", rowData.CON_TDOC_SEP_IN);
				AssertEquals("CON_TOP_ID_OPS_ACT", "S", rowData.CON_TOP_ID_OPS_ACT);
				AssertEquals("CON_TOT_PIECE_QT", 4, rowData.CON_TOT_PIECE_QT);
				AssertEquals("CON_UN_CD", 0, rowData.CON_UN_CD);
				AssertEquals("CON_VAL_OF_GOODS_AM", 400000.000M, rowData.CON_VAL_OF_GOODS_AM);
				AssertEquals("SEN_ACC_ID", "000075559", rowData.SEN_ACC_ID);
				AssertEquals("SEN_ACG_ID", "001", rowData.SEN_ACG_ID);
				AssertEquals("SEN_ADDR_1_DS", "34/13 PRACHANUKROH ROAD,", rowData.SEN_ADDR_1_DS);
				AssertEquals("SEN_ADDR_2_DS", "", rowData.SEN_ADDR_2_DS);
				AssertEquals("SEN_ADDR_3_DS", "", rowData.SEN_ADDR_3_DS);
				AssertEquals("SEN_CITY_NM", "KATHU", rowData.SEN_CITY_NM);
				AssertEquals("SEN_COU_ID", "TH", rowData.SEN_COU_ID);
				AssertEquals("SEN_COU_CODE", 680, rowData.SEN_COU_CODE);
				AssertEquals("SEN_COU_DES", "THAILAND", rowData.SEN_COU_DES);
				AssertEquals("SEN_CPF_LAST_NM", "PRAPHUT", rowData.SEN_CPF_LAST_NM);
				AssertEquals("SEN_CPF_TEL_1_ID", "076", rowData.SEN_CPF_TEL_1_ID);
				AssertEquals("SEN_CPF_TEL_2_ID", "344687", rowData.SEN_CPF_TEL_2_ID);
				AssertEquals("SEN_NAD_ID", "MVS010023329", rowData.SEN_NAD_ID);
				AssertEquals("SEN_NM", "ANGELINA TRAVEL & TOUR", rowData.SEN_NM);
				AssertEquals("SEN_PCODE_CD", "83150", rowData.SEN_PCODE_CD);
				AssertEquals("SEN_PRV_NM", "PHUKET", rowData.SEN_PRV_NM);
				AssertEquals("SEN_VAT_ID", "", rowData.SEN_VAT_ID);
				AssertEquals("REC_ACC_ID", "", rowData.REC_ACC_ID);
				AssertEquals("REC_ACG_ID", "", rowData.REC_ACG_ID);
				AssertEquals("REC_ADDR_1_DS", "VIA FULGOSIO 17A", rowData.REC_ADDR_1_DS);
				AssertEquals("REC_ADDR_2_DS", "", rowData.REC_ADDR_2_DS);
				AssertEquals("REC_ADDR_3_DS", "", rowData.REC_ADDR_3_DS);

				#endregion

				#region Page 7

				AssertEquals("REC_CITY_NM", "PIACENZA", rowData.REC_CITY_NM);
				AssertEquals("REC_COU_ID", "IT", rowData.REC_COU_ID);
				AssertEquals("REC_COU_CODE", 5, rowData.REC_COU_CODE);
				AssertEquals("REC_COU_DES", "ITALY", rowData.REC_COU_DES);
				AssertEquals("REC_CPF_LAST_NM", "", rowData.REC_CPF_LAST_NM);
				AssertEquals("REC_CPF_TEL_1_ID", "", rowData.REC_CPF_TEL_1_ID);
				AssertEquals("REC_CPF_TEL_2_ID", "", rowData.REC_CPF_TEL_2_ID);
				AssertEquals("REC_NAD_ID", "", rowData.REC_NAD_ID);
				AssertEquals("REC_NM", "SIMONA BAZZONI MS", rowData.REC_NM);
				AssertEquals("REC_PCODE_CD", "29100", rowData.REC_PCODE_CD);
				AssertEquals("REC_PRV_NM", "PC", rowData.REC_PRV_NM);
				AssertEquals("REC_VAT_ID", "", rowData.REC_VAT_ID);
				AssertEquals("COL_ACC_ID", "", rowData.COL_ACC_ID);
				AssertEquals("COL_ACG_ID", "", rowData.COL_ACG_ID);

				#endregion

				#region Page 8

				AssertEquals("COL_ADDR_1_DS", "", rowData.COL_ADDR_1_DS);
				AssertEquals("COL_ADDR_2_DS", "", rowData.COL_ADDR_2_DS);
				AssertEquals("COL_ADDR_3_DS", "", rowData.COL_ADDR_3_DS);
				AssertEquals("COL_CITY_NM", "", rowData.COL_CITY_NM);
				AssertEquals("COL_COU_ID", "", rowData.COL_COU_ID);
				AssertEquals("COL_COU_CODE", 0, rowData.COL_COU_CODE);
				AssertEquals("COL_COU_DES", "", rowData.COL_COU_DES);
				AssertEquals("COL_CPF_LAST_NM", "", rowData.COL_CPF_LAST_NM);
				AssertEquals("COL_CPF_TEL_1_ID", "", rowData.COL_CPF_TEL_1_ID);
				AssertEquals("COL_CPF_TEL_2_ID", "", rowData.COL_CPF_TEL_2_ID);
				AssertEquals("COL_NAD_ID", "", rowData.COL_NAD_ID);
				AssertEquals("COL_NM", "", rowData.COL_NM);
				AssertEquals("COL_PCODE_CD", "", rowData.COL_PCODE_CD);
				AssertEquals("COL_PRV_NM", "", rowData.COL_PRV_NM);
				AssertEquals("COL_VAT_ID", "", rowData.COL_VAT_ID);
				AssertEquals("DEL_ACC_ID", "", rowData.DEL_ACC_ID);
				AssertEquals("DEL_ACG_ID", "", rowData.DEL_ACG_ID);
				AssertEquals("DEL_ADDR_1_DS", "", rowData.DEL_ADDR_1_DS);
				AssertEquals("DEL_ADDR_2_DS", "", rowData.DEL_ADDR_2_DS);
				AssertEquals("DEL_ADDR_3_DS", "", rowData.DEL_ADDR_3_DS);
				AssertEquals("DEL_CITY_NM", "", rowData.DEL_CITY_NM);
				AssertEquals("DEL_COU_ID", "", rowData.DEL_COU_ID);
				AssertEquals("DEL_COU_CODE", 0, rowData.DEL_COU_CODE);
				AssertEquals("DEL_COU_DES", "", rowData.DEL_COU_DES);
				AssertEquals("DEL_CPF_LAST_NM", "", rowData.DEL_CPF_LAST_NM);
				AssertEquals("DEL_CPF_TEL_1_ID", "", rowData.DEL_CPF_TEL_1_ID);
				AssertEquals("DEL_CPF_TEL_2_ID", "", rowData.DEL_CPF_TEL_2_ID);
				AssertEquals("DEL_NAD_ID", "", rowData.DEL_NAD_ID);
				AssertEquals("DEL_NM", "", rowData.DEL_NM);
				AssertEquals("DEL_PCODE_CD", "", rowData.DEL_PCODE_CD);
				AssertEquals("DEL_PRV_NM", "", rowData.DEL_PRV_NM);
				AssertEquals("DEL_VAT_ID", "", rowData.DEL_VAT_ID);
				AssertEquals("MOV_CUSTOMS_REF_NR", "", rowData.MOV_CUSTOMS_REF_NR);
				AssertEquals("MOV_ACT_ARR_DT", "", rowData.MOV_ACT_ARR_DT);
				AssertEquals("MOV_ACT_ARR_TM", "", rowData.MOV_ACT_ARR_TM);
				AssertEquals("MOV_ACT_DEP_DT", "20101212", rowData.MOV_ACT_DEP_DT);
				AssertEquals("MOV_ACT_DEP_TM", "2359", rowData.MOV_ACT_DEP_TM);
				AssertEquals("MOV_COM_ID", "WW", rowData.MOV_COM_ID);
				AssertEquals("MOV_COU_ID1", "", rowData.MOV_COU_ID1);
				AssertEquals("MOV_COU_CODE1", "000", rowData.MOV_COU_CODE1);
				AssertEquals("MOV_COU_DES1", "", rowData.MOV_COU_DES1);
				AssertEquals("MOV_COU_ID2", "", rowData.MOV_COU_ID2);
				AssertEquals("MOV_COU_CODE2", "000", rowData.MOV_COU_CODE2);
				AssertEquals("MOV_COU_DES2", "", rowData.MOV_COU_DES2);
				AssertEquals("MOV_CRR_ID", "KL", rowData.MOV_CRR_ID);
				AssertEquals("MOV_DEST_BUL_ID", "AMS", rowData.MOV_DEST_BUL_ID);

				#endregion

				#region Page 9

				AssertEquals("MOV_DEP_DT", "20101212", rowData.MOV_DEP_DT);
				AssertEquals("MOV_FIS_CONT_TYPE_CD", "", rowData.MOV_FIS_CONT_TYPE_CD);
				AssertEquals("MOV_FIS_CONT_NR1", "", rowData.MOV_FIS_CONT_NR1);
				AssertEquals("MOV_FIS_CONT_NR2", "", rowData.MOV_FIS_CONT_NR2);
				AssertEquals("MOV_FIS_CONT_NR3", "", rowData.MOV_FIS_CONT_NR3);
				AssertEquals("MOV_FIS_CONT_NR4", "", rowData.MOV_FIS_CONT_NR4);
				AssertEquals("MOV_FIS_TYPE_CD1", "", rowData.MOV_FIS_TYPE_CD1);
				AssertEquals("MOV_FIS_TYPE_CD2", "", rowData.MOV_FIS_TYPE_CD2);
				AssertEquals("MOV_LIC_PLATE_NR1", "", rowData.MOV_LIC_PLATE_NR1);

				#endregion

				#region Page 10
				AssertEquals("MOV_LIC_PLATE_NR2", "", rowData.MOV_LIC_PLATE_NR2);
				AssertEquals("MOV_MAWB_CD", "07446126371", rowData.MOV_MAWB_CD);
				AssertEquals("MOV_NET_CD", "", rowData.MOV_NET_CD);
				AssertEquals("MOV_NR", "0878", rowData.MOV_NR);
				AssertEquals("MOV_ORIG_BUL_ID", "BKK", rowData.MOV_ORIG_BUL_ID);
				AssertEquals("MOV_PORT_XING_CD", "", rowData.MOV_PORT_XING_CD);
				AssertEquals("MOV_SEAL_NR1", "", rowData.MOV_SEAL_NR1);
				AssertEquals("MOV_SEAL_NR2", "", rowData.MOV_SEAL_NR2);
				AssertEquals("MOV_SECTOR_TYPE_CD", "A", rowData.MOV_SECTOR_TYPE_CD);
				AssertEquals("MOV_SEQ_ID", 0, rowData.MOV_SEQ_ID);
				AssertEquals("MOV_SMD_ID", "A", rowData.MOV_SMD_ID);
				AssertEquals("MOV_SUBTYPE_CD", "O", rowData.MOV_SUBTYPE_CD);
				AssertEquals("MOV_TYPE_CD", "S", rowData.MOV_TYPE_CD);
				AssertEquals("MOV_TRANSPORT_ID", "", rowData.MOV_TRANSPORT_ID);
				AssertEquals("LAV_VALUE_DS1", "USD", rowData.LAV_VALUE_DS1);
				AssertEquals("LAV_VALUE_DS2", "", rowData.LAV_VALUE_DS2);
				AssertEquals("AGENT_COU_ID", "TH", rowData.AGENT_COU_ID);
				AssertEquals("AGENT_COU_CODE", 680, rowData.AGENT_COU_CODE);
				AssertEquals("AGENT_COU_DES", "THAILAND", rowData.AGENT_COU_DES);
				AssertEquals("BUL_NM", "BANGKOK  AIR HUB", rowData.BUL_NM);
				AssertEquals("BUL_ADDR_1_DS", "999 M 7  RACHATHEWA", rowData.BUL_ADDR_1_DS);
				AssertEquals("BUL_ADDR_2_DS", "SUVARNABHUMI INT L CUSTOMS", rowData.BUL_ADDR_2_DS);
				AssertEquals("BUL_ADDR_3_DS", "T201 WFS-PG CARGO BUILDIN", rowData.BUL_ADDR_3_DS);
				AssertEquals("BUL_PCODE_CD", "10540", rowData.BUL_PCODE_CD);
				AssertEquals("BUL_CITY_NM", "BANG PHLI", rowData.BUL_CITY_NM);
				AssertEquals("BUL_TNT_NM", "TNT EXPRESS WORLDWIDE CO  LTD", rowData.BUL_TNT_NM);
				AssertEquals("BUL_PBOX_CD", "", rowData.BUL_PBOX_CD);
				AssertEquals("BUL_PBOX_CITY_NM", "", rowData.BUL_PBOX_CITY_NM);
				AssertEquals("BUL_PBOX_PCODE_CD", "", rowData.BUL_PBOX_PCODE_CD);
				AssertEquals("BUL_VAT_ID", "", rowData.BUL_VAT_ID);
				AssertEquals("BUL_PROVINCE_NM", "BANGKOK", rowData.BUL_PROVINCE_NM);
				AssertEquals("REQUEST_USER", "XDCSFM", rowData.REQUEST_USER);

				#endregion
			});
		}

		const string singleRowFileContent = @"100WW193878238      999001001PERSONAL EFFECT                                                                                                                                                                                                                  0000040000000   N             THB   USDUSD                                                    EX               00088000    000          ADDRESS   00088000TH 680THAILAND                                                                                                        0000000000000                                                                                  0000001330888                                                                        990500                                          000000000400000008   HKT  LIN  HKT  QPZ  BG5                               0000000000000               Y   000                              201012101456CIFPIACENZA                8   THB                                                                20101220G  NN                                                                                          PERSONAL EFFECT                   0000000000000R 0000001000880000008800040000010008800000088            CTCARTON              48N   NY S00004    0000040000000000007555900134/13 PRACHANUKROH ROAD,                                                                  KATHU                         TH 680THAILAND                      PRAPHUT               076    344687   MVS010023329ANGELINA TRAVEL & TOUR                            83150    PHUKET                                                        VIA FULGOSIO 17A                                                                          PIACENZA                      IT 005ITALY                                                                           SIMONA BAZZONI MS                                 29100    PC                                                                                                                                                                                       000                                                                                                                                                                                                                                                                                                                                    000                                                                                                                                                                                                             201012122359WW   000                                 000                              KL AMS  20101212                                                       07446126371         0878    BKK                      A0A O  S                 USD                                               TH 680THAILAND                      BANGKOK  AIR HUB              999 M 7  RACHATHEWA           SUVARNABHUMI INT L CUSTOMS    T201 WFS-PG CARGO BUILDIN     10540    BANG PHLI                     TNT EXPRESS WORLDWIDE CO  LTD                                                                 BANGKOK                       XDCSFM";
	}
}
