using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface ITemplateRecordProvider
	{
		bool IsTemplateRecord { get; set; }

		ITemplateRecord TemplateRecord { get; set; }

		void SaveToTemplateRecord();

		void LoadFromTemplateRecord(ITemplateRecord templateRecord);

		BusinessObject InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord);
	}
}
