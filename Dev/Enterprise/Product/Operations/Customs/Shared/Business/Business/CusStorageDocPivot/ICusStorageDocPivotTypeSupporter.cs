using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business
{
	public interface ICusStorageDocPivotTypeSupporter
	{
		Type CusStorageDocPivotType { get; }
		void ReloadCollection();
		IEnumerable<IStorageDocsBaseCollection> EDocCollections { get; }
		ZString HumanReadableName { get; }
	}
}
