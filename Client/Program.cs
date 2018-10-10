using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private const string SECRET_KEY = "secret";
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            Console.WriteLine("ClientCredentialGrandType");

            await Identify(async (DiscoveryResponse disco) =>
            {
                var tokenClient = new TokenClient(disco.TokenEndpoint, "client", SECRET_KEY);
                return await tokenClient.RequestClientCredentialsAsync("api1");
            });

            Console.WriteLine("ResourceOwnerPasswordGrandType");

            await Identify(async (DiscoveryResponse disco) =>
            {
                var tokenClient = new TokenClient(disco.TokenEndpoint, "resourse.owner.password.client", SECRET_KEY);
                return await tokenClient.RequestResourceOwnerPasswordAsync("Volodymyr", "password", "api1");
            });

            Console.ReadLine();
        }

        private static async Task Identify(Func<DiscoveryResponse, Task<TokenResponse>> tokenResponseFunc)
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            var tokenResponse = await tokenResponseFunc(disco);

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }

    }
}
