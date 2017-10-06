namespace IdentityBase.Extensions
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;

    public static class AuthenticationManagerExtensions
    {
        public static async Task SignInAsync(
            this HttpContext context,
            UserAccount userAccount,
            AuthenticationProperties properties)
        {
            await context.SignInAsync(
                userAccount.Id.ToString(),
                userAccount.Email,
                properties);
        }
    }    
}