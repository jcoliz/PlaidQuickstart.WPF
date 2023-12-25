namespace Core.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Matches Plaid API")]
public class LinkResult
{
    public class t_metadata
    {
        public class t_institution
        {
            public string? name { get; set; } = string.Empty;
            public string? institution_id { get; set; } = string.Empty;
        }

        public string? link_session_id { get; set; } = null;
        public string? status { get; set; } = null;
        public string? request_id { get; set; } = null;
        public t_institution? institution { get; set; }
    }


    public bool ok { get; set; }
    public string? public_token { get; set; } = string.Empty;
    public WireError? error { get; set; } = null;
    public t_metadata? metadata { get; set; } = null;
};
