using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Represents the base class of a copy recipient collection.
	/// </summary>
	/// <typeparam name="TCopyRecipient">The type of the copy recipient.</typeparam>
	/// <typeparam name="TCopyRecipientOwner">The type of the owner of the copy recipient.</typeparam>
	public abstract class CopyRecipientCollection<TCopyRecipient, TCopyRecipientOwner> : ActiveBusinessObjectCollection<TCopyRecipient> where TCopyRecipient : BusinessObject where TCopyRecipientOwner : BusinessObject, ILinkable
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">The owner of the copy recipients in the collection.</param>
		/// <param name="type">The type of the copy recipients in the collection.</param>
		/// <param name="emailAddressColumn">The schema column that stores the Email addresses of the copy recipients.</param>
		/// <param name="typeColumn">The schema column that stores the types of the copy recipients.</param>
		protected CopyRecipientCollection(TCopyRecipientOwner owner, string type, SchemaColumn emailAddressColumn, SchemaColumn typeColumn) : base(owner, new ZQuery(typeColumn, type))
		{
			Owner = owner;
			Type = type;
			EmailAddressColumn = emailAddressColumn;
			TypeColmmn = typeColumn;
			((IBindingList)this).ListChanged += CopyRecipientCollection_ListChanged;
		}

		#region Properties

		/// <summary>
		/// Gets the schema column that stores the Email addresses of the copy recipients.
		/// </summary>
		public SchemaColumn EmailAddressColumn { get; private set; }

		/// <summary>
		/// Gets the owner of the copy recipients.
		/// </summary>
		public TCopyRecipientOwner Owner { get; private set; }

		/// <summary>
		/// Gets the schema column that stores the type of the copy recipients in the collection.
		/// </summary>
		public SchemaColumn TypeColmmn { get; private set; }

		/// <summary>
		/// Gets the type of the copy recipients in the collection.
		/// </summary>
		public string Type { get; private set; }

		/// <summary>
		/// Gets or sets the string representation of the copy recipient collection.
		/// </summary>
		public ZString Value
		{
			get => string.Join(", ", Emails);
			set
			{
				DeleteAll();
				var emailAddresses = value.Trim().ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(ea => ea.Trim());
				foreach (var emailAddress in emailAddresses)
				{
					var copyRecipient = AddNew();
					copyRecipient[EmailAddressColumn] = emailAddress;
					copyRecipient[TypeColmmn] = Type;
				}
			}
		}

		public IEnumerable<string> Emails => this.Select(copyRecipient => copyRecipient[EmailAddressColumn].ToString());

		#endregion

		#region Events

		/// <summary>
		/// Triggered when the collection gets updated.
		/// </summary>
		public event EventHandler Updated;

		void OnUpdated()
		{
			if (Updated != null)
			{
				Updated(this, EventArgs.Empty);
			}
		}

		void CopyRecipientCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
				case ListChangedType.ItemChanged:
				case ListChangedType.ItemDeleted:
				case ListChangedType.ItemMoved:
				case ListChangedType.Reset:
					OnUpdated();
					break;
			}
		}

		#endregion

		#region Methods

		protected override void OnAdded(TCopyRecipient businessObject)
		{
			base.OnAdded(businessObject);
			businessObject[TypeColmmn] = Type;
		}

		protected override void SetHasChanges(bool hasChanges)
		{
		}

		#endregion
	}
}