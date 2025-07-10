using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.VersionInfo
{
	public class PackageVersionInfo
	{
		public ZDateTime ExeVersionDate { get; private set; }
		public int MajorVersion { get; private set; }
		public int MinorVersion { get; private set; }
		public int Release { get; private set; }
		public int Patch { get; private set; }

		public PackageVersionInfo(ZDateTime exeVersionDate, int majorVersion, int minorVersion, int release, int patch)
		{
			ExeVersionDate = exeVersionDate;
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
			Release = release;
			Patch = patch;
		}

		public PackageVersionInfo(string edpFileName)
		{
			Regex fileNamePattern = new Regex(@"^Package\d{4}\d{2}\d{2}_\d{2}\d{2}\d{2}_\d+_\d+_\d+_\d+.edp$", RegexOptions.IgnoreCase);
			if (fileNamePattern.IsMatch(edpFileName))
			{
				int prefixIndex = edpFileName.IndexOf(StmUpgrade.EDPFileStartTag, StringComparison.OrdinalIgnoreCase);
				int extensionIndex = edpFileName.LastIndexOf(StmUpgrade.EDPFileExtension, StringComparison.OrdinalIgnoreCase);

				if (prefixIndex == 0 && extensionIndex >= 30)
				{
					string fileNameToSplit = edpFileName.Substring(StmUpgrade.EDPFileStartTag.Length, extensionIndex - StmUpgrade.EDPFileStartTag.Length);
					string[] fileNameParts = fileNameToSplit.Split('_');

					if (fileNameParts.Length == 6)
					{
						ZInt majorVersion;
						ZInt minorVersion;
						ZInt release;
						ZInt patch;

						if (!ZInt.TryParse(fileNameParts[2], out majorVersion) ||
							!ZInt.TryParse(fileNameParts[3], out minorVersion) ||
							!ZInt.TryParse(fileNameParts[4], out release) ||
							!ZInt.TryParse(fileNameParts[5], out patch))
						{
							majorVersion = minorVersion = release = patch = 0;
						}

						ExeVersionDate = VersionDate(fileNameParts[0], fileNameParts[1]);
						MajorVersion = majorVersion;
						MinorVersion = minorVersion;
						Release = release;
						Patch = patch;
					}
				}
			}
		}

		public bool IsValid
		{
			get
			{
				return ExeVersionDate.IsValid && !ExeVersionDate.IsEmpty &&
					MajorVersion >= 0 && MinorVersion >= 0 && Release >= 0 && Patch >= 0;
			}
		}

		public string PackageFileName
		{
			get
			{
				return IsValid ? ZString.Format(StmUpgrade.EDPFileStartTag + "{0}_{1}_{2}_{3}_{4}" + StmUpgrade.EDPFileExtension,
					ExeVersionDate.ToString("yyyyMMdd_HHmm00", CultureInfo.InvariantCulture), MajorVersion, MinorVersion, Release, Patch) : new ZString((NoResString)"<Invalid>");
			}
		}

		static ZDateTime VersionDate(string datePart, string timePart)
		{
			ZDateTime result = ZDateTime.Invalid;
			if (datePart.Length == 8 && timePart.Length == 6)
			{
				ZDateTime.TryParseExact(datePart + timePart.Substring(0, 4), out result, "yyyyMMddHHmm");
			}
			return result;
		}
	}
}
