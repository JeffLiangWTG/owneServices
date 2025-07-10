using System;
using System.ComponentModel;
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
	[CodeProperty(WhsProductStyleSizeSchema.Constants.WSZ_Size), DescriptionProperty(WhsProductStyleSizeSchema.Constants.WSZ_Size)]
	public class WhsProductStyleSize : AutoWhsProductStyleSize, IWhsProductStyleSize
	{
		public WhsProductStyleSize(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		public WhsProductStyle ProductStyle
		{
			get { return Factory.Load<WhsProductStyle>(WSZ_WST_ProductStyle); }
		}

		OrgSupplierPart[] GetProducts()
		{
			return Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize, PK));
		}

		#endregion

		#region Properties

		// overriden

		#region WSZ_WST_ProductStyle

		[RelatedBusinessObject("ProductStyle")]
		public override ZGuid WSZ_WST_ProductStyle
		{
			get { return base.WSZ_WST_ProductStyle; }
			set { base.WSZ_WST_ProductStyle = value; }
		}

		#endregion

		#region WSZ_Sequence

		[ReadOnly(true)]
		public override ZByte WSZ_Sequence
		{
			get { return base.WSZ_Sequence; }
			set { base.WSZ_Sequence = value; }
		}

		#endregion

		// calculated

		#endregion

		#region Delete

		public override void Delete()
		{
			var sizes = ProductStyle != null && !ProductStyle.IsDeleting ? ProductStyle.Sizes : null;

			base.Delete();

			if (sizes != null)
			{
				sizes.Sequence();
			}
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
					result = ResString.GetMultilingualString("WhsProductStyleSize|ReasonForNotAbleToDelete", "This Size cannot be deleted as it is being used on the following Products:\r\n{0}", msgLines.ToStringWithNewLineBetweenAppends());
				}

				return result;
			}
		}

		#endregion
	}
}
