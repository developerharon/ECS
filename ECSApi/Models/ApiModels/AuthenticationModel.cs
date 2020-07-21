using System;
using System.IO;
using System.Net.Http;

namespace ECSApi.Models.ApiModels
{
    public class AuthenticationModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
