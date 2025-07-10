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
	[CodeProperty(WhsProductStyleClassificationSchema.Constants.WSS_Code), DescriptionProperty(WhsProductStyleClassificationSchema.Constants.WSS_Description)]
	public class WhsProductStyleClassification : AutoWhsProductStyleClassification, IWhsProductStyleClassification
	{
		public WhsProductStyleClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related

		public WhsProductStyle ProductStyle
		{
			get { return Factory.Load<WhsProductStyle>(WSS_WST_ProductStyle); }
		}

		OrgSupplierPart[] GetProducts()
		{
			return Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification, PK));
		}

		#endregion

		#region Properties

		// overriden

		#region WSS_WST_ProductStyle

		[RelatedBusinessObject("ProductStyle")]
		public override ZGuid WSS_WST_ProductStyle
		{
			get { return base.WSS_WST_ProductStyle; }
			set { base.WSS_WST_ProductStyle = value; }
		}

		#endregion

		// calculated

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return base.CanDelete && (!IsInDatabase || GetProducts().Length == 0); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = (NoResString)string.Empty;

				if (!CanDelete)
				{
					var msgLines = new ZStringBuilder();
					Array.ForEach(GetProducts().OrderBy(p => p.OP_PartNum).ToArray(), p => msgLines.Append(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", p.OP_PartNum, p.OP_Desc)));
					result = ResString.GetMultilingualString("WhsProductStyleClassification|ReasonForNotAbleToDelete", "This Classification cannot be deleted as it is being used on the following Products:\r\n{0}", msgLines.ToStringWithNewLineBetweenAppends());
				}

				return result;
			}
		}

		#endregion
	}
}
