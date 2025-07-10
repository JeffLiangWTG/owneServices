using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class GPSSupporterActivityTestData : GPSSupporterActivity
	{
		public new abstract class Schema : AutoLocalCartageVehicleActivity.Schema
		{
			public const string ClientPK = "ClientPK";
		}

		public GPSSupporterActivityTestData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get { return false; }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new GPSSupporterActivityTestDataValidation Validation
		{
			get { return (GPSSupporterActivityTestDataValidation)GetNewValidation(); }
		}

		protected override LocalCartageVehicleActivityValidation GetNewValidation()
		{
			return new GPSSupporterActivityTestDataValidation(this);
		}

		[List("ActivityTypes")]
		public override ZString EN_ActivityType
		{
			get { return base.EN_ActivityType; }
			set
			{
				base.EN_ActivityType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEN_ActivityType();
				}
			}
		}

		[RelatedBusinessObject("Client")]
		[List("Clients")]
		[ResourceStringData("GPSClientActivityTestData|ClientPK", Caption = "Client")]
		public ZGuid ClientPK
		{
			get
			{
				if (clientPK.IsEmpty && !EN_ActivityInformation.IsEmpty)
				{
					var orgCode = EN_ActivityInformation.Split('-')[0].TrimStart();
					var clientQuery = new ZQuery(OrgHeaderSchema.OH_Code, orgCode);

					var client = Factory.LoadTop1<OrgHeader>(clientQuery);
					clientPK = client != null ? client.PK : ZGuid.Empty;
				}

				return clientPK;
			}
			set
			{
				if (clientPK != value)
				{
					SetNonPersistentPropertyValue(ClientPKInfo, ref clientPK, value);

					var mainOrdAddress = GetMainOrgAddress(Client);
					if (mainOrdAddress != null)
					{
						EN_ActivityInformation = mainOrdAddress.Header.OH_Code + " - " + mainOrdAddress.OA_Code;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateClient();
				}

				ClientPKInfo.RefreshBinding();
			}
		}

		ZGuid clientPK;

		static OrgAddress GetMainOrgAddress(OrgHeader client)
		{
			OrgAddress result = null;

			if (client != null)
			{
				result = client.MainAddress ?? client.Addresses.Cast<OrgAddress>().FirstOrDefault();
			}

			return result;
		}

		public ZPropertyInfo ClientPKInfo
		{
			get { return GetZPropertyInfo(Schema.ClientPK); }
		}

		public OrgHeader Client
		{
			get { return Factory.Load<OrgHeader>(ClientPK); }
		}

		public CodeDescriptionPairList ActivityTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(GPSConstants.GPSInOutActivityType.Codes.GIN, GPSConstants.GPSInOutActivityType.Descriptions.GIN);
				result.AddPair(GPSConstants.GPSInOutActivityType.Codes.GOT, GPSConstants.GPSInOutActivityType.Descriptions.GOT);

				return result;
			}
		}

		public OrgHeaderCollection Clients
		{
			get
			{
				OrgHeaderCollection result = null;
				var query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
				query.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_IsConsignor, true);
				query.AddToFilter(OrgHeaderSchema.OH_IsActive, true);

				result = new OrgHeaderCollection(Factory, query);

				return result;
			}
		}
	}
}
