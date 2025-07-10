using System;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityNewSecurityCheckpoint : SecurityCheckpoint
	{
		public RefFacilityNewSecurityCheckpoint()
			: base("RefFacilityNew", (NoResString)"Cannot create new Facility", null, null, false)
		{
		}

		public override bool IsAllowed => false;

		public override void AddChild(SecurityCheckpoint child)
		{
			throw new NotSupportedException("AddChild() is not supported by RefFacilityNewSecurityCheckpoint.");
		}

		public override void ShowError()
		{
			throw new NotSupportedException("ShowError() is not supported by RefFacilityNewSecurityCheckpoint.");
		}

		public override MultilingualString ErrorMessageForNotAllowed
		{
			get { return ResString.GetMultilingualString("B6C69122-806C-42BD-8681-138B249BDAF1", "To register a new facility with CargoWise, please raise a CR8 Compliance. Once this request has been processed the new record will be available for selection in your system."); }
		}
	}
}
