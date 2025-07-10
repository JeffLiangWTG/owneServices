using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IDataRepo
	{
		DateTime PublicationTime { get; set; }
		void Add(string key, RefDataRepoModelEntityType data);
		void AddRange<TKey, TValue>(IDictionary<TKey, TValue> dataPairs);
		void Remove(string key, RefDataRepoModelEntityType data);
		void RemoveInvalidData();
		void Sort();
		void Clear();
	}

	public interface ITopLevelDataRepo<T> : IDataRepo where T : RefDataRepoModelEntityType
	{
		IReadOnlyCollection<T> Get();
		T Load(string key);
	}
}
