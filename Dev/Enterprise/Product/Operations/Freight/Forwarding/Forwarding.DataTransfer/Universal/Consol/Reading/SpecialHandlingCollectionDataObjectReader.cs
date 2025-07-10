using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class SpecialHandlingCollectionDataObjectReader
	{
		public SpecialHandlingCollectionDataObjectReader(CodeDescriptionPair[] specialHandlingDataObjects, UniversalObjectFactory factory, ForwardingConsol consolBusinessObject)
		{
			consolBO = consolBusinessObject;

			var codeSet = new HashSet<ZString>();
			var accepted = new List<CodeDescriptionPair>();

			foreach (var dataObject in specialHandlingDataObjects)
			{
				if (!dataObject.Code.HasValue || string.IsNullOrWhiteSpace(dataObject.Code) || codeSet.Contains(dataObject.Code.Value))
				{
					continue;
				}

				codeSet.Add(dataObject.Code.Value);

				if (AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(dataObject.Code.Value))
				{
					if (!string.IsNullOrWhiteSpace(securityStatusCode))
					{
						throw new MessageProcessingBusinessFailureException("Conflicting security special handling codes set", false, string.Empty);
					}
					securityStatusCode = dataObject.Code.Value;
				}
				else
				{
					accepted.Add(dataObject);
				}
			}

			nonSecuritySpecialHandlingCollectionDataObjectReader =
				new NonSecuritySpecialHandlingCollectionDataObjectReader(accepted.ToArray(), factory, consolBusinessObject);
		}

		readonly ForwardingConsol consolBO;
		readonly ZString? securityStatusCode;
		readonly NonSecuritySpecialHandlingCollectionDataObjectReader nonSecuritySpecialHandlingCollectionDataObjectReader;

		public void ReadIntoCollection()
		{
			nonSecuritySpecialHandlingCollectionDataObjectReader.ReadIntoCollection();

			if (securityStatusCode.HasValue)
			{
				consolBO.SecurityStatusCode = securityStatusCode.Value;
			}
			else
			{
				consolBO.SecurityStatusCode = string.Empty;
			}
		}
	}
}
