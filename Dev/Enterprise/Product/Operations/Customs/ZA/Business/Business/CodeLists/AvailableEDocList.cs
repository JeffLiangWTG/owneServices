using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class AvailableEDocList : Customs.Business.AvailableEDocList
	{
		public AvailableEDocList(List<ZString> filter, params IStorageDocsBaseCollection[] eDocCollections) : base(filter, eDocCollections)
		{
		}

		protected override CodeDescriptionPair GetEDocListPair(IeDoc eDoc, IStorageMain bizO)
		{
			return new CodeDescriptionPair(bizO.DocumentOwnerDescription + "-" + eDoc.DocType.PadRight(3) + "-" + eDoc.FileName, Res.GetString("EC773E09-AD53-48F6-B66B-33024D74E45A", "Added: {0} - {1}", eDoc.DateAdded.ToShortDateString(), eDoc.Description));
		}
	}
}
