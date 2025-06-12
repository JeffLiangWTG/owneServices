using System;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using MimeKit;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("3D17E0CF-82D2-42EA-8CF3-CE4512E651CB")]
	public class MimePartyParser : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Resolve eHub ID by name first, and then address"; }
		}

		public string Name
		{
			get { return "MIME Party Parser"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public System.Collections.IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		public IBaseMessage Execute(IPipelineContext context, IBaseMessage message)
		{
			if (Enable)
			{
				try
				{
					var partyAccessor = GetPartyAccessor();

					var mime = MimeMessage.Load(message.BodyPart.GetOriginalDataStream());

					var from = mime.From.First();
					var sender = partyAccessor.GetClientIDFromEmail(from.Name, ((MailboxAddress)from).Address);

					var to = mime.To.First();
					var recipient = partyAccessor.GetClientIDFromEmail(to.Name, ((MailboxAddress)to).Address);

					message.Context.WriteProperty<BTS.SourceParty>(sender);
					message.Context.WriteProperty<BTS.DestinationParty>(recipient);
				}
				catch (Exception ex)
				{
					throw new ApplicationException(ex.Message + Environment.NewLine + ex.StackTrace);
				}
			}

			return message;
		}

		internal virtual IPartyAccessor GetPartyAccessor()
		{
			return DataAccessFactories.NewPartyAccessorInstance();
		}

		#endregion

		#region IPersistPropertyBag Members

		public bool Enable { get; set; }

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("3D17E0CF-82D2-42EA-8CF3-CE4512E651CB");
		}

		public void InitNew() { }

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			var var = LoadProperty(propertyBag, "Enable", errorLog);
			if (var != null) Enable = Convert.ToBoolean(var);
		}

		object LoadProperty(IPropertyBag propertyBag, string propertyName, int errorLog)
		{
			object result = null;
			try
			{
				propertyBag.Read(propertyName, out result, errorLog);
			}
			catch { }
			return result;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enable;
			propertyBag.Write("Enable", ref val);
		}

		#endregion
	}
}
