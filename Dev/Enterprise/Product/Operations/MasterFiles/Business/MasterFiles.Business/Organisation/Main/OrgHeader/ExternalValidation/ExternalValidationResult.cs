using System;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public enum ErrorSource
	{
		None = 0,
		External,
		Timeout,
		Cancelled,
		Other
	}

	/// <summary>
	/// Represents a piece of result data from external validation service.
	/// </summary>
	[XmlRoot("OrgStatus", Namespace = "http://www.wisetechglobal.com/EnterpriseService/", IsNullable = false)]
	public class ExternalValidationResult
	{
		/// <summary>
		/// Gets/Sets the validation value. ("Valid" for valid or "Invalid" for invalid)
		/// </summary>
		[XmlElement("Result")]
		public string Value { get; set; }

		/// <summary>
		/// Gets whether this is a valid validation result.
		/// </summary>
		[XmlIgnore]
		public bool IsValid
		{
			get
			{
				return String.Equals(Value, (NoResString)"Valid", StringComparison.OrdinalIgnoreCase);
			}
		}

		/// <summary>
		/// Gets whether this contains errors.
		/// </summary>
		[XmlIgnore]
		public bool HasErrors
		{
			get
			{
				return Errors != null && Errors.Length > 0;
			}
		}

		/// <summary>
		/// Gets whether this contains warnings.
		/// </summary>
		[XmlIgnore]
		public bool HasWarnings
		{
			get
			{
				return Warnings != null && Warnings.Length > 0;
			}
		}

		/// <summary>
		/// Gets/Sets the collection of errors.
		/// </summary>
		[XmlArray("ErrorLog")]
		[XmlArrayItem("ErrorItem")]
		public string[] Errors { get; set; }

		/// <summary>
		/// Gets/Sets the collection of warnings.
		/// </summary>
		[XmlArray("WarningLog")]
		[XmlArrayItem("WarningItem")]
		public string[] Warnings { get; set; }

		[XmlIgnore]
		public ErrorSource ErrorSource { get; set; }
	}
}