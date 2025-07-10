using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.UserAccountReport
{
	public class UserAccountReport
	{
		#region Construction / Extraction of Xml Data

		public UserAccountReport(IProductRegistrationKey regKey, DateTime lastRunTime)
		{
			databaseNumber = regKey.DatabaseNumber;
			isFullStaffList = SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.Value;
			AddBranches(lastRunTime);
			AddStaffs(lastRunTime);
			AddStaffCount();
			AddTotalNonDemoBranchCount();
			AddReportTimeUtc();
		}

		public UserAccountReport(string xML)
		{
			ExtractUserAccountInfo(xML);
		}

		void ExtractUserAccountInfo(ZString xML)
		{
			try
			{
				using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xML)))
				using (XmlTextReader xmlParser = new XmlTextReader(stream))
				{
					while (xmlParser.Read())
					{
						HandleDatabaseNumber(xmlParser);
						HandleBranchList(xmlParser);
						HandleStaffList(xmlParser);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = xML.IsEmpty ? (NoResString)"xmlData is empty." : string.Format((NoResString)"xmlData is not valid.\r\n\r\nxmlData:\r\n\r\n{0}", xML);
				throw new InvalidOperationException(message, ex);
			}
		}

		void HandleDatabaseNumber(XmlTextReader parser)
		{
			if (parser.Name == ElementNames.DatabaseNumber)
			{
				int.TryParse(parser.ReadString(), out databaseNumber);
			}
		}

		void HandleBranchList(XmlTextReader parser)
		{
			if (parser.Name == ElementNames.BranchList)
			{
				do
				{
					parser.Read();
					if (parser.Name == ElementNames.Branch)
					{
						var branchReport = new BranchReport();
						branchReport.Parse(parser);
						BranchList.Add(branchReport);
					}
				}
				while (parser.Name != ElementNames.BranchList);
			}
		}

		void HandleStaffList(XmlTextReader parser)
		{
			if (parser.Name == ElementNames.StaffList)
			{
				do
				{
					parser.Read();
					if (parser.Name == ElementNames.Staff)
					{
						var staffReport = new StaffReport();
						staffReport.Parse(parser);
						StaffList.Add(staffReport);
					}
				}
				while (parser.Name != ElementNames.StaffList);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { RefreshEnabled = false }); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region UserAccountReport.XML

		string reportXml;
		public ZString XML
		{
			get
			{
				if (reportXml == null)
				{
					using (MemoryStream stream = new MemoryStream())
					using (XmlTextWriter writer = new XmlTextWriter(stream, new UTF8Encoding()))
					{
						WriteXMLHeader(writer);
						WriteXMLBody(writer);
						WriteXMLFooter(writer);
						writer.Flush();
						reportXml = Encoding.UTF8.GetString(stream.ToArray());
					}
				}

				return reportXml;
			}
		}

		public void WriteXMLHeader(XmlTextWriter writer)
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteStartDocument();
		}

		public void WriteXMLBody(XmlTextWriter writer)
		{
			writer.WriteStartElement(ElementNames.UserAccountReport);
			writer.WriteElementString(ElementNames.DatabaseNumber, DatabaseNumber.ToString());
			writer.WriteElementString(ElementNames.ReportTimeUtc, ReportTimeUtc.ToString("yyyyMMdd_HHmm"));
			if (ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.UARCounts) != null)
			{
				writer.WriteElementString(ElementNames.ActiveStaffCount, ActiveStaffCount);
				writer.WriteElementString(ElementNames.TotalNonDemoBranchCount, TotalNonDemoBranchCount);
			}
			WriteBranches(writer);
			WriteStaff(writer);
		}

		public void WriteXMLFooter(XmlTextWriter writer)
		{
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		#endregion

		#region User Account Report Properties

		public List<BranchReport> BranchList
		{
			get { return branchList ?? (branchList = new List<BranchReport>()); }
		}
		List<BranchReport> branchList;

		public List<StaffReport> StaffList
		{
			get { return staffList ?? (staffList = new List<StaffReport>()); }
		}
		List<StaffReport> staffList;

		public ZDateTime ReportTimeUtc { get; set; }

		public string ActiveStaffCount { get; set; }

		public string TotalNonDemoBranchCount { get; set; }

		#endregion

		#region Element Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name - not a res string")]
		public static class ElementNames
		{
			public const string UserAccountReport = "UserAccountReport";
			public const string DatabaseNumber = "DatabaseNumber";
			public const string BranchList = "BranchList";
			public const string Branch = "Branch";
			public const string StaffList = "StaffList";
			public const string Staff = "Staff";
			public const string IsFullStaffList = "IsFullStaffList";
			public const string ReportTimeUtc = "ReportTimeUtc";
			public const string ActiveStaffCount = "ActiveStaffCount";
			public const string TotalNonDemoBranchCount = "TotalNonDemoBranchCount";
		}

		#endregion

		#region Database Number

		public int DatabaseNumber
		{
			get { return databaseNumber; }
		}
		int databaseNumber;

		#endregion

		#region Branch

		void WriteBranches(XmlTextWriter writer)
		{
			if (branchList != null && branchList.Count > 0)
			{
				writer.WriteStartElement(ElementNames.BranchList);
				foreach (var branch in branchList)
				{
					branch.Write(writer);
				}
				writer.WriteEndElement();
			}
		}

		void AddBranches(DateTime lastRunTime)
		{
			foreach (var branch in LoadBrancheList(lastRunTime))
			{
				var report = new BranchReport(branch);
				BranchList.Add(report);
			}
		}

		IEnumerable<GlbBranch> LoadBrancheList(DateTime lastRunTime)
		{
			var query = new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode);
			if (!isFullStaffList)
			{
				query.AddToFilter(GlbBranchSchema.GB_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastRunTime);
			}
			return Factory.Load<GlbBranch>(query);
		}

		void AddTotalNonDemoBranchCount()
		{
			var query = new ZQuery(GlbBranchSchema.GB_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode);
			TotalNonDemoBranchCount = Factory.GetDatabaseCount(typeof(GlbBranch), query).ToString();
		}

		#endregion

		#region Staff

		public ZBool IsFullStaffList
		{
			get { return isFullStaffList; }
		}
		readonly ZBool isFullStaffList;

		void AddStaffCount()
		{
			var query = new ZQuery();
			query.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			query.AddToFilter(GlbStaffSchema.GS_IsRobot, false);
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			ActiveStaffCount = Factory.GetDatabaseCount(typeof(GlbStaff), query).ToString();
		}

		void AddStaffs(DateTime lastRunTime)
		{
			var branches = Factory.Load<GlbBranch>(new ZQuery());
			var branchDict = branches.ToDictionary(x => x.PK);

			foreach (var staff in LoadStaffList(lastRunTime))
			{
				var report = new StaffReport(staff);
				var staffBranchPk = !staff.GS_GB_HomeBranch.IsEmpty ? staff.GS_GB_HomeBranch : staff.GS_GB_LastLogonBranch;
				if (branchDict.TryGetValue(staffBranchPk, out GlbBranch staffBranch))
				{
					report.BranchCode = staffBranch.GB_Code;
				}
				StaffList.Add(report);
			}

			if (isFullStaffList)
			{
				SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		IEnumerable<GlbStaff> LoadStaffList(DateTime lastRunTime)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			if (!isFullStaffList)
			{
				query.AddToFilter(GlbStaffSchema.GS_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastRunTime);
			}
			return Factory.Load<GlbStaff>(query);
		}

		void WriteStaff(XmlTextWriter writer)
		{
			if (staffList != null && staffList.Count > 0)
			{
				writer.WriteStartElement(ElementNames.StaffList);
				writer.WriteElementString(ElementNames.IsFullStaffList, IsFullStaffList ? "1" : "0");
				foreach (var staff in staffList)
				{
					staff.Write(writer);
				}
				writer.WriteEndElement();
			}
		}
		#endregion

		#region ReportTime

		void AddReportTimeUtc()
		{
			ReportTimeUtc = ZDateTime.UtcNow;
		}

		#endregion
	}
}
