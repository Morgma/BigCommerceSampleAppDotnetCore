using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace BigCommerceSampleAppDotnetCore.Controllers
{
	public class LoadController : BCAppController
	{
		public LoadController(IMemoryCache memoryCache) : base(memoryCache) {}

		public string Index(string signed_payload)
		{
			string data = VerifySignedRequest(signed_payload);
			JObject jsonData = JObject.Parse(data);
			JToken userData = jsonData["user"];
			string email = userData["email"].ToString();

			if (string.IsNullOrEmpty(data))
			{
				return "Invalid signed payload.";
			}

			string key = GetUserKey(jsonData["store_hash"].ToString(), email);
			string user = "";

			if (!_cache.TryGetValue(key, out user))
			{
				return "Invalid user.";
			}

			return $"Welcome, {user}";
		}

		string VerifySignedRequest(string signedRequest)
		{
			string[] requestSplit = signedRequest.Split('.');
			string signature = "", jsonStr = "";

			if (requestSplit.Length == 2)
			{
				jsonStr = Encoding.UTF8.GetString(Convert.FromBase64String(requestSplit[0]));
				signature = Encoding.UTF8.GetString(Convert.FromBase64String(requestSplit[1]));
			}
			else
			{
				throw new InvalidOperationException("Incorrect amount of JWT segments.");
			}

			string expectedSignature = CreateToken(jsonStr, BCClientSecret);

			if (!string.Equals(signature, expectedSignature)) // this works!
			{
				Console.WriteLine("Bad signed request from BigCommerce!");
			}

			return jsonStr;
		}

		string CreateToken(string message, string secret)
		{
			secret = secret ?? "";
			var encoding = new ASCIIEncoding();
			byte[] keyByte = encoding.GetBytes(secret);
			byte[] messageBytes = encoding.GetBytes(message);

			using (var hmacsha256 = new HMACSHA256(keyByte))
			{
				byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
				return ToHexString(hashmessage);
			}
		}
		
	        public static string ToHexString(byte[] array)
		{
		    StringBuilder hex = new StringBuilder(array.Length * 2);
		    foreach (byte b in array)
		    {
			hex.AppendFormat("{0:x2}", b);
		    }
		    return hex.ToString();
		}
		
		
	}
}
