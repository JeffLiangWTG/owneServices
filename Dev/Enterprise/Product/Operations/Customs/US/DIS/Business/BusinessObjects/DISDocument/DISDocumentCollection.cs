using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISDocumentCollection : DISDocumentCollectionBase<DISDocument>
	{
		public DISDocumentCollection(DISHostWrapper wrapper)
			: base(wrapper)
		{
		}

		new DISHostWrapper wrapper => base.wrapper as DISHostWrapper;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var result = new DISDocument(wrapper);

			using (result.SuspendSettingHasChanges())
			{
				var itemWithMaxSuffix = this.Cast<DISDocument>().OrderByDescending(x => x.IDSuffix).FirstOrDefault();
				result.IDSuffix = (itemWithMaxSuffix != null ? itemWithMaxSuffix.IDSuffix : ZInt.Zero) + 1;
				if (wrapper.IsExport)
				{
					result.DefaultShipmentNo();
					result.CBPRequest.ID = MiscCBPRequestIDList.Codes.Unknown;
				}
			}
			return result;
		}

		protected override string XmlNamespace => "http://www.cargowise.com/Schemas/DISDocument";

		protected override string ApplciationCode => Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

		protected override IDISDocumentBase CreateElement() => new DISDocument(wrapper);
	}
}
