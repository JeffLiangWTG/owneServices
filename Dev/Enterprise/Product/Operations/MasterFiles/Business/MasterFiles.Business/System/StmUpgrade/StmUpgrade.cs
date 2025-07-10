using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmUpgrade : AutoStmUpgrade
	{
		public StmUpgrade(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public abstract class StmUpgradeType
		{
			public const string EDP = "EDP";
			public const string CMM = "CMM";
			public const string CMC = "CMC";
		}

		public abstract class StmUpgradeStatus
		{
			public const string Applied = "APL";
			public const string CurrentVersion = "CUR";
			public const string Deleted = "DEL";
			public const string NotApplied = "NAP";
			public const string Obsolete = "OBS";
			public const string Ready = "RDY";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This string is used to parse a file name pattern")]
		public const string EDPFileStartTag = "Package";
		public const string EDPFileExtension = ".edp";
		const string CMMFileExtension = "main.tar.gz";
		const string CMCFileExtension = "change.tar.gz";

		#region Schema

		public new abstract class Schema : AutoStmUpgrade.Schema
		{
			public const string Size = "Size";
		}
		#endregion

		#endregion

		#region Update Version Details

		public void UpdateVersionDetails(string eDPFileName)
		{
			if (eDPFileName.ToUpper().StartsWith(EDPFileStartTag.ToUpper()) && eDPFileName.ToUpper().EndsWith(EDPFileExtension.ToUpper()))
			{
				SZ_Type = StmUpgradeType.EDP;

				var info = new VersionInfo.PackageVersionInfo(eDPFileName);

				SZ_ExeVersionDate = info.ExeVersionDate;
				SZ_MajorVersion = info.MajorVersion;
				SZ_MinorVersion = info.MinorVersion;
				SZ_Release = info.Release;
				SZ_Patch = info.Patch;
			}
			else if (eDPFileName.ToUpper().EndsWith(CMMFileExtension.ToUpper()))
			{
				SZ_Type = StmUpgradeType.CMM;
				SetCMRVersionDetails(eDPFileName);
			}
			else if (eDPFileName.ToUpper().EndsWith(CMCFileExtension.ToUpper()))
			{
				SZ_Type = StmUpgradeType.CMC;
				SetCMRVersionDetails(eDPFileName);
			}
		}

		void SetCMRVersionDetails(string fileName)
		{
			string[] fileNameParts = fileName.Split('-');

			if (fileNameParts.Length == 4)
			{
				SZ_MajorVersion = int.Parse(fileNameParts[0] + fileNameParts[1] + fileNameParts[2]);
				SZ_ExeVersionDate = CMRFudgeDate;
			}
		}

		readonly ZDateTime CMRFudgeDate = new ZDateTime(2020, 01, 01);

		#endregion

		#region Set Status

		public void SetStatus(string status)
		{
			SetStatus(status, "");
		}

		public void SetStatus(string status, string comment)
		{
			SZ_Status = status;
			SZ_StatusTime = ZDateTime.Now;
			SZ_StatusComment = comment;
		}

		#endregion

		#region Properties

		[List("Lookups.Statuses")]
		public override ZString SZ_Status
		{
			get { return base.SZ_Status; }
			set { base.SZ_Status = value; }
		}

		#region IsNotDeployable

		public bool IsNotDeployable
		{
			get
			{
				return (SZ_Status == StmUpgradeStatus.Deleted ||
					SZ_Status == StmUpgradeStatus.Obsolete);
			}
		}

		#endregion

		#region IsCMRReferenceFiles

		public bool IsCMRReferenceFiles
		{
			get { return SZ_Type == StmUpgradeType.CMC || SZ_Type == StmUpgradeType.CMM; }
		}

		#endregion

		#region Filename

		public ZString Filename
		{
			get
			{
				if (SZ_Type == StmUpgradeType.EDP)
				{
					return EDPFileStartTag + SZ_ExeVersionDate.ToString("yyyyMMdd_HHmmss") + "_" + SZ_MajorVersion + "_" + SZ_MinorVersion + "_" + SZ_Release + "_" + SZ_Patch + EDPFileExtension;
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region IsCurrentVersion

		public bool IsCurrentVersion
		{
			get { return SZ_Status == StmUpgradeStatus.CurrentVersion; }
		}

		public bool IsExecutingVersion
		{
			get { return (VersionNumber == ReleaseInfo.Instance.VersionNumber); }
		}

		#endregion

		#region Size

		public ZInt Size
		{
			get
			{
				if (!IsInDatabase)
				{
					return 0;
				}
				else
				{
					if (fSize == -1)
					{
						ZString query = "SELECT convert(int, DATALENGTH(" + Schema.SZ_UpgradeData_Compressed + ")) AS Length FROM " + Schema.TableName + " WHERE " + Schema.PK + " = @PK";
						ZSqlParameterCollection @params = new ZSqlParameterCollection();
						@params.Add("@PK", PK, StmUpgradeSchema.PK);
						DynamicBusinessObjectCollection result = new DynamicBusinessObjectCollection(Factory);
						result.Load(query, @params);
						if (result.Count > 0)
						{
							object length = result[0]["Length"];
							fSize = (ZInt)length;
						}
						else
						{
							fSize = 0;
						}
					}
					return fSize;
				}
			}
		}
		protected ZInt fSize = -1;

		public ZPropertyInfo SizeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Size);
			}
		}

		#endregion

		#region FullVersionString

		public ZString FullVersionString
		{
			get
			{
				if (fFullVersionString.IsEmpty)
				{
					fFullVersionString = SZ_MajorVersion + "." + SZ_MinorVersion + "." + SZ_Release + "." + SZ_Patch;
				}
				return fFullVersionString;
			}
		}
		ZString fFullVersionString;

		public ZPropertyInfo FullVersionStringInfo
		{
			get { return GetZPropertyInfo(nameof(FullVersionString)); }
		}

		#endregion

		#region StatusDescription

		[MaxLength(0)]
		public ZString StatusDescription
		{
			get
			{
				string currentStatus = (IsCurrentVersion) ? StmUpgradeStatus.CurrentVersion : (string)SZ_Status;
				return Lookups.Statuses.GetDescriptionFromCode(currentStatus);
			}
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDescription)); }
		}

		#endregion

		public bool IsOlderThanCurrentVersion
		{
			get { return ReleaseInfo.Instance.VersionNumber > VersionNumber; }
		}

		public VersionNumber VersionNumber
		{
			get { return new VersionNumber(SZ_MajorVersion, SZ_MinorVersion, SZ_Release, SZ_Patch); }
			set
			{
				SZ_MajorVersion = value.Major;
				SZ_MinorVersion = value.Minor;
				SZ_Release = value.Release;
				SZ_Patch = value.Patch;
			}
		}

		#endregion
	}
}
