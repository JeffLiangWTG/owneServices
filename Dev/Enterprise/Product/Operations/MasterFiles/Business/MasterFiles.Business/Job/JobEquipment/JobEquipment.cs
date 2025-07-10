using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[PreventDelete(false)]
	[CodeProperty(JobEquipmentSchema.Constants.JEQ_Code), DescriptionProperty(JobEquipmentSchema.Constants.JEQ_Description)]
	public class JobEquipment : AutoJobEquipment
	{
		public JobEquipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("675fe35f-4662-49fe-8d00-69aa745451f4", "Equipment Combination - {0}", CalculateShortcutName());

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
