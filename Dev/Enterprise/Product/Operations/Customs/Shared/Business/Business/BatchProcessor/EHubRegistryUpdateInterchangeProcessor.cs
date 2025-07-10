using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.XmlCredential;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class EHubRegistryUpdateInterchangeProcessor : BaseInboundInterchangeProcessor
	{
		public EHubRegistryUpdateInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes => new string[] { EDIInterchange.ApplicationCodes.eHub };
		protected override bool IsNoBranchFilter => true;

		protected override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, new[] { EDIInterchangeTypeList.Codes.EHubRegistryUpdate, EDIInterchangeTypeList.Codes.Configuration });
			query.AddToFilter(EDIInterchangeSchema.EI_GB, GlbBranch.CurrentBranch.PK);
		}

		protected override bool ProcessInterchange(EDIInterchange interchange)
		{
			var result = false;
			var configuration = interchange.GetEI_BodyTextReader().DeserializeToConfiguration();
			var xmlCredentialConfigurationHandler = GetXmlCredentialConfigurationHandler(configuration.Name);
			if (xmlCredentialConfigurationHandler == null)
			{
				Logger.LogError(Res.GetString("{6C6E5F04-C3B1-4472-A459-B75858C86E66}", "There is no handler for configuration '{0}'.", configuration.Name));
			}
			else
			{
				result = xmlCredentialConfigurationHandler.Process(configuration);
			}
			return result;
		}

		IXmlCredentialConfigurationHandler GetXmlCredentialConfigurationHandler(string configurationName)
		{
			IXmlCredentialConfigurationHandler result = null;
			if (!string.IsNullOrEmpty(configurationName))
			{
				var types = (Hashtable)ObjectFactory.Get("XmlCredentialConfigurationHandlers");
				var objectHandle = (ObjectHandle)types[configurationName];
				if (objectHandle != null)
				{
					result = (IXmlCredentialConfigurationHandler)objectHandle.GetObject(Logger);
				}
			}
			return result;
		}

		protected override void HandleProcessingException(Exception ex, EDIInterchange interchange, ZGuid interchangePK)
		{
			base.HandleProcessingException(ex, interchange, interchangePK);

			var newFacotry = new BusinessObjectFactory();
			var note = newFacotry.New<StmNote>();
			var noteType = PredefinedNoteTypes.Instance.DataImportLogNote;
			note.ST_ParentID = interchangePK;
			note.ST_Table = EDIInterchangeSchema.Constants.TableName;
			note.ST_Description = noteType.Description;
			note.ST_NoteText = ZString.Format("An exception has occurred:\r\nMessage:{0}\r\nCallstack:{1}", ex.Message, ex.StackTrace);

			newFacotry.Save();
		}
	}
}
