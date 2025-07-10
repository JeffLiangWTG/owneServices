using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	public class JobComInvoiceHeaderWithInterfaceForTesting : BaseJobComInvoiceHeader, ICusAddInfoTypeSupporter, IAddInfoManager
	{
		public JobComInvoiceHeaderWithInterfaceForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Func<IDictionary<ZString, Type>> getCusAddInfoTypeForTesting;

		public TestAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = new TestAddInfo(this)); }
		}
		TestAddInfo addInfo;

		IAddInfo IAddInfoManager.AddInfo => AddInfo;

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return getCusAddInfoTypeForTesting?.Invoke();
		}
	}
}
