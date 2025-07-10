using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	public class USDISDocumentIDsProvider : IUSDISDocumentIDsProvider
	{
		#region IDISDocumentIDsProvider Members

		public ICodeDescriptionPairList GetDocumentIDList(IUSDISHost host)
		{
			var wrapper = new DISHostWrapper(host);
			var result = new CodeDescriptionPairList();
			foreach (DISDocument document in wrapper.DISDocuments)
			{
				result.AddPair(document.DocumentID, document.DocumentDescription);
			}
			return result;
		}

		public bool HasBeenAccepted(IUSDISHost host, ZString documentLabel)
		{
			var wrapper = new DISHostWrapper(host);
			return wrapper.DISDocuments.Cast<DISDocument>().Any(x => x.DocumentLabel == documentLabel && x.Status == StatusList.Codes.COS);
		}
		#endregion
	}
}
