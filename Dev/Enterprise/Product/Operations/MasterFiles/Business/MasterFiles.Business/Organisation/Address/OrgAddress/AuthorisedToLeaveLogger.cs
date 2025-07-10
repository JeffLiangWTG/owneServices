using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	static class AuthorisedToLeaveLogger
	{
		public static StmALog Log(IStmALogParent bizo, ZPropertyInfo info, string freeText = "")
		{
			StmALog result = null;
			if ((!((BusinessObject)bizo).IsInDatabase && (ZString)info.Value != AuthorityToLeaveOptions.Codes.DEF) || info.HasChanges)
			{
				result = bizo.Logs.CreateRecreateOrUpdateEventLog(((ZString)info.Value == AuthorityToLeaveOptions.Codes.YES) ? Events.Authorised : Events.AuthorisationWithdrawn, EstimateActual.Actual, ZDateTimeOffset.Now, freeText,
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceActionAuthorisedTypes.AuthorisedToLeave),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, (ZString)info.OriginalValue),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, (ZString)info.Value));
			}

			return result;
		}
	}
}
