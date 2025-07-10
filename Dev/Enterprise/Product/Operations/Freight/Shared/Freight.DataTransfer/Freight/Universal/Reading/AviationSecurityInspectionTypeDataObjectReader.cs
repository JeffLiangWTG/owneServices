using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EZC = Enterprise.ZArchitecture.Core;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer
{
	public class AviationSecurityInspectionTypeDataObjectReader : DataObjectReader<CodeDescriptionPair, CusEntryNumber>
	{
		public AviationSecurityInspectionTypeDataObjectReader(CodeDescriptionPair inspectionTypeDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ISupportInspectionType inspectionType, Logs logs, bool isInDatabase, string reason = null)
			: base(inspectionTypeDataObject, logger, factory)
		{
			InspectionType = inspectionType.InspectionType;
			IsInDatabase = isInDatabase;
			Logs = logs;
			Reason = reason;
		}

		protected CusEntryNumber InspectionType;
		protected bool IsInDatabase;
		protected Logs Logs;
		protected string Reason;

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			return InspectionType;
		}

		protected override void PopulateBusinessObject(CusEntryNumber cusEntryNumberBO)
		{
			LogInspectionTypeCodeChangedIfNeeded(cusEntryNumberBO);
			SetValue(cusEntryNumberBO, CusEntryNumSchema.CE_EntryNum, dataObject.Code);
		}

		protected virtual string Type => (EZC.NoResString)"Inspection";

		void LogInspectionTypeCodeChangedIfNeeded(CusEntryNumber cusEntryNumberBO)
		{
			if (dataObject.Code.HasValue
				&& IsInDatabase
				&& dataObject.Code.Value != cusEntryNumberBO.CE_EntryNum)
			{
				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(Params.Old, cusEntryNumberBO.CE_EntryNum),
						new KeyValuePair<string, string>(Params.New, dataObject.Code),
						new KeyValuePair<string, string>(Params.Type, Type),
						new KeyValuePair<string, string>(Params.Reason, (Reason ?? (EZC.NoResString)"Changed by Data Import"))
					};

				Logs.AddNew(Events.SecurityModified, ZDateTimeOffset.Now, parameters);
			}
		}
	}
}
