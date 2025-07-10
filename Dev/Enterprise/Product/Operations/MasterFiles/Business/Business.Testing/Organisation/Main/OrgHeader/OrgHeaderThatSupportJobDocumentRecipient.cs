using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgHeaderThatSupportJobDocumentRecipient : OrgHeader, ISupportJobDocumentRecipient
	{
		public OrgHeaderThatSupportJobDocumentRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> SuggestedOrganisations { get; }
	}
}
