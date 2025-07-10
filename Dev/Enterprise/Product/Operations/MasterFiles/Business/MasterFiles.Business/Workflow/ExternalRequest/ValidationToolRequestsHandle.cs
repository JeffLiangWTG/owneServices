using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business;

internal class ValidationToolRequestsHandle : IValidationToolRequestsHandle
{
	public void Handle(BusinessObjectFactory factory, IList<ValidationToolRequestHandleParameter> handleParameters)
	{
		if (handleParameters.Any())
		{
			var requestTypeDic = factory.Load<ExternalRequestType>(new ZQuery(ExternalRequestTypeSchema.PK, handleParameters.Where(x => !x.RequestTypePKOnFailure.IsEmpty).Select(x => x.RequestTypePKOnFailure).ToList())).ToDictionary(x => x.PK);
			var requestTemplateDic = factory.Load<ExternalRequestInfoTemplate>(new ZQuery(ExternalRequestInfoTemplateSchema.PK, requestTypeDic.Values.Where(x => !x.RQT_RIT_Template.IsEmpty).Select(x => x.RQT_RIT_Template).ToArray())).ToDictionary(x => x.PK);

			foreach (var warning in handleParameters)
			{
				if (warning.BusinessEntity is IExternalRequestGenerationProvider provider
					&& warning.BusinessEntity is BusinessObject entity
					&& requestTypeDic.TryGetValue(warning.RequestTypePKOnFailure, out var requestType)
					&& requestTemplateDic.TryGetValue(requestType.RQT_RIT_Template, out var requestTemplate))
				{
					var (assigneeOrgPK, assigneeContactPK) = provider.GetRequestSupportedAddressInfo(requestType.RQT_Assignee);
					var (reviewerOrgPK, reviewerContactPK) = provider.GetRequestSupportedAddressInfo(requestType.RQT_Reviewer);
					var request = factory.New<ExternalRequest>();
					request.REQ_ParentID = entity.PK;
					request.REQ_ParentTableCode = entity.TablePrefix;
					request.REQ_ParentJobID = provider.GetRequestJobID();
					request.REQ_Status = ExternalRequestStatuses.Codes.Assign;
					request.REQ_RQT_Type = requestType.PK;
					request.REQ_Type = provider.GetRequestTypeCode();
					request.REQ_Description = Truncate(request.REQ_ParentJobID + ":" + requestType.RQT_Description, request.REQ_DescriptionInfo.MaxLength);
					request.REQ_RequiredByDate = ZDateTimeOffset.Now.AddDays(requestType.RQT_RequiredInDays);
					request.REQ_OC_AssignedContact = assigneeContactPK;
					request.REQ_OH_AssignedOrganization = assigneeOrgPK;
					request.REQ_OC_ReviewerContact = reviewerContactPK;
					request.REQ_OH_ReviewerOrganization = reviewerOrgPK;
					request.REQ_Notes = ZBlob.FromUTF8(ZString.Empty);
				}
			}
		}
	}

	ZString Truncate(ZString value, int maxLength)
	{
		return (value.IsEmpty || value.Length <= maxLength) ? value : value.SubstringSafe(0, maxLength);
	}
}
