using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml
{
	public class ValueObject : IValueObject
	{
		[XmlIgnore]
		public virtual bool IsSpecified
		{
			get { return fIsSpecified; }
			set { fIsSpecified = value; }
		}
		bool fIsSpecified = true;

		[XmlIgnore]
		public virtual bool ShouldCreateElementForEmptyValue { get; set; }
	}
}
