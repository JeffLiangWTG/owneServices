using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationAction : AutoProcessTemplateValidationAction
	{
		public ProcessTemplateValidationAction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("155b702f-763a-4e31-aa43-c3b98573dc53", Caption = "Action Source")]
		[List(nameof(Lookups) + "." + nameof(ProcessTemplateValidationActionLookups.ActionSourceList))]
		public override ZString P0A_ActionSource
		{
			get => base.P0A_ActionSource;
			set => base.P0A_ActionSource = value;
		}

		[ResourceStringData("eb72a494-aa15-4457-841d-1f7afdf089b1", Caption = "Path")]
		public ZString ActionSourceDescription => P0A_ActionSource.IsEmpty ? ZString.Empty : Lookups.ActionSourceList.GetDescriptionFromCode(P0A_ActionSource);

		public ZPropertyInfo ActionSourceDescriptionInfo => GetZPropertyInfo(nameof(ActionSourceDescription));

		public ZString CountryCode => WorkflowTemplate?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;
	}
}
