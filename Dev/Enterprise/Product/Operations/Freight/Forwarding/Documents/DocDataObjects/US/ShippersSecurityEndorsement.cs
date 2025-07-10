using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class ShippersSecurityEndorsement : DocDataObject, IDataSourceProvider
	{
		public ShippersSecurityEndorsement(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region Properties

		#region Consignor

		public Address Consignor
		{
			get => consignor;
			set => consignor = SetChild(consignor, value);
		}

		Address consignor;

		#endregion

		#region DocumentDate

		public ZDateTime DocumentDate
		{
			get => documentDate;
			set
			{
				if (SetNonPersistentPropertyValue(DocumentDateInfo, ref documentDate, value))
				{
					Validate(DocumentDateInfo);
				}
			}
		}
		ZDateTime documentDate;

		public ZPropertyInfo DocumentDateInfo => GetZPropertyInfo(nameof(DocumentDate));

		#endregion

		#region ForwardingAgentReference

		public ZString ForwardingAgentReference
		{
			get => forwardingAgentReference;
			set
			{
				if (SetNonPersistentPropertyValue(ForwardingAgentReferenceInfo, ref forwardingAgentReference, value))
				{
					Validate(ForwardingAgentReferenceInfo);
				}
			}
		}
		ZString forwardingAgentReference;

		public ZPropertyInfo ForwardingAgentReferenceInfo => GetZPropertyInfo(nameof(ForwardingAgentReference));

		#endregion

		#region SignatureText

		public ZString SignatureText
		{
			get => signatureText;
			set
			{
				if (SetNonPersistentPropertyValue(SignatureTextInfo, ref signatureText, value))
				{
					Validate(SignatureTextInfo);
				}
			}
		}
		ZString signatureText;

		public ZPropertyInfo SignatureTextInfo => GetZPropertyInfo(nameof(SignatureText));

		#endregion

		#region IndividualName

		public ZString IndividualName
		{
			get => individualName;
			set
			{
				if (SetNonPersistentPropertyValue(IndividualNameInfo, ref individualName, value))
				{
					Validate(IndividualNameInfo);
				}
			}
		}
		ZString individualName;

		public ZPropertyInfo IndividualNameInfo => GetZPropertyInfo(nameof(IndividualName));

		#endregion

		#region WarningText

		public ZString WarningText
		{
			get => warning;
			set
			{
				if (SetNonPersistentPropertyValue(WarningTextInfo, ref warning, value))
				{
					Validate(WarningTextInfo);
				}
			}
		}
		ZString warning;

		public ZPropertyInfo WarningTextInfo => GetZPropertyInfo(nameof(WarningText));

		#endregion

		#region HasWarning

		public ZBool HasWarning
		{
			get => hasWarning;
			set
			{
				if (SetNonPersistentPropertyValue(HasWarningInfo, ref hasWarning, value))
				{
					Validate(HasWarningInfo);
				}
			}
		}
		ZBool hasWarning;

		public ZPropertyInfo HasWarningInfo => GetZPropertyInfo(nameof(HasWarning));

		#endregion

		#endregion

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion IDataSourceProvider members
	}
}
