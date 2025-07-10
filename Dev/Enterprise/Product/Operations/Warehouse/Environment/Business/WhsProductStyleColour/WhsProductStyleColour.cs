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
	[CodeProperty(WhsProductStyleColourSchema.Constants.WSC_Code), DescriptionProperty(WhsProductStyleColourSchema.Constants.WSC_Description)]
	public class WhsProductStyleColour : AutoWhsProductStyleColour, IWhsProductStyleColour
	{
		public WhsProductStyleColour(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		public WhsProductStyle ProductStyle
		{
			get { return Factory.Load<WhsProductStyle>(WSC_WST_ProductStyle); }
		}

		OrgSupplierPart[] GetProducts()
		{
			return Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour, PK));
		}

		#endregion

		#region Properties

		// overriden

		#region WSZ_WST_ProductStyle

		[RelatedBusinessObject("ProductStyle")]
		public override ZGuid WSC_WST_ProductStyle
		{
			get { return base.WSC_WST_ProductStyle; }
			set { base.WSC_WST_ProductStyle = value; }
		}

		#endregion

		// calculated

		#endregion

		#region Delete

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
					result = ResString.GetMultilingualString("WhsProductStyleColour|ReasonForNotAbleToDelete", "This Color cannot be deleted as it is being used on the following Products:\r\n{0}", msgLines.ToStringWithNewLineBetweenAppends());
				}

				return result;
			}
		}

		#endregion
	}
}
