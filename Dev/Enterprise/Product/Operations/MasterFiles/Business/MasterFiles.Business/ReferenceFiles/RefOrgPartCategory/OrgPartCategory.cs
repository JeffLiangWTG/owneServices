using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoOrgPartCategory.Schema.OPC_CategoryCode), DescriptionProperty(AutoOrgPartCategory.Schema.OPC_CategoryDescription)]
	public class OrgPartCategory : AutoOrgPartCategory
	{
		public OrgPartCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region OPC_CategoryDescription

		[TranslatableDataField(Schema.TableName, Schema.OPC_CategoryDescription, MaxLength = Schema.OPC_CategoryDescriptionMaxLength, Type = typeof(OrgPartCategory), SecurityCheckpoint = "RefOrgPartCategoryModify", Asmid = ResString.AssemblyId)]
		public override ZString OPC_CategoryDescription
		{
			get { return base.OPC_CategoryDescription; }
			set { base.OPC_CategoryDescription = value; }
		}

		public MultilingualString OPC_CategoryDescriptionMultilingual
		{
			get { return GetMultilingual(OPC_CategoryDescriptionInfo); }
		}

		#endregion

		#region OPC_OPC_Parent

		[List("Lookups.Parents")]
		[BusinessObjectTestExclude]
		public override ZGuid OPC_OPC_Parent
		{
			get { return base.OPC_OPC_Parent; }
			set { base.OPC_OPC_Parent = value; }
		}

		#endregion

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
