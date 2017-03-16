using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace BigCommerceSampleAppDotnetCore.Controllers
{
	// BCAppControllers load stored BigCommerce configuration in AppSettings into variables accessible by its children

	public class BCAppController : Controller
	{
		protected IConfigurationRoot Configuration { get; set; }
		protected string BCAuthService, BCClientId, BCClientSecret, RedirectUri;
		protected IMemoryCache _cache;
		
		public BCAppController(IMemoryCache memoryCache)
		{
			var builder = new ConfigurationBuilder()
            	.SetBasePath(Directory.GetCurrentDirectory())
            	.AddJsonFile("appsettings.Development.json");

	        Configuration = builder.Build();

			// Grab BigCommerce credentials from configuration (appsettings.json)

			BCAuthService = Configuration["BC_Credentials:AuthService"];
			BCClientId = Configuration["BC_Credentials:ClientId"];
			BCClientSecret = Configuration["BC_Credentials:ClientSecret"];
			RedirectUri = Configuration["BC_Credentials:RedirectUri"];

			_cache = memoryCache;
		}

		protected string GetUserKey(string storeHash, string email)
		{
			return $"{storeHash}:{email}";
		}
	}
}
