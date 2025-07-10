using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer
{
	public abstract class DeclarationDataImporter
	{
		public DeclarationDataImporter()
		{
		}

		public DeclarationDataImporter(BaseJobDeclaration toJobDec)
		{
			if (toJobDec == null)
			{
				throw new ArgumentNullException(nameof(toJobDec), "toJobDec argument is null");
			}
			this.factory = toJobDec.Factory;
			this.toJobDec = toJobDec;
			this.toJobDec.AutoCreateChargesBasedOnIncoTerm = false;
			this.supplierCodeMap = new NameValueCollection();
		}

		public virtual void Import()
		{
			CancelImport = false;
		}

		public virtual void Save()
		{
			factory.Save();
		}

		public string LogEntry;
		public bool CancelImport;

		public int PercentageComplete
		{
			get
			{
				int result = 100;
				if (TotalRecordCount > 0)
				{
					result = (int)((((float)ProcessedRecordCount) / TotalRecordCount) * 100);
				}
				return result;
			}
		}

		#region Abstract Methods

		public abstract int TotalRecordCount { get; }
		public abstract int ProcessedRecordCount { get; }
		public abstract int FailedRecordCount { get; }

		#endregion

		#region Events

		public event EventHandler LogEvent;
		protected void FireLogEvent(string logEntry)
		{
			this.LogEntry = logEntry;
			if (LogEvent != null)
			{
				LogEvent(this, new EventArgs());
			}
		}

		public event UnknownOrganisationCodeEventHandler UnknownOrganisationCodeFound;
		protected void FireUnknownOrganisationCodeFound(UnknownOrganisationCodeEventArgs e)
		{
			if (UnknownOrganisationCodeFound != null)
			{
				UnknownOrganisationCodeFound(this, e);
			}
		}

		#endregion

		#region Implementation

		protected bool FindOrganisation(ZString orgCode, out ZGuid result)
		{
			bool foundOrgCode = false;
			result = ZGuid.Empty;

			if (supplierCodeMap.Count > 0)
			{
				ZString tempOrgCode = (ZString)supplierCodeMap[orgCode];
				if (!tempOrgCode.IsEmpty)
				{
					orgCode = tempOrgCode;
				}
			}

			OrgHeader org = OrgHeader.LoadFromCode(factory, orgCode);
			if (org != null)
			{
				result = org.PK;
				foundOrgCode = true;
			}

			return foundOrgCode;
		}

		protected void SetPropertyInfoValue(ZPropertyInfo propertyInfo, string value)
		{
			if (propertyInfo.PropertyType == typeof(ZString))
			{
				propertyInfo.Value = ((ZString)value).Left(propertyInfo.MaxLength);
			}
			else if (propertyInfo.PropertyType == typeof(ZShort))
			{
				ZShort result = 0;
				if (ZShort.TryParse(value, out result))
				{
					propertyInfo.Value = result;
				}
			}
			else if (propertyInfo.PropertyType == typeof(ZInt))
			{
				ZInt result = 0;
				if (ZInt.TryParse(value, out result))
				{
					propertyInfo.Value = result;
				}
			}
			else if (propertyInfo.PropertyType == typeof(ZDateTime))
			{
				ZDateTime dateTimeValue;
				bool parsed = ZDateTime.TryParseISO8601Date(value, out dateTimeValue);
				if (parsed)
				{
					propertyInfo.Value = dateTimeValue;
				}
			}
			else if (propertyInfo.PropertyType == typeof(ZDecimal))
			{
				ZDecimal result = 0;
				if (ZDecimal.TryParse(value, out result))
				{
					propertyInfo.Value = result;
				}
			}
			else if (propertyInfo.PropertyType == typeof(ZBool))
			{
				propertyInfo.Value = (ZBool)(value.ToUpper() == "TRUE" || value.ToUpper() == "Y");
			}
		}

		protected BusinessObjectFactory factory;
		protected BaseJobDeclaration toJobDec;
		protected NameValueCollection supplierCodeMap;

		#endregion
	}
}
