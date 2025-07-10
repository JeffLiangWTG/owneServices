using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V3.Business
{
	public class V3JobDeclaration : V4.Business.JobDeclaration
	{
		public V3JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public new V3MessagesCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new V3MessagesCollection(Factory);
					messages.AdditionalFilter = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, PK);
				}
				return messages;
			}
		}
		V3MessagesCollection messages;

		#region PermitNumber

		public ZString PermitNumber
		{
			get
			{
				if (permitNumber.IsEmpty)
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, "PMT");
					query.AddToFilter(CusEntryNumSchema.CE_ParentID, PK);

					CusEntryNumber[] entryNumbers = Factory.Load<CusEntryNumber>(query);

					if (entryNumbers.Length == 1)
					{
						permitNumber = entryNumbers[0].CE_EntryNum;
					}
					else
					{
						permitNumber = "";
					}
				}

				return permitNumber;
			}
		}
		ZString permitNumber;

		public ZPropertyInfo PermitNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PermitNumber)); }
		}

		#endregion

		#region Bind To Lists

		public OrgHeaderCollection Importers
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}

		public OrgHeaderCollection Exporters
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}

		OrgHeaderCollection organisations;

		#endregion
	}
}
