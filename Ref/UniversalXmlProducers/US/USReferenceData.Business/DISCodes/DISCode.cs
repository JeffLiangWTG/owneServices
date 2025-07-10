namespace CargoWise.RefDbRepo.USReferenceData.Business.DISCodes
{
	public class DISCode
	{
		public string AgencyCode
		{
			get
			{
				return agencyCode;
			}
			set
			{
				if (agencyCode == null)
				{
					agencyCode = value;
				}
				else
				{
					agencyCode += value;
				}
			}
		}
		string agencyCode;

		public string DocumentDescription
		{
			get
			{
				return documentDescription;
			}
			set
			{
				if (documentDescription == null)
				{
					documentDescription = value;
				}
				else
				{
					documentDescription += value;
				}
			}
		}
		string documentDescription;

		public string DocumentType
		{
			get
			{
				return documentType;
			}
			set
			{
				if (documentType == null)
				{
					documentType = value;
				}
				else
				{
					documentType += value;
				}
			}
		}
		string documentType;

		public string DocumentLabelCode
		{
			get
			{
				return documentLabelCode;
			}
			set
			{
				if (documentLabelCode == null)
				{
					documentLabelCode = value;
				}
				else
				{
					documentLabelCode += value;
				}
			}
		}
		string documentLabelCode;

		public string DocCode
		{
			get
			{
				return docCode;
			}
			set
			{
				if (docCode == null)
				{
					docCode = value;
				}
				else
				{
					docCode += value;
				}
			}
		}
		string docCode;

		public string Metadata
		{
			get
			{
				return metadata;
			}
			set
			{
				if (metadata == null)
				{
					metadata = value;
				}
				else
				{
					metadata += value;
				}
			}
		}
		string metadata;
	}
}
