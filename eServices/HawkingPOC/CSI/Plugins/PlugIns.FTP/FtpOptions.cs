namespace Hawking.CSI.Plugins.FTP
{
    public class FtpOptions
    {
        public string Server { get; set; }
        public int Port { get; set; } = 21;
        public string User { get; set; }
        public string Password { get; set; }
        public FtpMode Mode { get; set; } = FtpMode.Passive;
        public int Timeout { get; set; } = 90000;
        public bool UseNLST { get; set; } = false;
        public bool MoveWorkingDirectory { get; set; } = false;
        public FtpsMode FtpsMode { get; set; } = FtpsMode.None;
        public bool FtpsValidateServerCert { get; set; } = false;
        public string FtpsClientCertThumbprint { get; set; }
        public bool FtpsUseDataEncryption { get; set; } = false;

    }

    public enum FtpMode
    {
        Passive,
        Active,
        PASV,
        PASVEX,
        EPSV,
        PORT,
        EPRT
    }

    public enum FtpsMode
    {
        None,
        Explicit,
        Implicit
    }
}