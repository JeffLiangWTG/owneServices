using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using DataContext2011 = Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
using DataContext2012 = Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
using DataSource2012 = Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataSource;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging
{
	public static class Extensions
	{
		public static UniversalShipment ToDataObject(this ForwardingConsol consol)
		{
			var manager = consol.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol));
			var writer = manager.GetShipmentDataObjectWriter(writerManager);
			var consolDataObject = writer.GetDataObject(consol) as UniversalShipment;

			var dataContext = new DataContext2012
			{
				DataSource = new DataSource2012
				{
					Type = nameof(ForwardingConsol),
					Key = consol.JK_UniqueConsignRef,
				}
			};

			consolDataObject.DataContext = dataContext.AddDataProvider();

			if (consolDataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in consolDataObject.SubShipmentCollection)
				{
					if (subShipment.DataContext is DataContext2011 dataContext2011)
					{
						subShipment.DataContext = dataContext2011.ToDataContext2012();
					}
				}
			}

			return consolDataObject;
		}

		public static UniversalShipment IncludeAddInfo(this UniversalShipment dataObject, KeyValuePair<string, string>[] addInfoCollection)
		{
			if (addInfoCollection == null || addInfoCollection.Length == 0)
			{
				return dataObject;
			}

			foreach (var keyValuePair in addInfoCollection)
			{
				dataObject.AddAddInfo(keyValuePair.Key, (ZString)keyValuePair.Value, addEmpty: true);
			}

			return dataObject;
		}

		public static ZString? UnlocoToIata(this ZString? unlocoCode, BusinessObjectFactory factory)
		{
			if (!unlocoCode.HasValue)
			{
				return unlocoCode;
			}

			var result = unlocoCode.Value;
			if (result.Length == 5)
			{
				var unloco = new RefUNLOCO.Loader(factory).Load(unlocoCode);
				result = !string.IsNullOrEmpty(unloco?.RL_IATA) ? unloco.RL_IATA : result.Right(3);
			}
			return result;
		}

		static DataContext2012 AddDataProvider(this DataContext2012 dataContext)
		{
			if (dataContext == null)
			{
				return null;
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var serverCode = registrationKey.ServerCode;

			var dataProvider = new UniversalDataBuss.DataObjects.Universal._2012_11.DataProvider()
			{
				Code = enterpriseCode + serverCode + GlbCompany.CurrentCompany.GC_Code,
				Type = UniversalDataBuss.DataObjects.Universal._2012_11.DataProviderType.EnterpriseID
			};

			dataContext.DataSource ??= new DataSource2012();
			dataContext.DataSource.DataProvider = dataProvider;

			if (dataContext is IDataContextDataObject dataContextDataObject)
			{
				dataContextDataObject.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			}

			return dataContext;
		}

		static DataContext2012 ToDataContext2012(this DataContext2011 dataContext2011)
		{
			if (dataContext2011 == null)
			{
				return null;
			}

			var dataContext2012 = new DataContext2012();
			var dataSource = dataContext2011.DataSourceCollection?.FirstOrDefault();
			if (dataSource != null)
			{
				dataContext2012.DataSource = new DataSource2012
				{
					Key = dataSource.Key,
					Type = dataSource.Type,
				};
			}

			return dataContext2012;
		}
	}
}
