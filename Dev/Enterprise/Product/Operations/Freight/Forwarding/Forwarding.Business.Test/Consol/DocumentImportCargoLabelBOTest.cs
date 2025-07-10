using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DocumentImportCargoLabel))]
	sealed class DocumentImportCargoLabelBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		#region DocumentImportCargoLabel

		DocumentImportCargoLabel DocumentImportCargoLabel
		{
			get
			{
				if (fDocumentImportCargoLabel == null)
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_OuterPacks = 7;

					fDocumentImportCargoLabel = new DocumentImportCargoLabel(shipment);
				}
				return fDocumentImportCargoLabel;
			}
		}
		DocumentImportCargoLabel fDocumentImportCargoLabel;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocumentImportCargoLabel;
		}

		#endregion
	}
}
