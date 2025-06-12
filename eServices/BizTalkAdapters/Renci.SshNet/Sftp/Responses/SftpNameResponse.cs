using System.Collections.Generic;
using System.Text;

namespace Renci.SshNet.Sftp.Responses
{
    internal class SftpNameResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Name; }
        }

        public uint Count { get; private set; }

        public Encoding Encoding { get; private set; }

        public KeyValuePair<string, SftpFileAttributes>[] Files { get; private set; }

        public SftpNameResponse(uint protocolVersion, Encoding encoding)
            : base(protocolVersion)
        {
            this.Files = new KeyValuePair<string, SftpFileAttributes>[0];
            this.Encoding = encoding;
        }

        protected override void LoadData()
        {
            base.LoadData();
            
            this.Count = this.ReadUInt32();
            this.Files = new KeyValuePair<string, SftpFileAttributes>[this.Count];
            
            for (int i = 0; i < this.Count; i++)
            {
                var fileName = this.ReadString(this.Encoding);
                this.ReadString();   //  This field value has meaningless information
                var attributes = this.ReadAttributes();
                this.Files[i] = new KeyValuePair<string, SftpFileAttributes>(fileName, attributes);
            }
        }

		/// <customization>
		/// Added for logging.
		/// </customization>
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(base.ToString());

			sb.AppendFormat("   {0,-19} {1,14} {2,-11} {3}", "MODIFYTIME", "SIZE", "PERMISSIONS", "NAME").AppendLine();
			foreach (var file in this.Files)
			{
				sb.AppendFormat("   {0:s} ", file.Value.LastAccessTime);

				if (file.Value.IsDirectory)
					sb.Append(' ', 15);
				else
					sb.AppendFormat("{0,14:N0} ", file.Value.Size);

				sb.Append(file.Value.IsDirectory ? 'd' : '-');
				sb.Append(file.Value.OwnerCanRead ? 'r' : '-');
				sb.Append(file.Value.OwnerCanWrite ? 'w' : '-');
				sb.Append(file.Value.OwnerCanExecute ? 'x' : '-');
				sb.Append(file.Value.GroupCanRead ? 'r' : '-');
				sb.Append(file.Value.GroupCanWrite ? 'w' : '-');
				sb.Append(file.Value.GroupCanExecute ? 'x' : '-');
				sb.Append(file.Value.OthersCanRead ? 'r' : '-');
				sb.Append(file.Value.OthersCanWrite ? 'w' : '-');
				sb.Append(file.Value.OthersCanExecute ? 'x' : '-');
				sb.Append("  ");

				sb.AppendLine(file.Key);
			}

			return sb.ToString();
		}
	}
}
