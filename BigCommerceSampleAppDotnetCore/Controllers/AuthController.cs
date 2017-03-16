using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace BigCommerceSampleAppDotnetCore.Controllers
{
	public class AuthController : BCAppController
	{
		public AuthController(IMemoryCache memoryCache) : base(memoryCache) { }
		
		public async Task<string> Callback(string code, string scope = "", string context = "")
		{
			var kvp = new Dictionary<string, string>
			{
				{ "client_id", BCClientId },
				{ "client_secret", BCClientSecret },
				{ "redirect_uri", RedirectUri },
				{ "grant_type", "authorization_code" },
				{ "code", code },
				{ "scope", scope },
				{ "context", context },
			};

			//var payload = $"client_id={BCClientId}&client_secret={BCClientSecret}&redirect_uri={RedirectUri}&
			//grant_type=authorization_code&code={code}&scope={scope}&context={context}";

			var payload = new FormUrlEncodedContent(kvp);
			var client = new HttpClient();
			client.BaseAddress = new Uri(BCAuthService);
			
			string result = null;

			HttpResponseMessage response = await client.PostAsync("/oauth2/token", payload);
			HttpContent content = response.Content;
			result = await content.ReadAsStringAsync();
			
			if (response.IsSuccessStatusCode)
			{
				JObject data = JObject.Parse(result);
				string[] storeDetails = data["context"].ToString().Split('/');
				string contextFromData = "", storeHash = "";
				string user = data["user"].ToString();

				if (storeDetails.Length == 2)
				{
					contextFromData = storeDetails[0];
					storeHash = storeDetails[1];

					string userEmail = data.SelectToken(@"user.email").Value<string>();
					string key = GetUserKey(storeHash, userEmail);

					_cache.Set(key, user);
					_cache.Set($"stores/{storeHash}/auth", data.ToString());

					return $"Hello, {user}";
				}

				return "Incorrect segments in store details.";
			}

			return $"Something went wrong... [{response.StatusCode}] with this request. Request: {payload}; Response: {result}";
		}
	}
}
