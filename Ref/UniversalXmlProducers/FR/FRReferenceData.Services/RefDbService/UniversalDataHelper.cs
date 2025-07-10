using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.Utils;
using XmlWriter = CargoWise.RefDbRepo.Common.UniversalXmlWriter.XmlWriter;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public static class UniversalDataHelper
	{
		public static class Constants
		{
			public const string ImportFile = "I";
			public const string ExportFile = "E";
			public const string France = "FR";
			public const string DeltaIE = "DIE";
			public const string EuropeanUnion = "EUN";
			public const string RITA = "RITA";
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Yes = "Y";
			public const string No = "N";
			public const string _0 = "0";
			public const string _1 = "1";
			public const string Control = "CTRL";

			public static class AttributeDataTypes
			{
				public const string Boolean = "Boolean";
				public const string Decimal = "Decimal";
				public const string Integer = "Integer";
				public const string String = "String";
			}
		}

		public static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static string GetTagValue(XmlNode node, string tag)
		{
			XmlNode tagNode = node.SelectSingleNode(tag);
			if (tagNode == null)
			{
				return "";
			}
			else
			{
				return tagNode.InnerText.Trim();
			}
		}

		public static string GetTagBooleanValue(XmlNode node, string tag)
		{
			var tagValue = GetTagValue(node, tag);
			return tagValue switch
			{
				Constants._1 => Constants.Yes,
				Constants._0 => Constants.No,
				_ => throw new InvalidOperationException("Not a boolean number provided")
			};
		}

		public static DateTime GetStartDateFromTag(XmlNode node, string tagName)
		{
			return GetStartDateFromTag(node, tagName, MinimumDateTime);
		}

		public static DateTime GetStartDateFromTag(XmlNode node, string tagName, DateTime defaultDate)
		{
			DateTime result = GetDateFromTag(node, tagName, defaultDate);

			if (result < MinimumDateTime || result > MaximumDateTime)
			{
				result = MinimumDateTime;
			}

			return result;
		}

		public static DateTime GetEndDateFromTag(XmlNode node, string tagName)
		{
			return GetEndDateFromTag(node, tagName, MaximumDateTime);
		}

		public static DateTime GetEndDateFromTag(XmlNode node, string tagName, DateTime defaultDate)
		{
			DateTime result = GetDateFromTag(node, tagName, defaultDate);

			if (result < MinimumDateTime || result > MaximumDateTime)
			{
				result = MaximumDateTime;
			}

			return result;
		}

		public static DateTime GetDateFromTag(XmlNode node, string tagName, DateTime defaultValue)
		{
			DateTime result = defaultValue;

			var collectedDate = GetTagValue(node, tagName);
			if (!string.IsNullOrEmpty(collectedDate))
			{
				if (!DateTime.TryParseExact(collectedDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					result = defaultValue;
				}
			}

			return result;
		}
		public static bool CheckRateIsValid(decimal rate)
		{
			var result = true;

			if (rate == 0)
			{
				result = false;
			}

			return result;
		}

		public static bool CheckDatesAreValid(DateTime startDate, DateTime endDate)
		{
			var result = true;

			if (startDate > endDate)
			{
				result = false;
			}

			return result;
		}

		public static bool CheckDatesIncludeToday(RefCusCodeList data)
		{
			return CheckDatesIncludeToday(data.ZZD_StartDate, data.ZZD_EndDate);
		}

		public static bool CheckDatesIncludeToday(DateTime startDate, DateTime endDate)
		{
			var result = true;
			var current = DateTime.Today;

			if (current < startDate || endDate < current)
			{
				result = false;
			}
			return result;
		}

		public static List<RefCusCodeList> FilterOutputListByDate(List<RefCusCodeList> dataCollection)
		{
			return dataCollection.Where(x => CheckDatesAreValid(x.ZZD_StartDate, x.ZZD_EndDate))
				.OrderBy(x => x.ZZD_Code)
				.ThenByDescending(x => x.ZZD_EndDate)
				.GroupBy(x => x.ZZD_Code)
				.Select(x => x.FirstOrDefault(CheckDatesIncludeToday) ?? x.First())
				.ToList();
		}

		public static RefCusCodeListAttribute CreateRefCusCodeListAttribute(string name, string value)
		{
			return new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = name,
				ZZE_Value = value
			};
		}

		public static RefCusProcedureAttribute CreateRefCusProcedureAttribute(string name, string value)
		{
			return new RefCusProcedureAttribute()
			{
				ZXB_Name = name,
				ZXB_Value = value
			};
		}

		public static void SendEmail(string subject, string body)
		{
			var email = new Email
			{
				From = ApplicationConfig.Instance.EmailSender,
				To = ApplicationConfig.Instance.EmailRecipients,
				Subject = subject,
				Body = body
			};
			if (UnitTestDetector.IsRunningTests.Value)
			{
				SentEmails.Add(email);
			}
			else
			{
				RetryHelper.RetryWithDelay(() => { SendEmail(email); },
											TimeSpan.FromMilliseconds(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError),
											ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError);

			}
		}

		static void SendEmail(Email email)
		{
			using (var client = new SmtpClient(ApplicationConfig.Instance.EmailSmtpServer)
			{
				Credentials = new NetworkCredential(ApplicationConfig.Instance.EmailUsername, ApplicationConfig.Instance.EmailCredentialsPassword),
				Port = ApplicationConfig.Instance.EmailSmtpPort
			})
			{
				using (var message = new MailMessage())
				{
					message.From = new MailAddress(email.From);
					message.To.Add(email.To);
					message.Subject = email.Subject;
					message.Body = email.Body;

					client.Send(message);
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static List<Email> SentEmails = new List<Email>();

		public static void ExportToXml<T>(XmlWriter xmlWriter, IEnumerable<T> collection, string filePath)
		{
			foreach (var data in collection)
			{
				xmlWriter.PopulateData(data);
			}

			xmlWriter.SaveXml(filePath);
		}

		public static void InitializeWriter(XmlWriter xmlWriter, DateTime publishTime, string dataSource, UpdateType updateType, Dependency[] dependencies = null)
		{
			xmlWriter.SetDataSource(dataSource);
			xmlWriter.SetPublicationTime(publishTime);
			xmlWriter.SetUpdateType(updateType);
			if(dependencies != null)
			{
				foreach (var dependency in dependencies)
				{
					xmlWriter.SetDependency(dependency);
				}
			}
		}
	}
}
