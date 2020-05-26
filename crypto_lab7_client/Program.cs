using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace crypto_lab7_client
{
    class Program
    {
        private const string APP_PATH = "https://localhost:44312";
        private static string token;

        static void Main(string[] args)
        {
            string userName = string.Empty;
            string password = string.Empty;
            string values = string.Empty;
            HttpResponseMessage response = null;

            while (true)
            {
                Console.WriteLine("- Use one of available command (Auth/GetUserByToken/GetValues)");
                string command = Console.ReadLine();

                Console.WriteLine();

                if (command == "GetValues")
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Token is empty");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Current token: ");
                        Console.WriteLine(token);
                        Console.ResetColor();
                    }

                    Console.WriteLine();
                    values = GetValues(token);
                    Console.WriteLine("Result:");
                    Console.WriteLine(values);
                    Console.WriteLine();
                }
                else if (command == "Auth")
                {
                    while (true)
                    {
                        Console.Write("Login: ");
                        userName = Console.ReadLine();

                        Console.Write("Password: ");
                        password = Console.ReadLine();

                        Console.WriteLine();

                        HttpStatusCode httpStatusCode = Register(userName, password, out response);
                        
                        if (httpStatusCode == HttpStatusCode.OK)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("HTTP Status Code: {0}", httpStatusCode);
                            Console.ResetColor();
                            break;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("HTTP Status Code: {0}", httpStatusCode);
                            Console.ResetColor();
                        }

                        Console.WriteLine();
                    }

                    Dictionary<string, string> tokenDictionary = GetTokenDictionary(userName, password);
                    token = tokenDictionary["access_token"];

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("User info:");
                    string userInfo = GetUserInfo(token);
                    Console.WriteLine(userInfo);

                    Console.WriteLine();

                    Console.WriteLine("Access Token:");
                    Console.WriteLine(token);
                    Console.ResetColor();

                    Console.WriteLine();
                }
                else if (command == "GetUserByToken")
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Token is empty");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Current token: ");
                        Console.WriteLine(token);
                        Console.WriteLine();
                        
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("User info:");
                    string userInfo = GetUserInfo(token);
                    Console.WriteLine(userInfo);
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
        }

        static HttpStatusCode Register(string email, string password, out HttpResponseMessage response)
        {
            var registerModel = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            using (var client = new HttpClient())
            {
                response = client.PostAsJsonAsync(APP_PATH + "/api/Account/Register", registerModel).Result;
                return response.StatusCode;
            }
        }

        static Dictionary<string, string> GetTokenDictionary(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( "grant_type", "password" ),
                new KeyValuePair<string, string>( "username", userName ),
                new KeyValuePair<string, string> ( "Password", password )
            };

            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var response =
                    client.PostAsync(APP_PATH + "/Token", content).Result;
                var result = response.Content.ReadAsStringAsync().Result;
                // Десериализация полученного JSON-объекта
                Dictionary<string, string> tokenDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                return tokenDictionary;
            }
        }

        static HttpClient CreateClient(string accessToken = "")
        {
            var client = new HttpClient();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }

            return client;
        }

        static string GetUserInfo(string token)
        {
            using (var client = CreateClient(token))
            {
                var response = client.GetAsync(APP_PATH + "/api/Account/UserInfo").Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        static string GetValues(string token)
        {
            using (var client = CreateClient(token))
            {
                var response = client.GetAsync(APP_PATH + "/api/values").Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
