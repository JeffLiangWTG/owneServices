using System;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.VersionReport
{
	public class CompanyReport
	{
		public CompanyReport()
		{
		}

		public CompanyReport(GlbCompany company)
		{
			Address1 = company.GC_Address1;
			Address2 = company.GC_Address2;
			BusinessRegNo = company.GC_BusinessRegNo;
			BusinessRegNo2 = company.GC_BusinessRegNo2;
			City = company.GC_City;
			Code = company.GC_Code;
			CountryCode = company.GC_RN_NKCountryCode;
			CurrencyCode = company.GC_RX_NKLocalCurrency;
			CustomsRegistrationNo = company.GC_CustomsRegistrationNo;
			IsActive = company.GC_IsActive;
			IsGSTCashBasis = company.GC_IsGSTCashBasis;
			IsGSTRegistered = company.GC_IsGSTRegistered;
			IsReciprocal = company.GC_IsReciprocal;
			IsWHTCashBasis = company.GC_IsWHTCashBasis;
			IsWHTRegistered = company.GC_IsWHTRegistered;
			Name = company.GC_Name;
			Phone = company.GC_Phone;
			PK = company.PK.ToGuid();
			PostCode = company.GC_PostCode;
			State = company.GC_State;
			WebAddress = company.GC_WebAddress;
		}

		public string Code { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public Guid PK { get; set; }
		public string CountryCode { get; set; }
		public string CurrencyCode { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string PostCode { get; set; }
		public string State { get; set; }
		public string Phone { get; set; }
		public string BusinessRegNo { get; set; }
		public string BusinessRegNo2 { get; set; }
		public string CustomsRegistrationNo { get; set; }
		public string WebAddress { get; set; }
		public string Email { get; set; }
		public bool IsGSTRegistered { get; set; }
		public bool IsGSTCashBasis { get; set; }
		public bool IsWHTRegistered { get; set; }
		public bool IsWHTCashBasis { get; set; }
		public bool IsReciprocal { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void Parse(XmlTextReader xmlParser)
		{
			do
			{
				xmlParser.Read();
				switch (xmlParser.Name)
				{
					case CompanyNames.Code:
						Code = xmlParser.ReadString();
						break;
					case CompanyNames.Name:
						Name = xmlParser.ReadString();
						break;
					case CompanyNames.CountryCode:
						CountryCode = xmlParser.ReadString();
						break;
					case CompanyNames.CurrencyCode:
						CurrencyCode = xmlParser.ReadString();
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
					case CompanyNames.Phone:
						Phone = xmlParser.ReadString();
						break;
					case CompanyNames.BusinessRegNo:
						BusinessRegNo = xmlParser.ReadString();
						break;
					case CompanyNames.BusinessRegNo2:
						BusinessRegNo2 = xmlParser.ReadString();
						break;
					case CompanyNames.CustomsRegistrationNo:
						CustomsRegistrationNo = xmlParser.ReadString();
						break;
					case CompanyNames.WebAddress:
						WebAddress = xmlParser.ReadString();
						break;
					case CompanyNames.Email:
						Email = xmlParser.ReadString();
						break;

					case CompanyNames.IsActive:
						IsActive = ReadBool(xmlParser);
						break;
					case CompanyNames.IsGSTRegistered:
						IsGSTRegistered = ReadBool(xmlParser);
						break;
					case CompanyNames.IsGSTCashBasis:
						IsGSTCashBasis = ReadBool(xmlParser);
						break;
					case CompanyNames.IsWHTRegistered:
						IsWHTRegistered = ReadBool(xmlParser);
						break;
					case CompanyNames.IsWHTCashBasis:
						IsWHTCashBasis = ReadBool(xmlParser);
						break;
					case CompanyNames.IsReciprocal:
						IsReciprocal = ReadBool(xmlParser);
						break;

					case CompanyNames.PK:
						PK = SafeReadGuid(xmlParser);
						break;

					default:
						break;
				}
			}
			while (xmlParser.Name != VersionReport.ElementNames.Company);
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
			writer.WriteStartElement(VersionReport.ElementNames.Company);

			writer.WriteElementString(CompanyNames.Code, Code);
			writer.WriteElementString(CompanyNames.Name, Name);
			writer.WriteElementString(CompanyNames.CountryCode, CountryCode);
			writer.WriteElementString(CompanyNames.CurrencyCode, CurrencyCode);
			writer.WriteElementString(CompanyNames.Address1, Address1);
			writer.WriteElementString(CompanyNames.Address2, Address2);
			writer.WriteElementString(CompanyNames.City, City);
			writer.WriteElementString(CompanyNames.State, State);
			writer.WriteElementString(CompanyNames.PostCode, PostCode);
			writer.WriteElementString(CompanyNames.Phone, Phone);
			writer.WriteElementString(CompanyNames.BusinessRegNo, BusinessRegNo);
			writer.WriteElementString(CompanyNames.BusinessRegNo2, BusinessRegNo2);
			writer.WriteElementString(CompanyNames.CustomsRegistrationNo, CustomsRegistrationNo);
			writer.WriteElementString(CompanyNames.WebAddress, WebAddress);
			writer.WriteElementString(CompanyNames.Email, Email);

			writer.WriteElementString(CompanyNames.IsActive, BoolToString(IsActive));
			writer.WriteElementString(CompanyNames.IsGSTRegistered, BoolToString(IsGSTRegistered));
			writer.WriteElementString(CompanyNames.IsGSTCashBasis, BoolToString(IsGSTCashBasis));
			writer.WriteElementString(CompanyNames.IsWHTRegistered, BoolToString(IsWHTRegistered));
			writer.WriteElementString(CompanyNames.IsWHTCashBasis, BoolToString(IsWHTCashBasis));
			writer.WriteElementString(CompanyNames.IsReciprocal, BoolToString(IsReciprocal));

			writer.WriteElementString(CompanyNames.PK, XmlConvert.ToString(PK));

			writer.WriteEndElement();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element name is not to be translated")]
		static class CompanyNames
		{
			public const string Code = "Code";
			public const string Name = "Name";
			public const string IsActive = "IsActive";
			public const string PK = "PK";
			public const string CountryCode = "CountryCode";
			public const string CurrencyCode = "CurrencyCode";
			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string City = "City";
			public const string PostCode = "PostCode";
			public const string State = "State";
			public const string Phone = "Phone";
			public const string BusinessRegNo = "BusinessRegNo";
			public const string BusinessRegNo2 = "BusinessRegNo2";
			public const string CustomsRegistrationNo = "CustomsRegistrationNo";
			public const string WebAddress = "WebAddress";
			public const string Email = "Email";
			public const string IsGSTRegistered = "IsGSTRegistered";
			public const string IsGSTCashBasis = "IsGSTCashBasis";
			public const string IsWHTRegistered = "IsWHTRegistered";
			public const string IsWHTCashBasis = "IsWHTCashBasis";
			public const string IsReciprocal = "IsReciprocal";
		}
	}
}
