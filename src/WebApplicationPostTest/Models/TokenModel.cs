using System.ComponentModel;

namespace WebApplicationPostTest.Models
{
    public class TokenModel
    {
        [DisplayName("access_token")]
        public string AccessToken { get; set; }

        [DisplayName("token_type")]
        public string TokenType { get; set; }

        [DisplayName("expires_in")]
        public string Expirein { get; set; }

        [DisplayName("userName")]
        public string UserName { get; set; }

        [DisplayName(".issued")]
        public string Issued { get; set; }

        [DisplayName(".expires")]
        public string Expires { get; set; }
    }
}
