using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerLinker<TContainer, TConsol> : IContainerLinker
		where TContainer : CommonContainer
		where TConsol : CommonConsol
	{
		internal ContainerLinker(BusinessObjectFactory factory, IUniversalFreightHelper helper)
		{
			this.factory = factory;
			this.helper = Argument.NotNull(helper, "helper");
		}

		readonly BusinessObjectFactory factory;
		readonly IUniversalFreightHelper helper;

		public virtual CommonContainer[] GetLogParent(IXmlEventValueObject xmlEvent)
		{
			Argument.NotNull(xmlEvent, "xmlEvent");

			var uldIdentifications = xmlEvent.Context.ULDIdentifications;
			var containerIDs = uldIdentifications != null && uldIdentifications.Any()
				? uldIdentifications
				: xmlEvent.Context.ContainerNumbers;

			CommonContainer[] result = null;
			if (containerIDs != null && containerIDs.Any())
			{
				result = containerIDs.Where(c => !c.IsEmpty).Select(c => FindContainer(c, xmlEvent)).WhereNotNull().ToArray();
			}

			return result != null && result.Any() ? result : null;
		}

		CommonContainer FindContainer(ZString containerID, IXmlEventValueObject xmlEvent)
		{
			var consolQuery = new ZDBOnlySubQuery(typeof(TConsol), JobConsolSchema.PK);
			helper.AddConsolParameters(consolQuery);
			BuildConsolQueryWithCSRSubQuery(consolQuery, xmlEvent);

			var query = new ZDBOnlyQuery(typeof(TContainer));
			query.AddToFilter(JobContainerSchema.JC_ContainerNum, containerID);
			query.AddSubQuery(JobContainerSchema.JC_JK, consolQuery, JoinCondition.And);

			var containers = factory.Load<TContainer>(query);
			CommonConsol resultConsol = null;
			CommonShipment resultShipment = null;
			CommonContainer resultShipmentContainer = null;
			CommonContainer resultConsolContainer = null;

			foreach (var container in containers)
			{
				foreach (CommonShipment shipment in container.ParentShipmentsCached)
				{
					if (resultShipment == null || shipment.JS_SystemCreateTimeUtc > resultShipment.JS_SystemCreateTimeUtc)
					{
						resultShipment = shipment;
						resultShipmentContainer = container;
					}
				}

				if (container.JC_JK != ZGuid.Empty)
				{
					var consol = factory.Load<TConsol>(container.JC_JK);
					if (consol != null)
					{
						if (resultConsol == null || consol.Transports.MostInterestingTransport.JW_ETD > resultConsol.Transports.MostInterestingTransport.JW_ETD)
						{
							resultConsol = consol;
							resultConsolContainer = container;
						}
					}
				}
			}

			return resultShipmentContainer ?? resultConsolContainer;
		}

		void BuildConsolQueryWithCSRSubQuery(ZDBOnlyQuery consolQuery, IXmlEventValueObject xmlEvent)
		{
			var documentName = xmlEvent?.DataContext?.DocumentaryOverride?.DocumentName ?? ZString.Empty;
			if (documentName == VGMDocumentName)
			{
				ZDBOnlySubQuery csrNumberQuery;
				var key = xmlEvent.GetMatchingDataTarget(DataContextType.ForwardingConsol)?.Key ?? ZString.Empty;

				if (ConsolCarrierShipperReferenceNumberCalculator.IsValidCarrierShipperReferenceNumber(key))
				{
					csrNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
					csrNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, key);
				}
				else
				{
					csrNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, true); // CSR number should NOT matched when the original responding message received.
				}

				csrNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference);
				csrNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, true);
				consolQuery.AddSubQuery(csrNumberQuery, JoinCondition.And);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Document name")]
		const string VGMDocumentName = "Verified Gross Container Weight";
	}
}
