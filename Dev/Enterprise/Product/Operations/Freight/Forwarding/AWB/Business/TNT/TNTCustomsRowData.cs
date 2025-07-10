namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	public class TNTCustomsRowData : RowData
	{
		#region Page 2

		/// <summary>
		/// Record type code. Value 100. Standard information. (key).
		/// </summary>
		[FieldDefinition(0, 3)]
		public int RECORD_TYPE { get; private set; }

		/// <summary>
		/// Company id (key)
		/// </summary>
		[FieldDefinition(1, 2)]
		public string CON_COM_ID { get; private set; }

		/// <summary>
		/// Consignment id. (key)
		/// </summary>
		[FieldDefinition(2, 15)]
		public string CON_ID { get; private set; }

		/// <summary>
		/// Consignment sequence number (key)
		/// </summary>
		[FieldDefinition(3, 3)]
		public int CON_SEQ_ID { get; private set; }

		/// <summary>
		/// Article sequence number, system generated (key)
		/// </summary>
		[FieldDefinition(4, 3)]
		public int CNA_CON_SEQ_ID { get; private set; }

		/// <summary>
		/// Article connote sequence number, system generated
		/// </summary>
		[FieldDefinition(5, 3)]
		public int ADS_CNA_ID { get; private set; }

		/// <summary>
		/// Article description
		/// </summary>
		[FieldDefinition(6, 225)]
		public string ADS_DS { get; private set; }

		/// <summary>
		/// Article value
		/// </summary>
		[FieldDefinition(7, 11, 2)]
		public decimal CNA_ART_AM { get; private set; }

		/// <summary>
		/// Box 39 text
		/// </summary>
		[FieldDefinition(8, 3)]
		public string CNA_BOX39_TX { get; private set; }

		/// <summary>
		/// Certificate of Origin Type
		/// </summary>
		[FieldDefinition(9, 4)]
		public string CNA_CERT_ORIG_TYPE_CD { get; private set; }

		/// <summary>
		/// Certificate of Origin Number
		/// </summary>
		[FieldDefinition(10, 10)]
		public string CNA_CERT_ORIG_NR { get; private set; }

		/// <summary>
		/// Consignment article value currency code
		/// </summary>
		[FieldDefinition(11, 3)]
		public string CNA_CUY_ID_AM { get; private set; }

		/// <summary>
		/// X(03) Consignment article invoice currency code. (Space-filled.)
		/// </summary>
		[FieldDefinition(12, 3)]
		public string CNA_CUY_ID_INVOICE { get; private set; }

		/// <summary>
		/// X(03) Consignment article sample value currency code
		/// </summary>
		[FieldDefinition(13, 3)]
		public string CNA_CUY_ID_SAMPLE { get; private set; }

		/// <summary>
		/// X(03) Consignment article stats value currency code
		/// </summary>
		[FieldDefinition(14, 3)]
		public string CNA_CUY_ID_STATS { get; private set; }

		/// <summary>
		/// X(01) Export document code
		/// </summary>
		[FieldDefinition(15, 1)]
		public string CNA_EXPDOC_CD { get; private set; }

		/// <summary>
		/// X(30) 54 Consignment article export document city name
		/// </summary>
		[FieldDefinition(16, 30)]
		public string CNA_EXPDOC_CITY_NM { get; private set; }

		/// <summary>
		/// X(02) Consignment article export document date, century
		/// </summary>
		[FieldDefinition(17, 2)]
		public string CNA_EXPDOC_DT_CC { get; private set; }

		/// <summary>
		/// X(02) Consignment article export document date, year
		/// </summary>
		[FieldDefinition(18, 2)]
		public string CNA_EXPDOC_DT_YY { get; private set; }

		/// <summary>
		/// X(02) Consignment article export document date, month
		/// </summary>
		[FieldDefinition(19, 2)]
		public string CNA_EXPDOC_DT_MM { get; private set; }

		/// <summary>
		/// X(02) Consignment article export document date, day
		/// </summary>
		[FieldDefinition(20, 2)]
		public string CNA_EXPDOC_DT_DD { get; private set; }

		/// <summary>
		/// X(13) Consignment article export document number
		/// </summary>
		[FieldDefinition(21, 13)]
		public string CNA_EXPDOC_NR { get; private set; }

		/// <summary>
		/// X(04) 1 Consignment article export document type
		/// </summary>
		[FieldDefinition(22, 4)]
		public string CNA_EXPDOC_TYPE_CD { get; private set; }

		/// <summary>
		/// X(13) Contains the unofficial export declaration number provided by TNT to customs
		/// </summary>
		[FieldDefinition(23, 13)]
		public string CNA_EXPDOC_UNOFF_NR { get; private set; }

		/// <summary>
		/// 9(05)v999 35 Total gross weight
		/// </summary>
		[FieldDefinition(24, 5, 3)]
		public decimal CNA_GROSS_WT { get; private set; }

		/// <summary>
		/// X(04) Hazardous goods code
		/// </summary>
		[FieldDefinition(25, 4)]
		public string CNA_HAZ_CD { get; private set; }

		/// <summary>
		/// 9(03) Total number of items. (Zero_filled.)
		/// </summary>
		[FieldDefinition(26, 3)]
		public int CNA_ITEM_QT { get; private set; }

		/// <summary>
		/// X(10) 44 Number of export licence, required to export this article
		/// </summary>
		[FieldDefinition(27, 10)]
		public string CNA_LIC_ID { get; private set; }

		/// <summary>
		/// X(10) 31 Consignment article marks
		/// </summary>
		[FieldDefinition(28, 10)]
		public string CNA_MARKS_TX { get; private set; }

		/// <summary>
		/// 9(05)v999 38 Net weight of the article item(s)
		/// </summary>
		[FieldDefinition(29, 5, 3)]
		public decimal CNA_NET_WT { get; private set; }

		/// <summary>
		/// X(03) 34 The country of origin of an article on a consignment
		/// </summary>
		[FieldDefinition(30, 3)]
		public string CNA_ORIG_COU_ID { get; private set; }

		/// <summary>
		/// country code 9(03) 34 Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(31, 3)]
		public int CNA_ORIG_COU_Code { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) 16 Country description for article origin
		/// </summary>
		[FieldDefinition(32, 30)]
		public string CNA_ORIG_COU_DES { get; private set; }

		/// <summary>
		/// X(20) Kind of packing description. (Space_filled.)
		/// </summary>
		[FieldDefinition(33, 20)]
		public string CNA_PACK_DS { get; private set; }

		/// <summary>
		/// X(30) Consignment article previous customs document city name
		/// </summary>
		[FieldDefinition(34, 30)]
		public string CNA_PREVDOC_CITY_NM { get; private set; }

		/// <summary>
		/// X(02) Consignment article previous customs document date, century
		/// </summary>
		[FieldDefinition(35, 2)]
		public string CNA_PREVDOC_DT_CC { get; private set; }

		/// <summary>
		/// X(02) Consignment article previous customs document date, year
		/// </summary>
		[FieldDefinition(36, 2)]
		public string CNA_PREVDOC_DT_YY { get; private set; }

		/// <summary>
		/// X(02) Consignment article previous customs document date, month
		/// </summary>
		[FieldDefinition(37, 2)]
		public string CNA_PREVDOC_DT_MM { get; private set; }

		/// <summary>
		/// X(02) Consignment article previous customs document date, day
		/// </summary>
		[FieldDefinition(38, 2)]
		public string CNA_PREVDOC_DT_DD { get; private set; }

		/// <summary>
		/// X(13) 40 Previous customer document number
		/// </summary>
		[FieldDefinition(39, 13)]
		public string CNA_PREVDOC_NR { get; private set; }

		/// <summary>
		/// X(04) 40 Previous customer document type
		/// </summary>
		[FieldDefinition(40, 4)]
		public string CNA_PREVDOC_TYPE_CD { get; private set; }

		/// <summary>
		/// X(04) 37 Procedure Code 1
		/// </summary>
		[FieldDefinition(41, 4)]
		public string CNA_PROC_CD_1 { get; private set; }

		/// <summary>
		/// X(03) 37 Procedure Code 2
		/// </summary>
		[FieldDefinition(42, 3)]
		public string CNA_PROC_CD_2 { get; private set; }

		/// <summary>
		/// 9(11)v99 Sample value of an article for a consignment
		/// </summary>
		[FieldDefinition(43, 11, 2)]
		public decimal CNA_SAMPLE_AM { get; private set; }

		/// <summary>
		/// X(01) Separate indicator
		/// </summary>
		[FieldDefinition(44, 1)]
		public string CNA_SEP_IN { get; private set; }

		#endregion

		#region page 3
		/// <summary>
		/// X(01) Simplified indicator
		/// </summary>
		[FieldDefinition(45, 1)]
		public string CNA_SIMPL_IN { get; private set; }

		/// <summary>
		/// X(40) 44 Special mention text 1
		/// </summary>
		[FieldDefinition(46, 40)]
		public string CNA_SPEC_MEN_TX_1 { get; private set; }

		/// <summary>
		/// X(40) 44 Special mention text 2
		/// </summary>
		[FieldDefinition(47, 40)]
		public string CNA_SPEC_MEN_TX_2 { get; private set; }

		/// <summary>
		/// 9(11)v99 46 Statistical value
		/// </summary>
		[FieldDefinition(48, 11, 2)]
		public decimal CNA_STATS_AM { get; private set; }

		#endregion

		#region page 4

		/// <summary>
		///  X(30) Consignment article trans_shipment document city name
		/// </summary>
		[FieldDefinition(49, 30)]
		public string CN_TDOC_CITY_NM { get; private set; }

		/// <summary>
		/// X(02) Consignment article trans_shipment document date, century
		/// </summary>
		[FieldDefinition(50, 2)]
		public string CNA_TDOC_DT_CC { get; private set; }

		/// <summary>
		/// X(02) Consignment article trans_shipment document date, year
		/// </summary>
		[FieldDefinition(51, 2)]
		public string CNA_TDOC_DT_YY { get; private set; }

		/// <summary>
		/// X(02) Consignment article trans_shipment document date, month
		/// </summary>
		[FieldDefinition(52, 2)]
		public string CNA_TDOC_DT_MM { get; private set; }

		/// <summary>
		/// X(02) Consignment article trans_shipment document date, day
		/// </summary>
		[FieldDefinition(53, 2)]
		public string CNA_TDOC_DT_DD { get; private set; }

		/// <summary>
		/// X(15) 44 TDOC number
		/// </summary>
		[FieldDefinition(54, 15)]
		public string CNA_TDOC_NR { get; private set; }

		/// <summary>
		/// X(04) 44 Type of TDOC, as created on the local system
		/// </summary>
		[FieldDefinition(55, 4)]
		public string CNA_TDOC_TYPE_CD { get; private set; }

		/// <summary>
		/// X(15) Unofficial TDOC number
		/// </summary>
		[FieldDefinition(56, 15)]
		public string CNA_TDOC_UNOFF_NR { get; private set; }

		/// <summary>
		/// X(30) 33 Tariff Number (HTS) or Non_European equivalent for article on a consignment
		/// </summary>
		[FieldDefinition(57, 30)]
		public string CNA_TF_ID { get; private set; }

		/// <summary>
		/// X(02) 24 Transaction type
		/// </summary>
		[FieldDefinition(58, 2)]
		public string CNA_TRANS_TYPE_CD { get; private set; }

		/// <summary>
		/// 9(04) United Nations code associated with hazard class code
		/// </summary>
		[FieldDefinition(59, 4)]
		public string CNA_UN_CD { get; private set; }

		/// <summary>
		/// X(12) 41 Units description
		/// </summary>
		[FieldDefinition(60, 12)]
		public string CNA_UNITS_DS { get; private set; }

		/// <summary>
		/// 9(04) 41 Units number
		/// </summary>
		[FieldDefinition(61, 4)]
		public int CNA_UNITS_NR { get; private set; }

		/// <summary>
		/// 9(11)v99 Calculated: CNA_ART_AM + CNA_SAMPLE_AM
		/// </summary>
		[FieldDefinition(62, 11, 2)]
		public decimal CNA_VALUE_FOR_CUSTOMS { get; private set; }

		/// <summary>
		/// X(04) Actual Trade status
		/// </summary>
		[FieldDefinition(63, 4)]
		public string CON_ACT_TRADE_CD { get; private set; }

		/// <summary>
		/// X(05) Consignment origin
		/// </summary>
		[FieldDefinition(64, 5)]
		public string CON_BUL_ID_CON_ORIG { get; private set; }

		/// <summary>
		/// X(05) Location of clearance depot returned from Auto_allocation
		/// </summary>
		[FieldDefinition(65, 5)]
		public string CON_BUL_ID_CLR { get; private set; }

		/// <summary>
		/// X(05) Consignment creation business location
		/// </summary>
		[FieldDefinition(66, 5)]
		public string CON_BUL_ID_CREATE { get; private set; }

		/// <summary>
		/// X(05) Consignment destination
		/// </summary>
		[FieldDefinition(67, 5)]
		public string CON_BUL_ID_DEST { get; private set; }

		/// <summary>
		/// X(05) Location of documentation returned from Auto_allocation
		/// </summary>
		[FieldDefinition(68, 5)]
		public string CON_BUL_ID_DOC { get; private set; }

		/// <summary>
		/// X(05) Location of next depot returned from Auto_allocation
		/// </summary>
		[FieldDefinition(69, 5)]
		public string CON_BUL_ID_NEXT { get; private set; }

		/// <summary>
		/// X(24) Client reference text
		/// </summary>
		[FieldDefinition(70, 24)]
		public string CON_CLNT_REF_TX { get; private set; }

		/// <summary>
		/// 9(11)v99 Cash on delivery client value
		/// </summary>
		[FieldDefinition(71, 11, 2)]
		public decimal CON_COD_AM { get; private set; }

		/// <summary>
		/// X(15) Consignment id. Alternative Con id. Used currently for Airborne’s Con id
		/// </summary>
		[FieldDefinition(72, 15)]
		public string CON_CON_ID_ALT { get; private set; }

		/// <summary>
		/// X(01) Controlled indicator
		/// </summary>
		[FieldDefinition(73, 1)]
		public string CON_CONTROLLED_IN { get; private set; }

		/// <summary>
		/// X(03) Country id of goods origin
		/// </summary>
		[FieldDefinition(74, 3)]
		public string CON_COU_ID_GOODS_ORIG { get; private set; }

		/// <summary>
		/// 9(03) Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(75, 3)]
		public int CON_COU_CODE_GOODS_ORIG { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) Country description goods origin
		/// </summary>
		[FieldDefinition(76, 30)]
		public string CON_COU_DES_GOODS_ORIG { get; private set; }

		/// <summary>
		/// X(08) Creation Date
		/// </summary>
		[FieldDefinition(77, 8)]
		public string CON_CREATE_DATE { get; private set; }

		/// <summary>
		/// X(04) Creation time
		/// </summary>
		[FieldDefinition(78, 4)]
		public string CON_CREATE_TIME { get; private set; }

		/// <summary>
		/// X(03) 20 Customer delivery number (Terms)
		/// </summary>
		[FieldDefinition(79, 3)]
		public string CON_CUST_DELIV_CD { get; private set; }

		/// <summary>
		/// X(24) 20 Customer delivery city name
		/// </summary>
		[FieldDefinition(80, 24)]
		public string CON_CUST_DELIV_CITY_NM { get; private set; }

		/// <summary>
		/// X(04) Customer trade number, this relates to the agreements between trading zones(values 0_9) to find out the values use ‘grep TRADE XDGAV|more’
		/// </summary>
		[FieldDefinition(81, 4)]
		public string CON_CUST_TRADE_CD { get; private set; }

		/// <summary>
		/// X(03) 22 Currency code value of goods
		/// </summary>
		[FieldDefinition(82, 3)]
		public string CON_CUY_ID_VAL_OF_GOODS { get; private set; }

		/// <summary>
		/// X(03) Consignment insurance currency code
		/// </summary>
		[FieldDefinition(83, 3)]
		public string CON_CUY_ID_INS { get; private set; }

		/// <summary>
		/// X(1) Delivery instruction. (Space_filled.)
		/// </summary>
		[FieldDefinition(84, 1)]
		public string CON_DELIV_INSTR_CODE { get; private set; }

		/// <summary>
		/// X(60) Special instructions
		/// </summary>
		[FieldDefinition(85, 60)]
		public string CON_DELIV_INST_TX { get; private set; }

		/// <summary>
		/// X(02) Consignment depot action date, century: i.e. the latest date for receipt at the delivery depot
		/// </summary>
		[FieldDefinition(86, 2)]
		public string CON_DEPOT_ACTION_DT_CC { get; private set; }

		/// <summary>
		/// X(02) Consignment depot action date, year: i.e. the latest date for receipt at the delivery depot
		/// </summary>
		[FieldDefinition(87, 2)]
		public string CON_DEPOT_ACTION_DT_YY { get; private set; }

		/// <summary>
		/// X(02) Consignment depot action date, month: i.e. the latest date for receipt at the delivery depot
		/// </summary>
		[FieldDefinition(88, 2)]
		public string CON_DEPOT_ACTION_DT_MM { get; private set; }

		#endregion

		#region page 5

		/// <summary>
		/// X(02) Consignment depot action date, day: i.e. the latest date for receipt at the delivery depot
		/// </summary>
		[FieldDefinition(89, 2)]
		public string CON_DEPOT_ACTION_DT_DD { get; private set; }

		/// <summary>
		/// X(03) Consignment division id
		/// </summary>
		[FieldDefinition(90, 3)]
		public string CON_DIV_ID { get; private set; }

		/// <summary>
		/// X(01) Consignment document (Y/N) indicator
		/// </summary>
		[FieldDefinition(91, 1)]
		public string CON_DOCUMENT_IN { get; private set; }

		/// <summary>
		/// X(01) Required for GDOM, Indicates that the con is required for an exhibition
		/// </summary>
		[FieldDefinition(92, 1)]
		public string CON_EXHIBITION_IN { get; private set; }

		/// <summary>
		/// X(30) Contains invoice number and other invoice related details
		/// </summary>
		[FieldDefinition(93, 30)]
		public string CON_FINANCE_TX_1 { get; private set; }

		/// <summary>
		/// X(30) Contains invoice date
		/// </summary>
		[FieldDefinition(94, 30)]
		public string CON_FINANCE_TX_2 { get; private set; }

		/// <summary>
		/// X(30) Contains invoice related details
		/// </summary>
		[FieldDefinition(95, 30)]
		public string CON_FINANCE_TX_3 { get; private set; }

		/// <summary>
		/// X(30) Consignment goods description
		/// </summary>
		[FieldDefinition(96, 30)]
		public string CON_GOODS_DS { get; private set; }

		/// <summary>
		/// X(04) Hazardous goods code
		/// </summary>
		[FieldDefinition(97, 4)]
		public string CON_HAZ_CD { get; private set; }

		/// <summary>
		/// 9(11)V99 Consignment insurance amount
		/// </summary>
		[FieldDefinition(98, 11, 2)]
		public decimal CON_INS_AM { get; private set; }

		/// <summary>
		/// X(02) Service level code, air or road
		/// </summary>
		[FieldDefinition(99, 2)]
		public string CON_NET_SLVL_CD { get; private set; }

		#endregion

		#region Page 6

		/// <summary>
		/// 9(04)V999 Actual volume of consignment
		/// </summary>
		[FieldDefinition(100, 4, 3)]
		public decimal CON_OA_TOT_VL { get; private set; }

		/// <summary>
		/// 9(05)V999 Contractual gross weight of consignment Total contractual gross weight rounded 9(05) Rounded to nearest whole number. Up to 1 if less than 1.
		/// </summary>
		[FieldDefinition(101, 5, 3)]
		public decimal CON_OC_TGRS_WT { get; private set; }

		/// <summary>
		/// 9(05) Rounded to nearest whole number. Up to 1 if <1.
		/// </summary>
		[FieldDefinition(102, 5)]
		public int CON_TOTAL_CONTRACTUAL_GROSS_WEIGHT_ROUNDED { get; private set; }

		/// <summary>
		/// 9(04) Total item quantity, contractual
		/// </summary>
		[FieldDefinition(103, 4)]
		public int CON_OC_TITEM_QT { get; private set; }

		/// <summary>
		/// 9(03)V999 Contractual total volume of consignment
		/// </summary>
		[FieldDefinition(104, 3, 3)]
		public decimal CON_OC_TOT_VL { get; private set; }

		/// <summary>
		/// 9(05)V999 Consignment total gross weight 
		/// </summary>
		[FieldDefinition(105, 5, 3)]
		public decimal CON_OPSA_TGRS_WT { get; private set; }

		/// <summary>
		/// 9(05) Rounded to nearest whole number. Up to 1 if <1.
		/// </summary>
		[FieldDefinition(106, 5)]
		public int CON_TOTALACTUALGROSSWEIGHTROUNDED { get; private set; }

		/// <summary>
		/// X(03) Option code No 1
		/// </summary>
		[FieldDefinition(107, 3)]
		public string CON_OPTION_1 { get; private set; }

		/// <summary>
		/// X(03) Option code No 2
		/// </summary>
		[FieldDefinition(108, 3)]
		public string CON_OPTION_2 { get; private set; }

		/// <summary>
		/// X(03) Option code No 3
		/// </summary>
		[FieldDefinition(109, 3)]
		public string CON_OPTION_3 { get; private set; }

		/// <summary>
		/// X(03) Option code No 4
		/// </summary>
		[FieldDefinition(110, 3)]
		public string CON_OPTION_4 { get; private set; }

		/// <summary>
		/// X(02) Consignment packing code
		/// </summary>
		[FieldDefinition(111, 2)]
		public string CON_PACKING_CD { get; private set; }

		/// <summary>
		/// X(20) 31 Consignment packing description, used for ESAD document
		/// </summary>
		[FieldDefinition(112, 20)]
		public string CON_PACKING_DS { get; private set; }

		/// <summary>
		/// X(04) 7 Product indicator
		/// </summary>
		[FieldDefinition(113, 4)]
		public string CON_PRD_ID { get; private set; }

		/// <summary>
		/// X(01) Indicates that the paperwork for the consignment is complete
		/// </summary>
		[FieldDefinition(114, 1)]
		public string CON_PWORK_COMPL_IN { get; private set; }

		/// <summary>
		/// X(01) Indicates that the paperwork has been scanned.
		/// </summary>
		[FieldDefinition(115, 1)]
		public string CON_PWORK_SCAN_IN { get; private set; }

		/// <summary>
		/// X(1) Self collection indicator
		/// </summary>
		[FieldDefinition(116, 1)]
		public string CON_SELF_COLL_IND { get; private set; }

		/// <summary>
		/// X(01) TDOC print request has been set
		/// </summary>
		[FieldDefinition(117, 1)]
		public string CON_TDOC_PRT_IN { get; private set; }

		/// <summary>
		/// X(01) TDOC’s are separate from consignment indicator.
		/// </summary>
		[FieldDefinition(118, 1)]
		public string CON_TDOC_SEP_IN { get; private set; }

		/// <summary>
		/// X(01) Consignment payment type
		/// </summary>
		[FieldDefinition(119, 1)]
		public string CON_TOP_ID_OPS_ACT { get; private set; }

		/// <summary>
		/// 9(05) 31 Total number of pieces on consignment, actual
		/// </summary>
		[FieldDefinition(120, 5)]
		public int CON_TOT_PIECE_QT { get; private set; }

		/// <summary>
		/// 9(04) United Nations code associated with hazard class code
		/// </summary>
		[FieldDefinition(121, 4)]
		public int CON_UN_CD { get; private set; }

		/// <summary>
		/// 9(11)V999 Consignment value
		/// </summary>
		[FieldDefinition(122, 11, 3)]
		public decimal CON_VAL_OF_GOODS_AM { get; private set; }

		/// <summary>
		/// X(09) Account id
		/// </summary>
		[FieldDefinition(123, 9)]
		public string SEN_ACC_ID { get; private set; }

		/// <summary>
		/// X(03) Account group id
		/// </summary>
		[FieldDefinition(124, 3)]
		public string SEN_ACG_ID { get; private set; }

		/// <summary>
		/// X(30) 2 1st line of address
		/// </summary>
		[FieldDefinition(125, 30)]
		public string SEN_ADDR_1_DS { get; private set; }

		/// <summary>
		/// X(30) 2 2nd line of address
		/// </summary>
		[FieldDefinition(126, 30)]
		public string SEN_ADDR_2_DS { get; private set; }

		/// <summary>
		/// X(30) 2 3rd line of address
		/// </summary>
		[FieldDefinition(127, 30)]
		public string SEN_ADDR_3_DS { get; private set; }

		/// <summary>
		/// X(30) 2 City name
		/// </summary>
		[FieldDefinition(128, 30)]
		public string SEN_CITY_NM { get; private set; }

		/// <summary>
		/// X(03) 2/15a Country id of sender
		/// </summary>
		[FieldDefinition(129, 3)]
		public string SEN_COU_ID { get; private set; }

		/// <summary>
		/// 9(03) 15a Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(130, 3)]
		public int SEN_COU_CODE { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) 15 Country description
		/// </summary>
		[FieldDefinition(131, 30)]
		public string SEN_COU_DES { get; private set; }

		/// <summary>
		/// X(22) Sender contact name
		/// </summary>
		[FieldDefinition(132, 22)]
		public string SEN_CPF_LAST_NM { get; private set; }

		/// <summary>
		/// X(7) Sender telephone number
		/// </summary>
		[FieldDefinition(133, 7)]
		public string SEN_CPF_TEL_1_ID { get; private set; }

		/// <summary>
		/// X(9) Sender telephone number
		/// </summary>
		[FieldDefinition(134, 9)]
		public string SEN_CPF_TEL_2_ID { get; private set; }

		/// <summary>
		/// X(12) NAD id
		/// </summary>
		[FieldDefinition(135, 12)]
		public string SEN_NAD_ID { get; private set; }

		/// <summary>
		/// X(50) 2 Name
		/// </summary>
		[FieldDefinition(136, 50)]
		public string SEN_NM { get; private set; }

		/// <summary>
		/// X(09) 2 Postcode
		/// </summary>
		[FieldDefinition(137, 9)]
		public string SEN_PCODE_CD { get; private set; }

		/// <summary>
		/// X(30) Province name
		/// </summary>
		[FieldDefinition(138, 30)]
		public string SEN_PRV_NM { get; private set; }

		/// <summary>
		/// X(20) 2 VAT number
		/// </summary>
		[FieldDefinition(139, 20)]
		public string SEN_VAT_ID { get; private set; }

		/// <summary>
		/// X(09) Account id
		/// </summary>
		[FieldDefinition(140, 9)]
		public string REC_ACC_ID { get; private set; }

		/// <summary>
		/// X(03) Account group id
		/// </summary>
		[FieldDefinition(141, 3)]
		public string REC_ACG_ID { get; private set; }

		/// <summary>
		/// X(30) 8 1st line of address
		/// </summary>
		[FieldDefinition(142, 30)]
		public string REC_ADDR_1_DS { get; private set; }

		/// <summary>
		/// X(30) 8 2nd line of address
		/// </summary>
		[FieldDefinition(143, 30)]
		public string REC_ADDR_2_DS { get; private set; }

		/// <summary>
		/// X(30) 3rd line of address
		/// </summary>
		[FieldDefinition(144, 30)]
		public string REC_ADDR_3_DS { get; private set; }

		#endregion

		#region Page 7

		/// <summary>
		/// X(30) 8 City name
		/// </summary>
		[FieldDefinition(145, 30)]
		public string REC_CITY_NM { get; private set; }

		/// <summary>
		/// X(03) 17a Country id receiver
		/// </summary>
		[FieldDefinition(146, 3)]
		public string REC_COU_ID { get; private set; }

		/// <summary>
		/// 9(03) 17a Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(147, 3)]
		public int REC_COU_CODE { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) 8 / 17 Country description
		/// </summary>
		[FieldDefinition(148, 30)]
		public string REC_COU_DES { get; private set; }

		/// <summary>
		/// X(22) Receiver contact name
		/// </summary>
		[FieldDefinition(149, 22)]
		public string REC_CPF_LAST_NM { get; private set; }

		/// <summary>
		/// X(7) Receiver telephone number
		/// </summary>
		[FieldDefinition(150, 7)]
		public string REC_CPF_TEL_1_ID { get; private set; }

		/// <summary>
		/// X(9) Receiver telephone number
		/// </summary>
		[FieldDefinition(151, 9)]
		public string REC_CPF_TEL_2_ID { get; private set; }

		/// <summary>
		/// X(12) NAD id
		/// </summary>
		[FieldDefinition(152, 12)]
		public string REC_NAD_ID { get; private set; }

		/// <summary>
		/// X(50) 8 Name
		/// </summary>
		[FieldDefinition(153, 50)]
		public string REC_NM { get; private set; }

		/// <summary>
		/// X(09) 8 Postcode
		/// </summary>
		[FieldDefinition(154, 9)]
		public string REC_PCODE_CD { get; private set; }

		/// <summary>
		/// X(30) Province name
		/// </summary>
		[FieldDefinition(155, 30)]
		public string REC_PRV_NM { get; private set; }

		/// <summary>
		/// X(20) 8 VAT number
		/// </summary>
		[FieldDefinition(156, 20)]
		public string REC_VAT_ID { get; private set; }

		/// <summary>
		/// X(09) Account id
		/// </summary>
		[FieldDefinition(157, 9)]
		public string COL_ACC_ID { get; private set; }

		/// <summary>
		/// X(03) Account group id
		/// </summary>
		[FieldDefinition(158, 3)]
		public string COL_ACG_ID { get; private set; }

		#endregion

		#region Page 8

		/// <summary>
		/// X(30) 1st line of address
		/// </summary>
		[FieldDefinition(159, 30)]
		public string COL_ADDR_1_DS { get; private set; }

		/// <summary>
		/// X(30) 2nd line of address
		/// </summary>
		[FieldDefinition(160, 30)]
		public string COL_ADDR_2_DS { get; private set; }

		/// <summary>
		/// X(30) 3rd line of address
		/// </summary>
		[FieldDefinition(161, 30)]
		public string COL_ADDR_3_DS { get; private set; }

		/// <summary>
		/// X(30) City name
		/// </summary>
		[FieldDefinition(162, 30)]
		public string COL_CITY_NM { get; private set; }

		/// <summary>
		/// X(03) 15a Country id collection
		/// </summary>
		[FieldDefinition(163, 3)]
		public string COL_COU_ID { get; private set; }

		/// <summary>
		/// 9(03) 15a Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(164, 3)]
		public int COL_COU_CODE { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) 15 Country description
		/// </summary>
		[FieldDefinition(165, 30)]
		public string COL_COU_DES { get; private set; }

		/// <summary>
		/// X(22) Collection contact name
		/// </summary>
		[FieldDefinition(166, 22)]
		public string COL_CPF_LAST_NM { get; private set; }

		/// <summary>
		/// X(7) Collection telephone number
		/// </summary>
		[FieldDefinition(167, 7)]
		public string COL_CPF_TEL_1_ID { get; private set; }

		/// <summary>
		/// X(9) Collection telephone number
		/// </summary>
		[FieldDefinition(168, 9)]
		public string COL_CPF_TEL_2_ID { get; private set; }

		/// <summary>
		/// X(12) NAD id
		/// </summary>
		[FieldDefinition(169, 12)]
		public string COL_NAD_ID { get; private set; }

		/// <summary>
		/// X(50) Name
		/// </summary>
		[FieldDefinition(170, 50)]
		public string COL_NM { get; private set; }

		/// <summary>
		/// X(09) Postcode
		/// </summary>
		[FieldDefinition(171, 9)]
		public string COL_PCODE_CD { get; private set; }

		/// <summary>
		/// X(30) Province name
		/// </summary>
		[FieldDefinition(172, 30)]
		public string COL_PRV_NM { get; private set; }

		/// <summary>
		/// X(20) VAT number
		/// </summary>
		[FieldDefinition(173, 20)]
		public string COL_VAT_ID { get; private set; }

		/// <summary>
		/// X(09) Account id
		/// </summary>
		[FieldDefinition(174, 9)]
		public string DEL_ACC_ID { get; private set; }

		/// <summary>
		/// X(03) Account group id
		/// </summary>
		[FieldDefinition(175, 3)]
		public string DEL_ACG_ID { get; private set; }

		/// <summary>
		/// X(30) 1st line of address
		/// </summary>
		[FieldDefinition(176, 30)]
		public string DEL_ADDR_1_DS { get; private set; }

		/// <summary>
		/// X(30) 2nd line of address
		/// </summary>
		[FieldDefinition(177, 30)]
		public string DEL_ADDR_2_DS { get; private set; }

		/// <summary>
		/// X(30) 3rd line of address
		/// </summary>
		[FieldDefinition(178, 30)]
		public string DEL_ADDR_3_DS { get; private set; }

		/// <summary>
		/// X(30) City name
		/// </summary>
		[FieldDefinition(179, 30)]
		public string DEL_CITY_NM { get; private set; }

		/// <summary>
		/// X(03) Country id delivery
		/// </summary>
		[FieldDefinition(180, 3)]
		public string DEL_COU_ID { get; private set; }

		/// <summary>
		/// 9(03) Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(181, 3)]
		public int DEL_COU_CODE { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) Country description
		/// </summary>
		[FieldDefinition(182, 30)]
		public string DEL_COU_DES { get; private set; }

		/// <summary>
		/// X(22) Delivery contact name
		/// </summary>
		[FieldDefinition(183, 22)]
		public string DEL_CPF_LAST_NM { get; private set; }

		/// <summary>
		/// X(7) Delivery telephone number
		/// </summary>
		[FieldDefinition(184, 7)]
		public string DEL_CPF_TEL_1_ID { get; private set; }

		/// <summary>
		/// X(9) Delivery telephone number
		/// </summary>
		[FieldDefinition(185, 9)]
		public string DEL_CPF_TEL_2_ID { get; private set; }

		/// <summary>
		/// X(12) NAD id
		/// </summary>
		[FieldDefinition(186, 12)]
		public string DEL_NAD_ID { get; private set; }

		/// <summary>
		/// X(50) Name
		/// </summary>
		[FieldDefinition(187, 50)]
		public string DEL_NM { get; private set; }

		/// <summary>
		/// X(09) Postcode
		/// </summary>
		[FieldDefinition(188, 9)]
		public string DEL_PCODE_CD { get; private set; }

		/// <summary>
		/// X(30) Province name
		/// </summary>
		[FieldDefinition(189, 30)]
		public string DEL_PRV_NM { get; private set; }

		/// <summary>
		/// X(20) VAT number
		/// </summary>
		[FieldDefinition(190, 20)]
		public string DEL_VAT_ID { get; private set; }

		/// <summary>
		/// X(04) Space_filled.Yet to be given a Quantum name on the MOV file.
		/// </summary>
		[FieldDefinition(191, 4)]
		public string MOV_CUSTOMS_REF_NR { get; private set; }

		/// <summary>
		/// X(08) Actual arrival date keyed in by Quantum at flight receive time
		/// </summary>
		[FieldDefinition(192, 8)]
		public string MOV_ACT_ARR_DT { get; private set; }

		/// <summary>
		/// X(04) Actual arrival time keyed in by Quantum
		/// </summary>
		[FieldDefinition(193, 4)]
		public string MOV_ACT_ARR_TM { get; private set; }

		/// <summary>
		/// X(08) Actual departure date keyed in by Quantum
		/// </summary>
		[FieldDefinition(194, 8)]
		public string MOV_ACT_DEP_DT { get; private set; }

		/// <summary>
		/// X(04) Actual departure time keyed in by Quantum
		/// </summary>
		[FieldDefinition(195, 4)]
		public string MOV_ACT_DEP_TM { get; private set; }

		/// <summary>
		/// X(02) Company Id
		/// </summary>
		[FieldDefinition(196, 2)]
		public string MOV_COM_ID { get; private set; }

		/// <summary>
		/// (1) X(03) 18 Departure country id, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(197, 3)]
		public string MOV_COU_ID1 { get; private set; }

		/// <summary>
		/// 9(03) 18 Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(198, 3)]
		public string MOV_COU_CODE1 { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) Country description of departure
		/// </summary>
		[FieldDefinition(199, 30)]
		public string MOV_COU_DES1 { get; private set; }

		/// <summary>
		/// (2) X(03) 21 Departure country id, input by FIS linehaul , used for customs
		/// </summary>
		[FieldDefinition(200, 3)]
		public string MOV_COU_ID2 { get; private set; }

		/// <summary>
		/// 9(03) 21 Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(201, 3)]
		public string MOV_COU_CODE2 { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) Country description of departure
		/// </summary>
		[FieldDefinition(202, 30)]
		public string MOV_COU_DES2 { get; private set; }

		/// <summary>
		/// X(03) 7 This is the Carrier ie BA
		/// </summary>
		[FieldDefinition(203, 3)]
		public string MOV_CRR_ID { get; private set; }

		/// <summary>
		/// X(05) Destination of the movement
		/// </summary>
		[FieldDefinition(204, 5)]
		public string MOV_DEST_BUL_ID { get; private set; }

		#endregion

		#region Page 9

		/// <summary>
		/// X(08) The departure date as keyed in by the user
		/// </summary>
		[FieldDefinition(205, 8)]
		public string MOV_DEP_DT { get; private set; }

		/// <summary>
		/// X(1) 19 Container type code applies to all 4 containers
		/// </summary>
		[FieldDefinition(206, 1)]
		public string MOV_FIS_CONT_TYPE_CD { get; private set; }

		/// <summary>
		/// X(05) x4 31 Container number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(207, 5)]
		public string MOV_FIS_CONT_NR1 { get; private set; }

		/// <summary>
		/// X(05) x4 31 Container number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(208, 5)]
		public string MOV_FIS_CONT_NR2 { get; private set; }

		/// <summary>
		/// X(05) x4 31 Container number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(209, 5)]
		public string MOV_FIS_CONT_NR3 { get; private set; }

		/// <summary>
		/// X(05) x4 31 Container number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(210, 5)]
		public string MOV_FIS_CONT_NR4 { get; private set; }

		/// <summary>
		/// (1) X(02) 26 Departure type code added for short term use in linehaul (Voyage transport type )
		/// </summary>
		[FieldDefinition(211, 2)]
		public string MOV_FIS_TYPE_CD1 { get; private set; }

		/// <summary>
		/// (2) X(02) 25 Border type code added for short term use in linehaul (Voyage transport type)
		/// </summary>
		[FieldDefinition(212, 2)]
		public string MOV_FIS_TYPE_CD2 { get; private set; }

		/// <summary>
		/// (1) X(15) 18 Departure license plate number, input by FIS linehaul, used for customs (registration number of vehicle, very similar to TRANSPORT_ID), this subscript used to fill SECTRANSPORT_ID in Track and Trace.
		/// </summary>
		[FieldDefinition(213, 15)]
		public string MOV_LIC_PLATE_NR1 { get; private set; }

		#endregion

		#region Page 10

		/// <summary>
		/// (2) X(15) 21 Border license plate number, input by FIS linehaul, used for customs (registration number of vehicle, very similar to TRANSPORT_ID), this subscript used to fill SECTRANSPORT_ID in Track and Trace if first subscript empty)
		/// </summary>
		[FieldDefinition(214, 15)]
		public string MOV_LIC_PLATE_NR2 { get; private set; }

		/// <summary>
		/// X(15) The master airway bill entered in Q
		/// </summary>
		[FieldDefinition(215, 15)]
		public string MOV_MAWB_CD { get; private set; }

		/// <summary>
		/// X(05) All ways blank
		/// </summary>
		[FieldDefinition(216, 5)]
		public string MOV_NET_CD { get; private set; }

		/// <summary>
		/// X(08) 7 This is the flight number i e 1234 It has been agreed by all applications that 8th byte will never be used
		/// </summary>
		[FieldDefinition(217, 8)]
		public string MOV_NR { get; private set; }

		/// <summary>
		/// X(05) Origin of the flight
		/// </summary>
		[FieldDefinition(218, 5)]
		public string MOV_ORIG_BUL_ID { get; private set; }

		/// <summary>
		/// X(04) Code of the border being crossed by this movement
		/// </summary>
		[FieldDefinition(219, 4)]
		public string MOV_PORT_XING_CD { get; private set; }

		/// <summary>
		/// (1) X(08) Departure seal number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(220, 8)]
		public string MOV_SEAL_NR1 { get; private set; }

		/// <summary>
		/// (2) X(08) Border seal number, input by FIS linehaul, used for customs
		/// </summary>
		[FieldDefinition(221, 8)]
		public string MOV_SEAL_NR2 { get; private set; }

		/// <summary>
		/// X Type of sector i e Air or Road
		/// </summary>
		[FieldDefinition(222, 1)]
		public string MOV_SECTOR_TYPE_CD { get; private set; }

		/// <summary>
		/// 9(01) Starts at 0 thru to 9
		/// </summary>
		[FieldDefinition(223, 1)]
		public int MOV_SEQ_ID { get; private set; }

		/// <summary>
		/// X(02) Mode (A=TNT C=Carrier)
		/// </summary>
		[FieldDefinition(224, 2)]
		public string MOV_SMD_ID { get; private set; }

		/// <summary>
		/// X(03) Set to I or O for inbound or outbound
		/// </summary>
		[FieldDefinition(225, 3)]
		public string MOV_SUBTYPE_CD { get; private set; }

		/// <summary>
		/// X(03) The sector type either F(light) V(oyage) or S(sector) this will, in the future, have flight and voyage removed and other values like T(rip) and J(ourney)
		/// </summary>
		[FieldDefinition(226, 3)]
		public string MOV_TYPE_CD { get; private set; }

		/// <summary>
		/// X(15) Probably the same as LIC_PLATE_NR
		/// </summary>
		[FieldDefinition(227, 15)]
		public string MOV_TRANSPORT_ID { get; private set; }

		/// <summary>
		/// X(25) Default currency. Read on WWDEPOT DEFAULTSDECURR
		/// </summary>
		[FieldDefinition(228, 25)]
		public string LAV_VALUE_DS1 { get; private set; }

		/// <summary>
		/// X(25) Border crossing. Read on WWOCPORTBORD with LAVVALUE_ID = MOV_PORT_XING_ID
		/// </summary>
		[FieldDefinition(229, 25)]
		public string LAV_VALUE_DS2 { get; private set; }

		/// <summary>
		/// Xgrblt01:cou_id X(3) 14 Import Agent – Country
		/// </summary>
		[FieldDefinition(230, 3)]
		public string AGENT_COU_ID { get; private set; }

		/// <summary>
		/// 9(03) Numeric version of country code from table XNKIN02
		/// </summary>
		[FieldDefinition(231, 3)]
		public int AGENT_COU_CODE { get; private set; }

		/// <summary>
		/// Xgrltt01:lxt_nm X(30) Country description of agent
		/// </summary>
		[FieldDefinition(232, 30)]
		public string AGENT_COU_DES { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_nm X(30) 14 Name
		/// </summary>
		[FieldDefinition(233, 30)]
		public string BUL_NM { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_addr_1_ds X(30) 14 Address 1
		/// </summary>
		[FieldDefinition(234, 30)]
		public string BUL_ADDR_1_DS { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_addr_2_ds X(30) 14 Address 2
		/// </summary>
		[FieldDefinition(235, 30)]
		public string BUL_ADDR_2_DS { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_addr_3_ds X(30) Address 3
		/// </summary>
		[FieldDefinition(236, 30)]
		public string BUL_ADDR_3_DS { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_pcode_cd X(9) 14 Postcode
		/// </summary>
		[FieldDefinition(237, 9)]
		public string BUL_PCODE_CD { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_city_nm X(30) 14 City
		/// </summary>
		[FieldDefinition(238, 30)]
		public string BUL_CITY_NM { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_tnt_nm X(30) TNT name
		/// </summary>
		[FieldDefinition(239, 30)]
		public string BUL_TNT_NM { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_pbox_cd X(9) Post box code
		/// </summary>
		[FieldDefinition(240, 9)]
		public string BUL_PBOX_CD { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_pbox_city_nm X(30) Post box city name
		/// </summary>
		[FieldDefinition(241, 30)]
		public string BUL_PBOX_CITY_NM { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_pbox_pcode_cd X(9) Post box postcode code
		/// </summary>
		[FieldDefinition(242, 9)]
		public string BUL_PBOX_PCODE_CD { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_vat_id X(16) 14 Vat id
		/// </summary>
		[FieldDefinition(243, 16)]
		public string BUL_VAT_ID { get; private set; }

		/// <summary>
		/// Xgrblt01:bul_province_nm X(30) Province name
		/// </summary>
		[FieldDefinition(244, 30)]
		public string BUL_PROVINCE_NM { get; private set; }

		/// <summary>
		/// X(08) USER_ID (User who updated Con. If blank, User who added Con. If this is also blank, environment variable.)
		/// </summary>
		[FieldDefinition(245, 8)]
		public string REQUEST_USER { get; private set; }

		#endregion
	}
}
