using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Redirects either to decaration or to a shipment
	/// can take either Ref=X&Table=Y OR Number=X (the 'Number' param is for MFI external calls)
	/// </summary>
	public partial class Shipment : BasePage
	{
		override protected void OnLoad(EventArgs e)
		{
			Response.Redirect(GetNavigateURL(Request.Params));
		}

		internal ZString GetNavigateURL(NameValueCollection requestParams)
		{
			string tableName = requestParams["Table"];
			ZGuid pk = GetGuidFromParameter("Ref", requestParams);

			if (pk.IsEmpty && (requestParams["Number"] != null))
			{
				BusinessObject shipmentOrDeclaration =
					(BusinessObject)Factory.LoadTop1<TrackingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, new ZString(requestParams["Number"]))) ??
					(BusinessObject)Factory.LoadTop1<BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, new ZString(requestParams["Number"])));

				if (shipmentOrDeclaration != null)
				{
					tableName = shipmentOrDeclaration is TrackingShipment ? JobShipmentSchema.Constants.TableName : JobDeclarationSchema.Constants.TableName;
					pk = shipmentOrDeclaration.PK;
				}
			}

			if (string.IsNullOrEmpty(tableName) && !pk.IsEmpty)
			{
				if (Factory.Load<TrackingShipment>(pk) != null)
				{
					tableName = JobShipmentSchema.Constants.TableName;
				}
				else if (Factory.Load<BaseJobDeclaration>(pk) != null)
				{
					tableName = JobDeclarationSchema.Constants.TableName;
				}
			}

			string navigateURL = tableName == JobShipmentSchema.Constants.TableName
								? AppInstance.ShipmentDetailsPage
								: tableName == JobDeclarationSchema.Constants.TableName
									? AppInstance.DeclarationDetailsPage
									: string.Empty;

			return String.Format((NoResString)"{0}?Ref={1}", navigateURL, pk); // non-semantic text
		}
	}
}
