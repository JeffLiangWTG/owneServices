using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AESDeclaration : NonPersistentBusinessObject, IAESDeclaration, IObsoleteValidation
	{
		public AESDeclaration(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public AESEntryCollection Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new AESEntryCollection(declaration);
					entries.RunPreSaveValidation();
				}
				return entries;
			}
		}
		AESEntryCollection entries;

		#region IAESDeclaration Members

		void IAESDeclaration.LogCustomsCommencedIfNeeded()
		{
			declaration.LogCustomsCommencedIfNeeded();
		}

		Customs.Business.ISendsMessagesToCustoms IAESDeclaration.MessageInitiator
		{
			get { return declaration.MessageInitiator; }
		}

		IEnumerable<IAESEntry> IAESDeclaration.ActiveEntryHeaders
		{
			get
			{
				foreach (IAESEntry entry in Entries)
				{
					yield return entry;
				}
			}
		}

		#endregion
	}
}
