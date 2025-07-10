using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousDocumentWrapper : IPreviousDocument
	{
		readonly ZString id;
		readonly ZInt lineNumeric;

		public PreviousDocumentWrapper(ZString id)
			: this(id, 0)
		{
		}

		public PreviousDocumentWrapper(ZString id, ZInt lineNumeric)
		{
			this.id = id;
			this.lineNumeric = lineNumeric;
		}

		ZString IPreviousDocument.ID => id;

		ZInt IPreviousDocument.LineNumeric => lineNumeric;

		#region Not Applicable

		ZString IPreviousDocument.FunctionalReferenceID => ZString.Empty;

		#endregion
	}
}
