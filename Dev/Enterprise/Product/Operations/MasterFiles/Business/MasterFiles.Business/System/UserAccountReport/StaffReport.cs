using System.Xml;

namespace Enterprise.MasterFiles.Business.UserAccountReport
{
	public class StaffReport
	{
		public StaffReport()
		{
		}

		public StaffReport(GlbStaff staff)
		{
			Code = staff.GS_Code;
			IsActive = staff.GS_IsActive;
			Name = staff.GS_FullName;
			EmailAddress = staff.GS_EmailAddress;
			IsRobot = staff.GS_IsRobot;
			if (IsActive)
			{
				LanguageCode = staff.GS_WorkingLanguage;
				WorkPhone = staff.GS_WorkPhone;
				WorkPhoneExtension = staff.GS_WorkExtension;
				JobTitle = staff.GS_Title;
			}
		}

		public string Code { get; set; }
		public string Name { get; set; }
		public string EmailAddress { get; set; }
		public string LanguageCode { get; set; }
		public string BranchCode { get; set; }
		public string WorkPhone { get; set; }
		public string WorkPhoneExtension { get; set; }
		public string JobTitle { get; set; }
		public bool IsActive { get; set; }
		public bool IsRobot { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Parse(XmlTextReader xmlParser)
		{
			do
			{
				xmlParser.Read();
				switch (xmlParser.Name)
				{
					case StaffNames.IsActive:
						IsActive = XmlConvert.ToBoolean(xmlParser.ReadString());
						break;
					case StaffNames.IsRobot:
						IsRobot = XmlConvert.ToBoolean(xmlParser.ReadString());
						break;
					case StaffNames.Code:
						Code = xmlParser.ReadString();
						break;
					case StaffNames.Name:
						Name = xmlParser.ReadString();
						break;
					case StaffNames.EmailAddress:
						EmailAddress = xmlParser.ReadString();
						break;
					case StaffNames.LanguageCode:
						LanguageCode = xmlParser.ReadString();
						break;
					case StaffNames.BranchCode:
						BranchCode = xmlParser.ReadString();
						break;
					case StaffNames.WorkPhone:
						WorkPhone = xmlParser.ReadString();
						break;
					case StaffNames.WorkPhoneExtension:
						WorkPhoneExtension = xmlParser.ReadString();
						break;
					case StaffNames.JobTitle:
						JobTitle = xmlParser.ReadString();
						break;
					default:
						break;
				}
			}
			while (xmlParser.Name != UserAccountReport.ElementNames.Staff);
		}

		internal void Write(XmlTextWriter writer)
		{
			writer.WriteStartElement(UserAccountReport.ElementNames.Staff);

			writer.WriteElementString(StaffNames.IsActive, IsActive ? "1" : "0");
			writer.WriteElementString(StaffNames.IsRobot, IsRobot ? "1" : "0");
			writer.WriteElementString(StaffNames.Code, Code);
			writer.WriteElementString(StaffNames.Name, Name);
			writer.WriteElementString(StaffNames.EmailAddress, EmailAddress);
			writer.WriteElementString(StaffNames.LanguageCode, LanguageCode);
			writer.WriteElementString(StaffNames.BranchCode, BranchCode);
			writer.WriteElementString(StaffNames.WorkPhone, WorkPhone);
			writer.WriteElementString(StaffNames.WorkPhoneExtension, WorkPhoneExtension);
			writer.WriteElementString(StaffNames.JobTitle, JobTitle);

			writer.WriteEndElement();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element name is not to be translated")]
		static class StaffNames
		{
			public const string IsActive = "IsActive";
			public const string IsRobot = "IsRobot";
			public const string Code = "Code";
			public const string Name = "Name";
			public const string EmailAddress = "Email";
			public const string BranchCode = "Branch";
			public const string LanguageCode = "Language";
			public const string WorkPhone = "WorkPhone";
			public const string WorkPhoneExtension = "Extension";
			public const string JobTitle = "JobTitle";
		}
	}
}
