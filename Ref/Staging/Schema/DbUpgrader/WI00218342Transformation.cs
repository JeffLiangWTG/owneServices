using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00218342Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00218342Transformation(int version) : base(version) { }

		public void Run(IDbTransaction trans)
		{
			var sql = @"IF ((SELECT COUNT(*) FROM NamedEntityClassification WHERE NEC_Name = 'Brunei Darussalam') = 0)
BEGIN
INSERT INTO NamedEntityClassification(NEC_PK, NEC_Name, NEC_Class, NEC_Language, NEC_Code)
VALUES
(newid(), 'Brunei Darussalam', 'COUNTRY', 'EN', 'BN'),
(newid(), 'Bonaire, Sint Eustatius and Saba', 'COUNTRY', 'EN', 'BQ'),
(newid(), 'Congo, the Democratic Republic of T', 'COUNTRY', 'EN', 'CD'),
(newid(), 'Congo', 'COUNTRY', 'EN', 'CG'),
(newid(), 'Serbia and Montenegro', 'COUNTRY', 'EN', 'CS'),
(newid(), 'Micronesia, Federated States of', 'COUNTRY', 'EN', 'FM'),
(newid(), 'Iran, Islamic Republic of', 'COUNTRY', 'EN', 'IR'),
(newid(), 'Korea, Democratic People''s Republic', 'COUNTRY', 'EN', 'KP'),
(newid(), 'Korea, Republic of', 'COUNTRY', 'EN', 'KR'),
(newid(), 'Lao People''s Democratic Republic', 'COUNTRY', 'EN', 'LA'),
(newid(), 'Moldova, Republic of', 'COUNTRY', 'EN', 'MD'),
(newid(), 'Saint Martin (French Part)', 'COUNTRY', 'EN', 'MF'),
(newid(), 'Pitcairn', 'COUNTRY', 'EN', 'PN'),
(newid(), 'Russian Federation', 'COUNTRY', 'EN', 'RU'),
(newid(), 'Syrian Arab Republic', 'COUNTRY', 'EN', 'SY'),
(newid(), 'Taiwan, Province of China', 'COUNTRY', 'EN', 'TW'),
(newid(), 'Tanzania, United Republic of', 'COUNTRY', 'EN', 'TZ'),
(newid(), 'Virgin Islands, British', 'COUNTRY', 'EN', 'VG'),
(newid(), 'Virgin Islands, U.S.', 'COUNTRY', 'EN', 'VI'),
(newid(), 'International Waters Installations', 'COUNTRY', 'EN', 'XZ')
END
";
			DbHelper.ExecuteNonQuery(trans, sql, 0);
		}
	}
}
