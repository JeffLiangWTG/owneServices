using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public sealed class eDocsCollectionForTesting : IStorageDocsBaseCollection
	{
		public eDocsCollectionForTesting(params IeDoc[] elements)
		{
			docs = new List<IeDoc>();
			if (elements != null)
			{
				docs.AddRange(elements);
			}
		}

		readonly List<IeDoc> docs;

		public void Add(IeDoc elementToAdd)
		{
			docs.Add(elementToAdd);
		}

		public bool Contains(IeDoc element)
		{
			return docs.Contains(element);
		}

		public int Count
		{
			get { return docs.Count; }
		}

		public IeDoc GetFromUniqueKey(Guid uniqueKey)
		{
			return docs.Find(doc => doc.UniqueKey == uniqueKey);
		}

		public IeDoc GetMostRecentEDoc(string docType)
		{
			return null;
		}

		public void Remove(IeDoc elementToRemove)
		{
		}

		public IeDoc this[int index]
		{
			get { return docs[index]; }
		}

		public IEnumerator GetEnumerator()
		{
			return docs.GetEnumerator();
		}

		public bool ContainsDocType(ZString docType)
		{
			return false;
		}
	}
}
