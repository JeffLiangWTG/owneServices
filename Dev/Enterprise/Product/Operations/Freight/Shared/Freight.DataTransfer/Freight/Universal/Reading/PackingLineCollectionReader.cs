using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class PackingLineCollectionReader<T, U> : DataObjectCollectionReader<PackingLine, T>
		where T : PackLine
		where U : CommonShipment
	{
		public PackingLineCollectionReader(PackingLine[] packingLines, IXmlImportLogger logger, UniversalObjectFactory factory, U parentShipment, IBusinessObjectCollection packLinesCollection, IEnumerable<CommonContainer> containersCollection = null)
			: base(packingLines)
		{
			this.logger = Argument.NotNull(logger, "logger");
			this.factory = Argument.NotNull(factory, "factory");
			this.parentShipment = Argument.NotNull(parentShipment, "parentShipment");
			this.packLinesCollection = Argument.NotNull(packLinesCollection, "packLinesCollection");
			this.containersCollection = containersCollection;
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly U parentShipment;
		readonly IBusinessObjectCollection packLinesCollection;
		readonly IEnumerable<CommonContainer> containersCollection;

		#region Implementation

		protected override T[] BusinessObjects
		{
			get { return packLines ?? (packLines = packLinesCollection.ToArray<T>()); }
		}

		T[] packLines;

		protected CommonContainer[] Containers
		{
			get
			{
				if (containers == null)
				{
					containers = containersCollection != null ? containersCollection.ToArray() : System.Array.Empty<CommonContainer>();
				}
				return containers;
			}
		}

		CommonContainer[] containers;

		protected override T FindMatchingBusinessObject(PackingLine dataObject)
		{
			return null; // all packlines will be recreated
		}

		#region ReadIntoBusinessObject

		protected override T ReadIntoBusinessObject(PackingLine dataObject, T packLine)
		{
			var reader = new PackingLineDataObjectReader<T, U>(dataObject, logger, factory, parentShipment, data => packLine);
			var readPackline = reader.ReadIntoBusinessObject();

			if (Containers.Length > 0)
			{
				var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();

				if (!containerNumber.IsEmpty)
				{
					SetContainer(readPackline, containerNumber);
				}
			}

			return readPackline;
		}

		void SetContainer(T packLine, ZString containerNumber)
		{
			var container = Containers.FirstOrDefault(c => c.JC_ContainerNum == containerNumber);

			if (container != null && !container.PackLines.Contains(packLine.PK))
			{
				packLine.SetContainer(container.PK);
			}
		}

		#endregion

		protected override void AddToCollection(T packLine)
		{
			packLinesCollection.Add(packLine);
		}

		protected override void RemoveFromCollection(T packLine)
		{
			packLinesCollection.Delete(packLine);
		}

		#endregion
	}
}
