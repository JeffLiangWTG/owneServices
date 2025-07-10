using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class EDocsProviderSupporter
	{
		readonly BusinessContext businessContext;
		List<MenuItemIdentifier> consumers;

		public EDocsProviderSupporter(IEDocsProvider eDocsProvider)
		{
			businessContext = eDocsProvider.DocumentSupporter.BusinessContext;
		}

		public BusinessContext BusinessContext
		{
			get { return businessContext; }
		}

		public void AddConsumer(MenuItemIdentifier consumer)
		{
			if (consumers == null)
			{
				consumers = new List<MenuItemIdentifier>();
			}
			consumers.Add(consumer);
		}

		public T CreateProviderPlaceholder<T>(StmMenuItem consumer) where T : StmMenuItem
		{
			T result = consumer.Factory.New<T>();
			result.SU_BusinessContext = BusinessContext.ToString();
			result.SU_FilterList = GetProviderPlaceholderKey(consumer);
			result.SU_IsSystemDefined = true;
			result.SU_MenuName = consumer.SU_MenuName + (NoResString)" (Read-only placeholder for adding eDocs attachments)";
			result.SU_GS_NKStaffCode = ZString.Empty;
			return result;
		}

		public MenuItemIdentifier[] GetConsumers()
		{
			return (consumers == null) ? System.Array.Empty<MenuItemIdentifier>() : consumers.ToArray();
		}

		public T GetProviderPlaceholder<T>(StmMenuItem consumer) where T : StmMenuItem
		{
			ZQuery query = GetProviderPlaceholderQuery(consumer);
			return consumer.Factory.LoadTop1<T>(query);
		}

		string GetProviderPlaceholderKey(StmMenuItem consumer)
		{
			return string.Format("\"{0}\"==\"{1}\\{2}\"", Constants.MenuItemFilters.EDocsProviderPlaceholderTag, consumer.SU_BusinessContext, consumer.SU_MenuName);
		}

		ZQuery GetProviderPlaceholderQuery(StmMenuItem consumer)
		{
			DocumentZQuery result = new DocumentZQuery(BusinessContext);
			result.AddToFilter(StmMenuItemSchema.SU_FilterList, GetProviderPlaceholderKey(consumer));
			result.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			return result;
		}
	}
}
