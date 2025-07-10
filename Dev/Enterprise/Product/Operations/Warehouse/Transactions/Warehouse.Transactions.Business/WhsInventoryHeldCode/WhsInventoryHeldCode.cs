using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[CodeProperty(WhsInventoryHeldCodeSchema.Constants.WHC_Code), DescriptionProperty(WhsInventoryHeldCodeSchema.Constants.WHC_Description)]
	public class WhsInventoryHeldCode : AutoWhsInventoryHeldCode, IWhsInventoryHeldCode
	{
		#region Constructor

		public WhsInventoryHeldCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		// Properties

		#region WHC_Code

		[ReadOnlyMember(nameof(WHC_IsSystem))]
		public override ZString WHC_Code
		{
			get { return base.WHC_Code; }
			set { base.WHC_Code = value; }
		}

		#endregion

		#region WHC_Description

		[ReadOnlyMember(nameof(WHC_IsSystem))]
		[TranslatableDataField(Schema.TableName, Schema.WHC_Description, @"Database\Odyssey\Data\Public\WhsInventoryHeldCode\WhsInventoryHeldCode.xml", MaxLength = Schema.WHC_DescriptionMaxLength, Type = typeof(WhsInventoryHeldCode), Asmid = ResString.AssemblyId)]
		public override ZString WHC_Description
		{
			get { return base.WHC_Description; }
			set { base.WHC_Description = value; }
		}

		public MultilingualString WHC_DescriptionMultilingual
		{
			get { return GetMultilingual(WHC_DescriptionInfo); }
		}

		#endregion

		#region WHC_IsSystem

		[ReadOnly(true)]
		public override ZBool WHC_IsSystem
		{
			get { return base.WHC_IsSystem; }
			set { base.WHC_IsSystem = value; }
		}

		#endregion

		#region WHC_OH_Client

		[ReadOnlyMember(nameof(WHC_IsSystem))]
		[RelatedBusinessObject(nameof(Client))]
		[List("Lookups.Clients")]
		public override ZGuid WHC_OH_Client
		{
			get { return base.WHC_OH_Client; }
			set { base.WHC_OH_Client = value; }
		}

		#endregion

		// Calculated

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("WhsInventoryHeldCode|HumanReadableName", "Inventory Hold Code", IsInDatabase ? string.Format(Culture.Current, "{0} - {1}", WHC_Code, WHC_DescriptionMultilingual) : ""); }
		}

		#endregion

		#region IsDamaged

		public bool IsDamaged
		{
			get { return WHC_Code.EqualsIgnoringCase(InventoryHoldCodes.Codes.Damaged); }
		}

		#endregion

		#region IsOriginalHeldCodeUsed

		public bool IsOriginalHeldCodeUsed()
		{
			var filter = new ZQuery(WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, WHC_CodeInfo.OriginalValue.ToString());
			return Factory.LoadTop1<IWhsDocketLine>(filter) != null;
		}

		#endregion

		// Business Object Overrides

		#region Delete

		public override bool CanDelete
		{
			get { return !WHC_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("WhsInventoryHeldCode|ReasonForNotAbleToDelete", "Cannot delete System Hold Codes."); }
		}

		#endregion
	}
}
