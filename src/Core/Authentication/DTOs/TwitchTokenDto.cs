using Newtonsoft.Json;

namespace Core.Authentication.DTOs
{
    public class TwitchTokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        public string[] Scope { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
