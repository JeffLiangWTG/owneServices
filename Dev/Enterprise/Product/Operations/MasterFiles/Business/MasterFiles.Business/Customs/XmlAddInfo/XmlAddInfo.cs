using System;
using System.Data;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business
{
	public abstract class XmlAddInfo : NonPersistentBusinessObject
	{
		protected XmlAddInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected ZPropertyInfoString ParentPropertyInfo
		{
			get { return fParentPropertyInfo; }
			set
			{
				if (fParentPropertyInfo != null && fParentPropertyInfo != value)
				{
					throw new InvalidOperationException("ParentPropertyInfo already has a value");
				}
				fParentPropertyInfo = value;
			}
		}
		ZPropertyInfoString fParentPropertyInfo;

		internal void Serialise()
		{
			if (ParentPropertyInfo == null)
			{
				ErrorReporter.ReportOnce("Parent propertyInfo is null", "Parent propertyInfo is null");
			}
			else
			{
				using (StringWriter stringWriter = new StringWriter())
				using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
				{
					bool hasWrittenStartTag = false;
					foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
					{
						if (propertyInfo.HasSetter && propertyInfo.IsPersistent && !propertyInfo.Value.IsDefault)
						{
							ZString stringRepresentation;
							if (propertyInfo.Value is ZDateTime)
							{
								ZDateTime dateTime = (ZDateTime)propertyInfo.Value;
								stringRepresentation = dateTime.IsValid ? dateTime.SqlFormat : ZString.Empty;
							}
							else
							{
								stringRepresentation = propertyInfo.Value.ToString();
							}

							if (!hasWrittenStartTag)
							{
								writer.WriteStartElement(ParentPropertyInfo.Name.Substring(3));
								hasWrittenStartTag = true;
							}

							writer.WriteElementString(propertyInfo.Name.Substring(3), stringRepresentation);

							GenAddOnColumnSynchroniser.Synchronise(ColumnsForFastSearch);
						}
					}

					if (hasWrittenStartTag)
					{
						writer.WriteEndElement();
					}

					ParentPropertyInfo.Value = stringWriter.ToString();
				}
			}
		}

		protected virtual SchemaColumn[] ColumnsForFastSearch
		{
			get { return Array.Empty<SchemaColumn>(); }
		}

		AddInfoGenAddOnColumnSynchroniser GenAddOnColumnSynchroniser
		{
			get { return synchroniser ?? (synchroniser = new AddInfoGenAddOnColumnSynchroniser(ParentPropertyInfo.BizObj, this)); }
		}
		AddInfoGenAddOnColumnSynchroniser synchroniser;

		public void Deserialise(bool clearAllPropertiesBeforeDeserialisation = true)
		{
			if (ParentPropertyInfo != null)
			{
				ZString addInfoStringToDeserialise = ParentPropertyInfo.Value;

				if (clearAllPropertiesBeforeDeserialisation)
				{
					ClearAllProperties();
				}

				if (!addInfoStringToDeserialise.IsEmpty)
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(addInfoStringToDeserialise);

					XmlNode root = doc.FirstChild;

					foreach (XmlNode xmlNode in root.ChildNodes)
					{
						ZPropertyInfo propertyInfo = null;

						string key = TablePrefix + xmlNode.Name;
						if (ZPropertyInfoHash.ContainsKey(key))
						{
							propertyInfo = ZPropertyInfoHash[key];
						}

						if (propertyInfo != null)
						{
							propertyInfo.SetValueFromString(xmlNode.InnerText);
						}
					}
				}
			}
		}

		protected void ClearAllProperties()
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.HasSetter && !info.Value.IsEmpty)
				{
					info.ClearValue();
				}
			}
		}

		public override bool IsInDatabase
		{
			get { return ParentPropertyInfo.BizObj.IsInDatabase; }
		}

		protected override void OnFactorySaving()
		{
			if (ParentPropertyInfo != null && !ParentPropertyInfo.BizObj.IsDeleted &&
				(HasChanges || !ParentPropertyInfo.BizObj.IsInDatabase))
			{
				Serialise();
			}
			base.OnFactorySaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				HasChanges = false;
			}
		}

		public override string TablePrefix
		{
			get
			{
				if (fTablePrefix == null)
				{
					fTablePrefix = ((INeedTable)this).Table.Columns[0].ColumnName.Substring(0, 3);
				}
				return fTablePrefix;
			}
		}
		string fTablePrefix;
	}
}
