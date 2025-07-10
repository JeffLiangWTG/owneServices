using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	public class DeclarationScheduleRelatedJobType : ScheduleRelatedJobType
	{
		public DeclarationScheduleRelatedJobType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		public override Type BizOType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(); }
		}
	}
}
