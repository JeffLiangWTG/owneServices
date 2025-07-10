using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ReportingBookAccountingJournalPrintOption : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ReportingBook = "ReportingBook";
			public const string DisplayParentAccount = "DisplayParentAccount";
			public const string DisplayAttribute = "DisplayAttribute";
			public const string Default = "Default";
		}

		#endregion Schema

		public ReportingBookAccountingJournalPrintOption()
		{
		}

		public ReportingBookAccountingJournalPrintOption(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ReportingBookAccountingJournalPrintOption(fallbackLevel);
		}

		#region Properties

		[List("Lookups.AccReportingBookList")]
		public ZGuid ReportingBook
		{
			get => reportingBook;
			set
			{
				SetNonPersistentPropertyValue(ReportingBookInfo, ref reportingBook, value);
				accReportingBook = null;

				if (!IsValidationSuspended)
				{
					Validation.ValidateReportingBookPK();
				}
			}
		}
		ZGuid reportingBook = ZGuid.Empty;

		public ZPropertyInfo ReportingBookInfo
		{
			get { return GetZPropertyInfo(Schema.ReportingBook); }
		}

		AccReportingBook AccReportingBook => accReportingBook ?? (accReportingBook = CurrentFactory.Load<AccReportingBook>(ReportingBook));
		AccReportingBook accReportingBook;

		public ZString ReportingBookCode => AccReportingBook?.ARB_Code ?? ZString.Empty;

		public ZString ReportingBookDescription => AccReportingBook?.ARB_Description ?? ZString.Empty;

		public ZBool DisplayParentAccount
		{
			get => displayParentAccount;
			set
			{
				SetNonPersistentPropertyValue(DisplayParentAccountInfo, ref displayParentAccount, value);
			}
		}
		ZBool displayParentAccount = false;

		public ZPropertyInfo DisplayParentAccountInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayParentAccount); }
		}

		public ZBool DisplayAttribute
		{
			get => displayAttribute;
			set
			{
				SetNonPersistentPropertyValue(DisplayAttributeInfo, ref displayAttribute, value);
			}
		}
		ZBool displayAttribute = false;

		public ZPropertyInfo DisplayAttributeInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayAttribute); }
		}

		public ZBool Default
		{
			get => defaultValue;
			set
			{
				SetNonPersistentPropertyValue(DefaultInfo, ref defaultValue, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDefault();
				}
			}
		}
		ZBool defaultValue = false;

		public ZPropertyInfo DefaultInfo
		{
			get { return GetZPropertyInfo(Schema.Default); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public ReportingBookAccountingJournalPrintOptionValidation Validation => validation ?? (validation = new ReportingBookAccountingJournalPrintOptionValidation(this));
		ReportingBookAccountingJournalPrintOptionValidation validation;

		#endregion

		public ReportingBookAccountingJournalPrintOptionLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new ReportingBookAccountingJournalPrintOptionLookups(CurrentFallbackLevel?.CompanyPK(false) ?? Guid.Empty);
				}

				return lookups;
			}
		}
		ReportingBookAccountingJournalPrintOptionLookups lookups;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ReportingBook, ReportingBook.ToString());
			writer.WriteElementString(Schema.DisplayParentAccount, DisplayParentAccount.ToString());
			writer.WriteElementString(Schema.DisplayAttribute, DisplayAttribute.ToString());
			writer.WriteElementString(Schema.Default, Default.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReportingBook = new ZGuid(reader.ReadElementString(Schema.ReportingBook));
			DisplayParentAccount = reader.ReadElementStringAsZBool(Schema.DisplayParentAccount);
			DisplayAttribute = reader.ReadElementStringAsZBool(Schema.DisplayAttribute);
			Default = reader.ReadElementStringAsZBool(Schema.Default);
		}

		#endregion
	}
}
