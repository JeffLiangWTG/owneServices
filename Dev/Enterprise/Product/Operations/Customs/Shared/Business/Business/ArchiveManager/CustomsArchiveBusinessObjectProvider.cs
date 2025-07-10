using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Freight.Business.ArchiveManager;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.ArchiveManager
{
	public class CustomsArchiveBusinessObjectProvider : IArchiveableBusinessObjectProvider
	{
		public CustomsArchiveBusinessObjectProvider()
		{
			assistantDictionary = new Dictionary<string, IArchiveableBusinessObjectProviderAssistant>();
			IEnumerable assistantList = ObjectFactory.Get<IEnumerable>("ArchiveableBusinessObjectProviderAssistantList");

			foreach (IArchiveableBusinessObjectProviderAssistant assistant in assistantList)
			{
				assistantDictionary.Add(assistant.CountryCode, assistant);
			}
		}

		readonly Dictionary<string, IArchiveableBusinessObjectProviderAssistant> assistantDictionary;

		#region IArchiveableBusinessObjectProvider Members

		public IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory)
		{
			if (item.PKColumn.TableName == JobDeclarationSchema.Constants.TableName)
			{
				var dec = factory.Load<BaseJobDeclaration>(item.PK);

				if (assistantDictionary.ContainsKey(dec.CountryCode))
				{
					return new IArchiveableBusinessObject[] { assistantDictionary[dec.CountryCode].LoadArchiveableBusinessObject(dec) };
				}
				else
				{
					return new IArchiveableBusinessObject[] { new ArchiveableBaseJobDeclaration(dec) };
				}
			}
			else if (item.PKColumn.TableName == EDIMessageSchema.Constants.TableName)
			{
				EDIMessage message = factory.Load<EDIMessage>(item.PK);
				return new IArchiveableBusinessObject[] { new ArchiveableEDIMessage(message) };
			}

			return null;
		}

		public IEnumerable<string> TableNamesSupported
		{
			get
			{
				List<string> tableList = new List<string>();
				tableList.Add(BaseJobDeclaration.Schema.TableName);
				tableList.Add(EDIMessage.Schema.TableName);

				foreach (IArchiveableBusinessObjectProviderAssistant assistant in assistantDictionary.Values)
				{
					foreach (string tablename in assistant.TableNamesSupported)
					{
						if (!tableList.Contains(tablename))
						{
							tableList.Add(tablename);
						}
					}
				}

				return tableList;
			}
		}

		public IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported
		{
			get
			{
				List<ReferenceKeyType> keyList = new List<ReferenceKeyType>();
				keyList.Add(ArchiveReferenceKey.CommonTypes.DeclarationNo);
				keyList.Add(ArchiveReferenceKey.CommonTypes.JobNo);
				keyList.Add(ArchiveReferenceKey.CommonTypes.Masterbill);
				keyList.Add(ArchiveReferenceKey.CommonTypes.Housebill);
				keyList.Add(FreightArchiveKeyTypes.Order);
				keyList.Add(ArchiveReferenceKey.CommonTypes.Consignor);
				keyList.Add(ArchiveReferenceKey.CommonTypes.Consignee);
				keyList.Add(ArchiveReferenceKey.CommonTypes.InvoiceNo);
				keyList.Add(ArchiveReferenceKey.CommonTypes.FlightAndDate);
				keyList.Add(ArchiveReferenceKey.CommonTypes.RegoAndDate);
				keyList.Add(ArchiveReferenceKey.CommonTypes.VesselVoyage);
				keyList.Add(ArchiveReferenceKey.CommonTypes.ContainerNo);
				keyList.Add(ArchiveReferenceKey.CommonTypes.PartNo);
				keyList.Add(CustomsArchiveReferenceKeys.CustomsReferenceNumber);
				keyList.Add(CustomsArchiveReferenceKeys.EDIInterchangeNumber);
				keyList.Add(CustomsArchiveReferenceKeys.EDIMessage);
				keyList.Add(CustomsArchiveReferenceKeys.EDIMessageApplicationReference);
				keyList.Add(CustomsArchiveReferenceKeys.EDIMessageNum);
				keyList.Add(CustomsArchiveReferenceKeys.EDIMessageReceiveTransmit);
				keyList.Add(CustomsArchiveReferenceKeys.EDIMessageSystemCreateUser);

				foreach (IArchiveableBusinessObjectProviderAssistant assistant in assistantDictionary.Values)
				{
					foreach (ReferenceKeyType key in assistant.ReferenceKeyTypesSupported)
					{
						if (!keyList.Contains(key))
						{
							keyList.Add(key);
						}
					}
				}

				return keyList;
			}
		}

		#endregion
	}
}
