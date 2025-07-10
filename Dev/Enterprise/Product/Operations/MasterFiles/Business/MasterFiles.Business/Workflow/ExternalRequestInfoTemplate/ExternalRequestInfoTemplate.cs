using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.RIT_Code), DescriptionProperty(Schema.RIT_Description)]
	public class ExternalRequestInfoTemplate : AutoExternalRequestInfoTemplate, IJobNumber
	{
		public ExternalRequestInfoTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string JobNumber => RIT_Code;

		protected bool RIT_JobType_ReadOnly => IsInDatabase;

		protected override ZString HumanReadableNameCore => Res.GetString("b39285f2-e300-4844-8874-acae01dd24bf", "Request Template {0}", this.RIT_Code);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[List("Lookups.JobTypeList")]
		public override ZString RIT_JobType { get => base.RIT_JobType; set => base.RIT_JobType = value; }

		#region RIT_Template_HTML

		public ZBlob RIT_Template_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(RIT_Template);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.RIT_Template = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion
	}
}
