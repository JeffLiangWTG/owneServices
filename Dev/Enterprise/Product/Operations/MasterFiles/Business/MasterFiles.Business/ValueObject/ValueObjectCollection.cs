using System.Collections;
using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Subtypes used in XML serialization")]
	public abstract class ValueObjectCollection : CollectionBase, IValueObject
	{
		#region operator== and operator!=

		public static bool operator ==(ValueObjectCollection lhs, ValueObjectCollection rhs)
		{
			bool isLhsSpecified = (object)lhs != null && lhs.IsSpecified;
			bool isRhsSpecified = (object)rhs != null && rhs.IsSpecified;
			return
				(!isLhsSpecified && !isRhsSpecified) ||
				(isLhsSpecified && isRhsSpecified && lhs == (object)rhs);
		}

		public static bool operator !=(ValueObjectCollection lhs, ValueObjectCollection rhs)
		{
			return !(lhs == rhs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0060 error")]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0061 error")]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		[XmlIgnore]
		public virtual bool IsSpecified
		{
			get { return fIsSpecified || Count > 0; }
			set
			{
				if (!value)
				{
					Clear();
				}
				fIsSpecified = value;
			}
		}
		bool fIsSpecified;

		[XmlIgnore]
		public virtual bool ShouldCreateElementForEmptyValue { get; set; }
	}
}
