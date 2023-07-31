namespace Science.DTO.Auth.Responses
{
    public class AuthResult
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public bool Result { get; set; }

        public string Error { get; set; }
    }
}
