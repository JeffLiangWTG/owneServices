using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse : ShipmentDataObjectReader<CusOutturn>
	{
		public CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(Shipment dataObject, CusOutturnHeader header, Container container, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.container = container;
		}

		readonly CusOutturnHeader header;
		readonly Container container;

		public override DataContextType DataContextType => DataContextType.Outturn;

		protected override IMatchingBusinessEntityFinder<CusOutturn> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CusOutturn GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var houseBill = Shipment?.WayBillNumber;

			var masterBillReference = Shipment.AdditionalReferenceCollection?.Where(a => a.Type != null && a.Type.Code.HasValue
				&& a.Type.Code.Value == (ZString)AdditionalReferenceTypes.Codes.MasterBill && a.ReferenceNumber.HasValue).FirstOrDefault();

			var hasValidHouseBill = !string.IsNullOrEmpty(houseBill);
			var hasValidMasterBill = !string.IsNullOrEmpty(masterBillReference?.ReferenceNumber);
			var containerNumber = container?.ContainerNumber ?? ZString.Empty;

			if (!containerNumber.IsEmpty && hasValidHouseBill && hasValidMasterBill)
			{
				var masterBill = masterBillReference.ReferenceNumber;

				var query = new ZQuery(CusOutturnSchema.C5_C6, header.PK);
				query.AddToFilter(CusOutturnSchema.C5_ContainerNumber, containerNumber);
				query.AddToFilter(CusOutturnSchema.C5_MasterBill, masterBill);
				query.AddToFilter(CusOutturnSchema.C5_HouseBill, houseBill);

				var matchingOutturns = factory.BOFactory.Load<CusOutturn>(query);
				switch (matchingOutturns.Length)
				{
					case 1:
						logger.Log(Integration.LogType.Information,
							Res.GetString("5a9c7548-7fe7-4047-9efa-1ff8f5b6c041",
								"Matching Cargo Line found for Container Number: {0}, Master Bill: {1} and House Bill: {2}.",
								containerNumber, masterBill, houseBill));
						return matchingOutturns[0];
					case 0:
						throw new DataObjectReadFailureException(Res.GetString("daeddd60-227c-4ce3-9648-4608aabe32af",
							"No matching Cargo Lines found for Master Bill: {0}, House Bill: {1} and Container Number: {2}.",
							masterBill, houseBill, containerNumber));
					default:
						var concatenatedCargoTypes = string.Join(", ", matchingOutturns.Select(o => o.C5_CargoType).Distinct().OrderBy(c => c));
						throw new DataObjectReadFailureException(Res.GetString("975d3c58-ded0-46af-8564-ad33958adaab",
							"Multiple matching Cargo Lines found for Master Bill: {0}, House Bill: {1} and Container Number: {2}. Cargo Types were {3}.",
							masterBill, houseBill, containerNumber, concatenatedCargoTypes));
				}
			}

			throw new DataObjectReadFailureException(Res.GetString("fff43d72-3869-4890-a6df-51222e9605d6",
@"UXML received could not be used to match with any Cargo lines because of below errors. Correct them and try again.
{0}{1}{2}",
!containerNumber.IsEmpty ? "" : "Container details are missing.\r\n",
hasValidMasterBill ? "" : "Master Bill Number is not found.\r\n",
hasValidHouseBill ? "" : "House Bill Number is not found."));
		}

		protected override void PopulateBusinessObject(CusOutturn targetBO)
		{
			var packLines = Shipment.PackingLineCollection;
			if (packLines != null && packLines.Count > 0)
			{
				var packLinesWithOutturn = packLines.Where(p => p.OutturnQty.HasValue).ToArray();
				targetBO.C5_PackagesOutturned = packLinesWithOutturn.Sum(p => p.OutturnQty.Value);
				targetBO.C5_PackagesUnits = targetBO.C5_OuterPackUnits;
				targetBO.C5_VolumeOutturned = packLinesWithOutturn.Where(p => p.OutturnedVolume.HasValue).Sum(p => p.OutturnedVolume.Value);
				targetBO.C5_WeightOutturned = packLinesWithOutturn.Where(p => p.OutturnedWeight.HasValue).Sum(p => p.OutturnedWeight.Value);
				targetBO.C5_PillageIndicator = packLinesWithOutturn.Any(p => p.OutturnPillagedQty.HasValue && p.OutturnPillagedQty.Value > 0);
				targetBO.C5_DamageIndicator = packLinesWithOutturn.Any(p => p.OutturnDamagedQty.HasValue && p.OutturnDamagedQty.Value > 0);
				if (container != null)
				{
					targetBO.C5_CargoUnpackDate = container.LCLUnpack.GetValueOrDefault();
					if (targetBO.C5_CargoReceiptDate.IsEmpty)
					{
						targetBO.C5_CargoReceiptDate = container.LCLUnpack.GetValueOrDefault();
					}
				}
			}
		}

		Shipment Shipment => (Shipment)DataObject;
	}
}
