using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class BaseContainerDataObjectReader<T> : DataObjectReader<Container, T>
		where T : BusinessObject
	{
		protected BaseContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(containerDataObject, logger, factory) { }

		protected void SetValue(BusinessObject containerBO, SchemaColumn column, ContainerType containerType)
		{
			SetValue((x) => containerBO[column] = x, containerType);
		}

		protected void SetValue(IColumnIndexer containerRow, SchemaColumn column, ContainerType containerType)
		{
			SetValue((x) => containerRow.SetValue(column, x), containerType);
		}

		void SetValue(Action<ZGuid> dataSetter, ContainerType containerType)
		{
			if (containerType != null && containerType.Code.HasValue)
			{
				var refContainer = factory.LoadFromUniqueKey<RefContainer>(RefContainerSchema.RC_Code, containerType.Code.Value);
				if (refContainer == null)
				{
					logger.Log(LogType.Warning, Res.GetString("db720bde-45ef-43ac-87e9-bd229be1c4c2", "Container Type '{0}' is invalid.", containerType.Code.Value));
					dataSetter(ZGuid.Empty);
				}
				else
				{
					if (!refContainer.RC_IsActive)
					{
						logger.Log(LogType.Warning, Res.GetString("df587164-59c9-4c27-aa57-372928e09737", "Container Type '{0}' is inactive so it will not be used. Please go to the consol and choose a container type.", containerType.Code.Value));
						dataSetter(ZGuid.Empty);
					}
					else
					{
						logger.Log(LogType.Information, Res.GetString("a51d234f-85ed-4087-992b-af793fe4a7ae", "Successfully loaded matching Container Type."));
						dataSetter(refContainer.PK);
					}
				}
			}
		}
	}
}
