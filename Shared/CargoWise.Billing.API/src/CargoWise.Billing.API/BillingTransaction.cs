using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CargoWise.Billing.API
{
	[Serializable]
	public sealed class BillingTransaction
	{

		public int Version { get; set; }

		[Required]
		[StringLength(3)]
		[RegularExpression("^[a-zA-Z0-9-._]+$", ErrorMessage = "Field Category has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.")]
		public string Category { get; set; }

		[Required]
		[StringLength(3)]
		[RegularExpression("^[a-zA-Z0-9-._#]+$", ErrorMessage = "Field PriceItemCode has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, hash or underscore.")]
		public string PriceItemCode { get; set; }

		public int BillableCount { get; set; }

		[Required]
		[StringLength(3)]
		[RegularExpression("^[a-zA-Z0-9-._]+$", ErrorMessage = "Field ReportingSource has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.")]
		public string ReportingSource { get; set; }

		[Date(-5, 0, 0, 0, 1, 0, ErrorMessage = "The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.")]
		public DateTime ServiceOccuredUTC { get; set; }

		[Required]
		[StringLength(9)]
		[RegularExpression("^[a-zA-Z0-9-._?]+$", ErrorMessage = "Field ClientID has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, question mark or underscore.")]
		public string ClientID { get; set; }

		[StringLength(50)]
		[RegularExpression("^[a-zA-Z0-9-._]+$", ErrorMessage = "Field ClientNumber has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen or underscore.")]
		public string ClientNumber { get; set; }

		[StringLength(3)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field ClientStaffCode has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string ClientStaffCode { get; set; }

		[StringLength(3)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Branch has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string Branch { get; set; }

		[Required]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Reference1 has invalid characters. It must not contain ASCII control characters except whitespace.")]
		[StringLength(50)]
		public string Reference1 { get; set; }

		[StringLength(50)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Reference2 has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string Reference2 { get; set; }

		[StringLength(50)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Reference3 has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string Reference3 { get; set; }

		[StringLength(50)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Reference4 has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string Reference4 { get; set; }

		[StringLength(50)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field Reference5 has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string Reference5 { get; set; }

		[StringLength(36)]
		[RegularExpression(@"^[\u0009\u000A\u000D\u0020-\uFFFF]+$", ErrorMessage = "Field MessageTrackingID has invalid characters. It must not contain ASCII control characters except whitespace.")]
		public string MessageTrackingID { get; set; }

		[Description("At this stage, we do not need to insert this data into Billing DB so Category attribute is added to ignore comparing the property in tests. The Category attribute should be removed after the logic inserting property's value completed"), Category("Non-billed")]
		public string AdditionalRefs { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is BillingTransaction other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Version;
				hashCode = (hashCode * 397) ^ (Category != null ? Category.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (PriceItemCode != null ? PriceItemCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ BillableCount;
				hashCode = (hashCode * 397) ^ (ReportingSource != null ? ReportingSource.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ServiceOccuredUTC.GetHashCode();
				hashCode = (hashCode * 397) ^ (ClientID != null ? ClientID.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ClientNumber != null ? ClientNumber.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ClientStaffCode != null ? ClientStaffCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Branch != null ? Branch.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Reference1 != null ? Reference1.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Reference2 != null ? Reference2.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Reference3 != null ? Reference3.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Reference4 != null ? Reference4.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Reference5 != null ? Reference5.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (MessageTrackingID != null ? MessageTrackingID.GetHashCode() : 0);
				return hashCode;
			}
		}

		bool Equals(BillingTransaction other)
		{
			return
				Version == other.Version &&
				BillableCount == other.BillableCount &&
				ServiceOccuredUTC.Equals(other.ServiceOccuredUTC) &&
				string.Equals(Category, other.Category) &&
				string.Equals(PriceItemCode, other.PriceItemCode) &&
				string.Equals(ReportingSource, other.ReportingSource) &&
				string.Equals(ClientID, other.ClientID) &&
				string.Equals(ClientNumber, other.ClientNumber) &&
				string.Equals(ClientStaffCode, other.ClientStaffCode) &&
				string.Equals(Branch, other.Branch) &&
				string.Equals(Reference1, other.Reference1) &&
				string.Equals(Reference2, other.Reference2) &&
				string.Equals(Reference3, other.Reference3) &&
				string.Equals(Reference4, other.Reference4) &&
				string.Equals(Reference5, other.Reference5) &&
				string.Equals(MessageTrackingID, other.MessageTrackingID);
		}

		public override string ToString()
		{
			return string.Format("Version: {0}, Category: {1}, PriceItemCode: {2}, BillableCount: {3}, ReportingSource: {4}, ServiceOccuredUTC: {5}, ClientID: {6}, ClientNumber: {7}, ClientStaffCode: {8}, Branch: {9}, Reference1: {10}, Reference2: {11}, Reference3: {12}, Reference4: {13}, Reference5: {14}, MessageTrackingID: {15}, AdditionalRefs: {16}", Version, Category, PriceItemCode, BillableCount, ReportingSource, ServiceOccuredUTC.ToString("s"), ClientID, ClientNumber, ClientStaffCode, Branch, Reference1, Reference2, Reference3, Reference4, Reference5, MessageTrackingID, AdditionalRefs);
		}

	}
}
