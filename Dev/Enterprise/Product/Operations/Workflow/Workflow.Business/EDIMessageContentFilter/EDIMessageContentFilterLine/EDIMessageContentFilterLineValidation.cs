using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterLineValidation : ZValidation
	{
		public EDIMessageContentFilterLineValidation(EDIMessageContentFilterLine parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected EDIMessageContentFilterLine Parent { get; }

		public override Type AutoValidationType => typeof(EDIMessageContentFilterLine);

		public override void ValidateAll()
		{
			ValidateSchemaElement();
			ValidateDepth();
			ValidateDataContext();
		}

		#region SchemaElement

		public void ValidateSchemaElement() => ValidateCalculatedProperty(Parent.SchemaElementInfo);

		protected void CheckSchemaElement()
		{
			MandatoryValidation.CheckEntered(Parent.SchemaElementInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SchemaElementInfo, Parent.Lookups.AllSchemaElements);

			if (Parent.Parent.Lines.Cast<EDIMessageContentFilterLine>().Any(
					l => l.SchemaElement == Parent.SchemaElement
						 && l.DataContext == Parent.DataContext
						 && l != Parent))
			{
				Parent.SchemaElementInfo.AddError(GetDuplicateMessage(Parent.SchemaElement));
			}
		}

		public static string GetDuplicateMessage(string element) => Res.GetString("afde21b8-abc5-48d8-9adf-872e08d65e37", "Duplicate found on Schema Element [{0}].", element);

		#endregion

		#region DataContext

		public void ValidateDataContext() => ValidateCalculatedProperty(Parent.DataContextInfo);

		protected void CheckDataContext()
		{
			var ediMessageContentFilter = Parent.Parent;
			if (!ediMessageContentFilter.IsExclude && ediMessageContentFilter.Lines.Cast<EDIMessageContentFilterLine>().Any(l => !string.IsNullOrEmpty(l.DataContext)))
			{
				Parent.DataContextInfo.AddError(Res.GetString("074f56d9-948f-461d-8f7d-98c1ac630509", "Data Context can not be set under include conditions."));
			}

			if(ediMessageContentFilter.IsExclude && Parent.SchemaElement == "SubShipmentCollection")
			{
				MandatoryValidation.CheckEntered(Parent.DataContextInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.DataContextInfo, Parent.Lookups.DataContexts);
		}

		#endregion

		#region Depth

		public void ValidateDepth() => ValidateCalculatedProperty(Parent.DepthInfo);

		const int minDepth = 0;
		const int maxDepth = 256;
		protected void CheckDepth()
		{
			if (!Parent.Depth.IsInRange(minDepth, maxDepth))
			{
				Parent.DepthInfo.AddError(DepthMessage);
			}
		}

		public static string DepthMessage => Res.GetString("EDIMessageContentFilterLineValidation.Depth", "Depth should be between {0} and {1}", minDepth, maxDepth);

		#endregion
	}
}
