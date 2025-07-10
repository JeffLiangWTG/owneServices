using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WarehouseDocketLinker : IWarehouseDocketLinker
	{
		/// <summary>
		/// Do *NOT* use this constructor in Warehouse, this constructor exists for the purposes of spring.
		/// </summary>
		public WarehouseDocketLinker(DataContextType docketDataContexType, ITopLevelDataObject topLevelDataObject, UniversalObjectFactory factory)
		{
			DocketDataContexType = docketDataContexType;
			TopLevelDataObject = Argument.NotNull(topLevelDataObject, "topLevelDataObject");
			Argument.NotNull(TopLevelDataObject.DataContext, "TopLevelDataObject.DataContext");

			Factory = Argument.NotNull(factory, "factory");
			UsedSpringConstructor = true;
		}

		internal WarehouseDocketLinker(WhsDocket docket, ITopLevelDataObject topLevelDataObject, UniversalObjectFactory factory)
			: this(GetDataContextTypeWithNullCheck(docket), topLevelDataObject, factory)
		{
			this.docket = docket;
			UsedSpringConstructor = false;
		}

		static DataContextType GetDataContextTypeWithNullCheck(WhsDocket docket)
		{
			Argument.NotNull(docket, "docket");
			return docket.GetUniversalDataContextManager().DataContextType;
		}

		readonly DataContextType DocketDataContexType;
		readonly UniversalObjectFactory Factory;
		readonly ITopLevelDataObject TopLevelDataObject;
		readonly bool UsedSpringConstructor;

		#region Docket

		BusinessObject Docket
		{
			get { return docket ?? (docket = DocketDataSource.GetLoadedJobFromDataContextType(TopLevelDataObject, Factory.BOFactory)); }
		}

		IDataSourceDataObject DocketDataSource
		{
			get { return TopLevelDataObject.GetMatchingDataSource(DocketDataContexType); }
		}

		IEntityID DocketEntityID
		{
			get { return Docket.GetUniversalDataContextManager(); }
		}

		BusinessObject docket;

		#endregion

		#region LinkDocket

		public void LinkDocket(IDataSourceDataObject parentDataSource, IXmlImportLogger logger)
		{
			if (UsedSpringConstructor)
			{
				throw new InvalidOperationException("LinkDocket(IDataSourceDataObject, IXmlImportLogger) should not be used when this class is invoked through Spring.");
			}

			var parent = parentDataSource.GetLoadedJobFromDataContextType(TopLevelDataObject, Factory.BOFactory);
			if (parent != null)
			{
				LinkDocketCore(parent, parent.GetUniversalDataContextManager(), logger);
			}
		}

		bool IWarehouseDocketLinker.LinkDocket(IColumnIndexer parent, IEntityID parentEntityID, IXmlImportLogger logger)
		{
			if (!UsedSpringConstructor)
			{
				throw new InvalidOperationException("IWarehouseDocketLinker.LinkDocket(IColumnIndexer, IEntityID, IXmlImportLogger) should only be used when this class is invoked through Spring.");
			}

			return LinkDocketCore(parent, parentEntityID, logger);
		}

		bool LinkDocketCore(IColumnIndexer parent, IEntityID parentEntityID, IXmlImportLogger logger)
		{
			bool result = false;

			if (logger.IsInternalImport() && parent != null && Docket != null && PivotDoesNotExist(parent))
			{
				var pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(parent.TableName);
				var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.TableName);
				var pivot = Factory.New<WhsDocketJobPivot>();
				pivot.WV_DocketType = Docket.GetValue(WhsDocketSchema.WD_DocketType);
				pivot.WV_WD_Docket = Docket.PK;
				pivot.WV_ParentId = parent.GetValue(pkColumn);
				pivot.WV_ParentTableCode = tablePrefix;

				result = true;
				logger.LogLinkCreated(Factory, parentEntityID, DocketEntityID);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		bool PivotDoesNotExist(IColumnIndexer parent)
		{
			var pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(parent.TableName);
			var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parent.TableName);
			var query = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, Docket.PK);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentId, parent.GetValue(pkColumn));
			query.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, tablePrefix);
			query.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, Docket.GetValue(WhsDocketSchema.WD_DocketType));

			return Factory.BOFactory.GetDatabaseCount(typeof(WhsDocketJobPivot), query) == 0
				&& Factory.LoadTop1<WhsDocketJobPivot>(query) == null; // for anything in memory
		}

		#endregion
	}
}