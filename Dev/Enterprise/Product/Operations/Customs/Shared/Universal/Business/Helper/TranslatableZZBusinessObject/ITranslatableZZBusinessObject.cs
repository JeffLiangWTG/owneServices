using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Customs.Universal
{
	public interface ITranslatableZZBusinessObject : IBusiness
	{
		ITableSchema LanguageTableSchema { get; }
		Type LanguageTableType { get; }
	}
}
