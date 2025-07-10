using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	class MovementReferenceNumber : DocDataObject
	{
		public MovementReferenceNumber(object identifier)
			: base(identifier)
		{
		}

		#region MRN

		public ZString MRN
		{
			get => mrn;
			set
			{
				if (SetNonPersistentPropertyValue(MRNInfo, ref mrn, value))
				{
					Validate(MRNInfo);
				}
			}
		}

		ZString mrn;

		public ZPropertyInfo MRNInfo => GetZPropertyInfo(nameof(MRN));

		#endregion

		#region CustomsDocumentCode

		public ICodeDescription CustomsDocumentCode
		{
			get => customsDocumentCode;
			set => customsDocumentCode = SetChild(customsDocumentCode, value);
		}

		ICodeDescription customsDocumentCode;

		#endregion

		#region CustomsOfficeCode

		public ZString CustomsOfficeCode
		{
			get => customsOfficeCode;
			set
			{
				if (SetNonPersistentPropertyValue(CustomsOfficeCodeInfo, ref customsOfficeCode, value))
				{
					Validate(CustomsOfficeCodeInfo);
				}
			}
		}

		ZString customsOfficeCode;

		public ZPropertyInfo CustomsOfficeCodeInfo => GetZPropertyInfo(nameof(CustomsOfficeCode));

		#endregion
	}
}
