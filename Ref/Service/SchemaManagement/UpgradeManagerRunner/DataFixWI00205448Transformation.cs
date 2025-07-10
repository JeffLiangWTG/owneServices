using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00205448Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00205448Transformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sqlText = @"
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'EX') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'EXTREMADURA', '', 1, 'EX', 'ES')
END
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'CL') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'CASTILLA Y LEON', '', 1, 'CL', 'ES')
END
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'ES' AND RW_Code = 'CN') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'CANARIAS', '', 1, 'CN', 'ES')
END
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'FJ' AND RW_Code = 'W') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'Western', '', 1, 'W', 'FJ')
END
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'FJ' AND RW_Code = 'E') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'Eastern', '', 1, 'E', 'FJ')
END
IF ((select COUNT(*) FROM RefCountryStates where RW_RN_NKCountryCode = 'GR' AND RW_Code = '69') = 0)
BEGIN
	INSERT INTO RefCountryStates (RW_PK, RW_Description, RW_RegionName, RW_IsActive, RW_Code, RW_RN_NKCountryCode)
	VALUES(NEWID(), 'Agion Oros', '', 1, '69', 'GR')
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sqlText;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
