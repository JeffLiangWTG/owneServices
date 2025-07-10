using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoRefDocSource.Schema.RDS_Code), DescriptionProperty(AutoRefDocSource.Schema.RDS_Desc)]
	public class RefDocSource : AutoRefDocSource
	{
		public RefDocSource(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		[TranslatableDataField(Schema.TableName, Schema.RDS_Desc, DataXmlFilePaths.Documents, Type = typeof(RefDocSource), SecurityCheckpoint = "DocumentSourcesModify", Asmid = ResString.AssemblyId)]
		public override ZString RDS_Desc
		{
			get { return base.RDS_Desc; }
			set { base.RDS_Desc = value; }
		}

		public MultilingualString RDS_DescMultilingual
		{
			get { return GetMultilingual(RDS_DescInfo); }
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
