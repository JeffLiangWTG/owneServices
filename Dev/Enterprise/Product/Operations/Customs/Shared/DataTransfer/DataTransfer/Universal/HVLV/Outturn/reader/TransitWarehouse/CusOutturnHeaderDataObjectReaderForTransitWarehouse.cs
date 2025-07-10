using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public partial class CusOutturnHeaderDataObjectReaderForTransitWarehouse : ShipmentDataObjectReader<CusOutturnHeader>
	{
		public CusOutturnHeaderDataObjectReaderForTransitWarehouse(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		#region Override

		public override DataContextType DataContextType => DataContextType.SeaCargoOutturn;

		protected override IMatchingBusinessEntityFinder<CusOutturnHeader> GetCombinedReferenceMatcher() => null;

		protected override CusOutturnHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var containerCollection = dataObject.ContainerCollection;
			var containerCount = containerCollection?.Count ?? 0;

			switch (containerCount)
			{
				case 1:
					if (string.IsNullOrEmpty(containerCollection[0].ContainerNumber))
					{
						throw new DataObjectReadFailureException(Res.GetString("aaa4b43f-4a07-42ed-a23d-fe866c6f1657",
							"No Container number found from the UXML received."));
					}
					break;
				case 0:
					throw new DataObjectReadFailureException(Res.GetString("625f77c5-3150-4583-9f48-d37c48bbfe8d",
						"No Container found from the UXML received."));
				default:
					var concatenatedContainerNumbers = string.Join(", ", dataObject.ContainerCollection.Select(x => x.ContainerNumber.GetValueOrDefault()));
					throw new DataObjectReadFailureException(Res.GetString("f3cb56d6-ef70-4b05-b546-22299b3943aa",
						"Multiple Containers found from the UXML received. Container Number(s): {0}.", concatenatedContainerNumbers));
			}

			if (LloydsIMO.HasValue && PremiseID.HasValue && VoyageFlightNo.HasValue)
			{
				var query = new ZQuery(CusOutturnHeaderSchema.C6_LloydsIMO, LloydsIMO.Value);
				query.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, PremiseID.Value);
				query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, VoyageFlightNo.Value);

				var matchingHeaders = factory.BOFactory.Load<CusOutturnHeader>(query);
				switch (matchingHeaders.Length)
				{
					case 1:
						logger.Log(Integration.LogType.Information,
							Res.GetString("3ed517c4-2d38-46de-89f5-c291051b4420",
								"Matching Sea Cargo Outturn found for Vessel Lloyds/IMO: {0}, Premise ID: {1} and Voyage Flight Number: {2}.",
								LloydsIMO, PremiseID, VoyageFlightNo));
						return matchingHeaders[0];
					case 0:
						throw new DataObjectReadFailureException(Res.GetString("58acbb65-8847-4e3a-9537-754eaa4b80dc",
							"No matching Sea Cargo Outturn found for Vessel Lloyds/IMO: {0}, Premise ID: {1} and Voyage Flight Number: {2}.",
							LloydsIMO, PremiseID, VoyageFlightNo));
					default:
						throw new DataObjectReadFailureException(Res.GetString("bc6400ab-ad05-4976-8303-ee3ebdef305f",
							"Multiple matching Sea Cargo Outturn found for Vessel Lloyds/IMO: {0}, Premise ID: {1} and Voyage Flight Number: {2}.",
							LloydsIMO, PremiseID, VoyageFlightNo));
				}
			}

			throw new DataObjectReadFailureException(Res.GetString("d9bd3ecc-d96f-4b36-8520-329f7b41a1de",
@"UXML received could not be used to match with any Sea Cargo Outturn because of below errors. Correct them and try again.
{0}{1}{2}",
LloydsIMO.HasValue ? "" : "Vessel Lloyds is not found.\r\n",
PremiseID.HasValue ? "" : "Premise ID is not found.\r\n",
VoyageFlightNo.HasValue ? "" : "Voyage Flight Number is not found."));
		}

		protected override void PopulateBusinessObject(CusOutturnHeader targetBO)
		{
			var subshipments = dataObject.SubShipmentCollection;
			var container = dataObject.ContainerCollection.FirstOrDefault();
			if (container != null)
			{
				new CusOutturnContainerDataObjectReaderForTransitWarehouse(dataObject, targetBO, logger, factory).ReadIntoBusinessObject();
			}

			if (subshipments != null && subshipments.Count > 0)
			{
				foreach (var subshipment in subshipments)
				{
					new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(subshipment, targetBO, container, logger, factory).ReadIntoBusinessObject();
				}
			}
		}

		#endregion

		#region PremiseID

		protected internal ZString? PremiseID
		{
			get
			{
				CalculatePremiseID();
				return premiseIDCached;
			}
		}
		ZString? premiseIDCached;

		void CalculatePremiseID()
		{
			if (!hasCalculatedPremiseID)
			{
				hasCalculatedPremiseID = true;
				premiseIDCached = CalculatePremiseIDCore();
			}
		}
		internal bool hasCalculatedPremiseID;

		protected virtual ZString? CalculatePremiseIDCore()
		{
			ZString? result = null;
			var additionalReference = dataObject.AdditionalReferenceCollection?.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Enterprise.Customs.DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID);
			if (additionalReference != null)
			{
				result = additionalReference.ReferenceNumber;
			}
			return result;
		}
		#endregion

		#region LloydsIMO

		protected internal ZString? LloydsIMO
		{
			get
			{
				CalculateLloyds();
				return lloydsIMOCached;
			}
		}
		ZString? lloydsIMOCached;

		void CalculateLloyds()
		{
			if (!hasCalculatedLloyds)
			{
				hasCalculatedLloyds = true;
				lloydsIMOCached = CalculateLloydsCore();
			}
		}
		internal bool hasCalculatedLloyds;

		protected virtual ZString? CalculateLloydsCore() => dataObject.LloydsIMO;

		#endregion

		#region VoyageFlightNo

		protected internal ZString? VoyageFlightNo
		{
			get
			{
				CalculateVoyageFlightNo();
				return voyageFlightNoCached;
			}
		}
		ZString? voyageFlightNoCached;

		void CalculateVoyageFlightNo()
		{
			if (!hasCalculatedVoyageFlightNo)
			{
				hasCalculatedVoyageFlightNo = true;
				voyageFlightNoCached = CalculateVoyageFlightNoCore();
			}
		}
		internal bool hasCalculatedVoyageFlightNo;

		protected virtual ZString? CalculateVoyageFlightNoCore() => dataObject.VoyageFlightNo;

		#endregion
	}
}
