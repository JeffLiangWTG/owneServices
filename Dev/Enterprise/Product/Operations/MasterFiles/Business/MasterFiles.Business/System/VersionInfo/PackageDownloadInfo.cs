using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.VersionInfo
{
	public class PackageDownloadInfo
	{
		public PackageDownloadInfo(string xmlData)
		{
			try
			{
				using (MemoryStream newStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData)))
				{
					XmlTextReader reader = new XmlTextReader(newStream);

					reader.Read();
					reader.ReadStartElement();

					ZDateTime exeVersionDate;
					if (!ZDateTime.TryParseISO8601Date(reader.ReadElementString(ExeVersionDateElementName), out exeVersionDate))
					{
						throw new FormatException("Date format is not valid");
					}
					ZInt majorVersion = ZInt.Parse(reader.ReadElementString(MajorVersionElementName));
					ZInt minorVersion = ZInt.Parse(reader.ReadElementString(MinorVersionElementName));
					ZInt release = ZInt.Parse(reader.ReadElementString(ReleaseElementName));
					ZInt patch = ZInt.Parse(reader.ReadElementString(PatchElementName));
					Comment = reader.ReadElementString(CommentElementName);
					ForceDownload = new ZBool(reader.ReadElementString(ForceDownloadElementName));
					PackageURL = reader.ReadElementString(PackageURLElementName);

					VersionInfo = new PackageVersionInfo(exeVersionDate, majorVersion, minorVersion, release, patch);
					reader.ReadEndElement();
				}
			}
			catch (Exception ex)
			{
				throw new ArgumentException("XmlData is not valid!\n\n" + ex.Message + "\n\nXmlData:\n\n" + xmlData);
			}
		}

		public PackageVersionInfo VersionInfo { get; private set; }
		public ZString Comment { get; private set; }
		public ZBool ForceDownload { get; private set; }
		public ZString PackageURL { get; private set; }

		public string PackageFileName { get { return VersionInfo.PackageFileName; } }
		public ZDateTime ExeVersionDate { get { return VersionInfo.ExeVersionDate; } }
		public int MajorVersion { get { return VersionInfo.MajorVersion; } }
		public int MinorVersion { get { return VersionInfo.MinorVersion; } }
		public int Release { get { return VersionInfo.Release; } }
		public int Patch { get { return VersionInfo.Patch; } }

		public const string ExeVersionDateElementName = "ExeVersionDate";
		public const string MajorVersionElementName = "MajorVersion";
		public const string MinorVersionElementName = "MinorVersion";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public const string ReleaseElementName = "Release";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public const string PatchElementName = "Patch";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public const string CommentElementName = "Comment";
		public const string ForceDownloadElementName = "ForceDownload";
		public const string PackageURLElementName = "PackageURL";
	}
}
