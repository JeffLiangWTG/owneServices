using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnContainerDataObjectReaderForTransitWarehouse : ShipmentDataObjectReader<CusOutturn>
	{
		public CusOutturnContainerDataObjectReaderForTransitWarehouse(Shipment dataObject, CusOutturnHeader header, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.container = Argument.NotNull(dataObject.ContainerCollection.SingleOrDefault(), "Container");
			this.containerNumber = Argument.NotNullOrEmpty(container.ContainerNumber, "Container.ContainerNumber");
		}

		readonly CusOutturnHeader header;
		readonly Container container;
		readonly ZString containerNumber;

		public override DataContextType DataContextType => DataContextType.Outturn;

		protected override IMatchingBusinessEntityFinder<CusOutturn> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			if (!containerNumber.IsEmpty)
			{
				var query = new ZQuery(CusOutturnSchema.C5_C6, header.PK);
				query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);
				query.AddToFilter(CusOutturnSchema.C5_CargoType, ContainerModes.FCL);

				var matchingOutturns = factory.BOFactory.Load<CusOutturn>(query);
				switch (matchingOutturns.Length)
				{
					case 1:
						logger.Log(Integration.LogType.Information,
							Res.GetString("9ae77fde-1d02-49f8-afb5-67d54e4812cc",
								"Matching FCL Cargo Line found for Container Number: {0}.",
								containerNumber));
						return matchingOutturns[0];
					case 0:
						throw new DataObjectReadFailureException(Res.GetString("b1ae539a-ecaa-4654-ab19-44023da9577b",
							"No matching FCL Cargo Line found for Container Number: {0}.", containerNumber));
					default:
						throw new DataObjectReadFailureException(Res.GetString("6212b933-0a0c-48ff-a460-bdd5e556fae6",
							"Multiple matching FCL Cargo Lines found for Container Number: {0}.", containerNumber));
				}
			}

			throw new DataObjectReadFailureException(Res.GetString("c7ab0fad-3607-4981-8a2c-50a7155cd46c",
@"UXML received could not be used to match with any FCL Cargo lines because Container details are missing."));
		}

		protected override void PopulateBusinessObject(CusOutturn targetBO)
		{
			targetBO.C5_PackagesOutturned = dataObject.TotalNoOfPiecesLanded.GetValueOrDefault();
			targetBO.C5_PackagesUnits = targetBO.C5_OuterPackUnits;
			targetBO.C5_SealIntactIndicator = container.IsSealOk.GetValueOrDefault();

			if (targetBO.C5_CargoReceiptDate.IsEmpty)
			{
				targetBO.C5_CargoReceiptDate = container.LCLUnpack.GetValueOrDefault();
			}
		}
	}
}
