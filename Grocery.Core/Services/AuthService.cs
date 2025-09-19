using Grocery.Core.Helpers;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IClientService _clientService;

        public AuthService(IClientService clientService)
        {
            _clientService = clientService;
        }

        public Client? Login(string email, string password)
        {
            // Zoek de client op basis van email
            var client = _clientService.Get(email);

            if (client == null)
                return null;

            // Controleer het wachtwoord
            bool isValid = PasswordHelper.VerifyPassword(password, client.Password);

            if (isValid)
            {
                return client;
            }

            return null;
        }
    }
}
