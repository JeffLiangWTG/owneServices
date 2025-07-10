using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class MNRServiceTemplateLine : AutoMNRServiceTemplateLine
	{
		public MNRServiceTemplateLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("ServiceTemplate")]
		public override ZGuid MSL_MST_MNRServiceTemplate { get => base.MSL_MST_MNRServiceTemplate; set => base.MSL_MST_MNRServiceTemplate = value; }

		public MNRServiceTemplate ServiceTemplate
		{
			get => Factory.Load<MNRServiceTemplate>(MSL_MST_MNRServiceTemplate);
		}

		#endregion
	}
}
