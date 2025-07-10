using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.NZ
{
	sealed public class NZCFTALineItem : CertificateOfOriginLineItemDocDataObject
	{
		public NZCFTALineItem(object id)
			: base(id)
		{
		}

		#region QuantityUnit

		public ICodeDescription QuantityUnit
		{
			get => quantityUnit;
			set => quantityUnit = SetChild(quantityUnit, value);
		}

		ICodeDescription quantityUnit;

		#endregion

		#region QuantityNumber

		public ZInt QuantityNumber
		{
			get => quantityNumber;
			set
			{
				if (SetNonPersistentPropertyValue(QuantityNumberInfo, ref quantityNumber, value))
				{
					Validate(QuantityNumberInfo);
				}
			}
		}

		ZInt quantityNumber;

		public ZPropertyInfo QuantityNumberInfo => GetZPropertyInfo(nameof(QuantityNumber));

		#endregion
	}
}
