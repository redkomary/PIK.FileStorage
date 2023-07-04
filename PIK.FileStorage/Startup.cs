using System.Reflection;

namespace PIK.FileStorage;


internal class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();

		services.AddSwaggerGen(config =>
		{
			string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			config.IncludeXmlComments(xmlPath);
		});
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
	}
}