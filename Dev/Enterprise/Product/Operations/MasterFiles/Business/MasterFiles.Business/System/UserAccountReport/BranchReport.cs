using System;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.UserAccountReport
{
	public class BranchReport
	{
		public BranchReport()
		{
		}

		public BranchReport(GlbBranch branch)
		{
			IsActive = branch.GB_IsActive;
			PK = branch.PK.ToGuid();
			Code = branch.GB_Code;
			BranchName = branch.GB_BranchName;
			CompanyName = branch.Company.GC_Name;
			CompanyCode = branch.Company.GC_Code;
			Address1 = branch.GB_Address1;
			Address2 = branch.GB_Address2;
			City = branch.GB_City;
			PostCode = branch.GB_PostCode;
			State = branch.GB_State;
			CountryCode = branch.GB_RN_NKCountryCode;
			Unloco = branch.GB_RL_NKHomePort;
			ValidationStatus = branch.GB_ValidationStatus;
		}

		public string Code { get; set; }
		public string BranchName { get; set; }
		public string CompanyName { get; set; }
		public string CompanyCode { get; set; }
		public bool IsActive { get; set; }
		public Guid PK { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string PostCode { get; set; }
		public string State { get; set; }
		public string CountryCode { get; set; }
		public string Unloco { get; set; }
		public string ValidationStatus { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Parse(XmlTextReader xmlParser)
		{
			do
			{
				xmlParser.Read();
				switch (xmlParser.Name)
				{
					case CompanyNames.IsActive:
						IsActive = ReadBool(xmlParser);
						break;
					case CompanyNames.PK:
						PK = SafeReadGuid(xmlParser);
						break;
					case CompanyNames.Code:
						Code = xmlParser.ReadString();
						break;
					case CompanyNames.BranchName:
						BranchName = xmlParser.ReadString();
						break;
					case CompanyNames.CompanyName:
						CompanyName = xmlParser.ReadString();
						break;
					case CompanyNames.CompanyCode:
						CompanyCode = xmlParser.ReadString();
						break;
					case CompanyNames.CountryCode:
						CountryCode = xmlParser.ReadString();
						break;
					case CompanyNames.Address1:
						Address1 = xmlParser.ReadString();
						break;
					case CompanyNames.Address2:
						Address2 = xmlParser.ReadString();
						break;
					case CompanyNames.City:
						City = xmlParser.ReadString();
						break;
					case CompanyNames.State:
						State = xmlParser.ReadString();
						break;
					case CompanyNames.PostCode:
						PostCode = xmlParser.ReadString();
						break;
					case CompanyNames.Unloco:
						Unloco = xmlParser.ReadString();
						break;
					case CompanyNames.ValidationStatus:
						ValidationStatus = xmlParser.ReadString();
						break;

					default:
						break;
				}
			}
			while (xmlParser.Name != UserAccountReport.ElementNames.Branch);
		}

		static Guid SafeReadGuid(XmlReader xmlParser)
		{
			try
			{
				return new ZGuid(xmlParser.ReadString()).ToGuid();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return Guid.Empty;
			}
		}

		static bool ReadBool(XmlReader xmlParser)
		{
			return XmlConvert.ToBoolean(xmlParser.ReadString());
		}

		static string BoolToString(ZBool b)
		{
			return b ? "1" : "0";
		}

		internal void Write(XmlTextWriter writer)
		{
			writer.WriteStartElement(UserAccountReport.ElementNames.Branch);

			writer.WriteElementString(CompanyNames.IsActive, BoolToString(IsActive));
			writer.WriteElementString(CompanyNames.PK, XmlConvert.ToString(PK));
			writer.WriteElementString(CompanyNames.Code, Code);
			writer.WriteElementString(CompanyNames.BranchName, BranchName);
			writer.WriteElementString(CompanyNames.CompanyName, CompanyName);
			writer.WriteElementString(CompanyNames.CompanyCode, CompanyCode);
			writer.WriteElementString(CompanyNames.Address1, Address1);
			writer.WriteElementString(CompanyNames.Address2, Address2);
			writer.WriteElementString(CompanyNames.City, City);
			writer.WriteElementString(CompanyNames.State, State);
			writer.WriteElementString(CompanyNames.PostCode, PostCode);
			writer.WriteElementString(CompanyNames.CountryCode, CountryCode);
			writer.WriteElementString(CompanyNames.Unloco, Unloco);
			writer.WriteElementString(CompanyNames.ValidationStatus, ValidationStatus);

			writer.WriteEndElement();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element name is not to be translated")]
		static class CompanyNames
		{
			public const string Code = "Code";
			public const string BranchName = "BranchName";
			public const string CompanyName = "CompanyName";
			public const string CompanyCode = "CompanyCode";
			public const string IsActive = "IsActive";
			public const string PK = "PK";
			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string City = "City";
			public const string PostCode = "PostCode";
			public const string State = "State";
			public const string CountryCode = "CountryCode";
			public const string Unloco = "Unloco";
			public const string ValidationStatus = "ValidationStatus";
		}
	}
}
