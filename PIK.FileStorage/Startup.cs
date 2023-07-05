using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace PIK.FileStorage;


internal class Startup
{
	private readonly IConfiguration _configuration;


	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}


	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers(options =>
		{
			IEnumerable<IConfigurationSection> cacheProfiles = _configuration
				.GetSection("CacheProfiles")
				.GetChildren();

			foreach (IConfigurationSection cacheProfile in cacheProfiles)
			{
				options.CacheProfiles.Add(cacheProfile.Key, cacheProfile.Get<CacheProfile>());
			}
		});

		services.AddSwaggerGen(config =>
		{
			string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			config.IncludeXmlComments(xmlPath);
		});

		services.AddResponseCaching();
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseSwagger();
		app.UseSwaggerUI(config =>
		{
			config.RoutePrefix = string.Empty;
			config.SwaggerEndpoint("/swagger/v1/swagger.json", "PIK.FileStorage API");
		});

		app.UseRouting();
		app.UseHttpsRedirection();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		app.UseResponseCaching();
	}
}