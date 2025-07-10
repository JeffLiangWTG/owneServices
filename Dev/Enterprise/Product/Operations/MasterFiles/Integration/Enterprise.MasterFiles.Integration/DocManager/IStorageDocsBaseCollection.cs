using System;
using System.Collections;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IStorageDocsBaseCollection : IEnumerable
	{
		IeDoc GetMostRecentEDoc(string docType);
		IeDoc GetFromUniqueKey(Guid uniqueKey);

		int Count { get; }
		void Remove(IeDoc elementToRemove);
		void Add(IeDoc elementToAdd);
		bool Contains(IeDoc element);
		bool ContainsDocType(ZString docType);
		IeDoc this[int index] { get; }
	}
}
