using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterLine : NonPersistentBusinessObject
	{
		public EDIMessageContentFilterLine(EDIMessageContentFilterSpec parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}

		public EDIMessageContentFilterSpec Parent { get; }

		internal ZString Schema => Parent.Schema;

		#region Properties

		#region SchemaElement

		[XmlColumnProperty]
		[List("Lookups.SchemaElements")]
		[ResourceStringData("EDIMessageContentFilterLine.SchemaElement", Caption = "Schema Element", ShortCaption = "Element")]
		public ZString SchemaElement
		{
			get { return GetXmlColumnPropertyValue<ZString>(SchemaElementInfo); }
			set
			{
				var previousValue = SchemaElement;
				SetXmlColumnPropertyValue(SchemaElementInfo, value.TrimEnd(), Validation.ValidateSchemaElement);

				if (previousValue == "SubShipmentCollection" && value.TrimEnd() != "SubShipmentCollection")
				{
					DataContext = string.Empty;
				}
			}
		}

		public ZPropertyInfo SchemaElementInfo => GetZPropertyInfo(nameof(SchemaElement));

		#endregion

		#region Type

		[ResourceStringData("EDIMessageContentFilterLine.ElementType", Caption = "Element Type", ShortCaption = "Type")]
		public ZString ElementType => this.Lookups.AllSchemaElements.GetDescriptionFromCode(SchemaElement) ?? ZString.Empty;

		#endregion

		#region DataContext

		[XmlColumnProperty]
		[List("Lookups.DataContexts")]
		[ResourceStringData("EDIMessageContentFilterLine.DataContext", Caption = "Data Context", ShortCaption = "Context")]
		public ZString DataContext
		{
			get { return GetXmlColumnPropertyValue<ZString>(DataContextInfo); }
			set { SetXmlColumnPropertyValue(DataContextInfo, value.TrimEnd(), Validation.ValidateDataContext); }
		}

		public ZPropertyInfo DataContextInfo => GetZPropertyInfo(nameof(DataContext));

		public bool DataContext_ReadOnly => !Parent.IsExclude || !SchemaElement.Equals("SubShipmentCollection");

		#endregion

		#region Depth

		[XmlColumnProperty]
		[ResourceStringData("EDIMessageContentFilterLine.Depth", Caption = "Depth")]
		public ZInt Depth
		{
			get { return GetXmlColumnPropertyValue<ZInt>(DepthInfo); }
			set { SetXmlColumnPropertyValue(DepthInfo, value, Validation.ValidateDepth); }
		}

		public ZPropertyInfo DepthInfo => GetZPropertyInfo(nameof(Depth));

		#endregion

		#endregion

		#region Lookups

		public EDIMessageContentFilterLineLookups Lookups => new EDIMessageContentFilterLineLookups(this);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public EDIMessageContentFilterLineValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual EDIMessageContentFilterLineValidation GetNewValidation()
		{
			return new EDIMessageContentFilterLineValidation(this);
		}

		#endregion
	}
}
