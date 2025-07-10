using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DIS.Business
{
	public class EDIMessageCollection : BusinessObjectCollection<EDIMessage>
	{
		public EDIMessageCollection(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		readonly DISDocument disDocument;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			if (disDocument.RequiredDocumentAddInfo != null)
			{
				result.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, disDocument.RequiredDocumentAddInfo.PK);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.USCustomsDIS);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}
	}
}
