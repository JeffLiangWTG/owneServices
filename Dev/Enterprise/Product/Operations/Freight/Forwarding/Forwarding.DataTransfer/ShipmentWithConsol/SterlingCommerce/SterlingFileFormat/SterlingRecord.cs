using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingRecord : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SterlingRecord(BusinessObjectFactory factory) : base(factory)
		{ }

		public SterlingRecord()
		{ }

		public SterlingRecord(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
		}

		StringBuilder sb;

		StringBuilder Sb
		{
			get
			{
				if (sb == null)
				{
					sb = new StringBuilder(RecordHeader);
				}
				return sb;
			}
		}

		protected SterlingCommerceConsolAndShipmentExporter Master;

		#region voids to override

		public virtual void GenerateRecord()
		{
			fRecord = RecordHeader;
			foreach (ZPropertyInfo property in this.PropertiesWithNotifications)
			{
				AddField(property.Value.ToString());
			}
			TerminateRecord();
		}

		#endregion

		#region Implementation

		public ZString ToTimeFormat(ZDateTime date)
		{
			return date.ToString("yyyy-MM-dd HH:mm:ss zzzzzz", CultureInfo.InvariantCulture);
		}

		public byte[] ToUTF8()
		{
			return new UTF8Encoding(true).GetBytes(Record);
		}

		public void AddField(ZString field)
		{
			field = Regex.Replace(field, @">|[\|\s]+", " ");
			Sb.Append(Delimiter);
			Sb.Append(field.Trim());
		}

		public void TerminateRecord()
		{
			Sb.Append(RecTerminator);
			Sb.Append("\r\n");
		}

		#region Statement

		public virtual ZString RecordHeader
		{
			get
			{
				return "RecordHeader";
			}
		}
		const string Delimiter = "|";
		const string RecTerminator = ">";

		#endregion

		#region Record

		internal ZString Record
		{
			get
			{
				if (fRecord.IsEmpty)
				{
					GenerateRecord();
					fRecord = Sb.ToString();
				}
				return fRecord;
			}
		}
		ZString fRecord;

		#endregion

		#endregion
	}
}
