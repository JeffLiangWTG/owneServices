using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class PackLineBulkUpdateDataSource : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string OuterPackLinesInspectionTypeBulkSetter = "OuterPackLinesInspectionTypeBulkSetter";
		}

		public PackLineBulkUpdateDataSource(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PackLineBulkUpdateRecordCollection OuterPackLines
		{
			get
			{
				if (outerPackLines == null)
				{
					outerPackLines = new PackLineBulkUpdateRecordCollection(Factory);
					RegisterEditableChildObject(outerPackLines);
				}
				return outerPackLines;
			}
		}
		PackLineBulkUpdateRecordCollection outerPackLines;

		[MaxLength(3)]
		[List(nameof(InspectionTypesForOuterPackLinesBulkSetterList))]
		public ZString OuterPackLinesInspectionTypeBulkSetter
		{
			get
			{
				return bulkSetInspectionType;
			}
			set
			{
				SetNonPersistentPropertyValue(OuterPackLinesInspectionTypeBulkSetterInfo, ref bulkSetInspectionType, value);

				if (InspectionTypesForOuterPackLinesBulkSetterList.ContainsCode(bulkSetInspectionType))
				{
					foreach (var packLine in OuterPackLines.OfType<PackLineBulkUpdateRecord>())
					{
						if (!packLine.PackLineInspectionTypeInfo.ReadOnly)
						{
							packLine.PackLineInspectionType = value;
							HasChanges = true;
						}
					}
				}

				if (!IsValidationSuspended)
				{
					ValidateOuterPackLinesInspectionTypeBulkSetter();
				}

				OuterPackLinesInspectionTypeBulkSetterInfo.RefreshBinding();
				OuterPackLines.RefreshBindingIncludingChildren();
			}
		}
		ZString bulkSetInspectionType;

		public bool OuterPackLinesInspectionTypeBulkSetter_ReadOnly => !Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed;

		public ZPropertyInfo OuterPackLinesInspectionTypeBulkSetterInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				var propertyInfo = GetZPropertyInfo(Schema.OuterPackLinesInspectionTypeBulkSetter);
				propertyInfo.HumanReadableName = ResString.GetMultilingualString("3AB9BBEF-8B9D-469A-A1F9-332597BEDEDB", "Inspection Status");
				return propertyInfo;
			}
		}

		void ValidateOuterPackLinesInspectionTypeBulkSetter()
		{
			OuterPackLinesInspectionTypeBulkSetterInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(OuterPackLinesInspectionTypeBulkSetterInfo);
		}

		public CodeDescriptionPairList InspectionTypesForOuterPackLinesBulkSetterList
		{
			get
			{
				if (OuterPackLines.Any())
				{
					return OuterPackLines[0].PackLineInspectionTypes;
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		public void AddAllOuterPackLines(IEnumerable<ForwardingShipment> shipments)
		{
			foreach (var shipment in shipments)
			{
				AddAllOuterPackLines(shipment);
			}
		}

		public void AddAllOuterPackLines(ForwardingShipment shipment)
		{
			OuterPackLineCollection packLines = shipment.OuterPackLines;
			foreach (BusinessObject packLine in packLines)
			{
				if (packLine is ForwardingPackLine forwardingPackLine)
				{
					OuterPackLines.Add(new PackLineBulkUpdateRecord(forwardingPackLine));
				}
			}
		}
	}
}
