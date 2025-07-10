using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[CodeProperty(WhsProductStyleSchema.Constants.WST_Code), DescriptionProperty(WhsProductStyleSchema.Constants.WST_Description)]
	public class WhsProductStyle : AutoWhsProductStyle, IWhsProductStyle
	{
		public WhsProductStyle(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		#region Colours

		[ChildEditable(true)]
		public WhsProductStyleColourCollection Colours
		{
			get
			{
				if (colours == null)
				{
					colours = new WhsProductStyleColourCollection(this);
					RegisterEditableChildObject(colours);
				}
				return colours;
			}
		}
		WhsProductStyleColourCollection colours;

		#endregion

		#region Classifications

		[ChildEditable(true)]
		public WhsProductStyleClassificationCollection Classifications
		{
			get
			{
				if (classifications == null)
				{
					classifications = new WhsProductStyleClassificationCollection(this);
					RegisterEditableChildObject(classifications);
				}
				return classifications;
			}
		}
		WhsProductStyleClassificationCollection classifications;

		#endregion

		#region Sizes

		[ChildEditable(true)]
		public WhsProductStyleSizeCollection Sizes
		{
			get
			{
				if (sizes == null)
				{
					sizes = new WhsProductStyleSizeCollection(this);
					RegisterEditableChildObject(sizes);
				}
				return sizes;
			}
		}
		WhsProductStyleSizeCollection sizes;

		#endregion

		#region Products

		internal OrgSupplierPart[] GetProducts()
		{
			var query = new ZQuery(OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour, Colours.Select(c => c.PK));
			query.AddToFilter(JoinCondition.Or, OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize, Sizes.Select(s => s.PK));
			return Factory.Load<OrgSupplierPart>(query);
		}

		#endregion

		#endregion

		#region Override

		public override void Delete()
		{
			Colours.DeleteAll();
			Classifications.DeleteAll();
			using (new SemaphoreManager(Sizes.SequenceSemaphore))
			{
				Sizes.DeleteAll();
			}

			base.Delete();
		}

		public override bool CanDelete
			=> base.CanDelete && (!IsInDatabase || GetProducts().Length == 0);

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;

				if (!CanDelete)
				{
					var msgLines = new ZStringBuilder();
					Array.ForEach(GetProducts().OrderBy(p => p.OP_PartNum).ToArray(), p => msgLines.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", p.OP_PartNum, p.OP_Desc)));
					result = ResString.GetMultilingualString("WhsProductStyle|ReasonForNotAbleToDelete", "This Product Style cannot be deleted as it is being used on the following Products:\r\n{0}", msgLines.ToStringWithNewLineBetweenAppends());
				}

				return result;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WST_Code.IsEmpty ?
					Res.GetString("44755597-7060-4FBA-9A6D-D1DA2A5B26F2", "Product Style") :
					Res.GetString("F53E3302-BC0D-46F5-86D3-FCDA0C216C5A", "Product Style {0}", WST_Code);
			}
		}

		#endregion
	}
}
