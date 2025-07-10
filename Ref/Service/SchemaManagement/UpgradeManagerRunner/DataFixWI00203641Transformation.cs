using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00203641Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00203641Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = UpdateQuery();

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}

		static string UpdateQuery()
		{
			var sql = @"
UPDATE RefCountryStates SET RW_Description = N'Kunaṟ'							where RW_Description = 'Kuna?'							and RW_Code = 'KNR'	and RW_RN_NKCountryCode = 'AF'
UPDATE RefCountryStates SET RW_Description = N'Loṙi'							where RW_Description = 'Lo?i'							and RW_Code = 'LO'	and RW_RN_NKCountryCode = 'AM'
UPDATE RefCountryStates SET RW_Description = N'Aragac̣otn'						where RW_Description = 'Aragac?otn'						and RW_Code = 'AG'	and RW_RN_NKCountryCode = 'AM'
UPDATE RefCountryStates SET RW_Description = N'Cəlilabad'						where RW_Description = 'C?lilabad'						and RW_Code = 'CAL'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Kürdəmir'						where RW_Description = 'Kürd?mir'						and RW_Code = 'KUR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Gəncə'							where RW_Description = 'G?nc?'							and RW_Code = 'GA'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Şərur'							where RW_Description = 'S?rur'							and RW_Code = 'SAR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Lənkəran'						where RW_Description = 'L?nk?ran'						and RW_Code = 'LAN'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Tərtər'							where RW_Description = 'T?rt?r'							and RW_Code = 'TAR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Sədərək'							where RW_Description = 'S?d?r?k'						and RW_Code = 'SAD'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Kǝngǝrli'						where RW_Description = 'K?ng?rli'						and RW_Code = 'KAN'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Xocavənd'						where RW_Description = 'Xocav?nd'						and RW_Code = 'XVD'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Şəki'							where RW_Description = 'S?ki'							and RW_Code = 'SAK'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Ağcabədi'						where RW_Description = 'Agcab?di'						and RW_Code = 'AGC'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Siyəzən'							where RW_Description = 'Siy?z?n'						and RW_Code = 'SIY'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Zərdab'							where RW_Description = 'Z?rdab'							and RW_Code = 'ZAR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Kəlbəcər'						where RW_Description = 'K?lb?c?r'						and RW_Code = 'KAL'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Balakən'							where RW_Description = 'Balak?n'						and RW_Code = 'BAL'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Şəki'							where RW_Description = 'S?ki'							and RW_Code = 'SA'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Daşkəsən'						where RW_Description = 'Dask?s?n'						and RW_Code = 'DAS'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Babək'							where RW_Description = 'Bab?k'							and RW_Code = 'BAB'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Mingəçevir'						where RW_Description = 'Ming?çevir'						and RW_Code = 'MI'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Cəbrayıl'						where RW_Description = 'C?brayil'						and RW_Code = 'CAB'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Xankəndi'						where RW_Description = 'Xank?ndi'						and RW_Code = 'XA'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Biləsuvar'						where RW_Description = 'Bil?suvar'						and RW_Code = 'BIL'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Bərdə'							where RW_Description = 'B?rd?'							and RW_Code = 'BAR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Gədəbəy'							where RW_Description = 'G?d?b?y'						and RW_Code = 'GAD'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Beyləqan'						where RW_Description = 'Beyl?qan'						and RW_Code = 'BEY'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Qəbələ'							where RW_Description = 'Q?b?l?'							and RW_Code = 'QAB'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Şəmkir'							where RW_Description = 'S?mkir'							and RW_Code = 'SKR'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Lənkəran'						where RW_Description = 'L?nk?ran'						and RW_Code = 'LA'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Zəngilan'						where RW_Description = 'Z?ngilan'						and RW_Code = 'ZAN'	and RW_RN_NKCountryCode = 'AZ'
UPDATE RefCountryStates SET RW_Description = N'Al Muḩarraq'						where RW_Description = 'Al Mu?arraq'					and RW_Code = '15'	and RW_RN_NKCountryCode = 'BH'
UPDATE RefCountryStates SET RW_Description = N'Al Baḩr al Aḩmar'				where RW_Description = 'Al Ba?r al A?mar'				and RW_Code = 'BA'	and RW_RN_NKCountryCode = 'EG'
UPDATE RefCountryStates SET RW_Description = N'Al Buḩayrah'						where RW_Description = 'Al Bu?ayrah'					and RW_Code = 'BH'	and RW_RN_NKCountryCode = 'EG'
UPDATE RefCountryStates SET RW_Description = N'Semienawi K’eyyĭḥ Baḥri'			where RW_Description = 'Semienawi K’eyyi? Ba?ri'		and RW_Code = 'SK'	and RW_RN_NKCountryCode = 'ER'
UPDATE RefCountryStates SET RW_Description = N'Debubawi K’eyyĭḥ Baḥri'			where RW_Description = 'Debubawi K’eyyi? Ba?ri'			and RW_Code = 'DK'	and RW_RN_NKCountryCode = 'ER'
UPDATE RefCountryStates SET RW_Description = N'Kohgīlūyeh va Bowyer Aḩmad'		where RW_Description = 'Kohgiluyeh va Bowyer A?mad'		and RW_Code = '18'	and RW_RN_NKCountryCode = 'IR'
UPDATE RefCountryStates SET RW_Description = N'Chahār Maḩāl va Bakhtīārī'		where RW_Description = 'Chahar Ma?al va Bakhtiari'		and RW_Code = '08'	and RW_RN_NKCountryCode = 'IR'
UPDATE RefCountryStates SET RW_Description = N'Khorāsān-e Raẕavī'				where RW_Description = 'Khorasan-e Ra?avi'				and RW_Code = '30'	and RW_RN_NKCountryCode = 'IR'
UPDATE RefCountryStates SET RW_Description = N'Al Aḩmadī'						where RW_Description = 'Al A?madi'						and RW_Code = 'AH'	and RW_RN_NKCountryCode = 'KW'
UPDATE RefCountryStates SET RW_Description = N'Ḩawallī'							where RW_Description = '?awalli'						and RW_Code = 'HA'	and RW_RN_NKCountryCode = 'KW'
UPDATE RefCountryStates SET RW_Description = N'Næ̆gĕnahira paḷāta'				where RW_Description = 'Næ?genahira pa?ata'				and RW_Code = '5'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Basnāhira paḷāta'				where RW_Description = 'Basnahira pa?ata'				and RW_Code = '1'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Uturumæ̆da paḷāta'				where RW_Description = 'Uturumæ?da pa?ata'				and RW_Code = '7'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Vayamba paḷāta'					where RW_Description = 'Vayamba pa?ata'					and RW_Code = '6'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Uturu paḷāta'					where RW_Description = 'Uturu pa?ata'					and RW_Code = '4'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Dakuṇu paḷāta'					where RW_Description = 'Daku?u pa?ata'					and RW_Code = '3'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Sabaragamuva paḷāta'				where RW_Description = 'Sabaragamuva pa?ata'			and RW_Code = '9'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Madhyama paḷāta'					where RW_Description = 'Madhyama pa?ata'				and RW_Code = '2'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Ūva paḷāta'						where RW_Description = 'Uva pa?ata'						and RW_Code = '8'	and RW_RN_NKCountryCode = 'LK'
UPDATE RefCountryStates SET RW_Description = N'Al Jabal al Akhḑar'				where RW_Description = 'Al Jabal al Akh?ar'				and RW_Code = 'JA'	and RW_RN_NKCountryCode = 'LY'
UPDATE RefCountryStates SET RW_Description = N'Wādī ash Shāţiʾ'					where RW_Description = 'Wadi ash Shati?'				and RW_Code = 'WS'	and RW_RN_NKCountryCode = 'LY'
UPDATE RefCountryStates SET RW_Description = N'Al Wāḩāt'						where RW_Description = 'Al Wa?at'						and RW_Code = 'WA'	and RW_RN_NKCountryCode = 'LY'
UPDATE RefCountryStates SET RW_Description = N'Ash Shīḩānīyah'					where RW_Description = 'Ash Shi?aniyah'					and RW_Code = 'SH'	and RW_RN_NKCountryCode = 'QA'
UPDATE RefCountryStates SET RW_Description = N'Al Baḩr al Aḩmar'				where RW_Description = 'Al Ba?r al A?mar'				and RW_Code = 'RS'	and RW_RN_NKCountryCode = 'SD'
UPDATE RefCountryStates SET RW_Description = N'An Nīl al Abyaḑ'					where RW_Description = 'An Nil al Abya?'				and RW_Code = 'NW'	and RW_RN_NKCountryCode = 'SD'
UPDATE RefCountryStates SET RW_Description = N'Baḩr al Ghazāl'					where RW_Description = 'Ba?r al Ghazal'					and RW_Code = 'BG'	and RW_RN_NKCountryCode = 'TD'
UPDATE RefCountryStates SET RW_Description = N'Nam Ðịnh'						where RW_Description = 'Nam Ð?nh'						and RW_Code = '67'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bắc Giang'						where RW_Description = 'B?c Giang'						and RW_Code = '54'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bà Rịa - Vũng Tàu'				where RW_Description = 'Bà R?a - Vung Tàu'				and RW_Code = '43'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Quảng Bình'						where RW_Description = 'Qu?ng Bình'						and RW_Code = '24'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bình Thuận'						where RW_Description = 'Bình Thu?n'						and RW_Code = '40'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bình Phước'						where RW_Description = 'Bình Phu?c'						and RW_Code = '58'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bắc Kạn'							where RW_Description = 'B?c K?n'						and RW_Code = '53'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bình Định'						where RW_Description = 'Bình Ð?nh'						and RW_Code = '31'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Lạng Sơn'						where RW_Description = 'L?ng Son'						and RW_Code = '09'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Kiến Giang'						where RW_Description = 'Ki?n Giang'						and RW_Code = '47'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Nghệ An'							where RW_Description = 'Ngh? An'						and RW_Code = '22'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Hậu Giang'						where RW_Description = 'H?u Giang'						and RW_Code = '73'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Ðồng Nai'						where RW_Description = 'Ð?ng Nai'						and RW_Code = '39'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Hải Dương'						where RW_Description = 'H?i Duong'						and RW_Code = '61'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Điện Biên'						where RW_Description = 'Ði?n Biên'						and RW_Code = '71'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Đắk Nông'						where RW_Description = 'Ð?k Nông'						and RW_Code = '72'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bến Tre'							where RW_Description = 'B?n Tre'						and RW_Code = '50'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Lâm Ðồng'						where RW_Description = 'Lâm Ð?ng'						and RW_Code = '35'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Quảng Ninh'						where RW_Description = 'Qu?ng Ninh'						and RW_Code = '13'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Quảng Ngãi'						where RW_Description = 'Qu?ng Ngãi'						and RW_Code = '29'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bạc Liêu'						where RW_Description = 'B?c Liêu'						and RW_Code = '55'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Ninh Thuận'						where RW_Description = 'Ninh Thu?n'						and RW_Code = '36'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Phú Thọ'							where RW_Description = 'Phú Th?'						and RW_Code = '68'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Tiền Giang'						where RW_Description = 'Ti?n Giang'						and RW_Code = '46'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Đắk Lắk'							where RW_Description = 'Ð?k L?k'						and RW_Code = '33'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Cao Bằng'						where RW_Description = 'Cao B?ng'						and RW_Code = '04'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Quảng Trị'						where RW_Description = 'Qu?ng Tr?'						and RW_Code = '25'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Bắc Ninh'						where RW_Description = 'B?c Ninh'						and RW_Code = '56'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Quảng Nam'						where RW_Description = 'Qu?ng Nam'						and RW_Code = '27'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Thừa Thiên-Huế'					where RW_Description = 'Th?a Thiên-Hu?'					and RW_Code = '26'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Ðồng Tháp'						where RW_Description = 'Ð?ng Tháp'						and RW_Code = '45'	and RW_RN_NKCountryCode = 'VN'
UPDATE RefCountryStates SET RW_Description = N'Ḩaḑramawt'						where RW_Description = '?a?ramawt'						and RW_Code = 'HD'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Şanʻā'''							where RW_Description = 'San?a'''						and RW_Code = 'SN'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Aḑ Ḑāli‘'						where RW_Description = 'A? ?ali‘'						and RW_Code = 'DA'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Al Ḩudaydah'						where RW_Description = 'Al ?udaydah'					and RW_Code = 'HU'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Al Maḩwīt'						where RW_Description = 'Al Ma?wit'						and RW_Code = 'MW'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Ḩajjah'							where RW_Description = '?ajjah'							and RW_Code = 'HJ'	and RW_RN_NKCountryCode = 'YE'
UPDATE RefCountryStates SET RW_Description = N'Al Bayḑā’'						where RW_Description = 'Al Bay?a’'						and RW_Code = 'BA'	and RW_RN_NKCountryCode = 'YE'
";

			return sql;
		}
	}
}
